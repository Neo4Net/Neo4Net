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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;

	using Node = Org.Neo4j.Graphdb.Node;
	using IndexManager = Org.Neo4j.Graphdb.index.IndexManager;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;

	public class ReadOnlyIndexReferenceFactoryTest
	{
		private bool InstanceFieldsInitialized = false;

		public ReadOnlyIndexReferenceFactoryTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _cleanupRule ).around( _expectedException ).around( _testDirectory ).around( _fileSystemRule );
		}

		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly ExpectedException _expectedException = ExpectedException.none();
		 private readonly CleanupRule _cleanupRule = new CleanupRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(cleanupRule).around(expectedException).around(testDirectory).around(fileSystemRule);
		 public RuleChain RuleChain;

		 private const string INDEX_NAME = "testIndex";
		 private LuceneDataSource.LuceneFilesystemFacade _filesystemFacade = LuceneDataSource.LuceneFilesystemFacade.Fs;
		 private IndexIdentifier _indexIdentifier = new IndexIdentifier( IndexEntityType.Node, INDEX_NAME );
		 private IndexConfigStore _indexStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  SetupIndexInfrastructure();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createReadOnlyIndexReference() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateReadOnlyIndexReference()
		 {
			  ReadOnlyIndexReferenceFactory indexReferenceFactory = ReadOnlyIndexReferenceFactory;
			  IndexReference indexReference = indexReferenceFactory.CreateIndexReference( _indexIdentifier );
			  _cleanupRule.add( indexReference );

			  _expectedException.expect( typeof( System.NotSupportedException ) );
			  indexReference.Writer;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void refreshReadOnlyIndexReference() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RefreshReadOnlyIndexReference()
		 {
			  ReadOnlyIndexReferenceFactory indexReferenceFactory = ReadOnlyIndexReferenceFactory;
			  IndexReference indexReference = indexReferenceFactory.CreateIndexReference( _indexIdentifier );
			  _cleanupRule.add( indexReference );

			  IndexReference refreshedIndex = indexReferenceFactory.Refresh( indexReference );
			  assertSame( "Refreshed instance should be the same.", indexReference, refreshedIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setupIndexInfrastructure() throws Exception
		 private void SetupIndexInfrastructure()
		 {
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  _indexStore = new IndexConfigStore( databaseLayout, _fileSystemRule.get() );
			  _indexStore.set( typeof( Node ), INDEX_NAME, MapUtil.stringMap( Org.Neo4j.Graphdb.index.IndexManager_Fields.PROVIDER, "lucene", "type", "fulltext" ) );
			  LuceneDataSource luceneDataSource = new LuceneDataSource( databaseLayout, Config.defaults(), _indexStore, _fileSystemRule.get(), OperationalMode.single );
			  try
			  {
					luceneDataSource.Init();
					luceneDataSource.GetIndexSearcher( _indexIdentifier );
			  }
			  finally
			  {
					luceneDataSource.Shutdown();
			  }
		 }

		 private ReadOnlyIndexReferenceFactory ReadOnlyIndexReferenceFactory
		 {
			 get
			 {
				  return new ReadOnlyIndexReferenceFactory( _filesystemFacade, _testDirectory.databaseLayout().file("index"), new IndexTypeCache(_indexStore) );
			 }
		 }
	}

}