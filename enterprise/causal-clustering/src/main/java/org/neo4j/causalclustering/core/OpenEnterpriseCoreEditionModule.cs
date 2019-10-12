/*
 * Copyright (c) 2002-2018 "Neo Technology,"
 * Network Engine for Objects in Lund AB [http://neotechnology.com]
 *
 * Modifications Copyright (c) 2018-2019 "GraphFoundation" [https://graphfoundation.org]
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

namespace Org.Neo4j.causalclustering.core
{
	using ClusterStateDirectory = Org.Neo4j.causalclustering.core.state.ClusterStateDirectory;
	using ClusteringModule = Org.Neo4j.causalclustering.core.state.ClusteringModule;
	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using SecureHazelcastDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.SecureHazelcastDiscoveryServiceFactory;
	using DuplexPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.DuplexPipelineWrapperFactory;
	using SecurePipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.SecurePipelineWrapperFactory;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;


	public class OpenEnterpriseCoreEditionModule : EnterpriseCoreEditionModule
	{
		 /// <param name="platformModule"> </param>
		 /// <param name="discoveryServiceFactory"> </param>
		 internal OpenEnterpriseCoreEditionModule( PlatformModule platformModule, DiscoveryServiceFactory discoveryServiceFactory ) : base( platformModule, discoveryServiceFactory )
		 {

		 }

		 /// 
		 /// <param name="platformModule"> </param>
		 /// <param name="procedures"> </param>
		 public virtual void CreateSecurityModule( PlatformModule platformModule, Procedures procedures )
		 {
			  EnterpriseEditionModule.createEnterpriseSecurityModule( this, platformModule, procedures );
		 }

		 /// 
		 /// <param name="platformModule"> </param>
		 /// <param name="discoveryServiceFactory"> </param>
		 /// <param name="clusterStateDirectory"> </param>
		 /// <param name="identityModule"> </param>
		 /// <param name="dependencies"> </param>
		 /// <param name="databaseLayout">
		 /// @return </param>
		 protected internal virtual ClusteringModule GetClusteringModule( PlatformModule platformModule, DiscoveryServiceFactory discoveryServiceFactory, ClusterStateDirectory clusterStateDirectory, IdentityModule identityModule, Dependencies dependencies, DatabaseLayout databaseLayout )
		 {
			  SslPolicyLoader sslPolicyFactory = ( SslPolicyLoader ) dependencies.SatisfyDependency( SslPolicyLoader.create( this.Config, this.LogProvider ) );
			  SslPolicy sslPolicy = sslPolicyFactory.GetPolicy( ( string ) this.Config.get( CausalClusteringSettings.SslPolicy ) );
			  if ( discoveryServiceFactory is SecureHazelcastDiscoveryServiceFactory )
			  {
					( ( SecureHazelcastDiscoveryServiceFactory ) discoveryServiceFactory ).SslPolicy = sslPolicy;
			  }

			  return new ClusteringModule( discoveryServiceFactory, identityModule.Myself(), platformModule, clusterStateDirectory.Get(), databaseLayout );
		 }

		 /// <returns> SecurePipelineWrapperFactory </returns>
		 protected internal virtual DuplexPipelineWrapperFactory PipelineWrapperFactory()
		 {
			  return new SecurePipelineWrapperFactory();
		 }
	}


}