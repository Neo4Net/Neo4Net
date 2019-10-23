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
namespace Neo4Net.causalclustering.routing.load_balancing.procedure
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using NoLeaderFoundException = Neo4Net.causalclustering.core.consensus.NoLeaderFoundException;
	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.Collections;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using ProcedureSignature = Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.cluster_allow_reads_on_followers;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.Util.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.Util.extractBoltAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ParameterNames.CONTEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ParameterNames.SERVERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ParameterNames.TTL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.procedure.ProcedureNames.GET_SERVERS_V2;

	/// <summary>
	/// Returns endpoints and their capabilities.
	/// 
	/// GetServersV2 extends upon V1 by allowing a client context consisting of
	/// key-value pairs to be supplied to and used by the concrete load
	/// balancing strategies.
	/// </summary>
	public class GetServersProcedureForSingleDC : CallableProcedure
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_procedureSignature = ProcedureSignature.procedureSignature( GET_SERVERS_V2.fullyQualifiedProcedureName() ).@in(CONTEXT.parameterName(), Neo4NetTypes.NTMap).@out(TTL.parameterName(), Neo4NetTypes.NTInteger).@out(SERVERS.parameterName(), Neo4NetTypes.NTList(Neo4NetTypes.NTMap)).description(_description).build();
		}

		 private readonly string _description = "Returns cluster endpoints and their capabilities for single data center setup.";

		 private ProcedureSignature _procedureSignature;

		 private readonly TopologyService _topologyService;
		 private readonly LeaderLocator _leaderLocator;
		 private readonly Config _config;
		 private readonly Log _log;

		 public GetServersProcedureForSingleDC( TopologyService topologyService, LeaderLocator leaderLocator, Config config, LogProvider logProvider )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._topologyService = topologyService;
			  this._leaderLocator = leaderLocator;
			  this._config = config;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override ProcedureSignature Signature()
		 {
			  return _procedureSignature;
		 }

		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
			  IList<Endpoint> routeEndpoints = routeEndpoints();
			  IList<Endpoint> writeEndpoints = writeEndpoints();
			  IList<Endpoint> readEndpoints = readEndpoints();

			  return RawIterator.of<object[], ProcedureException>( ResultFormatV1.Build( new LoadBalancingResult( routeEndpoints, writeEndpoints, readEndpoints, _config.get( CausalClusteringSettings.cluster_routing_ttl ).toMillis() ) ) );
		 }

		 private Optional<AdvertisedSocketAddress> LeaderBoltAddress()
		 {
			  MemberId leader;
			  try
			  {
					leader = _leaderLocator.Leader;
			  }
			  catch ( NoLeaderFoundException )
			  {
					_log.debug( "No leader server found. This can happen during a leader switch. No write end points available" );
					return null;
			  }

			  return _topologyService.localCoreServers().find(leader).map(extractBoltAddress());
		 }

		 private IList<Endpoint> RouteEndpoints()
		 {
			  Stream<AdvertisedSocketAddress> routers = _topologyService.localCoreServers().members().Values.Select(extractBoltAddress());
			  IList<Endpoint> routeEndpoints = routers.map( Endpoint.route ).collect( toList() );
			  Collections.shuffle( routeEndpoints );
			  return routeEndpoints;
		 }

		 private IList<Endpoint> WriteEndpoints()
		 {
			  return new IList<Endpoint> { LeaderBoltAddress().map(Endpoint.write) };
		 }

		 private IList<Endpoint> ReadEndpoints()
		 {
			  IList<AdvertisedSocketAddress> readReplicas = _topologyService.localReadReplicas().allMemberInfo().Select(extractBoltAddress()).ToList();
			  bool addFollowers = readReplicas.Count == 0 || _config.get( cluster_allow_reads_on_followers );
			  Stream<AdvertisedSocketAddress> readCore = addFollowers ? CoreReadEndPoints() : Stream.empty();
			  IList<Endpoint> readEndPoints = concat( readReplicas.stream(), readCore ).map(Endpoint.read).collect(toList());
			  Collections.shuffle( readEndPoints );
			  return readEndPoints;
		 }

		 private Stream<AdvertisedSocketAddress> CoreReadEndPoints()
		 {
			  Optional<AdvertisedSocketAddress> leader = LeaderBoltAddress();
			  ICollection<CoreServerInfo> coreServerInfo = _topologyService.localCoreServers().members().Values;
			  Stream<AdvertisedSocketAddress> boltAddresses = _topologyService.localCoreServers().members().Values.Select(extractBoltAddress());

			  // if the leader is present and it is not alone filter it out from the read end points
			  if ( leader.Present && coreServerInfo.Count > 1 )
			  {
					AdvertisedSocketAddress advertisedSocketAddress = leader.get();
					return boltAddresses.filter( address => !advertisedSocketAddress.Equals( address ) );
			  }

			  // if there is only the leader return it as read end point
			  // or if we cannot locate the leader return all cores as read end points
			  return boltAddresses;
		 }
	}

}