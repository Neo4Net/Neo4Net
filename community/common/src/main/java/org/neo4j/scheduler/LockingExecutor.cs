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
namespace Org.Neo4j.Scheduler
{

	public class LockingExecutor : Executor
	{
		 private readonly JobScheduler _jobScheduler;
		 private readonly Group _group;
		 private readonly AtomicBoolean _latch = new AtomicBoolean( false );

		 public LockingExecutor( JobScheduler jobScheduler, Group group )
		 {
			  this._jobScheduler = jobScheduler;
			  this._group = group;
		 }

		 public override void Execute( ThreadStart runnable )
		 {
			  if ( _latch.compareAndSet( false, true ) )
			  {
					_jobScheduler.schedule(_group, () =>
					{
					 try
					 {
						  runnable.run();
					 }
					 finally
					 {
						  _latch.set( false );
					 }
					});
			  }
		 }
	}

}