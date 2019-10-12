/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.causalclustering.core.state.machines.locks
{

	using Neo4Net.causalclustering.core.state.machines;
	using Neo4Net.causalclustering.core.state.storage;

	/// <summary>
	/// Listens for <seealso cref="ReplicatedLockTokenRequest"/>. Keeps track of the current holder of the replicated token,
	/// which is identified by a monotonically increasing id, and an owning member.
	/// </summary>
	public class ReplicatedLockTokenStateMachine : StateMachine<ReplicatedLockTokenRequest>
	{
		 private readonly StateStorage<ReplicatedLockTokenState> _storage;
		 private ReplicatedLockTokenState _state;

		 public ReplicatedLockTokenStateMachine( StateStorage<ReplicatedLockTokenState> storage )
		 {
			  this._storage = storage;
		 }

		 public override void ApplyCommand( ReplicatedLockTokenRequest tokenRequest, long commandIndex, System.Action<Result> callback )
		 {
			 lock ( this )
			 {
				  if ( commandIndex <= State().ordinal() )
				  {
						return;
				  }
      
				  bool requestAccepted = tokenRequest.Id() == LockToken.nextCandidateId(CurrentToken().id());
				  if ( requestAccepted )
				  {
						State().set(tokenRequest, commandIndex);
				  }
      
				  callback( Result.of( requestAccepted ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void flush() throws java.io.IOException
		 public override void Flush()
		 {
			 lock ( this )
			 {
				  _storage.persistStoreData( State() );
			 }
		 }

		 public override long LastAppliedIndex()
		 {
			  return State().ordinal();
		 }

		 private ReplicatedLockTokenState State()
		 {
			  if ( _state == null )
			  {
					_state = _storage.InitialState;
			  }
			  return _state;
		 }

		 public virtual ReplicatedLockTokenState Snapshot()
		 {
			 lock ( this )
			 {
				  return State().newInstance();
			 }
		 }

		 public virtual void InstallSnapshot( ReplicatedLockTokenState snapshot )
		 {
			 lock ( this )
			 {
				  _state = snapshot;
			 }
		 }

		 /// <returns> The currently valid token. </returns>
		 public virtual ReplicatedLockTokenRequest CurrentToken()
		 {
			 lock ( this )
			 {
				  return State().get();
			 }
		 }
	}

}