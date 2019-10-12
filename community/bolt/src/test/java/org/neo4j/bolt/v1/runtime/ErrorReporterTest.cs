using System;

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
namespace Org.Neo4j.Bolt.v1.runtime
{
	using Test = org.junit.Test;

	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ErrorReporterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyDatabaseErrorsAreLogged()
		 public virtual void OnlyDatabaseErrorsAreLogged()
		 {
			  AssertableLogProvider userLog = new AssertableLogProvider();
			  AssertableLogProvider internalLog = new AssertableLogProvider();
			  ErrorReporter reporter = NewErrorReporter( userLog, internalLog );

			  foreach ( Org.Neo4j.Kernel.Api.Exceptions.Status_Classification classification in Enum.GetValues( typeof( Org.Neo4j.Kernel.Api.Exceptions.Status_Classification ) ) )
			  {
					if ( classification != Org.Neo4j.Kernel.Api.Exceptions.Status_Classification.DatabaseError )
					{
						 Org.Neo4j.Kernel.Api.Exceptions.Status_Code code = NewStatusCode( classification );
						 Neo4jError error = Neo4jError.from( () => code, "Database error" );
						 reporter.Report( error );

						 userLog.AssertNoLoggingOccurred();
						 internalLog.AssertNoLoggingOccurred();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseErrorShouldLogFullMessageInDebugLogAndHelpfulPointerInUserLog()
		 public virtual void DatabaseErrorShouldLogFullMessageInDebugLogAndHelpfulPointerInUserLog()
		 {
			  // given
			  AssertableLogProvider userLog = new AssertableLogProvider();
			  AssertableLogProvider internalLog = new AssertableLogProvider();
			  ErrorReporter reporter = NewErrorReporter( userLog, internalLog );

			  Neo4jError error = Neo4jError.fatalFrom( new TestDatabaseError() );
			  System.Guid reference = error.Reference();

			  // when
			  reporter.Report( error );

			  // then
			  userLog.RawMessageMatcher().assertContains("Client triggered an unexpected error");
			  userLog.RawMessageMatcher().assertContains(reference.ToString());
			  userLog.RawMessageMatcher().assertContains("Database error");

			  internalLog.RawMessageMatcher().assertContains(reference.ToString());
			  internalLog.RawMessageMatcher().assertContains("Database error");
		 }

		 private static ErrorReporter NewErrorReporter( LogProvider userLog, LogProvider internalLog )
		 {
			  return new ErrorReporter( userLog.GetLog( "userLog" ), internalLog.GetLog( "internalLog" ) );
		 }

		 private static Org.Neo4j.Kernel.Api.Exceptions.Status_Code NewStatusCode( Org.Neo4j.Kernel.Api.Exceptions.Status_Classification classification )
		 {
			  Org.Neo4j.Kernel.Api.Exceptions.Status_Code code = mock( typeof( Org.Neo4j.Kernel.Api.Exceptions.Status_Code ) );
			  when( code.Classification() ).thenReturn(classification);
			  return code;
		 }

		 private class TestDatabaseError : Exception, Org.Neo4j.Kernel.Api.Exceptions.Status_HasStatus
		 {
			  internal TestDatabaseError() : base("Database error")
			  {
			  }

			  public override Status Status()
			  {
					return () => NewStatusCode(Org.Neo4j.Kernel.Api.Exceptions.Status_Classification.DatabaseError);
			  }
		 }
	}

}