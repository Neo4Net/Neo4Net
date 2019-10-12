using System;
using System.Collections;
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
namespace Org.Neo4j.Kernel.configuration
{

	using InvalidSettingException = Org.Neo4j.Graphdb.config.InvalidSettingException;
	using Org.Neo4j.Graphdb.config;
	using Org.Neo4j.Graphdb.config;


	public abstract class ConnectorValidator : SettingGroup<object>
	{
		 private static readonly ISet<string> _validTypes = Arrays.stream( Enum.GetValues( typeof( Connector.ConnectorType ) ) ).map( Enum.name ).collect( toSet() );
		 internal static readonly string DeprecatedConnectorMsg = "Warning: connectors with names other than [http,https,bolt] are%n" +
							  "deprecated and support for them will be removed in a future%n" +
							  "version of Neo4j. Offending lines in " + Config.DEFAULT_CONFIG_FILE_NAME + ":%n%n%s";
		 protected internal readonly Connector.ConnectorType Type;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ConnectorValidator(@Nonnull Connector.ConnectorType type)
		 public ConnectorValidator( Connector.ConnectorType type )
		 {
			  this.Type = type;
		 }

		 /// <summary>
		 /// Determine if this instance is responsible for validating a setting.
		 /// </summary>
		 /// <param name="key"> the key of the setting </param>
		 /// <param name="rawConfig"> raw map of config settings to validate </param>
		 /// <returns> true if this instance is responsible for parsing the setting, false otherwise. </returns>
		 /// <exception cref="InvalidSettingException"> if an answer can not be determined, for example in case of a missing second
		 /// mandatory setting. </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public boolean owns(@Nonnull String key, @Nonnull Map<String,String> rawConfig) throws org.neo4j.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual bool Owns( string key, IDictionary<string, string> rawConfig )
		 {
			  string[] parts = key.Split( "\\.", true );
			  if ( parts.Length < 2 )
			  {
					return false;
			  }
			  if ( !parts[0].Equals( "dbms" ) || !parts[1].Equals( "connector" ) )
			  {
					return false;
			  }

			  // Do not allow invalid settings under 'dbms.connector.**'
			  if ( parts.Length != 4 )
			  {
					throw new InvalidSettingException( format( "Invalid connector setting: %s", key ) );
			  }

			  /*if ( !subSettings().contains( parts[3] ) )
			  {
			      return false;
			  }*/

			  // A type must be specified, or it is not possible to know who owns this setting
			  string groupKey = parts[2];
			  string typeKey = string.join( ".", parts[0], parts[1], groupKey, "type" );
			  string typeValue = rawConfig.get( typeKey );

			  if ( string.ReferenceEquals( typeValue, null ) )
			  {
					// We can infer the type of the connector from some names
					if ( groupKey.Equals( "http", StringComparison.OrdinalIgnoreCase ) || groupKey.Equals( "https", StringComparison.OrdinalIgnoreCase ) )
					{
						 typeValue = Connector.ConnectorType.Http.name();
					}
					else if ( groupKey.Equals( "bolt", StringComparison.OrdinalIgnoreCase ) )
					{
						 typeValue = Connector.ConnectorType.Bolt.name();
					}
			  }

			  // If this is a connector not called bolt or http, then we require the type
			  if ( string.ReferenceEquals( typeValue, null ) )
			  {
					throw new InvalidSettingException( format( "Missing mandatory value for '%s'", typeKey ) );
			  }

			  if ( !_validTypes.Contains( typeValue ) )
			  {
					throw new InvalidSettingException( format( "'%s' must be one of %s; not '%s'", typeKey, string.join( ", ", _validTypes ), typeValue ) );
			  }

			  return this.Type.name().Equals(typeValue);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.stream.Stream<java.util.Map.Entry<String,String>> ownedEntries(@Nonnull Map<String,String> params) throws org.neo4j.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual Stream<KeyValuePair<string, string>> OwnedEntries( IDictionary<string, string> @params )
		 {
			  return @params.entrySet().Where(it => Owns(it.Key, @params));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public java.util.Map<String,String> validate(@Nonnull Map<String,String> rawConfig, @Nonnull Consumer<String> warningConsumer) throws org.neo4j.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override IDictionary<string, string> Validate( IDictionary<string, string> rawConfig, Consumer<string> warningConsumer )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.HashMap<String,String> result = new java.util.HashMap<>();
			  Dictionary<string, string> result = new Dictionary<string, string>();

//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  OwnedEntries( rawConfig ).forEach( s => result.putAll( GetSettingFor( s.Key, rawConfig ).orElseThrow( () => new InvalidSettingException(format("Invalid connector setting: %s", s.Key)) ).validate(rawConfig, warningConsumer) ) );

			  WarnAboutDeprecatedConnectors( result, warningConsumer );

			  return result;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private void warnAboutDeprecatedConnectors(@Nonnull Map<String,String> connectorSettings, @Nonnull Consumer<String> warningConsumer)
		 private void WarnAboutDeprecatedConnectors( IDictionary<string, string> connectorSettings, Consumer<string> warningConsumer )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.HashSet<String> nonDefaultConnectors = new java.util.HashSet<>();
			  HashSet<string> nonDefaultConnectors = new HashSet<string>();
			  connectorSettings.entrySet().Select(DictionaryEntry.getKey).Where(settingKey =>
			  {
						  string name = settingKey.Split( "\\." )[2];
						  return IsDeprecatedConnectorName( name );
			  }).ForEach( nonDefaultConnectors.add );

			  if ( nonDefaultConnectors.Count > 0 )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					warningConsumer.accept( format( DeprecatedConnectorMsg, nonDefaultConnectors.OrderBy( c => c ).Select( s => format( ">  %s%n", s ) ).collect( joining() ) ) );
			  }
		 }

		 protected internal virtual bool IsDeprecatedConnectorName( string name )
		 {
			  return !( name.Equals( "http", StringComparison.OrdinalIgnoreCase ) || name.Equals( "https", StringComparison.OrdinalIgnoreCase ) || name.Equals( "bolt", StringComparison.OrdinalIgnoreCase ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public java.util.Map<String,Object> values(@Nonnull Map<String,String> params)
		 public override IDictionary<string, object> Values( IDictionary<string, string> @params )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.HashMap<String,Object> result = new java.util.HashMap<>();
			  Dictionary<string, object> result = new Dictionary<string, object>();

//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  OwnedEntries( @params ).forEach( s => result.putAll( GetSettingFor( s.Key, @params ).orElseThrow( () => new InvalidSettingException(format("Invalid connector setting: %s", s.Key)) ).values(@params) ) );

			  return result;
		 }

		 /// 
		 /// <returns> a setting which is not necessarily literally defined in the map provided </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull protected abstract java.util.Optional<org.neo4j.graphdb.config.Setting<Object>> getSettingFor(@Nonnull String settingName, @Nonnull Map<String,String> params);
		 protected internal abstract Optional<Setting<object>> GetSettingFor( string settingName, IDictionary<string, string> @params );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public java.util.List<org.neo4j.graphdb.config.Setting<Object>> settings(@Nonnull Map<String,String> params)
		 public override IList<Setting<object>> Settings( IDictionary<string, string> @params )
		 {
			  return OwnedEntries( @params ).map( e => GetSettingFor( e.Key, @params ) ).filter( Optional.isPresent ).map( Optional.get ).collect( toList() );
		 }

		 public override bool Deprecated()
		 {
			  return false;
		 }

		 public override Optional<string> Replacement()
		 {
			  return null;
		 }

		 public override bool Internal()
		 {
			  return false;
		 }

		 public override bool Secret()
		 {
			  return false;
		 }

		 public override Optional<string> DocumentedDefaultValue()
		 {
			  return null;
		 }

		 public override string ValueDescription()
		 {
			  return "a group of connector settings";
		 }

		 public override Optional<string> Description()
		 {
			  return null;
		 }

		 public override bool Dynamic()
		 {
			  return false;
		 }
	}

}