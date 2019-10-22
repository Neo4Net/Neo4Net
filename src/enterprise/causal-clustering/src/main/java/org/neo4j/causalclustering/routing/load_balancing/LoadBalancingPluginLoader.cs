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
namespace Neo4Net.causalclustering.routing.load_balancing
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using ServerShufflingProcessor = Neo4Net.causalclustering.routing.load_balancing.plugins.ServerShufflingProcessor;
	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;
	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Loads and initialises any service implementations of <class>LoadBalancingPlugin</class>.
	/// Exposes configured instances of that interface via an iterator.
	/// </summary>
	public class LoadBalancingPluginLoader
	{
		 private LoadBalancingPluginLoader()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void validate(org.Neo4Net.kernel.configuration.Config config, org.Neo4Net.logging.Log log) throws org.Neo4Net.graphdb.config.InvalidSettingException
		 public static void Validate( Config config, Log log )
		 {
			  LoadBalancingPlugin plugin = FindPlugin( config );
			  plugin.Validate( config, log );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static LoadBalancingProcessor load(org.Neo4Net.causalclustering.discovery.TopologyService topologyService, org.Neo4Net.causalclustering.core.consensus.LeaderLocator leaderLocator, org.Neo4Net.logging.LogProvider logProvider, org.Neo4Net.kernel.configuration.Config config) throws Throwable
		 public static LoadBalancingProcessor Load( TopologyService topologyService, LeaderLocator leaderLocator, LogProvider logProvider, Config config )
		 {
			  LoadBalancingPlugin plugin = FindPlugin( config );
			  plugin.Init( topologyService, leaderLocator, logProvider, config );

			  if ( config.Get( CausalClusteringSettings.load_balancing_shuffle ) )
			  {
					return new ServerShufflingProcessor( plugin );
			  }

			  return plugin;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static LoadBalancingPlugin findPlugin(org.Neo4Net.kernel.configuration.Config config) throws org.Neo4Net.graphdb.config.InvalidSettingException
		 private static LoadBalancingPlugin FindPlugin( Config config )
		 {
			  ISet<string> availableOptions = new HashSet<string>();
			  IEnumerable<LoadBalancingPlugin> allImplementationsOnClasspath = Service.load( typeof( LoadBalancingPlugin ) );

			  string configuredName = config.Get( CausalClusteringSettings.load_balancing_plugin );

			  foreach ( LoadBalancingPlugin plugin in allImplementationsOnClasspath )
			  {
					if ( plugin.PluginName().Equals(configuredName) )
					{
						 return plugin;
					}
					availableOptions.Add( plugin.PluginName() );
			  }

			  throw new InvalidSettingException( string.Format( "Could not find load balancing plugin with name: '{0}'" + " among available options: {1}", configuredName, availableOptions ) );
		 }
	}

}