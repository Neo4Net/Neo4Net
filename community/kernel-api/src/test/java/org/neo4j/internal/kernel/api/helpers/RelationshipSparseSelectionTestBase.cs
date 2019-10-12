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
namespace Org.Neo4j.@internal.Kernel.Api.helpers
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Org.Neo4j.Graphdb;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public abstract class RelationshipSparseSelectionTestBase<Traverser> : RelationshipSelectionTestBase where Traverser : RelationshipSparseSelection
	{
		 private StubRelationshipCursor innerByGroup = new StubRelationshipCursor(new TestRelationshipChain(42L)
										 .outgoing( 0, 10, typeA ).incoming( 1, 11, typeA ).loop( 2, typeA ).outgoing( 3, 20, typeB ).incoming( 4, 21, typeB ).loop( 5, typeB ).outgoing( 6, 30, typeC ).incoming( 7, 31, typeC ).loop( 8, typeC ));

		 private StubRelationshipCursor innerByDir = new StubRelationshipCursor(new TestRelationshipChain(42L)
										 .outgoing( 1, 10, typeA ).outgoing( 2, 11, typeB ).outgoing( 3, 12, typeC ).incoming( 4, 20, typeA ).incoming( 5, 21, typeB ).incoming( 6, 22, typeC ).loop( 7, typeA ).loop( 8, typeB ).loop( 9, typeC ));

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void rewindInner()
		 public void rewindInner()
		 {
			  innerByDir.rewind();
			  innerByGroup.rewind();
		 }

		 protected abstract Traverser Make();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectOutgoing()
		 public void shouldSelectOutgoing()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.Outgoing( innerByGroup );

			  // then
			  AssertOutgoing( traverser, 10, typeA );
			  AssertLoop( traverser, typeA );
			  AssertOutgoing( traverser, 20, typeB );
			  AssertLoop( traverser, typeB );
			  AssertOutgoing( traverser, 30, typeC );
			  AssertLoop( traverser, typeC );
			  AssertEmptyAndClosed( traverser, innerByGroup );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectIncoming()
		 public void shouldSelectIncoming()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.Incoming( innerByGroup );

			  // then
			  AssertIncoming( traverser, 11, typeA );
			  AssertLoop( traverser, typeA );
			  AssertIncoming( traverser, 21, typeB );
			  AssertLoop( traverser, typeB );
			  AssertIncoming( traverser, 31, typeC );
			  AssertLoop( traverser, typeC );
			  AssertEmptyAndClosed( traverser, innerByGroup );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectAll()
		 public void shouldSelectAll()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.All( innerByGroup );

			  // then
			  AssertOutgoing( traverser, 10, typeA );
			  AssertIncoming( traverser, 11, typeA );
			  AssertLoop( traverser, typeA );
			  AssertOutgoing( traverser, 20, typeB );
			  AssertIncoming( traverser, 21, typeB );
			  AssertLoop( traverser, typeB );
			  AssertOutgoing( traverser, 30, typeC );
			  AssertIncoming( traverser, 31, typeC );
			  AssertLoop( traverser, typeC );
			  AssertEmptyAndClosed( traverser, innerByGroup );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectOutgoingOfType()
		 public void shouldSelectOutgoingOfType()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.Outgoing( innerByDir, types( typeA, typeC ) );

			  // then
			  AssertOutgoing( traverser, 10, typeA );
			  AssertOutgoing( traverser, 12, typeC );
			  AssertLoop( traverser, typeA );
			  AssertLoop( traverser, typeC );
			  AssertEmptyAndClosed( traverser, innerByDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectIncomingOfType()
		 public void shouldSelectIncomingOfType()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.Incoming( innerByDir, types( typeA, typeC ) );

			  // then
			  AssertIncoming( traverser, 20, typeA );
			  AssertIncoming( traverser, 22, typeC );
			  AssertLoop( traverser, typeA );
			  AssertLoop( traverser, typeC );
			  AssertEmptyAndClosed( traverser, innerByDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectAllOfType()
		 public void shouldSelectAllOfType()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.All( innerByDir, types( typeA, typeC ) );

			  // then
			  AssertOutgoing( traverser, 10, typeA );
			  AssertOutgoing( traverser, 12, typeC );
			  AssertIncoming( traverser, 20, typeA );
			  AssertIncoming( traverser, 22, typeC );
			  AssertLoop( traverser, typeA );
			  AssertLoop( traverser, typeC );
			  AssertEmptyAndClosed( traverser, innerByDir );
		 }

		 abstract void assertOutgoing( Traverser cursor, int targetNode, int type );

		 abstract void assertIncoming( Traverser cursor, int sourceNode, int type );

		 abstract void assertLoop( Traverser cursor, int type );

		 abstract void assertEmpty( Traverser cursor );

		 public static class IteratorTest extends RelationshipSparseSelectionTestBase<RelationshipSparseSelectionIterator<R>>
		 {
			  protected RelationshipSparseSelectionIterator<R> Make()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return new RelationshipSparseSelectionIterator<>( R::new );
			  }

			  void assertOutgoing( RelationshipSparseSelectionIterator<R> iterator, int targetNode, int type )
			  {
					AssertOutgoing( ( ResourceIterator<R> ) iterator, targetNode, type );
			  }

			  void assertIncoming( RelationshipSparseSelectionIterator<R> iterator, int sourceNode, int type )
			  {
					AssertIncoming( ( ResourceIterator<R> ) iterator, sourceNode, type );
			  }

			  void assertLoop( RelationshipSparseSelectionIterator<R> iterator, int type )
			  {
					AssertLoop( ( ResourceIterator<R> ) iterator, type );
			  }

			  void assertEmpty( RelationshipSparseSelectionIterator<R> iterator )
			  {
					AssertEmpty( ( ResourceIterator<R> ) iterator );
			  }
		 }

		 public static class CursorTest extends RelationshipSparseSelectionTestBase<RelationshipSparseSelectionCursor>
		 {
			  protected RelationshipSparseSelectionCursor Make()
			  {
					return new RelationshipSparseSelectionCursor();
			  }

			  void assertOutgoing( RelationshipSparseSelectionCursor iterator, int targetNode, int type )
			  {
					AssertOutgoing( ( RelationshipSelectionCursor ) iterator, targetNode, type );
			  }

			  void assertIncoming( RelationshipSparseSelectionCursor iterator, int sourceNode, int type )
			  {
					AssertIncoming( ( RelationshipSelectionCursor ) iterator, sourceNode, type );
			  }

			  void assertLoop( RelationshipSparseSelectionCursor iterator, int type )
			  {
					AssertLoop( ( RelationshipSelectionCursor ) iterator, type );
			  }

			  void assertEmpty( RelationshipSparseSelectionCursor iterator )
			  {
					AssertEmpty( ( RelationshipSelectionCursor ) iterator );
			  }
		 }

		 private void assertEmptyAndClosed( Traverser traverser, RelationshipTraversalCursor inner )
		 {
			  AssertEmpty( traverser );
			  assertTrue( "closed traversal cursor", inner.Closed );
		 }
	}

}