using System;

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
namespace Neo4Net.Kernel.@internal
{
	using Test = org.junit.Test;

	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.@event.ErrorState.TX_MANAGER_NOT_OK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class DatabaseHealthTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateDatabasePanicEvents()
		 public virtual void ShouldGenerateDatabasePanicEvents()
		 {
			  // GIVEN
			  DatabasePanicEventGenerator generator = mock( typeof( DatabasePanicEventGenerator ) );
			  DatabaseHealth databaseHealth = new DatabaseHealth( generator, NullLogProvider.Instance.getLog( typeof( DatabaseHealth ) ) );
			  databaseHealth.Healed();

			  // WHEN
			  Exception cause = new Exception( "My own fault" );
			  databaseHealth.Panic( cause );
			  databaseHealth.Panic( cause );

			  // THEN
			  verify( generator, times( 1 ) ).generateEvent( TX_MANAGER_NOT_OK, cause );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogDatabasePanicEvent()
		 public virtual void ShouldLogDatabasePanicEvent()
		 {
			  // GIVEN
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  DatabaseHealth databaseHealth = new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), logProvider.GetLog( typeof( DatabaseHealth ) ) );
			  databaseHealth.Healed();

			  // WHEN
			  string message = "Listen everybody... panic!";
			  Exception exception = new Exception( message );
			  databaseHealth.Panic( exception );

			  // THEN
			  logProvider.AssertAtLeastOnce( inLog( typeof( DatabaseHealth ) ).error( @is( "Database panic: The database has encountered a critical error, " + "and needs to be restarted. Please see database logs for more details." ), sameInstance( exception ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void healDatabaseWithoutCriticalErrors()
		 public virtual void HealDatabaseWithoutCriticalErrors()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  DatabaseHealth databaseHealth = new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), logProvider.GetLog( typeof( DatabaseHealth ) ) );

			  assertTrue( databaseHealth.Healthy );

			  databaseHealth.Panic( new IOException( "Space exception." ) );

			  assertFalse( databaseHealth.Healthy );
			  assertTrue( databaseHealth.Healed() );
			  logProvider.RawMessageMatcher().assertContains("Database health set to OK");
			  logProvider.RawMessageMatcher().assertNotContains("Database encountered a critical error and can't be healed. Restart required.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseWithCriticalErrorsCanNotBeHealed()
		 public virtual void DatabaseWithCriticalErrorsCanNotBeHealed()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  DatabaseHealth databaseHealth = new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), logProvider.GetLog( typeof( DatabaseHealth ) ) );

			  assertTrue( databaseHealth.Healthy );

			  IOException criticalException = new IOException( "Space exception.", new System.OutOfMemoryException( "Out of memory." ) );
			  databaseHealth.Panic( criticalException );

			  assertFalse( databaseHealth.Healthy );
			  assertFalse( databaseHealth.Healed() );
			  logProvider.RawMessageMatcher().assertNotContains("Database health set to OK");
			  logProvider.RawMessageMatcher().assertContains("Database encountered a critical error and can't be healed. Restart required.");
		 }
	}

}