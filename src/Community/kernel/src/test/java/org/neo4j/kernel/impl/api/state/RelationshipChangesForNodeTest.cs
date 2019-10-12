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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Matcher = org.hamcrest.Matcher;
	using Test = org.junit.Test;

	using PrimitiveLongCollections = Neo4Net.Collection.PrimitiveLongCollections;
	using RelationshipDirection = Neo4Net.Storageengine.Api.RelationshipDirection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.LOOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.OUTGOING;

	public class RelationshipChangesForNodeTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelationships()
		 public virtual void ShouldGetRelationships()
		 {
			  RelationshipChangesForNode changes = new RelationshipChangesForNode( RelationshipChangesForNode.DiffStrategy.Add );

			  const int type = 2;

			  changes.AddRelationship( 1, type, INCOMING );
			  changes.AddRelationship( 2, type, OUTGOING );
			  changes.AddRelationship( 3, type, OUTGOING );
			  changes.AddRelationship( 4, type, LOOP );
			  changes.AddRelationship( 5, type, LOOP );
			  changes.AddRelationship( 6, type, LOOP );

			  LongIterator rawRelationships = changes.Relationships;
			  assertThat( PrimitiveLongCollections.asArray( rawRelationships ), Ids( 1, 2, 3, 4, 5, 6 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelationshipsByTypeAndDirection()
		 public virtual void ShouldGetRelationshipsByTypeAndDirection()
		 {
			  RelationshipChangesForNode changes = new RelationshipChangesForNode( RelationshipChangesForNode.DiffStrategy.Add );

			  const int type = 2;
			  const int decoyType = 666;

			  changes.AddRelationship( 1, type, INCOMING );
			  changes.AddRelationship( 2, type, OUTGOING );
			  changes.AddRelationship( 3, type, OUTGOING );
			  changes.AddRelationship( 4, type, LOOP );
			  changes.AddRelationship( 5, type, LOOP );
			  changes.AddRelationship( 6, type, LOOP );

			  changes.AddRelationship( 10, decoyType, INCOMING );
			  changes.AddRelationship( 11, decoyType, OUTGOING );
			  changes.AddRelationship( 12, decoyType, LOOP );

			  LongIterator rawIncoming = changes.GetRelationships( RelationshipDirection.INCOMING, type );
			  assertThat( PrimitiveLongCollections.asArray( rawIncoming ), Ids( 1 ) );

			  LongIterator rawOutgoing = changes.GetRelationships( RelationshipDirection.OUTGOING, type );
			  assertThat( PrimitiveLongCollections.asArray( rawOutgoing ), Ids( 2, 3 ) );

			  LongIterator rawLoops = changes.GetRelationships( RelationshipDirection.LOOP, type );
			  assertThat( PrimitiveLongCollections.asArray( rawLoops ), Ids( 4, 5, 6 ) );
		 }

		 private Matcher<long[]> Ids( params long[] ids )
		 {
			  return equalTo( ids );
		 }
	}

}