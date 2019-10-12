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
namespace Org.Neo4j.Test
{

	public class ThreadTestUtils
	{
		 private ThreadTestUtils()
		 {
		 }

		 public static Thread Fork( ThreadStart runnable )
		 {
			  string name = "Forked-from-" + Thread.CurrentThread.Name;
			  Thread thread = new Thread( runnable, name );
			  thread.Daemon = true;
			  thread.Start();
			  return thread;
		 }

		 public static Future<T> ForkFuture<T>( Callable<T> callable )
		 {
			  FutureTask<T> task = new FutureTask<T>( callable );
			  Fork( task );
			  return task;
		 }

		 public static void AwaitThreadState( Thread thread, long maxWaitMillis, Thread.State first, params Thread.State[] rest )
		 {
			  EnumSet<Thread.State> set = EnumSet.of( first, rest );
			  long deadline = maxWaitMillis + DateTimeHelper.CurrentUnixTimeMillis();
			  Thread.State currentState;
			  do
			  {
					currentState = thread.State;
					if ( DateTimeHelper.CurrentUnixTimeMillis() > deadline )
					{
						 throw new AssertionError( "Timed out waiting for thread state of <" + set + ">: " + thread + " (state = " + thread.State + ")" );
					}
			  } while ( !set.contains( currentState ) );
		 }
	}

}