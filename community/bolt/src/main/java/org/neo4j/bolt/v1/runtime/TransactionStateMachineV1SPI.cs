using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Bolt.v1.runtime
{

	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using BoltResultHandle = Org.Neo4j.Bolt.runtime.BoltResultHandle;
	using TransactionStateMachineSPI = Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI;
	using QueryResultProvider = Org.Neo4j.Cypher.@internal.javacompat.QueryResultProvider;
	using Result = Org.Neo4j.Graphdb.Result;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using TransactionIdTracker = Org.Neo4j.Kernel.api.txtracking.TransactionIdTracker;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using PropertyContainerLocker = Org.Neo4j.Kernel.impl.coreapi.PropertyContainerLocker;
	using Neo4jTransactionalContextFactory = Org.Neo4j.Kernel.impl.query.Neo4jTransactionalContextFactory;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Org.Neo4j.Kernel.impl.query.TransactionalContextFactory;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@implicit;

	public class TransactionStateMachineV1SPI : TransactionStateMachineSPI
	{
		 private static readonly PropertyContainerLocker _locker = new PropertyContainerLocker();

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
//ORIGINAL LINE: public void awaitUpToDate(long oldestAcceptableTxId) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
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
					tx = _db.beginTransaction( type, loginContext );
			  }
			  else
			  {
					tx = _db.beginTransaction( type, loginContext, txTimeout.toMillis(), TimeUnit.MILLISECONDS );
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
			  return Neo4jTransactionalContextFactory.create( queryService, _locker );
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
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltResult start() throws org.neo4j.internal.kernel.api.exceptions.KernelException
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