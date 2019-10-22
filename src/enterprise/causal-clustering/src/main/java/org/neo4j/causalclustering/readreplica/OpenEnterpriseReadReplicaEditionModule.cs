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

namespace Neo4Net.causalclustering.readreplica
{
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using DiscoveryServiceFactory = Neo4Net.causalclustering.discovery.DiscoveryServiceFactory;
	using SecureHazelcastDiscoveryServiceFactory = Neo4Net.causalclustering.discovery.SecureHazelcastDiscoveryServiceFactory;
	using DuplexPipelineWrapperFactory = Neo4Net.causalclustering.handlers.DuplexPipelineWrapperFactory;
	using SecurePipelineWrapperFactory = Neo4Net.causalclustering.handlers.SecurePipelineWrapperFactory;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using Config = Neo4Net.Kernel.configuration.Config;
	using SslPolicyLoader = Neo4Net.Kernel.configuration.ssl.SslPolicyLoader;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using SslPolicy = Neo4Net.Ssl.SslPolicy;

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