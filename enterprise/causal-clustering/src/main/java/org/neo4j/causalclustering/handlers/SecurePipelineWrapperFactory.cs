﻿/*
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
namespace Org.Neo4j.causalclustering.handlers
{
	using Org.Neo4j.Graphdb.config;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;

	public class SecurePipelineWrapperFactory : DuplexPipelineWrapperFactory
	{

		 /// 
		 public SecurePipelineWrapperFactory()
		 {
		 }

		 /// <param name="config"> </param>
		 /// <param name="dependencies"> </param>
		 /// <param name="logProvider"> </param>
		 /// <param name="policyName">
		 /// @return </param>
		 public override PipelineWrapper ForServer( Config config, Dependencies dependencies, LogProvider logProvider, Setting<string> policyName )
		 {
			  SslPolicy policy = this.GetSslPolicy( config, dependencies, policyName );
			  return new SecureServerPipelineWrapper( policy );
		 }

		 /// <param name="config"> </param>
		 /// <param name="dependencies"> </param>
		 /// <param name="logProvider"> </param>
		 /// <param name="policyName">
		 /// @return </param>
		 public override PipelineWrapper ForClient( Config config, Dependencies dependencies, LogProvider logProvider, Setting<string> policyName )
		 {
			  SslPolicy policy = this.GetSslPolicy( config, dependencies, policyName );
			  return new SecureClientPipelineWrapper( policy );
		 }

		 /// <param name="config"> </param>
		 /// <param name="dependencies"> </param>
		 /// <param name="policyNameSetting">
		 /// @return </param>
		 private SslPolicy GetSslPolicy( Config config, Dependencies dependencies, Setting<string> policyNameSetting )
		 {
			  SslPolicyLoader sslPolicyLoader = ( SslPolicyLoader ) dependencies.ResolveDependency( typeof( SslPolicyLoader ) );
			  string policyName = ( string ) config.Get( policyNameSetting );
			  return sslPolicyLoader.GetPolicy( policyName );
		 }
	}

}