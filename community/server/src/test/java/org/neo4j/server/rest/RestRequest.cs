using System;
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
namespace Org.Neo4j.Server.rest
{
	using Client = com.sun.jersey.api.client.Client;
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;
	using WebResource = com.sun.jersey.api.client.WebResource;
	using Builder = com.sun.jersey.api.client.WebResource.Builder;
	using HTTPBasicAuthFilter = com.sun.jersey.api.client.filter.HTTPBasicAuthFilter;


	using HTTP = Org.Neo4j.Test.server.HTTP;

	public class RestRequest
	{

		 private readonly URI _baseUri;
		 private static readonly Client _defaultClient = Client.create();
		 private readonly Client _client;
		 private MediaType _accept = MediaType.APPLICATION_JSON_TYPE;
		 private IDictionary<string, string> _headers = new Dictionary<string, string>();

		 public RestRequest( URI baseUri ) : this( baseUri, null, null )
		 {
		 }

		 public RestRequest( URI baseUri, string username, string password )
		 {
			  this._baseUri = UriWithoutSlash( baseUri );
			  if ( !string.ReferenceEquals( username, null ) )
			  {
					_client = Client.create();
					_client.addFilter( new HTTPBasicAuthFilter( username, password ) );
			  }
			  else
			  {
					_client = _defaultClient;
			  }
		 }

		 public RestRequest( URI uri, Client client )
		 {
			  this._baseUri = UriWithoutSlash( uri );
			  this._client = client;
		 }

		 public RestRequest() : this(null)
		 {
		 }

		 private URI UriWithoutSlash( URI uri )
		 {
			  if ( uri == null )
			  {
					return null;
			  }
			  string uriString = uri.ToString();
			  return uriString.EndsWith( "/", StringComparison.Ordinal ) ? uri( uriString.Substring( 0, uriString.Length - 1 ) ) : uri;
		 }

		 private WebResource.Builder Builder( string path )
		 {
			  return Builder( path, _accept );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private com.sun.jersey.api.client.WebResource.Builder builder(String path, final javax.ws.rs.core.MediaType accept)
		 private WebResource.Builder Builder( string path, MediaType accept )
		 {
			  WebResource resource = _client.resource( Uri( PathOrAbsolute( path ) ) );
			  WebResource.Builder builder = resource.accept( accept );
			  if ( _headers.Count > 0 )
			  {
					foreach ( KeyValuePair<string, string> header in _headers.SetOfKeyValuePairs() )
					{
						 builder = builder.header( header.Key, header.Value );
					}
			  }

			  return builder;
		 }

		 private string PathOrAbsolute( string path )
		 {
			  if ( path.StartsWith( "http://", StringComparison.Ordinal ) )
			  {
					return path;
			  }
			  return _baseUri + "/" + path;
		 }

		 public virtual JaxRsResponse Get( string path )
		 {
			  return JaxRsResponse.ExtractFrom( HTTP.sanityCheck( Builder( path ).get( typeof( ClientResponse ) ) ) );
		 }

		 public virtual JaxRsResponse Delete( string path )
		 {
			  return JaxRsResponse.ExtractFrom( HTTP.sanityCheck( Builder( path ).delete( typeof( ClientResponse ) ) ) );
		 }

		 public virtual JaxRsResponse Post( string path, string data )
		 {
			  return Post( path, data, MediaType.APPLICATION_JSON_TYPE );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public JaxRsResponse post(String path, String data, final javax.ws.rs.core.MediaType mediaType)
		 public virtual JaxRsResponse Post( string path, string data, MediaType mediaType )
		 {
			  WebResource.Builder builder = builder( path );
			  if ( !string.ReferenceEquals( data, null ) )
			  {
					builder = builder.entity( data, mediaType );
			  }
			  else
			  {
					builder = builder.type( mediaType );
			  }
			  return JaxRsResponse.ExtractFrom( HTTP.sanityCheck( builder.post( typeof( ClientResponse ) ) ) );
		 }

		 public virtual JaxRsResponse Put( string path, string data )
		 {
			  WebResource.Builder builder = builder( path );
			  if ( !string.ReferenceEquals( data, null ) )
			  {
					builder = builder.entity( data, MediaType.APPLICATION_JSON_TYPE );
			  }
			  return new JaxRsResponse( HTTP.sanityCheck( builder.put( typeof( ClientResponse ) ) ) );
		 }

		 private URI Uri( string uri )
		 {
			  try
			  {
					return new URI( uri );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public virtual JaxRsResponse Get()
		 {
			  return Get( "" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public JaxRsResponse get(String path, final javax.ws.rs.core.MediaType acceptType)
		 public virtual JaxRsResponse Get( string path, MediaType acceptType )
		 {
			  WebResource.Builder builder = builder( path, acceptType );
			  return JaxRsResponse.ExtractFrom( HTTP.sanityCheck( builder.get( typeof( ClientResponse ) ) ) );
		 }

		 public static RestRequest Req()
		 {
			  return new RestRequest();
		 }

		 public virtual JaxRsResponse Delete( URI location )
		 {
			  return Delete( location.ToString() );
		 }

		 public virtual JaxRsResponse Put( URI uri, string data )
		 {
			  return Put( uri.ToString(), data );
		 }

		 public virtual RestRequest Accept( MediaType accept )
		 {
			  this._accept = accept;
			  return this;
		 }

		 public virtual RestRequest Header( string header, string value )
		 {
			  this._headers[header] = value;
			  return this;
		 }

		 public virtual RestRequest Host( string hostname )
		 {
			  // 'host' is one of a handful of so-called restricted headers (wrongly!).
			  // Need to rectify that with a property change.
			  Header( "Host", hostname );
			  return this;
		 }

		 public virtual IDictionary<string, string> Headers
		 {
			 get
			 {
				  return _headers;
			 }
		 }
	}

}