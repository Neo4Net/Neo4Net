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
namespace Neo4Net.Server.security.ssl
{
	using Connector = org.eclipse.jetty.server.Connector;
	using HttpChannel = org.eclipse.jetty.server.HttpChannel;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using HttpInput = org.eclipse.jetty.server.HttpInput;
	using HttpOutput = org.eclipse.jetty.server.HttpOutput;
	using Request = org.eclipse.jetty.server.Request;
	using Response = org.eclipse.jetty.server.Response;
	using Test = org.junit.Test;

	using Config = Neo4Net.Kernel.configuration.Config;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.jetty.http.HttpHeader.STRICT_TRANSPORT_SECURITY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.jetty.http.HttpScheme.HTTPS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.jetty.server.HttpConfiguration.Customizer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class HttpsRequestCustomizerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetRequestSchemeToHttps()
		 public virtual void ShouldSetRequestSchemeToHttps()
		 {
			  Customizer customizer = NewCustomizer();
			  Request request = mock( typeof( Request ) );

			  Customize( customizer, request );

			  verify( request ).Scheme = HTTPS.asString();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddHstsHeaderWhenConfigured()
		 public virtual void ShouldAddHstsHeaderWhenConfigured()
		 {
			  string configuredValue = "max-age=3600; includeSubDomains";
			  Customizer customizer = NewCustomizer( configuredValue );
			  Request request = NewRequest();

			  Customize( customizer, request );

			  string receivedValue = request.Response.HttpFields.get( STRICT_TRANSPORT_SECURITY );
			  assertEquals( configuredValue, receivedValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAddHstsHeaderWhenNotConfigured()
		 public virtual void ShouldNotAddHstsHeaderWhenNotConfigured()
		 {
			  Customizer customizer = NewCustomizer();
			  Request request = NewRequest();

			  Customize( customizer, request );

			  string hstsValue = request.Response.HttpFields.get( STRICT_TRANSPORT_SECURITY );
			  assertNull( hstsValue );
		 }

		 private static void Customize( Customizer customizer, Request request )
		 {
			  customizer.customize( mock( typeof( Connector ) ), new HttpConfiguration(), request );
		 }

		 private static Request NewRequest()
		 {
			  HttpChannel channel = mock( typeof( HttpChannel ) );
			  Response response = new Response( channel, mock( typeof( HttpOutput ) ) );
			  Request request = new Request( channel, mock( typeof( HttpInput ) ) );
			  when( channel.Request ).thenReturn( request );
			  when( channel.Response ).thenReturn( response );
			  return request;
		 }

		 private static Customizer NewCustomizer()
		 {
			  return NewCustomizer( null );
		 }

		 private static Customizer NewCustomizer( string hstsValue )
		 {
			  Config config = Config.defaults( ServerSettings.http_strict_transport_security, hstsValue );
			  return new HttpsRequestCustomizer( config );
		 }
	}

}