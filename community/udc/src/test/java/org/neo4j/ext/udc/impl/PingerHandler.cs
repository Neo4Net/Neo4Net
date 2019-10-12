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
namespace Org.Neo4j.Ext.Udc.impl
{
	using HttpRequest = org.apache.http.HttpRequest;
	using HttpResponse = org.apache.http.HttpResponse;
	using HttpContext = org.apache.http.protocol.HttpContext;
	using HttpRequestHandler = org.apache.http.protocol.HttpRequestHandler;


	public class PingerHandler : HttpRequestHandler
	{
		 private readonly IDictionary<string, string> _queryMap = new Dictionary<string, string>();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(org.apache.http.HttpRequest httpRequest, org.apache.http.HttpResponse httpResponse, org.apache.http.protocol.HttpContext httpContext) throws java.io.IOException
		 public override void Handle( HttpRequest httpRequest, HttpResponse httpResponse, HttpContext httpContext )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String requestUri = httpRequest.getRequestLine().getUri();
			  string requestUri = httpRequest.RequestLine.Uri;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int offset = requestUri.indexOf('?');
			  int offset = requestUri.IndexOf( '?' );
			  if ( offset > -1 )
			  {
					string query = requestUri.Substring( offset + 1 );
					string[] @params = query.Split( "\\+", true );
					if ( @params.Length > 0 )
					{
						 foreach ( string param in @params )
						 {
							  string[] pair = param.Split( "=", true );
							  string key = URLDecoder.decode( pair[0], StandardCharsets.UTF_8.name() );
							  string value = URLDecoder.decode( pair[1], StandardCharsets.UTF_8.name() );
							  _queryMap[key] = value;
						 }
					}
			  }
		 }

		 public virtual IDictionary<string, string> QueryMap
		 {
			 get
			 {
				  return _queryMap;
			 }
		 }
	}

}