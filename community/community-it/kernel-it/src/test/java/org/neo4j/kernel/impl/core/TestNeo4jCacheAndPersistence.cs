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
namespace Org.Neo4j.Kernel.impl.core
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;

	public class TestNeo4jCacheAndPersistence : AbstractNeo4jTestCase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private long _node1Id = -1;
		 private long _node2Id = -1;
		 private readonly string _key1 = "key1";
		 private readonly string _key2 = "key2";
		 private readonly string _arrayKey = "arrayKey";
		 private readonly int? _int1 = 1;
		 private readonly int? _int2 = 2;
		 private readonly string _string1 = "1";
		 private readonly string _string2 = "2";
		 private readonly int[] _array = new int[] { 1, 2, 3, 4, 5, 6, 7 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createTestingGraph()
		 public virtual void CreateTestingGraph()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  _node1Id = node1.Id;
			  _node2Id = node2.Id;
			  node1.SetProperty( _key1, _int1 );
			  node1.SetProperty( _key2, _string1 );
			  node2.SetProperty( _key1, _int2 );
			  node2.SetProperty( _key2, _string2 );
			  rel.SetProperty( _key1, _int1 );
			  rel.SetProperty( _key2, _string1 );
			  node1.SetProperty( _arrayKey, _array );
			  node2.SetProperty( _arrayKey, _array );
			  rel.SetProperty( _arrayKey, _array );
			  Transaction tx = Transaction;
			  tx.Success();
			  tx.Close();
			  tx = GraphDb.beginTx();
			  assertEquals( 1, node1.GetProperty( _key1 ) );
			  Transaction = tx;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void deleteTestingGraph()
		 public virtual void DeleteTestingGraph()
		 {
			  Node node1 = GraphDb.getNodeById( _node1Id );
			  Node node2 = GraphDb.getNodeById( _node2Id );
			  node1.GetSingleRelationship( MyRelTypes.TEST, Direction.BOTH ).delete();
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddProperty()
		 public virtual void TestAddProperty()
		 {
			  string key3 = "key3";

			  Node node1 = GraphDb.getNodeById( _node1Id );
			  Node node2 = GraphDb.getNodeById( _node2Id );
			  Relationship rel = node1.GetSingleRelationship( MyRelTypes.TEST, Direction.BOTH );
			  // add new property
			  node2.SetProperty( key3, _int1 );
			  rel.SetProperty( key3, _int2 );
			  assertTrue( node1.HasProperty( _key1 ) );
			  assertTrue( node2.HasProperty( _key1 ) );
			  assertTrue( node1.HasProperty( _key2 ) );
			  assertTrue( node2.HasProperty( _key2 ) );
			  assertTrue( node1.HasProperty( _arrayKey ) );
			  assertTrue( node2.HasProperty( _arrayKey ) );
			  assertTrue( rel.HasProperty( _arrayKey ) );
			  assertTrue( !node1.HasProperty( key3 ) );
			  assertTrue( node2.HasProperty( key3 ) );
			  assertEquals( _int1, node1.GetProperty( _key1 ) );
			  assertEquals( _int2, node2.GetProperty( _key1 ) );
			  assertEquals( _string1, node1.GetProperty( _key2 ) );
			  assertEquals( _string2, node2.GetProperty( _key2 ) );
			  assertEquals( _int1, rel.GetProperty( _key1 ) );
			  assertEquals( _string1, rel.GetProperty( _key2 ) );
			  assertEquals( _int2, rel.GetProperty( key3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeRemoveProperty()
		 public virtual void TestNodeRemoveProperty()
		 {
			  Node node1 = GraphDb.getNodeById( _node1Id );
			  Node node2 = GraphDb.getNodeById( _node2Id );
			  Relationship rel = node1.GetSingleRelationship( MyRelTypes.TEST, Direction.BOTH );

			  // test remove property
			  assertEquals( 1, node1.RemoveProperty( _key1 ) );
			  assertEquals( 2, node2.RemoveProperty( _key1 ) );
			  assertEquals( 1, rel.RemoveProperty( _key1 ) );
			  assertEquals( _string1, node1.RemoveProperty( _key2 ) );
			  assertEquals( _string2, node2.RemoveProperty( _key2 ) );
			  assertEquals( _string1, rel.RemoveProperty( _key2 ) );
			  assertNotNull( node1.RemoveProperty( _arrayKey ) );
			  assertNotNull( node2.RemoveProperty( _arrayKey ) );
			  assertNotNull( rel.RemoveProperty( _arrayKey ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeChangeProperty()
		 public virtual void TestNodeChangeProperty()
		 {
			  Node node1 = GraphDb.getNodeById( _node1Id );
			  Node node2 = GraphDb.getNodeById( _node2Id );
			  Relationship rel = node1.GetSingleRelationship( MyRelTypes.TEST, Direction.BOTH );

			  // test change property
			  node1.SetProperty( _key1, _int2 );
			  node2.SetProperty( _key1, _int1 );
			  rel.SetProperty( _key1, _int2 );
			  int[] newIntArray = new int[] { 3, 2, 1 };
			  node1.SetProperty( _arrayKey, newIntArray );
			  node2.SetProperty( _arrayKey, newIntArray );
			  rel.SetProperty( _arrayKey, newIntArray );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeGetProperties()
		 public virtual void TestNodeGetProperties()
		 {
			  Node node1 = GraphDb.getNodeById( _node1Id );

			  assertTrue( !node1.HasProperty( null ) );
			  IEnumerator<string> keys = node1.PropertyKeys.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  keys.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  keys.next();
			  assertTrue( node1.HasProperty( _key1 ) );
			  assertTrue( node1.HasProperty( _key2 ) );
		 }

		 private Relationship[] GetRelationshipArray( IEnumerable<Relationship> relsIterable )
		 {
			  List<Relationship> relList = new List<Relationship>();
			  foreach ( Relationship rel in relsIterable )
			  {
					relList.Add( rel );
			  }
			  return relList.ToArray();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDirectedRelationship1()
		 public virtual void TestDirectedRelationship1()
		 {
			  Node node1 = GraphDb.getNodeById( _node1Id );
			  Relationship rel = node1.GetSingleRelationship( MyRelTypes.TEST, Direction.BOTH );
			  Node[] nodes = rel.Nodes;
			  assertEquals( 2, nodes.Length );

			  Node node2 = GraphDb.getNodeById( _node2Id );
			  assertTrue( nodes[0].Equals( node1 ) && nodes[1].Equals( node2 ) );
			  assertEquals( node1, rel.StartNode );
			  assertEquals( node2, rel.EndNode );

			  Relationship[] relArray = GetRelationshipArray( node1.GetRelationships( MyRelTypes.TEST, Direction.OUTGOING ) );
			  assertEquals( 1, relArray.Length );
			  assertEquals( rel, relArray[0] );
			  relArray = GetRelationshipArray( node2.GetRelationships( MyRelTypes.TEST, Direction.INCOMING ) );
			  assertEquals( 1, relArray.Length );
			  assertEquals( rel, relArray[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelCountInSameTx()
		 public virtual void TestRelCountInSameTx()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  assertEquals( 1, GetRelationshipArray( node1.Relationships ).Length );
			  assertEquals( 1, GetRelationshipArray( node2.Relationships ).Length );
			  rel.Delete();
			  assertEquals( 0, GetRelationshipArray( node1.Relationships ).Length );
			  assertEquals( 0, GetRelationshipArray( node2.Relationships ).Length );
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetDirectedRelationship()
		 public virtual void TestGetDirectedRelationship()
		 {
			  Node node1 = GraphDb.getNodeById( _node1Id );
			  Relationship rel = node1.GetSingleRelationship( MyRelTypes.TEST, Direction.OUTGOING );
			  assertEquals( _int1, rel.GetProperty( _key1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSameTxWithArray()
		 public virtual void TestSameTxWithArray()
		 {
			  Commit();
			  NewTransaction();

			  Node nodeA = GraphDb.createNode();
			  Node nodeB = GraphDb.createNode();
			  Relationship relA = nodeA.CreateRelationshipTo( nodeB, MyRelTypes.TEST );
			  nodeA.SetProperty( _arrayKey, _array );
			  relA.SetProperty( _arrayKey, _array );
			  assertNotNull( nodeA.GetProperty( _arrayKey ) );
			  assertNotNull( relA.GetProperty( _arrayKey ) );
			  relA.Delete();
			  nodeA.Delete();
			  nodeB.Delete();

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddCacheCleared()
		 public virtual void TestAddCacheCleared()
		 {
			  Node nodeA = GraphDb.createNode();
			  nodeA.SetProperty( "1", 1 );
			  Node nodeB = GraphDb.createNode();
			  Relationship rel = nodeA.CreateRelationshipTo( nodeB, MyRelTypes.TEST );
			  rel.SetProperty( "1", 1 );
			  Commit();
			  NewTransaction();
			  nodeA.CreateRelationshipTo( nodeB, MyRelTypes.TEST );
			  int count = 0;
			  foreach ( Relationship relToB in nodeA.GetRelationships( MyRelTypes.TEST ) )
			  {
					count++;
			  }
			  assertEquals( 2, count );
			  nodeA.SetProperty( "2", 2 );
			  assertEquals( 1, nodeA.GetProperty( "1" ) );
			  rel.SetProperty( "2", 2 );
			  assertEquals( 1, rel.GetProperty( "1" ) );
			  // trigger empty load
			  GraphDb.getNodeById( nodeA.Id );
			  GraphDb.getRelationshipById( rel.Id );
			  // apply COW maps
			  Commit();
			  NewTransaction();
			  count = 0;
			  foreach ( Relationship relToB in nodeA.GetRelationships( MyRelTypes.TEST ) )
			  {
					count++;
			  }
			  assertEquals( 2, count );
			  assertEquals( 1, nodeA.GetProperty( "1" ) );
			  assertEquals( 1, rel.GetProperty( "1" ) );
			  assertEquals( 2, nodeA.GetProperty( "2" ) );
			  assertEquals( 2, rel.GetProperty( "2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeMultiRemoveProperty()
		 public virtual void TestNodeMultiRemoveProperty()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "key0", "0" );
			  node.SetProperty( "key1", "1" );
			  node.SetProperty( "key2", "2" );
			  node.SetProperty( "key3", "3" );
			  node.SetProperty( "key4", "4" );
			  NewTransaction();
			  node.RemoveProperty( "key3" );
			  node.RemoveProperty( "key2" );
			  node.RemoveProperty( "key3" );
			  NewTransaction();
			  assertEquals( "0", node.GetProperty( "key0" ) );
			  assertEquals( "1", node.GetProperty( "key1" ) );
			  assertEquals( "4", node.GetProperty( "key4" ) );
			  assertTrue( !node.HasProperty( "key2" ) );
			  assertTrue( !node.HasProperty( "key3" ) );
			  node.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelMultiRemoveProperty()
		 public virtual void TestRelMultiRemoveProperty()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  rel.SetProperty( "key0", "0" );
			  rel.SetProperty( "key1", "1" );
			  rel.SetProperty( "key2", "2" );
			  rel.SetProperty( "key3", "3" );
			  rel.SetProperty( "key4", "4" );
			  NewTransaction();
			  rel.RemoveProperty( "key3" );
			  rel.RemoveProperty( "key2" );
			  rel.RemoveProperty( "key3" );
			  NewTransaction();
			  assertEquals( "0", rel.GetProperty( "key0" ) );
			  assertEquals( "1", rel.GetProperty( "key1" ) );
			  assertEquals( "4", rel.GetProperty( "key4" ) );
			  assertTrue( !rel.HasProperty( "key2" ) );
			  assertTrue( !rel.HasProperty( "key3" ) );
			  rel.Delete();
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLowGrabSize()
		 public virtual void TestLowGrabSize()
		 {
			  IDictionary<string, string> config = new Dictionary<string, string>();
			  config["relationship_grab_size"] = "1";
			  GraphDatabaseService graphDb = GetImpermanentDatabase( config );

			  Node node1;
			  Node node2;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					node1 = graphDb.CreateNode();
					node2 = graphDb.CreateNode();
					node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
					node2.CreateRelationshipTo( node1, MyRelTypes.TEST2 );
					node1.CreateRelationshipTo( node2, MyRelTypes.TEST_TRAVERSAL );
					tx.Success();
			  }

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					RelationshipType[] types = new RelationshipType[]{ MyRelTypes.TEST, MyRelTypes.TEST2, MyRelTypes.TEST_TRAVERSAL };

					assertEquals( 3, Iterables.count( node1.GetRelationships( types ) ) );

					assertEquals( 3, Iterables.count( node1.Relationships ) );

					assertEquals( 3, Iterables.count( node2.GetRelationships( types ) ) );

					assertEquals( 3, Iterables.count( node2.Relationships ) );

					assertEquals( 2, Iterables.count( node1.GetRelationships( OUTGOING ) ) );

					assertEquals( 1, Iterables.count( node1.GetRelationships( INCOMING ) ) );

					assertEquals( 1, Iterables.count( node2.GetRelationships( OUTGOING ) ) );

					assertEquals( 2, Iterables.count( node2.GetRelationships( INCOMING ) ) );

					tx.Success();
			  }
			  graphDb.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAnotherLowGrabSize()
		 public virtual void TestAnotherLowGrabSize()
		 {
			  TestLowGrabSize( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAnotherLowGrabSizeWithLoops()
		 public virtual void TestAnotherLowGrabSizeWithLoops()
		 {
			  TestLowGrabSize( true );
		 }

		 private void TestLowGrabSize( bool includeLoops )
		 {
			  IDictionary<string, string> config = new Dictionary<string, string>();
			  config["relationship_grab_size"] = "2";
			  GraphDatabaseService graphDb = GetImpermanentDatabase( config );
			  Transaction tx = graphDb.BeginTx();
			  Node node1 = graphDb.CreateNode();
			  Node node2 = graphDb.CreateNode();
			  Node node3 = graphDb.CreateNode();

			  // These are expected relationships for node2
			  ICollection<Relationship> outgoingOriginal = new HashSet<Relationship>();
			  ICollection<Relationship> incomingOriginal = new HashSet<Relationship>();
			  ICollection<Relationship> loopsOriginal = new HashSet<Relationship>();

			  int total = 0;
			  int totalOneDirection = 0;
			  for ( int i = 0; i < 33; i++ )
			  {
					if ( includeLoops )
					{
						 loopsOriginal.Add( node2.CreateRelationshipTo( node2, MyRelTypes.TEST ) );
						 total++;
						 totalOneDirection++;
					}

					if ( i % 2 == 0 )
					{
						 incomingOriginal.Add( node1.CreateRelationshipTo( node2, MyRelTypes.TEST ) );
						 outgoingOriginal.Add( node2.CreateRelationshipTo( node3, MyRelTypes.TEST ) );
					}
					else
					{
						 outgoingOriginal.Add( node2.CreateRelationshipTo( node1, MyRelTypes.TEST ) );
						 incomingOriginal.Add( node3.CreateRelationshipTo( node2, MyRelTypes.TEST ) );
					}
					total += 2;
					totalOneDirection++;
			  }
			  tx.Success();
			  tx.Close();

			  tx = graphDb.BeginTx();
			  ISet<Relationship> rels = new HashSet<Relationship>();

			  ICollection<Relationship> outgoing = new HashSet<Relationship>( outgoingOriginal );
			  ICollection<Relationship> incoming = new HashSet<Relationship>( incomingOriginal );
			  ICollection<Relationship> loops = new HashSet<Relationship>( loopsOriginal );
			  foreach ( Relationship rel in node2.GetRelationships( MyRelTypes.TEST ) )
			  {
					assertTrue( rels.Add( rel ) );
					if ( rel.StartNode.Equals( node2 ) && rel.EndNode.Equals( node2 ) )
					{
						 assertTrue( loops.remove( rel ) );
					}
					else if ( rel.StartNode.Equals( node2 ) )
					{
						 assertTrue( outgoing.remove( rel ) );
					}
					else
					{
						 assertTrue( incoming.remove( rel ) );
					}
			  }
			  assertEquals( total, rels.Count );
			  assertEquals( 0, loops.Count );
			  assertEquals( 0, incoming.Count );
			  assertEquals( 0, outgoing.Count );
			  rels.Clear();

			  outgoing = new HashSet<Relationship>( outgoingOriginal );
			  incoming = new HashSet<Relationship>( incomingOriginal );
			  loops = new HashSet<Relationship>( loopsOriginal );
			  foreach ( Relationship rel in node2.GetRelationships( Direction.OUTGOING ) )
			  {
					assertTrue( rels.Add( rel ) );
					if ( rel.StartNode.Equals( node2 ) && rel.EndNode.Equals( node2 ) )
					{
						 assertTrue( loops.remove( rel ) );
					}
					else if ( rel.StartNode.Equals( node2 ) )
					{
						 assertTrue( outgoing.remove( rel ) );
					}
					else
					{
						 fail( "There should be no incoming relationships " + rel );
					}
			  }
			  assertEquals( totalOneDirection, rels.Count );
			  assertEquals( 0, loops.Count );
			  assertEquals( 0, outgoing.Count );
			  rels.Clear();

			  outgoing = new HashSet<Relationship>( outgoingOriginal );
			  incoming = new HashSet<Relationship>( incomingOriginal );
			  loops = new HashSet<Relationship>( loopsOriginal );
			  foreach ( Relationship rel in node2.GetRelationships( Direction.INCOMING ) )
			  {
					assertTrue( rels.Add( rel ) );
					if ( rel.StartNode.Equals( node2 ) && rel.EndNode.Equals( node2 ) )
					{
						 assertTrue( loops.remove( rel ) );
					}
					else if ( rel.EndNode.Equals( node2 ) )
					{
						 assertTrue( incoming.remove( rel ) );
					}
					else
					{
						 fail( "There should be no outgoing relationships " + rel );
					}
			  }
			  assertEquals( totalOneDirection, rels.Count );
			  assertEquals( 0, loops.Count );
			  assertEquals( 0, incoming.Count );
			  rels.Clear();

			  tx.Success();
			  tx.Close();
			  graphDb.Shutdown();
		 }

		 private GraphDatabaseService GetImpermanentDatabase( IDictionary<string, string> config )
		 {
			  return ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder(TestDirectory.directory("impermanent")).setConfig(config).newGraphDatabase();
		 }
	}

}