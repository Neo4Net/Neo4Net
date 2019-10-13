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
namespace Neo4Net.CommandLine.dbms
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using RealOutsideWorld = Neo4Net.CommandLine.Admin.RealOutsideWorld;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class DiagnosticsReportCommandIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private GraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _database = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.storeDir()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _database.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAttachToPidAndRunThreadDump() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToAttachToPidAndRunThreadDump()
		 {
			  long pid = PID;
			  assertThat( pid, @is( not( 0 ) ) );

			  // Write config file
			  Files.createFile( TestDirectory.file( "neo4j.conf" ).toPath() );

			  // write neo4j.pid file
			  File run = TestDirectory.directory( "run" );
			  Files.write( Paths.get( run.AbsolutePath, "neo4j.pid" ), pid.ToString().GetBytes() );

			  // Run command, should detect running instance
			  try
			  {
					  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld() )
					  {
						string[] args = new string[] { "threads", "--to=" + TestDirectory.absolutePath().AbsolutePath + "/reports" };
						Path homeDir = TestDirectory.directory().toPath();
						DiagnosticsReportCommand diagnosticsReportCommand = new DiagnosticsReportCommand( homeDir, homeDir, outsideWorld );
						diagnosticsReportCommand.Execute( args );
					  }
			  }
			  catch ( IncorrectUsage e )
			  {
					if ( e.Message.Equals( "Unknown classifier: threads" ) )
					{
						 return; // If we get attach API is not available for example in some IBM jdk installs, ignore this test
					}
					throw e;
			  }

			  // Verify that we took a thread dump
			  File reports = TestDirectory.directory( "reports" );
			  File[] files = reports.listFiles();
			  assertThat( files, notNullValue() );
			  assertThat( Files.Length, @is( 1 ) );

			  Path report = files[0].toPath();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.URI uri = java.net.URI.create("jar:file:" + report.toUri().getRawPath());
			  URI uri = URI.create( "jar:file:" + report.toUri().RawPath );

			  using ( FileSystem fs = FileSystems.newFileSystem( uri, Collections.emptyMap() ) )
			  {
					string threadDump = new string( Files.readAllBytes( fs.getPath( "threaddump.txt" ) ) );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					assertThat( threadDump, containsString( typeof( DiagnosticsReportCommandIT ).FullName ) );
			  }
		 }

		 private static long PID
		 {
			 get
			 {
				  string processName = java.lang.management.ManagementFactory.RuntimeMXBean.Name;
				  if ( !string.ReferenceEquals( processName, null ) && processName.Length > 0 )
				  {
						try
						{
							 return long.Parse( processName.Split( "@", true )[0] );
						}
						catch ( Exception )
						{
						}
				  }
   
				  return 0;
			 }
		 }
	}

}