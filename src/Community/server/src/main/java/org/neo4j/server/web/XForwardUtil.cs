using System;

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
namespace Neo4Net.Server.web
{

	public class XForwardUtil
	{
		 public const string X_FORWARD_HOST_HEADER_KEY = "X-Forwarded-Host";
		 public const string X_FORWARD_PROTO_HEADER_KEY = "X-Forwarded-Proto";

		 private XForwardUtil()
		 {
		 }

		 public static string ExternalUri( string internalUri, string xForwardedHost, string xForwardedProto )
		 {
			  return externalUri( UriBuilder.fromUri( internalUri ), xForwardedHost, xForwardedProto ).ToString();
		 }

		 public static URI ExternalUri( URI internalUri, string xForwardedHost, string xForwardedProto )
		 {
			  return externalUri( UriBuilder.fromUri( internalUri ), xForwardedHost, xForwardedProto );
		 }

		 private static URI ExternalUri( UriBuilder builder, string xForwardedHost, string xForwardedProto )
		 {
			  ForwardedHost forwardedHost = new ForwardedHost( xForwardedHost );
			  ForwardedProto forwardedProto = new ForwardedProto( xForwardedProto );

			  if ( forwardedHost.IsValid )
			  {
					builder.host( forwardedHost.Host );

					if ( forwardedHost.HasExplicitlySpecifiedPort() )
					{
						 builder.port( forwardedHost.Port );
					}
			  }

			  if ( forwardedProto.Valid )
			  {
					builder.scheme( forwardedProto.Scheme );
			  }

			  return builder.build();
		 }

		 private sealed class ForwardedHost
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string HostConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int PortConflict = -1;
			  internal bool IsValid;

			  internal ForwardedHost( string headerValue )
			  {
					if ( string.ReferenceEquals( headerValue, null ) )
					{
						 this.IsValid = false;
						 return;
					}

					string firstHostInXForwardedHostHeader = headerValue.Split( ",", true )[0].Trim();

					try
					{
						 UriBuilder.fromUri( firstHostInXForwardedHostHeader ).build();
					}
					catch ( System.ArgumentException )
					{
						 this.IsValid = false;
						 return;
					}

					string[] strings = firstHostInXForwardedHostHeader.Split( ":", true );
					if ( strings.Length > 0 )
					{
						 this.HostConflict = strings[0];
						 IsValid = true;
					}
					if ( strings.Length > 1 )
					{
						 this.PortConflict = Convert.ToInt32( strings[1] );
						 IsValid = true;
					}
					if ( strings.Length > 2 )
					{
						 this.IsValid = false;
					}
			  }

			  internal bool HasExplicitlySpecifiedPort()
			  {
					return PortConflict >= 0;
			  }

			  internal string Host
			  {
				  get
				  {
						return HostConflict;
				  }
			  }

			  internal int Port
			  {
				  get
				  {
						return PortConflict;
				  }
			  }
		 }

		 private sealed class ForwardedProto
		 {
			  internal readonly string HeaderValue;

			  internal ForwardedProto( string headerValue )
			  {
					if ( !string.ReferenceEquals( headerValue, null ) )
					{
						 this.HeaderValue = headerValue;
					}
					else
					{
						 this.HeaderValue = "";
					}
			  }

			  internal bool Valid
			  {
				  get
				  {
						return HeaderValue.ToLower().Equals("http") || HeaderValue.ToLower().Equals("https");
				  }
			  }

			  internal string Scheme
			  {
				  get
				  {
						return HeaderValue;
				  }
			  }
		 }
	}

}