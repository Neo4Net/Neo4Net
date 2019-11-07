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
namespace Neo4Net.Kernel.impl.recovery
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.logical_logs_location;

	public class RecoveryRequiredCheckerTest
	{
		private bool InstanceFieldsInitialized = false;

		public RecoveryRequiredCheckerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fileSystemRule.get() );
			RuleChain = RuleChain.outerRule( _pageCacheRule ).around( _fileSystemRule ).around( _testDirectory );
		}

		 private readonly EphemeralFileSystemRule _fileSystemRule = new EphemeralFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(pageCacheRule).around(fileSystemRule).around(testDirectory);
		 public RuleChain RuleChain;

		 private readonly Monitors _monitors = new Monitors();
		 private EphemeralFileSystemAbstraction _fileSystem;
		 private File _storeDir;
		 private DatabaseLayout _databaseLayout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _databaseLayout = _testDirectory.databaseLayout();
			  _storeDir = _databaseLayout.databaseDirectory();
			  _fileSystem = _fileSystemRule.get();
			  ( new TestGraphDatabaseFactory() ).setFileSystem(_fileSystem).newImpermanentDatabase(_storeDir).shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWantToRecoverIntactStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotWantToRecoverIntactStore()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  RecoveryRequiredChecker recoverer = GetRecoveryCheckerWithDefaultConfig( _fileSystem, pageCache );

			  assertThat( recoverer.IsRecoveryRequiredAt( _databaseLayout ), @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowIfIntactStore() throws RecoveryRequiredException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotThrowIfIntactStore()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  RecoveryRequiredChecker.AssertRecoveryIsNotRequired( _fileSystem, pageCache, Config.defaults(), _databaseLayout, new Monitors() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWantToRecoverBrokenStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWantToRecoverBrokenStore()
		 {
			  using ( FileSystemAbstraction fileSystemAbstraction = CreateAndCrashWithDefaultConfig() )
			  {

					PageCache pageCache = _pageCacheRule.getPageCache( fileSystemAbstraction );
					RecoveryRequiredChecker recoverer = GetRecoveryCheckerWithDefaultConfig( fileSystemAbstraction, pageCache );

					assertThat( recoverer.IsRecoveryRequiredAt( _databaseLayout ), @is( true ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = RecoveryRequiredException.class) public void shouldThrowIfBrokenStore() throws java.io.IOException, RecoveryRequiredException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfBrokenStore()
		 {
			  using ( FileSystemAbstraction fileSystemAbstraction = CreateAndCrashWithDefaultConfig() )
			  {
					PageCache pageCache = _pageCacheRule.getPageCache( fileSystemAbstraction );
					RecoveryRequiredChecker.AssertRecoveryIsNotRequired( fileSystemAbstraction, pageCache, Config.defaults(), _databaseLayout, new Monitors() );
					fail();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRecoverBrokenStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRecoverBrokenStore()
		 {
			  using ( FileSystemAbstraction fileSystemAbstraction = CreateAndCrashWithDefaultConfig() )
			  {
					PageCache pageCache = _pageCacheRule.getPageCache( fileSystemAbstraction );

					RecoveryRequiredChecker recoverer = GetRecoveryCheckerWithDefaultConfig( fileSystemAbstraction, pageCache );

					assertThat( recoverer.IsRecoveryRequiredAt( _databaseLayout ), @is( true ) );

					( new TestGraphDatabaseFactory() ).setFileSystem(fileSystemAbstraction).newImpermanentDatabase(_storeDir).shutdown();

					assertThat( recoverer.IsRecoveryRequiredAt( _databaseLayout ), @is( false ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRecoverBrokenStoreWithLogsInSeparateRelativeLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRecoverBrokenStoreWithLogsInSeparateRelativeLocation()
		 {
			  File customTransactionLogsLocation = new File( _storeDir, "tx-logs" );
			  Config config = Config.defaults( logical_logs_location, customTransactionLogsLocation.Name );
			  RecoverBrokenStoreWithConfig( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRecoverBrokenStoreWithLogsInSeparateAbsoluteLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRecoverBrokenStoreWithLogsInSeparateAbsoluteLocation()
		 {
			  File customTransactionLogsLocation = _testDirectory.directory( "tx-logs" );
			  Config config = Config.defaults( logical_logs_location, customTransactionLogsLocation.AbsolutePath );
			  RecoverBrokenStoreWithConfig( config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void recoverBrokenStoreWithConfig(Neo4Net.kernel.configuration.Config config) throws java.io.IOException
		 private void RecoverBrokenStoreWithConfig( Config config )
		 {
			  using ( FileSystemAbstraction fileSystemAbstraction = CreateSomeDataAndCrash( _storeDir, _fileSystem, config ) )
			  {
					PageCache pageCache = _pageCacheRule.getPageCache( fileSystemAbstraction );

					RecoveryRequiredChecker recoverer = GetRecoveryChecker( fileSystemAbstraction, pageCache, config );

					assertThat( recoverer.IsRecoveryRequiredAt( _databaseLayout ), @is( true ) );

					( new TestGraphDatabaseFactory() ).setFileSystem(fileSystemAbstraction).newEmbeddedDatabaseBuilder(_storeDir).setConfig(config.Raw).newGraphDatabase().shutdown();

					assertThat( recoverer.IsRecoveryRequiredAt( _databaseLayout ), @is( false ) );
			  }
		 }

		 private FileSystemAbstraction CreateAndCrashWithDefaultConfig()
		 {
			  return CreateSomeDataAndCrash( _storeDir, _fileSystem, Config.defaults() );
		 }

		 private RecoveryRequiredChecker GetRecoveryCheckerWithDefaultConfig( FileSystemAbstraction fileSystem, PageCache pageCache )
		 {
			  return GetRecoveryChecker( fileSystem, pageCache, Config.defaults() );
		 }

		 private RecoveryRequiredChecker GetRecoveryChecker( FileSystemAbstraction fileSystem, PageCache pageCache, Config config )
		 {
			  return new RecoveryRequiredChecker( fileSystem, pageCache, config, _monitors );
		 }

		 private static FileSystemAbstraction CreateSomeDataAndCrash( File store, EphemeralFileSystemAbstraction fileSystem, Config config )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.GraphDatabaseService db = new Neo4Net.test.TestGraphDatabaseFactory().setFileSystem(fileSystem).newImpermanentDatabaseBuilder(store).setConfig(config.getRaw()).newGraphDatabase();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fileSystem).newImpermanentDatabaseBuilder(store).setConfig(config.Raw).newGraphDatabase();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  EphemeralFileSystemAbstraction snapshot = fileSystem.Snapshot();
			  Db.shutdown();
			  return snapshot;
		 }
	}

}