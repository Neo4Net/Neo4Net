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
namespace Neo4Net.Kernel.impl.store
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.RecordFormatSelector.selectForStoreOrConfig;

	public class StoreFactoryTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreFactoryTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fsRule );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _testDirectory ).around( _pageCacheRule );
		}

		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(testDirectory).around(pageCacheRule);
		 public RuleChain RuleChain;

		 private NeoStores _neoStores;
		 private IdGeneratorFactory _idGeneratorFactory;
		 private PageCache _pageCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  FileSystemAbstraction fs = _fsRule.get();
			  _pageCache = _pageCacheRule.getPageCache( fs );
			  _idGeneratorFactory = new DefaultIdGeneratorFactory( fs );
		 }

		 private StoreFactory StoreFactory( Config config, params OpenOption[] openOptions )
		 {
			  LogProvider logProvider = NullLogProvider.Instance;
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  RecordFormats recordFormats = selectForStoreOrConfig( config, databaseLayout, _fsRule, _pageCache, logProvider );
			  return new StoreFactory( databaseLayout, config, _idGeneratorFactory, _pageCache, _fsRule.get(), recordFormats, logProvider, EmptyVersionContextSupplier.EMPTY, openOptions );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _neoStores != null )
			  {
					_neoStores.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveSameCreationTimeAndUpgradeTimeOnStartup()
		 public virtual void ShouldHaveSameCreationTimeAndUpgradeTimeOnStartup()
		 {
			  // When
			  _neoStores = StoreFactory( Config.defaults() ).openAllNeoStores(true);
			  MetaDataStore metaDataStore = _neoStores.MetaDataStore;

			  // Then
			  assertThat( metaDataStore.UpgradeTime, equalTo( metaDataStore.CreationTime ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveSameCommittedTransactionAndUpgradeTransactionOnStartup()
		 public virtual void ShouldHaveSameCommittedTransactionAndUpgradeTransactionOnStartup()
		 {
			  // When
			  _neoStores = StoreFactory( Config.defaults() ).openAllNeoStores(true);
			  MetaDataStore metaDataStore = _neoStores.MetaDataStore;

			  // Then
			  assertEquals( metaDataStore.UpgradeTransaction, metaDataStore.LastCommittedTransaction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveSpecificCountsTrackerForReadOnlyDatabase() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveSpecificCountsTrackerForReadOnlyDatabase()
		 {
			  // when
			  StoreFactory readOnlyStoreFactory = StoreFactory( Config.defaults( GraphDatabaseSettings.read_only, Settings.TRUE ) );
			  _neoStores = readOnlyStoreFactory.OpenAllNeoStores( true );
			  long lastClosedTransactionId = _neoStores.MetaDataStore.LastClosedTransactionId;

			  // then
			  assertEquals( -1, _neoStores.Counts.rotate( lastClosedTransactionId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = StoreNotFoundException.class) public void shouldThrowWhenOpeningNonExistingNeoStores()
		 public virtual void ShouldThrowWhenOpeningNonExistingNeoStores()
		 {
			  using ( NeoStores neoStores = StoreFactory( Config.defaults() ).openAllNeoStores() )
			  {
					neoStores.MetaDataStore;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateDeletionOptionToStores()
		 public virtual void ShouldDelegateDeletionOptionToStores()
		 {
			  // GIVEN
			  StoreFactory storeFactory = storeFactory( Config.defaults(), DELETE_ON_CLOSE );

			  // WHEN
			  _neoStores = storeFactory.OpenAllNeoStores( true );
			  assertTrue( _fsRule.get().listFiles(_testDirectory.databaseDir()).length >= StoreType.values().length );

			  // THEN
			  _neoStores.close();
			  assertEquals( 0, _fsRule.get().listFiles(_testDirectory.databaseDir()).length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleStoreConsistingOfOneEmptyFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleStoreConsistingOfOneEmptyFile()
		 {
			  StoreFactory storeFactory = storeFactory( Config.defaults() );
			  FileSystemAbstraction fs = _fsRule.get();
			  fs.Create( _testDirectory.databaseLayout().file("neostore.nodestore.db.labels") );
			  storeFactory.OpenAllNeoStores( true ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompleteInitializationOfStoresWithIncompleteHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompleteInitializationOfStoresWithIncompleteHeaders()
		 {
			  StoreFactory storeFactory = storeFactory( Config.defaults() );
			  storeFactory.OpenAllNeoStores( true ).close();
			  FileSystemAbstraction fs = _fsRule.get();
			  foreach ( File f in fs.ListFiles( _testDirectory.databaseDir() ) )
			  {
					fs.Truncate( f, 0 );
			  }
			  storeFactory.OpenAllNeoStores( true ).close();
		 }
	}

}