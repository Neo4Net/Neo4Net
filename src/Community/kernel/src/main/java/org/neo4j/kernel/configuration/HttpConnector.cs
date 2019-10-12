using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.configuration
{
	using Description = Neo4Net.Configuration.Description;
	using ReplacedBy = Neo4Net.Configuration.ReplacedBy;
	using Neo4Net.Graphdb.config;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.legacyFallback;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.listenAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.optionsObeyCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	[Description("Configuration options for HTTP connectors. " + "\"(http-connector-key)\" is a placeholder for a unique name for the connector, for instance " + "\"http-public\" or some other name that describes what the connector is for.")]
	public class HttpConnector : Connector
	{
		 [Description("Enable TLS for this connector")]
		 public readonly Setting<Encryption> Encryption;

		 [Description("Address the connector should bind to. " + "This setting is deprecated and will be replaced by `+listen_address+`"), Obsolete, ReplacedBy("dbms.connector.X.listen_address")]
		 public readonly Setting<ListenSocketAddress> Address;

		 [Description("Address the connector should bind to")]
		 public readonly Setting<ListenSocketAddress> ListenAddress;

		 [Description("Advertised address for this connector")]
		 public readonly Setting<AdvertisedSocketAddress> AdvertisedAddress;
		 private readonly Encryption _encryptionLevel;

		 // Used by config doc generator
		 public HttpConnector() : this("(http-connector-key)")
		 {
		 }

		 public HttpConnector( Encryption encryptionLevel ) : this( "(http-connector-key)", encryptionLevel )
		 {
		 }

		 public HttpConnector( string key ) : this( key, Encryption.None )
		 {
		 }

		 public HttpConnector( string key, Encryption encryptionLevel ) : base( key )
		 {
			  this._encryptionLevel = encryptionLevel;
			  Encryption = Group.scope( setting( "encryption", optionsObeyCase( typeof( HttpConnector.Encryption ) ), NO_DEFAULT ) );
			  Setting<ListenSocketAddress> legacyAddressSetting = listenAddress( "address", encryptionLevel.defaultPort );
			  Setting<ListenSocketAddress> listenAddressSetting = legacyFallback( legacyAddressSetting, listenAddress( "listen_address", encryptionLevel.defaultPort ) );

			  this.Address = Group.scope( legacyAddressSetting );
			  this.ListenAddress = Group.scope( listenAddressSetting );
			  this.AdvertisedAddress = Group.scope( advertisedAddress( "advertised_address", listenAddressSetting ) );
		 }

		 /// <returns> this connector's configured encryption level </returns>
		 public virtual Encryption EncryptionLevel()
		 {
			  return _encryptionLevel;
		 }

		 public sealed class Encryption
		 {
			  public static readonly Encryption None = new Encryption( "None", InnerEnum.None, "http", 7474 );
			  public static readonly Encryption Tls = new Encryption( "Tls", InnerEnum.Tls, "https", 7473 );

			  private static readonly IList<Encryption> valueList = new List<Encryption>();

			  static Encryption()
			  {
				  valueList.Add( None );
				  valueList.Add( Tls );
			  }

			  public enum InnerEnum
			  {
				  None,
				  Tls
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Public readonly;
			  internal Public readonly;

			  internal Encryption( string name, InnerEnum innerEnum, string uriScheme, int defaultPort )
			  {
					this.UriScheme = uriScheme;
					this.DefaultPort = defaultPort;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<Encryption> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Encryption valueOf( string name )
			 {
				 foreach ( Encryption enumInstance in Encryption.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}