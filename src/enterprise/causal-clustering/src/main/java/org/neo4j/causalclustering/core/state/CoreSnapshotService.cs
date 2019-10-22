using System.Threading;

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
namespace Neo4Net.causalclustering.core.state
{

	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using CoreSnapshot = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot;
	using CoreStateType = Neo4Net.causalclustering.core.state.snapshot.CoreStateType;

	public class CoreSnapshotService
	{
		 private const string OPERATION_NAME = "snapshot request";

		 private readonly CommandApplicationProcess _applicationProcess;
		 private readonly CoreState _coreState;
		 private readonly RaftLog _raftLog;
		 private readonly RaftMachine _raftMachine;

		 public CoreSnapshotService( CommandApplicationProcess applicationProcess, CoreState coreState, RaftLog raftLog, RaftMachine raftMachine )
		 {
			  this._applicationProcess = applicationProcess;
			  this._coreState = coreState;
			  this._raftLog = raftLog;
			  this._raftMachine = raftMachine;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized org.Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot snapshot() throws Exception
		 public virtual CoreSnapshot Snapshot()
		 {
			 lock ( this )
			 {
				  _applicationProcess.pauseApplier( OPERATION_NAME );
				  try
				  {
						long lastApplied = _applicationProcess.lastApplied();
      
						long prevTerm = _raftLog.readEntryTerm( lastApplied );
						CoreSnapshot coreSnapshot = new CoreSnapshot( lastApplied, prevTerm );
      
						_coreState.augmentSnapshot( coreSnapshot );
						coreSnapshot.Add( CoreStateType.RAFT_CORE_STATE, _raftMachine.coreState() );
      
						return coreSnapshot;
				  }
				  finally
				  {
						_applicationProcess.resumeApplier( OPERATION_NAME );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void installSnapshot(org.Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot coreSnapshot) throws java.io.IOException
		 public virtual void InstallSnapshot( CoreSnapshot coreSnapshot )
		 {
			 lock ( this )
			 {
				  long snapshotPrevIndex = coreSnapshot.PrevIndex();
				  _raftLog.skip( snapshotPrevIndex, coreSnapshot.PrevTerm() );
      
				  _coreState.installSnapshot( coreSnapshot );
				  _raftMachine.installCoreState( coreSnapshot.Get( CoreStateType.RAFT_CORE_STATE ) );
				  _coreState.flush( snapshotPrevIndex );
      
				  _applicationProcess.installSnapshot( coreSnapshot );
				  Monitor.PulseAll( this );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized void awaitState() throws InterruptedException
		 internal virtual void AwaitState()
		 {
			 lock ( this )
			 {
				  while ( _raftMachine.state().appendIndex() < 0 )
				  {
						Monitor.Wait( this );
				  }
			 }
		 }
	}

}