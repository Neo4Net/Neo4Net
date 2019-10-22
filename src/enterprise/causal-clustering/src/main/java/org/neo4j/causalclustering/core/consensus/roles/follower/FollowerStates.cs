using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.roles.follower
{

	/// <summary>
	/// This presents a read only view over the map of members to their states. Instances that are not present
	/// in the map will have the default FollowerState returned. </summary>
	/// @param <MEMBER> The type of member id </param>
	public class FollowerStates<MEMBER>
	{
		 private readonly IDictionary<MEMBER, FollowerState> _states;

		 public FollowerStates( FollowerStates<MEMBER> original, MEMBER updatedMember, FollowerState updatedState )
		 {
			  this._states = new Dictionary<MEMBER, FollowerState>( original._states );
			  _states[updatedMember] = updatedState;
		 }

		 public FollowerStates()
		 {
			  _states = new Dictionary<MEMBER, FollowerState>();
		 }

		 public virtual FollowerState Get( MEMBER member )
		 {
			  FollowerState result = _states[member];
			  if ( result == null )
			  {
					result = new FollowerState();
			  }
			  return result;
		 }

		 public override string ToString()
		 {
			  return format( "FollowerStates%s", _states );
		 }

		 public virtual int Size()
		 {
			  return _states.Count;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  FollowerStates that = ( FollowerStates ) o;

			  return !( _states != null ?!_states.Equals( that._states ) : that._states != null );
		 }

		 public override int GetHashCode()
		 {
			  return _states != null ? _states.GetHashCode() : 0;
		 }

		 public virtual FollowerStates<MEMBER> OnSuccessResponse( MEMBER member, long newMatchIndex )
		 {
			  return new FollowerStates<MEMBER>( this, member, Get( member ).onSuccessResponse( newMatchIndex ) );
		 }
	}

}