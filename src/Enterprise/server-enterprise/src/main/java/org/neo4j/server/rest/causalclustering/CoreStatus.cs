using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.causalclustering
{

	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using DurationSinceLastMessageMonitor = Neo4Net.causalclustering.core.consensus.DurationSinceLastMessageMonitor;
	using NoLeaderFoundException = Neo4Net.causalclustering.core.consensus.NoLeaderFoundException;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using RaftMembershipManager = Neo4Net.causalclustering.core.consensus.membership.RaftMembershipManager;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using CommandIndexTracker = Neo4Net.causalclustering.core.state.machines.id.CommandIndexTracker;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.causalclustering.CausalClusteringService.BASE_PATH;

	internal class CoreStatus : BaseStatus
	{
		 private readonly OutputFormat _output;
		 private readonly CoreGraphDatabase _db;

		 // Dependency resolved
		 private readonly RaftMembershipManager _raftMembershipManager;
		 private readonly DatabaseHealth _databaseHealth;
		 private readonly TopologyService _topologyService;
		 private readonly DurationSinceLastMessageMonitor _raftMessageTimerResetMonitor;
		 private readonly RaftMachine _raftMachine;
		 private readonly CommandIndexTracker _commandIndexTracker;

		 internal CoreStatus( OutputFormat output, CoreGraphDatabase db ) : base( output )
		 {
			  this._output = output;
			  this._db = db;

			  DependencyResolver dependencyResolver = Db.DependencyResolver;
			  this._raftMembershipManager = dependencyResolver.ResolveDependency( typeof( RaftMembershipManager ) );
			  this._databaseHealth = dependencyResolver.ResolveDependency( typeof( DatabaseHealth ) );
			  this._topologyService = dependencyResolver.ResolveDependency( typeof( TopologyService ) );
			  this._raftMachine = dependencyResolver.ResolveDependency( typeof( RaftMachine ) );
			  this._raftMessageTimerResetMonitor = dependencyResolver.ResolveDependency( typeof( DurationSinceLastMessageMonitor ) );
			  _commandIndexTracker = dependencyResolver.ResolveDependency( typeof( CommandIndexTracker ) );
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
			  Role role = _db.Role;
			  return Arrays.asList( Role.FOLLOWER, Role.CANDIDATE ).contains( role ) ? PositiveResponse() : NegativeResponse();
		 }

		 public override Response Writable()
		 {
			  return _db.Role == Role.LEADER ? PositiveResponse() : NegativeResponse();
		 }

		 public override Response Description()
		 {
			  MemberId myself = _topologyService.myself();
			  MemberId leader = Leader;
			  IList<MemberId> votingMembers = new List<MemberId>( _raftMembershipManager.votingMembers() );
			  bool participatingInRaftGroup = votingMembers.Contains( myself ) && Objects.nonNull( leader );

			  long lastAppliedRaftIndex = _commandIndexTracker.AppliedCommandIndex;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.Duration millisSinceLastLeaderMessage;
			  Duration millisSinceLastLeaderMessage;
			  if ( myself.Equals( leader ) )
			  {
					millisSinceLastLeaderMessage = Duration.ofMillis( 0 );
			  }
			  else
			  {
					millisSinceLastLeaderMessage = _raftMessageTimerResetMonitor.durationSinceLastMessage();
			  }

			  return StatusResponse( lastAppliedRaftIndex, participatingInRaftGroup, votingMembers, _databaseHealth.Healthy, myself, leader, millisSinceLastLeaderMessage, true );
		 }

		 private MemberId Leader
		 {
			 get
			 {
				  try
				  {
						return _raftMachine.Leader;
				  }
				  catch ( NoLeaderFoundException )
				  {
						return null;
				  }
			 }
		 }
	}

}