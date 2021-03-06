﻿using System;
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
namespace Org.Neo4j.Server.configuration
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using DatabaseManagementSystemSettings = Org.Neo4j.Dbms.DatabaseManagementSystemSettings;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppressAll;

	public class ConfigLoaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public readonly SuppressOutput SuppressOutput = suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TemporaryFolder folder = new org.junit.rules.TemporaryFolder();
		 public readonly TemporaryFolder Folder = new TemporaryFolder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideAConfiguration()
		 public virtual void ShouldProvideAConfiguration()
		 {
			  // given
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).build();

			  // when
			  Config config = Config.fromFile( configFile ).withHome( Folder.Root ).build();

			  // then
			  assertNotNull( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseSpecifiedConfigFile()
		 public virtual void ShouldUseSpecifiedConfigFile()
		 {
			  // given
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).withNameValue( GraphDatabaseSettings.default_advertised_address.name(), "bar" ).build();

			  // when
			  Config testConf = Config.fromFile( configFile ).withHome( Folder.Root ).build();

			  // then
			  const string expectedValue = "bar";
			  assertEquals( expectedValue, testConf.Get( GraphDatabaseSettings.default_advertised_address ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseSpecifiedHomeDir()
		 public virtual void ShouldUseSpecifiedHomeDir()
		 {
			  // given
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).build();

			  // when
			  Config testConf = Config.fromFile( configFile ).withHome( Folder.Root ).build();

			  // then
			  assertEquals( Folder.Root, testConf.Get( GraphDatabaseSettings.neo4j_home ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseWorkingDirForHomeDirIfUnspecified()
		 public virtual void ShouldUseWorkingDirForHomeDirIfUnspecified()
		 {
			  // given
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).build();

			  // when
			  Config testConf = Config.fromFile( configFile ).build();

			  // then
			  assertEquals( new File( System.getProperty( "user.dir" ) ), testConf.Get( GraphDatabaseSettings.neo4j_home ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptDuplicateKeysWithSameValue()
		 public virtual void ShouldAcceptDuplicateKeysWithSameValue()
		 {
			  // given
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).withNameValue( GraphDatabaseSettings.default_advertised_address.name(), "bar" ).withNameValue(GraphDatabaseSettings.default_advertised_address.name(), "bar").build();

			  // when
			  Config testConf = Config.fromFile( configFile ).withHome( Folder.Root ).build();

			  // then
			  assertNotNull( testConf );
			  const string expectedValue = "bar";
			  assertEquals( expectedValue, testConf.Get( GraphDatabaseSettings.default_advertised_address ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void loadOfflineConfigShouldDisableBolt()
		 public virtual void LoadOfflineConfigShouldDisableBolt()
		 {
			  // given
			  BoltConnector defaultBoltConf = new BoltConnector( "bolt" );
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).withNameValue( defaultBoltConf.Enabled.name(), Settings.TRUE ).build();

			  // when
			  Config testConf = Config.fromFile( configFile ).withHome( Folder.Root ).withConnectorsDisabled().build();

			  // then
			  assertNotNull( testConf );
			  assertEquals( false, testConf.Get( defaultBoltConf.Enabled ) );
			  assertEquals( false, testConf.Get( ( new BoltConnector() ).enabled ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void loadOfflineConfigAddDisabledBoltConnector()
		 public virtual void LoadOfflineConfigAddDisabledBoltConnector()
		 {
			  // given
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).build();

			  // when
			  Config testConf = Config.fromFile( configFile ).withHome( Folder.Root ).withConnectorsDisabled().build();

			  // then
			  assertNotNull( testConf );
			  assertEquals( false, testConf.Get( ( new BoltConnector() ).enabled ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindThirdPartyJaxRsPackages() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindThirdPartyJaxRsPackages()
		 {
			  // given
			  File file = ServerTestUtils.createTempConfigFile( Folder.Root );

			  using ( StreamWriter @out = new StreamWriter( new StreamWriter( file, true ) ) )
			  {
					@out.BaseStream.WriteByte( ServerSettings.third_party_packages.name() );
					@out.Write( "=" );
					@out.Write( "com.foo.bar=\"mount/point/foo\"," );
					@out.Write( "com.foo.baz=\"/bar\"," );
					@out.Write( "com.foo.foobarbaz=\"/\"" );
					@out.Write( Environment.NewLine );
			  }

			  // when
			  Config config = Config.fromFile( file ).withHome( Folder.Root ).build();

			  // then
			  IList<ThirdPartyJaxRsPackage> thirdpartyJaxRsPackages = config.Get( ServerSettings.third_party_packages );
			  assertNotNull( thirdpartyJaxRsPackages );
			  assertEquals( 3, thirdpartyJaxRsPackages.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetainRegistrationOrderOfThirdPartyJaxRsPackages()
		 public virtual void ShouldRetainRegistrationOrderOfThirdPartyJaxRsPackages()
		 {
			  // given
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).withNameValue( ServerSettings.third_party_packages.name(), "org.neo4j.extension.extension1=/extension1,org.neo4j.extension.extension2=/extension2," + "org.neo4j.extension.extension3=/extension3" ).build();

			  // when
			  Config config = Config.fromFile( configFile ).withHome( Folder.Root ).build();

			  // then
			  IList<ThirdPartyJaxRsPackage> thirdpartyJaxRsPackages = config.Get( ServerSettings.third_party_packages );

			  assertEquals( 3, thirdpartyJaxRsPackages.Count );
			  assertEquals( "/extension1", thirdpartyJaxRsPackages[0].MountPoint );
			  assertEquals( "/extension2", thirdpartyJaxRsPackages[1].MountPoint );
			  assertEquals( "/extension3", thirdpartyJaxRsPackages[2].MountPoint );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.io.UncheckedIOException.class) public void shouldThrowWhenSpecifiedConfigFileDoesNotExist()
		 public virtual void ShouldThrowWhenSpecifiedConfigFileDoesNotExist()
		 {
			  // Given
			  File nonExistentConfigFile = new File( "/tmp/" + DateTimeHelper.CurrentUnixTimeMillis() );

			  // When
			  Config config = Config.fromFile( nonExistentConfigFile ).withHome( Folder.Root ).build();

			  // Then
			  assertNotNull( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkFineWhenSpecifiedConfigFileDoesNotExist()
		 public virtual void ShouldWorkFineWhenSpecifiedConfigFileDoesNotExist()
		 {
			  // Given
			  File nonExistentConfigFile = new File( "/tmp/" + DateTimeHelper.CurrentUnixTimeMillis() );

			  // When
			  Config config = Config.fromFile( nonExistentConfigFile ).withHome( Folder.Root ).withNoThrowOnFileLoadFailure().build();

			  // Then
			  assertNotNull( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDefaultToCorrectValueForAuthStoreLocation()
		 public virtual void ShouldDefaultToCorrectValueForAuthStoreLocation()
		 {
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).withoutSetting( GraphDatabaseSettings.data_directory ).build();
			  Config config = Config.fromFile( configFile ).withHome( Folder.Root ).build();

			  assertThat( config.Get( DatabaseManagementSystemSettings.auth_store_directory ), @is( ( new File( Folder.Root, "data/dbms" ) ).AbsoluteFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAValueForAuthStoreLocation()
		 public virtual void ShouldSetAValueForAuthStoreLocation()
		 {
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).withSetting( GraphDatabaseSettings.data_directory, "the-data-dir" ).build();
			  Config config = Config.fromFile( configFile ).withHome( Folder.Root ).build();

			  assertThat( config.Get( DatabaseManagementSystemSettings.auth_store_directory ), @is( ( new File( Folder.Root, "the-data-dir/dbms" ) ).AbsoluteFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOverwriteAuthStoreLocationIfProvided()
		 public virtual void ShouldNotOverwriteAuthStoreLocationIfProvided()
		 {
			  File configFile = ConfigFileBuilder.Builder( Folder.Root ).withSetting( GraphDatabaseSettings.data_directory, "the-data-dir" ).withSetting( GraphDatabaseSettings.auth_store, "foo/bar/auth" ).build();
			  Config config = Config.fromFile( configFile ).withHome( Folder.Root ).build();

			  assertThat( config.Get( GraphDatabaseSettings.auth_store ), @is( ( new File( Folder.Root, "foo/bar/auth" ) ).AbsoluteFile ) );
		 }
	}

}