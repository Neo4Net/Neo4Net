using System;
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
namespace Neo4Net.Index.@internal.gbptree
{

	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// Runs cleanup work as they're added in <seealso cref="add(CleanupJob)"/>, but the thread that calls <seealso cref="add(CleanupJob)"/> will not execute them itself.
	/// </summary>
	public class GroupingRecoveryCleanupWorkCollector : RecoveryCleanupWorkCollector
	{
		 private readonly BlockingQueue<CleanupJob> _jobs = new LinkedBlockingQueue<CleanupJob>();
		 private readonly JobScheduler _jobScheduler;
		 private volatile bool _started;
		 private JobHandle _handle;

		 /// <param name="jobScheduler"> <seealso cref="JobScheduler"/> to queue <seealso cref="CleanupJob"/> into. </param>
		 public GroupingRecoveryCleanupWorkCollector( JobScheduler jobScheduler )
		 {
			  this._jobScheduler = jobScheduler;
		 }

		 public override void Init()
		 {
			  _started = false;
			  if ( !_jobs.Empty )
			  {
					StringJoiner joiner = new StringJoiner( string.Format( "%n  " ), "Did not expect there to be any cleanup jobs still here. Jobs[", "]" );
					ConsumeAndCloseJobs( cj => joiner.add( _jobs.ToString() ) );
					throw new System.InvalidOperationException( joiner.ToString() );
			  }
			  ScheduleJobs();
		 }

		 public override void Add( CleanupJob job )
		 {
			  if ( _started )
			  {
					throw new System.InvalidOperationException( "Index clean jobs can't be added after collector start." );
			  }
			  _jobs.add( job );
		 }

		 public override void Start()
		 {
			  _started = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws java.util.concurrent.ExecutionException, InterruptedException
		 public override void Shutdown()
		 {
			  _started = true;
			  if ( _handle != null )
			  {
					// Also set the started flag which acts as a signal to exit the scheduled job on empty queue,
					// this is of course a special case where perhaps not start() gets called, i.e. if something fails
					// before reaching that phase in the lifecycle.
					_handle.waitTermination();
			  }
			  ConsumeAndCloseJobs(cj =>
			  {
			  });
		 }

		 private void ScheduleJobs()
		 {
			  _handle = _jobScheduler.schedule( Group.STORAGE_MAINTENANCE, AllJobs() );
		 }

		 private ThreadStart AllJobs()
		 {
			  return () => executeWithExecutor(executor =>
			  {
						  CleanupJob job = null;
						  do
						  {
								try
								{
									 job = _jobs.poll( 100, TimeUnit.MILLISECONDS );
									 if ( job != null )
									 {
										  job.Run( executor );
									 }
								}
								catch ( Exception )
								{
									 // There's no audience for these exceptions. The jobs themselves know if they've failed and communicates
									 // that to its tree. The scheduled job is just a vessel for running these cleanup jobs.
								}
								finally
								{
									 if ( job != null )
									 {
										  job.Close();
									 }
								}
						  } while ( !_jobs.Empty || !_started );
						  // Even if there are no jobs in the queue then continue looping until we go to started state
			  });
		 }

		 private void ConsumeAndCloseJobs( System.Action<CleanupJob> consumer )
		 {
			  CleanupJob job;
			  while ( ( job = _jobs.poll() ) != null )
			  {
					consumer( job );
					job.Close();
			  }
		 }
	}

}