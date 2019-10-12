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
namespace Neo4Net.Server.integration
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;

	using ConfigurationBean = Neo4Net.Jmx.impl.ConfigurationBean;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.transaction_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.jmx.JmxUtils.getAttribute;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.jmx.JmxUtils.getObjectName;

	public class ServerConfigIT : ExclusiveServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder tempDir = new org.junit.rules.TemporaryFolder();
		 public TemporaryFolder TempDir = new TemporaryFolder();

		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void durationsAlwaysHaveUnitsInJMX() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DurationsAlwaysHaveUnitsInJMX()
		 {
			  // Given
			  _server = CommunityServerBuilder.serverOnRandomPorts().withProperty(transaction_timeout.name(), "10").build();

			  // When
			  _server.start();

			  // Then
			  ObjectName name = getObjectName( _server.Database.Graph, ConfigurationBean.CONFIGURATION_MBEAN_NAME );
			  string attr = getAttribute( name, transaction_timeout.name() );
			  assertThat( attr, equalTo( "10000ms" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverConfigShouldBeVisibleInJMX() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ServerConfigShouldBeVisibleInJMX()
		 {
			  // Given
			  string configValue = TempDir.newFile().AbsolutePath;
			  _server = CommunityServerBuilder.serverOnRandomPorts().withProperty(ServerSettings.run_directory.name(), configValue).build();

			  // When
			  _server.start();

			  // Then
			  ObjectName name = getObjectName( _server.Database.Graph, ConfigurationBean.CONFIGURATION_MBEAN_NAME );
			  string attr = getAttribute( name, ServerSettings.run_directory.name() );
			  assertThat( attr, equalTo( configValue ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _server.stop();
		 }
	}

}