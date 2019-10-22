using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.cluster
{

	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Executor that executes the Runnables when drain() is called. Allows async jobs to be scheduled, and then
	/// run in a synchronous fashion.
	/// </summary>
	public class DelayedDirectExecutor : AbstractExecutorService
	{
		 private IList<ThreadStart> _runnables = new List<ThreadStart>();

		 private readonly Log _log;

		 public DelayedDirectExecutor( LogProvider logProvider )
		 {
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void Shutdown()
		 {
		 }

		 public override IList<ThreadStart> ShutdownNow()
		 {
			  return Collections.emptyList();
		 }

		 public override bool Shutdown
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override bool Terminated
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override bool AwaitTermination( long timeout, TimeUnit unit )
		 {
			  return true;
		 }

		 public override void Execute( ThreadStart command )
		 {
			 lock ( this )
			 {
				  _runnables.Add( command );
			 }
		 }

		 public virtual void Drain()
		 {
			  IList<ThreadStart> currentRunnables;
			  lock ( this )
			  {
					currentRunnables = _runnables;
					_runnables = new List<ThreadStart>();
			  }
			  foreach ( ThreadStart runnable in currentRunnables )
			  {
					try
					{
						 runnable.run();
					}
					catch ( Exception t )
					{
						 _log.error( "Runnable failed", t );
					}
			  }
		 }
	}

}