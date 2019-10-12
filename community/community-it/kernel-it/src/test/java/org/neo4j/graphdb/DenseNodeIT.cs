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
namespace Org.Neo4j.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using MyRelTypes = Org.Neo4j.Kernel.impl.MyRelTypes;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asResourceIterator;

	public class DenseNodeIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule databaseRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DatabaseRule = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBringingNodeOverDenseThresholdIsConsistent()
		 public virtual void TestBringingNodeOverDenseThresholdIsConsistent()
		 {
			  // GIVEN
			  GraphDatabaseService db = DatabaseRule.GraphDatabaseAPI;

			  Node root;
			  using ( Transaction tx = Db.beginTx() )
			  {
					root = Db.createNode();
					CreateRelationshipsOnNode( db, root, 40 );
					tx.Success();
			  }

			  // WHEN
			  using ( Transaction tx = Db.beginTx() )
			  {
					CreateRelationshipsOnNode( db, root, 60 );

					// THEN
					assertEquals( 100, root.Degree );
					assertEquals( 100, root.GetDegree( Direction.Outgoing ) );
					assertEquals( 0, root.GetDegree( Direction.Incoming ) );
					assertEquals( 25, root.GetDegree( RelationshipType.withName( "Type0" ) ) );
					assertEquals( 25, root.GetDegree( RelationshipType.withName( "Type1" ) ) );
					assertEquals( 25, root.GetDegree( RelationshipType.withName( "Type2" ) ) );
					assertEquals( 25, root.GetDegree( RelationshipType.withName( "Type3" ) ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( 100, root.Degree );
					assertEquals( 100, root.GetDegree( Direction.Outgoing ) );
					assertEquals( 0, root.GetDegree( Direction.Incoming ) );
					assertEquals( 25, root.GetDegree( RelationshipType.withName( "Type0" ) ) );
					assertEquals( 25, root.GetDegree( RelationshipType.withName( "Type1" ) ) );
					assertEquals( 25, root.GetDegree( RelationshipType.withName( "Type2" ) ) );
					assertEquals( 25, root.GetDegree( RelationshipType.withName( "Type3" ) ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deletingRelationshipsFromDenseNodeIsConsistent()
		 public virtual void DeletingRelationshipsFromDenseNodeIsConsistent()
		 {
			  // GIVEN
			  GraphDatabaseService db = DatabaseRule.GraphDatabaseAPI;

			  Node root;
			  using ( Transaction tx = Db.beginTx() )
			  {
					root = Db.createNode();
					CreateRelationshipsOnNode( db, root, 100 );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					DeleteRelationshipsFromNode( root, 80 );

					assertEquals( 20, root.Degree );
					assertEquals( 20, root.GetDegree( Direction.Outgoing ) );
					assertEquals( 0, root.GetDegree( Direction.Incoming ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( 20, root.Degree );
					assertEquals( 20, root.GetDegree( Direction.Outgoing ) );
					assertEquals( 0, root.GetDegree( Direction.Incoming ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void movingBilaterallyOfTheDenseNodeThresholdIsConsistent()
		 public virtual void MovingBilaterallyOfTheDenseNodeThresholdIsConsistent()
		 {
			  // GIVEN
			  GraphDatabaseService db = DatabaseRule.GraphDatabaseAPI;

			  Node root;
			  // WHEN
			  using ( Transaction tx = Db.beginTx() )
			  {
					root = Db.createNode();
					CreateRelationshipsOnNode( db, root, 100 );
					DeleteRelationshipsFromNode( root, 80 );

					assertEquals( 20, root.Degree );
					assertEquals( 20, root.GetDegree( Direction.Outgoing ) );
					assertEquals( 0, root.GetDegree( Direction.Incoming ) );

					tx.Success();
			  }

			  // THEN
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( 20, root.Degree );
					assertEquals( 20, root.GetDegree( Direction.Outgoing ) );
					assertEquals( 0, root.GetDegree( Direction.Incoming ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBringingTwoConnectedNodesOverDenseThresholdIsConsistent()
		 public virtual void TestBringingTwoConnectedNodesOverDenseThresholdIsConsistent()
		 {
			  // GIVEN
			  GraphDatabaseService db = DatabaseRule.GraphDatabaseAPI;

			  Node source;
			  Node sink;
			  using ( Transaction tx = Db.beginTx() )
			  {
					source = Db.createNode();
					sink = Db.createNode();
					CreateRelationshipsBetweenNodes( source, sink, 40 );
					tx.Success();
			  }

			  // WHEN
			  using ( Transaction tx = Db.beginTx() )
			  {
					CreateRelationshipsBetweenNodes( source, sink, 60 );

					// THEN
					assertEquals( 100, source.Degree );
					assertEquals( 100, source.GetDegree( Direction.Outgoing ) );
					assertEquals( 0, source.GetDegree( Direction.Incoming ) );
					assertEquals( 25, source.GetDegree( RelationshipType.withName( "Type0" ) ) );
					assertEquals( 25, source.GetDegree( RelationshipType.withName( "Type1" ) ) );
					assertEquals( 25, source.GetDegree( RelationshipType.withName( "Type2" ) ) );
					assertEquals( 25, source.GetDegree( RelationshipType.withName( "Type3" ) ) );

					assertEquals( 100, sink.Degree );
					assertEquals( 0, sink.GetDegree( Direction.Outgoing ) );
					assertEquals( 100, sink.GetDegree( Direction.Incoming ) );
					assertEquals( 25, sink.GetDegree( RelationshipType.withName( "Type0" ) ) );
					assertEquals( 25, sink.GetDegree( RelationshipType.withName( "Type1" ) ) );
					assertEquals( 25, sink.GetDegree( RelationshipType.withName( "Type2" ) ) );
					assertEquals( 25, sink.GetDegree( RelationshipType.withName( "Type3" ) ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( 100, source.Degree );
					assertEquals( 100, source.GetDegree( Direction.Outgoing ) );
					assertEquals( 0, source.GetDegree( Direction.Incoming ) );
					assertEquals( 25, source.GetDegree( RelationshipType.withName( "Type0" ) ) );
					assertEquals( 25, source.GetDegree( RelationshipType.withName( "Type1" ) ) );
					assertEquals( 25, source.GetDegree( RelationshipType.withName( "Type2" ) ) );
					assertEquals( 25, source.GetDegree( RelationshipType.withName( "Type3" ) ) );

					assertEquals( 100, sink.Degree );
					assertEquals( 0, sink.GetDegree( Direction.Outgoing ) );
					assertEquals( 100, sink.GetDegree( Direction.Incoming ) );
					assertEquals( 25, sink.GetDegree( RelationshipType.withName( "Type0" ) ) );
					assertEquals( 25, sink.GetDegree( RelationshipType.withName( "Type1" ) ) );
					assertEquals( 25, sink.GetDegree( RelationshipType.withName( "Type2" ) ) );
					assertEquals( 25, sink.GetDegree( RelationshipType.withName( "Type3" ) ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCreateRelationshipsInEmptyDenseNode()
		 public virtual void ShouldBeAbleToCreateRelationshipsInEmptyDenseNode()
		 {
			  // GIVEN
			  Node node;
			  using ( Transaction tx = DatabaseRule.beginTx() )
			  {
					node = DatabaseRule.createNode();
					CreateRelationshipsBetweenNodes( node, DatabaseRule.createNode(), DenseNodeThreshold(DatabaseRule) + 1 );
					tx.Success();
			  }
			  using ( Transaction tx = DatabaseRule.beginTx() )
			  {
					node.Relationships.forEach( Relationship.delete );
					tx.Success();
			  }

			  // WHEN
			  Relationship rel;
			  using ( Transaction tx = DatabaseRule.beginTx() )
			  {
					rel = node.CreateRelationshipTo( DatabaseRule.createNode(), MyRelTypes.TEST );
					tx.Success();
			  }

			  using ( Transaction tx = DatabaseRule.beginTx() )
			  {
					// THEN
					assertEquals( rel, single( node.Relationships ) );
					tx.Success();
			  }
		 }

		 private int DenseNodeThreshold( GraphDatabaseAPI db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( Config ) ).get( GraphDatabaseSettings.dense_node_threshold );
		 }

		 private void DeleteRelationshipsFromNode( Node root, int numberOfRelationships )
		 {
			  int deleted = 0;
			  using ( ResourceIterator<Relationship> iterator = asResourceIterator( root.Relationships.GetEnumerator() ) )
			  {
					while ( iterator.MoveNext() )
					{
						 Relationship relationship = iterator.Current;
						 relationship.Delete();
						 deleted++;
						 if ( deleted == numberOfRelationships )
						 {
							  break;
						 }
					}
			  }
		 }

		 private void CreateRelationshipsOnNode( GraphDatabaseService db, Node root, int numberOfRelationships )
		 {
			  for ( int i = 0; i < numberOfRelationships; i++ )
			  {
					root.CreateRelationshipTo( Db.createNode(), RelationshipType.withName("Type" + (i % 4)) ).setProperty("" + i, i);

			  }
		 }

		 private void CreateRelationshipsBetweenNodes( Node source, Node sink, int numberOfRelationships )
		 {
			  for ( int i = 0; i < numberOfRelationships; i++ )
			  {
					source.CreateRelationshipTo( sink, RelationshipType.withName( "Type" + ( i % 4 ) ) ).setProperty( "" + i, i );

			  }
		 }
	}

}