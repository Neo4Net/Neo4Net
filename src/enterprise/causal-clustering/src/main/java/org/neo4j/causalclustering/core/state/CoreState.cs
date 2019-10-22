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

	using CoreStateMachines = Neo4Net.causalclustering.core.state.machines.CoreStateMachines;
	using CoreSnapshot = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot;
	using CoreStateType = Neo4Net.causalclustering.core.state.snapshot.CoreStateType;
	using Neo4Net.causalclustering.core.state.storage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;

	public class CoreState
	{
		 private CoreStateMachines _coreStateMachines;
		 private readonly SessionTracker _sessionTracker;
		 private readonly StateStorage<long> _lastFlushedStorage;

		 public CoreState( CoreStateMachines coreStateMachines, SessionTracker sessionTracker, StateStorage<long> lastFlushedStorage )
		 {
			  this._coreStateMachines = coreStateMachines;
			  this._sessionTracker = sessionTracker;
			  this._lastFlushedStorage = lastFlushedStorage;
		 }

		 internal virtual void AugmentSnapshot( CoreSnapshot coreSnapshot )
		 {
			 lock ( this )
			 {
				  _coreStateMachines.addSnapshots( coreSnapshot );
				  coreSnapshot.Add( CoreStateType.SESSION_TRACKER, _sessionTracker.snapshot() );
			 }
		 }

		 internal virtual void InstallSnapshot( CoreSnapshot coreSnapshot )
		 {
			 lock ( this )
			 {
				  _coreStateMachines.installSnapshots( coreSnapshot );
				  _sessionTracker.installSnapshot( coreSnapshot.Get( CoreStateType.SESSION_TRACKER ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized void flush(long lastApplied) throws java.io.IOException
		 internal virtual void Flush( long lastApplied )
		 {
			 lock ( this )
			 {
				  _coreStateMachines.flush();
				  _sessionTracker.flush();
				  _lastFlushedStorage.persistStoreData( lastApplied );
			 }
		 }

		 public virtual CommandDispatcher CommandDispatcher()
		 {
			  return _coreStateMachines.commandDispatcher();
		 }

		 internal virtual long LastAppliedIndex
		 {
			 get
			 {
				  return max( _coreStateMachines.LastAppliedIndex, _sessionTracker.LastAppliedIndex );
			 }
		 }

		 internal virtual long LastFlushed
		 {
			 get
			 {
				  return _lastFlushedStorage.InitialState;
			 }
		 }
	}

}