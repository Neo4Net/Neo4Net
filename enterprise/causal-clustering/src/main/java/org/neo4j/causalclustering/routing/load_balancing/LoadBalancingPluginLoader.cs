using System.Collections.Generic;

/*
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
namespace Org.Neo4j.causalclustering.routing.load_balancing
{

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using LeaderLocator = Org.Neo4j.causalclustering.core.consensus.LeaderLocator;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using ServerShufflingProcessor = Org.Neo4j.causalclustering.routing.load_balancing.plugins.ServerShufflingProcessor;
	using InvalidSettingException = Org.Neo4j.Graphdb.config.InvalidSettingException;
	using Service = Org.Neo4j.Helpers.Service;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

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
//ORIGINAL LINE: public static void validate(org.neo4j.kernel.configuration.Config config, org.neo4j.logging.Log log) throws org.neo4j.graphdb.config.InvalidSettingException
		 public static void Validate( Config config, Log log )
		 {
			  LoadBalancingPlugin plugin = FindPlugin( config );
			  plugin.Validate( config, log );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static LoadBalancingProcessor load(org.neo4j.causalclustering.discovery.TopologyService topologyService, org.neo4j.causalclustering.core.consensus.LeaderLocator leaderLocator, org.neo4j.logging.LogProvider logProvider, org.neo4j.kernel.configuration.Config config) throws Throwable
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
//ORIGINAL LINE: private static LoadBalancingPlugin findPlugin(org.neo4j.kernel.configuration.Config config) throws org.neo4j.graphdb.config.InvalidSettingException
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