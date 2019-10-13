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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.sampling.IndexSamplingMode.BACKGROUND_REBUILD_UPDATED;

	public class IndexSamplingController
	{
		 private readonly IndexSamplingJobFactory _jobFactory;
		 private readonly IndexSamplingJobQueue<long> _jobQueue;
		 private readonly IndexSamplingJobTracker _jobTracker;
		 private readonly IndexMapSnapshotProvider _indexMapSnapshotProvider;
		 private readonly JobScheduler _scheduler;
		 private readonly RecoveryCondition _indexRecoveryCondition;
		 private readonly bool _backgroundSampling;
		 private readonly Lock _samplingLock = new ReentrantLock();

		 private JobHandle _backgroundSamplingHandle;

		 // use IndexSamplingControllerFactory.create do not instantiate directly
		 internal IndexSamplingController( IndexSamplingConfig config, IndexSamplingJobFactory jobFactory, IndexSamplingJobQueue<long> jobQueue, IndexSamplingJobTracker jobTracker, IndexMapSnapshotProvider indexMapSnapshotProvider, JobScheduler scheduler, RecoveryCondition indexRecoveryCondition )
		 {
			  this._backgroundSampling = config.BackgroundSampling();
			  this._jobFactory = jobFactory;
			  this._indexMapSnapshotProvider = indexMapSnapshotProvider;
			  this._jobQueue = jobQueue;
			  this._jobTracker = jobTracker;
			  this._scheduler = scheduler;
			  this._indexRecoveryCondition = indexRecoveryCondition;
		 }

		 public virtual void SampleIndexes( IndexSamplingMode mode )
		 {
			  IndexMap indexMap = _indexMapSnapshotProvider.indexMapSnapshot();
			  _jobQueue.addAll( !mode.sampleOnlyIfUpdated, PrimitiveLongCollections.toIterator( indexMap.IndexIds() ) );
			  ScheduleSampling( mode, indexMap );
		 }

		 public virtual void SampleIndex( long indexId, IndexSamplingMode mode )
		 {
			  IndexMap indexMap = _indexMapSnapshotProvider.indexMapSnapshot();
			  _jobQueue.add( !mode.sampleOnlyIfUpdated, indexId );
			  ScheduleSampling( mode, indexMap );
		 }

		 public virtual void RecoverIndexSamples()
		 {
			  _samplingLock.@lock();
			  try
			  {
					IndexMap indexMap = _indexMapSnapshotProvider.indexMapSnapshot();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator indexIds = indexMap.indexIds();
					LongIterator indexIds = indexMap.IndexIds();
					while ( indexIds.hasNext() )
					{
						 long indexId = indexIds.next();
						 if ( _indexRecoveryCondition.test( indexMap.GetIndexProxy( indexId ).Descriptor ) )
						 {
							  SampleIndexOnCurrentThread( indexMap, indexId );
						 }
					}
			  }
			  finally
			  {
					_samplingLock.unlock();
			  }
		 }

		 public interface RecoveryCondition
		 {
			  bool Test( StoreIndexDescriptor descriptor );
		 }

		 private void ScheduleSampling( IndexSamplingMode mode, IndexMap indexMap )
		 {
			  if ( mode.blockUntilAllScheduled )
			  {
					// Wait until last sampling job has been started
					ScheduleAllSampling( indexMap );
			  }
			  else
			  {
					// Try to schedule as many sampling jobs as possible
					TryScheduleSampling( indexMap );
			  }
		 }

		 private void TryScheduleSampling( IndexMap indexMap )
		 {
			  if ( TryEmptyLock() )
			  {
					try
					{
						 while ( _jobTracker.canExecuteMoreSamplingJobs() )
						 {
							  long? indexId = _jobQueue.poll();
							  if ( indexId == null )
							  {
									return;
							  }

							  SampleIndexOnTracker( indexMap, indexId.Value );
						 }
					}
					finally
					{
						 _samplingLock.unlock();
					}
			  }
		 }

		 private bool TryEmptyLock()
		 {
			  try
			  {
					return _samplingLock.tryLock( 0, SECONDS );
			  }
			  catch ( InterruptedException )
			  {
					// ignored
					return false;
			  }
		 }

		 private void ScheduleAllSampling( IndexMap indexMap )
		 {
			  _samplingLock.@lock();
			  try
			  {
					IEnumerable<long> indexIds = _jobQueue.pollAll();

					foreach ( long? indexId in indexIds )
					{
						 _jobTracker.waitUntilCanExecuteMoreSamplingJobs();
						 SampleIndexOnTracker( indexMap, indexId.Value );
					}
			  }
			  finally
			  {
					_samplingLock.unlock();
			  }
		 }

		 private void SampleIndexOnTracker( IndexMap indexMap, long indexId )
		 {
			  IndexSamplingJob job = CreateSamplingJob( indexMap, indexId );
			  if ( job != null )
			  {
					_jobTracker.scheduleSamplingJob( job );
			  }
		 }

		 private void SampleIndexOnCurrentThread( IndexMap indexMap, long indexId )
		 {
			  IndexSamplingJob job = CreateSamplingJob( indexMap, indexId );
			  if ( job != null )
			  {
					job.run();
			  }
		 }

		 private IndexSamplingJob CreateSamplingJob( IndexMap indexMap, long indexId )
		 {
			  IndexProxy proxy = indexMap.GetIndexProxy( indexId );
			  if ( proxy == null || proxy.State != InternalIndexState.ONLINE )
			  {
					return null;
			  }
			  return _jobFactory.create( indexId, proxy );
		 }

		 public virtual void Start()
		 {
			  if ( _backgroundSampling )
			  {
					ThreadStart samplingRunner = () => sampleIndexes(BACKGROUND_REBUILD_UPDATED);
					_backgroundSamplingHandle = _scheduler.scheduleRecurring( Group.INDEX_SAMPLING, samplingRunner, 10, SECONDS );
			  }
		 }

		 public virtual void Stop()
		 {
			  if ( _backgroundSamplingHandle != null )
			  {
					_backgroundSamplingHandle.cancel( true );
			  }
			  _jobTracker.stopAndAwaitAllJobs();
		 }
	}

}