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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using Neo4Net.Functions;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using IEntityNotFoundException = Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using StubCursorFactory = Neo4Net.Kernel.Api.Internal.Helpers.StubCursorFactory;
	using StubNodeCursor = Neo4Net.Kernel.Api.Internal.Helpers.StubNodeCursor;
	using StubRead = Neo4Net.Kernel.Api.Internal.Helpers.StubRead;
	using StubRelationshipCursor = Neo4Net.Kernel.Api.Internal.Helpers.StubRelationshipCursor;
	using TestRelationshipChain = Neo4Net.Kernel.Api.Internal.Helpers.TestRelationshipChain;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.set;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer.NONE;

	public class TwoPhaseNodeForRelationshipLockingTest
	{
		 private readonly Transaction _transaction = mock( typeof( Transaction ) );
		 private readonly Neo4Net.Kernel.impl.locking.Locks_Client _locks = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
		 private readonly long _nodeId = 42L;
		 private static int _type = 77;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLockNodesInOrderAndConsumeTheRelationships() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLockNodesInOrderAndConsumeTheRelationships()
		 {
			  // given
			  Collector collector = new Collector();
			  TwoPhaseNodeForRelationshipLocking locking = new TwoPhaseNodeForRelationshipLocking( collector, _locks, NONE );

			  ReturnRelationships( _transaction, false, ( new TestRelationshipChain( _nodeId ) ).outgoing( 21L, 43L, 0 ).incoming( 22L, 40L, _type ).outgoing( 23L, 41L, _type ).outgoing( 2L, 3L, _type ).incoming( 3L, 49L, _type ).outgoing( 50L, 41L, _type ) );
			  InOrder inOrder = inOrder( _locks );

			  // when
			  locking.LockAllNodesAndConsumeRelationships( _nodeId, _transaction, ( new StubNodeCursor( false ) ).withNode( _nodeId ) );

			  // then
			  inOrder.verify( _locks ).acquireExclusive( NONE, NODE, 3L, 40L, 41L, _nodeId, 43L, 49L );
			  assertEquals( set( 21L, 22L, 23L, 2L, 3L, 50L ), collector.Set );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLockNodesInOrderAndConsumeTheRelationshipsAndRetryIfTheNewRelationshipsAreCreated() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLockNodesInOrderAndConsumeTheRelationshipsAndRetryIfTheNewRelationshipsAreCreated()
		 {
			  // given
			  Collector collector = new Collector();
			  TwoPhaseNodeForRelationshipLocking locking = new TwoPhaseNodeForRelationshipLocking( collector, _locks, NONE );

			  TestRelationshipChain chain = ( new TestRelationshipChain( _nodeId ) ).outgoing( 21L, 43L, _type ).incoming( 22L, 40, _type ).outgoing( 23L, 41L, _type );
			  ReturnRelationships( _transaction, true, chain );

			  InOrder inOrder = inOrder( _locks );

			  // when
			  locking.LockAllNodesAndConsumeRelationships( _nodeId, _transaction, ( new StubNodeCursor( false ) ).withNode( _nodeId ) );

			  // then
			  inOrder.verify( _locks ).acquireExclusive( NONE, NODE, 40L, 41L, _nodeId );

			  inOrder.verify( _locks ).releaseExclusive( NODE, 40L, 41L, _nodeId );

			  inOrder.verify( _locks ).acquireExclusive( NONE, NODE, 40L, 41L, _nodeId, 43L );
			  assertEquals( set( 21L, 22L, 23L ), collector.Set );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lockNodeWithoutRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LockNodeWithoutRelationships()
		 {
			  Collector collector = new Collector();
			  TwoPhaseNodeForRelationshipLocking locking = new TwoPhaseNodeForRelationshipLocking( collector, _locks, NONE );
			  ReturnRelationships( _transaction, false, new TestRelationshipChain( 42 ) );

			  locking.LockAllNodesAndConsumeRelationships( _nodeId, _transaction, ( new StubNodeCursor( false ) ).withNode( _nodeId ) );

			  verify( _locks ).acquireExclusive( NONE, NODE, _nodeId );
			  verifyNoMoreInteractions( _locks );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void returnRelationships(org.Neo4Net.Kernel.Api.Internal.Transaction transaction, final boolean skipFirst, final org.Neo4Net.Kernel.Api.Internal.Helpers.TestRelationshipChain relIds) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 internal static void ReturnRelationships( Transaction transaction, bool skipFirst, TestRelationshipChain relIds )
		 {

			  StubRead read = new StubRead();
			  when( transaction.DataRead() ).thenReturn(read);
			  StubCursorFactory cursorFactory = new StubCursorFactory( true );
			  if ( skipFirst )
			  {
					cursorFactory.WithRelationshipTraversalCursors( new StubRelationshipCursor( relIds.Tail() ), new StubRelationshipCursor(relIds) );
			  }
			  else
			  {
					cursorFactory.WithRelationshipTraversalCursors( new StubRelationshipCursor( relIds ) );
			  }

			  when( transaction.Cursors() ).thenReturn(cursorFactory);
		 }

		 private class Collector : ThrowingConsumer<long, KernelException>
		 {
			  public readonly ISet<long> Set = new HashSet<long>();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(System.Nullable<long> input) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
			  public override void Accept( long? input )
			  {
					assertNotNull( input );
					Set.Add( input );
			  }
		 }
	}

}