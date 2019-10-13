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
namespace Neo4Net.@internal.Kernel.Api
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexReadAsserts.assertFoundRelationships;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexReadAsserts.assertNodeCount;

	public abstract class ExplicitIndexCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
		 public override void CreateTestGraph( GraphDatabaseService graphDb )
		 {
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.Index().forNodes("foo").add(graphDb.CreateNode(), "bar", "this is it");
					Relationship edge = graphDb.CreateNode().createRelationshipTo(graphDb.CreateNode(), withName("LALA"));
					graphDb.Index().forRelationships("rels").add(edge, "alpha", "betting on the wrong string");

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindNodeByLookup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindNodeByLookup()
		 {
			  // given
			  using ( NodeExplicitIndexCursor cursor = cursors.allocateNodeExplicitIndexCursor() )
			  {
					MutableLongSet nodes = new LongHashSet();

					// when
					indexRead.nodeExplicitIndexLookup( cursor, "foo", "bar", "this is it" );

					// then
					assertNodeCount( cursor, 1, nodes );

					// when
					indexRead.nodeExplicitIndexLookup( cursor, "foo", "bar", "not that" );

					// then
					assertNodeCount( cursor, 0, nodes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindNodeByQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindNodeByQuery()
		 {
			  // given
			  using ( NodeExplicitIndexCursor cursor = cursors.allocateNodeExplicitIndexCursor() )
			  {
					MutableLongSet nodes = new LongHashSet();

					// when
					indexRead.nodeExplicitIndexQuery( cursor, "foo", "bar:this*" );

					// then
					assertNodeCount( cursor, 1, nodes );

					// when
					nodes.clear();
					indexRead.nodeExplicitIndexQuery( cursor, "foo", "bar", "this*" );

					// then
					assertNodeCount( cursor, 1, nodes );

					// when
					indexRead.nodeExplicitIndexQuery( cursor, "foo", "bar:that*" );

					// then
					assertNodeCount( cursor, 0, nodes );

					// when
					indexRead.nodeExplicitIndexQuery( cursor, "foo", "bar", "that*" );

					// then
					assertNodeCount( cursor, 0, nodes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindRelationshipByLookup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindRelationshipByLookup()
		 {
			  // given
			  using ( RelationshipExplicitIndexCursor cursor = cursors.allocateRelationshipExplicitIndexCursor(), )
			  {
					MutableLongSet edges = new LongHashSet();

					// when
					indexRead.relationshipExplicitIndexLookup( cursor, "rels", "alpha", "betting on the wrong string", -1, -1 );

					// then
					assertFoundRelationships( cursor, 1, edges );

					// when
					indexRead.relationshipExplicitIndexLookup( cursor, "rels", "bar", "not that", -1, -1 );

					// then
					assertFoundRelationships( cursor, 0, edges );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindRelationshipByQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindRelationshipByQuery()
		 {
			  // given
			  using ( RelationshipExplicitIndexCursor cursor = cursors.allocateRelationshipExplicitIndexCursor(), )
			  {
					MutableLongSet relationships = new LongHashSet();

					// when
					indexRead.relationshipExplicitIndexQuery( cursor, "rels", "alpha:betting*", -1, -1 );

					// then
					assertFoundRelationships( cursor, 1, relationships );

					// when
					relationships.clear();
					indexRead.relationshipExplicitIndexQuery( cursor, "rels", "alpha", "betting*", -1,-1 );

					// then
					assertFoundRelationships( cursor, 1, relationships );

					// when
					indexRead.relationshipExplicitIndexQuery( cursor, "rels", "alpha:that*", -1, -1 );

					// then
					assertFoundRelationships( cursor, 0, relationships );

					// when
					indexRead.relationshipExplicitIndexQuery( cursor, "rels", "alpha", "that*", -1, -1 );

					// then
					assertFoundRelationships( cursor, 0, relationships );
			  }
		 }
	}

}