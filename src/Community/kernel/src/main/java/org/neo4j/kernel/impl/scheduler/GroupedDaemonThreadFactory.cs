using System.Text;
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
namespace Neo4Net.Kernel.impl.scheduler
{

	using Group = Neo4Net.Scheduler.Group;
	using SchedulerThreadFactory = Neo4Net.Scheduler.SchedulerThreadFactory;

	internal sealed class GroupedDaemonThreadFactory : SchedulerThreadFactory
	{
		 private readonly Group _group;
		 private readonly ThreadGroup _threadGroup;

		 internal GroupedDaemonThreadFactory( Group group, ThreadGroup parentThreadGroup )
		 {
			  this._group = group;
			  _threadGroup = new ThreadGroup( parentThreadGroup, group.groupName() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public Thread newThread(@SuppressWarnings("NullableProblems") Runnable job)
		 public override Thread NewThread( ThreadStart job )
		 {
			  Thread thread = new Thread(_threadGroup, job, _group.threadName()() =>
			  {
				 StringBuilder sb = ( new StringBuilder( "Thread[" ) ).Append( Name );
				 ThreadGroup group = ThreadGroup;
				 string sep = ", in ";
				 while ( group != null )
				 {
					  sb.Append( sep ).Append( group.Name );
					  group = group.Parent;
					  sep = "/";
				 }
				 return sb.Append( ']' ).ToString();
			  });
			  thread.Daemon = true;
			  return thread;
		 }

		 public override ForkJoinWorkerThread NewThread( ForkJoinPool pool )
		 {
			  // We do this complicated dance of allocating the ForkJoinThread in a separate thread,
			  // because there is no way to give it a specific ThreadGroup, other than through inheritance
			  // from the allocating thread.
			  ForkJoinPool.ForkJoinWorkerThreadFactory factory = ForkJoinPool.defaultForkJoinWorkerThreadFactory;
			  AtomicReference<ForkJoinWorkerThread> reference = new AtomicReference<ForkJoinWorkerThread>();
			  Thread allocator = NewThread( () => reference.set(factory.newThread(pool)) );
			  allocator.Start();
			  do
			  {
					try
					{
						 allocator.Join();
					}
					catch ( InterruptedException )
					{
					}
			  } while ( reference.get() == null );
			  ForkJoinWorkerThread worker = reference.get();
			  worker.Name = _group.threadName();
			  return worker;
		 }
	}

}