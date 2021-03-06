﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.routing.load_balancing.plugins.server_policies
{

	using Org.Neo4j.causalclustering.routing.load_balancing.filters;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.load_balancing_config;

	/// <summary>
	/// Loads filters under the name space of a particular plugin.
	/// </summary>
	internal class FilteringPolicyLoader
	{
		 private FilteringPolicyLoader()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static Policies load(org.neo4j.kernel.configuration.Config config, String pluginName, org.neo4j.logging.Log log) throws InvalidFilterSpecification
		 internal static Policies Load( Config config, string pluginName, Log log )
		 {
			  Policies policies = new Policies( log );

			  string prefix = PolicyPrefix( pluginName );
			  IDictionary<string, string> rawConfig = config.Raw;

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<string> configKeys = rawConfig.Keys.Where( e => e.StartsWith( prefix ) ).collect( Collectors.toSet() );

			  foreach ( string configKey in configKeys )
			  {
					string policyName = configKey.Substring( prefix.Length );
					string filterSpec = rawConfig[configKey];

					Filter<ServerInfo> filter = FilterConfigParser.Parse( filterSpec );
					policies.AddPolicy( policyName, new FilteringPolicy( filter ) );
			  }

			  return policies;
		 }

		 private static string PolicyPrefix( string pluginName )
		 {
			  return format( "%s.%s.", load_balancing_config.name(), pluginName );
		 }
	}

}