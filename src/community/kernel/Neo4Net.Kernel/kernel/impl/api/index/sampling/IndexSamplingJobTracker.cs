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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{

	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	public class IndexSamplingJobTracker
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_canSchedule = @lock.newCondition();
			_allJobsFinished = @lock.newCondition();
		}

		 private readonly JobScheduler _jobScheduler;
		 private readonly int _jobLimit;
		 private readonly ISet<long> _executingJobs;
		 private readonly Lock @lock = new ReentrantLock( true );
		 private Condition _canSchedule;
		 private Condition _allJobsFinished;

		 private bool _stopped;

		 public IndexSamplingJobTracker( IndexSamplingConfig config, JobScheduler jobScheduler )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._jobScheduler = jobScheduler;
			  this._jobLimit = config.JobLimit();
			  this._executingJobs = new HashSet<long>();
		 }

		 public virtual bool CanExecuteMoreSamplingJobs()
		 {
			  @lock.@lock();
			  try
			  {
					return !_stopped && _executingJobs.Count < _jobLimit;
			  }
			  finally
			  {
					@lock.unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void scheduleSamplingJob(final IndexSamplingJob samplingJob)
		 public virtual void ScheduleSamplingJob( IndexSamplingJob samplingJob )
		 {
			  @lock.@lock();
			  try
			  {
					if ( _stopped )
					{
						 return;
					}

					long indexId = samplingJob.IndexId();
					if ( _executingJobs.Contains( indexId ) )
					{
						 return;
					}

					_executingJobs.Add( indexId );
					_jobScheduler.schedule(Group.INDEX_SAMPLING, () =>
					{
					 try
					 {
						  samplingJob.run();
					 }
					 finally
					 {
						  SamplingJobCompleted( samplingJob );
					 }
					});
			  }
			  finally
			  {
					@lock.unlock();
			  }
		 }

		 private void SamplingJobCompleted( IndexSamplingJob samplingJob )
		 {
			  @lock.@lock();
			  try
			  {
					_executingJobs.remove( samplingJob.IndexId() );
					_canSchedule.signalAll();
					_allJobsFinished.signalAll();
			  }
			  finally
			  {
					@lock.unlock();
			  }
		 }

		 public virtual void WaitUntilCanExecuteMoreSamplingJobs()
		 {
			  @lock.@lock();
			  try
			  {
					while ( !CanExecuteMoreSamplingJobs() )
					{
						 if ( _stopped )
						 {
							  return;
						 }

						 _canSchedule.awaitUninterruptibly();
					}
			  }
			  finally
			  {
					@lock.unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitAllJobs(long time, java.util.concurrent.TimeUnit unit) throws InterruptedException
		 public virtual void AwaitAllJobs( long time, TimeUnit unit )
		 {
			  @lock.@lock();
			  try
			  {
					if ( _stopped )
					{
						 return;
					}

					while ( _executingJobs.Count > 0 )
					{
						 _allJobsFinished.await( time, unit );
					}
			  }
			  finally
			  {
					@lock.unlock();
			  }
		 }

		 public virtual void StopAndAwaitAllJobs()
		 {
			  @lock.@lock();
			  try
			  {
					_stopped = true;

					while ( _executingJobs.Count > 0 )
					{
						 _allJobsFinished.awaitUninterruptibly();
					}
			  }
			  finally
			  {
					@lock.unlock();
			  }
		 }
	}

}