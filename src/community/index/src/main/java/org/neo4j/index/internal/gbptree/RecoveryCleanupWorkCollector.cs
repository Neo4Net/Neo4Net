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
namespace Neo4Net.Index.Internal.gbptree
{

	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// Place to add recovery cleanup work to be done as part of recovery of <seealso cref="GBPTree"/>.
	/// <para>
	/// <seealso cref="Lifecycle.init()"/> must prepare implementing class to be reused, probably by cleaning current state. After
	/// this, implementing class must be ready to receive new jobs through <seealso cref="add(CleanupJob)"/>.
	/// </para>
	/// <para>
	/// Jobs may be processed during <seealso cref="add(CleanupJob) add"/> or <seealso cref="Lifecycle.start() start"/>.
	/// </para>
	/// <para>
	/// Take full responsibility for closing added <seealso cref="CleanupJob CleanupJobs"/> as soon as possible after run.
	/// </para>
	/// </summary>
	public abstract class RecoveryCleanupWorkCollector : LifecycleAdapter
	{
		 private static ImmediateRecoveryCleanupWorkCollector _immediateInstance;
		 private static IgnoringRecoveryCleanupWorkCollector _ignoringInstance;

		 /// <summary>
		 /// Adds <seealso cref="CleanupJob"/> to this collector.
		 /// </summary>
		 /// <param name="job"> cleanup job to perform, now or at some point in the future. </param>
		 internal abstract void Add( CleanupJob job );

		 internal virtual void ExecuteWithExecutor( CleanupJobGroupAction action )
		 {
			  ExecutorService executor = Executors.newFixedThreadPool( Runtime.Runtime.availableProcessors() );
			  try
			  {
					action( executor );
			  }
			  finally
			  {
					ShutdownExecutorAndVerifyNoLeaks( executor );
			  }
		 }
		 private void ShutdownExecutorAndVerifyNoLeaks( ExecutorService executor )
		 {
			  IList<ThreadStart> leakedTasks = executor.shutdownNow();
			  if ( leakedTasks.Count != 0 )
			  {
					throw new System.InvalidOperationException( "Tasks leaked from CleanupJob. Tasks where " + leakedTasks.ToString() );
			  }
		 }

		 /// <summary>
		 /// <seealso cref="CleanupJob.run( ExecutorService ) Runs"/> <seealso cref="add(CleanupJob) added"/> cleanup jobs right away in the thread
		 /// calling <seealso cref="add(CleanupJob)"/>.
		 /// </summary>
		 public static RecoveryCleanupWorkCollector Immediate()
		 {
			  if ( _immediateInstance == null )
			  {
					_immediateInstance = new ImmediateRecoveryCleanupWorkCollector();
			  }
			  return _immediateInstance;
		 }

		 /// <summary>
		 /// Ignore all clean jobs.
		 /// </summary>
		 public static RecoveryCleanupWorkCollector Ignore()
		 {
			  if ( _ignoringInstance == null )
			  {
					_ignoringInstance = new IgnoringRecoveryCleanupWorkCollector();
			  }
			  return _ignoringInstance;
		 }

		 /// <summary>
		 /// <seealso cref="RecoveryCleanupWorkCollector"/> which runs added <seealso cref="CleanupJob"/> as part of the <seealso cref="add(CleanupJob)"/>
		 /// call in the caller thread.
		 /// </summary>
		 internal class ImmediateRecoveryCleanupWorkCollector : RecoveryCleanupWorkCollector
		 {
			  public override void Add( CleanupJob job )
			  {
					ExecuteWithExecutor(executor =>
					{
					 try
					 {
						  job.Run( executor );
					 }
					 finally
					 {
						  job.Close();
					 }
					});
			  }
		 }

		 /// <summary>
		 /// <seealso cref="RecoveryCleanupWorkCollector"/> ignoring all <seealso cref="CleanupJob"/> added to it.
		 /// </summary>
		 internal class IgnoringRecoveryCleanupWorkCollector : RecoveryCleanupWorkCollector
		 {
			  public override void Add( CleanupJob job )
			  {
					job.Close();
			  }
		 }

		 delegate void CleanupJobGroupAction( ExecutorService executor );
	}

}