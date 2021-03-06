﻿using System.Collections.Generic;

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
namespace Org.Neo4j.@internal.Kernel.Api
{

	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public class RelationshipTestSupport
	{
		 private RelationshipTestSupport()
		 {
		 }

		 internal static void SomeGraph( GraphDatabaseService graphDb )
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

		 internal static StartNode Sparse( GraphDatabaseService graphDb )
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

		 internal static StartNode Dense( GraphDatabaseService graphDb )
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
//ORIGINAL LINE: static java.util.Map<String,int> count(org.neo4j.internal.kernel.api.Transaction transaction, RelationshipTraversalCursor relationship) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 internal static IDictionary<string, int> Count( Org.Neo4j.@internal.Kernel.Api.Transaction transaction, RelationshipTraversalCursor relationship )
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
//ORIGINAL LINE: static void assertCount(org.neo4j.internal.kernel.api.Transaction transaction, RelationshipTraversalCursor relationship, java.util.Map<String,int> expectedCounts, int expectedType, org.neo4j.graphdb.Direction direction) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 internal static void AssertCount( Org.Neo4j.@internal.Kernel.Api.Transaction transaction, RelationshipTraversalCursor relationship, IDictionary<string, int> expectedCounts, int expectedType, Direction direction )
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
//ORIGINAL LINE: private static String computeKey(org.neo4j.internal.kernel.api.Transaction transaction, RelationshipTraversalCursor r) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private static string ComputeKey( Org.Neo4j.@internal.Kernel.Api.Transaction transaction, RelationshipTraversalCursor r )
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