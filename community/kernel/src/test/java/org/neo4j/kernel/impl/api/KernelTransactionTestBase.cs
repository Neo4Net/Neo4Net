using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;
	using Before = org.junit.Before;
	using Mockito = org.mockito.Mockito;


	using Org.Neo4j.Collection.pool;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Transaction_Type = Org.Neo4j.@internal.Kernel.Api.Transaction_Type;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using AutoIndexing = Org.Neo4j.Kernel.api.explicitindex.AutoIndexing;
	using ExplicitIndexTransactionState = Org.Neo4j.Kernel.api.txstate.ExplicitIndexTransactionState;
	using AuxiliaryTransactionStateManager = Org.Neo4j.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using StandardConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.StandardConstraintSemantics;
	using CanWrite = Org.Neo4j.Kernel.impl.factory.CanWrite;
	using ExplicitIndexStore = Org.Neo4j.Kernel.impl.index.ExplicitIndexStore;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using NoOpClient = Org.Neo4j.Kernel.impl.locking.NoOpClient;
	using SimpleStatementLocks = Org.Neo4j.Kernel.impl.locking.SimpleStatementLocks;
	using StatementLocks = Org.Neo4j.Kernel.impl.locking.StatementLocks;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Org.Neo4j.Kernel.impl.transaction.TransactionMonitor;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionTracer = Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using CollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactory;
	using OnHeapCollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.OnHeapCollectionsFactory;
	using MutableLongDiffSetsImpl = Org.Neo4j.Kernel.impl.util.diffsets.MutableLongDiffSetsImpl;
	using MemoryTracker = Org.Neo4j.Memory.MemoryTracker;
	using CpuClock = Org.Neo4j.Resources.CpuClock;
	using HeapAllocation = Org.Neo4j.Resources.HeapAllocation;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using ResourceLocker = Org.Neo4j.Storageengine.Api.@lock.ResourceLocker;
	using ReadableTransactionState = Org.Neo4j.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor;
	using Clocks = Org.Neo4j.Time.Clocks;
	using FakeClock = Org.Neo4j.Time.FakeClock;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.MockedNeoStores.mockedTokenHolders;

	public class KernelTransactionTestBase
	{
		private bool InstanceFieldsInitialized = false;

		public KernelTransactionTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			CommitProcess = new CapturingCommitProcess( this );
		}

		 protected internal readonly StorageEngine StorageEngine = mock( typeof( StorageEngine ) );
		 protected internal readonly NeoStores NeoStores = mock( typeof( NeoStores ) );
		 protected internal readonly MetaDataStore MetaDataStore = mock( typeof( MetaDataStore ) );
		 protected internal readonly StorageReader ReadLayer = mock( typeof( StorageReader ) );
		 protected internal readonly TransactionHooks Hooks = new TransactionHooks();
		 protected internal readonly AuxiliaryTransactionStateManager AuxTxStateManager = new KernelAuxTransactionStateManager();
		 protected internal readonly ExplicitIndexTransactionState ExplicitIndexState = mock( typeof( ExplicitIndexTransactionState ) );
		 protected internal readonly TransactionMonitor TransactionMonitor = mock( typeof( TransactionMonitor ) );
		 protected internal CapturingCommitProcess CommitProcess;
		 protected internal readonly TransactionHeaderInformation HeaderInformation = mock( typeof( TransactionHeaderInformation ) );
		 protected internal readonly TransactionHeaderInformationFactory HeaderInformationFactory = mock( typeof( TransactionHeaderInformationFactory ) );
		 protected internal readonly SchemaWriteGuard SchemaWriteGuard = mock( typeof( SchemaWriteGuard ) );
		 protected internal readonly FakeClock Clock = Clocks.fakeClock();
		 protected internal readonly Pool<KernelTransactionImplementation> TxPool = mock( typeof( Pool ) );
		 protected internal readonly StatementOperationParts StatementOperations = mock( typeof( StatementOperationParts ) );
		 protected internal CollectionsFactory CollectionsFactory;

		 private readonly long _defaultTransactionTimeoutMillis = Config.defaults().get(GraphDatabaseSettings.transaction_timeout).toMillis();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  CollectionsFactory = Mockito.spy( new TestCollectionsFactory( this ) );
			  when( HeaderInformation.AdditionalHeader ).thenReturn( new sbyte[0] );
			  when( HeaderInformationFactory.create() ).thenReturn(HeaderInformation);
			  when( NeoStores.MetaDataStore ).thenReturn( MetaDataStore );
			  when( StorageEngine.newReader() ).thenReturn(ReadLayer);
			  doAnswer( invocation => ( ( ICollection<StorageCommand> ) invocation.getArgument( 0 ) ).Add( new Command.RelationshipCountsCommand( 1, 2,3, 4L ) ) ).when( StorageEngine ).createCommands( anyCollection(), any(typeof(ReadableTransactionState)), any(typeof(StorageReader)), any(typeof(ResourceLocker)), anyLong(), any(typeof(Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor_Decorator)) );
		 }

		 public virtual KernelTransactionImplementation NewTransaction( long transactionTimeoutMillis )
		 {
			  return NewTransaction( 0, AUTH_DISABLED, transactionTimeoutMillis );
		 }

		 public virtual KernelTransactionImplementation NewTransaction( LoginContext loginContext )
		 {
			  return newTransaction( 0, loginContext );
		 }

		 public virtual KernelTransactionImplementation NewTransaction( LoginContext loginContext, Org.Neo4j.Kernel.impl.locking.Locks_Client locks )
		 {
			  return NewTransaction( 0, loginContext, locks, _defaultTransactionTimeoutMillis );
		 }

		 public virtual KernelTransactionImplementation NewTransaction( long lastTransactionIdWhenStarted, LoginContext loginContext )
		 {
			  return NewTransaction( lastTransactionIdWhenStarted, loginContext, _defaultTransactionTimeoutMillis );
		 }

		 public virtual KernelTransactionImplementation NewTransaction( long lastTransactionIdWhenStarted, LoginContext loginContext, long transactionTimeoutMillis )
		 {
			  return NewTransaction( lastTransactionIdWhenStarted, loginContext, new NoOpClient(), transactionTimeoutMillis );
		 }

		 public virtual KernelTransactionImplementation NewTransaction( long lastTransactionIdWhenStarted, LoginContext loginContext, Org.Neo4j.Kernel.impl.locking.Locks_Client locks, long transactionTimeout )
		 {
			  KernelTransactionImplementation tx = NewNotInitializedTransaction();
			  StatementLocks statementLocks = new SimpleStatementLocks( locks );
			  SecurityContext securityContext = loginContext.Authorize( s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  tx.Initialize( lastTransactionIdWhenStarted, BASE_TX_COMMIT_TIMESTAMP,statementLocks, Transaction_Type.@implicit, securityContext, transactionTimeout, 1L );
			  return tx;
		 }

		 public virtual KernelTransactionImplementation NewNotInitializedTransaction()
		 {
			  return new KernelTransactionImplementation( Config.defaults(), StatementOperations, SchemaWriteGuard, Hooks, null, null, HeaderInformationFactory, CommitProcess, TransactionMonitor, AuxTxStateManager, TxPool, Clock, new AtomicReference<CpuClock>(CpuClock.NOT_AVAILABLE), new AtomicReference<HeapAllocation>(HeapAllocation.NOT_AVAILABLE), Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer_Fields.Null, LockTracer.NONE, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, StorageEngine, new CanWrite(), AutoIndexing.UNSUPPORTED, mock(typeof(ExplicitIndexStore)), EmptyVersionContextSupplier.EMPTY, () => CollectionsFactory, new StandardConstraintSemantics(), mock(typeof(SchemaState)), mock(typeof(IndexingService)), mockedTokenHolders(), new Dependencies() );
		 }

		 public class CapturingCommitProcess : TransactionCommitProcess
		 {
			 private readonly KernelTransactionTestBase _outerInstance;

			 public CapturingCommitProcess( KernelTransactionTestBase outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal long TxId = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
			  public TransactionRepresentation Transaction;

			  public override long Commit( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
			  {
					Debug.Assert( Transaction == null, "Designed to only allow one transaction" );
					Debug.Assert( batch.Next() == null, "Designed to only allow one transaction" );
					Transaction = batch.TransactionRepresentation();
					return ++TxId;
			  }
		 }

		 private class TestCollectionsFactory : CollectionsFactory
		 {
			 private readonly KernelTransactionTestBase _outerInstance;

			 public TestCollectionsFactory( KernelTransactionTestBase outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


			  public override MutableLongSet NewLongSet()
			  {
					return OnHeapCollectionsFactory.INSTANCE.newLongSet();
			  }

			  public override MutableLongDiffSetsImpl NewLongDiffSets()
			  {
					return OnHeapCollectionsFactory.INSTANCE.newLongDiffSets();
			  }

			  public override MutableLongObjectMap<Value> NewValuesMap()
			  {
					return new LongObjectHashMap<Value>();
			  }

			  public virtual MemoryTracker MemoryTracker
			  {
				  get
				  {
						return OnHeapCollectionsFactory.INSTANCE.MemoryTracker;
				  }
			  }

			  public override void Release()
			  {
					// nop
			  }
		 }
	}

}