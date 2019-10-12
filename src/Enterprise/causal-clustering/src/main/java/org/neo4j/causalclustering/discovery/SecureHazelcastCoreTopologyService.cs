/// <summary>
/// Copyright (c) 2019 "GraphFoundation" <https://graphfoundation.org>
/// <para>
/// Neo4j is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
/// </para>
/// <para>
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
/// </para>
/// <para>
/// You should have received a copy of the GNU Affero General Public License
/// along with this program. If not, see <http://www.gnu.org/licenses/>.
/// 
/// </para>
/// </summary>

namespace Neo4Net.causalclustering.discovery
{
	using NetworkConfig = com.hazelcast.config.NetworkConfig;

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using SslPolicy = Neo4Net.Ssl.SslPolicy;

	internal class SecureHazelcastCoreTopologyService : HazelcastCoreTopologyService
	{
		 private readonly SslPolicy _sslPolicy;

		 /// <param name="config"> </param>
		 /// <param name="sslPolicy"> </param>
		 /// <param name="myself"> </param>
		 /// <param name="jobScheduler"> </param>
		 /// <param name="logProvider"> </param>
		 /// <param name="userLogProvider"> </param>
		 /// <param name="remoteMembersResolver"> </param>
		 /// <param name="topologyServiceRetryStrategy"> </param>
		 /// <param name="monitors"> </param>
		 internal SecureHazelcastCoreTopologyService( Config config, SslPolicy sslPolicy, MemberId myself, JobScheduler jobScheduler, LogProvider logProvider, LogProvider userLogProvider, RemoteMembersResolver remoteMembersResolver, TopologyServiceRetryStrategy topologyServiceRetryStrategy, Monitors monitors ) : base( config, myself, jobScheduler, logProvider, userLogProvider, remoteMembersResolver, topologyServiceRetryStrategy, monitors )
		 {
			  this._sslPolicy = sslPolicy;
		 }

		 /// <param name="networkConfig"> </param>
		 /// <param name="logProvider"> </param>
		 protected internal virtual void AdditionalConfig( NetworkConfig networkConfig, LogProvider logProvider )
		 {
			  SecureHazelcastConfiguration.ConfigureSsl( networkConfig, this._sslPolicy, logProvider );
		 }
	}


}