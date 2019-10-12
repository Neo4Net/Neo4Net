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
namespace Org.Neo4j.causalclustering.core.state.machines.id
{

	using Org.Neo4j.causalclustering.core.state.machines;
	using Org.Neo4j.causalclustering.core.state.storage;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;

	/// <summary>
	/// Keeps track of global id allocations for all members.
	/// </summary>
	public class ReplicatedIdAllocationStateMachine : StateMachine<ReplicatedIdAllocationRequest>
	{
		 private readonly StateStorage<IdAllocationState> _storage;
		 private IdAllocationState _state;

		 public ReplicatedIdAllocationStateMachine( StateStorage<IdAllocationState> storage )
		 {
			  this._storage = storage;
		 }

		 public override void ApplyCommand( ReplicatedIdAllocationRequest request, long commandIndex, System.Action<Result> callback )
		 {
			 lock ( this )
			 {
				  if ( commandIndex <= State().logIndex() )
				  {
						return;
				  }
      
				  State().logIndex(commandIndex);
      
				  IdType idType = request.IdType();
      
				  bool requestAccepted = request.IdRangeStart() == FirstUnallocated(idType);
				  if ( requestAccepted )
				  {
						State().firstUnallocated(idType, request.IdRangeStart() + request.IdRangeLength());
				  }
      
				  callback( Result.of( requestAccepted ) );
			 }
		 }

		 internal virtual long FirstUnallocated( IdType idType )
		 {
			 lock ( this )
			 {
				  return State().firstUnallocated(idType);
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  _storage.persistStoreData( State() );
		 }

		 public override long LastAppliedIndex()
		 {
			  return State().logIndex();
		 }

		 private IdAllocationState State()
		 {
			  if ( _state == null )
			  {
					_state = _storage.InitialState;
			  }
			  return _state;
		 }

		 public virtual IdAllocationState Snapshot()
		 {
			 lock ( this )
			 {
				  return State().newInstance();
			 }
		 }

		 public virtual void InstallSnapshot( IdAllocationState snapshot )
		 {
			 lock ( this )
			 {
				  _state = snapshot;
			 }
		 }
	}

}