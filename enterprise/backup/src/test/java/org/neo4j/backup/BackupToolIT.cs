/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.backup
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using BackupProtocolService = Org.Neo4j.backup.impl.BackupProtocolService;
	using ConsistencyCheck = Org.Neo4j.backup.impl.ConsistencyCheck;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Org.Neo4j.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using StandardV2_3 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_4 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_4;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupProtocolServiceFactory.backupProtocolService;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;

	public class BackupToolIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException Expected = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.EmbeddedDatabaseRule dbRule = new org.neo4j.test.rule.EmbeddedDatabaseRule().startLazily();
		 public readonly EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule().startLazily();

		 private DefaultFileSystemAbstraction _fs;
		 private PageCache _pageCache;
		 private Path _backupDir;
		 private BackupTool _backupTool;
		 private DatabaseLayout _backupLayout;
		 private JobScheduler _jobScheduler;
		 private BackupProtocolService _backupProtocolService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _backupLayout = TestDirectory.databaseLayout( "backups" );
			  _backupDir = _backupLayout.databaseDirectory().toPath();
			  _fs = new DefaultFileSystemAbstraction();
			  _jobScheduler = new ThreadPoolJobScheduler();
			  _pageCache = StandalonePageCacheFactory.createPageCache( _fs, _jobScheduler );
			  _backupProtocolService = _backupProtocolService();
			  _backupTool = new BackupTool( _backupProtocolService, mock( typeof( PrintStream ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _backupProtocolService.close();
			  _pageCache.close();
			  _jobScheduler.close();
			  _fs.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oldIncompatibleBackupsThrows() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OldIncompatibleBackupsThrows()
		 {
			  // Prepare an "old" backup
			  PrepareNeoStoreFile( StandardV2_3.STORE_VERSION );

			  // Start database to backup
			  int backupPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = StartGraphDatabase( backupPort );
			  try
			  {
					Expected.expect( typeof( BackupTool.ToolFailureException ) );
					Expected.expectMessage( "Failed to perform backup because existing backup is from a different version." );

					// Perform backup
					_backupTool.executeBackup( new HostnamePort( "localhost", backupPort ), _backupDir, ConsistencyCheck.NONE, Config.defaults( GraphDatabaseSettings.record_format, StandardV3_4.NAME ), 20L * 60L * 1000L, false );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private GraphDatabaseService StartGraphDatabase( int backupPort )
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.directory()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.TRUE).setConfig(OnlineBackupSettings.online_backup_server, "127.0.0.1:" + backupPort).setConfig(GraphDatabaseSettings.keep_logical_logs, Settings.TRUE).setConfig(GraphDatabaseSettings.record_format, StandardV2_3.NAME).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareNeoStoreFile(String storeVersion) throws Exception
		 private void PrepareNeoStoreFile( string storeVersion )
		 {
			  File neoStoreFile = CreateNeoStoreFile();
			  long value = MetaDataStore.versionStringToLong( storeVersion );
			  MetaDataStore.setRecord( _pageCache, neoStoreFile, STORE_VERSION, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createNeoStoreFile() throws Exception
		 private File CreateNeoStoreFile()
		 {
			  File neoStoreFile = _backupLayout.metadataStore();
			  _fs.create( neoStoreFile ).close();
			  return neoStoreFile;
		 }
	}

}