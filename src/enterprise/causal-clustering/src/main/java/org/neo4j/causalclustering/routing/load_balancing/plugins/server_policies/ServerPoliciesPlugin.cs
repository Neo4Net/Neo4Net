﻿using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using NoLeaderFoundException = Neo4Net.causalclustering.core.consensus.NoLeaderFoundException;
	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using ReadReplicaTopology = Neo4Net.causalclustering.discovery.ReadReplicaTopology;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;
	using Service = Neo4Net.Helpers.Service;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.Util.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.Util.extractBoltAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies.FilteringPolicyLoader.load;

	/// <summary>
	/// The server policies plugin defines policies on the server-side which
	/// can be bound to by a client by supplying a appropriately formed context.
	/// 
	/// An example would be to define different policies for different regions.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(LoadBalancingPlugin.class) public class ServerPoliciesPlugin implements org.Neo4Net.causalclustering.routing.load_balancing.LoadBalancingPlugin
	public class ServerPoliciesPlugin : LoadBalancingPlugin
	{
		 public const string PLUGIN_NAME = "server_policies";

		 private TopologyService _topologyService;
		 private LeaderLocator _leaderLocator;
		 private long? _timeToLive;
		 private bool _allowReadsOnFollowers;
		 private Policies _policies;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validate(org.Neo4Net.kernel.configuration.Config config, org.Neo4Net.logging.Log log) throws org.Neo4Net.graphdb.config.InvalidSettingException
		 public override void Validate( Config config, Log log )
		 {
			  try
			  {
					load( config, PLUGIN_NAME, log );
			  }
			  catch ( InvalidFilterSpecification e )
			  {
					throw new InvalidSettingException( "Invalid filter specification", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init(org.Neo4Net.causalclustering.discovery.TopologyService topologyService, org.Neo4Net.causalclustering.core.consensus.LeaderLocator leaderLocator, org.Neo4Net.logging.LogProvider logProvider, org.Neo4Net.kernel.configuration.Config config) throws InvalidFilterSpecification
		 public override void Init( TopologyService topologyService, LeaderLocator leaderLocator, LogProvider logProvider, Config config )
		 {
			  this._topologyService = topologyService;
			  this._leaderLocator = leaderLocator;
			  this._timeToLive = config.Get( CausalClusteringSettings.cluster_routing_ttl ).toMillis();
			  this._allowReadsOnFollowers = config.Get( CausalClusteringSettings.cluster_allow_reads_on_followers );
			  this._policies = load( config, PLUGIN_NAME, logProvider.getLog( this.GetType() ) );
		 }

		 public override string PluginName()
		 {
			  return PLUGIN_NAME;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result run(java.util.Map<String,String> context) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public override Neo4Net.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result Run( IDictionary<string, string> context )
		 {
			  Policy policy = _policies.selectFor( context );

			  CoreTopology coreTopology = _topologyService.localCoreServers();
			  ReadReplicaTopology rrTopology = _topologyService.localReadReplicas();

			  return new LoadBalancingResult( RouteEndpoints( coreTopology ), WriteEndpoints( coreTopology ), ReadEndpoints( coreTopology, rrTopology, policy ), _timeToLive.Value );
		 }

		 private IList<Endpoint> RouteEndpoints( CoreTopology cores )
		 {
			  return cores.Members().Values.Select(extractBoltAddress()).Select(Endpoint.route).ToList();
		 }

		 private IList<Endpoint> WriteEndpoints( CoreTopology cores )
		 {

			  MemberId leader;
			  try
			  {
					leader = _leaderLocator.Leader;
			  }
			  catch ( NoLeaderFoundException )
			  {
					return emptyList();
			  }

			  Optional<Endpoint> endPoint = cores.find( leader ).map( extractBoltAddress() ).map(Endpoint.write);

			  return new IList<Endpoint> { endPoint };
		 }

		 private IList<Endpoint> ReadEndpoints( CoreTopology coreTopology, ReadReplicaTopology rrTopology, Policy policy )
		 {

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<ServerInfo> possibleReaders = rrTopology.Members().SetOfKeyValuePairs().Select(entry => new ServerInfo(entry.Value.connectors().boltAddress(), entry.Key, entry.Value.groups())).collect(Collectors.toSet());

			  if ( _allowReadsOnFollowers || possibleReaders.Count == 0 )
			  {
					ISet<MemberId> validCores = coreTopology.Members().Keys;
					try
					{
						 MemberId leader = _leaderLocator.Leader;
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
						 validCores = validCores.Where( memberId => !memberId.Equals( leader ) ).collect( Collectors.toSet() );
					}
					catch ( NoLeaderFoundException )
					{
						 // we might end up using the leader for reading during this ttl, should be fine in general
					}

					foreach ( MemberId validCore in validCores )
					{
						 Optional<CoreServerInfo> coreServerInfo = coreTopology.find( validCore );
						 coreServerInfo.ifPresent( coreServerInfo1 => possibleReaders.Add( new ServerInfo( coreServerInfo1.connectors().boltAddress(), validCore, coreServerInfo1.groups() ) ) );
					}
			  }

			  ISet<ServerInfo> readers = policy.Apply( possibleReaders );
			  return readers.Select( r => Endpoint.read( r.boltAddress() ) ).ToList();
		 }
	}

}