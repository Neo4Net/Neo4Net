using System.Collections.Generic;

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
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;
	using ReadableRelationshipIndex = Neo4Net.Graphdb.index.ReadableRelationshipIndex;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TestAutoIndexReopen
	{

		 private GraphDatabaseAPI _graphDb;

		 private long _id1 = -1;
		 private long _id2 = -1;
		 private long _id3 = -1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startDb()
		 public virtual void StartDb()
		 {
			  _graphDb = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(new Dictionary<string, string>()).newGraphDatabase();

			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					// Create the node and relationship auto-indexes
					_graphDb.index().NodeAutoIndexer.Enabled = true;
					_graphDb.index().NodeAutoIndexer.startAutoIndexingProperty("nodeProp");
					_graphDb.index().RelationshipAutoIndexer.Enabled = true;
					_graphDb.index().RelationshipAutoIndexer.startAutoIndexingProperty("type");

					tx.Success();
			  }

			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					Node node1 = _graphDb.createNode();
					Node node2 = _graphDb.createNode();
					Node node3 = _graphDb.createNode();
					_id1 = node1.Id;
					_id2 = node2.Id;
					_id3 = node3.Id;
					Relationship rel = node1.CreateRelationshipTo( node2, RelationshipType.withName( "FOO" ) );
					rel.SetProperty( "type", "FOO" );

					tx.Success();

			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopDb()
		 public virtual void StopDb()
		 {
			  if ( _graphDb != null )
			  {
					_graphDb.shutdown();
			  }
			  _graphDb = null;
		 }

		 private ReadableRelationshipIndex RelationShipAutoIndex()
		 {
			  return _graphDb.index().RelationshipAutoIndexer.AutoIndex;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testForceOpenIfChanged()
		 public virtual void TestForceOpenIfChanged()
		 {
			  // do some actions to force the indexreader to be reopened
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					Node node1 = _graphDb.getNodeById( _id1 );
					Node node2 = _graphDb.getNodeById( _id2 );
					Node node3 = _graphDb.getNodeById( _id3 );

					node1.SetProperty( "np2", "test property" );

					node1.GetRelationships( RelationshipType.withName( "FOO" ) ).forEach( Relationship.delete );

					// check first node
					Relationship rel;
					using ( IndexHits<Relationship> hits = RelationShipAutoIndex().get("type", "FOO", node1, node3) )
					{
						 assertEquals( 0, hits.Size() );
					}
					// create second relation ship
					rel = node1.CreateRelationshipTo( node3, RelationshipType.withName( "FOO" ) );
					rel.SetProperty( "type", "FOO" );

					// check second node -> crashs with old FullTxData
					using ( IndexHits<Relationship> indexHits = RelationShipAutoIndex().get("type", "FOO", node1, node2) )
					{
						 assertEquals( 0, indexHits.Size() );
					}
					// create second relation ship
					rel = node1.CreateRelationshipTo( node2, RelationshipType.withName( "FOO" ) );
					rel.SetProperty( "type", "FOO" );
					using ( IndexHits<Relationship> relationships = RelationShipAutoIndex().get("type", "FOO", node1, node2) )
					{
						 assertEquals( 1, relationships.Size() );
					}

					tx.Success();
			  }
		 }
	}


}