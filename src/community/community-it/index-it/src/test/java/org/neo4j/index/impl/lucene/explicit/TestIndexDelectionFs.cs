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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TestIndexDelectionFs
	{
		 private static GraphDatabaseAPI _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  _db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.databaseDir());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void doAfter()
		 public virtual void DoAfter()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexDeleteShouldDeleteDirectory()
		 public virtual void IndexDeleteShouldDeleteDirectory()
		 {
			  string indexName = "index";
			  string otherIndexName = "other-index";

			  File indexBaseDir = new File( TestDirectory.databaseDir(), "index" );
			  File pathToLuceneIndex = LuceneDataSource.GetFileDirectory( indexBaseDir, new IndexIdentifier( IndexEntityType.Node, indexName ) );
			  File pathToOtherLuceneIndex = LuceneDataSource.GetFileDirectory( indexBaseDir, new IndexIdentifier( IndexEntityType.Node, otherIndexName ) );

			  Index<Node> index;
			  using ( Transaction tx = _db.beginTx() )
			  {
					index = _db.index().forNodes(indexName);
					Index<Node> otherIndex = _db.index().forNodes(otherIndexName);
					Node node = _db.createNode();
					index.Add( node, "someKey", "someValue" );
					otherIndex.Add( node, "someKey", "someValue" );
					tx.Success();
			  }

			  // Here "index" and "other-index" indexes should exist

			  assertTrue( pathToLuceneIndex.exists() );
			  assertTrue( pathToOtherLuceneIndex.exists() );
			  using ( Transaction tx = _db.beginTx() )
			  {
					index.Delete();
					assertTrue( pathToLuceneIndex.exists() );
					assertTrue( pathToOtherLuceneIndex.exists() );
					tx.Success();
			  }

			  // Here only "other-index" should exist

			  assertFalse( pathToLuceneIndex.exists() );
			  assertTrue( pathToOtherLuceneIndex.exists() );
		 }
	}

}