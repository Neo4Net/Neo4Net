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

	using ContainerRequest = com.sun.jersey.spi.container.ContainerRequest;
	using ContainerRequestFilter = com.sun.jersey.spi.container.ContainerRequestFilter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.web.XForwardUtil.X_FORWARD_HOST_HEADER_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.web.XForwardUtil.X_FORWARD_PROTO_HEADER_KEY;

	/// <summary>
	/// Changes the value of the base and request URIs to match the provided
	/// X-Forwarded-Host and X-Forwarded-Proto header values.
	/// <para>
	/// In doing so, it means Neo4Net server can use those URIs as if they were the
	/// actual request URIs.
	/// </para>
	/// </summary>
	public class XForwardFilter : ContainerRequestFilter
	{
		 public override ContainerRequest Filter( ContainerRequest containerRequest )
		 {
			  string xForwardedHost = containerRequest.getHeaderValue( X_FORWARD_HOST_HEADER_KEY );
			  string xForwardedProto = containerRequest.getHeaderValue( X_FORWARD_PROTO_HEADER_KEY );

			  URI externalBaseUri = XForwardUtil.externalUri( containerRequest.BaseUri, xForwardedHost, xForwardedProto );
			  URI externalRequestUri = XForwardUtil.externalUri( containerRequest.RequestUri, xForwardedHost, xForwardedProto );

			  containerRequest.setUris( externalBaseUri, externalRequestUri );
			  return containerRequest;
		 }
	}

}