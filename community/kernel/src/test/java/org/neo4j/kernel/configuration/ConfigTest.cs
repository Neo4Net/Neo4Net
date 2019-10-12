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
namespace Org.Neo4j.Kernel.configuration
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using InOrder = org.mockito.InOrder;


	using DocumentedDefaultValue = Org.Neo4j.Configuration.DocumentedDefaultValue;
	using Dynamic = Org.Neo4j.Configuration.Dynamic;
	using ExternalSettings = Org.Neo4j.Configuration.ExternalSettings;
	using Internal = Org.Neo4j.Configuration.Internal;
	using LoadableConfig = Org.Neo4j.Configuration.LoadableConfig;
	using ReplacedBy = Org.Neo4j.Configuration.ReplacedBy;
	using Secret = Org.Neo4j.Configuration.Secret;
	using InvalidSettingException = Org.Neo4j.Graphdb.config.InvalidSettingException;
	using Org.Neo4j.Graphdb.config;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Log = Org.Neo4j.Logging.Log;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	public class ConfigTest
	{
		 private const string ORIGIN = "test";

		 public class MyMigratingSettings : LoadableConfig
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Migrator public static ConfigurationMigrator migrator = new BaseConfigurationMigrator()
			  public static ConfigurationMigrator migrator = new BaseConfigurationMigratorAnonymousInnerClass();

			  private class BaseConfigurationMigratorAnonymousInnerClass : BaseConfigurationMigrator
			  {
	//			  {
	//					add(new SpecificPropertyMigration("old", "Old has been replaced by newer!")
	//					{
	//						 @@Override public void setValueWithOldSetting(String value, Map<String,String> rawConfiguration)
	//						 {
	//							  rawConfiguration.put(newer.name(), value);
	//						 }
	//					}
	//				  );
	//			  }
			  }

			  public static Setting<string> Newer = setting( "newer", STRING, "" );
		 }

		 public class MySettingsWithDefaults : LoadableConfig
		 {
			  public static readonly Setting<string> Hello = setting( "hello", STRING, "Hello, World!" );

			  public static readonly Setting<bool> BoolSetting = setting( "bool_setting", BOOLEAN, Settings.TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @DocumentedDefaultValue("<documented default value>") public static final org.neo4j.graphdb.config.Setting<bool> secretSetting = setting("secret_setting", BOOLEAN, Settings.TRUE);
			  [DocumentedDefaultValue("<documented default value>")]
			  public static readonly Setting<bool> SecretSetting = setting( "secret_setting", BOOLEAN, Settings.TRUE );

			  [Obsolete, ReplacedBy("hello")]
			  public static readonly Setting<string> OldHello = setting( "old_hello", STRING, "Hello, Bob" );

			  [Obsolete]
			  public static readonly Setting<string> OldSetting = setting( "some_setting", STRING, "Has no replacement" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Secret public static final org.neo4j.graphdb.config.Setting<String> password = setting("password", STRING, "This text should not appear in logs or toString");
			  public static readonly Setting<string> Password = setting( "password", STRING, "This text should not appear in logs or toString" );
		 }

		 private class HelloHasToBeNeo4jConfigurationValidator : ConfigurationValidator
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public java.util.Map<String,String> validate(@Nonnull Config config, @Nonnull Log log) throws org.neo4j.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public override IDictionary<string, string> Validate( Config config, Log log )
			  {
					if ( !config.Get( MySettingsWithDefaults.Hello ).Equals( "neo4j" ) )
					{
						 throw new InvalidSettingException( "Setting hello has to set to neo4j" );
					}

					return Collections.emptyMap();
			  }
		 }

		 private static MyMigratingSettings _myMigratingSettings = new MyMigratingSettings();
		 private static MySettingsWithDefaults _mySettingsWithDefaults = new MySettingsWithDefaults();

		 private static Config Config()
		 {
			  return Config( Collections.emptyMap() );
		 }

		 private static Config Config( IDictionary<string, string> @params )
		 {
			  return Config.FromSettings( @params ).withConfigClasses( Arrays.asList( _mySettingsWithDefaults, _myMigratingSettings ) ).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expect = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expect = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyDefaults()
		 public virtual void ShouldApplyDefaults()
		 {
			  Config config = Config();

			  assertThat( config.Get( MySettingsWithDefaults.Hello ), @is( "Hello, World!" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyMigrations()
		 public virtual void ShouldApplyMigrations()
		 {
			  // When
			  Config config = Config( stringMap( "old", "hello!" ) );

			  // Then
			  assertThat( config.Get( MyMigratingSettings.Newer ), @is( "hello!" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.config.InvalidSettingException.class) public void shouldNotAllowSettingInvalidValues()
		 public virtual void ShouldNotAllowSettingInvalidValues()
		 {
			  Config( stringMap( MySettingsWithDefaults.BoolSetting.name(), "asd" ) );
			  fail( "Expected validation to fail." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAugmentConfig()
		 public virtual void ShouldBeAbleToAugmentConfig()
		 {
			  // Given
			  Config config = Config();

			  // When
			  config.Augment( MySettingsWithDefaults.BoolSetting, Settings.FALSE );
			  config.Augment( MySettingsWithDefaults.Hello, "Bye" );

			  // Then
			  assertThat( config.Get( MySettingsWithDefaults.BoolSetting ), equalTo( false ) );
			  assertThat( config.Get( MySettingsWithDefaults.Hello ), equalTo( "Bye" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void augmentAnotherConfig()
		 public virtual void AugmentAnotherConfig()
		 {
			  Config config = Config();
			  config.Augment( MySettingsWithDefaults.Hello, "Hi" );

			  Config anotherConfig = Config();
			  anotherConfig.Augment( stringMap( MySettingsWithDefaults.BoolSetting.name(), Settings.FALSE, MySettingsWithDefaults.Hello.name(), "Bye" ) );

			  config.Augment( anotherConfig );

			  assertThat( config.Get( MySettingsWithDefaults.BoolSetting ), equalTo( false ) );
			  assertThat( config.Get( MySettingsWithDefaults.Hello ), equalTo( "Bye" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAndDiscardUnknownOptionsInReservedNamespaceAndPassOnBufferedLogInWithMethods() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWarnAndDiscardUnknownOptionsInReservedNamespaceAndPassOnBufferedLogInWithMethods()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  File confFile = TestDirectory.file( "test.conf" );
			  assertTrue( confFile.createNewFile() );

			  Config config = Config.FromFile( confFile ).withSetting( GraphDatabaseSettings.strict_config_validation, "false" ).withSetting( "ha.jibberish", "baah" ).withSetting( "dbms.jibberish", "booh" ).build();

			  // When
			  config.Logger = log;
			  config.Augment( "causal_clustering.jibberish", "baah" );

			  // Then
			  verify( log ).warn( "Unknown config option: %s", "dbms.jibberish" );
			  verify( log ).warn( "Unknown config option: %s", "ha.jibberish" );
			  verifyNoMoreInteractions( log );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogDeprecationWarnings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogDeprecationWarnings()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  File confFile = TestDirectory.file( "test.conf" );
			  assertTrue( confFile.createNewFile() );

			  Config config = Config.FromFile( confFile ).withSetting( MySettingsWithDefaults.OldHello, "baah" ).withSetting( MySettingsWithDefaults.OldSetting, "booh" ).withConfigClasses( Arrays.asList( _mySettingsWithDefaults, _myMigratingSettings, new GraphDatabaseSettings() ) ).build();

			  // When
			  config.Logger = log;

			  // Then
			  verify( log ).warn( "%s is deprecated. Replaced by %s", MySettingsWithDefaults.OldHello.name(), MySettingsWithDefaults.Hello.name() );
			  verify( log ).warn( "%s is deprecated.", MySettingsWithDefaults.OldSetting.name() );
			  verifyNoMoreInteractions( log );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogIfConfigFileCouldNotBeFound()
		 public virtual void ShouldLogIfConfigFileCouldNotBeFound()
		 {
			  Log log = mock( typeof( Log ) );
			  File confFile = TestDirectory.file( "test.conf" ); // Note: we don't create the file.

			  Config config = Config.FromFile( confFile ).withNoThrowOnFileLoadFailure().build();

			  config.Logger = log;

			  verify( log ).warn( "Config file [%s] does not exist.", confFile );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogIfConfigFileCouldNotBeRead() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogIfConfigFileCouldNotBeRead()
		 {
			  Log log = mock( typeof( Log ) );
			  File confFile = TestDirectory.file( "test.conf" );
			  assertTrue( confFile.createNewFile() );
			  assumeTrue( confFile.setReadable( false ) );

			  Config config = Config.FromFile( confFile ).withNoThrowOnFileLoadFailure().build();

			  config.Logger = log;

			  verify( log ).error( "Unable to load config file [%s]: %s", confFile, confFile + " (Permission denied)" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = ConfigLoadIOException.class) public void mustThrowIfConfigFileCouldNotBeFound()
		 public virtual void MustThrowIfConfigFileCouldNotBeFound()
		 {
			  File confFile = TestDirectory.file( "test.conf" );

			  Config.FromFile( confFile ).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = ConfigLoadIOException.class) public void mustThrowIfConfigFileCoutNotBeRead() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustThrowIfConfigFileCoutNotBeRead()
		 {
			  File confFile = TestDirectory.file( "test.conf" );
			  assertTrue( confFile.createNewFile() );
			  assumeTrue( confFile.setReadable( false ) );
			  Config.FromFile( confFile ).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustWarnIfFileContainsDuplicateSettings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustWarnIfFileContainsDuplicateSettings()
		 {
			  Log log = mock( typeof( Log ) );
			  File confFile = TestDirectory.createFile( "test.conf" );
			  Files.write( confFile.toPath(), Arrays.asList(ExternalSettings.initialHeapSize.name() + "=5g", ExternalSettings.initialHeapSize.name() + "=4g", ExternalSettings.initialHeapSize.name() + "=3g", ExternalSettings.maxHeapSize.name() + "=10g", ExternalSettings.maxHeapSize.name() + "=10g") );

			  Config config = Config.FromFile( confFile ).build();
			  config.Logger = log;

			  // We should only log the warning once for each.
			  verify( log ).warn( "The '%s' setting is specified more than once. Settings only be specified once, to avoid ambiguity. " + "The setting value that will be used is '%s'.", ExternalSettings.initialHeapSize.name(), "5g" );
			  verify( log ).warn( "The '%s' setting is specified more than once. Settings only be specified once, to avoid ambiguity. " + "The setting value that will be used is '%s'.", ExternalSettings.maxHeapSize.name(), "10g" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotWarnAboutDuplicateJvmAdditionalSettings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotWarnAboutDuplicateJvmAdditionalSettings()
		 {
			  Log log = mock( typeof( Log ) );
			  File confFile = TestDirectory.createFile( "test.conf" );
			  Files.write( confFile.toPath(), Arrays.asList(ExternalSettings.additionalJvm.name() + "=-Dsysprop=val", ExternalSettings.additionalJvm.name() + "=-XX:+UseG1GC", ExternalSettings.additionalJvm.name() + "=-XX:+AlwaysPreTouch") );

			  Config config = Config.FromFile( confFile ).build();
			  config.Logger = log;

			  // The ExternalSettings.additionalJvm setting is allowed to be specified more than once.
			  verifyNoMoreInteractions( log );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetInternalParameter()
		 public virtual void ShouldSetInternalParameter()
		 {
			  // Given
			  Config config = Config.Builder().withSetting(MySettingsWithDefaults.SecretSetting, "false").withSetting(MySettingsWithDefaults.Hello, "ABC").withConfigClasses(Arrays.asList(_mySettingsWithDefaults, _myMigratingSettings)).build();

			  // Then
			  assertTrue( config.ConfigValues[MySettingsWithDefaults.SecretSetting.name()].@internal() );
			  assertFalse( config.ConfigValues[MySettingsWithDefaults.Hello.name()].@internal() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetSecretParameter()
		 public virtual void ShouldSetSecretParameter()
		 {
			  // Given
			  Config config = Config.Builder().withSetting(MySettingsWithDefaults.Password, "this should not be visible").withSetting(MySettingsWithDefaults.Hello, "ABC").withConfigClasses(Arrays.asList(_mySettingsWithDefaults, _myMigratingSettings)).build();

			  // Then
			  assertTrue( config.ConfigValues[MySettingsWithDefaults.Password.name()].secret() );
			  assertFalse( config.ConfigValues[MySettingsWithDefaults.Hello.name()].secret() );
			  string configText = config.ToString();
			  assertTrue( configText.Contains( Secret.OBSFUCATED ) );
			  assertFalse( configText.Contains( "this should not be visible" ) );
			  assertFalse( configText.Contains( config.Get( MySettingsWithDefaults.Password ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetDocumentedDefaultValue()
		 public virtual void ShouldSetDocumentedDefaultValue()
		 {
			  // Given
			  Config config = Config.Builder().withSetting(MySettingsWithDefaults.SecretSetting, "false").withSetting(MySettingsWithDefaults.Hello, "ABC").withConfigClasses(Arrays.asList(new MySettingsWithDefaults(), _myMigratingSettings)).build();

			  // Then
			  assertEquals( "<documented default value>", config.ConfigValues[MySettingsWithDefaults.SecretSetting.name()].documentedDefaultValue() );
			  assertEquals( null, config.ConfigValues[MySettingsWithDefaults.Hello.name()].documentedDefaultValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validatorsShouldBeCalledWhenBuilding()
		 public virtual void ValidatorsShouldBeCalledWhenBuilding()
		 {
			  // Should not throw
			  Config.Builder().withSetting(MySettingsWithDefaults.Hello, "neo4j").withValidator(new HelloHasToBeNeo4jConfigurationValidator()).withConfigClasses(Arrays.asList(_mySettingsWithDefaults, _myMigratingSettings)).build();

			  Expect.expect( typeof( InvalidSettingException ) );
			  Expect.expectMessage( "Setting hello has to set to neo4j" );

			  // Should throw
			  Config.Builder().withSetting(MySettingsWithDefaults.Hello, "not-neo4j").withValidator(new HelloHasToBeNeo4jConfigurationValidator()).withConfigClasses(Arrays.asList(_mySettingsWithDefaults, _myMigratingSettings)).build();
		 }

		 [Group("a.b.c")]
		 private class GroupedSetting
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void identifiersFromGroup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IdentifiersFromGroup()
		 {
			  // Given
			  File confFile = TestDirectory.file( "test.conf" );
			  assertTrue( confFile.createNewFile() );

			  Config config = Config.FromFile( confFile ).withSetting( GraphDatabaseSettings.strict_config_validation, "false" ).withSetting( "a.b.c.first.jibberish", "baah" ).withSetting( "a.b.c.second.jibberish", "baah" ).withSetting( "a.b.c.third.jibberish", "baah" ).withSetting( "a.b.c.forth.jibberish", "baah" ).build();

			  ISet<string> identifiers = config.IdentifiersFromGroup( typeof( GroupedSetting ) );
			  ISet<string> expectedIdentifiers = new HashSet<string>( Arrays.asList( "first", "second", "third", "forth" ) );

			  assertEquals( expectedIdentifiers, identifiers );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isConfigured()
		 public virtual void isConfigured()
		 {
			  Config config = Config();
			  assertFalse( config.IsConfigured( MySettingsWithDefaults.Hello ) );
			  config.Augment( MySettingsWithDefaults.Hello, "Hi" );
			  assertTrue( config.IsConfigured( MySettingsWithDefaults.Hello ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isConfiguredShouldNotReturnTrueEvenThoughDefaultValueExists()
		 public virtual void isConfiguredShouldNotReturnTrueEvenThoughDefaultValueExists()
		 {
			  Config config = Config();
			  assertFalse( config.IsConfigured( MySettingsWithDefaults.Hello ) );
			  assertEquals( "Hello, World!", config.Get( MySettingsWithDefaults.Hello ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void withConnectorsDisabled()
		 public virtual void WithConnectorsDisabled()
		 {
			  Connector httpConnector = new HttpConnector();
			  Connector boltConnector = new BoltConnector();
			  Config config = Config.Builder().withSetting(httpConnector.Enabled, "true").withSetting(httpConnector.Type, Connector.ConnectorType.Http.name()).withSetting(boltConnector.Enabled, "true").withSetting(boltConnector.Type, Connector.ConnectorType.Bolt.name()).withConnectorsDisabled().build();
			  assertFalse( config.Get( httpConnector.Enabled ) );
			  assertFalse( config.Get( boltConnector.Enabled ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void augmentDefaults()
		 public virtual void AugmentDefaults()
		 {
			  Config config = Config();
			  assertEquals( "Hello, World!", config.Get( MySettingsWithDefaults.Hello ) );
			  config.AugmentDefaults( MySettingsWithDefaults.Hello, "new default" );
			  assertEquals( "new default", config.Get( MySettingsWithDefaults.Hello ) );
		 }

		 public class MyDynamicSettings : LoadableConfig
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Dynamic public static final org.neo4j.graphdb.config.Setting<bool> boolSetting = setting("bool_setting", BOOLEAN, Settings.TRUE);
			  public static readonly Setting<bool> BoolSetting = setting( "bool_setting", BOOLEAN, Settings.TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Dynamic @Secret public static final org.neo4j.graphdb.config.Setting<String> secretSetting = setting("password", STRING, "secret");
			  public static readonly Setting<string> SecretSetting = setting( "password", STRING, "secret" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateDynamicShouldLogChanges()
		 public virtual void UpdateDynamicShouldLogChanges()
		 {
			  string settingName = MyDynamicSettings.BoolSetting.name();
			  string changedMessage = "Setting changed: '%s' changed from '%s' to '%s' via '%s'";
			  Config config = Config.Builder().withConfigClasses(singletonList(new MyDynamicSettings())).build();

			  Log log = mock( typeof( Log ) );
			  config.Logger = log;

			  config.UpdateDynamicSetting( settingName, "false", ORIGIN );
			  config.UpdateDynamicSetting( settingName, "true", ORIGIN );
			  config.UpdateDynamicSetting( settingName, "", ORIGIN );

			  InOrder order = inOrder( log );
			  order.verify( log ).info( changedMessage, settingName, "default (true)", "false", "test" );
			  order.verify( log ).info( changedMessage, settingName, "false", "true", "test" );
			  order.verify( log ).info( changedMessage, settingName, "true", "default (true)", "test" );
			  verifyNoMoreInteractions( log );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateDynamicShouldThrowIfSettingIsNotDynamic()
		 public virtual void UpdateDynamicShouldThrowIfSettingIsNotDynamic()
		 {
			  Config config = Config.Builder().withConfigClasses(singletonList(_mySettingsWithDefaults)).build();
			  Expect.expect( typeof( System.ArgumentException ) );
			  config.UpdateDynamicSetting( MySettingsWithDefaults.Hello.name(), "hello", ORIGIN );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateDynamicShouldInformRegisteredListeners()
		 public virtual void UpdateDynamicShouldInformRegisteredListeners()
		 {
			  Config config = Config.Builder().withConfigClasses(singletonList(new MyDynamicSettings())).build();
			  AtomicInteger counter = new AtomicInteger( 0 );
			  config.RegisterDynamicUpdateListener(MyDynamicSettings.BoolSetting, (previous, update) =>
			  {
				counter.AndIncrement;
				assertTrue( previous );
				assertFalse( update );
			  });
			  config.UpdateDynamicSetting( MyDynamicSettings.BoolSetting.name(), "false", ORIGIN );
			  assertThat( counter.get(), @is(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateDynamicShouldNotAllowInvalidSettings()
		 public virtual void UpdateDynamicShouldNotAllowInvalidSettings()
		 {
			  Config config = Config.Builder().withConfigClasses(singletonList(new MyDynamicSettings())).build();
			  Expect.expect( typeof( InvalidSettingException ) );
			  config.UpdateDynamicSetting( MyDynamicSettings.BoolSetting.name(), "this is not a boolean", ORIGIN );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void registeringUpdateListenerOnNonDynamicSettingMustThrow()
		 public virtual void RegisteringUpdateListenerOnNonDynamicSettingMustThrow()
		 {
			  Config config = Config.Builder().withConfigClasses(singletonList(_mySettingsWithDefaults)).build();
			  Expect.expect( typeof( System.ArgumentException ) );
			  config.RegisterDynamicUpdateListener( MySettingsWithDefaults.Hello, ( a, b ) => fail( "never called" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateDynamicShouldLogExceptionsFromUpdateListeners()
		 public virtual void UpdateDynamicShouldLogExceptionsFromUpdateListeners()
		 {
			  Config config = Config.Builder().withConfigClasses(singletonList(new MyDynamicSettings())).build();
			  System.InvalidOperationException exception = new System.InvalidOperationException( "Boo" );
			  config.RegisterDynamicUpdateListener(MyDynamicSettings.BoolSetting, (a, b) =>
			  {
				throw exception;
			  });
			  Log log = mock( typeof( Log ) );
			  config.Logger = log;
			  string settingName = MyDynamicSettings.BoolSetting.name();

			  config.UpdateDynamicSetting( settingName, "", ORIGIN );

			  verify( log ).error( "Failure when notifying listeners after dynamic setting change; " + "new setting might not have taken effect: Boo", exception );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateDynamicShouldWorkWithSecret() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpdateDynamicShouldWorkWithSecret()
		 {
			  // Given a secret dynamic setting with a registered update listener
			  string settingName = MyDynamicSettings.SecretSetting.name();
			  string changedMessage = "Setting changed: '%s' changed from '%s' to '%s' via '%s'";
			  Config config = Config.Builder().withConfigClasses(singletonList(new MyDynamicSettings())).build();

			  Log log = mock( typeof( Log ) );
			  config.Logger = log;

			  AtomicInteger counter = new AtomicInteger( 0 );
			  config.RegisterDynamicUpdateListener(MyDynamicSettings.SecretSetting, (previous, update) =>
			  {
				counter.AndIncrement;
				assertThat( "Update listener should not see obsfucated secret", previous, not( CoreMatchers.equalTo( Secret.OBSFUCATED ) ) );
				assertThat( "Update listener should not see obsfucated secret", update, not( CoreMatchers.equalTo( Secret.OBSFUCATED ) ) );
			  });

			  // When changing secret settings three times
			  config.UpdateDynamicSetting( settingName, "another", ORIGIN );
			  config.UpdateDynamicSetting( settingName, "secret2", ORIGIN );
			  config.UpdateDynamicSetting( settingName, "", ORIGIN );

			  // Then we should see obsfucated log messages
			  InOrder order = inOrder( log );
			  order.verify( log ).info( changedMessage, settingName, "default (" + Secret.OBSFUCATED + ")", Secret.OBSFUCATED, ORIGIN );
			  order.verify( log ).info( changedMessage, settingName, Secret.OBSFUCATED, Secret.OBSFUCATED, ORIGIN );
			  order.verify( log ).info( changedMessage, settingName, Secret.OBSFUCATED, "default (" + Secret.OBSFUCATED + ")", ORIGIN );
			  verifyNoMoreInteractions( log );

			  // And see 3 calls to the update listener
			  assertThat( counter.get(), @is(3) );
		 }
	}

}