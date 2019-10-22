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
namespace Neo4Net.Index
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index;
	using RelationshipIndex = Neo4Net.GraphDb.index.RelationshipIndex;
	using QueryContext = Neo4Net.Index.lucene.QueryContext;
	using ValueContext = Neo4Net.Index.lucene.ValueContext;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.security.AccessMode_Static.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.security.AuthSubject.ANONYMOUS;

	public class ExplicitIndexTest
	{
		 private static readonly RelationshipType _type = RelationshipType.withName( "TYPE" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removalOfNodeIndexDoesNotInfluenceRelationshipIndexWithSameName()
		 public virtual void RemovalOfNodeIndexDoesNotInfluenceRelationshipIndexWithSameName()
		 {
			  string indexName = "index";

			  CreateNodeExplicitIndexWithSingleNode( Db, indexName );
			  CreateRelationshipExplicitIndexWithSingleRelationship( Db, indexName );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship relationship = Db.createNode().createRelationshipTo(Db.createNode(), _type);
					Index<Relationship> relationshipIndex = Db.index().forRelationships(indexName);
					relationshipIndex.Add( relationship, "key", "otherValue" );

					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Delete();

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertFalse( Db.index().existsForNodes(indexName) );
					Index<Relationship> relationshipIndex = Db.index().forRelationships(indexName);
					assertEquals( 2, SizeOf( relationshipIndex ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removalOfRelationshipIndexDoesNotInfluenceNodeIndexWithSameName()
		 public virtual void RemovalOfRelationshipIndexDoesNotInfluenceNodeIndexWithSameName()
		 {
			  string indexName = "index";

			  CreateNodeExplicitIndexWithSingleNode( Db, indexName );
			  CreateRelationshipExplicitIndexWithSingleRelationship( Db, indexName );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Add( node, "key", "otherValue" );

					Index<Relationship> relationshipIndex = Db.index().forRelationships(indexName);
					relationshipIndex.Delete();

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertFalse( Db.index().existsForRelationships(indexName) );
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					assertEquals( 2, SizeOf( nodeIndex ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIllegalArgumentChangingTypeOfFieldOnNodeIndex()
		 public virtual void ShouldThrowIllegalArgumentChangingTypeOfFieldOnNodeIndex()
		 {
			  string indexName = "index";

			  CreateNodeExplicitIndexWithSingleNode( Db, indexName );

			  long nodeId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					nodeId = node.Id;
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Add( node, "key", "otherValue" );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Remove( Db.getNodeById( nodeId ), "key" );
					tx.Success();
			  }

			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Add( Db.getNodeById( nodeId ), "key", ValueContext.numeric( 52 ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIllegalArgumentChangingTypeOfFieldOnRelationshipIndex()
		 public virtual void ShouldThrowIllegalArgumentChangingTypeOfFieldOnRelationshipIndex()
		 {
			  string indexName = "index";

			  CreateRelationshipExplicitIndexWithSingleRelationship( Db, indexName );

			  long relId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					Relationship rel = node.CreateRelationshipTo( node, _type );
					relId = rel.Id;
					RelationshipIndex index = Db.index().forRelationships(indexName);
					index.add( rel, "key", "otherValue" );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					RelationshipIndex index = Db.index().forRelationships(indexName);
					index.remove( Db.getRelationshipById( relId ), "key" );
					tx.Success();
			  }

			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					RelationshipIndex index = Db.index().forRelationships(indexName);
					index.add( Db.getRelationshipById( relId ), "key", ValueContext.numeric( 52 ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAddNodesAfterRemovalOfKey()
		 public virtual void ShouldBeAbleToAddNodesAfterRemovalOfKey()
		 {
			  string indexName = "index";
			  long nodeId;
			  //add two keys and delete one of them
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					nodeId = node.Id;
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Add( node, "key", "hej" );
					nodeIndex.Add( node, "keydelete", "hej" );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Remove( Db.getNodeById( nodeId ), "keydelete" );
					tx.Success();
			  }

			  Db.shutdownAndKeepStore();
			  Db.GraphDatabaseAPI;

			  //should be able to add more stuff to the index
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Add( node, "key", "hej" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexContentsShouldStillBeOrderedAfterRemovalOfKey()
		 public virtual void IndexContentsShouldStillBeOrderedAfterRemovalOfKey()
		 {
			  string indexName = "index";
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.index().forNodes(indexName);
					tx.Success();
			  }

			  long delete;
			  long first;
			  long second;
			  long third;
			  long fourth;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					Node node = Db.createNode();
					delete = node.Id;
					nodeIndex.Add( node, "keydelte", "delete" );
					node = Db.createNode();
					second = node.Id;
					nodeIndex.Add( node, "key", ValueContext.numeric( 2 ) );
					nodeIndex.Add( node, "keydelte", "delete" );

					node = Db.createNode();
					fourth = node.Id;
					nodeIndex.Add( node, "key", ValueContext.numeric( 4 ) );
					nodeIndex.Add( node, "keydelte", "delete" );

					node = Db.createNode();
					first = node.Id;
					nodeIndex.Add( node, "key", ValueContext.numeric( 1 ) );
					nodeIndex.Add( node, "keydelte", "delete" );

					node = Db.createNode();
					third = node.Id;
					nodeIndex.Add( node, "key", ValueContext.numeric( 3 ) );
					nodeIndex.Add( node, "keydelte", "delete" );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					IndexHits<Node> query = nodeIndex.query( "key", QueryContext.numericRange( "key", 2, 3 ) );
					assertEquals( 2, query.Size() );
					query.forEachRemaining( node => assertTrue( node.Id == second || node.Id == third ) );
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Remove( Db.getNodeById( delete ), "keydelete" );
					nodeIndex.Remove( Db.getNodeById( first ), "keydelete" );
					nodeIndex.Remove( Db.getNodeById( second ), "keydelete" );
					nodeIndex.Remove( Db.getNodeById( third ), "keydelete" );
					nodeIndex.Remove( Db.getNodeById( fourth ), "keydelete" );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					IndexHits<Node> query = nodeIndex.query( "key", QueryContext.numericRange( "key", 2, 3 ) );
					assertEquals( 2, query.Size() );
					query.forEachRemaining( node => assertTrue( node.Id == second || node.Id == third ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipIndexShouldBeAbleToReindexInSameTransaction()
		 public virtual void RelationshipIndexShouldBeAbleToReindexInSameTransaction()
		 {
			  // Create relationship and index
			  Node startNode;
			  Node endNode;
			  Relationship relationship;
			  RelationshipIndex index;
			  using ( Transaction tx = Db.beginTx() )
			  {
					startNode = Db.createNode();
					endNode = Db.createNode();
					relationship = startNode.CreateRelationshipTo( endNode, _type );

					index = Db.index().forRelationships(_type.name());
					index.add( relationship, "key", ( new ValueContext( 1 ) ).indexNumeric() );

					tx.Success();
			  }

			  // Verify
			  assertTrue( "Find relationship by property", RelationshipExistsByQuery( index, startNode, endNode, false ) );
			  assertTrue( "Find relationship by property and start node", RelationshipExistsByQuery( index, startNode, endNode, true ) );

			  // Reindex
			  using ( Transaction tx = Db.beginTx() )
			  {
					index.remove( relationship );
					index.add( relationship, "key", ( new ValueContext( 2 ) ).indexNumeric() );
					tx.Success();
			  }

			  // Verify again
			  assertTrue( "Find relationship by property", RelationshipExistsByQuery( index, startNode, endNode, false ) );
			  assertTrue( "Find relationship by property and start node", RelationshipExistsByQuery( index, startNode, endNode, true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getSingleMustNotCloseStatementTwice()
		 public virtual void getSingleMustNotCloseStatementTwice()
		 {
			  // given
			  string indexName = "index";
			  long expected1;
			  long expected2;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node1 = Db.createNode();
					Node node2 = Db.createNode();
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Add( node1, "key", "hej" );
					nodeIndex.Add( node2, "key", "hejhej" );

					expected1 = node1.Id;
					expected2 = node2.Id;
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> nodeIndex = Db.index().forNodes(indexName);

					// when using getSingle this should not close statement for outer loop
					IndexHits<Node> hits = nodeIndex.query( "key", "hej" );
					while ( hits.MoveNext() )
					{
						 Node actual1 = hits.Current;
						 assertEquals( expected1, actual1.Id );

						 IndexHits<Node> hits2 = nodeIndex.query( "key", "hejhej" );
						 Node actual2 = hits2.Single;
						 assertEquals( expected2, actual2.Id );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetSingleHitAfterCallToHasNext()
		 public virtual void ShouldBeAbleToGetSingleHitAfterCallToHasNext()
		 {
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  try
			  {
					// given
					Index<Node> nodeIndex;
					Index<Relationship> relationshipIndex;
					using ( Transaction tx = Db.beginTx() )
					{
						 nodeIndex = Db.index().forNodes("MyIndex");
						 relationshipIndex = Db.index().forRelationships("MyIndex");
						 tx.Success();
					}
					string key = "key";
					string value = "value";
					Node node;
					Relationship relationship;
					using ( Transaction tx = Db.beginTx() )
					{
						 node = Db.createNode();
						 nodeIndex.Add( node, key, value );

						 relationship = node.CreateRelationshipTo( node, MyRelTypes.TEST );
						 relationshipIndex.Add( relationship, key, value );
						 tx.Success();
					}
					AssertFindSingleHit( db, nodeIndex, key, value, node );
					AssertFindSingleHit( db, relationshipIndex, key, value, relationship );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private void AssertFindSingleHit<T>( IGraphDatabaseService db, Index<T> nodeIndex, string key, string value, T IEntity ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  // when get using hasNext + next, then
			  assertEquals(entity, FindSingle(db, nodeIndex, key, value, hits =>
			  {
				assertTrue( hits.hasNext() );
				T result = hits.next();
				assertFalse( hits.hasNext() );
				return result;
			  }));
			  // when get using getSingle, then
			  assertEquals(entity, FindSingle(db, nodeIndex, key, value, hits =>
			  {
				T result = hits.Single;
				assertFalse( hits.hasNext() );
				return result;
			  }));
			  // when get using hasNext + getSingle, then
			  assertEquals(entity, FindSingle(db, nodeIndex, key, value, hits =>
			  {
				assertTrue( hits.hasNext() );
				T result = hits.Single;
				assertFalse( hits.hasNext() );
				return result;
			  }));
		 }

		 private T FindSingle<T>( IGraphDatabaseService db, Index<T> index, string key, string value, System.Func<IndexHits<T>, T> getter ) where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					using ( IndexHits<T> hits = index.get( key, value ) )
					{
						 T IEntity = getter( hits );
						 tx.Success();
						 return IEntity;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReadTransactionToSkipDeletedNodes()
		 public virtual void ShouldAllowReadTransactionToSkipDeletedNodes()
		 {
			  // given an indexed node
			  string indexName = "index";
			  Index<Node> nodeIndex;
			  Node node;
			  string key = "key";
			  string value = "value";
			  using ( Transaction tx = Db.beginTx() )
			  {
					nodeIndex = Db.index().forNodes(indexName);
					node = Db.createNode();
					nodeIndex.Add( node, key, value );
					tx.Success();
			  }
			  // delete the node, but keep it in the index
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTransaction( @explicit, new SecurityContext( ANONYMOUS, READ ) ) )
			  {
					IndexHits<Node> hits = nodeIndex.get( key, value );
					// then
					assertNull( hits.Single );
			  }
			  // also the fact that a read-only tx can do this w/o running into permission violation is good
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReadTransactionToSkipDeletedRelationships()
		 public virtual void ShouldAllowReadTransactionToSkipDeletedRelationships()
		 {
			  // given an indexed relationship
			  string indexName = "index";
			  Index<Relationship> relationshipIndex;
			  Relationship relationship;
			  string key = "key";
			  string value = "value";
			  using ( Transaction tx = Db.beginTx() )
			  {
					relationshipIndex = Db.index().forRelationships(indexName);
					relationship = Db.createNode().createRelationshipTo(Db.createNode(), MyRelTypes.TEST);
					relationshipIndex.Add( relationship, key, value );
					tx.Success();
			  }
			  // delete the relationship, but keep it in the index
			  using ( Transaction tx = Db.beginTx() )
			  {
					relationship.Delete();
					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTransaction( @explicit, new SecurityContext( ANONYMOUS, READ ) ) )
			  {
					IndexHits<Relationship> hits = relationshipIndex.get( key, value );
					// then
					assertNull( hits.Single );
			  }
			  // also the fact that a read-only tx can do this w/o running into permission violation is good
		 }

		 private bool RelationshipExistsByQuery( RelationshipIndex index, Node startNode, Node endNode, bool specifyStartNode )
		 {
			  bool found = false;

			  using ( Transaction tx = Db.beginTx(), IndexHits<Relationship> query = index.Query("key", QueryContext.numericRange("key", 0, 3), specifyStartNode ? startNode : null, null) )
			  {
					foreach ( Relationship relationship in query )
					{
						 if ( relationship.StartNodeId == startNode.Id && relationship.EndNodeId == endNode.Id )
						 {
							  found = true;
							  break;
						 }
					}

					tx.Success();
			  }
			  return found;
		 }

		 private static void CreateNodeExplicitIndexWithSingleNode( IGraphDatabaseService db, string indexName )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					Index<Node> nodeIndex = Db.index().forNodes(indexName);
					nodeIndex.Add( node, "key", DateTimeHelper.CurrentUnixTimeMillis() );
					tx.Success();
			  }
		 }

		 private static void CreateRelationshipExplicitIndexWithSingleRelationship( IGraphDatabaseService db, string indexName )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship relationship = Db.createNode().createRelationshipTo(Db.createNode(), _type);
					Index<Relationship> relationshipIndexIndex = Db.index().forRelationships(indexName);
					relationshipIndexIndex.Add( relationship, "key", DateTimeHelper.CurrentUnixTimeMillis() );
					tx.Success();
			  }
		 }

		 private static int SizeOf<T1>( Index<T1> index )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: try (org.Neo4Net.graphdb.index.IndexHits<?> indexHits = index.query("_id_:*"))
			  using ( IndexHits<object> indexHits = index.query( "_id_:*" ) )
			  {
					return indexHits.Size();
			  }
		 }
	}

}