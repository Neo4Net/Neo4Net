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
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;

	public abstract class RelationshipScanCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
		 private static IList<long> _relationshipIds;
		 private static long _none, _loop, _one, _c, _d;

		 public override void CreateTestGraph( IGraphDatabaseService graphDb )
		 {
			  Relationship deleted;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Node a = graphDb.CreateNode(), b = graphDb.CreateNode(), c = graphDb.CreateNode(), d = graphDb.CreateNode(), e = graphDb.CreateNode(), f = graphDb.CreateNode();

					a.CreateRelationshipTo( b, withName( "CIRCLE" ) );
					b.CreateRelationshipTo( c, withName( "CIRCLE" ) );
					_one = c.CreateRelationshipTo( d, withName( "CIRCLE" ) ).Id;
					d.CreateRelationshipTo( e, withName( "CIRCLE" ) );
					e.CreateRelationshipTo( f, withName( "CIRCLE" ) );
					f.CreateRelationshipTo( a, withName( "CIRCLE" ) );

					a.CreateRelationshipTo( b, withName( "TRIANGLE" ) );
					a.CreateRelationshipTo( c, withName( "TRIANGLE" ) );
					b.CreateRelationshipTo( c, withName( "TRIANGLE" ) );
					_none = ( deleted = c.CreateRelationshipTo( b, withName( "TRIANGLE" ) ) ).Id;
					RelationshipScanCursorTestBase._c = c.Id;
					RelationshipScanCursorTestBase._d = d.Id;

					d.CreateRelationshipTo( e, withName( "TRIANGLE" ) );
					e.CreateRelationshipTo( f, withName( "TRIANGLE" ) );
					f.CreateRelationshipTo( d, withName( "TRIANGLE" ) );

					_loop = a.CreateRelationshipTo( a, withName( "LOOP" ) ).Id;

					tx.Success();
			  }

			  _relationshipIds = new List<long>();
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					deleted.Delete();
					foreach ( Relationship relationship in graphDb.AllRelationships )
					{
						 _relationshipIds.Add( relationship.Id );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanRelationships()
		 public virtual void ShouldScanRelationships()
		 {
			  // given
			  IList<long> ids = new List<long>();
			  using ( RelationshipScanCursor relationships = cursors.allocateRelationshipScanCursor() )
			  {
					// when
					read.allRelationshipsScan( relationships );
					while ( relationships.Next() )
					{
						 ids.Add( relationships.RelationshipReference() );
					}
			  }

			  assertEquals( _relationshipIds, ids );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessRelationshipByReference()
		 public virtual void ShouldAccessRelationshipByReference()
		 {
			  // given
			  using ( RelationshipScanCursor relationships = cursors.allocateRelationshipScanCursor() )
			  {
					foreach ( long id in _relationshipIds )
					{
						 // when
						 read.singleRelationship( id, relationships );

						 // then
						 assertTrue( "should access defined relationship", relationships.Next() );
						 assertEquals( "should access the correct relationship", id, relationships.RelationshipReference() );
						 assertFalse( "should only access a single relationship", relationships.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAccessDeletedRelationship()
		 public virtual void ShouldNotAccessDeletedRelationship()
		 {
			  // given
			  using ( RelationshipScanCursor relationships = cursors.allocateRelationshipScanCursor() )
			  {
					// when
					read.singleRelationship( _none, relationships );

					// then
					assertFalse( "should not access deleted relationship", relationships.Next() );
			  }
		 }

		 // This is functionality which is only required for the hacky db.schema not to leak real data
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAccessNegativeReferences()
		 public virtual void ShouldNotAccessNegativeReferences()
		 {
			  // given
			  using ( RelationshipScanCursor relationship = cursors.allocateRelationshipScanCursor() )
			  {
					// when
					read.singleRelationship( -2L, relationship );

					// then
					assertFalse( "should not access negative reference relationship", relationship.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessRelationshipLabels()
		 public virtual void ShouldAccessRelationshipLabels()
		 {
			  // given
			  IDictionary<int, int> counts = new Dictionary<int, int>();

			  using ( RelationshipScanCursor relationships = cursors.allocateRelationshipScanCursor() )
			  {
					// when
					read.allRelationshipsScan( relationships );
					while ( relationships.Next() )
					{
						 Counts.compute( relationships.Type(), (k, v) => v == null ? 1 : v + 1 );
					}
			  }

			  // then
			  assertEquals( 3, Counts.Count );
			  int[] values = new int[3];
			  int i = 0;
			  foreach ( int value in Counts.Values )
			  {
					values[i++] = value;
			  }
			  Arrays.sort( values );
			  assertArrayEquals( new int[]{ 1, 6, 6 }, values );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessNodes()
		 public virtual void ShouldAccessNodes()
		 {
			  // given
			  using ( RelationshipScanCursor relationships = cursors.allocateRelationshipScanCursor() )
			  {
					// when
					read.singleRelationship( _one, relationships );

					// then
					assertTrue( relationships.Next() );
					assertEquals( _c, relationships.SourceNodeReference() );
					assertEquals( _d, relationships.TargetNodeReference() );
					assertFalse( relationships.Next() );

					// when
					read.singleRelationship( _loop, relationships );

					// then
					assertTrue( relationships.Next() );
					assertEquals( relationships.SourceNodeReference(), relationships.TargetNodeReference() );
					assertFalse( relationships.Next() );
			  }
		 }
	}

}