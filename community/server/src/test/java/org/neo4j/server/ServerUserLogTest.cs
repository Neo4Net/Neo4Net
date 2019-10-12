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
namespace Org.Neo4j.Server
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;
	using CommunityGraphFactory = Org.Neo4j.Server.database.CommunityGraphFactory;
	using GraphFactory = Org.Neo4j.Server.database.GraphFactory;
	using ServerModule = Org.Neo4j.Server.modules.ServerModule;
	using AdvertisableService = Org.Neo4j.Server.rest.management.AdvertisableService;
	using WebServer = Org.Neo4j.Server.web.WebServer;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.database_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_user_log_max_archives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_user_log_rotation_delay;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_user_log_rotation_threshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_user_log_to_stdout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.ServerBootstrapper.OK;

	public class ServerUserLogTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppress = org.neo4j.test.rule.SuppressOutput.suppress(org.neo4j.test.rule.SuppressOutput.System.out);
		 public readonly SuppressOutput Suppress = SuppressOutput.suppress( SuppressOutput.System.out );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory homeDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory HomeDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogToStdOutByDefault()
		 public virtual void ShouldLogToStdOutByDefault()
		 {
			  // given
			  ServerBootstrapper serverBootstrapper = ServerBootstrapper;
			  File dir = HomeDir.directory();
			  Log logBeforeStart = serverBootstrapper.Log;

			  // when
			  try
			  {
					int returnCode = Org.Neo4j.Server.ServerBootstrapper.Start( dir, null, stringMap( database_path.name(), HomeDir.absolutePath().AbsolutePath ) );

					// then no exceptions are thrown and
					assertEquals( OK, returnCode );
					assertTrue( serverBootstrapper.Server.Database.Running );
					assertThat( serverBootstrapper.Log, not( sameInstance( logBeforeStart ) ) );

					assertThat( StdOut, not( empty() ) );
					assertThat( StdOut, hasItem( containsString( "Started." ) ) );
			  }
			  finally
			  {
					// stop the server so that resources are released and test teardown isn't flaky
					serverBootstrapper.Stop();
			  }
			  assertFalse( Files.exists( GetUserLogFileLocation( dir ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogToFileWhenConfigured() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogToFileWhenConfigured()
		 {
			  // given
			  ServerBootstrapper serverBootstrapper = ServerBootstrapper;
			  File dir = HomeDir.directory();
			  Log logBeforeStart = serverBootstrapper.Log;

			  // when
			  try
			  {
					int returnCode = Org.Neo4j.Server.ServerBootstrapper.Start( dir, null, stringMap( database_path.name(), HomeDir.absolutePath().AbsolutePath, store_user_log_to_stdout.name(), "false" ) );
					// then no exceptions are thrown and
					assertEquals( OK, returnCode );
					assertTrue( serverBootstrapper.Server.Database.Running );
					assertThat( serverBootstrapper.Log, not( sameInstance( logBeforeStart ) ) );

			  }
			  finally
			  {
					// stop the server so that resources are released and test teardown isn't flaky
					serverBootstrapper.Stop();
			  }
			  assertThat( StdOut, empty() );
			  assertTrue( Files.exists( GetUserLogFileLocation( dir ) ) );
			  assertThat( ReadUserLogFile( dir ), not( empty() ) );
			  assertThat( ReadUserLogFile( dir ), hasItem( containsString( "Started." ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logShouldRotateWhenConfigured() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogShouldRotateWhenConfigured()
		 {
			  // given
			  ServerBootstrapper serverBootstrapper = ServerBootstrapper;
			  File dir = HomeDir.directory();
			  Log logBeforeStart = serverBootstrapper.Log;
			  int maxArchives = 4;

			  // when
			  try
			  {
					int returnCode = Org.Neo4j.Server.ServerBootstrapper.Start( dir, null, stringMap( database_path.name(), HomeDir.absolutePath().AbsolutePath, store_user_log_to_stdout.name(), "false", store_user_log_rotation_delay.name(), "0", store_user_log_rotation_threshold.name(), "16", store_user_log_max_archives.name(), Convert.ToString(maxArchives) ) );

					// then
					assertEquals( OK, returnCode );
					assertThat( serverBootstrapper.Log, not( sameInstance( logBeforeStart ) ) );
					assertTrue( serverBootstrapper.Server.Database.Running );

					// when we forcibly log some more stuff
					do
					{
						 serverBootstrapper.Log.info( "testing 123. This string should contain more than 16 bytes\n" );
						 Thread.Sleep( 2000 );
					} while ( AllUserLogFiles( dir ).Count <= 4 );
			  }
			  finally
			  {
					// stop the server so that resources are released and test teardown isn't flaky
					serverBootstrapper.Stop();
			  }

			  // then no exceptions are thrown and
			  assertThat( StdOut, empty() );
			  assertTrue( Files.exists( GetUserLogFileLocation( dir ) ) );
			  assertThat( ReadUserLogFile( dir ), not( empty() ) );
			  IList<string> userLogFiles = AllUserLogFiles( dir );
			  assertThat( userLogFiles, containsInAnyOrder( "neo4j.log", "neo4j.log.1", "neo4j.log.2", "neo4j.log.3", "neo4j.log.4" ) );
			  assertEquals( maxArchives + 1, userLogFiles.Count );
		 }

		 private IList<string> StdOut
		 {
			 get
			 {
				  IList<string> lines = Suppress.OutputVoice.lines();
				  // Remove empty lines
				  return lines.Where( line => !line.Equals( "" ) ).ToList();
			 }
		 }

		 private ServerBootstrapper ServerBootstrapper
		 {
			 get
			 {
				  return new ServerBootstrapperAnonymousInnerClass( this );
			 }
		 }

		 private class ServerBootstrapperAnonymousInnerClass : ServerBootstrapper
		 {
			 private readonly ServerUserLogTest _outerInstance;

			 public ServerBootstrapperAnonymousInnerClass( ServerUserLogTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override GraphFactory createGraphFactory( Config config )
			 {
				  return new CommunityGraphFactory();
			 }

			 protected internal override NeoServer createNeoServer( GraphFactory graphFactory, Config config, GraphDatabaseDependencies dependencies )
			 {
				  dependencies.UserLogProvider();
				  return new AbstractNeoServerAnonymousInnerClass( this, config, graphFactory, dependencies );
			 }

			 private class AbstractNeoServerAnonymousInnerClass : AbstractNeoServer
			 {
				 private readonly ServerBootstrapperAnonymousInnerClass _outerInstance;

				 public AbstractNeoServerAnonymousInnerClass( ServerBootstrapperAnonymousInnerClass outerInstance, Config config, GraphFactory graphFactory, GraphDatabaseDependencies dependencies ) : base( config, graphFactory, dependencies )
				 {
					 this.outerInstance = outerInstance;
				 }

				 protected internal override IEnumerable<ServerModule> createServerModules()
				 {
					  return new List<ServerModule>( 0 );
				 }

				 protected internal override void configureWebServer()
				 {
				 }

				 protected internal override void startWebServer()
				 {
				 }

				 protected internal override WebServer createWebServer()
				 {
					  return null;
				 }

				 public override IEnumerable<AdvertisableService> Services
				 {
					 get
					 {
						  return new List<AdvertisableService>( 0 );
					 }
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<String> readUserLogFile(java.io.File homeDir) throws java.io.IOException
		 private IList<string> ReadUserLogFile( File homeDir )
		 {
			  return Files.readAllLines( GetUserLogFileLocation( homeDir ) ).Where( line => !line.Equals( "" ) ).ToList();
		 }

		 private Path GetUserLogFileLocation( File homeDir )
		 {
			  return Paths.get( homeDir.AbsolutePath, "logs", "neo4j.log" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<String> allUserLogFiles(java.io.File homeDir) throws java.io.IOException
		 private IList<string> AllUserLogFiles( File homeDir )
		 {
			  using ( Stream<string> stream = Files.list( Paths.get( homeDir.AbsolutePath, "logs" ) ).map( x => x.FileName.ToString() ).filter(x => x.contains("neo4j.log")) )
			  {
					return stream.collect( Collectors.toList() );
			  }
		 }
	}

}