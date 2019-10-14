using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.routing.load_balancing
{
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using ServerShufflingProcessor = Neo4Net.causalclustering.routing.load_balancing.plugins.ServerShufflingProcessor;
	using ServerPoliciesPlugin = Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies.ServerPoliciesPlugin;
	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class LoadBalancingPluginLoaderTest
	{
		 private const string DUMMY_PLUGIN_NAME = "dummy";
		 private const string DOES_NOT_EXIST = "does_not_exist";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSelectedPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnSelectedPlugin()
		 {
			  // given
			  Config config = Config.builder().withSetting(CausalClusteringSettings.load_balancing_plugin, DUMMY_PLUGIN_NAME).withSetting(CausalClusteringSettings.load_balancing_shuffle, "false").build();

			  // when
			  LoadBalancingProcessor plugin = LoadBalancingPluginLoader.Load( mock( typeof( TopologyService ) ), mock( typeof( LeaderLocator ) ), NullLogProvider.Instance, config );

			  // then
			  assertTrue( plugin is DummyLoadBalancingPlugin );
			  assertEquals( DUMMY_PLUGIN_NAME, ( ( DummyLoadBalancingPlugin ) plugin ).PluginName() );
			  assertTrue( ( ( DummyLoadBalancingPlugin ) plugin ).WasInitialized );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnableShufflingOfDelegate() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnableShufflingOfDelegate()
		 {
			  // given
			  Config config = Config.builder().withSetting(CausalClusteringSettings.load_balancing_plugin, DUMMY_PLUGIN_NAME).withSetting(CausalClusteringSettings.load_balancing_shuffle, "true").build();

			  // when
			  LoadBalancingProcessor plugin = LoadBalancingPluginLoader.Load( mock( typeof( TopologyService ) ), mock( typeof( LeaderLocator ) ), NullLogProvider.Instance, config );

			  // then
			  assertTrue( plugin is ServerShufflingProcessor );
			  assertTrue( ( ( ServerShufflingProcessor ) plugin ).@delegate() is DummyLoadBalancingPlugin );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindServerPoliciesPlugin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindServerPoliciesPlugin()
		 {
			  // given
			  Config config = Config.builder().withSetting(CausalClusteringSettings.load_balancing_plugin, ServerPoliciesPlugin.PLUGIN_NAME).withSetting(CausalClusteringSettings.load_balancing_shuffle, "false").build();

			  // when
			  LoadBalancingProcessor plugin = LoadBalancingPluginLoader.Load( mock( typeof( TopologyService ) ), mock( typeof( LeaderLocator ) ), NullLogProvider.Instance, config );

			  // then
			  assertTrue( plugin is ServerPoliciesPlugin );
			  assertEquals( ServerPoliciesPlugin.PLUGIN_NAME, ( ( ServerPoliciesPlugin ) plugin ).pluginName() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnInvalidPlugin()
		 public virtual void ShouldThrowOnInvalidPlugin()
		 {
			  // given
			  Config config = Config.defaults( CausalClusteringSettings.load_balancing_plugin, DOES_NOT_EXIST );

			  try
			  {
					// when
					LoadBalancingPluginLoader.Validate( config, mock( typeof( Log ) ) );
					fail();
			  }
			  catch ( InvalidSettingException )
			  {
					// then
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptInvalidSetting()
		 public virtual void ShouldNotAcceptInvalidSetting()
		 {
			  // given
			  Config config = Config.builder().withSetting(SettingFor(DUMMY_PLUGIN_NAME, DummyLoadBalancingPlugin.DO_NOT_USE_THIS_CONFIG), "true").withSetting(CausalClusteringSettings.load_balancing_plugin, DUMMY_PLUGIN_NAME).build();

			  try
			  {
					// when
					LoadBalancingPluginLoader.Validate( config, mock( typeof( Log ) ) );
					fail();
			  }
			  catch ( InvalidSettingException )
			  {
					// then
			  }
		 }

		 private static string SettingFor( string pluginName, string settingName )
		 {
			  return string.Format( "{0}.{1}.{2}", CausalClusteringSettings.load_balancing_config.name(), pluginName, settingName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(LoadBalancingPlugin.class) public static class DummyLoadBalancingPlugin implements LoadBalancingPlugin
		 public class DummyLoadBalancingPlugin : LoadBalancingPlugin
		 {
			  internal const string DO_NOT_USE_THIS_CONFIG = "do_not_use";
			  internal bool WasInitialized;

			  public DummyLoadBalancingPlugin()
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validate(org.neo4j.kernel.configuration.Config config, org.neo4j.logging.Log log) throws org.neo4j.graphdb.config.InvalidSettingException
			  public override void Validate( Config config, Log log )
			  {
					Optional<string> invalidSetting = config.GetRaw( SettingFor( DUMMY_PLUGIN_NAME, DO_NOT_USE_THIS_CONFIG ) );
					invalidSetting.ifPresent(s =>
					{
					 throw new InvalidSettingException( "Do not use this setting" );
					});
			  }

			  public override void Init( TopologyService topologyService, LeaderLocator leaderLocator, LogProvider logProvider, Config config )
			  {
					WasInitialized = true;
			  }

			  public override string PluginName()
			  {
					return DUMMY_PLUGIN_NAME;
			  }

			  public override LoadBalancingProcessor_Result Run( IDictionary<string, string> context )
			  {
					return null;
			  }
		 }
	}

}