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
namespace Neo4Net.Kernel.Api.Internal.Helpers
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Neo4Net.GraphDb;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public abstract class RelationshipDenseSelectionTestBase<Traverser> : RelationshipSelectionTestBase where Traverser : RelationshipDenseSelection
	{
		private bool InstanceFieldsInitialized = false;

		public RelationshipDenseSelectionTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_innerGroupCursor = new StubGroupCursor( Group( _store, typeA, _outA, _inA, _loopA ), Group( _store, typeB, _outB, _inB, _loopB ), Group( _store, typeC, _outC, _inC, _loopC ) );
			_innerRelationshipCursor = new StubRelationshipCursor( _store );
		}


		 private TestRelationshipChain _outA = new TestRelationshipChain( 42L );

		 private TestRelationshipChain _inA = new TestRelationshipChain( 42L ).incoming( 0, 1, typeA ).incoming( 0, 2, typeA );

		 private TestRelationshipChain _loopA = new TestRelationshipChain( 42L ).loop( 0, typeA );

		 private TestRelationshipChain _outB = new TestRelationshipChain( 42L ).outgoing( 0, 10, typeB );

		 private TestRelationshipChain _inB = new TestRelationshipChain( 42L );

		 private TestRelationshipChain _loopB = new TestRelationshipChain( 42L ).loop( 0, typeB ).loop( 0, typeB );

		 private TestRelationshipChain _outC = new TestRelationshipChain( 42L ).outgoing( 0, 20, typeC ).outgoing( 0, 21, typeC );

		 private TestRelationshipChain _inC = new TestRelationshipChain( 42L ).incoming( 0, 22, typeC );

		 private TestRelationshipChain _loopC = new TestRelationshipChain( 42L );

		 private IList<TestRelationshipChain> _store = new List<TestRelationshipChain>();

		 private StubGroupCursor _innerGroupCursor;

		 private StubRelationshipCursor _innerRelationshipCursor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void rewindCursor()
		 public virtual void RewindCursor()
		 {
			  _innerGroupCursor.rewind();
		 }

		 protected internal abstract Traverser Make();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectOutgoing()
		 public virtual void ShouldSelectOutgoing()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.Outgoing( _innerGroupCursor, _innerRelationshipCursor );

			  // then
			  AssertLoop( traverser, typeA );
			  AssertOutgoing( traverser, 10, typeB );
			  AssertLoop( traverser, typeB );
			  AssertLoop( traverser, typeB );
			  AssertOutgoing( traverser, 20, typeC );
			  AssertOutgoing( traverser, 21, typeC );
			  AssertEmptyAndClosed( traverser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectIncoming()
		 public virtual void ShouldSelectIncoming()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.Incoming( _innerGroupCursor, _innerRelationshipCursor );

			  // then
			  AssertIncoming( traverser, 1, typeA );
			  AssertIncoming( traverser, 2, typeA );
			  AssertLoop( traverser, typeA );
			  AssertLoop( traverser, typeB );
			  AssertLoop( traverser, typeB );
			  AssertIncoming( traverser, 22, typeC );
			  AssertEmptyAndClosed( traverser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectAll()
		 public virtual void ShouldSelectAll()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.All( _innerGroupCursor, _innerRelationshipCursor );

			  // then
			  AssertIncoming( traverser, 1, typeA );
			  AssertIncoming( traverser, 2, typeA );
			  AssertLoop( traverser, typeA );
			  AssertOutgoing( traverser, 10, typeB );
			  AssertLoop( traverser, typeB );
			  AssertLoop( traverser, typeB );
			  AssertOutgoing( traverser, 20, typeC );
			  AssertOutgoing( traverser, 21, typeC );
			  AssertIncoming( traverser, 22, typeC );
			  AssertEmptyAndClosed( traverser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectOutgoingOfType()
		 public virtual void ShouldSelectOutgoingOfType()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.Outgoing( _innerGroupCursor, _innerRelationshipCursor, types( typeA, typeC ) );

			  // then
			  AssertLoop( traverser, typeA );
			  AssertOutgoing( traverser, 20, typeC );
			  AssertOutgoing( traverser, 21, typeC );
			  AssertEmptyAndClosed( traverser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectIncomingOfType()
		 public virtual void ShouldSelectIncomingOfType()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.Incoming( _innerGroupCursor, _innerRelationshipCursor, types( typeA, typeC ) );

			  // then
			  AssertIncoming( traverser, 1, typeA );
			  AssertIncoming( traverser, 2, typeA );
			  AssertLoop( traverser, typeA );
			  AssertIncoming( traverser, 22, typeC );
			  AssertEmptyAndClosed( traverser );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectAllOfType()
		 public virtual void ShouldSelectAllOfType()
		 {
			  // given
			  Traverser traverser = Make();

			  // when
			  traverser.All( _innerGroupCursor, _innerRelationshipCursor, types( typeA, typeC ) );

			  // then
			  AssertIncoming( traverser, 1, typeA );
			  AssertIncoming( traverser, 2, typeA );
			  AssertLoop( traverser, typeA );
			  AssertOutgoing( traverser, 20, typeC );
			  AssertOutgoing( traverser, 21, typeC );
			  AssertIncoming( traverser, 22, typeC );
			  AssertEmptyAndClosed( traverser );
		 }

		 internal abstract void AssertOutgoing( Traverser cursor, int targetNode, int type );

		 internal abstract void AssertIncoming( Traverser cursor, int sourceNode, int type );

		 internal abstract void AssertLoop( Traverser cursor, int type );

		 internal abstract void AssertEmpty( Traverser cursor );

		 public class IteratorTest : RelationshipDenseSelectionTestBase<RelationshipDenseSelectionIterator<R>>
		 {
			  protected internal override RelationshipDenseSelectionIterator<R> Make()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return new RelationshipDenseSelectionIterator<R>( R::new );
			  }

			  internal override void AssertOutgoing( RelationshipDenseSelectionIterator<R> iterator, int targetNode, int type )
			  {
					AssertOutgoing( ( ResourceIterator<R> ) iterator, targetNode, type );
			  }

			  internal override void AssertIncoming( RelationshipDenseSelectionIterator<R> iterator, int sourceNode, int type )
			  {
					AssertIncoming( ( ResourceIterator<R> ) iterator, sourceNode, type );
			  }

			  internal override void AssertLoop( RelationshipDenseSelectionIterator<R> iterator, int type )
			  {
					AssertLoop( ( ResourceIterator<R> ) iterator, type );
			  }

			  internal override void AssertEmpty( RelationshipDenseSelectionIterator<R> iterator )
			  {
					AssertEmpty( ( ResourceIterator<R> ) iterator );
			  }
		 }

		 public class CursorTest : RelationshipDenseSelectionTestBase<RelationshipDenseSelectionCursor>
		 {
			  protected internal override RelationshipDenseSelectionCursor Make()
			  {
					return new RelationshipDenseSelectionCursor();
			  }

			  internal override void AssertOutgoing( RelationshipDenseSelectionCursor iterator, int targetNode, int type )
			  {
					AssertOutgoing( ( RelationshipSelectionCursor ) iterator, targetNode, type );
			  }

			  internal override void AssertIncoming( RelationshipDenseSelectionCursor iterator, int sourceNode, int type )
			  {
					AssertIncoming( ( RelationshipSelectionCursor ) iterator, sourceNode, type );
			  }

			  internal override void AssertLoop( RelationshipDenseSelectionCursor iterator, int type )
			  {
					AssertLoop( ( RelationshipSelectionCursor ) iterator, type );
			  }

			  internal override void AssertEmpty( RelationshipDenseSelectionCursor iterator )
			  {
					AssertEmpty( ( RelationshipSelectionCursor ) iterator );
			  }
		 }

		 private void AssertEmptyAndClosed( Traverser traverser )
		 {
			  AssertEmpty( traverser );
			  assertTrue( "close group cursor", _innerGroupCursor.Closed );
			  assertTrue( "close traversal cursor", _innerRelationshipCursor.Closed );
		 }

		 private StubGroupCursor.GroupData Group( IList<TestRelationshipChain> store, int type, TestRelationshipChain @out, TestRelationshipChain @in, TestRelationshipChain loop )
		 {
			  return new StubGroupCursor.GroupData( AddToStore( store, @out ), AddToStore( store, @in ), AddToStore( store, loop ), type );
		 }

		 private int AddToStore( IList<TestRelationshipChain> store, TestRelationshipChain chain )
		 {
			  int @ref = store.Count;
			  store.Add( chain );
			  return @ref;
		 }
	}

}