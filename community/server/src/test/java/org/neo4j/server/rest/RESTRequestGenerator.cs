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
	using ClientRequest = com.sun.jersey.api.client.ClientRequest;
	using Builder = com.sun.jersey.api.client.ClientRequest.Builder;
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;


	using Predicates = Org.Neo4j.Function.Predicates;
	using Org.Neo4j.Helpers.Collection;
	using GraphDefinition = Org.Neo4j.Test.GraphDefinition;
	using Org.Neo4j.Test.TestData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class RESTRequestGenerator
	{
		 private static readonly ClientRequest.Builder _requestBuilder = ClientRequest.create();

		 private static readonly IList<string> _responseHeaders = Arrays.asList( "Content-Type", "Location" );

		 private static readonly IList<string> _requestHeaders = Arrays.asList( "Content-Type", "Accept" );

		 public static readonly Producer<RESTRequestGenerator> PRODUCER = new ProducerAnonymousInnerClass();

		 private class ProducerAnonymousInnerClass : Producer<RESTRequestGenerator>
		 {
			 public RESTRequestGenerator create( GraphDefinition graph, string title, string documentation )
			 {
				  return new RESTRequestGenerator();
			 }

			 public void destroy( RESTRequestGenerator product, bool successful )
			 {
			 }
		 }

		 private int _expectedResponseStatus = -1;
		 private MediaType _expectedMediaType = MediaType.valueOf( "application/json; charset=UTF-8" );
		 private MediaType _payloadMediaType = MediaType.APPLICATION_JSON_TYPE;
		 private readonly IList<Pair<string, System.Predicate<string>>> _expectedHeaderFields = new List<Pair<string, System.Predicate<string>>>();
		 private string _payload;
		 private readonly IDictionary<string, string> _addedRequestHeaders = new SortedDictionary<string, string>();

		 private RESTRequestGenerator()
		 {
		 }

		 /// <summary>
		 /// Set the expected status of the response. The test will fail if the
		 /// response has a different status. Defaults to HTTP 200 OK.
		 /// </summary>
		 /// <param name="expectedResponseStatus"> the expected response status </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RESTRequestGenerator expectedStatus(final int expectedResponseStatus)
		 public virtual RESTRequestGenerator ExpectedStatus( int expectedResponseStatus )
		 {
			  this._expectedResponseStatus = expectedResponseStatus;
			  return this;
		 }

		 /// <summary>
		 /// Set the expected status of the response. The test will fail if the
		 /// response has a different status. Defaults to HTTP 200 OK.
		 /// </summary>
		 /// <param name="expectedStatus"> the expected response status </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RESTRequestGenerator expectedStatus(final com.sun.jersey.api.client.ClientResponse.Status expectedStatus)
		 public virtual RESTRequestGenerator ExpectedStatus( ClientResponse.Status expectedStatus )
		 {
			  this._expectedResponseStatus = expectedStatus.StatusCode;
			  return this;
		 }

		 /// <summary>
		 /// Set the expected media type of the response. The test will fail if the
		 /// response has a different media type. Defaults to application/json.
		 /// </summary>
		 /// <param name="expectedMediaType"> the expected media type </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RESTRequestGenerator expectedType(final javax.ws.rs.core.MediaType expectedMediaType)
		 public virtual RESTRequestGenerator ExpectedType( MediaType expectedMediaType )
		 {
			  this._expectedMediaType = expectedMediaType;
			  return this;
		 }

		 /// <summary>
		 /// The media type of the request payload. Defaults to application/json.
		 /// </summary>
		 /// <param name="payloadMediaType"> the media type to use </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RESTRequestGenerator payloadType(final javax.ws.rs.core.MediaType payloadMediaType)
		 public virtual RESTRequestGenerator PayloadType( MediaType payloadMediaType )
		 {
			  this._payloadMediaType = payloadMediaType;
			  return this;
		 }

		 /// <summary>
		 /// The additional headers for the request
		 /// </summary>
		 /// <param name="key"> header key </param>
		 /// <param name="value"> header value </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RESTRequestGenerator withHeader(final String key, final String value)
		 public virtual RESTRequestGenerator WithHeader( string key, string value )
		 {
			  this._addedRequestHeaders[key] = value;
			  return this;
		 }

		 /// <summary>
		 /// Set the payload of the request.
		 /// </summary>
		 /// <param name="payload"> the payload </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RESTRequestGenerator payload(final String payload)
		 public virtual RESTRequestGenerator Payload( string payload )
		 {
			  this._payload = payload;
			  return this;
		 }

		 /// <summary>
		 /// Add an expected response header. If the heading is missing in the
		 /// response the test will fail. The header and its value are also included
		 /// in the documentation.
		 /// </summary>
		 /// <param name="expectedHeaderField"> the expected header </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RESTRequestGenerator expectedHeader(final String expectedHeaderField)
		 public virtual RESTRequestGenerator ExpectedHeader( string expectedHeaderField )
		 {
			  this._expectedHeaderFields.Add( Pair.of( expectedHeaderField, Predicates.notNull() ) );
			  return this;
		 }

		 /// <summary>
		 /// Add an expected response header. If the heading is missing in the
		 /// response the test will fail. The header and its value are also included
		 /// in the documentation.
		 /// </summary>
		 /// <param name="expectedHeaderField"> the expected header </param>
		 /// <param name="expectedValue"> the expected header value </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public RESTRequestGenerator expectedHeader(final String expectedHeaderField, String expectedValue)
		 public virtual RESTRequestGenerator ExpectedHeader( string expectedHeaderField, string expectedValue )
		 {
			  this._expectedHeaderFields.Add( Pair.of( expectedHeaderField, System.Predicate.isEqual( expectedValue ) ) );
			  return this;
		 }

		 /// <summary>
		 /// Send a GET request.
		 /// </summary>
		 /// <param name="uri"> the URI to use. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ResponseEntity get(final String uri)
		 public virtual ResponseEntity Get( string uri )
		 {
			  return RetrieveResponseFromRequest( "GET", uri, _expectedResponseStatus, _expectedMediaType, _expectedHeaderFields );
		 }

		 /// <summary>
		 /// Send a POST request.
		 /// </summary>
		 /// <param name="uri"> the URI to use. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ResponseEntity post(final String uri)
		 public virtual ResponseEntity Post( string uri )
		 {
			  return RetrieveResponseFromRequest( "POST", uri, _payload, _payloadMediaType, _expectedResponseStatus, _expectedMediaType, _expectedHeaderFields );
		 }

		 /// <summary>
		 /// Send a PUT request.
		 /// </summary>
		 /// <param name="uri"> the URI to use. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ResponseEntity put(final String uri)
		 public virtual ResponseEntity Put( string uri )
		 {
			  return RetrieveResponseFromRequest( "PUT", uri, _payload, _payloadMediaType, _expectedResponseStatus, _expectedMediaType, _expectedHeaderFields );
		 }

		 /// <summary>
		 /// Send a DELETE request.
		 /// </summary>
		 /// <param name="uri"> the URI to use. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ResponseEntity delete(final String uri)
		 public virtual ResponseEntity Delete( string uri )
		 {
			  return RetrieveResponseFromRequest( "DELETE", uri, _payload, _payloadMediaType, _expectedResponseStatus, _expectedMediaType, _expectedHeaderFields );
		 }

		 /// <summary>
		 /// Send a request with no payload.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private ResponseEntity retrieveResponseFromRequest(final String method, final String uri, final int responseCode, final javax.ws.rs.core.MediaType accept, final java.util.List<org.neo4j.helpers.collection.Pair<String,System.Predicate<String>>> headerFields)
		 private ResponseEntity RetrieveResponseFromRequest( string method, string uri, int responseCode, MediaType accept, IList<Pair<string, System.Predicate<string>>> headerFields )
		 {
			  ClientRequest request;
			  try
			  {
					request = WithHeaders( _requestBuilder ).accept( accept ).build( new URI( uri ), method );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new Exception( e );
			  }
			  return RetrieveResponse( uri, responseCode, accept, headerFields, request );
		 }

		 /// <summary>
		 /// Send a request with payload.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private ResponseEntity retrieveResponseFromRequest(final String method, final String uri, final String payload, final javax.ws.rs.core.MediaType payloadType, final int responseCode, final javax.ws.rs.core.MediaType accept, final java.util.List<org.neo4j.helpers.collection.Pair<String,System.Predicate<String>>> headerFields)
		 private ResponseEntity RetrieveResponseFromRequest( string method, string uri, string payload, MediaType payloadType, int responseCode, MediaType accept, IList<Pair<string, System.Predicate<string>>> headerFields )
		 {
			  ClientRequest request;
			  try
			  {
					if ( !string.ReferenceEquals( payload, null ) )
					{
						 request = WithHeaders( _requestBuilder ).type( payloadType ).accept( accept ).entity( payload ).build( new URI( uri ), method );
					}
					else
					{
						 request = WithHeaders( _requestBuilder ).accept( accept ).build( new URI( uri ), method );
					}
			  }
			  catch ( URISyntaxException e )
			  {
					throw new Exception( e );
			  }
			  return RetrieveResponse( uri, responseCode, accept, headerFields, request );
		 }

		 private T WithHeaders<T>( T builder ) where T : com.sun.jersey.api.client.ClientRequest.Builder
		 {
			  foreach ( KeyValuePair<string, string> entry in _addedRequestHeaders.SetOfKeyValuePairs() )
			  {
					builder.header( entry.Key,entry.Value );
			  }
			  return builder;
		 }

		 /// <summary>
		 /// Send the request and create the documentation.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private ResponseEntity retrieveResponse(final String uri, final int responseCode, final javax.ws.rs.core.MediaType type, final java.util.List<org.neo4j.helpers.collection.Pair<String, System.Predicate<String>>> headerFields, final com.sun.jersey.api.client.ClientRequest request)
		 private ResponseEntity RetrieveResponse( string uri, int responseCode, MediaType type, IList<Pair<string, System.Predicate<string>>> headerFields, ClientRequest request )
		 {
			  RequestData data = new RequestData();
			  GetRequestHeaders( data, request.Headers );
			  if ( request.Entity != null )
			  {
					data.Payload = request.Entity.ToString();
			  }
			  Client client = new Client();
			  ClientResponse response = client.handle( request );
			  if ( response.hasEntity() && response.Status != 204 )
			  {
					data.Entity = response.getEntity( typeof( string ) );
			  }
			  if ( response.Type != null )
			  {
					assertTrue( "wrong response type: " + data.EntityConflict, response.Type.isCompatible( type ) );
			  }
			  foreach ( Pair<string, System.Predicate<string>> headerField in headerFields )
			  {
					assertTrue( "wrong headers: " + response.Headers, headerField.Other()(response.Headers.getFirst(headerField.First())) );
			  }
			  data.Method = request.Method;
			  data.Uri = uri;
			  data.Status = responseCode;
			  assertEquals( "Wrong response status. response: " + data.EntityConflict, responseCode, response.Status );
			  GetResponseHeaders( data, response.Headers, HeaderNames( headerFields ) );
			  return new ResponseEntity( response, data.EntityConflict );
		 }

		 private IList<string> HeaderNames( IList<Pair<string, System.Predicate<string>>> headerPredicates )
		 {
			  IList<string> names = new List<string>();
			  foreach ( Pair<string, System.Predicate<string>> headerPredicate in headerPredicates )
			  {
					names.Add( headerPredicate.First() );
			  }
			  return names;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void getResponseHeaders(final RequestData data, final javax.ws.rs.core.MultivaluedMap<String, String> headers, final java.util.List<String> additionalFilter)
		 private void GetResponseHeaders( RequestData data, MultivaluedMap<string, string> headers, IList<string> additionalFilter )
		 {
			  data.ResponseHeaders = GetHeaders( headers, _responseHeaders, additionalFilter );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void getRequestHeaders(final RequestData data, final javax.ws.rs.core.MultivaluedMap<String, Object> headers)
		 private void GetRequestHeaders( RequestData data, MultivaluedMap<string, object> headers )
		 {
			  data.RequestHeaders = GetHeaders( headers, _requestHeaders, _addedRequestHeaders.Keys );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private <T> java.util.Map<String, String> getHeaders(final javax.ws.rs.core.MultivaluedMap<String, T> headers, final java.util.List<String> filter, final java.util.Collection<String> additionalFilter)
		 private IDictionary<string, string> GetHeaders<T>( MultivaluedMap<string, T> headers, IList<string> filter, ICollection<string> additionalFilter )
		 {
			  IDictionary<string, string> filteredHeaders = new SortedDictionary<string, string>();
			  foreach ( KeyValuePair<string, IList<T>> header in headers.entrySet() )
			  {
					string key = header.Key;
					if ( filter.Contains( key ) || additionalFilter.Contains( key ) )
					{
						 string values = "";
						 foreach ( T value in header.Value )
						 {
							  if ( values.Length > 0 )
							  {
									values += ", ";
							  }
							  values += value.ToString();
						 }
						 filteredHeaders[key] = values;
					}
			  }
			  return filteredHeaders;
		 }

		 /// <summary>
		 /// Wraps a response, to give access to the response entity as well.
		 /// </summary>
		 public class ResponseEntity
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string EntityConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly JaxRsResponse ResponseConflict;

			  public ResponseEntity( ClientResponse response, string entity )
			  {
					this.ResponseConflict = new JaxRsResponse( response, entity );
					this.EntityConflict = entity;
			  }

			  /// <summary>
			  /// The response entity as a String.
			  /// </summary>
			  public virtual string Entity()
			  {
					return EntityConflict;
			  }

			  /// <summary>
			  /// Note that the response object returned does not give access to the
			  /// response entity.
			  /// </summary>
			  public virtual JaxRsResponse Response()
			  {
					return ResponseConflict;
			  }
		 }
	}

}