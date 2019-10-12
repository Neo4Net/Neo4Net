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

	using Test = org.junit.Test;

	using Neo4Net.Graphdb.config;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.legacyFallback;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.listenAddress;

	public class ListenAddressSettingsTest
	{
		 private static Setting<ListenSocketAddress> _legacyAddressSetting = listenAddress( "address", 1234 );
		 private static Setting<ListenSocketAddress> _listenAddressSetting = legacyFallback( _legacyAddressSetting, listenAddress( "listen_address", 1234 ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseExplicitSettingValueWhenProvided()
		 public virtual void ShouldParseExplicitSettingValueWhenProvided()
		 {
			  // given
			  IDictionary<string, string> config = stringMap( GraphDatabaseSettings.default_listen_address.name(), "server1.example.com", _listenAddressSetting.name(), "server1.internal:4000" );

			  // when
			  ListenSocketAddress listenSocketAddress = _listenAddressSetting.apply( config.get );

			  // then
			  assertEquals( "server1.internal", listenSocketAddress.Hostname );
			  assertEquals( 4000, listenSocketAddress.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCombineDefaultHostnameWithSettingSpecificPortWhenNoValueProvided()
		 public virtual void ShouldCombineDefaultHostnameWithSettingSpecificPortWhenNoValueProvided()
		 {
			  // given
			  IDictionary<string, string> config = stringMap( GraphDatabaseSettings.default_listen_address.name(), "server1.example.com" );

			  // when
			  ListenSocketAddress listenSocketAddress = _listenAddressSetting.apply( config.get );

			  // then
			  assertEquals( "server1.example.com", listenSocketAddress.Hostname );
			  assertEquals( 1234, listenSocketAddress.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCombineDefaultHostnameWithExplicitPortWhenOnlyAPortProvided()
		 public virtual void ShouldCombineDefaultHostnameWithExplicitPortWhenOnlyAPortProvided()
		 {
			  // given
			  IDictionary<string, string> config = stringMap( GraphDatabaseSettings.default_listen_address.name(), "server1.example.com", _listenAddressSetting.name(), ":4000" );

			  // when
			  ListenSocketAddress listenSocketAddress = _listenAddressSetting.apply( config.get );

			  // then
			  assertEquals( "server1.example.com", listenSocketAddress.Hostname );
			  assertEquals( 4000, listenSocketAddress.Port );
		 }
	}

}