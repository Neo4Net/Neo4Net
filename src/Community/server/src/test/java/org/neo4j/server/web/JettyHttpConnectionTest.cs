using System.Threading;

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
namespace Neo4Net.Server.web
{
	using HttpCompliance = org.eclipse.jetty.http.HttpCompliance;
	using EndPoint = org.eclipse.jetty.io.EndPoint;
	using Connector = org.eclipse.jetty.server.Connector;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using Server = org.eclipse.jetty.server.Server;
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class JettyHttpConnectionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveId()
		 internal virtual void ShouldHaveId()
		 {
			  Connector connector = ConnectorMock( "https" );
			  JettyHttpConnection connection = NewConnection( connector );

			  assertEquals( "http-1", connection.Id() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveConnectTime()
		 internal virtual void ShouldHaveConnectTime()
		 {
			  JettyHttpConnection connection = NewConnection( ConnectorMock( "http" ) );

			  assertThat( connection.ConnectTime(), greaterThan(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveConnector()
		 internal virtual void ShouldHaveConnector()
		 {
			  JettyHttpConnection connection = NewConnection( ConnectorMock( "http+routing" ) );

			  assertEquals( "http+routing", connection.Connector() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveUsernameAndUserAgent()
		 internal virtual void ShouldHaveUsernameAndUserAgent()
		 {
			  JettyHttpConnection connection = NewConnection( ConnectorMock( "http+routing" ) );

			  assertNull( connection.Username() );
			  connection.UpdateUser( "hello", "my-http-driver/1.2.3" );
			  assertEquals( "hello", connection.Username() );
			  assertEquals( "my-http-driver/1.2.3", connection.UserAgent() );
		 }

		 private static JettyHttpConnection NewConnection( Connector connector )
		 {
			  return new JettyHttpConnection( "http-1", new HttpConfiguration(), connector, mock(typeof(EndPoint)), HttpCompliance.LEGACY, false );
		 }

		 private static Connector ConnectorMock( string name )
		 {
			  Connector connector = mock( typeof( Connector ) );
			  when( connector.Name ).thenReturn( name );
			  when( connector.Executor ).thenReturn( ThreadStart.run );
			  when( connector.Server ).thenReturn( new Server() );
			  return connector;
		 }
	}

}