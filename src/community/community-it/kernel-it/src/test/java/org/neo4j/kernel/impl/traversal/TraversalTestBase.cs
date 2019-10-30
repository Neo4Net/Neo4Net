using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.impl.traversal
{

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Traverser = Neo4Net.GraphDb.Traversal.Traverser;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using GraphDefinition = Neo4Net.Test.GraphDefinition;
	using GraphDescription = Neo4Net.Test.GraphDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public abstract class TraversalTestBase : AbstractNeo4NetTestCase
	{
		 private IDictionary<string, Node> _nodes;

		 protected internal override bool RestartGraphDbBetweenTests()
		 {
			  return true;
		 }

		 protected internal virtual Node Node( string name )
		 {
			  return _nodes[name];
		 }

		 protected internal virtual Node GetNode( long id )
		 {
			  return GraphDb.getNodeById( id );
		 }

		 protected internal virtual Transaction BeginTx()
		 {
			  return GraphDb.beginTx();
		 }

		 protected internal virtual void CreateGraph( params string[] description )
		 {
			  _nodes = CreateGraph( GraphDescription.create( description ) );
		 }

		 private IDictionary<string, Node> CreateGraph( GraphDefinition graph )
		 {
			  using ( Transaction tx = BeginTx() )
			  {
					IDictionary<string, Node> result = graph.Create( GraphDb );
					tx.Success();
					return result;
			  }
		 }

		 protected internal virtual Node GetNodeWithName( string name )
		 {
			  ResourceIterable<Node> allNodes = GraphDb.AllNodes;
			  using ( ResourceIterator<Node> nodeIterator = allNodes.GetEnumerator() )
			  {
					while ( nodeIterator.MoveNext() )
					{
						 Node node = nodeIterator.Current;
						 {
							  string nodeName = ( string ) node.GetProperty( "name", null );
							  if ( !string.ReferenceEquals( nodeName, null ) && nodeName.Equals( name ) )
							  {
									return node;
							  }
						 }
					}
			  }

			  return null;
		 }

		 protected internal virtual void AssertLevels( Traverser traverser, Stack<ISet<string>> levels )
		 {
			  ISet<string> current = levels.Pop();

			  foreach ( Path position in traverser )
			  {
					string nodeName = ( string ) position.EndNode().getProperty("name");
					if ( current.Count == 0 )
					{
						 current = levels.Pop();
					}
					assertTrue( "Should not contain node (" + nodeName + ") at level " + ( 3 - levels.Count ), current.remove( nodeName ) );
			  }

			  assertTrue( "Should have no more levels", levels.Count == 0 );
			  assertTrue( "Should be empty", current.Count == 0 );
		 }

		 protected internal static readonly Representation<PropertyContainer> NamePropertyRepresentation = new PropertyRepresentation( "name" );

		 protected internal static readonly Representation<Relationship> RelationshipTypeRepresentation = item => item.Type.name();

		 protected internal interface Representation<T>
		 {
			  string Represent( T item );
		 }

		 protected internal sealed class PropertyRepresentation : Representation<PropertyContainer>
		 {
			  public PropertyRepresentation( string key )
			  {
					this.Key = key;
			  }

			  internal readonly string Key;

			  public override string Represent( IPropertyContainer item )
			  {
					return ( string ) item.GetProperty( Key );
			  }
		 }

		 protected internal sealed class RelationshipRepresentation : Representation<Relationship>
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final Representation<? super org.Neo4Net.graphdb.Node> nodes;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly Representation<object> Nodes;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final Representation<? super org.Neo4Net.graphdb.Relationship> rel;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly Representation<object> Rel;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public RelationshipRepresentation(Representation<? super org.Neo4Net.graphdb.Node> nodes)
			  public RelationshipRepresentation<T1>( Representation<T1> nodes ) : this( nodes, RelationshipTypeRepresentation )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public RelationshipRepresentation(Representation<? super org.Neo4Net.graphdb.Node> nodes, Representation<? super org.Neo4Net.graphdb.Relationship> rel)
			  public RelationshipRepresentation<T1, T2>( Representation<T1> nodes, Representation<T2> rel )
			  {
					this.Nodes = nodes;
					this.Rel = rel;
			  }

			  public override string Represent( Relationship item )
			  {
					return Nodes.represent( item.StartNode ) + " "
							 + Rel.represent( item ) + " "
							 + Nodes.represent( item.EndNode );
			  }
		 }

		 protected internal sealed class NodePathRepresentation : Representation<Path>
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final Representation<? super org.Neo4Net.graphdb.Node> nodes;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly Representation<object> Nodes;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public NodePathRepresentation(Representation<? super org.Neo4Net.graphdb.Node> nodes)
			  public NodePathRepresentation<T1>( Representation<T1> nodes )
			  {
					this.Nodes = nodes;

			  }

			  public override string Represent( Path item )
			  {
					StringBuilder builder = new StringBuilder();
					foreach ( Node node in item.Nodes() )
					{
						 builder.Append( builder.Length > 0 ? "," : "" );
						 builder.Append( Nodes.represent( node ) );
					}
					return builder.ToString();
			  }
		 }

		 protected internal virtual void Expect<T, T1>( IEnumerable<T1> items, Representation<T> representation, params string[] expected ) where T1 : T
		 {
			  Expect( items, representation, new HashSet<string>( Arrays.asList( expected ) ) );
		 }

		 protected internal virtual void Expect<T, T1>( IEnumerable<T1> items, Representation<T> representation, ISet<string> expected ) where T1 : T
		 {
			  ICollection<string> encounteredItems = new List<string>();
			  using ( Transaction tx = BeginTx() )
			  {
					foreach ( T item in items )
					{
						 string repr = representation.Represent( item );
						 assertTrue( repr + " not expected ", expected.remove( repr ) );
						 encounteredItems.Add( repr );
					}
					tx.Success();
			  }

			  if ( expected.Count > 0 )
			  {
					fail( "The expected elements " + expected + " were not returned. Returned were: " + encounteredItems );
			  }
		 }

		 protected internal virtual void ExpectNodes( Traverser traverser, params string[] nodes )
		 {
			  Expect( traverser.Nodes(), NamePropertyRepresentation, nodes );
		 }

		 protected internal virtual void ExpectRelationships( Traverser traverser, params string[] relationships )
		 {
			  Expect( traverser.Relationships(), new RelationshipRepresentation(NamePropertyRepresentation), relationships );
		 }

		 protected internal virtual void ExpectPaths( Traverser traverser, params string[] paths )
		 {
			  ExpectPaths( traverser, new HashSet<string>( Arrays.asList( paths ) ) );
		 }

		 protected internal virtual void ExpectPaths( Traverser traverser, ISet<string> expected )
		 {
			  Expect( traverser, new NodePathRepresentation( NamePropertyRepresentation ), expected );
		 }

		 public static void AssertContains<E>( IEnumerator<E> actual, params E[] expected )
		 {
			  AssertContains( Iterators.asIterable( actual ), expected );
		 }

		 public static void AssertContains<E>( IEnumerable<E> actual, params E[] expected )
		 {
			  ISet<E> expectation = new HashSet<E>( Arrays.asList( expected ) );
			  foreach ( E element in actual )
			  {
					if ( !expectation.remove( element ) )
					{
						 fail( "unexpected element <" + element + ">" );
					}
			  }
			  if ( expectation.Count > 0 )
			  {
					fail( "the expected elements <" + expectation + "> were not contained" );
			  }
		 }

		 public static void AssertContainsInOrder<T>( ICollection<T> collection, params T[] expectedItems )
		 {
			  string collectionString = Join( ", ", collection.ToArray() );
			  assertEquals( collectionString, expectedItems.Length, collection.Count );
			  IEnumerator<T> itr = collection.GetEnumerator();
			  for ( int i = 0; itr.MoveNext(); i++ )
			  {
					assertEquals( expectedItems[i], itr.Current );
			  }
		 }

		 public static void AssertContainsInOrder<T>( IEnumerable<T> collection, params T[] expectedItems )
		 {
			  AssertContainsInOrder( Iterables.asCollection( collection ), expectedItems );
		 }

		 public static string Join<T>( string delimiter, params T[] items )
		 {
			  StringBuilder buffer = new StringBuilder();
			  foreach ( T item in items )
			  {
					if ( buffer.Length > 0 )
					{
						 buffer.Append( delimiter );
					}
					buffer.Append( item.ToString() );
			  }
			  return buffer.ToString();
		 }
	}

}