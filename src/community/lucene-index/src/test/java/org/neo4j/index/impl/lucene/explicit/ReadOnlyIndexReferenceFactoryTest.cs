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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;

	using Node = Neo4Net.Graphdb.Node;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

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
			  _indexStore.set( typeof( Node ), INDEX_NAME, MapUtil.stringMap( Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, "lucene", "type", "fulltext" ) );
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