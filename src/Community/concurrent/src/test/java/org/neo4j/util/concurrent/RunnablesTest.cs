using System;
using System.Collections.Generic;
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
namespace Neo4Net.Util.concurrent
{
	using Test = org.junit.jupiter.api.Test;


	using Exceptions = Neo4Net.Helpers.Exceptions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class RunnablesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void runAllMustRunAll()
		 internal virtual void RunAllMustRunAll()
		 {
			  // given
			  Task task1 = new Task( this );
			  Task task2 = new Task( this );
			  Task task3 = new Task( this );

			  // when
			  Runnables.RunAll( "", task1, task2, task3 );

			  // then
			  AssertRun( task1, task2, task3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void runAllMustRunAllAndPropagateError()
		 internal virtual void RunAllMustRunAllAndPropagateError()
		 {
			  // given
			  Task task1 = new Task( this );
			  Task task2 = new Task( this );
			  Task task3 = new Task( this );
			  Exception expectedError = new Exception( "Killroy was here" );
			  ThreadStart throwingTask = Error( expectedError );

			  IList<ThreadStart> runnables = Arrays.asList( task1, task2, task3, throwingTask );
			  Collections.shuffle( runnables );

			  // when
			  string failureMessage = "Something wrong, Killroy must be here somewhere.";
			  Exception actual = assertThrows( typeof( Exception ), () => Runnables.runAll(failureMessage, runnables.ToArray()) );

			  // then
			  AssertRun( task1, task2, task3 );
			  assertTrue( Exceptions.findCauseOrSuppressed( actual, t => t == expectedError ).Present );
			  assertEquals( failureMessage, actual.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void runAllMustRunAllAndPropagateMultipleErrors()
		 internal virtual void RunAllMustRunAllAndPropagateMultipleErrors()
		 {
			  // given
			  Task task1 = new Task( this );
			  Task task2 = new Task( this );
			  Task task3 = new Task( this );
			  Exception expectedError = new Exception( "Killroy was here" );
			  ThreadStart throwingTask1 = Error( expectedError );
			  Exception expectedException = new Exception( "and here" );
			  ThreadStart throwingTask2 = RuntimeException( expectedException );

			  IList<ThreadStart> runnables = Arrays.asList( task1, task2, task3, throwingTask1, throwingTask2 );
			  Collections.shuffle( runnables );

			  // when
			  string failureMessage = "Something wrong, Killroy must be here somewhere.";
			  Exception actual = assertThrows( typeof( Exception ), () => Runnables.runAll(failureMessage, runnables.ToArray()) );

			  // then
			  AssertRun( task1, task2, task3 );
			  assertTrue( Exceptions.findCauseOrSuppressed( actual, t => t == expectedError ).Present );
			  assertTrue( Exceptions.findCauseOrSuppressed( actual, t => t == expectedException ).Present );
			  assertEquals( failureMessage, actual.Message );
		 }

		 private ThreadStart Error( Exception error )
		 {
			  return () =>
			  {
				throw error;
			  };
		 }

		 private ThreadStart RuntimeException( Exception runtimeException )
		 {
			  return () =>
			  {
				throw runtimeException;
			  };
		 }

		 private void AssertRun( params Task[] tasks )
		 {
			  foreach ( Task task in tasks )
			  {
					assertTrue( task.RunConflict, "didn't run all expected tasks" );
			  }
		 }

		 private class Task : ThreadStart
		 {
			 private readonly RunnablesTest _outerInstance;

			 public Task( RunnablesTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool RunConflict;

			  public override void Run()
			  {
					RunConflict = true;
			  }
		 }
	}

}