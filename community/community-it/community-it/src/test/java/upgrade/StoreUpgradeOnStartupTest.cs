using System;
using System.Collections.Generic;

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
namespace Upgrade
{
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using ConsistencyCheckIncompleteException = Org.Neo4j.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using StandardV2_3 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV2_3;
	using StoreUpgrader = Org.Neo4j.Kernel.impl.storemigration.StoreUpgrader;
	using StoreVersionCheck = Org.Neo4j.Kernel.impl.storemigration.StoreVersionCheck;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.store.StoreAssertions.assertConsistentStore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.MigrationTestUtils.checkNeoStoreHasDefaultFormatVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.MigrationTestUtils.prepareSampleLegacyDatabase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.MigrationTestUtils.removeCheckPointFromTxLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class StoreUpgradeOnStartupTest
	public class StoreUpgradeOnStartupTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreUpgradeOnStartupTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDir ).around( _fileSystemRule ).around( _pageCacheRule );
		}

		 private readonly TestDirectory _testDir = TestDirectory.testDirectory();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDir).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String version;
		 public string Version;

		 private FileSystemAbstraction _fileSystem;
		 private DatabaseLayout _workingDatabaseLayout;
		 private StoreVersionCheck _check;
		 private File _workingStoreDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<String> versions()
		 public static ICollection<string> Versions()
		 {
			  return Collections.singletonList( StandardV2_3.STORE_VERSION );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _fileSystem = _fileSystemRule.get();
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  _workingStoreDir = _testDir.storeDir( "working_" + Version );
			  _workingDatabaseLayout = _testDir.databaseLayout( _workingStoreDir );
			  _check = new StoreVersionCheck( pageCache );
			  File prepareDirectory = _testDir.directory( "prepare_" + Version );
			  prepareSampleLegacyDatabase( Version, _fileSystem, _workingDatabaseLayout.databaseDirectory(), prepareDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpgradeAutomaticallyOnDatabaseStartup() throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpgradeAutomaticallyOnDatabaseStartup()
		 {
			  // when
			  GraphDatabaseService database = CreateGraphDatabaseService();
			  database.Shutdown();

			  // then
			  assertTrue( "Some store files did not have the correct version", checkNeoStoreHasDefaultFormatVersion( _check, _workingDatabaseLayout ) );
			  assertConsistentStore( _workingDatabaseLayout );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbortOnNonCleanlyShutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAbortOnNonCleanlyShutdown()
		 {
			  // given
			  removeCheckPointFromTxLog( _fileSystem, _workingDatabaseLayout.databaseDirectory() );
			  try
			  {
					// when
					GraphDatabaseService database = CreateGraphDatabaseService();
					database.Shutdown(); // shutdown db in case test fails
					fail( "Should have been unable to start upgrade on old version" );
			  }
			  catch ( Exception e )
			  {
					// then
					assertThat( Exceptions.rootCause( e ), Matchers.instanceOf( typeof( StoreUpgrader.UnableToUpgradeException ) ) );
			  }
		 }

		 private GraphDatabaseService CreateGraphDatabaseService()
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_workingDatabaseLayout.databaseDirectory()).setConfig(GraphDatabaseSettings.allow_upgrade, "true").newGraphDatabase();
		 }
	}

}