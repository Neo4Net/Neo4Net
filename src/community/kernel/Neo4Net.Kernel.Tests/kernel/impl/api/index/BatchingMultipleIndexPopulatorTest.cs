using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using NeoStoreIndexStoreView = Neo4Net.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.BatchingMultipleIndexPopulator.AWAIT_TIMEOUT_MINUTES_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.BatchingMultipleIndexPopulator.BATCH_SIZE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.BatchingMultipleIndexPopulator.TASK_QUEUE_SIZE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.IndexPopulationFailure.failure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.MultipleIndexPopulator.QUEUE_THRESHOLD_NAME;

	public class BatchingMultipleIndexPopulatorTest
	{
		 public const int PROPERTY_ID = 1;
		 public const int LABEL_ID = 1;
		 private readonly IndexDescriptor _index1 = TestIndexDescriptorFactory.forLabel( 1, 1 );
		 private readonly IndexDescriptor _index42 = TestIndexDescriptorFactory.forLabel( 42, 42 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  ClearProperty( QUEUE_THRESHOLD_NAME );
			  ClearProperty( TASK_QUEUE_SIZE_NAME );
			  ClearProperty( AWAIT_TIMEOUT_MINUTES_NAME );
			  ClearProperty( BATCH_SIZE_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populateFromQueueDoesNothingIfThresholdNotReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulateFromQueueDoesNothingIfThresholdNotReached()
		 {
			  SetProperty( QUEUE_THRESHOLD_NAME, 5 );

			  BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( mock( typeof( IndexStoreView ) ), ImmediateExecutor(), NullLogProvider.Instance, mock(typeof(SchemaState)) );

			  IndexPopulator populator = AddPopulator( batchingPopulator, _index1 );
			  IndexUpdater updater = mock( typeof( IndexUpdater ) );
			  when( populator.NewPopulatingUpdater( any() ) ).thenReturn(updater);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update1 = add(1, index1.schema(), "foo");
			  IndexEntryUpdate<object> update1 = add( 1, _index1.schema(), "foo" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update2 = add(2, index1.schema(), "bar");
			  IndexEntryUpdate<object> update2 = add( 2, _index1.schema(), "bar" );
			  batchingPopulator.QueueUpdate( update1 );
			  batchingPopulator.QueueUpdate( update2 );

			  batchingPopulator.PopulateFromQueueBatched( 42 );

			  verify( updater, never() ).process(any());
			  verify( populator, never() ).newPopulatingUpdater(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populateFromQueuePopulatesWhenThresholdReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulateFromQueuePopulatesWhenThresholdReached()
		 {
			  SetProperty( QUEUE_THRESHOLD_NAME, 2 );

			  NeoStores neoStores = mock( typeof( NeoStores ) );
			  NodeStore nodeStore = mock( typeof( NodeStore ) );
			  when( neoStores.NodeStore ).thenReturn( nodeStore );

			  NeoStoreIndexStoreView storeView = new NeoStoreIndexStoreView( LockService.NO_LOCK_SERVICE, neoStores );
			  BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( storeView, ImmediateExecutor(), NullLogProvider.Instance, mock(typeof(SchemaState)) );

			  IndexPopulator populator1 = AddPopulator( batchingPopulator, _index1 );
			  IndexUpdater updater1 = mock( typeof( IndexUpdater ) );
			  when( populator1.NewPopulatingUpdater( any() ) ).thenReturn(updater1);

			  IndexPopulator populator2 = AddPopulator( batchingPopulator, _index42 );
			  IndexUpdater updater2 = mock( typeof( IndexUpdater ) );
			  when( populator2.NewPopulatingUpdater( any() ) ).thenReturn(updater2);

			  batchingPopulator.IndexAllEntities();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update1 = add(1, index1.schema(), "foo");
			  IndexEntryUpdate<object> update1 = add( 1, _index1.schema(), "foo" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update2 = add(2, index42.schema(), "bar");
			  IndexEntryUpdate<object> update2 = add( 2, _index42.schema(), "bar" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> update3 = add(3, index1.schema(), "baz");
			  IndexEntryUpdate<object> update3 = add( 3, _index1.schema(), "baz" );
			  batchingPopulator.QueueUpdate( update1 );
			  batchingPopulator.QueueUpdate( update2 );
			  batchingPopulator.QueueUpdate( update3 );

			  batchingPopulator.PopulateFromQueueBatched( 42 );

			  verify( updater1 ).process( update1 );
			  verify( updater1 ).process( update3 );
			  verify( updater2 ).process( update2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executorShutdownAfterStoreScanCompletes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExecutorShutdownAfterStoreScanCompletes()
		 {
			  EntityUpdates update = NodeUpdates( 1, PROPERTY_ID, "foo", LABEL_ID );
			  IndexStoreView storeView = NewStoreView( update );

			  ExecutorService executor = ImmediateExecutor();
			  when( executor.awaitTermination( anyLong(), any() ) ).thenReturn(true);

			  BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( storeView, executor, NullLogProvider.Instance, mock( typeof( SchemaState ) ) );

			  StoreScan<IndexPopulationFailedKernelException> storeScan = batchingPopulator.IndexAllEntities();
			  verify( executor, never() ).shutdown();

			  storeScan.Run();
			  verify( executor, never() ).shutdown();
			  verify( executor, never() ).awaitTermination(anyLong(), any());

			  batchingPopulator.Close( true );
			  verify( executor ).shutdown();
			  verify( executor ).awaitTermination( anyLong(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void executorForcefullyShutdownIfStoreScanFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExecutorForcefullyShutdownIfStoreScanFails()
		 {
			  IndexStoreView storeView = mock( typeof( IndexStoreView ) );
			  StoreScan<Exception> failingStoreScan = mock( typeof( StoreScan ) );
			  Exception scanError = new Exception();
			  doThrow( scanError ).when( failingStoreScan ).run();
			  when( storeView.VisitNodes( any(), any(), any(), any(), anyBoolean() ) ).thenReturn(failingStoreScan);

			  ExecutorService executor = ImmediateExecutor();
			  when( executor.awaitTermination( anyLong(), any() ) ).thenReturn(true);

			  BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( storeView, executor, NullLogProvider.Instance, mock( typeof( SchemaState ) ) );

			  StoreScan<IndexPopulationFailedKernelException> storeScan = batchingPopulator.IndexAllEntities();
			  verify( executor, never() ).shutdown();

			  try
			  {
					storeScan.Run();
					fail( "Exception expected" );
			  }
			  catch ( Exception t )
			  {
					assertSame( scanError, t );
			  }

			  verify( executor, never() ).shutdownNow();
			  verify( executor, never() ).awaitTermination(anyLong(), any());

			  batchingPopulator.Close( false );
			  verify( executor ).shutdownNow();
			  verify( executor ).awaitTermination( anyLong(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pendingBatchesFlushedAfterStoreScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PendingBatchesFlushedAfterStoreScan()
		 {
			  EntityUpdates update1 = NodeUpdates( 1, PROPERTY_ID, "foo", LABEL_ID );
			  EntityUpdates update2 = NodeUpdates( 2, PROPERTY_ID, "bar", LABEL_ID );
			  EntityUpdates update3 = NodeUpdates( 3, PROPERTY_ID, "baz", LABEL_ID );
			  EntityUpdates update42 = NodeUpdates( 4, 42, "42", 42 );
			  IndexStoreView storeView = NewStoreView( update1, update2, update3, update42 );

			  BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( storeView, SameThreadExecutor(), NullLogProvider.Instance, mock(typeof(SchemaState)) );

			  IndexPopulator populator1 = AddPopulator( batchingPopulator, _index1 );
			  IndexPopulator populator42 = AddPopulator( batchingPopulator, _index42 );

			  batchingPopulator.IndexAllEntities().run();

			  verify( populator1 ).add( ForUpdates( _index1, update1, update2, update3 ) );
			  verify( populator42 ).add( ForUpdates( _index42, update42 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void batchIsFlushedWhenThresholdReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BatchIsFlushedWhenThresholdReached()
		 {
			  SetProperty( BATCH_SIZE_NAME, 2 );

			  EntityUpdates update1 = NodeUpdates( 1, PROPERTY_ID, "foo", LABEL_ID );
			  EntityUpdates update2 = NodeUpdates( 2, PROPERTY_ID, "bar", LABEL_ID );
			  EntityUpdates update3 = NodeUpdates( 3, PROPERTY_ID, "baz", LABEL_ID );
			  IndexStoreView storeView = NewStoreView( update1, update2, update3 );

			  BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( storeView, SameThreadExecutor(), NullLogProvider.Instance, mock(typeof(SchemaState)) );

			  IndexPopulator populator = AddPopulator( batchingPopulator, _index1 );

			  batchingPopulator.IndexAllEntities().run();

			  verify( populator ).add( ForUpdates( _index1, update1, update2 ) );
			  verify( populator ).add( ForUpdates( _index1, update3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatorMarkedAsFailed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatorMarkedAsFailed()
		 {
			  SetProperty( BATCH_SIZE_NAME, 2 );

			  EntityUpdates update1 = NodeUpdates( 1, PROPERTY_ID, "aaa", LABEL_ID );
			  EntityUpdates update2 = NodeUpdates( 1, PROPERTY_ID, "bbb", LABEL_ID );
			  IndexStoreView storeView = NewStoreView( update1, update2 );

			  Exception batchFlushError = new Exception( "Batch failed" );

			  IndexPopulator populator;
			  ExecutorService executor = Executors.newSingleThreadExecutor();
			  try
			  {
					BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( storeView, executor, NullLogProvider.Instance, mock( typeof( SchemaState ) ) );

					populator = AddPopulator( batchingPopulator, _index1 );
					IList<IndexEntryUpdate<IndexDescriptor>> expected = ForUpdates( _index1, update1, update2 );
					doThrow( batchFlushError ).when( populator ).add( expected );

					batchingPopulator.IndexAllEntities().run();
			  }
			  finally
			  {
					executor.shutdown();
					executor.awaitTermination( 1, TimeUnit.MINUTES );
			  }

			  verify( populator ).markAsFailed( failure( batchFlushError ).asString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populatorMarkedAsFailedAndUpdatesNotAdded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulatorMarkedAsFailedAndUpdatesNotAdded()
		 {
			  SetProperty( BATCH_SIZE_NAME, 2 );

			  EntityUpdates update1 = NodeUpdates( 1, PROPERTY_ID, "aaa", LABEL_ID );
			  EntityUpdates update2 = NodeUpdates( 1, PROPERTY_ID, "bbb", LABEL_ID );
			  EntityUpdates update3 = NodeUpdates( 1, PROPERTY_ID, "ccc", LABEL_ID );
			  EntityUpdates update4 = NodeUpdates( 1, PROPERTY_ID, "ddd", LABEL_ID );
			  EntityUpdates update5 = NodeUpdates( 1, PROPERTY_ID, "eee", LABEL_ID );
			  IndexStoreView storeView = NewStoreView( update1, update2, update3, update4, update5 );

			  Exception batchFlushError = new Exception( "Batch failed" );

			  BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( storeView, SameThreadExecutor(), NullLogProvider.Instance, mock(typeof(SchemaState)) );

			  IndexPopulator populator = AddPopulator( batchingPopulator, _index1 );
			  doThrow( batchFlushError ).when( populator ).add( ForUpdates( _index1, update3, update4 ) );

			  batchingPopulator.IndexAllEntities().run();

			  verify( populator ).add( ForUpdates( _index1, update1, update2 ) );
			  verify( populator ).add( ForUpdates( _index1, update3, update4 ) );
			  verify( populator ).markAsFailed( failure( batchFlushError ).asString() );
			  verify( populator, never() ).add(ForUpdates(_index1, update5));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyBatchesInParallel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyBatchesInParallel()
		 {
			  // given
			  SetProperty( BATCH_SIZE_NAME, 2 );
			  EntityUpdates[] updates = new EntityUpdates[9];
			  for ( int i = 0; i < updates.Length; i++ )
			  {
					updates[i] = NodeUpdates( i, PROPERTY_ID, i.ToString(), LABEL_ID );
			  }
			  IndexStoreView storeView = NewStoreView( updates );
			  ExecutorService executor = SameThreadExecutor();
			  BatchingMultipleIndexPopulator batchingPopulator = new BatchingMultipleIndexPopulator( storeView, executor, NullLogProvider.Instance, mock( typeof( SchemaState ) ) );
			  AddPopulator( batchingPopulator, _index1 );

			  // when
			  batchingPopulator.IndexAllEntities().run();

			  // then
			  verify( executor, atLeast( 5 ) ).execute( any( typeof( ThreadStart ) ) );
		 }

		 private IList<IndexEntryUpdate<IndexDescriptor>> ForUpdates( IndexDescriptor index, params EntityUpdates[] updates )
		 {
			  return Iterables.asList( Iterables.concat( Iterables.map( update => update.forIndexKeys( Iterables.asIterable( index ) ), Arrays.asList( updates ) ) ) );
		 }

		 private EntityUpdates NodeUpdates( int nodeId, int propertyId, string propertyValue, params long[] labelIds )
		 {
			  return EntityUpdates.ForEntity( ( long ) nodeId, false ).withTokens( labelIds ).withTokensAfter( labelIds ).added( propertyId, Values.of( propertyValue ) ).build();
		 }

		 private static IndexPopulator AddPopulator( BatchingMultipleIndexPopulator batchingPopulator, IndexDescriptor descriptor )
		 {
			  IndexPopulator populator = mock( typeof( IndexPopulator ) );

			  IndexProxyFactory indexProxyFactory = mock( typeof( IndexProxyFactory ) );
			  FailedIndexProxyFactory failedIndexProxyFactory = mock( typeof( FailedIndexProxyFactory ) );
			  FlippableIndexProxy flipper = new FlippableIndexProxy();
			  flipper.FlipTarget = indexProxyFactory;

			  batchingPopulator.AddPopulator( populator, descriptor.WithId( 1 ).withoutCapabilities(), flipper, failedIndexProxyFactory, "testIndex" );

			  return populator;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static IndexStoreView newStoreView(EntityUpdates... updates)
		 private static IndexStoreView NewStoreView( params EntityUpdates[] updates )
		 {
			  IndexStoreView storeView = mock( typeof( IndexStoreView ) );
			  when( storeView.VisitNodes( any(), any(), any(), any(), anyBoolean() ) ).thenAnswer(invocation =>
			  {
				Visitor<EntityUpdates, IndexPopulationFailedKernelException> visitorArg = invocation.getArgument( 2 );
				return new IndexEntryUpdateScan( updates, visitorArg );
			  });
			  return storeView;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.concurrent.ExecutorService sameThreadExecutor() throws InterruptedException
		 private static ExecutorService SameThreadExecutor()
		 {
			  ExecutorService executor = ImmediateExecutor();
			  when( executor.awaitTermination( anyLong(), any() ) ).thenReturn(true);
			  doAnswer(invocation =>
			  {
				( ( ThreadStart ) invocation.getArgument( 0 ) ).run();
				return null;
			  }).when( executor ).execute( any() );
			  return executor;
		 }

		 private static void SetProperty( string name, int value )
		 {
			  FeatureToggles.set( typeof( BatchingMultipleIndexPopulator ), name, value );
		 }

		 private static void ClearProperty( string name )
		 {
			  FeatureToggles.clear( typeof( BatchingMultipleIndexPopulator ), name );
		 }

		 private static ExecutorService ImmediateExecutor()
		 {
			  ExecutorService result = mock( typeof( ExecutorService ) );
			  doAnswer(invocation =>
			  {
				invocation.getArgument<ThreadStart>( 0 ).run();
				return null;
			  }).when( result ).execute( any( typeof( ThreadStart ) ) );
			  return result;
		 }

		 private class IndexEntryUpdateScan : StoreScan<IndexPopulationFailedKernelException>
		 {
			  internal readonly EntityUpdates[] Updates;
			  internal readonly Visitor<EntityUpdates, IndexPopulationFailedKernelException> Visitor;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool StopConflict;

			  internal IndexEntryUpdateScan( EntityUpdates[] updates, Visitor<EntityUpdates, IndexPopulationFailedKernelException> visitor )
			  {
					this.Updates = updates;
					this.Visitor = visitor;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run() throws org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException
			  public override void Run()
			  {
					foreach ( EntityUpdates update in Updates )
					{
						 if ( StopConflict )
						 {
							  return;
						 }
						 Visitor.visit( update );
					}
			  }

			  public override void Stop()
			  {
					StopConflict = true;
			  }

			  public override void AcceptUpdate<T1>( MultipleIndexPopulator.MultipleIndexUpdater updater, IndexEntryUpdate<T1> update, long currentlyIndexedNodeId )
			  {
			  }

			  public virtual PopulationProgress Progress
			  {
				  get
				  {
						return Neo4Net.Storageengine.Api.schema.PopulationProgress_Fields.None;
				  }
			  }
		 }
	}

}