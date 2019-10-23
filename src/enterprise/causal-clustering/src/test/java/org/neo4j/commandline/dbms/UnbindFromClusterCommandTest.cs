/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Dbms.CommandLine
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ClusterStateDirectory = Neo4Net.causalclustering.core.state.ClusterStateDirectory;
	using ClusterStateException = Neo4Net.causalclustering.core.state.ClusterStateException;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using CommandLocator = Neo4Net.CommandLine.Admin.CommandLocator;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Usage = Neo4Net.CommandLine.Admin.Usage;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IOUtils = Neo4Net.Io.IOUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class UnbindFromClusterCommandTest
	{
		private bool InstanceFieldsInitialized = false;

		public UnbindFromClusterCommandTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _testDir );
		}

		 private readonly TestDirectory _testDir = TestDirectory.testDirectory();
		 private readonly EphemeralFileSystemRule _fileSystemRule = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDir);
		 public RuleChain RuleChain;
		 private Path _homeDir;
		 private Path _confDir;

		 private FileSystemAbstraction _fs = new DefaultFileSystemAbstraction();
		 private OutsideWorld _outsideWorld = mock( typeof( OutsideWorld ) );
		 private FileChannel _channel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _homeDir = _testDir.directory( "home" ).toPath();
			  _confDir = _testDir.directory( "conf" ).toPath();
			  _fs.mkdir( _homeDir.toFile() );

			  when( _outsideWorld.fileSystem() ).thenReturn(_fs);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  IOUtils.closeAll( _channel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createClusterStateDir(org.Neo4Net.io.fs.FileSystemAbstraction fs) throws org.Neo4Net.causalclustering.core.state.ClusterStateException
		 private File CreateClusterStateDir( FileSystemAbstraction fs )
		 {
			  File dataDir = new File( _homeDir.toFile(), "data" );
			  ClusterStateDirectory clusterStateDirectory = new ClusterStateDirectory( dataDir, false );
			  clusterStateDirectory.Initialize( fs );
			  return clusterStateDirectory.Get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreIfSpecifiedDatabaseDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreIfSpecifiedDatabaseDoesNotExist()
		 {
			  // given
			  File clusterStateDir = CreateClusterStateDir( _fs );
			  UnbindFromClusterCommand command = new UnbindFromClusterCommand( _homeDir, _confDir, _outsideWorld );

			  // when
			  command.Execute( DatabaseNameParameter( "doesnotexist.db" ) );

			  // then
			  assertFalse( _fs.fileExists( clusterStateDir ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToUnbindLiveDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToUnbindLiveDatabase()
		 {
			  // given
			  CreateClusterStateDir( _fs );
			  UnbindFromClusterCommand command = new UnbindFromClusterCommand( _homeDir, _confDir, _outsideWorld );

			  FileLock fileLock = CreateLockedFakeDbDir( _homeDir );
			  try
			  {
					// when
					command.Execute( DatabaseNameParameter( GraphDatabaseSettings.DEFAULT_DATABASE_NAME ) );
					fail();
			  }
			  catch ( CommandFailed e )
			  {
					// then
					assertThat( e.Message, containsString( "Database is currently locked. Please shutdown Neo4Net." ) );
			  }
			  finally
			  {
					fileLock.release();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveClusterStateDirectoryForGivenDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveClusterStateDirectoryForGivenDatabase()
		 {
			  // given
			  File clusterStateDir = CreateClusterStateDir( _fs );
			  CreateUnlockedFakeDbDir( _homeDir );
			  UnbindFromClusterCommand command = new UnbindFromClusterCommand( _homeDir, _confDir, _outsideWorld );

			  // when
			  command.Execute( DatabaseNameParameter( "graph.db" ) );

			  // then
			  assertFalse( _fs.fileExists( clusterStateDir ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportWhenClusterStateDirectoryIsNotPresent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportWhenClusterStateDirectoryIsNotPresent()
		 {
			  // given
			  CreateUnlockedFakeDbDir( _homeDir );
			  UnbindFromClusterCommand command = new UnbindFromClusterCommand( _homeDir, _confDir, _outsideWorld );
			  command.Execute( DatabaseNameParameter( GraphDatabaseSettings.DEFAULT_DATABASE_NAME ) );
			  verify( _outsideWorld ).stdErrLine( "This instance was not bound. No work performed." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintUsage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintUsage()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "Neo4Net-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new UnbindFromClusterCommandProvider(), ps.println );

					assertThat( baos.ToString(), containsString("usage") );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createUnlockedFakeDbDir(java.nio.file.Path homeDir) throws java.io.IOException
		 private void CreateUnlockedFakeDbDir( Path homeDir )
		 {
			  Path fakeDbDir = CreateFakeDbDir( homeDir );
			  Files.createFile( DatabaseLayout.of( fakeDbDir.toFile() ).StoreLayout.storeLockFile().toPath() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.channels.FileLock createLockedFakeDbDir(java.nio.file.Path homeDir) throws java.io.IOException
		 private FileLock CreateLockedFakeDbDir( Path homeDir )
		 {
			  return CreateLockedStoreLockFileIn( CreateFakeDbDir( homeDir ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.file.Path createFakeDbDir(java.nio.file.Path homeDir) throws java.io.IOException
		 private Path CreateFakeDbDir( Path homeDir )
		 {
			  Path graphDb = homeDir.resolve( "data/databases/graph.db" );
			  _fs.mkdirs( graphDb.toFile() );
			  _fs.create( graphDb.resolve( "neostore" ).toFile() ).close();
			  return graphDb;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.channels.FileLock createLockedStoreLockFileIn(java.nio.file.Path databaseDir) throws java.io.IOException
		 private FileLock CreateLockedStoreLockFileIn( Path databaseDir )
		 {
			  Path storeLockFile = Files.createFile( DatabaseLayout.of( databaseDir.toFile() ).StoreLayout.storeLockFile().toPath() );
			  _channel = FileChannel.open( storeLockFile, READ, WRITE );
			  return _channel.@lock( 0, long.MaxValue, true );
		 }

		 private static string[] DatabaseNameParameter( string databaseName )
		 {
			  return new string[]{ "--database=" + databaseName };
		 }
	}

}