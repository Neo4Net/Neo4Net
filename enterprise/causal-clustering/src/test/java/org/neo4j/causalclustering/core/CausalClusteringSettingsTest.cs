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
namespace Org.Neo4j.causalclustering.core
{
	using Test = org.junit.Test;


	using Org.Neo4j.Graphdb.config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class CausalClusteringSettingsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldValidatePrefixBasedKeys()
		 public virtual void ShouldValidatePrefixBasedKeys()
		 {
			  // given
			  BaseSetting<string> setting = Settings.prefixSetting( "foo", Settings.STRING, "" );

			  IDictionary<string, string> rawConfig = new Dictionary<string, string>();
			  rawConfig["foo.us_east_1c"] = "abcdef";

			  // when
			  IDictionary<string, string> validConfig = setting.Validate(rawConfig, s =>
			  {
			  });

			  // then
			  assertEquals( 1, validConfig.Count );
			  assertEquals( rawConfig, validConfig );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldValidateMultiplePrefixBasedKeys()
		 public virtual void ShouldValidateMultiplePrefixBasedKeys()
		 {
			  // given
			  BaseSetting<string> setting = Settings.prefixSetting( "foo", Settings.STRING, "" );

			  IDictionary<string, string> rawConfig = new Dictionary<string, string>();
			  rawConfig["foo.us_east_1c"] = "abcdef";
			  rawConfig["foo.us_east_1d"] = "ghijkl";

			  // when
			  IDictionary<string, string> validConfig = setting.Validate(rawConfig, s =>
			  {
			  });

			  // then
			  assertEquals( 2, validConfig.Count );
			  assertEquals( rawConfig, validConfig );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldValidateLoadBalancingServerPolicies()
		 public virtual void ShouldValidateLoadBalancingServerPolicies()
		 {
			  // given
			  IDictionary<string, string> rawConfig = new Dictionary<string, string>();
			  rawConfig["causal_clustering.load_balancing.config.server_policies.us_east_1c"] = "all()";

			  // when
			  IDictionary<string, string> validConfig = CausalClusteringSettings.LoadBalancingConfig.validate(rawConfig, s =>
			  {
			  });

			  // then
			  assertEquals( 1, validConfig.Count );
			  assertEquals( rawConfig, validConfig );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeInvalidIfPrefixDoesNotMatch()
		 public virtual void ShouldBeInvalidIfPrefixDoesNotMatch()
		 {
			  // given
			  BaseSetting<string> setting = Settings.prefixSetting( "bar", Settings.STRING, "" );
			  IDictionary<string, string> rawConfig = new Dictionary<string, string>();
			  rawConfig["foo.us_east_1c"] = "abcdef";

			  // when
			  IDictionary<string, string> validConfig = setting.Validate(rawConfig, s =>
			  {
			  });

			  // then
			  assertEquals( 0, validConfig.Count );
		 }
	}

}