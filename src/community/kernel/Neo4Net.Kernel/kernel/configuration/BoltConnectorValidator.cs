﻿using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.configuration
{

	using Neo4Net.GraphDb.config;
	using Neo4Net.GraphDb.config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.BoltConnector.EncryptionLevel.OPTIONAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Connector.ConnectorType.BOLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.listenAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.optionsObeyCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.setting;

	public class BoltConnectorValidator : ConnectorValidator
	{
		 public BoltConnectorValidator() : base(BOLT)
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull protected java.util.Optional<org.Neo4Net.graphdb.config.Setting<Object>> getSettingFor(@Nonnull String settingName, @Nonnull Map<String,String> params)
		 protected internal override Optional<Setting<object>> GetSettingFor( string settingName, IDictionary<string, string> @params )
		 {
			  // owns has already verified that 'type' is correct and that this split is possible
			  string[] parts = settingName.Split( "\\.", true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String name = parts[2];
			  string name = parts[2];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String subsetting = parts[3];
			  string subsetting = parts[3];

			  BaseSetting setting;

			  switch ( subsetting )
			  {
			  case "enabled":
					setting = ( BaseSetting ) setting( settingName, BOOLEAN, "false" );
					setting.setDescription( "Enable this connector." );
					break;
			  case "type":
					setting = ( BaseSetting ) setting( settingName, optionsObeyCase( typeof( Connector.ConnectorType ) ), NO_DEFAULT );
					setting.setDeprecated( true );
					setting.setDescription( "Connector type. This setting is deprecated and its value will instead be " + "inferred from the name of the connector." );
					break;
			  case "tls_level":
					setting = ( BaseSetting ) setting( settingName, optionsObeyCase( typeof( BoltConnector.EncryptionLevel ) ), OPTIONAL.name() );
					setting.setDescription( "Encryption level to require this connector to use." );
					break;
			  case "address":
					setting = listenAddress( settingName, 7687 );
					setting.setDeprecated( true );
					setting.setReplacement( "dbms.connector." + name + ".listen_address" );
					setting.setDescription( "Address the connector should bind to. Deprecated and replaced by " + setting.replacement().get() + "." );
					break;
			  case "listen_address":
					setting = listenAddress( settingName, 7687 );
					setting.setDescription( "Address the connector should bind to." );
					break;
			  case "advertised_address":
					setting = advertisedAddress( settingName, listenAddress( settingName, 7687 ) );
					setting.setDescription( "Advertised address for this connector." );
					break;
			  case "thread_pool_min_size":
					setting = ( BaseSetting ) setting( settingName, INTEGER, NO_DEFAULT );
					setting.setDescription( "The number of threads to keep in the thread pool bound to this connector, even if they are idle." );
					break;
			  case "thread_pool_max_size":
					setting = ( BaseSetting ) setting( settingName, INTEGER, NO_DEFAULT );
					setting.setDescription( "The maximum number of threads allowed in the thread pool bound to this connector." );
					break;
			  case "thread_pool_keep_alive":
					setting = ( BaseSetting ) setting( settingName, DURATION, NO_DEFAULT );
					setting.setDescription( "The maximum time an idle thread in the thread pool bound to this connector will wait for new tasks." );
					break;
			  case "unsupported_thread_pool_queue_size":
					setting = ( BaseSetting ) setting( settingName, INTEGER, NO_DEFAULT );
					setting.setDescription( "The queue size of the thread pool bound to this connector (-1 for unbounded, 0 for direct handoff, > 0 for bounded)" );
					break;
			  default:
					return null;
			  }

			  // If not deprecated for other reasons
			  if ( IsDeprecatedConnectorName( name ) && !setting.deprecated() )
			  {
					setting.setDeprecated( true );
					setting.setReplacement( format( "%s.%s.%s.%s", parts[0], parts[1], "bolt", subsetting ) );
			  }
			  return setting;
		 }
	}

}