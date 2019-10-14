using System;
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
namespace Neo4Net.Helpers
{
	using Test = org.junit.jupiter.api.Test;


	using Barrier = Neo4Net.Test.Barrier;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class TaskCoordinatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCancelAllTasksWithOneCall()
		 internal virtual void ShouldCancelAllTasksWithOneCall()
		 {
			  // given
			  TaskCoordinator coordinator = new TaskCoordinator( 1, TimeUnit.MILLISECONDS );

			  using ( TaskControl task1 = coordinator.NewInstance(), TaskControl task2 = coordinator.NewInstance(), TaskControl task3 = coordinator.NewInstance() )
			  {
					assertFalse( task1.CancellationRequested() );
					assertFalse( task2.CancellationRequested() );
					assertFalse( task3.CancellationRequested() );

					// when
					coordinator.Cancel();

					// then
					assertTrue( task1.CancellationRequested() );
					assertTrue( task2.CancellationRequested() );
					assertTrue( task3.CancellationRequested() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAwaitCompletionOfAllTasks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAwaitCompletionOfAllTasks()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TaskCoordinator coordinator = new TaskCoordinator(1, java.util.concurrent.TimeUnit.MILLISECONDS);
			  TaskCoordinator coordinator = new TaskCoordinator( 1, TimeUnit.MILLISECONDS );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<String> state = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<string> state = new AtomicReference<string>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> states = new java.util.ArrayList<>();
			  IList<string> states = new List<string>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control phaseA = new org.neo4j.test.Barrier_Control();
			  Neo4Net.Test.Barrier_Control phaseA = new Neo4Net.Test.Barrier_Control();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control phaseB = new org.neo4j.test.Barrier_Control();
			  Neo4Net.Test.Barrier_Control phaseB = new Neo4Net.Test.Barrier_Control();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control phaseC = new org.neo4j.test.Barrier_Control();
			  Neo4Net.Test.Barrier_Control phaseC = new Neo4Net.Test.Barrier_Control();

			  state.set( "A" );
			  new Thread("awaitCompletion"() =>
			  {
				 try
				 {
					  states.Add( state.get() ); // expects A
					  phaseA.Reached();
					  states.Add( state.get() ); // expects B
					  phaseB.Await();
					  phaseB.Release();
					  coordinator.AwaitCompletion();
					  states.Add( state.get() ); // expects C
					  phaseC.Reached();
				 }
				 catch ( InterruptedException e )
				 {
					  Console.WriteLine( e.ToString() );
					  Console.Write( e.StackTrace );
				 }
			  });
			  .start();

			  // when
			  using ( TaskControl task1 = coordinator.NewInstance(), TaskControl task2 = coordinator.NewInstance() )
			  {
					phaseA.Await();
					state.set( "B" );
					phaseA.Release();
					phaseC.Release();
					phaseB.Reached();
					state.set( "C" );
			  }
			  phaseC.Await();

			  // then
			  assertEquals( Arrays.asList( "A", "B", "C" ), states );
		 }
	}

}