using System;
using System.Collections;
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
namespace Neo4Net.Server.configuration
{
	using Test = org.junit.Test;


	using ConfigValue = Neo4Net.Configuration.ConfigValue;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ServerSettingsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void webServerThreadCountDefaultShouldBeDocumented()
		 public virtual void WebServerThreadCountDefaultShouldBeDocumented()
		 {
			  Config config = Config.builder().withServerDefaults().build();

			  string documentedDefaultValue = config.ConfigValues.SetOfKeyValuePairs().Where(c => c.Key.Equals(ServerSettings.WebserverMaxThreads.name())).Select(DictionaryEntry.getValue).First().orElseThrow(() => new Exception("Setting not present!")).documentedDefaultValue().orElseThrow(() => new Exception("Default value not present!"));

			  assertEquals( "Number of available processors, or 500 for machines which have more than 500 processors.", documentedDefaultValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void configValuesContainsConnectors()
		 public virtual void ConfigValuesContainsConnectors()
		 {
			  Config config = Config.builder().withServerDefaults().build();

			  IList<string> connectorSettings = config.ConfigValues.SetOfKeyValuePairs().Select(DictionaryEntry.getKey).Where(c => c.StartsWith("dbms.connector")).Where(c => c.EndsWith(".enabled")).ToList();

			  assertThat( connectorSettings, containsInAnyOrder( "dbms.connector.http.enabled", "dbms.connector.https.enabled", "dbms.connector.bolt.enabled" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void connectorSettingHasItsOwnValues()
		 public virtual void ConnectorSettingHasItsOwnValues()
		 {
			  Config config = Config.builder().withServerDefaults().withSetting((new HttpConnector("http")).address, "localhost:123").build();

			  ConfigValue address = config.ConfigValues.SetOfKeyValuePairs().Where(c => c.Key.Equals("dbms.connector.http.address")).Select(DictionaryEntry.getValue).First().orElseThrow(() => new Exception("Setting not present!"));

			  assertTrue( address.Deprecated() );
			  assertEquals( "dbms.connector.http.listen_address", address.Replacement() );
		 }
	}

}