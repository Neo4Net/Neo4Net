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
namespace Org.Neo4j.causalclustering.discovery
{
	using GroupProperty = com.hazelcast.spi.properties.GroupProperty;

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public class HazelcastDiscoveryServiceFactory : DiscoveryServiceFactory
	{
		 public override CoreTopologyService CoreTopologyService( Config config, MemberId myself, JobScheduler jobScheduler, LogProvider logProvider, LogProvider userLogProvider, RemoteMembersResolver remoteMembersResolver, TopologyServiceRetryStrategy topologyServiceRetryStrategy, Monitors monitors )
		 {
			  ConfigureHazelcast( config, logProvider );
			  return new HazelcastCoreTopologyService( config, myself, jobScheduler, logProvider, userLogProvider, remoteMembersResolver, topologyServiceRetryStrategy, monitors );
		 }

		 public override TopologyService ReadReplicaTopologyService( Config config, LogProvider logProvider, JobScheduler jobScheduler, MemberId myself, RemoteMembersResolver remoteMembersResolver, TopologyServiceRetryStrategy topologyServiceRetryStrategy )
		 {
			  ConfigureHazelcast( config, logProvider );
			  return new HazelcastClient( new HazelcastClientConnector( config, logProvider, remoteMembersResolver ), jobScheduler, logProvider, config, myself );
		 }

		 protected internal static void ConfigureHazelcast( Config config, LogProvider logProvider )
		 {
			  GroupProperty.WAIT_SECONDS_BEFORE_JOIN.SystemProperty = "1";
			  GroupProperty.PHONE_HOME_ENABLED.SystemProperty = "false";
			  GroupProperty.SOCKET_BIND_ANY.SystemProperty = "false";
			  GroupProperty.SHUTDOWNHOOK_ENABLED.SystemProperty = "false";

			  string licenseKey = config.Get( CausalClusteringSettings.hazelcast_license_key );
			  if ( !string.ReferenceEquals( licenseKey, null ) )
			  {
					GroupProperty.ENTERPRISE_LICENSE_KEY.SystemProperty = licenseKey;
			  }

			  // Make hazelcast quiet
			  if ( config.Get( CausalClusteringSettings.disable_middleware_logging ) )
			  {
					// This is clunky, but the documented programmatic way doesn't seem to work
					GroupProperty.LOGGING_TYPE.SystemProperty = "none";
			  }
			  else
			  {
					HazelcastLogging.Enable( logProvider, new HazelcastLogLevel( config ) );
			  }
		 }

		 private class HazelcastLogLevel : Level
		 {
			  internal HazelcastLogLevel( Config config ) : base( "HAZELCAST", config.Get( CausalClusteringSettings.middleware_logging_level ) )
			  {
			  }
		 }
	}

}