using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

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

	using ConfigOptions = Neo4Net.Configuration.ConfigOptions;
	using ConfigValue = Neo4Net.Configuration.ConfigValue;
	using ExternalSettings = Neo4Net.Configuration.ExternalSettings;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Secret = Neo4Net.Configuration.Secret;
	using Neo4Net.Graphdb.config;
	using Configuration = Neo4Net.Graphdb.config.Configuration;
	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Neo4Net.Graphdb.config;
	using Neo4Net.Graphdb.config;
	using SettingValidator = Neo4Net.Graphdb.config.SettingValidator;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using DiagnosticsPhase = Neo4Net.@internal.Diagnostics.DiagnosticsPhase;
	using DiagnosticsProvider = Neo4Net.@internal.Diagnostics.DiagnosticsProvider;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;
	using Neo4Net.Kernel.impl.util;
	using BufferingLog = Neo4Net.Logging.BufferingLog;
	using Log = Neo4Net.Logging.Log;
	using Logger = Neo4Net.Logging.Logger;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Connector.ConnectorType.BOLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Connector.ConnectorType.HTTP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.HttpConnector.Encryption.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.HttpConnector.Encryption.TLS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;

	/// <summary>
	/// This class holds the overall configuration of a Neo4j database instance. Use the accessors to convert the internal
	/// key-value settings to other types.
	/// <para>
	/// Users can assume that old settings have been migrated to their new counterparts, and that defaults have been
	/// applied.
	/// </para>
	/// </summary>
	public class Config : DiagnosticsProvider, Configuration
	{
		 public const string DEFAULT_CONFIG_FILE_NAME = "neo4j.conf";

		 private readonly IList<ConfigOptions> _configOptions;

		 private readonly IDictionary<string, string> @params = new CopyOnWriteHashMap<string, string>(); // Read heavy workload
		 private readonly IDictionary<string, ICollection<System.Action<string, string>>> _updateListeners = new ConcurrentDictionary<string, ICollection<System.Action<string, string>>>();
		 private readonly ConfigurationMigrator _migrator;
		 private readonly IList<ConfigurationValidator> _validators = new List<ConfigurationValidator>();
		 private readonly IDictionary<string, string> _overriddenDefaults = new CopyOnWriteHashMap<string, string>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<String,org.neo4j.graphdb.config.BaseSetting<?>> settingsMap;
		 private readonly IDictionary<string, BaseSetting<object>> _settingsMap; // Only contains fixed settings and not groups

		 // Messages to this log get replayed into a real logger once logging has been instantiated.
		 private Log _log = new BufferingLog();

		 /// <summary>
		 /// Builder class for a configuration.
		 /// <para>
		 /// The configuration has three layers of values:
		 /// <ol>
		 ///   <li>Defaults settings, which is provided by validators.
		 ///   <li>File settings, parsed from the configuration file if one is provided.
		 ///   <li>Overridden settings, as provided by the user with the <seealso cref="Builder.withSettings(System.Collections.IDictionary)"/> methods.
		 /// </ol>
		 /// They are added in the order specified, and is thus overridden by each layer.
		 /// </para>
		 /// <para>
		 /// Although the builder allows you to override the <seealso cref="LoadableConfig"/>'s with <code>withConfigClasses</code>,
		 /// this functionality is mainly for testing. If no classes are provided to the builder, they will be located through
		 /// service loading, and this is probably what you want in most of the cases.
		 /// </para>
		 /// <para>
		 /// Loaded <seealso cref="LoadableConfig"/>'s, whether provided though service loading or explicitly passed, will be scanned
		 /// for validators that provides migration, validation and default values. Migrators can be specified with the
		 /// <seealso cref="Migrator"/> annotation and should reside in a class implementing <seealso cref="LoadableConfig"/>.
		 /// </para>
		 /// </summary>
		 public class Builder
		 {
			  internal IDictionary<string, string> InitialSettings = stringMap();
			  internal IDictionary<string, string> OverriddenDefaults = stringMap();
			  internal IList<ConfigurationValidator> Validators = new List<ConfigurationValidator>();
			  internal File ConfigFile;
			  internal IList<LoadableConfig> SettingsClasses;
			  internal bool ConnectorsDisabled;
			  internal bool ThrowOnFileLoadFailure = true;

			  /// <summary>
			  /// Augment the configuration with the passed setting.
			  /// </summary>
			  /// <param name="setting"> The setting to set. </param>
			  /// <param name="value"> The value of the setting, pre parsed. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Builder withSetting(final org.neo4j.graphdb.config.Setting<?> setting, final String value)
			  public virtual Builder WithSetting<T1>( Setting<T1> setting, string value )
			  {
					return WithSetting( setting.Name(), value );
			  }

			  /// <summary>
			  /// Augment the configuration with the passed setting.
			  /// </summary>
			  /// <param name="setting"> The setting to set. </param>
			  /// <param name="value"> The value of the setting, pre parsed. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Builder withSetting(final String setting, final String value)
			  public virtual Builder WithSetting( string setting, string value )
			  {
					InitialSettings[setting] = value;
					return this;
			  }

			  /// <summary>
			  /// Augment the configuration with the passed settings.
			  /// </summary>
			  /// <param name="initialSettings"> settings to augment the configuration with. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Builder withSettings(final java.util.Map<String,String> initialSettings)
			  public virtual Builder WithSettings( IDictionary<string, string> initialSettings )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					this.InitialSettings.putAll( initialSettings );
					return this;
			  }

			  /// <summary>
			  /// Set the classes that contains the <seealso cref="Setting"/> fields. If no classes are provided to the builder, they
			  /// will be located through service loading.
			  /// </summary>
			  /// <param name="loadableConfigs"> A collection fo class instances providing settings. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withConfigClasses(final java.util.Collection<? extends org.neo4j.configuration.LoadableConfig> loadableConfigs)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual Builder WithConfigClasses<T1>( ICollection<T1> loadableConfigs ) where T1 : Neo4Net.Configuration.LoadableConfig
			  {
					if ( SettingsClasses == null )
					{
						 SettingsClasses = new List<LoadableConfig>();
					}
					( ( IList<LoadableConfig> )SettingsClasses ).AddRange( loadableConfigs );
					return this;
			  }

			  /// <summary>
			  /// Provide an additional validator. Validators are automatically localed within classes with
			  /// <seealso cref="LoadableConfig"/>, but this allows you to add others.
			  /// </summary>
			  /// <param name="validator"> an additional validator. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withValidator(final ConfigurationValidator validator)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual Builder WithValidator( ConfigurationValidator validator )
			  {
					this.Validators.Add( validator );
					return this;
			  }

			  /// <seealso cref= Builder#withValidator(ConfigurationValidator) </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withValidators(final java.util.Collection<ConfigurationValidator> validators)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual Builder WithValidators( ICollection<ConfigurationValidator> validators )
			  {
					( ( IList<ConfigurationValidator> )this.Validators ).AddRange( validators );
					return this;
			  }

			  /// <summary>
			  /// Extends config with defaults for server, i.e. auth and connector settings.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withServerDefaults()
			  public virtual Builder WithServerDefaults()
			  {
					// Add server defaults
					HttpConnector http = new HttpConnector( "http", NONE );
					HttpConnector https = new HttpConnector( "https", TLS );
					BoltConnector bolt = new BoltConnector( "bolt" );
					OverriddenDefaults[GraphDatabaseSettings.auth_enabled.name()] = TRUE;
					OverriddenDefaults[http.Enabled.name()] = TRUE;
					OverriddenDefaults[https.Enabled.name()] = TRUE;
					OverriddenDefaults[bolt.Enabled.name()] = TRUE;

					return this;
			  }

			  /// <summary>
			  /// Provide a file for initial configuration. The settings added with the <seealso cref="Builder.withSettings(System.Collections.IDictionary)"/>
			  /// methods will be applied on top of the settings in the file.
			  /// </summary>
			  /// <param name="configFile"> A configuration file to parse for initial settings. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withFile(final @Nullable File configFile)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual Builder WithFile( File configFile )
			  {
					this.ConfigFile = configFile;
					return this;
			  }

			  /// <seealso cref= Builder#withFile(File) </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withFile(final java.nio.file.Path configFile)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual Builder WithFile( Path configFile )
			  {
					return WithFile( configFile.toFile() );
			  }

			  /// <param name="configFile"> an optional configuration file. If not present, this call changes nothing. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withFile(java.util.Optional<java.io.File> configFile)
			  public virtual Builder WithFile( Optional<File> configFile )
			  {
					configFile.ifPresent( file => this.ConfigFile = file );
					return this;
			  }

			  /// <summary>
			  /// Specifies the neo4j home directory to be set for this particular config. This will modify {@link
			  /// GraphDatabaseSettings#neo4j_home} to the same value as provided. If this is not called, the home directory
			  /// will be set to a system specific default home directory.
			  /// </summary>
			  /// <param name="homeDir"> The home directory this config belongs to. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withHome(final java.io.File homeDir)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual Builder WithHome( File homeDir )
			  {
					InitialSettings[GraphDatabaseSettings.neo4j_home.name()] = homeDir.AbsolutePath;
					return this;
			  }

			  /// <seealso cref= Builder#withHome(File) </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withHome(final java.nio.file.Path homeDir)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual Builder WithHome( Path homeDir )
			  {
					return WithHome( homeDir.toFile() );
			  }

			  /// <summary>
			  /// This will force all connectors to be disabled during creation of the config. This can be useful if an
			  /// offline mode is wanted, e.g. in dbms tools or test environments.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Builder withConnectorsDisabled()
			  public virtual Builder WithConnectorsDisabled()
			  {
					ConnectorsDisabled = true;
					return this;
			  }

			  /// <summary>
			  /// Prevent the <seealso cref="build()"/> method from throwing an <seealso cref="UncheckedIOException"/> if the given {@code withFile} configuration file could not be
			  /// loaded for some reason. Instead, an error will be logged. The defualt behaviour is to throw the exception.
			  /// </summary>
			  public virtual Builder WithNoThrowOnFileLoadFailure()
			  {
					ThrowOnFileLoadFailure = false;
					return this;
			  }

			  /// <returns> The config reflecting the state of the builder. </returns>
			  /// <exception cref="InvalidSettingException"> is thrown if an invalid setting is encountered and {@link
			  /// GraphDatabaseSettings#strict_config_validation} is true. </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public Config build() throws org.neo4j.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual Config Build()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					IList<LoadableConfig> loadableConfigs = Optional.ofNullable( SettingsClasses ).orElseGet( LoadableConfig::allConfigClasses );

					// If reading from a file, make sure we always have a neo4j_home
					if ( ConfigFile != null && !InitialSettings.ContainsKey( GraphDatabaseSettings.neo4j_home.name() ) )
					{
						 InitialSettings[GraphDatabaseSettings.neo4j_home.name()] = System.getProperty("user.dir");
					}

					Config config = new Config( ConfigFile, ThrowOnFileLoadFailure, InitialSettings, OverriddenDefaults, Validators, loadableConfigs );

					if ( ConnectorsDisabled )
					{
						 config.Augment( config.AllConnectorIdentifiers().ToDictionary(id => (new Connector(id)).Enabled.name(), id => Settings.FALSE) );
					}

					return config;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static Builder builder()
		 public static Builder Builder()
		 {
			  return new Builder();
		 }

		 /// <summary>
		 /// Convenient method for starting building from a file.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static Builder fromFile(@Nullable final java.io.File configFile)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static Builder FromFile( File configFile )
		 {
			  return Builder().withFile(configFile);
		 }

		 /// <summary>
		 /// Convenient method for starting building from a file.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static Builder fromFile(@Nonnull final java.nio.file.Path configFile)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static Builder FromFile( Path configFile )
		 {
			  return Builder().withFile(configFile);
		 }

		 /// <summary>
		 /// Convenient method for starting building from initial settings.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static Builder fromSettings(final java.util.Map<String,String> initialSettings)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static Builder FromSettings( IDictionary<string, string> initialSettings )
		 {
			  return Builder().withSettings(initialSettings);
		 }

		 /// <returns> a configuration with default values. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static Config defaults()
		 public static Config Defaults()
		 {
			  return Builder().build();
		 }

		 /// <param name="initialSettings"> a map with settings to be present in the config. </param>
		 /// <returns> a configuration with default values augmented with the provided <code>initialSettings</code>. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static Config defaults(@Nonnull final java.util.Map<String,String> initialSettings)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static Config Defaults( IDictionary<string, string> initialSettings )
		 {
			  return Builder().withSettings(initialSettings).build();
		 }

		 /// <summary>
		 /// Constructs a <code>Config</code> with default values and sets the supplied <code>setting</code> to the <code>value</code>. </summary>
		 /// <param name="setting"> The initial setting to use. </param>
		 /// <param name="value"> The initial value to give the setting. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public static Config defaults(@Nonnull final org.neo4j.graphdb.config.Setting<?> setting, final String value)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static Config Defaults<T1>( Setting<T1> setting, string value )
		 {
			  return Builder().withSetting(setting, value).build();
		 }

		 private Config( File configFile, bool throwOnFileLoadFailure, IDictionary<string, string> initialSettings, IDictionary<string, string> overriddenDefaults, ICollection<ConfigurationValidator> additionalValidators, IList<LoadableConfig> settingsClasses )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  _configOptions = settingsClasses.Select( LoadableConfig::getConfigOptions ).flatMap( System.Collections.IList.stream ).ToList();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: settingsMap = new java.util.HashMap<>();
			  _settingsMap = new Dictionary<string, BaseSetting<object>>();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  _configOptions.Select( ConfigOptions::settingGroup ).Where( typeof( BaseSetting ).isInstance ).Select( typeof( BaseSetting ).cast ).ForEach( setting => _settingsMap.put( setting.name(), setting ) );

			  ( ( IList<ConfigurationValidator> )_validators ).AddRange( additionalValidators );
			  _migrator = new AnnotationBasedConfigurationMigrator( settingsClasses );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  this._overriddenDefaults.putAll( overriddenDefaults );

			  bool fromFile = configFile != null;
			  if ( fromFile )
			  {
					LoadFromFile( configFile, _log, throwOnFileLoadFailure, initialSettings );
			  }

			  overriddenDefaults.forEach( initialSettings.putIfAbsent );

			  MigrateAndValidateAndUpdateSettings( initialSettings, fromFile );

			  // Only warn for deprecations if red from a file
			  if ( fromFile )
			  {
					WarnAboutDeprecations( @params );
			  }
		 }

		 /// <summary>
		 /// Retrieves a configuration value. If no value is configured, a default value will be returned instead. Note that
		 /// {@code null} is a valid value.
		 /// </summary>
		 /// <param name="setting"> The configuration property. </param>
		 /// @param <T> the underlying type of the setting. </param>
		 /// <returns> the value of the given setting, {@code null} can be returned. </returns>
		 public override T Get<T>( Setting<T> setting )
		 {
			  return setting.apply( @params.get );
		 }

		 /// <summary>
		 /// Test whether a setting is configured or not. Can be used to check if default value will be returned or not.
		 /// </summary>
		 /// <param name="setting"> The setting to check. </param>
		 /// <returns> {@code true} if the setting is configures, {@code false} otherwise implying that the default value will
		 /// be returned if applicable. </returns>
		 public virtual bool IsConfigured<T1>( Setting<T1> setting )
		 {
			  return @params.ContainsKey( setting.Name() );
		 }

		 /// <summary>
		 /// Returns the currently configured identifiers for grouped settings.
		 /// 
		 /// Identifiers for groups exists to allow multiple configured settings of the same setting type.
		 /// E.g. giving that prefix of a group is {@code dbms.ssl.policy} and the following settings are configured:
		 /// <ul>
		 /// <li> {@code dbms.ssl.policy.default.base_directory}
		 /// <li> {@code dbms.ssl.policy.other.base_directory}
		 /// </ul>
		 /// a call to this will method return {@code ["default", "other"]}.
		 /// <para>
		 /// The key difference to these identifiers are that they are only known at runtime after a valid configuration is
		 /// parsed and validated.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="groupClass"> A class that represents a setting group. Must be annotated with <seealso cref="Group"/> </param>
		 /// <returns> A set of configured identifiers for the given group. </returns>
		 /// <exception cref="IllegalArgumentException"> if the provided class is not annotated with <seealso cref="Group"/>. </exception>
		 public virtual ISet<string> IdentifiersFromGroup( Type groupClass )
		 {
			  if ( !groupClass.isAnnotationPresent( typeof( Group ) ) )
			  {
					throw new System.ArgumentException( "Class must be annotated with @Group" );
			  }

			  string prefix = groupClass.getAnnotation( typeof( Group ) ).value();
			  Pattern pattern = Pattern.compile( Pattern.quote( prefix ) + "\\.([^.]+)\\.(.+)" );

			  ISet<string> identifiers = new SortedSet<string>();
			  foreach ( string setting in @params.Keys )
			  {
					Matcher matcher = pattern.matcher( setting );
					if ( matcher.matches() )
					{
						 identifiers.Add( matcher.group( 1 ) );
					}
			  }
			  return identifiers;
		 }

		 /// <summary>
		 /// Augment the existing config with new settings, overriding any conflicting settings, but keeping all old
		 /// non-overlapping ones.
		 /// </summary>
		 /// <param name="settings"> to add and override. </param>
		 /// <exception cref="InvalidSettingException"> when and invalid setting is found and {@link
		 /// GraphDatabaseSettings#strict_config_validation} is true. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void augment(java.util.Map<String,String> settings) throws org.neo4j.graphdb.config.InvalidSettingException
		 public virtual void Augment( IDictionary<string, string> settings )
		 {
			  MigrateAndValidateAndUpdateSettings( settings, false );
		 }

		 /// <seealso cref= Config#augment(Map) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void augment(String setting, String value) throws org.neo4j.graphdb.config.InvalidSettingException
		 public virtual void Augment( string setting, string value )
		 {
			  Augment( singletonMap( setting, value ) );
		 }

		 /// <seealso cref= Config#augment(Map) </seealso>
		 public virtual void Augment<T1>( Setting<T1> setting, string value )
		 {
			  Augment( setting.Name(), value );
		 }

		 /// <summary>
		 /// Augment the existing config with new settings, overriding any conflicting settings, but keeping all old
		 /// non-overlapping ones.
		 /// </summary>
		 /// <param name="config"> config to add and override with. </param>
		 /// <exception cref="InvalidSettingException"> when and invalid setting is found and {@link
		 /// GraphDatabaseSettings#strict_config_validation} is true. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void augment(Config config) throws org.neo4j.graphdb.config.InvalidSettingException
		 public virtual void Augment( Config config )
		 {
			  Augment( config.@params );
		 }

		 /// <summary>
		 /// Augment the existing config with new settings, ignoring any conflicting settings.
		 /// </summary>
		 /// <param name="setting"> settings to add and override </param>
		 /// <exception cref="InvalidSettingException"> when and invalid setting is found and {@link
		 /// GraphDatabaseSettings#strict_config_validation} is true. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void augmentDefaults(org.neo4j.graphdb.config.Setting<?> setting, String value) throws org.neo4j.graphdb.config.InvalidSettingException
		 public virtual void AugmentDefaults<T1>( Setting<T1> setting, string value )
		 {
			  _overriddenDefaults[setting.Name()] = value;
			  if ( !@params.ContainsKey( setting.Name() ) ) @params.Add(setting.Name(), value);
		 }

		 /// <summary>
		 /// Specify a log where errors and warnings will be reported. Log messages that happens prior to setting a logger
		 /// will be buffered and replayed onto the first logger that is set.
		 /// </summary>
		 /// <param name="log"> to use. </param>
		 public virtual Log Logger
		 {
			 set
			 {
				  if ( this._log is BufferingLog )
				  {
						( ( BufferingLog ) this._log ).replayInto( value );
				  }
				  this._log = value;
			 }
		 }

		 /// <param name="key"> to lookup in the config </param>
		 /// <returns> the value or none if it doesn't exist in the config </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public java.util.Optional<String> getRaw(@Nonnull String key)
		 public virtual Optional<string> GetRaw( string key )
		 {
			  return Optional.ofNullable( @params[key] );
		 }

		 /// <returns> a copy of the raw configuration map </returns>
		 public virtual IDictionary<string, string> GetRaw()
		 {
			  return new Dictionary<string, string>( @params );
		 }

		 /// <returns> a configured setting </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public java.util.Optional<Object> getValue(@Nonnull String key)
		 public virtual Optional<object> GetValue( string key )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _configOptions.Select( it => it.asConfigValues( @params ) ).flatMap( System.Collections.IList.stream ).Where( it => it.name().Equals(key) ).Select(ConfigValue::value).First().orElseGet(Optional.empty);
		 }

		 /// <summary>
		 /// Updates a provided setting to a given value. This method is intended to be used for changing settings during
		 /// runtime. If you want to change settings at startup, use <seealso cref="Config.augment"/>.
		 /// 
		 /// @implNote No migration or config validation is done. If you need this you have to refactor this method.
		 /// </summary>
		 /// <param name="setting"> The setting to set to the specified value. </param>
		 /// <param name="update"> The new value to set, passing {@code null} or the empty string should reset the value back to default value. </param>
		 /// <param name="origin"> The source of the change, e.g. {@code dbms.setConfigValue()}. </param>
		 /// <exception cref="IllegalArgumentException"> if the provided setting is unknown or not dynamic. </exception>
		 /// <exception cref="InvalidSettingException"> if the value is not formatted correctly. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void updateDynamicSetting(String setting, String update, String origin) throws IllegalArgumentException, org.neo4j.graphdb.config.InvalidSettingException
		 public virtual void UpdateDynamicSetting( string setting, string update, string origin )
		 {
			  VerifyValidDynamicSetting( setting );

			  lock ( @params )
			  {
					bool oldValueIsDefault = false;
					bool newValueIsDefault = false;
					string oldValue;
					string newValue;
					if ( string.ReferenceEquals( update, null ) || update.Length == 0 )
					{
						 // Empty means we want to delete the configured value and fallback to the default value
						 string overriddenDefault = _overriddenDefaults[setting];
						 bool hasDefault = !string.ReferenceEquals( overriddenDefault, null );
						 oldValue = hasDefault ? @params.put( setting, overriddenDefault ) : @params.Remove( setting );
						 newValue = GetDefaultValueOf( setting );
						 newValueIsDefault = true;
					}
					else
					{
						 // Change setting, make sure it's valid
						 IDictionary<string, string> newEntry = stringMap( setting, update );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
						 IList<SettingValidator> settingValidators = _configOptions.Select( ConfigOptions::settingGroup ).ToList();
						 foreach ( SettingValidator validator in settingValidators )
						 {
							  validator.Validate(newEntry, ignore =>
							  {
							  }); // Throws if invalid
						 }

						 string previousValue = @params[setting] = update;
						 if ( !string.ReferenceEquals( previousValue, null ) )
						 {
							  oldValue = previousValue;
						 }
						 else
						 {
							  oldValue = GetDefaultValueOf( setting );
							  oldValueIsDefault = true;
						 }
						 newValue = update;
					}

					string oldValueForLog = ObsfucateIfSecret( setting, oldValue );
					string newValueForLog = ObsfucateIfSecret( setting, newValue );
					_log.info( "Setting changed: '%s' changed from '%s' to '%s' via '%s'", setting, oldValueIsDefault ? "default (" + oldValueForLog + ")" : oldValueForLog, newValueIsDefault ? "default (" + newValueForLog + ")" : newValueForLog, origin );
					_updateListeners.getOrDefault( setting, emptyList() ).forEach(l => l.accept(oldValue, newValue));
			  }
		 }

		 private void VerifyValidDynamicSetting( string setting )
		 {
			  Optional<ConfigValue> option = FindConfigValue( setting );

			  if ( !option.Present )
			  {
					throw new System.ArgumentException( "Unknown setting: " + setting );
			  }

			  ConfigValue configValue = option.get();
			  if ( !configValue.Dynamic() )
			  {
					throw new System.ArgumentException( "Setting is not dynamic and can not be changed at runtime" );
			  }
		 }

		 private string GetDefaultValueOf( string setting )
		 {
			  if ( _overriddenDefaults.ContainsKey( setting ) )
			  {
					return _overriddenDefaults[setting];
			  }
			  if ( _settingsMap.ContainsKey( setting ) )
			  {
					return _settingsMap[setting].DefaultValue;
			  }
			  return "<no default>";
		 }

		 private Optional<ConfigValue> FindConfigValue( string setting )
		 {
			  return _configOptions.Select( it => it.asConfigValues( @params ) ).flatMap( System.Collections.IList.stream ).Where( it => it.name().Equals(setting) ).First();
		 }

		 /// <summary>
		 /// Register a listener for dynamic updates to the given setting.
		 /// <para>
		 /// The listener will get called whenever the <seealso cref="updateDynamicSetting(string, string, string)"/> method is used
		 /// to change the given setting, and the listener will be supplied the parsed values of the old and the new
		 /// configuration value.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="setting"> The <seealso cref="Setting"/> to listen for changes to. </param>
		 /// <param name="listener"> The listener callback that will be notified of any configuration changes to the given setting. </param>
		 /// @param <V> The value type of the setting. </param>
		 public virtual void RegisterDynamicUpdateListener<V>( Setting<V> setting, System.Action<V, V> listener )
		 {
			  string settingName = setting.Name();
			  VerifyValidDynamicSetting( settingName );
			  System.Action<string, string> projectedListener = ( oldValStr, newValStr ) =>
			  {
				try
				{
					 V oldVal = setting.apply( s => oldValStr );
					 V newVal = setting.apply( s => newValStr );
					 listener( oldVal, newVal );
				}
				catch ( Exception e )
				{
					 _log.error( "Failure when notifying listeners after dynamic setting change; " + "new setting might not have taken effect: " + e.Message, e );
				}
			  };
			  _updateListeners.computeIfAbsent( settingName, k => new ConcurrentLinkedQueue<>() ).add(projectedListener);
		 }

		 /// <returns> all effective config values </returns>
		 public virtual IDictionary<string, ConfigValue> ConfigValues
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				  return _configOptions.Select( it => it.asConfigValues( @params ) ).flatMap( System.Collections.IList.stream ).ToDictionary(ConfigValue::name, it => it, (val1, val2) =>
				  {
							  throw new Exception( "Duplicate setting: " + val1.name() + ": " + val1 + " and " + val2 );
				  });
			 }
		 }

		 public virtual string DiagnosticsIdentifier
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return this.GetType().FullName;
			 }
		 }

		 public override void AcceptDiagnosticsVisitor( object visitor )
		 {
			  // nothing visits configuration
		 }

		 public override void Dump( DiagnosticsPhase phase, Logger logger )
		 {
			  if ( phase.Initialization || phase.ExplicitlyRequested )
			  {
					logger.Log( "Neo4j Kernel properties:" );
					foreach ( KeyValuePair<string, string> param in @params.SetOfKeyValuePairs() )
					{
						 logger.Log( "%s=%s", param.Key, ObsfucateIfSecret( param ) );
					}
			  }
		 }

		 private string ObsfucateIfSecret( KeyValuePair<string, string> param )
		 {
			  return ObsfucateIfSecret( param.Key, param.Value );
		 }

		 private string ObsfucateIfSecret( string key, string value )
		 {
			  if ( _settingsMap.ContainsKey( key ) && _settingsMap[key].secret() )
			  {
					return Secret.OBSFUCATED;
			  }
			  else
			  {
					return value;
			  }
		 }

		 /// <summary>
		 /// Migrates and validates all string values in the provided <code>settings</code> map.
		 /// 
		 /// This will update the configuration with the provided values regardless whether errors are encountered or not.
		 /// </summary>
		 /// <param name="settings"> the settings to migrate and validate. </param>
		 /// <param name="warnOnUnknownSettings"> if true method log messages to <seealso cref="Config.log"/>. </param>
		 /// <exception cref="InvalidSettingException"> when and invalid setting is found and {@link
		 /// GraphDatabaseSettings#strict_config_validation} is true. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void migrateAndValidateAndUpdateSettings(java.util.Map<String,String> settings, boolean warnOnUnknownSettings) throws org.neo4j.graphdb.config.InvalidSettingException
		 private void MigrateAndValidateAndUpdateSettings( IDictionary<string, string> settings, bool warnOnUnknownSettings )
		 {
			  IDictionary<string, string> migratedSettings = MigrateSettings( settings );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  @params.putAll( migratedSettings );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<SettingValidator> settingValidators = _configOptions.Select( ConfigOptions::settingGroup ).ToList();

			  // Validate settings
			  IDictionary<string, string> additionalSettings = ( new IndividualSettingsValidator( settingValidators, warnOnUnknownSettings ) ).Validate( this, _log );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  @params.putAll( additionalSettings );

			  // Validate configuration
			  foreach ( ConfigurationValidator validator in _validators )
			  {
					validator.Validate( this, _log );
			  }
		 }

		 private IDictionary<string, string> MigrateSettings( IDictionary<string, string> settings )
		 {
			  return _migrator.apply( settings, _log );
		 }

		 private void WarnAboutDeprecations( IDictionary<string, string> userSettings )
		 {
			  _configOptions.stream().flatMap(it => it.asConfigValues(userSettings).stream()).filter(config => userSettings.ContainsKey(config.name()) && config.deprecated()).forEach(c =>
			  {
						  if ( c.replacement().Present )
						  {
								_log.warn( "%s is deprecated. Replaced by %s", c.name(), c.replacement().get() );
						  }
						  else
						  {
								_log.warn( "%s is deprecated.", c.name() );
						  }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private static void loadFromFile(@Nonnull File file, @Nonnull Log log, boolean throwOnFileLoadFailure, java.util.Map<String,String> into)
		 private static void LoadFromFile( File file, Log log, bool throwOnFileLoadFailure, IDictionary<string, string> into )
		 {
			  if ( !file.exists() )
			  {
					if ( throwOnFileLoadFailure )
					{
						 throw new ConfigLoadIOException( new IOException( "Config file [" + file + "] does not exist." ) );
					}
					log.warn( "Config file [%s] does not exist.", file );
					return;
			  }
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("MismatchedQueryAndUpdateOfCollection") java.util.Properties loader = new java.util.Properties()
					Properties loader = new PropertiesAnonymousInnerClass( log, into );
					using ( FileStream stream = new FileStream( file, FileMode.Open, FileAccess.Read ) )
					{
						 loader.load( stream );
					}
			  }
			  catch ( IOException e )
			  {
					if ( throwOnFileLoadFailure )
					{
						 throw new ConfigLoadIOException( "Unable to load config file [" + file + "].", e );
					}
					log.error( "Unable to load config file [%s]: %s", file, e.Message );
			  }
		 }

		 private class PropertiesAnonymousInnerClass : Properties
		 {
			 private Log _log;
			 private IDictionary<string, string> _into;

			 public PropertiesAnonymousInnerClass( Log log, IDictionary<string, string> into )
			 {
				 this._log = log;
				 this._into = into;
			 }

			 public override object put( object key, object val )
			 {
				  string setting = key.ToString();
				  string value = val.ToString();
				  // We use the 'super' Hashtable as a set of all the settings we have logged warnings about.
				  // We only want to warn about each duplicate setting once.
				  if ( _into.putIfAbsent( setting, value ) != null && base.put( key, val ) == null && !key.Equals( ExternalSettings.additionalJvm.name() ) )
				  {
						_log.warn( "The '%s' setting is specified more than once. Settings only be specified once, to avoid ambiguity. " + "The setting value that will be used is '%s'.", setting, _into[setting] );
				  }
				  return null;
			 }
		 }

		 /// <returns> a list of all connector names like 'http' in 'dbms.connector.http.enabled = true' </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.Set<String> allConnectorIdentifiers()
		 public virtual ISet<string> AllConnectorIdentifiers()
		 {
			  return AllConnectorIdentifiers( @params );
		 }

		 /// <returns> a list of all connector names like 'http' in 'dbms.connector.http.enabled = true' </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.Set<String> allConnectorIdentifiers(@Nonnull Map<String,String> params)
		 public virtual ISet<string> AllConnectorIdentifiers( IDictionary<string, string> @params )
		 {
			  return IdentifiersFromGroup( typeof( Connector ) );
		 }

		 /// <returns> list of all configured bolt connectors </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.List<BoltConnector> boltConnectors()
		 public virtual IList<BoltConnector> BoltConnectors()
		 {
			  return BoltConnectors( @params ).collect( Collectors.toList() );
		 }

		 /// <returns> stream of all configured bolt connectors </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private java.util.stream.Stream<BoltConnector> boltConnectors(@Nonnull Map<String,String> params)
		 private Stream<BoltConnector> BoltConnectors( IDictionary<string, string> @params )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return AllConnectorIdentifiers( @params ).Select( BoltConnector::new ).Where( c => c.group.groupKey.equalsIgnoreCase( "bolt" ) || BOLT == c.type.apply( @params.get ) );
		 }

		 /// <returns> list of all configured bolt connectors which are enabled </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.List<BoltConnector> enabledBoltConnectors()
		 public virtual IList<BoltConnector> EnabledBoltConnectors()
		 {
			  return EnabledBoltConnectors( @params );
		 }

		 /// <returns> list of all configured bolt connectors which are enabled </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.List<BoltConnector> enabledBoltConnectors(@Nonnull Map<String,String> params)
		 public virtual IList<BoltConnector> EnabledBoltConnectors( IDictionary<string, string> @params )
		 {
			  return BoltConnectors( @params ).filter( c => c.enabled.apply( @params.get ) ).collect( Collectors.toList() );
		 }

		 /// <returns> list of all configured http connectors </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.List<HttpConnector> httpConnectors()
		 public virtual IList<HttpConnector> HttpConnectors()
		 {
			  return HttpConnectors( @params ).collect( Collectors.toList() );
		 }

		 /// <returns> stream of all configured http connectors </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private java.util.stream.Stream<HttpConnector> httpConnectors(@Nonnull Map<String,String> params)
		 private Stream<HttpConnector> HttpConnectors( IDictionary<string, string> @params )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return AllConnectorIdentifiers( @params ).Select( Connector::new ).Where( c => c.group.groupKey.equalsIgnoreCase( "http" ) || c.group.groupKey.equalsIgnoreCase( "https" ) || HTTP == c.type.apply( @params.get ) ).Select(c =>
			  {
						  string name = c.group.groupKey;
						  Encryption defaultEncryption;
						  switch ( name )
						  {
						  case "https":
								defaultEncryption = TLS;
								break;
						  case "http":
						  default:
								defaultEncryption = NONE;
								break;
						  }

						  return new HttpConnector( name, HttpConnectorValidator.EncryptionSetting( name, defaultEncryption ).apply( @params.get ) );
			  });
		 }

		 /// <returns> list of all configured http connectors which are enabled </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.List<HttpConnector> enabledHttpConnectors()
		 public virtual IList<HttpConnector> EnabledHttpConnectors()
		 {
			  return EnabledHttpConnectors( @params );
		 }

		 /// <returns> list of all configured http connectors which are enabled </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private java.util.List<HttpConnector> enabledHttpConnectors(@Nonnull Map<String,String> params)
		 private IList<HttpConnector> EnabledHttpConnectors( IDictionary<string, string> @params )
		 {
			  return HttpConnectors( @params ).filter( c => c.enabled.apply( @params.get ) ).collect( Collectors.toList() );
		 }

		 public override string ToString()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return @params.SetOfKeyValuePairs().OrderBy(System.Collections.IComparer.comparing(DictionaryEntry.getKey)).Select(entry => entry.Key + "=" + ObsfucateIfSecret(entry)).collect(Collectors.joining(", "));
		 }
	}

}