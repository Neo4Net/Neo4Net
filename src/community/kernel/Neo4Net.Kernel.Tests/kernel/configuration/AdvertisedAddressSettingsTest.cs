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

	using Test = org.junit.Test;

	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.listenAddress;

	public class AdvertisedAddressSettingsTest
	{
		 private static Setting<ListenSocketAddress> _listenAddressSetting = listenAddress( "listen_address", 1234 );
		 private static Setting<AdvertisedSocketAddress> _advertisedAddressSetting = advertisedAddress( "advertised_address", _listenAddressSetting );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseExplicitSettingValueWhenProvided()
		 public virtual void ShouldParseExplicitSettingValueWhenProvided()
		 {
			  // given
			  IDictionary<string, string> config = stringMap( GraphDatabaseSettings.default_advertised_address.name(), "server1.example.com", _advertisedAddressSetting.name(), "server1.internal:4000" );

			  // when
			  AdvertisedSocketAddress advertisedSocketAddress = _advertisedAddressSetting.apply( config.get );

			  // then
			  assertEquals( "server1.internal", advertisedSocketAddress.Hostname );
			  assertEquals( 4000, advertisedSocketAddress.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCombineDefaultHostnameWithPortFromListenAddressSettingWhenNoValueProvided()
		 public virtual void ShouldCombineDefaultHostnameWithPortFromListenAddressSettingWhenNoValueProvided()
		 {
			  // given
			  IDictionary<string, string> config = stringMap( GraphDatabaseSettings.default_advertised_address.name(), "server1.example.com" );

			  // when
			  AdvertisedSocketAddress advertisedSocketAddress = _advertisedAddressSetting.apply( config.get );

			  // then
			  assertEquals( "server1.example.com", advertisedSocketAddress.Hostname );
			  assertEquals( 1234, advertisedSocketAddress.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCombineDefaultHostnameWithExplicitPortWhenOnlyAPortProvided()
		 public virtual void ShouldCombineDefaultHostnameWithExplicitPortWhenOnlyAPortProvided()
		 {
			  // given
			  IDictionary<string, string> config = stringMap( GraphDatabaseSettings.default_advertised_address.name(), "server1.example.com", _advertisedAddressSetting.name(), ":4000" );

			  // when
			  AdvertisedSocketAddress advertisedSocketAddress = _advertisedAddressSetting.apply( config.get );

			  // then
			  assertEquals( "server1.example.com", advertisedSocketAddress.Hostname );
			  assertEquals( 4000, advertisedSocketAddress.Port );
		 }
	}

}