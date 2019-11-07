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
namespace Neo4Net.Kernel.Api.Internal
{

	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.RelationshipType.withName;

	public class RelationshipTestSupport
	{
		 private RelationshipTestSupport()
		 {
		 }

		 internal static void SomeGraph( IGraphDatabaseService graphDb )
		 {
			  Relationship dead;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Node a = graphDb.CreateNode(), b = graphDb.CreateNode(), c = graphDb.CreateNode(), d = graphDb.CreateNode();

					a.CreateRelationshipTo( a, withName( "ALPHA" ) );
					a.CreateRelationshipTo( b, withName( "BETA" ) );
					a.CreateRelationshipTo( c, withName( "GAMMA" ) );
					a.CreateRelationshipTo( d, withName( "DELTA" ) );

					graphDb.CreateNode().createRelationshipTo(a, withName("BETA"));
					a.CreateRelationshipTo( graphDb.CreateNode(), withName("BETA") );
					dead = a.CreateRelationshipTo( graphDb.CreateNode(), withName("BETA") );
					a.CreateRelationshipTo( graphDb.CreateNode(), withName("BETA") );

					Node clump = graphDb.CreateNode();
					clump.CreateRelationshipTo( clump, withName( "REL" ) );
					clump.CreateRelationshipTo( clump, withName( "REL" ) );
					clump.CreateRelationshipTo( clump, withName( "REL" ) );
					clump.CreateRelationshipTo( graphDb.CreateNode(), withName("REL") );
					clump.CreateRelationshipTo( graphDb.CreateNode(), withName("REL") );
					clump.CreateRelationshipTo( graphDb.CreateNode(), withName("REL") );
					graphDb.CreateNode().createRelationshipTo(clump, withName("REL"));
					graphDb.CreateNode().createRelationshipTo(clump, withName("REL"));
					graphDb.CreateNode().createRelationshipTo(clump, withName("REL"));

					tx.Success();
			  }

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Node node = dead.EndNode;
					dead.Delete();
					node.Delete();

					tx.Success();
			  }
		 }

		 internal static StartNode Sparse( IGraphDatabaseService graphDb )
		 {
			  Node node;
			  IDictionary<string, IList<StartRelationship>> relationshipMap;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					node = graphDb.CreateNode();
					relationshipMap = BuildSparseDenseRels( node );
					tx.Success();
			  }
			  return new StartNode( node.Id, relationshipMap );
		 }

		 internal static StartNode Dense( IGraphDatabaseService graphDb )
		 {
			  Node node;
			  IDictionary<string, IList<StartRelationship>> relationshipMap;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					node = graphDb.CreateNode();
					relationshipMap = BuildSparseDenseRels( node );

					IList<StartRelationship> bulk = new List<StartRelationship>();
					RelationshipType bulkType = withName( "BULK" );

					for ( int i = 0; i < 200; i++ )
					{
						 Relationship r = node.CreateRelationshipTo( graphDb.CreateNode(), bulkType );
						 bulk.Add( new StartRelationship( r.Id, Direction.OUTGOING, bulkType ) );
					}

					string bulkKey = ComputeKey( "BULK", Direction.OUTGOING );
					relationshipMap[bulkKey] = bulk;

					tx.Success();
			  }
			  return new StartNode( node.Id, relationshipMap );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.util.Map<String,int> count(Neo4Net.Kernel.Api.Internal.Transaction transaction, RelationshipTraversalCursor relationship) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 internal static IDictionary<string, int> Count( Neo4Net.Kernel.Api.Internal.Transaction transaction, RelationshipTraversalCursor relationship )
		 {
			  Dictionary<string, int> counts = new Dictionary<string, int>();
			  while ( relationship.next() )
			  {
					string key = ComputeKey( transaction, relationship );
					Counts.compute( key, ( k, value ) => value == null ? 1 : value + 1 );
			  }
			  return counts;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void assertCount(Neo4Net.Kernel.Api.Internal.Transaction transaction, RelationshipTraversalCursor relationship, java.util.Map<String,int> expectedCounts, int expectedType, Neo4Net.graphdb.Direction direction) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 internal static void AssertCount( Neo4Net.Kernel.Api.Internal.Transaction transaction, RelationshipTraversalCursor relationship, IDictionary<string, int> expectedCounts, int expectedType, Direction direction )
		 {
			  string key = ComputeKey( transaction.Token().relationshipTypeName(expectedType), direction );
			  int expectedCount = expectedCounts.getOrDefault( key, 0 );
			  int count = 0;

			  while ( relationship.next() )
			  {
					assertEquals( "same type", expectedType, relationship.Type() );
					count++;
			  }

			  assertEquals( format( "expected number of relationships for key '%s'", key ), expectedCount, count );
		 }

		 internal class StartRelationship
		 {
			  public readonly long Id;
			  public readonly Direction Direction;
			  public readonly RelationshipType Type;

			  internal StartRelationship( long id, Direction direction, RelationshipType type )
			  {
					this.Id = id;
					this.Type = type;
					this.Direction = direction;
			  }
		 }

		 internal class StartNode
		 {
			  public readonly long Id;
			  public readonly IDictionary<string, IList<StartRelationship>> Relationships;

			  internal StartNode( long id, IDictionary<string, IList<StartRelationship>> relationships )
			  {
					this.Id = id;
					this.Relationships = relationships;
			  }

			  internal virtual IDictionary<string, int> ExpectedCounts()
			  {
					IDictionary<string, int> expectedCounts = new Dictionary<string, int>();
					foreach ( KeyValuePair<string, IList<StartRelationship>> kv in Relationships.SetOfKeyValuePairs() )
					{
						 expectedCounts[kv.Key] = Relationships[kv.Key].Count;
					}
					return expectedCounts;
			  }
		 }

		 internal static void AssertCounts( IDictionary<string, int> expectedCounts, IDictionary<string, int> counts )
		 {
			  foreach ( KeyValuePair<string, int> expected in expectedCounts.SetOfKeyValuePairs() )
			  {
					assertEquals( format( "counts for relationship key '%s' are equal", expected.Key ), expected.Value, counts[expected.Key] );
			  }
		 }

		 private static IDictionary<string, IList<StartRelationship>> BuildSparseDenseRels( Node node )
		 {
			  IDictionary<string, IList<StartRelationship>> relationshipMap = new Dictionary<string, IList<StartRelationship>>();
			  foreach ( System.Func<Node, StartRelationship> rel in _sparseDenseRels )
			  {
					StartRelationship r = rel( node );
					IList<StartRelationship> relsOfType = relationshipMap.computeIfAbsent( ComputeKey( r ), key => new List<StartRelationship>() );
					relsOfType.Add( r );
			  }
			  return relationshipMap;
		 }

		 private static string ComputeKey( StartRelationship r )
		 {
			  return ComputeKey( r.Type.name(), r.Direction );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String computeKey(Neo4Net.Kernel.Api.Internal.Transaction transaction, RelationshipTraversalCursor r) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private static string ComputeKey( Neo4Net.Kernel.Api.Internal.Transaction transaction, RelationshipTraversalCursor r )
		 {
			  Direction d;
			  if ( r.SourceNodeReference() == r.TargetNodeReference() )
			  {
					d = Direction.BOTH;
			  }
			  else if ( r.SourceNodeReference() == r.OriginNodeReference() )
			  {
					d = Direction.OUTGOING;
			  }
			  else
			  {
					d = Direction.INCOMING;
			  }

			  return ComputeKey( transaction.Token().relationshipTypeName(r.Type()), d );
		 }

		 internal static string ComputeKey( string type, Direction direction )
		 {
			  return type + "-" + direction;
		 }

		 private static System.Func<Node, StartRelationship>[] _sparseDenseRels = Iterators.array( Loop( "FOO" ), Outgoing( "FOO" ), Outgoing( "BAR" ), Outgoing( "BAR" ), Incoming( "FOO" ), Outgoing( "FOO" ), Incoming( "BAZ" ), Incoming( "BAR" ), Outgoing( "BAZ" ), Loop( "FOO" ) );

		 private static System.Func<Node, StartRelationship> Outgoing( string type )
		 {
			  return node =>
			  {
				GraphDatabaseService db = node.GraphDatabase;
				RelationshipType relType = withName( type );
				return new StartRelationship( node.createRelationshipTo( Db.createNode(), relType ).Id, Direction.OUTGOING, relType );
			  };
		 }

		 private static System.Func<Node, StartRelationship> Incoming( string type )
		 {
			  return node =>
			  {
				GraphDatabaseService db = node.GraphDatabase;
				RelationshipType relType = withName( type );
				return new StartRelationship( Db.createNode().createRelationshipTo(node, relType).Id, Direction.INCOMING, relType );
			  };
		 }

		 private static System.Func<Node, StartRelationship> Loop( string type )
		 {
			  return node =>
			  {
				RelationshipType relType = withName( type );
				return new StartRelationship( node.createRelationshipTo( node, relType ).Id, Direction.BOTH, relType );
			  };
		 }
	}

}