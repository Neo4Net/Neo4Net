using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus
{

	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Invokes the supplied task continuously when started. The supplied task
	/// should be short since the abort flag is checked in between invocations.
	/// </summary>
	public class ContinuousJob : LifecycleAdapter
	{
		 private readonly AbortableJob _abortableJob;
		 private readonly Log _log;
		 private readonly Thread _thread;

		 public ContinuousJob( ThreadFactory threadFactory, ThreadStart task, LogProvider logProvider )
		 {
			  this._abortableJob = new AbortableJob( task );
			  this._thread = threadFactory.newThread( _abortableJob );
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void Start()
		 {
			  _abortableJob.keepRunning = true;
			  _thread.Start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _log.info( "ContinuousJob " + _thread.Name + " stopping" );
			  _abortableJob.keepRunning = false;
			  _thread.Join();
		 }

		 private class AbortableJob : ThreadStart
		 {
			  internal readonly ThreadStart Task;
			  internal volatile bool KeepRunning;

			  internal AbortableJob( ThreadStart task )
			  {
					this.Task = task;
			  }

			  public override void Run()
			  {
					while ( KeepRunning )
					{
						 Task.run();
					}
			  }
		 }
	}

}