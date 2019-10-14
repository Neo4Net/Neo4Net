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
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Neo4jTestCase.assertContains;

	public class TestIndexNames
	{
		 private static GraphDatabaseService _graphDb;
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpStuff()
		 public static void SetUpStuff()
		 {
			  _graphDb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownStuff()
		 public static void TearDownStuff()
		 {
			  _graphDb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void commitTx()
		 public virtual void CommitTx()
		 {
			  FinishTx( true );
		 }

		 public virtual void FinishTx( bool success )
		 {
			  if ( _tx != null )
			  {
					if ( success )
					{
						 _tx.success();
					}
					_tx.close();
					_tx = null;
			  }
		 }

		 public virtual void BeginTx()
		 {
			  if ( _tx == null )
			  {
					_tx = _graphDb.beginTx();
			  }
		 }

		 internal virtual void RestartTx()
		 {
			  FinishTx( true );
			  BeginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureIndexNamesCanBeRead()
		 public virtual void MakeSureIndexNamesCanBeRead()
		 {
			  BeginTx();
			  assertEquals( 0, _graphDb.index().nodeIndexNames().length );
			  string name1 = "my-index-1";
			  Index<Node> nodeIndex1 = _graphDb.index().forNodes(name1);
			  assertContains( Arrays.asList( _graphDb.index().nodeIndexNames() ), name1 );
			  string name2 = "my-index-2";
			  _graphDb.index().forNodes(name2);
			  assertContains( Arrays.asList( _graphDb.index().nodeIndexNames() ), name1, name2 );
			  _graphDb.index().forRelationships(name1);
			  assertContains( Arrays.asList( _graphDb.index().nodeIndexNames() ), name1, name2 );
			  assertContains( Arrays.asList( _graphDb.index().relationshipIndexNames() ), name1 );
			  FinishTx( true );

			  RestartTx();
			  assertContains( Arrays.asList( _graphDb.index().nodeIndexNames() ), name1, name2 );
			  assertContains( Arrays.asList( _graphDb.index().relationshipIndexNames() ), name1 );
			  nodeIndex1.Delete();
			  assertContains( Arrays.asList( _graphDb.index().nodeIndexNames() ), name1, name2 );
			  assertContains( Arrays.asList( _graphDb.index().relationshipIndexNames() ), name1 );
			  FinishTx( true );
			  BeginTx();
			  assertContains( Arrays.asList( _graphDb.index().nodeIndexNames() ), name2 );
			  assertContains( Arrays.asList( _graphDb.index().relationshipIndexNames() ), name1 );
			  FinishTx( false );
		 }
	}

}