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
namespace Neo4Net.Internal.Kernel.Api.helpers
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.helpers.Nodes.countAll;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.helpers.Nodes.countIncoming;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.helpers.Nodes.countOutgoing;

	public class NodesTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountOutgoingDense()
		 public virtual void ShouldCountOutgoingDense()
		 {
			  // Given
			  StubGroupCursor groupCursor = new StubGroupCursor( Group().withOutCount(1).withInCount(1).withLoopCount(5), Group().withOutCount(1).withInCount(1).withLoopCount(3), Group().withOutCount(2).withInCount(1).withLoopCount(2), Group().withOutCount(3).withInCount(1).withLoopCount(1), Group().withOutCount(5).withInCount(1).withLoopCount(1) );
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithGroupCursors(groupCursor);

			  // When
			  int count = countOutgoing( new StubNodeCursor( true ), cursors );

			  // Then
			  assertThat( count, equalTo( 24 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountOutgoingSparse()
		 public virtual void ShouldCountOutgoingSparse()
		 {
			  // Given
			  StubRelationshipCursor relationshipCursor = new StubRelationshipCursor(new TestRelationshipChain(11)
									.outgoing( 55, 0, 1 ).incoming( 56, 0, 1 ).outgoing( 57, 0, 1 ).loop( 58, 0 ));
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithRelationshipTraversalCursors(relationshipCursor);

			  // When
			  StubNodeCursor nodeCursor = ( new StubNodeCursor( false ) ).WithNode( 11 );
			  nodeCursor.Next();
			  int count = countOutgoing( nodeCursor, cursors );

			  // Then
			  assertThat( count, equalTo( 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountIncomingDense()
		 public virtual void ShouldCountIncomingDense()
		 {
			  // Given
			  StubGroupCursor groupCursor = new StubGroupCursor( Group().withOutCount(1).withInCount(1).withLoopCount(5), Group().withOutCount(1).withInCount(1).withLoopCount(3), Group().withOutCount(2).withInCount(1).withLoopCount(2), Group().withOutCount(3).withInCount(1).withLoopCount(1), Group().withOutCount(5).withInCount(1).withLoopCount(1) );
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithGroupCursors(groupCursor);

			  // When
			  int count = countIncoming( new StubNodeCursor( true ), cursors );

			  // Then
			  assertThat( count, equalTo( 17 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountIncomingSparse()
		 public virtual void ShouldCountIncomingSparse()
		 {
			  // Given
			  StubRelationshipCursor relationshipCursor = new StubRelationshipCursor(new TestRelationshipChain(11)
									.outgoing( 55, 0, 1 ).incoming( 56, 0, 1 ).outgoing( 57, 0, 1 ).loop( 58, 0 ));
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithRelationshipTraversalCursors(relationshipCursor);

			  StubNodeCursor nodeCursor = ( new StubNodeCursor( false ) ).WithNode( 11 );
			  nodeCursor.Next();

			  // When
			  int count = countIncoming( nodeCursor, cursors );

			  // Then
			  assertThat( count, equalTo( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountAllDense()
		 public virtual void ShouldCountAllDense()
		 {
			  // Given
			  StubGroupCursor groupCursor = new StubGroupCursor( Group().withOutCount(1).withInCount(1).withLoopCount(5), Group().withOutCount(1).withInCount(1).withLoopCount(3), Group().withOutCount(2).withInCount(1).withLoopCount(2), Group().withOutCount(3).withInCount(1).withLoopCount(1), Group().withOutCount(5).withInCount(1).withLoopCount(1) );
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithGroupCursors(groupCursor);

			  // When
			  int count = countAll( new StubNodeCursor( true ), cursors );

			  // Then
			  assertThat( count, equalTo( 29 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountAllSparse()
		 public virtual void ShouldCountAllSparse()
		 {
			  // Given
			  StubRelationshipCursor relationshipCursor = new StubRelationshipCursor(new TestRelationshipChain(11)
									.outgoing( 55, 0, 1 ).incoming( 56, 0, 1 ).outgoing( 57, 0, 1 ).loop( 58, 0 ));
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithRelationshipTraversalCursors(relationshipCursor);

			  StubNodeCursor nodeCursor = ( new StubNodeCursor( false ) ).WithNode( 11 );
			  nodeCursor.Next();

			  // When
			  int count = countAll( nodeCursor, cursors );

			  // Then
			  assertThat( count, equalTo( 4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountOutgoingDenseWithType()
		 public virtual void ShouldCountOutgoingDenseWithType()
		 {
			  // Given
			  StubGroupCursor groupCursor = new StubGroupCursor( Group( 1 ).withOutCount( 1 ).withInCount( 1 ).withLoopCount( 5 ), Group( 2 ).withOutCount( 1 ).withInCount( 1 ).withLoopCount( 3 ) );
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithGroupCursors(groupCursor, groupCursor);

			  // Then
			  assertThat( countOutgoing( new StubNodeCursor( true ), cursors, 1 ), equalTo( 6 ) );
			  assertThat( countOutgoing( new StubNodeCursor( true ), cursors, 2 ), equalTo( 4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountOutgoingSparseWithType()
		 public virtual void ShouldCountOutgoingSparseWithType()
		 {
			  // Given
			  StubRelationshipCursor relationshipCursor = new StubRelationshipCursor(new TestRelationshipChain(11)
									.outgoing( 55, 0, 1 ).incoming( 56, 0, 1 ).outgoing( 57, 0, 1 ).loop( 58, 2 ));
			  StubCursorFactory cursors = ( new StubCursorFactory( true ) ).WithRelationshipTraversalCursors( relationshipCursor );

			  // Then
			  StubNodeCursor nodeCursor = ( new StubNodeCursor( false ) ).WithNode( 11 );
			  nodeCursor.Next();
			  assertThat( countOutgoing( nodeCursor, cursors, 1 ), equalTo( 2 ) );
			  nodeCursor = ( new StubNodeCursor( false ) ).WithNode( 11 );
			  nodeCursor.Next();
			  assertThat( countOutgoing( nodeCursor, cursors, 2 ), equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountIncomingWithTypeDense()
		 public virtual void ShouldCountIncomingWithTypeDense()
		 {
			  // Given
			  StubGroupCursor groupCursor = new StubGroupCursor( Group( 1 ).withOutCount( 1 ).withInCount( 1 ).withLoopCount( 5 ), Group( 2 ).withOutCount( 1 ).withInCount( 1 ).withLoopCount( 3 ) );
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithGroupCursors(groupCursor, groupCursor);

			  // Then
			  assertThat( countIncoming( new StubNodeCursor( true ), cursors, 1 ), equalTo( 6 ) );
			  assertThat( countIncoming( new StubNodeCursor( true ), cursors, 2 ), equalTo( 4 ) );
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountIncomingWithTypeSparse()
		 public virtual void ShouldCountIncomingWithTypeSparse()
		 {
			  // Given
			  StubRelationshipCursor relationshipCursor = new StubRelationshipCursor(new TestRelationshipChain(11)
									.outgoing( 55, 0, 1 ).incoming( 56, 0, 1 ).outgoing( 57, 0, 1 ).loop( 58, 2 ));
			  StubCursorFactory cursors = ( new StubCursorFactory( true ) ).WithRelationshipTraversalCursors( relationshipCursor );

			  // Then
			  StubNodeCursor nodeCursor = ( new StubNodeCursor( false ) ).WithNode( 11 );
			  nodeCursor.Next();
			  assertThat( countIncoming( nodeCursor, cursors, 1 ), equalTo( 1 ) );
			  nodeCursor = ( new StubNodeCursor( false ) ).WithNode( 11 );
			  nodeCursor.Next();
			  assertThat( countIncoming( nodeCursor, cursors, 2 ), equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountAllWithTypeDense()
		 public virtual void ShouldCountAllWithTypeDense()
		 {
			  // Given
			  StubGroupCursor groupCursor = new StubGroupCursor( Group( 1 ).withOutCount( 1 ).withInCount( 1 ).withLoopCount( 5 ), Group( 2 ).withOutCount( 1 ).withInCount( 1 ).withLoopCount( 3 ) );
			  StubCursorFactory cursors = ( new StubCursorFactory() ).WithGroupCursors(groupCursor, groupCursor);

			  // Then
			  assertThat( countAll( new StubNodeCursor( true ), cursors, 1 ), equalTo( 7 ) );
			  assertThat( countAll( new StubNodeCursor( true ), cursors, 2 ), equalTo( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountAllWithTypeSparse()
		 public virtual void ShouldCountAllWithTypeSparse()
		 {
			  // Given
			  StubRelationshipCursor relationshipCursor = new StubRelationshipCursor(new TestRelationshipChain(11)
									.outgoing( 55, 0, 1 ).incoming( 56, 0, 1 ).outgoing( 57, 0, 1 ).loop( 58, 2 ));
			  StubCursorFactory cursors = ( new StubCursorFactory( true ) ).WithRelationshipTraversalCursors( relationshipCursor );

			  // Then
			  StubNodeCursor nodeCursor = ( new StubNodeCursor( false ) ).WithNode( 11 );
			  nodeCursor.Next();
			  assertThat( countAll( nodeCursor, cursors, 1 ), equalTo( 3 ) );
			  assertThat( countAll( nodeCursor, cursors, 2 ), equalTo( 1 ) );
		 }

		 private StubGroupCursor.GroupData Group()
		 {
			  return new StubGroupCursor.GroupData( 0, 0, 0, 0 );
		 }

		 private StubGroupCursor.GroupData Group( int type )
		 {
			  return new StubGroupCursor.GroupData( 0, 0, 0, type );
		 }
	}

}