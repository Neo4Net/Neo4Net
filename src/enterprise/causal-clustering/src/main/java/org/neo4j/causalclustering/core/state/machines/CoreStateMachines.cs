using System.Diagnostics;

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
namespace Neo4Net.causalclustering.core.state.machines
{

	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using DummyMachine = Neo4Net.causalclustering.core.state.machines.dummy.DummyMachine;
	using DummyRequest = Neo4Net.causalclustering.core.state.machines.dummy.DummyRequest;
	using ReplicatedIdAllocationRequest = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest;
	using ReplicatedIdAllocationStateMachine = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationStateMachine;
	using ReplicatedLockTokenRequest = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedLockTokenStateMachine = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine;
	using ReplicatedTokenRequest = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using ReplicatedTokenStateMachine = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenStateMachine;
	using RecoverConsensusLogIndex = Neo4Net.causalclustering.core.state.machines.tx.RecoverConsensusLogIndex;
	using ReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using ReplicatedTransactionStateMachine = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransactionStateMachine;
	using CoreSnapshot = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot;
	using CoreStateType = Neo4Net.causalclustering.core.state.snapshot.CoreStateType;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	public class CoreStateMachines
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_dispatcher = new StateMachineCommandDispatcher( this );
		}

		 private readonly ReplicatedTransactionStateMachine _replicatedTxStateMachine;

		 private readonly ReplicatedTokenStateMachine _labelTokenStateMachine;
		 private readonly ReplicatedTokenStateMachine _relationshipTypeTokenStateMachine;
		 private readonly ReplicatedTokenStateMachine _propertyKeyTokenStateMachine;

		 private readonly ReplicatedLockTokenStateMachine _replicatedLockTokenStateMachine;
		 private readonly ReplicatedIdAllocationStateMachine _idAllocationStateMachine;

		 private readonly DummyMachine _benchmarkMachine;

		 private readonly LocalDatabase _localDatabase;
		 private readonly RecoverConsensusLogIndex _consensusLogIndexRecovery;

		 private CommandDispatcher _dispatcher;
		 private volatile bool _runningBatch;

		 internal CoreStateMachines( ReplicatedTransactionStateMachine replicatedTxStateMachine, ReplicatedTokenStateMachine labelTokenStateMachine, ReplicatedTokenStateMachine relationshipTypeTokenStateMachine, ReplicatedTokenStateMachine propertyKeyTokenStateMachine, ReplicatedLockTokenStateMachine replicatedLockTokenStateMachine, ReplicatedIdAllocationStateMachine idAllocationStateMachine, DummyMachine benchmarkMachine, LocalDatabase localDatabase, RecoverConsensusLogIndex consensusLogIndexRecovery )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._replicatedTxStateMachine = replicatedTxStateMachine;
			  this._labelTokenStateMachine = labelTokenStateMachine;
			  this._relationshipTypeTokenStateMachine = relationshipTypeTokenStateMachine;
			  this._propertyKeyTokenStateMachine = propertyKeyTokenStateMachine;
			  this._replicatedLockTokenStateMachine = replicatedLockTokenStateMachine;
			  this._idAllocationStateMachine = idAllocationStateMachine;
			  this._benchmarkMachine = benchmarkMachine;
			  this._localDatabase = localDatabase;
			  this._consensusLogIndexRecovery = consensusLogIndexRecovery;
		 }

		 public virtual CommandDispatcher CommandDispatcher()
		 {
			  _localDatabase.assertHealthy( typeof( System.InvalidOperationException ) );
			  Debug.Assert( !_runningBatch );
			  _runningBatch = true;
			  return _dispatcher;
		 }

		 public virtual long LastAppliedIndex
		 {
			 get
			 {
				  long lastAppliedLockTokenIndex = _replicatedLockTokenStateMachine.lastAppliedIndex();
				  long lastAppliedIdAllocationIndex = _idAllocationStateMachine.lastAppliedIndex();
				  return max( lastAppliedLockTokenIndex, lastAppliedIdAllocationIndex );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public virtual void Flush()
		 {
			  Debug.Assert( !_runningBatch );

			  _replicatedTxStateMachine.flush();

			  _labelTokenStateMachine.flush();
			  _relationshipTypeTokenStateMachine.flush();
			  _propertyKeyTokenStateMachine.flush();

			  _replicatedLockTokenStateMachine.flush();
			  _idAllocationStateMachine.flush();
		 }

		 public virtual void AddSnapshots( CoreSnapshot coreSnapshot )
		 {
			  Debug.Assert( !_runningBatch );

			  coreSnapshot.Add( CoreStateType.ID_ALLOCATION, _idAllocationStateMachine.snapshot() );
			  coreSnapshot.Add( CoreStateType.LOCK_TOKEN, _replicatedLockTokenStateMachine.snapshot() );
			  // transactions and tokens live in the store
		 }

		 public virtual void InstallSnapshots( CoreSnapshot coreSnapshot )
		 {
			  Debug.Assert( !_runningBatch );

			  _idAllocationStateMachine.installSnapshot( coreSnapshot.Get( CoreStateType.ID_ALLOCATION ) );
			  _replicatedLockTokenStateMachine.installSnapshot( coreSnapshot.Get( CoreStateType.LOCK_TOKEN ) );
			  // transactions and tokens live in the store
		 }

		 public virtual void InstallCommitProcess( TransactionCommitProcess localCommit )
		 {
			  Debug.Assert( !_runningBatch );
			  long lastAppliedIndex = _consensusLogIndexRecovery.findLastAppliedIndex();

			  _replicatedTxStateMachine.installCommitProcess( localCommit, lastAppliedIndex );

			  _labelTokenStateMachine.installCommitProcess( localCommit, lastAppliedIndex );
			  _relationshipTypeTokenStateMachine.installCommitProcess( localCommit, lastAppliedIndex );
			  _propertyKeyTokenStateMachine.installCommitProcess( localCommit, lastAppliedIndex );
		 }

		 private class StateMachineCommandDispatcher : CommandDispatcher
		 {
			 private readonly CoreStateMachines _outerInstance;

			 public StateMachineCommandDispatcher( CoreStateMachines outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Dispatch( ReplicatedTransaction transaction, long commandIndex, System.Action<Result> callback )
			  {
					outerInstance.replicatedTxStateMachine.ApplyCommand( transaction, commandIndex, callback );
			  }

			  public override void Dispatch( ReplicatedIdAllocationRequest idRequest, long commandIndex, System.Action<Result> callback )
			  {
					outerInstance.replicatedTxStateMachine.EnsuredApplied();
					outerInstance.idAllocationStateMachine.ApplyCommand( idRequest, commandIndex, callback );
			  }

			  public override void Dispatch( ReplicatedTokenRequest tokenRequest, long commandIndex, System.Action<Result> callback )
			  {
					outerInstance.replicatedTxStateMachine.EnsuredApplied();
					switch ( tokenRequest.Type() )
					{
					case PROPERTY:
						 outerInstance.propertyKeyTokenStateMachine.ApplyCommand( tokenRequest, commandIndex, callback );
						 break;
					case RELATIONSHIP:
						 outerInstance.relationshipTypeTokenStateMachine.ApplyCommand( tokenRequest, commandIndex, callback );
						 break;
					case LABEL:
						 outerInstance.labelTokenStateMachine.ApplyCommand( tokenRequest, commandIndex, callback );
						 break;
					default:
						 throw new System.InvalidOperationException();
					}
			  }

			  public override void Dispatch( ReplicatedLockTokenRequest lockRequest, long commandIndex, System.Action<Result> callback )
			  {
					outerInstance.replicatedTxStateMachine.EnsuredApplied();
					outerInstance.replicatedLockTokenStateMachine.ApplyCommand( lockRequest, commandIndex, callback );
			  }

			  public override void Dispatch( DummyRequest dummyRequest, long commandIndex, System.Action<Result> callback )
			  {
					outerInstance.benchmarkMachine.ApplyCommand( dummyRequest, commandIndex, callback );
			  }

			  public override void Close()
			  {
					outerInstance.runningBatch = false;
					outerInstance.replicatedTxStateMachine.EnsuredApplied();
			  }
		 }
	}

}