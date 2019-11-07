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
namespace Neo4Net.backup
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using BackupProtocolService = Neo4Net.backup.impl.BackupProtocolService;
	using ConsistencyCheck = Neo4Net.backup.impl.ConsistencyCheck;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Neo4Net.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_4 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_4;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.backup.impl.BackupProtocolServiceFactory.backupProtocolService;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;

	public class BackupToolIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException Expected = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.EmbeddedDatabaseRule dbRule = new Neo4Net.test.rule.EmbeddedDatabaseRule().startLazily();
		 public readonly EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule().startLazily();

		 private DefaultFileSystemAbstraction _fs;
		 private PageCache _pageCache;
		 private Path _backupDir;
		 private BackupTool _backupTool;
		 private DatabaseLayout _backupLayout;
		 private IJobScheduler _jobScheduler;
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
			  IGraphDatabaseService db = StartGraphDatabase( backupPort );
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

		 private IGraphDatabaseService StartGraphDatabase( int backupPort )
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