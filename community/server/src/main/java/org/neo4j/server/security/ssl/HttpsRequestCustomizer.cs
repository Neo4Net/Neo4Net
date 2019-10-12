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
namespace Org.Neo4j.Server.security.ssl
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using HttpField = org.eclipse.jetty.http.HttpField;
	using HttpScheme = org.eclipse.jetty.http.HttpScheme;
	using PreEncodedHttpField = org.eclipse.jetty.http.PreEncodedHttpField;
	using Connector = org.eclipse.jetty.server.Connector;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using Request = org.eclipse.jetty.server.Request;

	using Config = Org.Neo4j.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.jetty.http.HttpHeader.STRICT_TRANSPORT_SECURITY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.configuration.ServerSettings.http_strict_transport_security;

	public class HttpsRequestCustomizer : HttpConfiguration.Customizer
	{
		 private readonly HttpField _hstsResponseField;

		 public HttpsRequestCustomizer( Config config )
		 {
			  _hstsResponseField = CreateHstsResponseField( config );
		 }

		 public override void Customize( Connector connector, HttpConfiguration channelConfig, Request request )
		 {
			  request.Scheme = HttpScheme.HTTPS.asString();

			  AddResponseFieldIfConfigured( request, _hstsResponseField );
		 }

		 private static void AddResponseFieldIfConfigured( Request request, HttpField field )
		 {
			  if ( field != null )
			  {
					request.Response.HttpFields.add( field );
			  }
		 }

		 private static HttpField CreateHstsResponseField( Config config )
		 {
			  string configuredValue = config.Get( http_strict_transport_security );
			  if ( StringUtils.isBlank( configuredValue ) )
			  {
					return null;
			  }
			  return new PreEncodedHttpField( STRICT_TRANSPORT_SECURITY, configuredValue );
		 }
	}

}