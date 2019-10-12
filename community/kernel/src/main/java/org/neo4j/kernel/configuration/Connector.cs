using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.configuration
{
	using Description = Org.Neo4j.Configuration.Description;
	using Org.Neo4j.Graphdb.config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.optionsObeyCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	[Group("dbms.connector")]
	public class Connector
	{
		 [Description("Enable this connector")]
		 public readonly Setting<bool> Enabled;

		 [Description("Connector type. You should always set this to the connector type you want")]
		 public readonly Setting<ConnectorType> Type;

		 // Note: Be careful about adding things here that does not apply to all connectors,
		 //       consider future options like non-tcp transports, making `address` a bad choice
		 //       as a setting that applies to every connector, for instance.

		 public readonly GroupSettingSupport Group;

		 /// <summary>
		 /// Deprecated, please use other constructor. This constructor will be removed in 4.0.
		 /// </summary>
		 /// <param name="key"> of connector </param>
		 /// <param name="typeDefault"> unused parameter </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated public Connector(String key, @SuppressWarnings("UnusedParameters") String typeDefault)
		 [Obsolete]
		 public Connector( string key, string typeDefault ) : this( key )
		 {
		 }

		 public Connector( string key )
		 {
			  Group = new GroupSettingSupport( typeof( Connector ), key );
			  Enabled = Group.scope( setting( "enabled", BOOLEAN, "false" ) );
			  Type = Group.scope( setting( "type", optionsObeyCase( typeof( ConnectorType ) ), NO_DEFAULT ) );
		 }

		 public enum ConnectorType
		 {
			  Bolt,
			  Http
		 }

		 public virtual string Key()
		 {
			  return Group.groupKey;
		 }
	}

}