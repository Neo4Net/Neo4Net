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
namespace Org.Neo4j.Configuration
{

	using InvalidSettingException = Org.Neo4j.Graphdb.config.InvalidSettingException;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfigurationValidator = Org.Neo4j.Kernel.configuration.ConfigurationValidator;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using Mode = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings.Mode;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.initial_hosts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.server_id;

	public class HaConfigurationValidator : ConfigurationValidator
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public java.util.Map<String,String> validate(@Nonnull Config config, @Nonnull Log log) throws org.neo4j.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override IDictionary<string, string> Validate( Config config, Log log )
		 {
			  // Make sure mode is HA
			  EnterpriseEditionSettings.Mode mode = config.get( EnterpriseEditionSettings.mode );
			  if ( mode.Equals( EnterpriseEditionSettings.Mode.HA ) || mode.Equals( EnterpriseEditionSettings.Mode.ARBITER ) )
			  {
					ValidateServerId( config );
					ValidateInitialHosts( config );
			  }

			  return Collections.emptyMap();
		 }

		 private static void ValidateServerId( Config config )
		 {
			  if ( !config.IsConfigured( server_id ) )
			  {
					throw new InvalidSettingException( string.Format( "Missing mandatory value for '{0}'", server_id.name() ) );
			  }
		 }

		 private static void ValidateInitialHosts( Config config )
		 {
			  IList<HostnamePort> hosts = config.Get( initial_hosts );
			  if ( hosts == null || hosts.Count == 0 )
			  {
					throw new InvalidSettingException( string.Format( "Missing mandatory non-empty value for '{0}'", initial_hosts.name() ) );
			  }
		 }
	}

}