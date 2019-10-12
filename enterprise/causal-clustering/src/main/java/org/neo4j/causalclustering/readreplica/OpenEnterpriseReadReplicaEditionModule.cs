/*
 * Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */

namespace Org.Neo4j.causalclustering.readreplica
{
	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using SecureHazelcastDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.SecureHazelcastDiscoveryServiceFactory;
	using DuplexPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.DuplexPipelineWrapperFactory;
	using SecurePipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.SecurePipelineWrapperFactory;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;

	/// 
	public class OpenEnterpriseReadReplicaEditionModule : EnterpriseReadReplicaEditionModule
	{

		 /// <param name="platformModule"> </param>
		 /// <param name="discoveryServiceFactory"> </param>
		 /// <param name="myself"> </param>
		 internal OpenEnterpriseReadReplicaEditionModule( PlatformModule platformModule, DiscoveryServiceFactory discoveryServiceFactory, MemberId myself ) : base( platformModule, discoveryServiceFactory, myself )
		 {
		 }

		 /// <param name="discoveryServiceFactory"> </param>
		 /// <param name="dependencies"> </param>
		 /// <param name="config"> </param>
		 /// <param name="logProvider"> </param>
		 protected internal virtual void ConfigureDiscoveryService( DiscoveryServiceFactory discoveryServiceFactory, Dependencies dependencies, Config config, LogProvider logProvider )
		 {
			  SslPolicyLoader sslPolicyFactory = ( SslPolicyLoader ) dependencies.SatisfyDependency( SslPolicyLoader.create( config, logProvider ) );
			  SslPolicy clusterSslPolicy = sslPolicyFactory.GetPolicy( ( string ) config.Get( CausalClusteringSettings.ssl_policy ) );
			  if ( discoveryServiceFactory is SecureHazelcastDiscoveryServiceFactory )
			  {
					( ( SecureHazelcastDiscoveryServiceFactory ) discoveryServiceFactory ).SslPolicy = clusterSslPolicy;
			  }
		 }

		 /// <summary>
		 /// We need to override this method because the parent class returns a VoidPipelineWrapperFactory.
		 /// </summary>
		 /// <returns> SecurePipelineWrapperFactory </returns>
		 protected internal virtual DuplexPipelineWrapperFactory PipelineWrapperFactory()
		 {
			  return new SecurePipelineWrapperFactory();
		 }
	}

}