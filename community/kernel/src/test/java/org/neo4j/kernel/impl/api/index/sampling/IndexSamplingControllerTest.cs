using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Api.index.sampling
{
	using Test = org.junit.Test;

	using Predicates = Org.Neo4j.Function.Predicates;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using DoubleLatch = Org.Neo4j.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.sampling.IndexSamplingMode.BACKGROUND_REBUILD_UPDATED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.sampling.IndexSamplingMode.TRIGGER_REBUILD_UPDATED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class IndexSamplingControllerTest
	{
		private bool InstanceFieldsInitialized = false;

		public IndexSamplingControllerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_descriptor = forSchema( forLabel( 3, 4 ), PROVIDER_DESCRIPTOR ).withId( _indexId ).withoutCapabilities();
			_anotherDescriptor = forSchema( forLabel( 5, 6 ), PROVIDER_DESCRIPTOR ).withId( _anotherIndexId ).withoutCapabilities();
			when( _samplingConfig.backgroundSampling() ).thenReturn(true);
			when( _samplingConfig.jobLimit() ).thenReturn(1);
			when( _indexProxy.Descriptor ).thenReturn( _descriptor );
			when( _anotherIndexProxy.Descriptor ).thenReturn( _anotherDescriptor );
			when( _snapshotProvider.indexMapSnapshot() ).thenReturn(_indexMap);
			when( _jobFactory.create( _indexId, _indexProxy ) ).thenReturn( _job );
			when( _jobFactory.create( _anotherIndexId, _anotherIndexProxy ) ).thenReturn( _anotherJob );
			_indexMap.putIndexProxy( _indexProxy );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartASamplingJobForEachIndexInTheDB()
		 public virtual void ShouldStartASamplingJobForEachIndexInTheDB()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( false ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(true);
			  when( _indexProxy.State ).thenReturn( ONLINE );

			  // when
			  controller.SampleIndexes( BACKGROUND_REBUILD_UPDATED );

			  // then
			  verify( _jobFactory ).create( _indexId, _indexProxy );
			  verify( _tracker ).scheduleSamplingJob( _job );
			  verify( _tracker, times( 2 ) ).canExecuteMoreSamplingJobs();
			  verifyNoMoreInteractions( _jobFactory, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartAJobIfTheIndexIsNotOnline()
		 public virtual void ShouldNotStartAJobIfTheIndexIsNotOnline()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( false ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(true);
			  when( _indexProxy.State ).thenReturn( POPULATING );

			  // when
			  controller.SampleIndexes( BACKGROUND_REBUILD_UPDATED );

			  // then
			  verify( _tracker, times( 2 ) ).canExecuteMoreSamplingJobs();
			  verifyNoMoreInteractions( _jobFactory, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartAJobIfTheTrackerCannotHandleIt()
		 public virtual void ShouldNotStartAJobIfTheTrackerCannotHandleIt()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( false ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(false);
			  when( _indexProxy.State ).thenReturn( ONLINE );

			  // when
			  controller.SampleIndexes( BACKGROUND_REBUILD_UPDATED );

			  // then
			  verify( _tracker, times( 1 ) ).canExecuteMoreSamplingJobs();
			  verifyNoMoreInteractions( _jobFactory, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotEmptyQueueConcurrently()
		 public virtual void ShouldNotEmptyQueueConcurrently()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger totalCount = new java.util.concurrent.atomic.AtomicInteger(0);
			  AtomicInteger totalCount = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger concurrentCount = new java.util.concurrent.atomic.AtomicInteger(0);
			  AtomicInteger concurrentCount = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch jobLatch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch jobLatch = new DoubleLatch();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch testLatch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch testLatch = new DoubleLatch();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ThreadLocal<bool> hasRun = ThreadLocal.withInitial(() -> false);
			  ThreadLocal<bool> hasRun = ThreadLocal.withInitial( () => false );

			  IndexSamplingJobFactory jobFactory = ( _indexId, proxy ) =>
			  {
				// make sure we execute this once per thread
				if ( hasRun.get() )
				{
					 return null;
				}
				hasRun.set( true );

				if ( !concurrentCount.compareAndSet( 0, 1 ) )
				{
					 throw new System.InvalidOperationException( "count !== 0 on create" );
				}
				totalCount.incrementAndGet();

				jobLatch.WaitForAllToStart();
				testLatch.StartAndWaitForAllToStart();
				jobLatch.WaitForAllToFinish();

				concurrentCount.decrementAndGet();

				testLatch.Finish();
				return null;
			  };

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingController controller = new IndexSamplingController(samplingConfig, jobFactory, jobQueue, tracker, snapshotProvider, scheduler, always(false));
			  IndexSamplingController controller = new IndexSamplingController( _samplingConfig, jobFactory, _jobQueue, _tracker, _snapshotProvider, _scheduler, Always( false ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(true);
			  when( _indexProxy.State ).thenReturn( ONLINE );

			  // when running once
			  ( new Thread( RunController( controller, BACKGROUND_REBUILD_UPDATED ) ) ).Start();

			  jobLatch.StartAndWaitForAllToStart();
			  testLatch.WaitForAllToStart();

			  // then blocking on first job
			  assertEquals( 1, concurrentCount.get() );
			  assertEquals( 1, totalCount.get() );

			  // when running a second time
			  controller.SampleIndexes( BACKGROUND_REBUILD_UPDATED );

			  // then no concurrent job execution
			  jobLatch.Finish();
			  testLatch.WaitForAllToFinish();

			  // and finally exactly one job has run to completion
			  assertEquals( 0, concurrentCount.get() );
			  assertEquals( 1, totalCount.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleAllTheIndexes()
		 public virtual void ShouldSampleAllTheIndexes()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( false ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(true);
			  when( _indexProxy.State ).thenReturn( ONLINE );
			  when( _anotherIndexProxy.State ).thenReturn( ONLINE );
			  _indexMap.putIndexProxy( _anotherIndexProxy );

			  // when
			  controller.SampleIndexes( TRIGGER_REBUILD_UPDATED );

			  // then
			  verify( _jobFactory ).create( _indexId, _indexProxy );
			  verify( _tracker ).scheduleSamplingJob( _job );
			  verify( _jobFactory ).create( _anotherIndexId, _anotherIndexProxy );
			  verify( _tracker ).scheduleSamplingJob( _anotherJob );

			  verify( _tracker, times( 2 ) ).waitUntilCanExecuteMoreSamplingJobs();
			  verifyNoMoreInteractions( _jobFactory, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleAllTheOnlineIndexes()
		 public virtual void ShouldSampleAllTheOnlineIndexes()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( false ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(true);
			  when( _indexProxy.State ).thenReturn( ONLINE );
			  when( _anotherIndexProxy.State ).thenReturn( POPULATING );
			  _indexMap.putIndexProxy( _anotherIndexProxy );

			  // when
			  controller.SampleIndexes( TRIGGER_REBUILD_UPDATED );

			  // then
			  verify( _jobFactory ).create( _indexId, _indexProxy );
			  verify( _tracker ).scheduleSamplingJob( _job );

			  verify( _tracker, times( 2 ) ).waitUntilCanExecuteMoreSamplingJobs();
			  verifyNoMoreInteractions( _jobFactory, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartOtherSamplingWhenSamplingAllTheIndexes()
		 public virtual void ShouldNotStartOtherSamplingWhenSamplingAllTheIndexes()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger totalCount = new java.util.concurrent.atomic.AtomicInteger(0);
			  AtomicInteger totalCount = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger concurrentCount = new java.util.concurrent.atomic.AtomicInteger(0);
			  AtomicInteger concurrentCount = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch jobLatch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch jobLatch = new DoubleLatch();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch testLatch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch testLatch = new DoubleLatch();

			  IndexSamplingJobFactory jobFactory = ( _indexId, proxy ) =>
			  {
				if ( !concurrentCount.compareAndSet( 0, 1 ) )
				{
					 throw new System.InvalidOperationException( "count !== 0 on create" );
				}
				totalCount.incrementAndGet();
				jobLatch.WaitForAllToStart();
				testLatch.StartAndWaitForAllToStart();
				jobLatch.WaitForAllToFinish();
				concurrentCount.decrementAndGet();
				testLatch.Finish();
				return null;
			  };

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingController controller = new IndexSamplingController(samplingConfig, jobFactory, jobQueue, tracker, snapshotProvider, scheduler, always(true));
			  IndexSamplingController controller = new IndexSamplingController( _samplingConfig, jobFactory, _jobQueue, _tracker, _snapshotProvider, _scheduler, Always( true ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(true);
			  when( _indexProxy.State ).thenReturn( ONLINE );

			  // when running once
			  ( new Thread( RunController( controller, TRIGGER_REBUILD_UPDATED ) ) ).Start();

			  jobLatch.StartAndWaitForAllToStart();
			  testLatch.WaitForAllToStart();

			  // then blocking on first job
			  assertEquals( 1, concurrentCount.get() );

			  // when running a second time
			  controller.SampleIndexes( BACKGROUND_REBUILD_UPDATED );

			  // then no concurrent job execution
			  jobLatch.Finish();
			  testLatch.WaitForAllToFinish();

			  // and finally exactly one job has run to completion
			  assertEquals( 0, concurrentCount.get() );
			  assertEquals( 1, totalCount.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverOnlineIndex()
		 public virtual void ShouldRecoverOnlineIndex()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( true ) );
			  when( _indexProxy.State ).thenReturn( ONLINE );

			  // when
			  controller.RecoverIndexSamples();

			  // then
			  verify( _jobFactory ).create( _indexId, _indexProxy );
			  verify( _job ).run();
			  verifyNoMoreInteractions( _jobFactory, _job, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRecoverOfflineIndex()
		 public virtual void ShouldNotRecoverOfflineIndex()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( true ) );
			  when( _indexProxy.State ).thenReturn( FAILED );

			  // when
			  controller.RecoverIndexSamples();

			  // then
			  verifyNoMoreInteractions( _jobFactory, _job, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRecoverOnlineIndexIfNotNeeded()
		 public virtual void ShouldNotRecoverOnlineIndexIfNotNeeded()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( false ) );
			  when( _indexProxy.State ).thenReturn( ONLINE );

			  // when
			  controller.RecoverIndexSamples();

			  // then
			  verifyNoMoreInteractions( _jobFactory, _job, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleIndex()
		 public virtual void ShouldSampleIndex()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( false ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(true);
			  when( _indexProxy.State ).thenReturn( ONLINE );
			  when( _anotherIndexProxy.State ).thenReturn( ONLINE );
			  _indexMap.putIndexProxy( _anotherIndexProxy );

			  // when
			  controller.SampleIndex( _indexId, TRIGGER_REBUILD_UPDATED );

			  // then
			  verify( _jobFactory, times( 1 ) ).create( _indexId, _indexProxy );
			  verify( _tracker, times( 1 ) ).scheduleSamplingJob( _job );
			  verify( _jobFactory, never() ).create(_anotherIndexId, _anotherIndexProxy);
			  verify( _tracker, never() ).scheduleSamplingJob(_anotherJob);

			  verify( _tracker, times( 1 ) ).waitUntilCanExecuteMoreSamplingJobs();
			  verifyNoMoreInteractions( _jobFactory, _tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartForSingleIndexAJobIfTheTrackerCannotHandleIt()
		 public virtual void ShouldNotStartForSingleIndexAJobIfTheTrackerCannotHandleIt()
		 {
			  // given
			  IndexSamplingController controller = NewSamplingController( Always( false ) );
			  when( _tracker.canExecuteMoreSamplingJobs() ).thenReturn(false);
			  when( _indexProxy.State ).thenReturn( ONLINE );

			  // when
			  controller.SampleIndex( _indexId, BACKGROUND_REBUILD_UPDATED );

			  // then
			  verify( _tracker, times( 1 ) ).canExecuteMoreSamplingJobs();
			  verifyNoMoreInteractions( _jobFactory, _tracker );
		 }

		 private class Always : IndexSamplingController.RecoveryCondition
		 {
			  internal readonly bool Ans;

			  internal Always( bool ans )
			  {
					this.Ans = ans;
			  }

			  public override bool Test( StoreIndexDescriptor descriptor )
			  {
					return Ans;
			  }
		 }

		 private readonly IndexSamplingConfig _samplingConfig = mock( typeof( IndexSamplingConfig ) );
		 private readonly IndexSamplingJobFactory _jobFactory = mock( typeof( IndexSamplingJobFactory ) );
		 private readonly IndexSamplingJobQueue<long> _jobQueue = new IndexSamplingJobQueue<long>( Predicates.alwaysTrue() );
		 private readonly IndexSamplingJobTracker _tracker = mock( typeof( IndexSamplingJobTracker ) );
		 private readonly JobScheduler _scheduler = mock( typeof( JobScheduler ) );
		 private readonly IndexMapSnapshotProvider _snapshotProvider = mock( typeof( IndexMapSnapshotProvider ) );
		 private readonly IndexMap _indexMap = new IndexMap();
		 private readonly long _indexId = 2;
		 private readonly long _anotherIndexId = 3;
		 private readonly IndexProxy _indexProxy = mock( typeof( IndexProxy ) );
		 private readonly IndexProxy _anotherIndexProxy = mock( typeof( IndexProxy ) );
		 private CapableIndexDescriptor _descriptor;
		 private CapableIndexDescriptor _anotherDescriptor;
		 private readonly IndexSamplingJob _job = mock( typeof( IndexSamplingJob ) );
		 private readonly IndexSamplingJob _anotherJob = mock( typeof( IndexSamplingJob ) );

		 private IndexSamplingController.RecoveryCondition Always( bool ans )
		 {
			  return new Always( ans );
		 }

		 private IndexSamplingController NewSamplingController( IndexSamplingController.RecoveryCondition recoveryPredicate )
		 {
			  return new IndexSamplingController( _samplingConfig, _jobFactory, _jobQueue, _tracker, _snapshotProvider, _scheduler, recoveryPredicate );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable runController(final IndexSamplingController controller, final IndexSamplingMode mode)
		 private ThreadStart RunController( IndexSamplingController controller, IndexSamplingMode mode )
		 {
			  return () => controller.sampleIndexes(mode);
		 }
	}

}