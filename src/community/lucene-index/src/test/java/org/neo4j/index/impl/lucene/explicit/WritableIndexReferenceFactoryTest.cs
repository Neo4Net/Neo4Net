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
	using Document = org.apache.lucene.document.Document;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using Node = Neo4Net.GraphDb.Node;
	using IndexManager = Neo4Net.GraphDb.Index.IndexManager;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;

	public class WritableIndexReferenceFactoryTest
	{
		private bool InstanceFieldsInitialized = false;

		public WritableIndexReferenceFactoryTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _cleanupRule ).around( _fileSystemRule );
		}


		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly CleanupRule _cleanupRule = new CleanupRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(cleanupRule).around(fileSystemRule);
		 public RuleChain RuleChain;

		 private const string INDEX_NAME = "testIndex";

		 private LuceneDataSource.LuceneFilesystemFacade _filesystemFacade = LuceneDataSource.LuceneFilesystemFacade.Memory;
		 private IndexIdentifier _indexIdentifier = new IndexIdentifier( IndexEntityType.Node, INDEX_NAME );
		 private IndexConfigStore _indexStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  SetupIndexInfrastructure();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createWritableIndexReference() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateWritableIndexReference()
		 {
			  WritableIndexReferenceFactory indexReferenceFactory = CreateFactory();
			  IndexReference indexReference = CreateIndexReference( indexReferenceFactory );

			  assertNotNull( "Index should have writer.", indexReference.Writer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void refreshNotChangedWritableIndexReference() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RefreshNotChangedWritableIndexReference()
		 {
			  WritableIndexReferenceFactory indexReferenceFactory = CreateFactory();
			  IndexReference indexReference = CreateIndexReference( indexReferenceFactory );

			  IndexReference refreshedInstance = indexReferenceFactory.Refresh( indexReference );
			  assertSame( indexReference, refreshedInstance );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void refreshChangedWritableIndexReference() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RefreshChangedWritableIndexReference()
		 {
			  WritableIndexReferenceFactory indexReferenceFactory = CreateFactory();
			  IndexReference indexReference = CreateIndexReference( indexReferenceFactory );

			  WriteSomething( indexReference );

			  IndexReference refreshedIndexReference = indexReferenceFactory.Refresh( indexReference );
			  _cleanupRule.add( refreshedIndexReference );

			  assertNotSame( "Should return new refreshed index reference.", indexReference, refreshedIndexReference );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeSomething(IndexReference indexReference) throws java.io.IOException
		 private static void WriteSomething( IndexReference indexReference )
		 {
			  IndexWriter writer = indexReference.Writer;
			  writer.addDocument( new Document() );
			  writer.commit();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private IndexReference createIndexReference(WritableIndexReferenceFactory indexReferenceFactory) throws Exception
		 private IndexReference CreateIndexReference( WritableIndexReferenceFactory indexReferenceFactory )
		 {
			  IndexReference indexReference = indexReferenceFactory.CreateIndexReference( _indexIdentifier );
			  _cleanupRule.add( indexReference );
			  return indexReference;
		 }

		 private WritableIndexReferenceFactory CreateFactory()
		 {
			  return new WritableIndexReferenceFactory( _filesystemFacade, _testDirectory.databaseLayout().file("index"), new IndexTypeCache(_indexStore) );
		 }

		 private void SetupIndexInfrastructure()
		 {
			  _indexStore = new IndexConfigStore( _testDirectory.databaseLayout(), _fileSystemRule.get() );
			  _indexStore.set( typeof( Node ), INDEX_NAME, MapUtil.stringMap( Neo4Net.GraphDb.Index.IndexManager_Fields.PROVIDER, "lucene", "type", "fulltext" ) );
		 }
	}

}