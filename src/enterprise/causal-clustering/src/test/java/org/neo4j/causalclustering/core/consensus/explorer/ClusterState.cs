﻿using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.causalclustering.core.consensus.explorer
{

	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using RaftState = Neo4Net.causalclustering.core.consensus.state.RaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.state.RaftStateBuilder.raftState;

	public class ClusterState
	{
		 public readonly IDictionary<MemberId, Role> Roles;
		 public readonly IDictionary<MemberId, ComparableRaftState> States;
		 public readonly IDictionary<MemberId, LinkedList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>> Queues;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClusterState(java.util.Set<org.neo4j.causalclustering.identity.MemberId> members) throws java.io.IOException
		 public ClusterState( ISet<MemberId> members )
		 {
			  this.Roles = new Dictionary<MemberId, Role>();
			  this.States = new Dictionary<MemberId, ComparableRaftState>();
			  this.Queues = new Dictionary<MemberId, LinkedList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>>();

			  foreach ( MemberId member in members )
			  {
					Roles[member] = Role.FOLLOWER;
					RaftState memberState = raftState().myself(member).votingMembers(members).build();
					States[member] = new ComparableRaftState( memberState );
					Queues[member] = new LinkedList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>();
			  }
		 }

		 public ClusterState( ClusterState original )
		 {
			  this.Roles = new Dictionary<MemberId, Role>( original.Roles );
			  this.States = new Dictionary<MemberId, ComparableRaftState>( original.States );
			  this.Queues = new Dictionary<MemberId, LinkedList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>>( original.Queues );
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
			  ClusterState that = ( ClusterState ) o;
			  return Objects.Equals( Roles, that.Roles ) && Objects.Equals( States, that.States ) && Objects.Equals( Queues, that.Queues );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( Roles, States, Queues );
		 }

		 public override string ToString()
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( MemberId member in Roles.Keys )
			  {
					builder.Append( member ).Append( " : " ).Append( Roles[member] ).Append( "\n" );
					builder.Append( "  state: " ).Append( States[member] ).Append( "\n" );
					builder.Append( "  queue: " ).Append( Queues[member] ).Append( "\n" );
			  }
			  return builder.ToString();
		 }
	}

}