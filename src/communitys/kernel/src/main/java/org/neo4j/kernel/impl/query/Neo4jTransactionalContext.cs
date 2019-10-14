using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.query
{
	using Lock = Neo4Net.Graphdb.Lock;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using ExecutionStatistics = Neo4Net.@internal.Kernel.Api.ExecutionStatistics;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using QueryRegistryOperations = Neo4Net.Kernel.api.QueryRegistryOperations;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using Statement = Neo4Net.Kernel.api.Statement;
	using DbmsOperations = Neo4Net.Kernel.api.dbms.DbmsOperations;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using TxStateHolder = Neo4Net.Kernel.api.txstate.TxStateHolder;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using PropertyContainerLocker = Neo4Net.Kernel.impl.coreapi.PropertyContainerLocker;
	using StatisticProvider = Neo4Net.Kernel.impl.query.statistic.StatisticProvider;

	public class Neo4jTransactionalContext : TransactionalContext
	{
		 private readonly GraphDatabaseQueryService _graph;
		 private readonly ThreadToStatementContextBridge _txBridge;
		 private readonly PropertyContainerLocker _locker;

		 public readonly KernelTransaction.Type TransactionType;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public readonly SecurityContext SecurityContextConflict;
		 private readonly ExecutingQuery _executingQuery;
		 private readonly Kernel _kernel;

		 /// <summary>
		 /// Current transaction.
		 /// Field can be read from a different thread in <seealso cref="terminate()"/>.
		 /// </summary>
		 private volatile InternalTransaction _transaction;
		 private KernelTransaction _kernelTransaction;
		 private Statement _statement;
		 private bool _isOpen = true;

		 private long _pageHits;
		 private long _pageMisses;

		 public Neo4jTransactionalContext( GraphDatabaseQueryService graph, ThreadToStatementContextBridge txBridge, PropertyContainerLocker locker, InternalTransaction initialTransaction, Statement initialStatement, ExecutingQuery executingQuery, Kernel kernel )
		 {
			  this._graph = graph;
			  this._txBridge = txBridge;
			  this._locker = locker;
			  this.TransactionType = initialTransaction.TransactionType();
			  this.SecurityContextConflict = initialTransaction.SecurityContext();
			  this._executingQuery = executingQuery;

			  this._transaction = initialTransaction;
			  this._kernelTransaction = txBridge.GetKernelTransactionBoundToThisThread( true );
			  this._statement = initialStatement;
			  this._kernel = kernel;
		 }

		 public override ExecutingQuery ExecutingQuery()
		 {
			  return _executingQuery;
		 }

		 public override DbmsOperations DbmsOperations()
		 {
			  return _graph.DbmsOperations;
		 }

		 public override KernelTransaction KernelTransaction()
		 {
			  return _kernelTransaction;
		 }

		 public virtual bool TopLevelTx
		 {
			 get
			 {
				  return _transaction.transactionType() == KernelTransaction.Type.@implicit;
			 }
		 }

		 public override void Close( bool success )
		 {
			  if ( _isOpen )
			  {
					try
					{
						 _statement.queryRegistration().unregisterExecutingQuery(_executingQuery);
						 _statement.close();

						 if ( success )
						 {
							  _transaction.success();
						 }
						 else
						 {
							  _transaction.failure();
						 }
						 _transaction.close();
					}
					finally
					{
						 _statement = null;
						 _kernelTransaction = null;
						 _transaction = null;
						 _isOpen = false;
					}
			  }
		 }

		 public override void Terminate()
		 {
			  InternalTransaction currentTransaction = _transaction;
			  if ( currentTransaction != null )
			  {
					currentTransaction.Terminate();
			  }
		 }

		 public override void CommitAndRestartTx()
		 {
			 /*
			  * This method is use by the Cypher runtime to cater for PERIODIC COMMIT, which allows a single query to
			  * periodically, after x number of rows, to commit a transaction and spawn a new one.
			  *
			  * To still keep track of the running stream after switching transactions, we need to open the new transaction
			  * before closing the old one. This way, a query will not disappear and appear when switching transactions.
			  *
			  * Since our transactions are thread bound, we must first unbind the old transaction from the thread before
			  * creating a new one. And then we need to do that thread switching again to close the old transaction.
			  */

			  CheckNotTerminated();

			  CollectTransactionExecutionStatistic();

			  // (1) Unbind current transaction
			  QueryRegistryOperations oldQueryRegistryOperations = _statement.queryRegistration();
			  Statement oldStatement = _statement;
			  InternalTransaction oldTransaction = _transaction;
			  KernelTransaction oldKernelTx = _txBridge.getKernelTransactionBoundToThisThread( true );
			  _txBridge.unbindTransactionFromCurrentThread();

			  // (2) Create, bind, register, and unbind new transaction
			  _transaction = _graph.beginTransaction( TransactionType, SecurityContextConflict );
			  _kernelTransaction = _txBridge.getKernelTransactionBoundToThisThread( true );
			  _statement = _kernelTransaction.acquireStatement();
			  _statement.queryRegistration().registerExecutingQuery(_executingQuery);
			  _txBridge.unbindTransactionFromCurrentThread();

			  // (3) Rebind old transaction just to commit and close it (and unregister as a side effect of that)
			  _txBridge.bindTransactionToCurrentThread( oldKernelTx );
			  oldQueryRegistryOperations.UnregisterExecutingQuery( _executingQuery );
			  try
			  {
					oldStatement.Close();
					oldTransaction.Success();
					oldTransaction.Close();
			  }
			  catch ( Exception t )
			  {
					// Corner case: The old transaction might have been terminated by the user. Now we also need to
					// terminate the new transaction.
					_txBridge.bindTransactionToCurrentThread( _kernelTransaction );
					_transaction.failure();
					_transaction.close();
					_txBridge.unbindTransactionFromCurrentThread();
					throw t;
			  }

			  // (4) Unbind the now closed old transaction and rebind the new transaction for continued execution
			  _txBridge.unbindTransactionFromCurrentThread();
			  _txBridge.bindTransactionToCurrentThread( _kernelTransaction );
		 }

		 public override void CleanForReuse()
		 {
			  // close the old statement reference after the statement has been "upgraded"
			  // to either a schema data or a schema statement, so that the locks are "handed over".
			  _statement.queryRegistration().unregisterExecutingQuery(_executingQuery);
			  _statement.close();
			  _statement = _txBridge.get();
			  _statement.queryRegistration().registerExecutingQuery(_executingQuery);
		 }

		 public virtual TransactionalContext OrBeginNewIfClosed
		 {
			 get
			 {
				  CheckNotTerminated();
   
				  if ( !_isOpen )
				  {
						_transaction = _graph.beginTransaction( TransactionType, SecurityContextConflict );
						_kernelTransaction = _txBridge.getKernelTransactionBoundToThisThread( true );
						_statement = _kernelTransaction.acquireStatement();
						_statement.queryRegistration().registerExecutingQuery(_executingQuery);
						_isOpen = true;
				  }
				  return this;
			 }
		 }

		 private void CheckNotTerminated()
		 {
			  InternalTransaction currentTransaction = _transaction;
			  if ( currentTransaction != null )
			  {
					currentTransaction.TerminationReason().ifPresent(status =>
					{
					 throw new TransactionTerminatedException( status );
					});
			  }
		 }

		 public virtual bool Open
		 {
			 get
			 {
				  return _isOpen;
			 }
		 }

		 public override GraphDatabaseQueryService Graph()
		 {
			  return _graph;
		 }

		 public override Statement Statement()
		 {
			  return _statement;
		 }

		 public override void Check()
		 {
			  KernelTransaction().assertOpen();
		 }

		 public override TxStateHolder StateView()
		 {
			  return ( KernelTransactionImplementation ) KernelTransaction();
		 }

		 public override Lock AcquireWriteLock( PropertyContainer p )
		 {
			  return _locker.exclusiveLock( KernelTransaction(), p );
		 }

		 public override Neo4Net.Kernel.api.KernelTransaction_Revertable RestrictCurrentTransaction( SecurityContext context )
		 {
			  return _transaction.overrideWith( context );
		 }

		 public override SecurityContext SecurityContext()
		 {
			  return SecurityContextConflict;
		 }

		 public override ResourceTracker ResourceTracker()
		 {
			  // We use the current statement as resourceTracker since it is attached to the KernelTransaction
			  // and is guaranteed to be cleaned up on transaction failure.
			  return _statement;
		 }

		 public override StatisticProvider KernelStatisticProvider()
		 {
			  return new TransactionalContextStatisticProvider( this, KernelTransaction().executionStatistics() );
		 }

		 private void CollectTransactionExecutionStatistic()
		 {
			  ExecutionStatistics stats = KernelTransaction().executionStatistics();
			  _pageHits += stats.PageHits();
			  _pageMisses += stats.PageFaults();
		 }

		 public virtual Neo4jTransactionalContext CopyFrom( GraphDatabaseQueryService graph, ThreadToStatementContextBridge txBridge, PropertyContainerLocker locker, InternalTransaction initialTransaction, Statement initialStatement, ExecutingQuery executingQuery )
		 {
			  return new Neo4jTransactionalContext( graph, txBridge, locker, initialTransaction, initialStatement, executingQuery, _kernel );
		 }

		 internal interface Creator
		 {
			  Neo4jTransactionalContext Create( InternalTransaction tx, Statement initialStatement, ExecutingQuery executingQuery );
		 }

		 private class TransactionalContextStatisticProvider : StatisticProvider
		 {
			 private readonly Neo4jTransactionalContext _outerInstance;

			  internal readonly ExecutionStatistics ExecutionStatistics;

			  internal TransactionalContextStatisticProvider( Neo4jTransactionalContext outerInstance, ExecutionStatistics executionStatistics )
			  {
				  this._outerInstance = outerInstance;
					this.ExecutionStatistics = executionStatistics;
			  }

			  public virtual long PageCacheHits
			  {
				  get
				  {
						return ExecutionStatistics.pageHits() + outerInstance.pageHits;
				  }
			  }

			  public virtual long PageCacheMisses
			  {
				  get
				  {
						return ExecutionStatistics.pageFaults() + outerInstance.pageMisses;
				  }
			  }
		 }
	}

}