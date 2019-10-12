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
namespace Org.Neo4j.Kernel.Impl.Newapi
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using Org.Neo4j.Function;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using StubCursorFactory = Org.Neo4j.@internal.Kernel.Api.helpers.StubCursorFactory;
	using StubNodeCursor = Org.Neo4j.@internal.Kernel.Api.helpers.StubNodeCursor;
	using StubRead = Org.Neo4j.@internal.Kernel.Api.helpers.StubRead;
	using StubRelationshipCursor = Org.Neo4j.@internal.Kernel.Api.helpers.StubRelationshipCursor;
	using TestRelationshipChain = Org.Neo4j.@internal.Kernel.Api.helpers.TestRelationshipChain;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;

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
//	import static org.neo4j.helpers.collection.Iterators.set;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.@lock.LockTracer.NONE;

	public class TwoPhaseNodeForRelationshipLockingTest
	{
		 private readonly Transaction _transaction = mock( typeof( Transaction ) );
		 private readonly Org.Neo4j.Kernel.impl.locking.Locks_Client _locks = mock( typeof( Org.Neo4j.Kernel.impl.locking.Locks_Client ) );
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
//ORIGINAL LINE: static void returnRelationships(org.neo4j.internal.kernel.api.Transaction transaction, final boolean skipFirst, final org.neo4j.internal.kernel.api.helpers.TestRelationshipChain relIds) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
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
//ORIGINAL LINE: public void accept(System.Nullable<long> input) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  public override void Accept( long? input )
			  {
					assertNotNull( input );
					Set.Add( input );
			  }
		 }
	}

}