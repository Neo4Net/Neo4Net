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
namespace Org.Neo4j.causalclustering.discovery
{
	using ClientNetworkConfig = com.hazelcast.client.config.ClientNetworkConfig;

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;

	public class SecureHazelcastClientConnector : HazelcastClientConnector
	{
		 private readonly SslPolicy _sslPolicy;

		 /// <param name="config"> </param>
		 /// <param name="logProvider"> </param>
		 /// <param name="sslPolicy"> </param>
		 /// <param name="remoteMembersResolver"> </param>
		 internal SecureHazelcastClientConnector( Config config, LogProvider logProvider, SslPolicy sslPolicy, RemoteMembersResolver remoteMembersResolver ) : base( config, logProvider, remoteMembersResolver )
		 {
			  this._sslPolicy = sslPolicy;
		 }

		 /// <param name="networkConfig"> </param>
		 /// <param name="logProvider"> </param>
		 protected internal virtual void AdditionalConfig( ClientNetworkConfig networkConfig, LogProvider logProvider )
		 {
			  SecureHazelcastConfiguration.ConfigureSsl( networkConfig, this._sslPolicy, logProvider );
		 }
	}


}