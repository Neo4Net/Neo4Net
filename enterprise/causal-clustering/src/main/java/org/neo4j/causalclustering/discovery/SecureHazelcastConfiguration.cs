/*
 * Copyright (c) 2002-2018 "Neo Technology,"
 * Network Engine for Objects in Lund AB [http://neotechnology.com]
 *
 * Modifications Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * You can redistribute this software and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.

 *
 * Modified from :  https://github.com/neo4j/neo4j/blob/3.3/enterprise/causal-clustering/src/main/java/org/neo4j/causalclustering/discovery/HazelcastSslConfiguration.java
 */

namespace Org.Neo4j.causalclustering.discovery
{
	using ClientNetworkConfig = com.hazelcast.client.config.ClientNetworkConfig;
	using NetworkConfig = com.hazelcast.config.NetworkConfig;
	using SSLConfig = com.hazelcast.config.SSLConfig;

	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;

	public class SecureHazelcastConfiguration
	{
		 /// 
		 public SecureHazelcastConfiguration()
		 {
		 }

		 /// <param name="networkConfig"> </param>
		 /// <param name="sslPolicy"> </param>
		 /// <param name="logProvider"> </param>
		 public static void ConfigureSsl( NetworkConfig networkConfig, SslPolicy sslPolicy, LogProvider logProvider )
		 {
			  SSLConfig sslConfig = CommonSslConfig( sslPolicy, logProvider );
			  networkConfig.SSLConfig = sslConfig;
		 }

		 /// <param name="clientNetworkConfig"> </param>
		 /// <param name="sslPolicy"> </param>
		 /// <param name="logProvider"> </param>
		 public static void ConfigureSsl( ClientNetworkConfig clientNetworkConfig, SslPolicy sslPolicy, LogProvider logProvider )
		 {
			  SSLConfig sslConfig = CommonSslConfig( sslPolicy, logProvider );
			  clientNetworkConfig.SSLConfig = sslConfig;
		 }

		 /// <param name="sslPolicy"> </param>
		 /// <param name="logProvider">
		 /// @return </param>
		 private static SSLConfig CommonSslConfig( SslPolicy sslPolicy, LogProvider logProvider )
		 {
			  SSLConfig sslConfig = new SSLConfig();
			  if ( sslPolicy == null )
			  {
					return sslConfig;
			  }
			  else
			  {
					sslConfig.setFactoryImplementation( new SecureHazelcastContextFactory( sslPolicy, logProvider ) ).setEnabled( true );
					switch ( sslPolicy.ClientAuth )
					{
					case REQUIRE:
						 sslConfig.setProperty( "javax.net.ssl.mutualAuthentication", "REQUIRED" );
						 break;
					case OPTIONAL:
						 sslConfig.setProperty( "javax.net.ssl.mutualAuthentication", "OPTIONAL" );
						goto case NONE;
					case NONE:
						 break;
					default:
						 throw new System.ArgumentException( "Not supported: " + sslPolicy.ClientAuth );
					}

					return sslConfig;
			  }
		 }
	}

}