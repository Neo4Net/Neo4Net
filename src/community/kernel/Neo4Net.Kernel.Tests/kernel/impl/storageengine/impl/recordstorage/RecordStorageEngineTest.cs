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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;
	using InOrder = org.mockito.InOrder;


	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Neo4Net.Collections.Helpers;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using DelegatingPageCache = Neo4Net.Io.pagecache.DelegatingPageCache;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using BatchTransactionApplier = Neo4Net.Kernel.Impl.Api.BatchTransactionApplier;
	using BatchTransactionApplierFacade = Neo4Net.Kernel.Impl.Api.BatchTransactionApplierFacade;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using Lock = Neo4Net.Kernel.impl.locking.Lock;
	using LockGroup = Neo4Net.Kernel.impl.locking.LockGroup;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using FakeCommitment = Neo4Net.Kernel.impl.transaction.log.FakeCommitment;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using StoreFileMetadata = Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RecordStorageEngineRule = Neo4Net.Test.rule.RecordStorageEngineRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RecordStorageEngineTest
	{
		private bool InstanceFieldsInitialized = false;

		public RecordStorageEngineTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fsRule );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _pageCacheRule ).around( _testDirectory ).around( _storageEngineRule );
		}

		 private readonly RecordStorageEngineRule _storageEngineRule = new RecordStorageEngineRule();
		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private TestDirectory _testDirectory;
		 private readonly DatabaseHealth _databaseHealth = mock( typeof( DatabaseHealth ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(pageCacheRule).around(testDirectory).around(storageEngineRule);
		 public RuleChain RuleChain;

		 private static readonly System.Func<Optional<StoreType>, StoreType> _assertIsPresentAndGet = optional =>
		 {
		  assertTrue( "Expected optional to be present", optional.Present );
		  return optional.get();
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 30_000) public void shutdownRecordStorageEngineAfterFailedTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShutdownRecordStorageEngineAfterFailedTransaction()
		 {
			  RecordStorageEngine engine = BuildRecordStorageEngine();
			  Exception applicationError = ExecuteFailingTransaction( engine );
			  assertNotNull( applicationError );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void panicOnExceptionDuringCommandsApply()
		 public virtual void PanicOnExceptionDuringCommandsApply()
		 {
			  System.InvalidOperationException failure = new System.InvalidOperationException( "Too many open files" );
			  RecordStorageEngine engine = _storageEngineRule.getWith( _fsRule.get(), _pageCacheRule.getPageCache(_fsRule.get()), _testDirectory.databaseLayout() ).databaseHealth(_databaseHealth).transactionApplierTransformer(facade => TransactionApplierFacadeTransformer(facade, failure)).build();
			  CommandsToApply commandsToApply = mock( typeof( CommandsToApply ) );

			  try
			  {
					engine.Apply( commandsToApply, TransactionApplicationMode.INTERNAL );
					fail( "Exception expected" );
			  }
			  catch ( Exception exception )
			  {
					assertSame( failure, Exceptions.rootCause( exception ) );
			  }

			  verify( _databaseHealth ).panic( any( typeof( Exception ) ) );
		 }

		 private static BatchTransactionApplierFacade TransactionApplierFacadeTransformer( BatchTransactionApplierFacade facade, Exception failure )
		 {
			  return new FailingBatchTransactionApplierFacade( failure, facade );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databasePanicIsRaisedWhenTxApplicationFails() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabasePanicIsRaisedWhenTxApplicationFails()
		 {
			  RecordStorageEngine engine = BuildRecordStorageEngine();
			  Exception applicationError = ExecuteFailingTransaction( engine );
			  ArgumentCaptor<Exception> captor = ArgumentCaptor.forClass( typeof( Exception ) );
			  verify( _databaseHealth ).panic( captor.capture() );
			  Exception exception = captor.Value;
			  if ( exception is KernelException )
			  {
					assertThat( ( ( KernelException ) exception ).status(), @is(Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError) );
					exception = exception.InnerException;
			  }
			  assertThat( exception, @is( applicationError ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 30_000) public void obtainCountsStoreResetterAfterFailedTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ObtainCountsStoreResetterAfterFailedTransaction()
		 {
			  RecordStorageEngine engine = BuildRecordStorageEngine();
			  Exception applicationError = ExecuteFailingTransaction( engine );
			  assertNotNull( applicationError );

			  CountsTracker countsStore = engine.TestAccessNeoStores().Counts;
			  // possible to obtain a resetting updater that internally has a write lock on the counts store
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater updater = countsStore.Reset( 0 ) )
			  {
					assertNotNull( updater );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFlushStoresWithGivenIOLimiter()
		 public virtual void MustFlushStoresWithGivenIOLimiter()
		 {
			  IOLimiter limiter = Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited;
			  FileSystemAbstraction fs = _fsRule.get();
			  AtomicReference<IOLimiter> observedLimiter = new AtomicReference<IOLimiter>();
			  PageCache pageCache = new DelegatingPageCacheAnonymousInnerClass( this, _pageCacheRule.getPageCache( fs ), limiter, observedLimiter );

			  RecordStorageEngine engine = _storageEngineRule.getWith( fs, pageCache, _testDirectory.databaseLayout() ).build();
			  engine.FlushAndForce( limiter );

			  assertThat( observedLimiter.get(), sameInstance(limiter) );
		 }

		 private class DelegatingPageCacheAnonymousInnerClass : DelegatingPageCache
		 {
			 private readonly RecordStorageEngineTest _outerInstance;

			 private IOLimiter _limiter;
			 private AtomicReference<IOLimiter> _observedLimiter;

			 public DelegatingPageCacheAnonymousInnerClass( RecordStorageEngineTest outerInstance, PageCache getPageCache, IOLimiter limiter, AtomicReference<IOLimiter> observedLimiter ) : base( getPageCache )
			 {
				 this.outerInstance = outerInstance;
				 this._limiter = limiter;
				 this._observedLimiter = observedLimiter;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(Neo4Net.io.pagecache.IOLimiter limiter) throws java.io.IOException
			 public override void flushAndForce( IOLimiter limiter )
			 {
				  base.flushAndForce( limiter );
				  _observedLimiter.set( limiter );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllStoreFiles()
		 public virtual void ShouldListAllStoreFiles()
		 {
			  RecordStorageEngine engine = BuildRecordStorageEngine();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata> files = engine.listStorageFiles();
			  ICollection<StoreFileMetadata> files = engine.ListStorageFiles();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<File> currentFiles = Files.Select( StoreFileMetadata::file ).collect( Collectors.toSet() );
			  // current engine files should contain everything except another count store file and label scan store
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  ISet<File> allPossibleFiles = databaseLayout.StoreFiles();
			  allPossibleFiles.remove( databaseLayout.CountStoreB() );
			  allPossibleFiles.remove( databaseLayout.LabelScanStore() );

			  assertEquals( currentFiles, allPossibleFiles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseLockGroupAfterAppliers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseLockGroupAfterAppliers()
		 {
			  // given
			  long nodeId = 5;
			  LockService lockService = mock( typeof( LockService ) );
			  Lock nodeLock = mock( typeof( Lock ) );
			  when( lockService.AcquireNodeLock( nodeId, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock ) ).thenReturn( nodeLock );
			  System.Action<bool> applierCloseCall = mock( typeof( System.Action ) ); // <-- simply so that we can use InOrder mockito construct
			  CapturingBatchTransactionApplierFacade applier = new CapturingBatchTransactionApplierFacade( this, applierCloseCall );
			  RecordStorageEngine engine = RecordStorageEngineBuilder().lockService(lockService).transactionApplierTransformer(applier.wrapAroundActualApplier).build();
			  CommandsToApply commandsToApply = mock( typeof( CommandsToApply ) );
			  when( commandsToApply.Accept( any() ) ).thenAnswer(invocationOnMock =>
			  {
				// Visit one node command
				Visitor<StorageCommand, IOException> visitor = invocationOnMock.getArgument( 0 );
				NodeRecord after = new NodeRecord( nodeId );
				after.InUse = true;
				visitor.visit( new Command.NodeCommand( new NodeRecord( nodeId ), after ) );
				return null;
			  });

			  // when
			  engine.Apply( commandsToApply, TransactionApplicationMode.INTERNAL );

			  // then
			  InOrder inOrder = inOrder( lockService, applierCloseCall, nodeLock );
			  inOrder.verify( lockService ).acquireNodeLock( nodeId, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  inOrder.verify( applierCloseCall ).accept( true );
			  inOrder.verify( nodeLock, times( 1 ) ).release();
			  inOrder.verifyNoMoreInteractions();
		 }

		 private RecordStorageEngine BuildRecordStorageEngine()
		 {
			  return RecordStorageEngineBuilder().build();
		 }

		 private RecordStorageEngineRule.Builder RecordStorageEngineBuilder()
		 {
			  return _storageEngineRule.getWith( _fsRule.get(), _pageCacheRule.getPageCache(_fsRule.get()), _testDirectory.databaseLayout() ).databaseHealth(_databaseHealth);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Exception executeFailingTransaction(RecordStorageEngine engine) throws java.io.IOException
		 private static Exception ExecuteFailingTransaction( RecordStorageEngine engine )
		 {
			  Exception applicationError = new UnderlyingStorageException( "No space left on device" );
			  TransactionToApply txToApply = NewTransactionThatFailsWith( applicationError );
			  try
			  {
					engine.Apply( txToApply, TransactionApplicationMode.INTERNAL );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertSame( applicationError, Exceptions.rootCause( e ) );
			  }
			  return applicationError;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Neo4Net.kernel.impl.api.TransactionToApply newTransactionThatFailsWith(Exception error) throws java.io.IOException
		 private static TransactionToApply NewTransactionThatFailsWith( Exception error )
		 {
			  TransactionRepresentation transaction = mock( typeof( TransactionRepresentation ) );
			  when( transaction.AdditionalHeader() ).thenReturn(new sbyte[0]);
			  // allow to build validated index updates but fail on actual tx application
			  doThrow( error ).when( transaction ).accept( any() );

			  long txId = ThreadLocalRandom.current().nextLong(0, 1000);
			  TransactionToApply txToApply = new TransactionToApply( transaction );
			  FakeCommitment commitment = new FakeCommitment( txId, mock( typeof( TransactionIdStore ) ) );
			  commitment.HasExplicitIndexChanges = false;
			  txToApply.Commitment( commitment, txId );
			  return txToApply;
		 }

		 private class FailingBatchTransactionApplierFacade : BatchTransactionApplierFacade
		 {
			  internal readonly Exception Failure;

			  internal FailingBatchTransactionApplierFacade( Exception failure, params BatchTransactionApplier[] appliers ) : base( appliers )
			  {
					this.Failure = failure;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
			  public override void Close()
			  {
					throw Failure;
			  }
		 }

		 private class CapturingBatchTransactionApplierFacade : BatchTransactionApplierFacade
		 {
			 private readonly RecordStorageEngineTest _outerInstance;

			  internal readonly System.Action<bool> ApplierCloseCall;
			  internal BatchTransactionApplierFacade Actual;

			  internal CapturingBatchTransactionApplierFacade( RecordStorageEngineTest outerInstance, System.Action<bool> applierCloseCall )
			  {
				  this._outerInstance = outerInstance;
					this.ApplierCloseCall = applierCloseCall;
			  }

			  internal virtual CapturingBatchTransactionApplierFacade WrapAroundActualApplier( BatchTransactionApplierFacade actual )
			  {
					this.Actual = actual;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.kernel.impl.api.TransactionApplier startTx(Neo4Net.Kernel.Api.StorageEngine.CommandsToApply transaction) throws java.io.IOException
			  public override TransactionApplier StartTx( CommandsToApply transaction )
			  {
					return Actual.startTx( transaction );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.kernel.impl.api.TransactionApplier startTx(Neo4Net.Kernel.Api.StorageEngine.CommandsToApply transaction, Neo4Net.kernel.impl.locking.LockGroup lockGroup) throws java.io.IOException
			  public override TransactionApplier StartTx( CommandsToApply transaction, LockGroup lockGroup )
			  {
					return Actual.startTx( transaction, lockGroup );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
			  public override void Close()
			  {
					ApplierCloseCall.accept( true );
					Actual.close();
			  }
		 }
	}

}