/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state.machines.id
{
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ReplicatedIdAllocationStateMachineTest
	{
		 private MemberId _me = new MemberId( System.Guid.randomUUID() );

		 private IdType _someType = IdType.NODE;
		 private IdType _someOtherType = IdType.RELATIONSHIP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotHaveAnyIdsInitially()
		 public virtual void ShouldNotHaveAnyIdsInitially()
		 {
			  // given
			  ReplicatedIdAllocationStateMachine stateMachine = new ReplicatedIdAllocationStateMachine( new InMemoryStateStorage<IdAllocationState>( new IdAllocationState() ) );

			  // then
			  assertEquals( 0, stateMachine.FirstUnallocated( _someType ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateStateOnlyForTypeRequested()
		 public virtual void ShouldUpdateStateOnlyForTypeRequested()
		 {
			  // given
			  ReplicatedIdAllocationStateMachine stateMachine = new ReplicatedIdAllocationStateMachine( new InMemoryStateStorage<IdAllocationState>( new IdAllocationState() ) );
			  ReplicatedIdAllocationRequest idAllocationRequest = new ReplicatedIdAllocationRequest( _me, _someType, 0, 1024 );

			  // when
			  stateMachine.ApplyCommand(idAllocationRequest, 0, r =>
			  {
			  });

			  // then
			  assertEquals( 1024, stateMachine.FirstUnallocated( _someType ) );
			  assertEquals( 0, stateMachine.FirstUnallocated( _someOtherType ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void severalDistinctRequestsShouldIncrementallyUpdate()
		 public virtual void SeveralDistinctRequestsShouldIncrementallyUpdate()
		 {
			  // given
			  ReplicatedIdAllocationStateMachine stateMachine = new ReplicatedIdAllocationStateMachine( new InMemoryStateStorage<IdAllocationState>( new IdAllocationState() ) );
			  long index = 0;

			  // when
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 0, 1024), index++, r =>
			  {
			  });
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 1024, 1024), index++, r =>
			  {
			  });
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 2048, 1024), index, r =>
			  {
			  });

			  // then
			  assertEquals( 3072, stateMachine.FirstUnallocated( _someType ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void severalEqualRequestsShouldOnlyUpdateOnce()
		 public virtual void SeveralEqualRequestsShouldOnlyUpdateOnce()
		 {
			  // given
			  ReplicatedIdAllocationStateMachine stateMachine = new ReplicatedIdAllocationStateMachine( new InMemoryStateStorage<IdAllocationState>( new IdAllocationState() ) );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 0, 1024), 0, r =>
			  {
			  });
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 0, 1024), 0, r =>
			  {
			  });
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 0, 1024), 0, r =>
			  {
			  });

			  // then
			  assertEquals( 1024, stateMachine.FirstUnallocated( _someType ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void outOfOrderRequestShouldBeIgnored()
		 public virtual void OutOfOrderRequestShouldBeIgnored()
		 {
			  // given
			  ReplicatedIdAllocationStateMachine stateMachine = new ReplicatedIdAllocationStateMachine( new InMemoryStateStorage<IdAllocationState>( new IdAllocationState() ) );

			  // when
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 0, 1024), 0, r =>
			  {
			  });
			  // apply command that doesn't consume ids because the requested range is non-contiguous
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 2048, 1024), 0, r =>
			  {
			  });

			  // then
			  assertEquals( 1024, stateMachine.FirstUnallocated( _someType ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreNotContiguousRequestAndAlreadySeenIndex()
		 public virtual void ShouldIgnoreNotContiguousRequestAndAlreadySeenIndex()
		 {
			  ReplicatedIdAllocationStateMachine stateMachine = new ReplicatedIdAllocationStateMachine( new InMemoryStateStorage<IdAllocationState>( new IdAllocationState() ) );

			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 0L, 10), 0L, r =>
			  {
			  });
			  assertEquals( 10L, stateMachine.FirstUnallocated( _someType ) );

			  // apply command that doesn't consume ids because the requested range is non-contiguous
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 20L, 10), 1L, r =>
			  {
			  });
			  assertEquals( 10L, stateMachine.FirstUnallocated( _someType ) );

			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 10L, 10), 2L, r =>
			  {
			  });
			  assertEquals( 20L, stateMachine.FirstUnallocated( _someType ) );

			  // try applying the same command again. The requested range is now contiguous, but the log index
			  // has already been exceeded
			  stateMachine.ApplyCommand(new ReplicatedIdAllocationRequest(_me, _someType, 20L, 10), 1L, r =>
			  {
			  });
			  assertEquals( 20L, stateMachine.FirstUnallocated( _someType ) );
		 }
	}

}