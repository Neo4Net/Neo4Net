using System;
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
namespace Org.Neo4j.@internal.Kernel.Api
{
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public abstract class RandomRelationshipTraversalCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
		 private const int N_TRAVERSALS = 10_000;
		 private static int _nNodes = 100;
		 private static int _nRelationships = 1000;
		 private static long _seed = ( new Random() ).Next();
		 private static Random _random = new Random( _seed );
		 private static IList<long> _nodeIds = new List<long>();

		 public override void CreateTestGraph( GraphDatabaseService graphDb )
		 {
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					for ( int i = 0; i < _nNodes; i++ )
					{
						 _nodeIds.Add( graphDb.CreateNode( Label.label( "LABEL" + i ) ).Id );
					}
					tx.Success();
			  }

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					for ( int i = 0; i < _nRelationships; i++ )
					{
						 long? source = _nodeIds[_random.Next( _nNodes )];
						 long? target = _nodeIds[_random.Next( _nNodes )];
						 graphDb.GetNodeById( source.Value ).createRelationshipTo( graphDb.GetNodeById( target.Value ), RelationshipType.withName( "REL" + ( i % 10 ) ) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldManageRandomTraversals()
		 public virtual void ShouldManageRandomTraversals()
		 {
			  // given
			  try
			  {
					  using ( NodeCursor node = cursors.allocateNodeCursor(), RelationshipGroupCursor group = cursors.allocateRelationshipGroupCursor(), RelationshipTraversalCursor relationship = cursors.allocateRelationshipTraversalCursor() )
					  {
						for ( int i = 0; i < N_TRAVERSALS; i++ )
						{
							 // when
							 long nodeId = _nodeIds[_random.Next( _nNodes )];
							 read.singleNode( nodeId, node );
							 assertTrue( "access root node", node.Next() );
							 node.Relationships( group );
							 assertFalse( "single root", node.Next() );
      
							 // then
							 while ( group.next() )
							 {
								  group.Incoming( relationship );
								  while ( relationship.next() )
								  {
										assertEquals( "incoming origin", nodeId, relationship.OriginNodeReference() );
										relationship.Neighbour( node );
								  }
								  group.Outgoing( relationship );
								  while ( relationship.next() )
								  {
										assertEquals( "outgoing origin", nodeId, relationship.OriginNodeReference() );
										relationship.Neighbour( node );
								  }
								  group.Loops( relationship );
								  while ( relationship.next() )
								  {
										assertEquals( "loop origin", nodeId, relationship.OriginNodeReference() );
										relationship.Neighbour( node );
								  }
							 }
						}
					  }
			  }
			  catch ( Exception t )
			  {
					throw new Exception( "Failed with random seed " + _seed, t );
			  }
		 }
	}

}