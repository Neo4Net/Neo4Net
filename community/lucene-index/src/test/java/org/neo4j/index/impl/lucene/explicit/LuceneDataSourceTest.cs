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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using Node = Org.Neo4j.Graphdb.Node;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexManager = Org.Neo4j.Graphdb.index.IndexManager;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class LuceneDataSourceTest
	{
		private bool InstanceFieldsInitialized = false;

		public LuceneDataSourceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _life ).around( _expectedException );
		}

		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly ExpectedException _expectedException = ExpectedException.none();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(life).around(expectedException);
		 public RuleChain RuleChain;

		 private IndexConfigStore _indexStore;
		 private LuceneDataSource _dataSource;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _indexStore = new IndexConfigStore( _directory.databaseLayout(), _fileSystemRule.get() );
			  AddIndex( "foo" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotTryToCommitWritersOnForceInReadOnlyMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotTryToCommitWritersOnForceInReadOnlyMode()
		 {
			  IndexIdentifier indexIdentifier = Identifier( "foo" );
			  PrepareIndexesByIdentifiers( indexIdentifier );
			  StopDataSource();

			  Config readOnlyConfig = Config.defaults( readOnlyConfig() );
			  LuceneDataSource readOnlyDataSource = _life.add( GetLuceneDataSource( readOnlyConfig ) );
			  assertNotNull( readOnlyDataSource.GetIndexSearcher( indexIdentifier ) );

			  readOnlyDataSource.Force();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notAllowIndexDeletionInReadOnlyMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NotAllowIndexDeletionInReadOnlyMode()
		 {
			  IndexIdentifier indexIdentifier = Identifier( "foo" );
			  PrepareIndexesByIdentifiers( indexIdentifier );
			  StopDataSource();

			  Config readOnlyConfig = Config.defaults( readOnlyConfig() );
			  _dataSource = _life.add( GetLuceneDataSource( readOnlyConfig, OperationalMode.single ) );
			  _expectedException.expect( typeof( System.InvalidOperationException ) );
			  _expectedException.expectMessage( "Index deletion in read only mode is not supported." );
			  _dataSource.deleteIndex( indexIdentifier, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useReadOnlyIndexSearcherInReadOnlyModeForSingleInstance() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseReadOnlyIndexSearcherInReadOnlyModeForSingleInstance()
		 {
			  IndexIdentifier indexIdentifier = Identifier( "foo" );
			  PrepareIndexesByIdentifiers( indexIdentifier );
			  StopDataSource();

			  Config readOnlyConfig = Config.defaults( readOnlyConfig() );
			  _dataSource = _life.add( GetLuceneDataSource( readOnlyConfig, OperationalMode.single ) );

			  IndexReference indexSearcher = _dataSource.getIndexSearcher( indexIdentifier );
			  assertTrue( "Read only index reference should be used in read only mode.", typeof( ReadOnlyIndexReference ).IsInstanceOfType( indexSearcher ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useWritableIndexSearcherInReadOnlyModeForNonSingleInstance() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseWritableIndexSearcherInReadOnlyModeForNonSingleInstance()
		 {
			  IndexIdentifier indexIdentifier = Identifier( "foo" );
			  PrepareIndexesByIdentifiers( indexIdentifier );
			  StopDataSource();

			  Config readOnlyConfig = Config.defaults( readOnlyConfig() );
			  _dataSource = _life.add( GetLuceneDataSource( readOnlyConfig, OperationalMode.ha ) );

			  IndexReference indexSearcher = _dataSource.getIndexSearcher( indexIdentifier );
			  assertTrue( "Writable index reference should be used in read only mode in ha mode.", typeof( WritableIndexReference ).IsInstanceOfType( indexSearcher ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void refreshReadOnlyIndexSearcherInReadOnlyMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RefreshReadOnlyIndexSearcherInReadOnlyMode()
		 {
			  IndexIdentifier indexIdentifier = Identifier( "foo" );
			  PrepareIndexesByIdentifiers( indexIdentifier );
			  StopDataSource();

			  Config readOnlyConfig = Config.defaults( readOnlyConfig() );
			  _dataSource = _life.add( GetLuceneDataSource( readOnlyConfig ) );

			  IndexReference indexSearcher = _dataSource.getIndexSearcher( indexIdentifier );
			  IndexReference indexSearcher2 = _dataSource.getIndexSearcher( indexIdentifier );
			  IndexReference indexSearcher3 = _dataSource.getIndexSearcher( indexIdentifier );
			  IndexReference indexSearcher4 = _dataSource.getIndexSearcher( indexIdentifier );
			  assertSame( "Refreshed read only searcher should be the same.", indexSearcher, indexSearcher2 );
			  assertSame( "Refreshed read only searcher should be the same.", indexSearcher2, indexSearcher3 );
			  assertSame( "Refreshed read only searcher should be the same.", indexSearcher3, indexSearcher4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testShouldReturnIndexWriterFromLRUCache() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestShouldReturnIndexWriterFromLRUCache()
		 {
			  Config config = Config.defaults();
			  _dataSource = _life.add( GetLuceneDataSource( config ) );
			  IndexIdentifier identifier = identifier( "foo" );
			  IndexWriter writer = _dataSource.getIndexSearcher( identifier ).Writer;
			  assertSame( writer, _dataSource.getIndexSearcher( identifier ).Writer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testShouldReturnIndexSearcherFromLRUCache() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestShouldReturnIndexSearcherFromLRUCache()
		 {
			  Config config = Config.defaults();
			  _dataSource = _life.add( GetLuceneDataSource( config ) );
			  IndexIdentifier identifier = identifier( "foo" );
			  IndexReference searcher = _dataSource.getIndexSearcher( identifier );
			  assertSame( searcher, _dataSource.getIndexSearcher( identifier ) );
			  searcher.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClosesOldestIndexWriterWhenCacheSizeIsExceeded() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestClosesOldestIndexWriterWhenCacheSizeIsExceeded()
		 {
			  AddIndex( "bar" );
			  AddIndex( "baz" );
			  Config config = Config.defaults( CacheSizeConfig() );
			  _dataSource = _life.add( GetLuceneDataSource( config ) );
			  IndexIdentifier fooIdentifier = Identifier( "foo" );
			  IndexIdentifier barIdentifier = Identifier( "bar" );
			  IndexIdentifier bazIdentifier = Identifier( "baz" );
			  IndexWriter fooIndexWriter = _dataSource.getIndexSearcher( fooIdentifier ).Writer;
			  _dataSource.getIndexSearcher( barIdentifier );
			  assertTrue( fooIndexWriter.Open );
			  _dataSource.getIndexSearcher( bazIdentifier );
			  assertFalse( fooIndexWriter.Open );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClosesOldestIndexSearcherWhenCacheSizeIsExceeded() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestClosesOldestIndexSearcherWhenCacheSizeIsExceeded()
		 {
			  AddIndex( "bar" );
			  AddIndex( "baz" );
			  Config config = Config.defaults( CacheSizeConfig() );
			  _dataSource = _life.add( GetLuceneDataSource( config ) );
			  IndexIdentifier fooIdentifier = Identifier( "foo" );
			  IndexIdentifier barIdentifier = Identifier( "bar" );
			  IndexIdentifier bazIdentifier = Identifier( "baz" );
			  IndexReference fooSearcher = _dataSource.getIndexSearcher( fooIdentifier );
			  IndexReference barSearcher = _dataSource.getIndexSearcher( barIdentifier );
			  assertFalse( fooSearcher.Closed );
			  IndexReference bazSearcher = _dataSource.getIndexSearcher( bazIdentifier );
			  assertTrue( fooSearcher.Closed );
			  barSearcher.Close();
			  bazSearcher.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRecreatesSearcherWhenRequestedAgain() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRecreatesSearcherWhenRequestedAgain()
		 {
			  AddIndex( "bar" );
			  AddIndex( "baz" );
			  Config config = Config.defaults( CacheSizeConfig() );
			  _dataSource = _life.add( GetLuceneDataSource( config ) );
			  IndexIdentifier fooIdentifier = Identifier( "foo" );
			  IndexIdentifier barIdentifier = Identifier( "bar" );
			  IndexIdentifier bazIdentifier = Identifier( "baz" );
			  IndexReference oldFooSearcher = _dataSource.getIndexSearcher( fooIdentifier );
			  IndexReference barSearcher = _dataSource.getIndexSearcher( barIdentifier );
			  IndexReference bazSearcher = _dataSource.getIndexSearcher( bazIdentifier );
			  IndexReference newFooSearcher = _dataSource.getIndexSearcher( bazIdentifier );
			  assertNotSame( oldFooSearcher, newFooSearcher );
			  assertFalse( newFooSearcher.Closed );
			  oldFooSearcher.Close();
			  barSearcher.Close();
			  bazSearcher.Close();
			  newFooSearcher.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRecreatesWriterWhenRequestedAgainAfterCacheEviction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRecreatesWriterWhenRequestedAgainAfterCacheEviction()
		 {
			  AddIndex( "bar" );
			  AddIndex( "baz" );
			  Config config = Config.defaults( CacheSizeConfig() );
			  _dataSource = _life.add( GetLuceneDataSource( config ) );
			  IndexIdentifier fooIdentifier = Identifier( "foo" );
			  IndexIdentifier barIdentifier = Identifier( "bar" );
			  IndexIdentifier bazIdentifier = Identifier( "baz" );
			  IndexWriter oldFooIndexWriter = _dataSource.getIndexSearcher( fooIdentifier ).Writer;
			  _dataSource.getIndexSearcher( barIdentifier );
			  _dataSource.getIndexSearcher( bazIdentifier );
			  IndexWriter newFooIndexWriter = _dataSource.getIndexSearcher( fooIdentifier ).Writer;
			  assertNotSame( oldFooIndexWriter, newFooIndexWriter );
			  assertTrue( newFooIndexWriter.Open );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void stopDataSource() throws java.io.IOException
		 private void StopDataSource()
		 {
			  _dataSource.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareIndexesByIdentifiers(IndexIdentifier indexIdentifier) throws Exception
		 private void PrepareIndexesByIdentifiers( IndexIdentifier indexIdentifier )
		 {
			  Config config = Config.defaults();
			  _dataSource = _life.add( GetLuceneDataSource( config ) );
			  _dataSource.getIndexSearcher( indexIdentifier );
			  _dataSource.force();
		 }

		 private static IDictionary<string, string> ReadOnlyConfig()
		 {
			  return stringMap( GraphDatabaseSettings.read_only.name(), "true" );
		 }

		 private static IDictionary<string, string> CacheSizeConfig()
		 {
			  return stringMap( GraphDatabaseSettings.lucene_searcher_cache_size.name(), "2" );
		 }

		 private void AddIndex( string name )
		 {
			  _indexStore.set( typeof( Node ), name, stringMap( Org.Neo4j.Graphdb.index.IndexManager_Fields.PROVIDER, "lucene", "type", "fulltext" ) );
		 }

		 private static IndexIdentifier Identifier( string name )
		 {
			  return new IndexIdentifier( IndexEntityType.Node, name );
		 }

		 private LuceneDataSource GetLuceneDataSource( Config config )
		 {
			  return GetLuceneDataSource( config, OperationalMode.unknown );
		 }

		 private LuceneDataSource GetLuceneDataSource( Config config, OperationalMode operationalMode )
		 {
			  return new LuceneDataSource( _directory.databaseLayout(), config, _indexStore, new DefaultFileSystemAbstraction(), operationalMode );
		 }
	}

}