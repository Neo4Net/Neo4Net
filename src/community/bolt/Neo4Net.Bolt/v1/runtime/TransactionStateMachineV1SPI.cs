using System;
using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.runtime
{

	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using BoltResultHandle = Neo4Net.Bolt.runtime.BoltResultHandle;
	using TransactionStateMachineSPI = Neo4Net.Bolt.runtime.TransactionStateMachineSPI;
	using QueryResultProvider = Neo4Net.Cypher.Internal.javacompat.QueryResultProvider;
	using Result = Neo4Net.GraphDb.Result;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using TransactionIdTracker = Neo4Net.Kernel.api.txtracking.TransactionIdTracker;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using IPropertyContainerLocker = Neo4Net.Kernel.impl.coreapi.PropertyContainerLocker;
	using Neo4NetTransactionalContextFactory = Neo4Net.Kernel.impl.query.Neo4NetTransactionalContextFactory;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Neo4Net.Kernel.impl.query.TransactionalContextFactory;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Transaction_Type.@implicit;

	public class TransactionStateMachineV1SPI : TransactionStateMachineSPI
	{
		 private static readonly IPropertyContainerLocker _locker = new IPropertyContainerLocker();

		 private readonly GraphDatabaseAPI _db;
		 private readonly BoltChannel _boltChannel;
		 private readonly ThreadToStatementContextBridge _txBridge;
		 private readonly QueryExecutionEngine _queryExecutionEngine;
		 private readonly TransactionIdTracker _transactionIdTracker;
		 private readonly TransactionalContextFactory _contextFactory;
		 private readonly Duration _txAwaitDuration;
		 private readonly Clock _clock;

		 public TransactionStateMachineV1SPI( GraphDatabaseAPI db, BoltChannel boltChannel, Duration txAwaitDuration, Clock clock )
		 {
			  this._db = db;
			  this._boltChannel = boltChannel;
			  this._txBridge = ResolveDependency( db, typeof( ThreadToStatementContextBridge ) );
			  this._queryExecutionEngine = ResolveDependency( db, typeof( QueryExecutionEngine ) );
			  this._transactionIdTracker = NewTransactionIdTracker( db );
			  this._contextFactory = NewTransactionalContextFactory( db );
			  this._txAwaitDuration = txAwaitDuration;
			  this._clock = clock;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitUpToDate(long oldestAcceptableTxId) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 public override void AwaitUpToDate( long oldestAcceptableTxId )
		 {
			  _transactionIdTracker.awaitUpToDate( oldestAcceptableTxId, _txAwaitDuration );
		 }

		 public override long NewestEncounteredTxId()
		 {
			  return _transactionIdTracker.newestEncounteredTxId();
		 }

		 public override KernelTransaction BeginTransaction( LoginContext loginContext, Duration txTimeout, IDictionary<string, object> txMetadata )
		 {
			  BeginTransaction( @explicit, loginContext, txTimeout, txMetadata );
			  return _txBridge.getKernelTransactionBoundToThisThread( false );
		 }

		 public override void BindTransactionToCurrentThread( KernelTransaction tx )
		 {
			  _txBridge.bindTransactionToCurrentThread( tx );
		 }

		 public override void UnbindTransactionFromCurrentThread()
		 {
			  _txBridge.unbindTransactionFromCurrentThread();
		 }

		 public override bool IsPeriodicCommit( string query )
		 {
			  return _queryExecutionEngine.isPeriodicCommit( query );
		 }

		 public override BoltResultHandle ExecuteQuery( LoginContext loginContext, string statement, MapValue @params, Duration txTimeout, IDictionary<string, object> txMetadata )
		 {
			  InternalTransaction internalTransaction = BeginTransaction( @implicit, loginContext, txTimeout, txMetadata );
			  TransactionalContext transactionalContext = _contextFactory.newContext( _boltChannel.info(), internalTransaction, statement, @params );
			  return NewBoltResultHandle( statement, @params, transactionalContext );
		 }

		 protected internal virtual BoltResultHandle NewBoltResultHandle( string statement, MapValue @params, TransactionalContext transactionalContext )
		 {
			  return new BoltResultHandleV1( this, statement, @params, transactionalContext );
		 }

		 private InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, Duration txTimeout, IDictionary<string, object> txMetadata )
		 {
			  InternalTransaction tx;
			  if ( txTimeout == null )
			  {
					tx = _db.BeginTransaction( type, loginContext );
			  }
			  else
			  {
					tx = _db.BeginTransaction( type, loginContext, txTimeout.toMillis(), TimeUnit.MILLISECONDS );
			  }

			  if ( txMetadata != null )
			  {
					tx.MetaData = txMetadata;
			  }
			  return tx;
		 }

		 private static TransactionIdTracker NewTransactionIdTracker( GraphDatabaseAPI db )
		 {
			  System.Func<TransactionIdStore> transactionIdStoreSupplier = Db.DependencyResolver.provideDependency( typeof( TransactionIdStore ) );
			  AvailabilityGuard guard = ResolveDependency( db, typeof( DatabaseAvailabilityGuard ) );
			  return new TransactionIdTracker( transactionIdStoreSupplier, guard );
		 }

		 private static TransactionalContextFactory NewTransactionalContextFactory( GraphDatabaseAPI db )
		 {
			  GraphDatabaseQueryService queryService = ResolveDependency( db, typeof( GraphDatabaseQueryService ) );
			  return Neo4NetTransactionalContextFactory.create( queryService, _locker );
		 }

		 private static T ResolveDependency<T>( GraphDatabaseAPI db, Type clazz )
		 {
				 clazz = typeof( T );
			  return Db.DependencyResolver.resolveDependency( clazz );
		 }

		 public class BoltResultHandleV1 : BoltResultHandle
		 {
			 private readonly TransactionStateMachineV1SPI _outerInstance;

			  internal readonly string Statement;
			  internal readonly MapValue Params;
			  internal readonly TransactionalContext TransactionalContext;

			  public BoltResultHandleV1( TransactionStateMachineV1SPI outerInstance, string statement, MapValue @params, TransactionalContext transactionalContext )
			  {
				  this._outerInstance = outerInstance;
					this.Statement = statement;
					this.Params = @params;
					this.TransactionalContext = transactionalContext;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.bolt.runtime.BoltResult start() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
			  public override BoltResult Start()
			  {
					try
					{
						 Result result = outerInstance.queryExecutionEngine.ExecuteQuery( Statement, Params, TransactionalContext );
						 if ( result is QueryResultProvider )
						 {
							  return NewBoltResult( ( QueryResultProvider ) result, outerInstance.clock );
						 }
						 else
						 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
							  throw new System.InvalidOperationException( format( "Unexpected query execution result. Expected to get instance of %s but was %s.", typeof( QueryResultProvider ).FullName, result.GetType().FullName ) );
						 }
					}
					catch ( KernelException e )
					{
						 Close( false );
						 throw new QueryExecutionKernelException( e );
					}
					catch ( Exception e )
					{
						 Close( false );
						 throw e;
					}
			  }

			  protected internal virtual BoltResult NewBoltResult( QueryResultProvider result, Clock clock )
			  {
					return new CypherAdapterStream( result.QueryResult(), clock );
			  }

			  public override void Close( bool success )
			  {
					TransactionalContext.close( success );
			  }

			  public override void Terminate()
			  {
					TransactionalContext.terminate();
			  }
		 }
	}

}