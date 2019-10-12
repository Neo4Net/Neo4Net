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
namespace Org.Neo4j.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class GraphDatabaseInternalLogIT
	{
		 private const string INTERNAL_LOG_FILE = "debug.log";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteToInternalDiagnosticsLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteToInternalDiagnosticsLog()
		 {
			  // Given
			  ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDir.databaseDir()).setConfig(GraphDatabaseSettings.logs_directory, TestDir.directory("logs").AbsolutePath).newGraphDatabase().shutdown();
			  File internalLog = new File( TestDir.directory( "logs" ), INTERNAL_LOG_FILE );

			  // Then
			  assertThat( internalLog.File, @is( true ) );
			  assertThat( internalLog.length(), greaterThan(0L) );

			  assertEquals( 1, CountOccurrences( internalLog, "Database graph.db is ready." ) );
			  assertEquals( 2, CountOccurrences( internalLog, "Database graph.db is unavailable." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWriteDebugToInternalDiagnosticsLogByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotWriteDebugToInternalDiagnosticsLogByDefault()
		 {
			  // Given
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDir.storeDir()).setConfig(GraphDatabaseSettings.logs_directory, TestDir.directory("logs").AbsolutePath).newGraphDatabase();

			  // When
			  LogService logService = ( ( GraphDatabaseAPI ) db ).DependencyResolver.resolveDependency( typeof( LogService ) );
			  logService.GetInternalLog( this.GetType() ).debug("A debug entry");

			  Db.shutdown();
			  File internalLog = new File( TestDir.directory( "logs" ), INTERNAL_LOG_FILE );

			  // Then
			  assertThat( internalLog.File, @is( true ) );
			  assertThat( internalLog.length(), greaterThan(0L) );

			  assertEquals( 0, CountOccurrences( internalLog, "A debug entry" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteDebugToInternalDiagnosticsLogForEnabledContexts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteDebugToInternalDiagnosticsLogForEnabledContexts()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDir.storeDir()).setConfig(GraphDatabaseSettings.store_internal_debug_contexts, this.GetType().FullName + ",java.io").setConfig(GraphDatabaseSettings.logs_directory, TestDir.directory("logs").AbsolutePath).newGraphDatabase();

			  // When
			  LogService logService = ( ( GraphDatabaseAPI ) db ).DependencyResolver.resolveDependency( typeof( LogService ) );
			  logService.GetInternalLog( this.GetType() ).debug("A debug entry");
			  logService.GetInternalLog( typeof( GraphDatabaseService ) ).debug( "A GDS debug entry" );
			  logService.GetInternalLog( typeof( StringWriter ) ).debug( "A SW debug entry" );

			  Db.shutdown();
			  File internalLog = new File( TestDir.directory( "logs" ), INTERNAL_LOG_FILE );

			  // Then
			  assertThat( internalLog.File, @is( true ) );
			  assertThat( internalLog.length(), greaterThan(0L) );

			  assertEquals( 1, CountOccurrences( internalLog, "A debug entry" ) );
			  assertEquals( 0, CountOccurrences( internalLog, "A GDS debug entry" ) );
			  assertEquals( 1, CountOccurrences( internalLog, "A SW debug entry" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long countOccurrences(java.io.File file, String substring) throws java.io.IOException
		 private static long CountOccurrences( File file, string substring )
		 {
			  using ( Stream<string> lines = Files.lines( file.toPath() ) )
			  {
					return lines.filter( line => line.contains( substring ) ).count();
			  }
		 }
	}

}