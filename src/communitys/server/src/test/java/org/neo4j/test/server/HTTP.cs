using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Test.server
{
	using Client = com.sun.jersey.api.client.Client;
	using ClientRequest = com.sun.jersey.api.client.ClientRequest;
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;
	using ClientConfig = com.sun.jersey.api.client.config.ClientConfig;
	using DefaultClientConfig = com.sun.jersey.api.client.config.DefaultClientConfig;
	using HTTPSProperties = com.sun.jersey.client.urlconnection.HTTPSProperties;
	using JsonNode = org.codehaus.jackson.JsonNode;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.sun.jersey.api.client.config.ClientConfig.PROPERTY_FOLLOW_REDIRECTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.sun.jersey.client.urlconnection.HTTPSProperties.PROPERTY_HTTPS_PROPERTIES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.createJsonFrom;

	/// <summary>
	/// A tool for performing REST HTTP requests
	/// </summary>
	public class HTTP
	{

		 private static readonly Builder _builder = new Builder().withHeaders("Accept", "application/json");
		 private static readonly Client _client = CreateClient();

		 private HTTP()
		 {
		 }

		 public static string BasicAuthHeader( string username, string password )
		 {
			  string usernamePassword = username + ':' + password;
			  return "Basic " + Base64.Encoder.encodeToString( usernamePassword.GetBytes() );
		 }

		 public static Builder WithBasicAuth( string username, string password )
		 {
			  return WithHeaders( AUTHORIZATION, BasicAuthHeader( username, password ) );
		 }

		 public static Builder WithHeaders( params string[] kvPairs )
		 {
			  return _builder.withHeaders( kvPairs );
		 }

		 public static Builder WithBaseUri( URI baseUri )
		 {
			  return _builder.withBaseUri( baseUri.ToString() );
		 }

		 public static Response Post( string uri )
		 {
			  return _builder.POST( uri );
		 }

		 public static Response Post( string uri, object payload )
		 {
			  return _builder.POST( uri, payload );
		 }

		 public static Response Post( string uri, RawPayload payload )
		 {
			  return _builder.POST( uri, payload );
		 }

		 public static Response Get( string uri )
		 {
			  return _builder.GET( uri );
		 }

		 public static Response Request( string method, string uri, object payload )
		 {
			  return _builder.request( method, uri, payload );
		 }

		 /// <summary>
		 /// Create a Jersey HTTP client that is able to talk HTTPS and trusts all certificates.
		 /// </summary>
		 /// <returns> new client. </returns>
		 private static Client CreateClient()
		 {
			  try
			  {
					HostnameVerifier hostnameVerifier = HttpsURLConnection.DefaultHostnameVerifier;
					ClientConfig config = new DefaultClientConfig();
					SSLContext ctx = SSLContext.getInstance( "TLS" );
					ctx.init( null, new TrustManager[]{ new InsecureTrustManager() }, null );
					IDictionary<string, object> properties = config.Properties;
					properties[PROPERTY_HTTPS_PROPERTIES] = new HTTPSProperties( hostnameVerifier, ctx );
					properties[PROPERTY_FOLLOW_REDIRECTS] = false;
					return Client.create( config );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 public class Builder
		 {
			  internal readonly IDictionary<string, string> Headers;
			  internal readonly string BaseUri;

			  internal Builder() : this(Collections.emptyMap(), "")
			  {
			  }

			  internal Builder( IDictionary<string, string> headers, string baseUri )
			  {
					this.BaseUri = baseUri;
					this.Headers = unmodifiableMap( headers );
			  }

			  public virtual Builder WithHeaders( params string[] kvPairs )
			  {
					return WithHeaders( stringMap( kvPairs ) );
			  }

			  public virtual Builder WithHeaders( IDictionary<string, string> newHeaders )
			  {
					Dictionary<string, string> combined = new Dictionary<string, string>();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					combined.putAll( Headers );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					combined.putAll( newHeaders );
					return new Builder( combined, BaseUri );
			  }

			  public virtual Builder WithBaseUri( string baseUri )
			  {
					return new Builder( Headers, baseUri );
			  }

			  public virtual Response Post( string uri )
			  {
					return Request( "POST", uri );
			  }

			  public virtual Response Post( string uri, object payload )
			  {
					return Request( "POST", uri, payload );
			  }

			  public virtual Response Post( string uri, RawPayload payload )
			  {
					return Request( "POST", uri, payload );
			  }

			  public virtual Response Delete( string uri )
			  {
					return Request( "DELETE", uri );
			  }

			  public virtual Response Get( string uri )
			  {
					return Request( "GET", uri );
			  }

			  public virtual Response Request( string method, string uri )
			  {
					return new Response( _client.handle( Build().build(BuildUri(uri), method) ) );
			  }

			  public virtual Response Request( string method, string uri, object payload )
			  {
					if ( payload == null )
					{
						 return Request( method, uri );
					}
					string jsonPayload = payload is RawPayload ? ( ( RawPayload ) payload ).Get() : createJsonFrom(payload);
					ClientRequest.Builder lastBuilder = Build().entity(jsonPayload, MediaType.APPLICATION_JSON_TYPE);

					return new Response( _client.handle( lastBuilder.build( BuildUri( uri ), method ) ) );
			  }

			  internal virtual URI BuildUri( string uri )
			  {
					URI unprefixedUri = URI.create( uri );
					if ( unprefixedUri.Absolute )
					{
						 return unprefixedUri;
					}
					else
					{
						 return URI.create( BaseUri + uri );
					}
			  }

			  internal virtual ClientRequest.Builder Build()
			  {
					ClientRequest.Builder builder = ClientRequest.create();
					foreach ( KeyValuePair<string, string> header in Headers.SetOfKeyValuePairs() )
					{
						 builder = builder.header( header.Key, header.Value );
					}

					return builder;
			  }
		 }

		 /// <summary>
		 /// Check some general validations that all REST responses should always pass.
		 /// </summary>
		 public static ClientResponse SanityCheck( ClientResponse response )
		 {
			  IList<string> contentEncodings = response.Headers.get( "Content-Encoding" );
			  string contentEncoding;
			  if ( contentEncodings != null && !string.ReferenceEquals( ( contentEncoding = Iterables.singleOrNull( contentEncodings ) ), null ) )
			  {
					// Specifically, this is never used for character encoding.
					contentEncoding = contentEncoding.ToLower();
					assertThat( contentEncoding, anyOf( containsString( "gzip" ), containsString( "deflate" ) ) );
					assertThat( contentEncoding, allOf( not( containsString( "utf-8" ) ) ) );
			  }
			  return response;
		 }

		 public class Response
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly ClientResponse ResponseConflict;
			  internal readonly string Entity;

			  public Response( ClientResponse response )
			  {
					this.ResponseConflict = SanityCheck( response );
					this.Entity = response.getEntity( typeof( string ) );
			  }

			  public virtual int Status()
			  {
					return ResponseConflict.Status;
			  }

			  public virtual string Location()
			  {
					if ( ResponseConflict.Location != null )
					{
						 return ResponseConflict.Location.ToString();
					}
					throw new Exception( "The request did not contain a location header, " + "unable to provide location. Status code was: " + Status() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public <T> T content()
			  public virtual T Content<T>()
			  {
					try
					{
						 return ( T ) JsonHelper.readJson( Entity );
					}
					catch ( JsonParseException e )
					{
						 throw new Exception( "Unable to deserialize: " + Entity, e );
					}
			  }

			  public virtual string RawContent()
			  {
					return Entity;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String stringFromContent(String key) throws org.neo4j.server.rest.domain.JsonParseException
			  public virtual string StringFromContent( string key )
			  {
					return Get( key ).asText();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.codehaus.jackson.JsonNode get(String fieldName) throws org.neo4j.server.rest.domain.JsonParseException
			  public virtual JsonNode Get( string fieldName )
			  {
					return JsonHelper.jsonNode( Entity ).get( fieldName );
			  }

			  public virtual string Header( string name )
			  {
					return ResponseConflict.Headers.getFirst( name );
			  }

			  public override string ToString()
			  {
					StringBuilder sb = new StringBuilder();
					sb.Append( "HTTP " ).Append( ResponseConflict.Status ).Append( "\n" );
					foreach ( KeyValuePair<string, IList<string>> header in ResponseConflict.Headers.entrySet() )
					{
						 foreach ( string headerEntry in header.Value )
						 {
							  sb.Append( header.Key + ": " ).Append( headerEntry ).Append( "\n" );
						 }
					}
					sb.Append( "\n" );
					sb.Append( Entity ).Append( "\n" );

					return sb.ToString();
			  }
		 }

		 public class RawPayload
		 {
			  internal readonly string Payload;

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public static RawPayload RawPayloadConflict( string payload )
			  {
					return new RawPayload( payload );
			  }

			  public static RawPayload QuotedJson( string json )
			  {
					return new RawPayload( json.replaceAll( "'", "\"" ) );
			  }

			  internal RawPayload( string payload )
			  {
					this.Payload = payload;
			  }

			  public virtual string Get()
			  {
					return Payload;
			  }
		 }
	}

}