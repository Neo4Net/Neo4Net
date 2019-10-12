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

	using LoadBalancingPluginLoader = Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingPluginLoader;
	using InvalidSettingException = Org.Neo4j.Graphdb.config.InvalidSettingException;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfigurationValidator = Org.Neo4j.Kernel.configuration.ConfigurationValidator;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using Mode = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings.Mode;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.minimum_core_cluster_size_at_runtime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.minimum_core_cluster_size_at_formation;

	public class CausalClusterConfigurationValidator : ConfigurationValidator
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public java.util.Map<String,String> validate(@Nonnull Config config, @Nonnull Log log) throws org.neo4j.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override IDictionary<string, string> Validate( Config config, Log log )
		 {
			  // Make sure mode is CC
			  EnterpriseEditionSettings.Mode mode = config.get( EnterpriseEditionSettings.mode );
			  if ( mode.Equals( EnterpriseEditionSettings.Mode.CORE ) || mode.Equals( EnterpriseEditionSettings.Mode.READ_REPLICA ) )
			  {
					ValidateInitialDiscoveryMembers( config );
					ValidateBoltConnector( config );
					ValidateLoadBalancing( config, log );
					ValidateDeclaredClusterSizes( config );
			  }

			  return Collections.emptyMap();
		 }

		 private void ValidateDeclaredClusterSizes( Config config )
		 {
			  int startup = config.Get( minimum_core_cluster_size_at_formation );
			  int runtime = config.Get( minimum_core_cluster_size_at_runtime );

			  if ( runtime > startup )
			  {
					throw new InvalidSettingException( string.Format( "'{0}' must be set greater than or equal to '{1}'", minimum_core_cluster_size_at_formation.name(), minimum_core_cluster_size_at_runtime.name() ) );
			  }
		 }

		 private void ValidateLoadBalancing( Config config, Log log )
		 {
			  LoadBalancingPluginLoader.validate( config, log );
		 }

		 private void ValidateBoltConnector( Config config )
		 {
			  if ( config.EnabledBoltConnectors().Count == 0 )
			  {
					throw new InvalidSettingException( "A Bolt connector must be configured to run a cluster" );
			  }
		 }

		 private void ValidateInitialDiscoveryMembers( Config config )
		 {
			  DiscoveryType discoveryType = config.Get( CausalClusteringSettings.DiscoveryType );
			  discoveryType.requiredSettings().forEach(setting =>
			  {
			  if ( !config.IsConfigured( setting ) )
			  {
				  throw new InvalidSettingException( string.Format( "Missing value for '{0}', which is mandatory with '{1}={2}'", setting.name(), CausalClusteringSettings.DiscoveryType.name(), discoveryType ) );
			  }
			  });
		 }
	}

}