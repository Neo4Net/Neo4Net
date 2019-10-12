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
namespace Org.Neo4j.Bolt.runtime
{
	using Test = org.junit.Test;

	using DatabaseShutdownException = Org.Neo4j.Graphdb.DatabaseShutdownException;
	using DeadlockDetectedException = Org.Neo4j.Kernel.DeadlockDetectedException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class Neo4jErrorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAssignUnknownStatusToUnpredictedException()
		 public virtual void ShouldAssignUnknownStatusToUnpredictedException()
		 {
			  // Given
			  Exception cause = new Exception( "This is not an error we know how to handle." );
			  Neo4jError error = Neo4jError.From( cause );

			  // Then
			  assertThat( error.Status(), equalTo(Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertDeadlockException()
		 public virtual void ShouldConvertDeadlockException()
		 {
			  // When
			  Neo4jError error = Neo4jError.From( new DeadlockDetectedException( null ) );

			  // Then
			  assertEquals( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.DeadlockDetected, error.Status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetStatusToDatabaseUnavailableOnDatabaseShutdownException()
		 public virtual void ShouldSetStatusToDatabaseUnavailableOnDatabaseShutdownException()
		 {
			  // Given
			  DatabaseShutdownException ex = new DatabaseShutdownException();

			  // When
			  Neo4jError error = Neo4jError.From( ex );

			  // Then
			  assertThat( error.Status(), equalTo(Org.Neo4j.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable) );
			  assertThat( error.Cause(), equalTo(ex) );
		 }
	}

}