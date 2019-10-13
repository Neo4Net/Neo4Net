using System.Collections.Generic;
using System.Reflection;

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
namespace Neo4Net.Graphdb.factory
{
	using Test = org.junit.jupiter.api.Test;


	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Neo4Net.Graphdb.config;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.keep_logical_logs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.HttpConnector.Encryption.TLS;

	internal class GraphDatabaseSettingsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustHaveNullDefaultPageCacheMemorySizeInBytes()
		 internal virtual void MustHaveNullDefaultPageCacheMemorySizeInBytes()
		 {
			  string bytes = Config.defaults().get(GraphDatabaseSettings.PagecacheMemory);
			  assertThat( bytes, @is( nullValue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCacheSettingMustAcceptArbitraryUserSpecifiedValue()
		 internal virtual void PageCacheSettingMustAcceptArbitraryUserSpecifiedValue()
		 {
			  Setting<string> setting = GraphDatabaseSettings.PagecacheMemory;
			  assertThat( Config.defaults( setting, "245760" ).get( setting ), @is( "245760" ) );
			  assertThat( Config.defaults( setting, "2244g" ).get( setting ), @is( "2244g" ) );
			  assertThat( Config.defaults( setting, "string" ).get( setting ), @is( "string" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noDuplicateSettingsAreAllowed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoDuplicateSettingsAreAllowed()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.HashMap<String,String> fields = new java.util.HashMap<>();
			  Dictionary<string, string> fields = new Dictionary<string, string>();
			  foreach ( System.Reflection.FieldInfo field in typeof( GraphDatabaseSettings ).GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ) )
			  {
					if ( field.Type == typeof( Setting ) )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.graphdb.config.Setting<?> setting = (org.neo4j.graphdb.config.Setting<?>) field.get(null);
						 Setting<object> setting = ( Setting<object> ) field.get( null );

						 assertFalse( fields.ContainsKey( setting.Name() ), format("'%s' in %s has already been defined in %s", setting.Name(), field.Name, fields[setting.Name()]) );
						 fields[setting.Name()] = field.Name;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void groupToScopeSetting()
		 internal virtual void GroupToScopeSetting()
		 {
			  // given
			  string hostname = "my_other_host";
			  int port = 9999;
			  string scoping = "bla";
			  IDictionary<string, string> config = stringMap( GraphDatabaseSettings.DefaultAdvertisedAddress.name(), hostname, (new BoltConnector(scoping)).advertised_address.name(), ":" + port );

			  // when
			  BoltConnector boltConnector = new BoltConnector( scoping );
			  Setting<AdvertisedSocketAddress> advertisedAddress = boltConnector.AdvertisedAddress;
			  AdvertisedSocketAddress advertisedSocketAddress = advertisedAddress.apply( config.get );

			  // then
			  assertEquals( hostname, advertisedSocketAddress.Hostname );
			  assertEquals( port, advertisedSocketAddress.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEnableBoltByDefault()
		 internal virtual void ShouldEnableBoltByDefault()
		 {
			  // given
			  Config config = Config.builder().withServerDefaults().build();

			  // when
			  BoltConnector boltConnector = config.BoltConnectors()[0];
			  ListenSocketAddress listenSocketAddress = config.Get( boltConnector.ListenAddress );

			  // then
			  assertEquals( new ListenSocketAddress( "127.0.0.1", 7687 ), listenSocketAddress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToDisableBoltConnectorWithJustOneParameter()
		 internal virtual void ShouldBeAbleToDisableBoltConnectorWithJustOneParameter()
		 {
			  // given
			  Config config = Config.defaults( ( new BoltConnector( "bolt" ) ).enabled, "false" );

			  // then
			  assertThat( config.BoltConnectors().Count, @is(1) );
			  assertThat( config.EnabledBoltConnectors(), empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToOverrideBoltListenAddressesWithJustOneParameter()
		 internal virtual void ShouldBeAbleToOverrideBoltListenAddressesWithJustOneParameter()
		 {
			  // given
			  Config config = Config.defaults( stringMap( "dbms.connector.bolt.enabled", "true", "dbms.connector.bolt.listen_address", ":8000" ) );

			  BoltConnector boltConnector = config.BoltConnectors()[0];

			  // then
			  assertEquals( new ListenSocketAddress( "127.0.0.1", 8000 ), config.Get( boltConnector.ListenAddress ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeriveBoltListenAddressFromDefaultListenAddress()
		 internal virtual void ShouldDeriveBoltListenAddressFromDefaultListenAddress()
		 {
			  // given
			  Config config = Config.defaults( stringMap( "dbms.connector.bolt.enabled", "true", "dbms.connectors.default_listen_address", "0.0.0.0" ) );

			  BoltConnector boltConnector = config.BoltConnectors()[0];

			  // then
			  assertEquals( new ListenSocketAddress( "0.0.0.0", 7687 ), config.Get( boltConnector.ListenAddress ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeriveBoltListenAddressFromDefaultListenAddressAndSpecifiedPort()
		 internal virtual void ShouldDeriveBoltListenAddressFromDefaultListenAddressAndSpecifiedPort()
		 {
			  // given
			  Config config = Config.defaults( stringMap( "dbms.connectors.default_listen_address", "0.0.0.0", "dbms.connector.bolt.enabled", "true", "dbms.connector.bolt.listen_address", ":8000" ) );

			  BoltConnector boltConnector = config.BoltConnectors()[0];

			  // then
			  assertEquals( new ListenSocketAddress( "0.0.0.0", 8000 ), config.Get( boltConnector.ListenAddress ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStillSupportCustomNameForBoltConnector()
		 internal virtual void ShouldStillSupportCustomNameForBoltConnector()
		 {
			  Config config = Config.defaults( stringMap( "dbms.connector.random_name_that_will_be_unsupported.type", "BOLT", "dbms.connector.random_name_that_will_be_unsupported.enabled", "true", "dbms.connector.random_name_that_will_be_unsupported.listen_address", ":8000" ) );

			  // when
			  BoltConnector boltConnector = config.BoltConnectors()[0];

			  // then
			  assertEquals( new ListenSocketAddress( "127.0.0.1", 8000 ), config.Get( boltConnector.ListenAddress ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportMultipleBoltConnectorsWithCustomNames()
		 internal virtual void ShouldSupportMultipleBoltConnectorsWithCustomNames()
		 {
			  Config config = Config.defaults( stringMap( "dbms.connector.bolt1.type", "BOLT", "dbms.connector.bolt1.enabled", "true", "dbms.connector.bolt1.listen_address", ":8000", "dbms.connector.bolt2.type", "BOLT", "dbms.connector.bolt2.enabled", "true", "dbms.connector.bolt2.listen_address", ":9000" ) );

			  // when
			  IList<ListenSocketAddress> addresses = config.BoltConnectors().Select(c => config.Get(c.listen_address)).ToList();

			  // then
			  assertEquals( 2, addresses.Count );

			  if ( addresses[0].Port == 8000 )
			  {
					assertEquals( new ListenSocketAddress( "127.0.0.1", 8000 ), addresses[0] );
					assertEquals( new ListenSocketAddress( "127.0.0.1", 9000 ), addresses[1] );
			  }
			  else
			  {
					assertEquals( new ListenSocketAddress( "127.0.0.1", 8000 ), addresses[1] );
					assertEquals( new ListenSocketAddress( "127.0.0.1", 9000 ), addresses[0] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportMultipleBoltConnectorsWithDefaultAndCustomName()
		 internal virtual void ShouldSupportMultipleBoltConnectorsWithDefaultAndCustomName()
		 {
			  Config config = Config.defaults( stringMap( "dbms.connector.bolt.type", "BOLT", "dbms.connector.bolt.enabled", "true", "dbms.connector.bolt.listen_address", ":8000", "dbms.connector.bolt2.type", "BOLT", "dbms.connector.bolt2.enabled", "true", "dbms.connector.bolt2.listen_address", ":9000" ) );

			  // when
			  BoltConnector boltConnector1 = config.BoltConnectors()[0];
			  BoltConnector boltConnector2 = config.BoltConnectors()[1];

			  // then
			  assertEquals( new ListenSocketAddress( "127.0.0.1", 8000 ), config.Get( boltConnector1.ListenAddress ) );
			  assertEquals( new ListenSocketAddress( "127.0.0.1", 9000 ), config.Get( boltConnector2.ListenAddress ) );
		 }

		 /// JONAS HTTP FOLLOWS
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testServerDefaultSettings()
		 internal virtual void TestServerDefaultSettings()
		 {
			  // given
			  Config config = Config.builder().withServerDefaults().build();

			  // when
			  IList<HttpConnector> connectors = config.HttpConnectors();

			  // then
			  assertEquals( 2, connectors.Count );
			  if ( connectors[0].EncryptionLevel().Equals(TLS) )
			  {
					assertEquals( new ListenSocketAddress( "localhost", 7474 ), config.Get( connectors[1].listen_address ) );
					assertEquals( new ListenSocketAddress( "localhost", 7473 ), config.Get( connectors[0].listen_address ) );
			  }
			  else
			  {
					assertEquals( new ListenSocketAddress( "127.0.0.1", 7474 ), config.Get( connectors[0].listen_address ) );
					assertEquals( new ListenSocketAddress( "127.0.0.1", 7473 ), config.Get( connectors[1].listen_address ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToDisableHttpConnectorWithJustOneParameter()
		 internal virtual void ShouldBeAbleToDisableHttpConnectorWithJustOneParameter()
		 {
			  // given
			  Config disableHttpConfig = Config.defaults( stringMap( "dbms.connector.http.enabled", "false", "dbms.connector.https.enabled", "false" ) );

			  // then
			  assertTrue( disableHttpConfig.EnabledHttpConnectors().Count == 0 );
			  assertEquals( 2, disableHttpConfig.HttpConnectors().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToOverrideHttpListenAddressWithJustOneParameter()
		 internal virtual void ShouldBeAbleToOverrideHttpListenAddressWithJustOneParameter()
		 {
			  // given
			  Config config = Config.defaults( stringMap( "dbms.connector.http.enabled", "true", "dbms.connector.http.listen_address", ":8000" ) );

			  // then
			  assertEquals( 1, config.EnabledHttpConnectors().Count );

			  HttpConnector httpConnector = config.EnabledHttpConnectors()[0];

			  assertEquals( new ListenSocketAddress( "127.0.0.1", 8000 ), config.Get( httpConnector.ListenAddress ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void hasDefaultBookmarkAwaitTimeout()
		 internal virtual void HasDefaultBookmarkAwaitTimeout()
		 {
			  Config config = Config.defaults();
			  long bookmarkReadyTimeoutMs = config.Get( GraphDatabaseSettings.BookmarkReadyTimeout ).toMillis();
			  assertEquals( TimeUnit.SECONDS.toMillis( 30 ), bookmarkReadyTimeoutMs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToOverrideHttpsListenAddressWithJustOneParameter()
		 internal virtual void ShouldBeAbleToOverrideHttpsListenAddressWithJustOneParameter()
		 {
			  // given
			  Config config = Config.defaults( stringMap( "dbms.connector.https.enabled", "true", "dbms.connector.https.listen_address", ":8000" ) );

			  // then
			  assertEquals( 1, config.EnabledHttpConnectors().Count );
			  HttpConnector httpConnector = config.EnabledHttpConnectors()[0];

			  assertEquals( new ListenSocketAddress( "127.0.0.1", 8000 ), config.Get( httpConnector.ListenAddress ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwsForIllegalBookmarkAwaitTimeout()
		 internal virtual void ThrowsForIllegalBookmarkAwaitTimeout()
		 {
			  string[] illegalValues = new string[] { "0ms", "0s", "10ms", "99ms", "999ms", "42ms" };

			  foreach ( string value in illegalValues )
			  {
					assertThrows(typeof(InvalidSettingException), () =>
					{
					 Config config = Config.defaults( stringMap( GraphDatabaseSettings.BookmarkReadyTimeout.name(), value ) );
					 config.get( GraphDatabaseSettings.BookmarkReadyTimeout );
					}, "Exception expected for value '" + value + "'");
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeriveListenAddressFromDefaultListenAddress()
		 internal virtual void ShouldDeriveListenAddressFromDefaultListenAddress()
		 {
			  // given
			  Config config = Config.fromSettings( stringMap( "dbms.connector.https.enabled", "true", "dbms.connector.http.enabled", "true", "dbms.connectors.default_listen_address", "0.0.0.0" ) ).withServerDefaults().build();

			  // then
			  assertEquals( 2, config.EnabledHttpConnectors().Count );
			  config.EnabledHttpConnectors().ForEach(c => assertEquals("0.0.0.0", config.Get(c.listen_address).Hostname));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeriveListenAddressFromDefaultListenAddressAndSpecifiedPorts()
		 internal virtual void ShouldDeriveListenAddressFromDefaultListenAddressAndSpecifiedPorts()
		 {
			  // given
			  Config config = Config.defaults( stringMap( "dbms.connector.https.enabled", "true", "dbms.connector.http.enabled", "true", "dbms.connectors.default_listen_address", "0.0.0.0", "dbms.connector.http.listen_address", ":8000", "dbms.connector.https.listen_address", ":9000" ) );

			  // then
			  assertEquals( 2, config.EnabledHttpConnectors().Count );

			  config.EnabledHttpConnectors().ForEach(c =>
			  {
						  if ( c.key().Equals("https") )
						  {
								assertEquals( new ListenSocketAddress( "0.0.0.0", 9000 ), config.Get( c.listen_address ) );
						  }
						  else
						  {
								assertEquals( new ListenSocketAddress( "0.0.0.0", 8000 ), config.Get( c.listen_address ) );
						  }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStillSupportCustomNameForHttpConnector()
		 internal virtual void ShouldStillSupportCustomNameForHttpConnector()
		 {
			  Config config = Config.defaults( stringMap( "dbms.connector.random_name_that_will_be_unsupported.type", "HTTP", "dbms.connector.random_name_that_will_be_unsupported.encryption", "NONE", "dbms.connector.random_name_that_will_be_unsupported.enabled", "true", "dbms.connector.random_name_that_will_be_unsupported.listen_address", ":8000" ) );

			  // then
			  assertEquals( 1, config.EnabledHttpConnectors().Count );
			  assertEquals( new ListenSocketAddress( "127.0.0.1", 8000 ), config.Get( config.EnabledHttpConnectors()[0].listen_address ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStillSupportCustomNameForHttpsConnector()
		 internal virtual void ShouldStillSupportCustomNameForHttpsConnector()
		 {
			  Config config = Config.defaults( stringMap( "dbms.connector.random_name_that_will_be_unsupported.type", "HTTP", "dbms.connector.random_name_that_will_be_unsupported.encryption", "TLS", "dbms.connector.random_name_that_will_be_unsupported.enabled", "true", "dbms.connector.random_name_that_will_be_unsupported.listen_address", ":9000" ) );

			  // then
			  assertEquals( 1, config.EnabledHttpConnectors().Count );
			  assertEquals( new ListenSocketAddress( "127.0.0.1", 9000 ), config.Get( config.EnabledHttpConnectors()[0].listen_address ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void validateRetentionPolicy()
		 internal virtual void ValidateRetentionPolicy()
		 {
			  string[] validSet = new string[]{ "true", "keep_all", "false", "keep_none", "10 files", "10k files", "10K size", "10m txs", "10M entries", "10g hours", "10G days" };

			  string[] invalidSet = new string[]{ "invalid", "all", "10", "10k", "10k a" };

			  foreach ( string valid in validSet )
			  {
					assertEquals( valid, Config.defaults( keep_logical_logs, valid ).get( keep_logical_logs ) );
			  }

			  foreach ( string invalid in invalidSet )
			  {
					assertThrows( typeof( InvalidSettingException ), () => Config.defaults(keep_logical_logs, invalid), "Value \"" + invalid + "\" should be considered invalid" );

			  }
		 }
	}

}