﻿/*
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
namespace Org.Neo4j.causalclustering.core
{
	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using WriteOperationsNotAllowedException = Org.Neo4j.Graphdb.security.WriteOperationsNotAllowedException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using AccessCapability = Org.Neo4j.Kernel.impl.factory.AccessCapability;

	public class LeaderCanWrite : AccessCapability
	{
		 private RaftMachine _raftMachine;
		 public static readonly string NotLeaderErrorMsg = "No write operations are allowed directly on this database. Writes must pass through the leader. " +
							  "The role of this server is: %s";

		 internal LeaderCanWrite( RaftMachine raftMachine )
		 {
			  this._raftMachine = raftMachine;
		 }

		 public override void AssertCanWrite()
		 {
			  Role currentRole = _raftMachine.currentRole();
			  if ( !currentRole.Equals( Role.LEADER ) )
			  {
					throw new WriteOperationsNotAllowedException( format( NotLeaderErrorMsg, currentRole ), Org.Neo4j.Kernel.Api.Exceptions.Status_Cluster.NotALeader );
			  }
		 }
	}

}