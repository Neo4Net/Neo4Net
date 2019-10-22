using System;
using System.Collections.Generic;

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
	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;
	using Neo4Net.GraphDb.config;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Connector.ConnectorType.HTTP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.HttpConnector.Encryption.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.HttpConnector.Encryption.TLS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.describeOneOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.listenAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.optionsObeyCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.setting;

	public class HttpConnectorValidator : ConnectorValidator
	{
		 private static readonly System.Action<string> _nullConsumer = s =>
		 {

		 };

		 public HttpConnectorValidator() : base(HTTP)
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

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean encrypted = encryptionSetting(name).apply(params::get) == org.Neo4Net.kernel.configuration.HttpConnector.Encryption.TLS;
			  bool encrypted = EncryptionSetting( name ).apply( @params.get ) == Encryption.TLS;
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
			  case "encryption":
					setting = EncryptionSetting( name );
					setting.setDescription( "Enable TLS for this connector." );
					break;
			  case "address":
					setting = listenAddress( settingName, DefaultPort( name, @params ) );
					setting.setDeprecated( true );
					setting.setReplacement( "dbms.connector." + name + ".listen_address" );
					setting.setDescription( "Address the connector should bind to. Deprecated and replaced by " + setting.replacement().get() + "." );
					break;
			  case "listen_address":
					setting = listenAddress( settingName, DefaultPort( name, @params ) );
					setting.setDescription( "Address the connector should bind to." );
					break;
			  case "advertised_address":
					setting = advertisedAddress( settingName, listenAddress( settingName, DefaultPort( name, @params ) ) );
					setting.setDescription( "Advertised address for this connector." );
					break;
			  default:
					return null;
			  }

			  // If not deprecated for other reasons
			  if ( IsDeprecatedConnectorName( name ) && !setting.deprecated() )
			  {
					setting.setDeprecated( true );
					setting.setReplacement( format( "%s.%s.%s.%s", parts[0], parts[1], encrypted ? "https" : "http", subsetting ) );
			  }
			  return setting;
		 }

		 /// <param name="name"> of connector, like 'bob' in 'dbms.connector.bob.type = HTTP' </param>
		 /// <param name="rawConfig"> to parse </param>
		 /// <returns> the default for the encryption level designated for the HTTP connector </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private int defaultPort(@Nonnull String name, @Nonnull Map<String,String> rawConfig)
		 private int DefaultPort( string name, IDictionary<string, string> rawConfig )
		 {
			  switch ( name )
			  {
			  case "http":
					return Encryption.NONE.defaultPort;
			  case "https":
					return TLS.defaultPort;
			  default:
					Setting<Encryption> es = EncryptionSetting( name );
					return es.apply( rawConfig.get ).defaultPort;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private static java.util.Map<String,String> assertEncryption(@Nonnull String name, @Nonnull Setting<?> setting, @Nonnull Map<String,String> rawConfig) throws org.Neo4Net.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static IDictionary<string, string> AssertEncryption<T1>( string name, Setting<T1> setting, IDictionary<string, string> rawConfig )
		 {
			  IDictionary<string, string> result = setting.validate( rawConfig, _nullConsumer );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Optional<?> encryption = java.util.Optional.ofNullable(setting.apply(rawConfig::get));
			  Optional<object> encryption = Optional.ofNullable( setting.apply( rawConfig.get ) );

			  if ( "https".Equals( name, StringComparison.OrdinalIgnoreCase ) )
			  {
					if ( encryption.Present && encryption.get() != TLS )
					{
						 throw new InvalidSettingException( format( "'%s' is only allowed to be '%s'; not '%s'", setting.name(), TLS.name(), encryption.get() ) );
					}
			  }
			  else if ( "http".Equals( name, StringComparison.OrdinalIgnoreCase ) )
			  {
					if ( encryption.Present && encryption.get() != NONE )
					{
						 throw new InvalidSettingException( format( "'%s' is only allowed to be '%s'; not '%s'", setting.name(), NONE.name(), encryption.get() ) );
					}
			  }

			  return result;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static org.Neo4Net.graphdb.config.BaseSetting<HttpConnector.Encryption> encryptionSetting(@Nonnull String name)
		 public static BaseSetting<HttpConnector.Encryption> EncryptionSetting( string name )
		 {
			  return EncryptionSetting( name, Encryption.NONE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static org.Neo4Net.graphdb.config.BaseSetting<HttpConnector.Encryption> encryptionSetting(@Nonnull String name, org.Neo4Net.kernel.configuration.HttpConnector.Encryption defaultValue)
		 public static BaseSetting<HttpConnector.Encryption> EncryptionSetting( string name, Encryption defaultValue )
		 {
			  Setting<Encryption> s = setting( "dbms.connector." + name + ".encryption", optionsObeyCase( typeof( Encryption ) ), defaultValue.name() );

			  return new BaseSettingAnonymousInnerClass( name, s );
		 }

		 private class BaseSettingAnonymousInnerClass : BaseSetting<Encryption>
		 {
			 private string _name;
			 private Setting<Encryption> _s;

			 public BaseSettingAnonymousInnerClass( string name, Setting<Encryption> s )
			 {
				 this._name = name;
				 this._s = s;
			 }

			 public override bool deprecated()
			 {
				  // For HTTP the encryption is decided by the connector name
				  return true;
			 }

			 public override Optional<string> replacement()
			 {
				  return null;
			 }

			 public override bool @internal()
			 {
				  return false;
			 }

			 public override Optional<string> documentedDefaultValue()
			 {
				  return null;
			 }

			 public override string valueDescription()
			 {
				  return describeOneOf( EnumSet.allOf( typeof( Encryption ) ) );
			 }

			 public override Optional<string> description()
			 {
				  return ( "Enable TLS for this connector. This is deprecated and is decided based on the " + "connector name instead." );
			 }

			 public override string name()
			 {
				  return _s.name();
			 }

			 public override void withScope( System.Func<string, string> scopingRule )
			 {
				  _s.withScope( scopingRule );
			 }

			 public override string DefaultValue
			 {
				 get
				 {
					  return _s.DefaultValue;
				 }
			 }

			 public override Encryption from( Configuration config )
			 {
				  return _s.from( config );
			 }

			 public override Encryption apply( System.Func<string, string> stringStringFunction )
			 {
				  return _s.apply( stringStringFunction );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String,String> validate(java.util.Map<String,String> rawConfig, System.Action<String> warningConsumer) throws org.Neo4Net.graphdb.config.InvalidSettingException
			 public override IDictionary<string, string> validate( IDictionary<string, string> rawConfig, System.Action<string> warningConsumer )
			 {
				  IDictionary<string, string> result = _s.validate( rawConfig, warningConsumer );
				  AssertEncryption( _name, _s, rawConfig );
				  return result;
			 }
		 }
	}

}