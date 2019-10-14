using System.Diagnostics;

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
namespace Neo4Net.causalclustering.core.consensus.roles.follower
{

	/// <summary>
	/// Things the leader thinks it knows about a follower.
	/// </summary>
	public class FollowerState
	{
		 // We know that the follower agrees with our (leader) log up until this index. Only updated by the leader when:
		 // * increased when it receives a successful AppendEntries.Response
		 private readonly long _matchIndex;

		 public FollowerState() : this(-1)
		 {
		 }

		 private FollowerState( long matchIndex )
		 {
			  Debug.Assert( matchIndex >= -1, format( "Match index can never be less than -1. Was %d", matchIndex ) );
			  this._matchIndex = matchIndex;
		 }

		 public virtual long MatchIndex
		 {
			 get
			 {
				  return _matchIndex;
			 }
		 }

		 public virtual FollowerState OnSuccessResponse( long newMatchIndex )
		 {
			  return new FollowerState( newMatchIndex );
		 }

		 public override string ToString()
		 {
			  return format( "State{matchIndex=%d}", _matchIndex );
		 }
	}

}