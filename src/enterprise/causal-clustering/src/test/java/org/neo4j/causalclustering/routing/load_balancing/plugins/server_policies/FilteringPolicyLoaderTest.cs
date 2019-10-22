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
namespace Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies
{
	using Test = org.junit.Test;

	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.load_balancing_config;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies.FilterBuilder.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	public class FilteringPolicyLoaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadConfiguredPolicies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadConfiguredPolicies()
		 {
			  // given
			  string pluginName = "server_policies";

			  object[][] input = new object[][]
			  {
				  new object[] { "asia_west", "groups(asia_west) -> min(2);" + "groups(asia) -> min(2);", filter().groups("asia_west").min(2).newRule().groups("asia").min(2).newRule().all().build() },
				  new object[] { "asia_east", "groups(asia_east) -> min(2);" + "groups(asia) -> min(2);", filter().groups("asia_east").min(2).newRule().groups("asia").min(2).newRule().all().build() },
				  new object[] { "asia_only", "groups(asia);" + "halt();", filter().groups("asia").build() }
			  };

			  Config config = Config.defaults();

			  foreach ( object[] row in input )
			  {
					string policyName = ( string ) row[0];
					string filterSpec = ( string ) row[1];
					config.Augment( ConfigNameFor( pluginName, policyName ), filterSpec );
			  }

			  // when
			  Policies policies = FilteringPolicyLoader.Load( config, pluginName, mock( typeof( Log ) ) );

			  // then
			  foreach ( object[] row in input )
			  {
					string policyName = ( string ) row[0];
					Policy loadedPolicy = policies.SelectFor( PolicyNameContext( policyName ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Policy expectedPolicy = new FilteringPolicy((org.Neo4Net.causalclustering.routing.load_balancing.filters.Filter<ServerInfo>) row[2]);
					Policy expectedPolicy = new FilteringPolicy( ( Filter<ServerInfo> ) row[2] );
					assertEquals( expectedPolicy, loadedPolicy );
			  }
		 }

		 private static IDictionary<string, string> PolicyNameContext( string policyName )
		 {
			  return stringMap( Policies.POLICY_KEY, policyName );
		 }

		 private static string ConfigNameFor( string pluginName, string policyName )
		 {
			  return format( "%s.%s.%s", load_balancing_config.name(), pluginName, policyName );
		 }
	}

}