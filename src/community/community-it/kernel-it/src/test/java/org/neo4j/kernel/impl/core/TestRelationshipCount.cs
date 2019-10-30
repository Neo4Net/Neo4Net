using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.core
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4Net.Collections.Helpers;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TestRelationshipCount
	public class TestRelationshipCount
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "denseNodeThreshold={0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  ICollection<object[]> data = new List<object[]>();
			  int max = parseInt( GraphDatabaseSettings.dense_node_threshold.DefaultValue );
			  for ( int i = 1; i < max; i++ )
			  {
					data.Add( new object[] { i } );
			  }
			  return data;
		 }

		 private static GraphDatabaseAPI _db;
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void shutdownDb()
		 public static void ShutdownDb()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public TestRelationshipCount(final int denseNodeThreshold)
		 public TestRelationshipCount( int denseNodeThreshold )
		 {
			  // This code below basically turns "db" into a ClassRule, but per dense node threshold
			  if ( _db == null || _db.DependencyResolver.resolveDependency( typeof( Config ) ).get( GraphDatabaseSettings.dense_node_threshold ) != denseNodeThreshold )
			  {
					if ( _db != null )
					{
						 _db.shutdown();
					}
					_db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.dense_node_threshold, denseNodeThreshold.ToString()).newGraphDatabase();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void convertNodeToDense()
		 public virtual void ConvertNodeToDense()
		 {
			  Node node = GraphDb.createNode();
			  Dictionary<MyRelTypes, ISet<Relationship>> rels = new Dictionary<MyRelTypes, ISet<Relationship>>( typeof( MyRelTypes ) );
			  foreach ( MyRelTypes type in Enum.GetValues( typeof( MyRelTypes ) ) )
			  {
					rels[type] = new HashSet<Relationship>();
			  }
			  int expectedRelCount = 0;
			  for ( int i = 0; i < 6; i++, expectedRelCount++ )
			  {
					MyRelTypes type = Enum.GetValues( typeof( MyRelTypes ) )[i % Enum.GetValues( typeof( MyRelTypes ) ).length];
					Relationship rel = node.CreateRelationshipTo( GraphDb.createNode(), type );
					rels[type].Add( rel );
			  }
			  NewTransaction();
			  for ( int i = 0; i < 1000; i++, expectedRelCount++ )
			  {
					node.CreateRelationshipTo( GraphDb.createNode(), MyRelTypes.TEST );
			  }

			  assertEquals( expectedRelCount, node.Degree );
			  assertEquals( expectedRelCount, node.GetDegree( Direction.BOTH ) );
			  assertEquals( expectedRelCount, node.GetDegree( Direction.OUTGOING ) );
			  assertEquals( 0, node.GetDegree( Direction.INCOMING ) );
			  assertEquals( rels[MyRelTypes.TEST2], Iterables.asSet( node.GetRelationships( MyRelTypes.TEST2 ) ) );
			  assertEquals( Join( rels[MyRelTypes.TEST_TRAVERSAL], rels[MyRelTypes.TEST2] ), Iterables.asSet( node.GetRelationships( MyRelTypes.TEST_TRAVERSAL, MyRelTypes.TEST2 ) ) );
		 }

		 private ISet<T> Join<T>( ISet<T> set, ISet<T> set2 )
		 {
			  ISet<T> result = new HashSet<T>();
			  result.addAll( set );
			  result.addAll( set2 );
			  return result;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetRelationshipTypesOnDiscreteNode()
		 public virtual void TestGetRelationshipTypesOnDiscreteNode()
		 {
			  TestGetRelationshipTypes( GraphDb.createNode(), new HashSet<string>() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetRelationshipTypesOnDenseNode()
		 public virtual void TestGetRelationshipTypesOnDenseNode()
		 {
			  Node node = GraphDb.createNode();
			  Node otherNode = GraphDb.createNode();
			  for ( int i = 0; i < 300; i++ )
			  {
					node.CreateRelationshipTo( otherNode, RelType.Initial );
			  }
			  TestGetRelationshipTypes( node, new HashSet<string>( asList( RelType.Initial.name() ) ) );
		 }

		 private void TestGetRelationshipTypes( Node node, ISet<string> expectedTypes )
		 {
			  AssertExpectedRelationshipTypes( expectedTypes, node, false );
			  node.CreateRelationshipTo( GraphDb.createNode(), RelType.Type1 );
			  expectedTypes.Add( RelType.Type1.name() );
			  AssertExpectedRelationshipTypes( expectedTypes, node, false );
			  node.CreateRelationshipTo( GraphDb.createNode(), RelType.Type1 );
			  AssertExpectedRelationshipTypes( expectedTypes, node, true );

			  Relationship rel = node.CreateRelationshipTo( GraphDb.createNode(), RelType.Type2 );
			  expectedTypes.Add( RelType.Type2.name() );
			  AssertExpectedRelationshipTypes( expectedTypes, node, false );
			  rel.Delete();
			  expectedTypes.remove( RelType.Type2.name() );
			  AssertExpectedRelationshipTypes( expectedTypes, node, true );

			  node.CreateRelationshipTo( GraphDb.createNode(), RelType.Type2 );
			  node.CreateRelationshipTo( GraphDb.createNode(), RelType.Type2 );
			  expectedTypes.Add( RelType.Type2.name() );
			  node.CreateRelationshipTo( GraphDb.createNode(), MyRelTypes.TEST );
			  expectedTypes.Add( MyRelTypes.TEST.name() );
			  AssertExpectedRelationshipTypes( expectedTypes, node, true );

			  foreach ( Relationship r in node.GetRelationships( RelType.Type1 ) )
			  {
					AssertExpectedRelationshipTypes( expectedTypes, node, false );
					r.Delete();
			  }
			  expectedTypes.remove( RelType.Type1.name() );
			  AssertExpectedRelationshipTypes( expectedTypes, node, true );
		 }

		 private void AssertExpectedRelationshipTypes( ISet<string> expectedTypes, Node node, bool commit )
		 {
			  ISet<string> actual = Iterables.asSet( AsStrings( node.RelationshipTypes ) );
			  assertEquals( expectedTypes, actual );
			  if ( commit )
			  {
					NewTransaction();
			  }
			  assertEquals( expectedTypes, Iterables.asSet( AsStrings( node.RelationshipTypes ) ) );
		 }

		 private IEnumerable<string> AsStrings( IEnumerable<RelationshipType> relationshipTypes )
		 {
			  return new IterableWrapperAnonymousInnerClass( this, relationshipTypes );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<string, RelationshipType>
		 {
			 private readonly TestRelationshipCount _outerInstance;

			 public IterableWrapperAnonymousInnerClass( TestRelationshipCount outerInstance, IEnumerable<RelationshipType> relationshipTypes ) : base( relationshipTypes )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override string underlyingObjectToObject( RelationshipType @object )
			 {
				  return @object.Name();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void withoutLoops()
		 public virtual void WithoutLoops()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  assertEquals( 0, node1.Degree );
			  assertEquals( 0, node2.Degree );
			  node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  assertEquals( 1, node1.Degree );
			  assertEquals( 1, node2.Degree );
			  node1.CreateRelationshipTo( GraphDb.createNode(), MyRelTypes.TEST2 );
			  assertEquals( 2, node1.Degree );
			  assertEquals( 1, node2.Degree );
			  NewTransaction();
			  assertEquals( 2, node1.Degree );
			  assertEquals( 1, node2.Degree );

			  for ( int i = 0; i < 1000; i++ )
			  {
					if ( i % 2 == 0 )
					{
						 node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
					}
					else
					{
						 node2.CreateRelationshipTo( node1, MyRelTypes.TEST );
					}
					assertEquals( i + 2 + 1, node1.Degree );
					assertEquals( i + 1 + 1, node2.Degree );
					if ( i % 10 == 0 )
					{
						 NewTransaction();
					}
			  }

			  for ( int i = 0; i < 2; i++ )
			  {
					assertEquals( 1002, node1.Degree );
					assertEquals( 1002, node1.GetDegree( Direction.BOTH ) );
					assertEquals( 502, node1.GetDegree( Direction.OUTGOING ) );
					assertEquals( 500, node1.GetDegree( Direction.INCOMING ) );
					assertEquals( 1, node1.GetDegree( MyRelTypes.TEST2 ) );
					assertEquals( 1001, node1.GetDegree( MyRelTypes.TEST ) );

					assertEquals( 1001, node1.GetDegree( MyRelTypes.TEST, Direction.BOTH ) );
					assertEquals( 501, node1.GetDegree( MyRelTypes.TEST, Direction.OUTGOING ) );
					assertEquals( 500, node1.GetDegree( MyRelTypes.TEST, Direction.INCOMING ) );
					assertEquals( 1, node1.GetDegree( MyRelTypes.TEST2, Direction.OUTGOING ) );
					assertEquals( 0, node1.GetDegree( MyRelTypes.TEST2, Direction.INCOMING ) );
					NewTransaction();
			  }

			  foreach ( Relationship rel in node1.Relationships )
			  {
					rel.Delete();
			  }
			  node1.Delete();
			  foreach ( Relationship rel in node2.Relationships )
			  {
					rel.Delete();
			  }
			  node2.Delete();
			  NewTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void withLoops()
		 public virtual void WithLoops()
		 {
			  // Just to make sure it doesn't work by accident what with ids aligning with count
			  for ( int i = 0; i < 10; i++ )
			  {
					GraphDb.createNode().createRelationshipTo(GraphDb.createNode(), MyRelTypes.TEST);
			  }

			  Node node = GraphDb.createNode();
			  assertEquals( 0, node.Degree );
			  node.CreateRelationshipTo( node, MyRelTypes.TEST );
			  assertEquals( 1, node.Degree );
			  Node otherNode = GraphDb.createNode();
			  Relationship rel2 = node.CreateRelationshipTo( otherNode, MyRelTypes.TEST2 );
			  assertEquals( 2, node.Degree );
			  assertEquals( 1, otherNode.Degree );
			  NewTransaction();
			  assertEquals( 2, node.Degree );
			  Relationship rel3 = node.CreateRelationshipTo( node, MyRelTypes.TEST_TRAVERSAL );
			  assertEquals( 3, node.Degree );
			  assertEquals( 1, otherNode.Degree );
			  rel2.Delete();
			  assertEquals( 2, node.Degree );
			  assertEquals( 0, otherNode.Degree );
			  rel3.Delete();
			  assertEquals( 1, node.Degree );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ensureRightDegree()
		 public virtual void EnsureRightDegree()
		 {
			  foreach ( int initialSize in new int[] { 0, 95, 200 } )
			  {
					EnsureRightDegree( initialSize, asList( Create( RelType.Type1, Direction.OUTGOING, 5 ), Create( RelType.Type1, Direction.INCOMING, 2 ), Create( RelType.Type2, Direction.OUTGOING, 6 ), Create( RelType.Type2, Direction.INCOMING, 7 ), Create( RelType.Type2, Direction.BOTH, 3 ) ), asList( Delete( RelType.Type1, Direction.OUTGOING, 0 ), Delete( RelType.Type1, Direction.INCOMING, 1 ), Delete( RelType.Type2, Direction.OUTGOING, int.MaxValue ), Delete( RelType.Type2, Direction.INCOMING, 1 ), Delete( RelType.Type2, Direction.BOTH, int.MaxValue ) ) );

					EnsureRightDegree( initialSize, asList( Create( RelType.Type1, Direction.BOTH, 1 ), Create( RelType.Type1, Direction.OUTGOING, 5 ), Create( RelType.Type2, Direction.OUTGOING, 6 ), Create( RelType.Type1, Direction.INCOMING, 2 ), Create( RelType.Type2, Direction.BOTH, 3 ), Create( RelType.Type2, Direction.INCOMING, 7 ), Create( RelType.Type2, Direction.BOTH, 3 ) ), null );

					EnsureRightDegree( initialSize, asList( Create( RelType.Type1, Direction.BOTH, 2 ), Create( RelType.Type2, Direction.BOTH, 1 ), Create( RelType.Type1, Direction.OUTGOING, 1 ), Create( RelType.Type2, Direction.OUTGOING, 10 ), Create( RelType.Type1, Direction.INCOMING, 2 ), Create( RelType.Type2, Direction.BOTH, 2 ), Create( RelType.Type2, Direction.INCOMING, 7 ) ), null );
			  }
		 }

		 private void EnsureRightDegree( int initialSize, ICollection<RelationshipCreationSpec> cspecs, ICollection<RelationshipDeletionSpec> dspecs )
		 {
			  IDictionary<RelType, int[]> expectedCounts = new Dictionary<RelType, int[]>( typeof( RelType ) );
			  foreach ( RelType type in RelType.values() )
			  {
					expectedCounts[type] = new int[3];
			  }
			  Node me = GraphDb.createNode();
			  for ( int i = 0; i < initialSize; i++ )
			  {
					me.CreateRelationshipTo( GraphDb.createNode(), RelType.Initial );
			  }
			  NewTransaction();
			  expectedCounts[RelType.Initial][0] = initialSize;

			  AssertCounts( me, expectedCounts );
			  int counter = 0;
			  foreach ( RelationshipCreationSpec spec in cspecs )
			  {
					for ( int i = 0; i < spec.Count; i++ )
					{
						 Node otherNode = null;
						 if ( spec.Dir == Direction.OUTGOING )
						 {
							  otherNode = GraphDb.createNode();
							  me.CreateRelationshipTo( otherNode, spec.Type );
						 }
						 else if ( spec.Dir == Direction.INCOMING )
						 {
							  otherNode = GraphDb.createNode();
							  otherNode.CreateRelationshipTo( me, spec.Type );
						 }
						 else
						 {
							  me.CreateRelationshipTo( me, spec.Type );
						 }
						 expectedCounts[spec.Type][spec.Dir.ordinal()]++;

						 if ( otherNode != null )
						 {
							  assertEquals( 1, otherNode.Degree );
						 }
						 AssertCounts( me, expectedCounts );
						 if ( counter % 3 == 0 && counter > 0 )
						 {
							  NewTransaction();
							  AssertCounts( me, expectedCounts );
						 }
					}
			  }

			  AssertCounts( me, expectedCounts );
			  NewTransaction();
			  AssertCounts( me, expectedCounts );

			  // Delete one of each type/direction combination
			  counter = 0;
			  if ( dspecs == null )
			  {
					foreach ( RelType type in RelType.values() )
					{
						 if ( !type.measure )
						 {
							  continue;
						 }
						 foreach ( Direction direction in Direction.values() )
						 {
							  int[] counts = expectedCounts[type];
							  if ( counts[direction.ordinal()] > 0 )
							  {
									DeleteOneRelationship( me, type, direction, 0 );
									counts[direction.ordinal()]--;
									AssertCounts( me, expectedCounts );
									if ( counter % 3 == 0 && counter > 0 )
									{
										 NewTransaction();
										 AssertCounts( me, expectedCounts );
									}
							  }
						 }
					}
			  }
			  else
			  {
					foreach ( RelationshipDeletionSpec spec in dspecs )
					{
						 DeleteOneRelationship( me, spec.Type, spec.Dir, spec.Which );
						 expectedCounts[spec.Type][spec.Dir.ordinal()]--;
						 AssertCounts( me, expectedCounts );
						 if ( counter % 3 == 0 && counter > 0 )
						 {
							  NewTransaction();
							  AssertCounts( me, expectedCounts );
						 }
					}
			  }

			  AssertCounts( me, expectedCounts );
			  NewTransaction();
			  AssertCounts( me, expectedCounts );

			  // Clean up
			  foreach ( Relationship rel in me.Relationships )
			  {
					Node otherNode = rel.GetOtherNode( me );
					if ( !otherNode.Equals( me ) )
					{
						 otherNode.Delete();
					}
					rel.Delete();
			  }
			  me.Delete();
		 }

		 private void AssertCounts( Node me, IDictionary<RelType, int[]> expectedCounts )
		 {
			  assertEquals( TotalCount( expectedCounts, Direction.BOTH ), me.Degree );
			  assertEquals( TotalCount( expectedCounts, Direction.BOTH ), me.GetDegree( Direction.BOTH ) );
			  assertEquals( TotalCount( expectedCounts, Direction.OUTGOING ), me.GetDegree( Direction.OUTGOING ) );
			  assertEquals( TotalCount( expectedCounts, Direction.INCOMING ), me.GetDegree( Direction.INCOMING ) );
			  foreach ( KeyValuePair<RelType, int[]> entry in expectedCounts.SetOfKeyValuePairs() )
			  {
					RelType type = entry.Key;
					assertEquals( TotalCount( entry.Value, Direction.BOTH ), me.GetDegree( type ) );
					assertEquals( TotalCount( entry.Value, Direction.OUTGOING ), me.GetDegree( type, Direction.OUTGOING ) );
					assertEquals( TotalCount( entry.Value, Direction.INCOMING ), me.GetDegree( type, Direction.INCOMING ) );
					assertEquals( TotalCount( entry.Value, Direction.BOTH ), me.GetDegree( type, Direction.BOTH ) );
			  }
		 }

		 private int TotalCount( IDictionary<RelType, int[]> expectedCounts, Direction direction )
		 {
			  int result = 0;
			  foreach ( KeyValuePair<RelType, int[]> entry in expectedCounts.SetOfKeyValuePairs() )
			  {
					result += TotalCount( entry.Value, direction );
			  }
			  return result;
		 }

		 private int TotalCount( int[] expectedCounts, Direction direction )
		 {
			  int result = 0;
			  if ( direction == Direction.OUTGOING || direction == Direction.BOTH )
			  {
					result += expectedCounts[0];
			  }
			  if ( direction == Direction.INCOMING || direction == Direction.BOTH )
			  {
					result += expectedCounts[1];
			  }
			  result += expectedCounts[2];
			  return result;
		 }

		 private void DeleteOneRelationship( Node node, RelType type, Direction direction, int which )
		 {
			  Relationship last = null;
			  int counter = 0;
			  IEnumerable<Relationship> relationships = node.GetRelationships( type, direction );
			  using ( ResourceIterator<Relationship> relationshipIterator = ( ResourceIterator ) relationships.GetEnumerator() )
			  {
					while ( relationshipIterator.MoveNext() )
					{
						 Relationship rel = relationshipIterator.Current;
						 if ( IsLoop( rel ) == ( direction == Direction.BOTH ) )
						 {
							  last = rel;
							  if ( counter++ == which )
							  {
									rel.Delete();
									return;
							  }
						 }
					}
			  }
			  if ( which == int.MaxValue && last != null )
			  {
					last.Delete();
					return;
			  }

			  fail( "Couldn't find " + ( direction == Direction.BOTH ? "loop" : "non-loop" ) + " relationship " + type.name() + " " + direction + " to delete" );
		 }

		 private bool IsLoop( Relationship r )
		 {
			  return r.StartNode.Equals( r.EndNode );
		 }

		 private class RelationshipCreationSpec
		 {
			  internal readonly RelType Type;
			  internal readonly Direction Dir;
			  internal readonly int Count;

			  internal RelationshipCreationSpec( RelType type, Direction dir, int count )
			  {
					this.Type = type;
					this.Dir = dir;
					this.Count = count;
			  }
		 }

		 private class RelationshipDeletionSpec
		 {
			  internal readonly RelType Type;
			  internal readonly Direction Dir;
			  internal readonly int Which;

			  internal RelationshipDeletionSpec( RelType type, Direction dir, int which )
			  {
					this.Type = type;
					this.Dir = dir;
					this.Which = which;
			  }
		 }

		 internal static RelationshipCreationSpec Create( RelType type, Direction dir, int count )
		 {
			  return new RelationshipCreationSpec( type, dir, count );
		 }

		 internal static RelationshipDeletionSpec Delete( RelType type, Direction dir, int which )
		 {
			  return new RelationshipDeletionSpec( type, dir, which );
		 }

		 private sealed class RelType : RelationshipType
		 {
			  public static readonly RelType Initial = new RelType( "Initial", InnerEnum.Initial, false );
			  public static readonly RelType Type1 = new RelType( "Type1", InnerEnum.Type1, true );
			  public static readonly RelType Type2 = new RelType( "Type2", InnerEnum.Type2, true );

			  private static readonly IList<RelType> valueList = new List<RelType>();

			  static RelType()
			  {
				  valueList.Add( Initial );
				  valueList.Add( Type1 );
				  valueList.Add( Type2 );
			  }

			  public enum InnerEnum
			  {
				  Initial,
				  Type1,
				  Type2
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Boolean measure;

			  internal RelType( string name, InnerEnum innerEnum, bool measure )
			  {
					this.Measure = measure;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<RelType> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static RelType ValueOf( string name )
			 {
				 foreach ( RelType enumInstance in RelType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void newTransaction()
		 public virtual void NewTransaction()
		 {
			  if ( _tx != null )
			  {
					CloseTransaction();
			  }
			  _tx = GraphDb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void closeTransaction()
		 public virtual void CloseTransaction()
		 {
			  Debug.Assert( _tx != null );
			  _tx.success();
			  _tx.close();
		 }

		 private IGraphDatabaseService GraphDb
		 {
			 get
			 {
				  return _db;
			 }
		 }
	}

}