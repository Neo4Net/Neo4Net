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
namespace Neo4Net.Kernel.configuration
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using LogTimeZone = Neo4Net.Logging.LogTimeZone;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class SystemTimeZoneLoggingIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseLogsUseSystemTimeZoneIfConfigure() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabaseLogsUseSystemTimeZoneIfConfigure()
		 {
			  TimeZone defaultTimeZone = TimeZone.Default;
			  try
			  {
					CheckStartLogLine( 5, "+0500" );
					CheckStartLogLine( -7, "-0700" );
			  }
			  finally
			  {
					TimeZone.Default = defaultTimeZone;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkStartLogLine(int hoursShift, String timeZoneSuffix) throws java.io.IOException
		 private void CheckStartLogLine( int hoursShift, string timeZoneSuffix )
		 {
			  TimeZone.Default = TimeZone.getTimeZone( ZoneOffset.ofHours( hoursShift ) );
			  File storeDir = TestDirectory.storeDir( hoursShift.ToString() );
			  File databaseDirectory = TestDirectory.databaseLayout( storeDir ).databaseDirectory();
			  IGraphDatabaseService database = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(databaseDirectory).setConfig(GraphDatabaseSettings.db_timezone, LogTimeZone.SYSTEM.name()).newGraphDatabase();
			  database.Shutdown();
			  Path databasePath = storeDir.toPath();
			  Path debugLog = Paths.get( "logs", "debug.log" );
			  string debugLogLine = GetLogLine( databasePath, debugLog );
			  assertTrue( debugLogLine, debugLogLine.Contains( timeZoneSuffix ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String getLogLine(java.nio.file.Path databasePath, java.nio.file.Path logFilePath) throws java.io.IOException
		 private static string GetLogLine( Path databasePath, Path logFilePath )
		 {
			  return Files.readAllLines( databasePath.resolve( logFilePath ) ).get( 0 );
		 }
	}

}