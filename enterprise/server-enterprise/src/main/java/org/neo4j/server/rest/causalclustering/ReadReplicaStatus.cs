using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.causalclustering
{

	using CommandIndexTracker = Org.Neo4j.causalclustering.core.state.machines.id.CommandIndexTracker;
	using RoleInfo = Org.Neo4j.causalclustering.discovery.RoleInfo;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using ReadReplicaGraphDatabase = Org.Neo4j.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.causalclustering.CausalClusteringService.BASE_PATH;

	internal class ReadReplicaStatus : BaseStatus
	{
		 private readonly OutputFormat _output;

		 // Dependency resolved
		 private readonly TopologyService _topologyService;
		 private readonly DatabaseHealth _dbHealth;
		 private readonly CommandIndexTracker _commandIndexTracker;

		 internal ReadReplicaStatus( OutputFormat output, ReadReplicaGraphDatabase db ) : base( output )
		 {
			  this._output = output;

			  DependencyResolver dependencyResolver = Db.DependencyResolver;
			  this._commandIndexTracker = dependencyResolver.ResolveDependency( typeof( CommandIndexTracker ) );
			  this._topologyService = dependencyResolver.ResolveDependency( typeof( TopologyService ) );
			  this._dbHealth = dependencyResolver.ResolveDependency( typeof( DatabaseHealth ) );
		 }

		 public override Response Discover()
		 {
			  return _output.ok( new CausalClusteringDiscovery( BASE_PATH ) );
		 }

		 public override Response Available()
		 {
			  return PositiveResponse();
		 }

		 public override Response Readonly()
		 {
			  return PositiveResponse();
		 }

		 public override Response Writable()
		 {
			  return NegativeResponse();
		 }

		 public override Response Description()
		 {
			  ICollection<MemberId> votingMembers = _topologyService.allCoreRoles().Keys;
			  bool isHealthy = _dbHealth.Healthy;
			  MemberId memberId = _topologyService.myself();
			  MemberId leader = _topologyService.allCoreRoles().Keys.Where(member => RoleInfo.LEADER.Equals(_topologyService.allCoreRoles()[member])).First().orElse(null);
			  long lastAppliedRaftIndex = _commandIndexTracker.AppliedCommandIndex;
			  // leader message duration is meaningless for replicas since communication is not guaranteed with leader and transactions are streamed periodically
			  Duration millisSinceLastLeaderMessage = null;
			  return StatusResponse( lastAppliedRaftIndex, false, votingMembers, isHealthy, memberId, leader, millisSinceLastLeaderMessage, false );
		 }
	}

}