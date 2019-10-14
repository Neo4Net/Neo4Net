/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state.machines
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using DummyMachine = Neo4Net.causalclustering.core.state.machines.dummy.DummyMachine;
	using ReplicatedIdAllocationRequest = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest;
	using ReplicatedIdAllocationStateMachine = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationStateMachine;
	using ReplicatedLockTokenRequest = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedLockTokenStateMachine = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine;
	using ReplicatedTokenRequest = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using ReplicatedTokenStateMachine = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenStateMachine;
	using TokenType = Neo4Net.causalclustering.core.state.machines.token.TokenType;
	using RecoverConsensusLogIndex = Neo4Net.causalclustering.core.state.machines.tx.RecoverConsensusLogIndex;
	using ReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using ReplicatedTransactionStateMachine = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransactionStateMachine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class CoreStateMachinesTest
	{
		private bool InstanceFieldsInitialized = false;

		public CoreStateMachinesTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_coreStateMachines = new CoreStateMachines( _txSM, _labelTokenSM, _relationshipTypeTokenSM, _propertyKeyTokenSM, _lockTokenSM, _idAllocationSM, _dummySM, mock( typeof( LocalDatabase ) ), _recoverConsensusLogIndex );
			when( _relationshipTypeTokenRequest.type() ).thenReturn(TokenType.RELATIONSHIP);
			_verifier = inOrder( _txSM, _labelTokenSM, _relationshipTypeTokenSM, _propertyKeyTokenSM, _lockTokenSM, _idAllocationSM );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowForBatchingOfTransactions()
		 public virtual void ShouldAllowForBatchingOfTransactions()
		 {
			  using ( CommandDispatcher dispatcher = _coreStateMachines.commandDispatcher() )
			  {
					dispatcher.Dispatch( _replicatedTransaction, 0, _callback );
					dispatcher.Dispatch( _replicatedTransaction, 1, _callback );
					dispatcher.Dispatch( _replicatedTransaction, 2, _callback );
			  }

			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 0, _callback );
			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 1, _callback );
			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 2, _callback );
			  _verifier.verify( _txSM ).ensuredApplied();
			  _verifier.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyTransactionBatchAsSoonAsThereIsADifferentKindOfRequestInTheBatch()
		 public virtual void ShouldApplyTransactionBatchAsSoonAsThereIsADifferentKindOfRequestInTheBatch()
		 {
			  using ( CommandDispatcher dispatcher = _coreStateMachines.commandDispatcher() )
			  {
					dispatcher.Dispatch( _replicatedTransaction, 0, _callback );
					dispatcher.Dispatch( _replicatedTransaction, 1, _callback );

					dispatcher.Dispatch( _iAllocationRequest, 2, _callback );

					dispatcher.Dispatch( _replicatedTransaction, 3, _callback );
					dispatcher.Dispatch( _replicatedTransaction, 4, _callback );

					dispatcher.Dispatch( _relationshipTypeTokenRequest, 5, _callback );

					dispatcher.Dispatch( _replicatedTransaction, 6, _callback );
					dispatcher.Dispatch( _replicatedTransaction, 7, _callback );

					dispatcher.Dispatch( _lockTokenRequest, 8, _callback );

					dispatcher.Dispatch( _replicatedTransaction, 9, _callback );
					dispatcher.Dispatch( _replicatedTransaction, 10, _callback );
			  }

			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 0, _callback );
			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 1, _callback );
			  _verifier.verify( _txSM ).ensuredApplied();

			  _verifier.verify( _idAllocationSM ).applyCommand( _iAllocationRequest, 2, _callback );

			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 3, _callback );
			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 4, _callback );
			  _verifier.verify( _txSM ).ensuredApplied();

			  _verifier.verify( _relationshipTypeTokenSM ).applyCommand( _relationshipTypeTokenRequest, 5, _callback );

			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 6, _callback );
			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 7, _callback );
			  _verifier.verify( _txSM ).ensuredApplied();

			  _verifier.verify( _lockTokenSM ).applyCommand( _lockTokenRequest, 8, _callback );

			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 9, _callback );
			  _verifier.verify( _txSM ).applyCommand( _replicatedTransaction, 10, _callback );
			  _verifier.verify( _txSM ).ensuredApplied();

			  _verifier.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnLastAppliedOfAllStateMachines()
		 public virtual void ShouldReturnLastAppliedOfAllStateMachines()
		 {
			  // tx state machines are backed by the same store (the tx log) so they should return the same lastApplied
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: StateMachine<?>[] txSMs = new StateMachine[]{labelTokenSM, relationshipTypeTokenSM, propertyKeyTokenSM, txSM};
			  StateMachine<object>[] txSMs = new StateMachine[]{ _labelTokenSM, _relationshipTypeTokenSM, _propertyKeyTokenSM, _txSM };

			  // these have separate storage
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: StateMachine<?>[] otherSMs = new StateMachine[]{idAllocationSM, lockTokenSM};
			  StateMachine<object>[] otherSMs = new StateMachine[]{ _idAllocationSM, _lockTokenSM };

			  int totalDistinctSMs = otherSMs.Length + 1; // distinct meaning backed by different storage
			  // here we try to order all the distinct state machines in different orders to prove that,
			  // regardless of which one is latest, we still recover the latest applied index
			  for ( long @base = 0; @base < totalDistinctSMs; @base++ )
			  {
					long expected = -1;
					long index = 0;
					long lastAppliedIndex;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (StateMachine<?> sm : otherSMs)
					foreach ( StateMachine<object> sm in otherSMs )
					{
						 lastAppliedIndex = ( @base + index ) % totalDistinctSMs;
						 expected = max( expected, lastAppliedIndex ); // this means that result is ignoring the txSMs
						 when( sm.LastAppliedIndex() ).thenReturn(lastAppliedIndex);
						 index++;
					}

					lastAppliedIndex = ( @base + index ) % totalDistinctSMs; // all the txSMs have the same backing store
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (StateMachine<?> sm : txSMs)
					foreach ( StateMachine<object> sm in txSMs )
					{
						 when( sm.LastAppliedIndex() ).thenReturn(lastAppliedIndex);
					}

					// then
					assertEquals( expected, _coreStateMachines.LastAppliedIndex );
			  }
		 }

		 private readonly ReplicatedTransactionStateMachine _txSM = mock( typeof( ReplicatedTransactionStateMachine ) );
		 private readonly ReplicatedTokenStateMachine _labelTokenSM = mock( typeof( ReplicatedTokenStateMachine ) );
		 private readonly ReplicatedTokenStateMachine _relationshipTypeTokenSM = mock( typeof( ReplicatedTokenStateMachine ) );
		 private readonly ReplicatedTokenStateMachine _propertyKeyTokenSM = mock( typeof( ReplicatedTokenStateMachine ) );
		 private readonly ReplicatedLockTokenStateMachine _lockTokenSM = mock( typeof( ReplicatedLockTokenStateMachine ) );
		 private readonly ReplicatedIdAllocationStateMachine _idAllocationSM = mock( typeof( ReplicatedIdAllocationStateMachine ) );
		 private readonly DummyMachine _dummySM = mock( typeof( DummyMachine ) );
		 private readonly RecoverConsensusLogIndex _recoverConsensusLogIndex = mock( typeof( RecoverConsensusLogIndex ) );

		 private CoreStateMachines _coreStateMachines;

		 private readonly ReplicatedTransaction _replicatedTransaction = mock( typeof( ReplicatedTransaction ) );
		 private readonly ReplicatedIdAllocationRequest _iAllocationRequest = mock( typeof( ReplicatedIdAllocationRequest ) );
		 private readonly ReplicatedTokenRequest _relationshipTypeTokenRequest = mock( typeof( ReplicatedTokenRequest ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final org.neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest lockTokenRequest = mock(org.neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest.class);
		 private readonly ReplicatedLockTokenRequest _lockTokenRequest = mock( typeof( ReplicatedLockTokenRequest ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final System.Action<org.neo4j.causalclustering.core.state.Result> callback = mock(System.Action.class);
		 private readonly System.Action<Result> _callback = mock( typeof( System.Action ) );

		 private InOrder _verifier;
	}

}