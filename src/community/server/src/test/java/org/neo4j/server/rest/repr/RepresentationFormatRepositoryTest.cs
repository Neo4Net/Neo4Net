using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.repr
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using Mockito = org.mockito.Mockito;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;

	public class RepresentationFormatRepositoryTest
	{
		 private readonly RepresentationFormatRepository _repository = new RepresentationFormatRepository( null );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canProvideJsonFormat()
		 public virtual void CanProvideJsonFormat()
		 {
			  assertNotNull( _repository.inputFormat( MediaType.ValueOf( "application/json" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canProvideUTF8EncodedJsonFormat()
		 public virtual void CanProvideUTF8EncodedJsonFormat()
		 {
			  assertNotNull( _repository.inputFormat( MediaType.ValueOf( "application/json;charset=UTF-8" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = MediaTypeNotSupportedException.class) public void canNotGetInputFormatBasedOnWildcardMediaType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanNotGetInputFormatBasedOnWildcardMediaType()
		 {
			  InputFormat format = _repository.inputFormat( MediaType.WILDCARD_TYPE );
			  format.ReadValue( "foo" );
			  fail( "Got InputFormat based on wild card type: " + format );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canProvideJsonOutputFormat()
		 public virtual void CanProvideJsonOutputFormat()
		 {
			  OutputFormat format = _repository.outputFormat( new IList<MediaType> { MediaType.APPLICATION_JSON_TYPE }, null, null );
			  assertNotNull( format );
			  assertEquals( "\"test\"", format.Assemble( ValueRepresentation.String( "test" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cannotProvideStreamingForOtherMediaTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CannotProvideStreamingForOtherMediaTypes()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.core.Response.ResponseBuilder responseBuilder = mock(javax.ws.rs.core.Response.ResponseBuilder.class);
			  Response.ResponseBuilder responseBuilder = mock( typeof( Response.ResponseBuilder ) );
			  // no streaming
			  when( responseBuilder.entity( any( typeof( sbyte[] ) ) ) ).thenReturn( responseBuilder );
			  Mockito.verify( responseBuilder, never() ).entity(isA(typeof(StreamingOutput)));
			  when( responseBuilder.type( ArgumentMatchers.any<MediaType>() ) ).thenReturn(responseBuilder);
			  when( responseBuilder.build() ).thenReturn(null);
			  OutputFormat format = _repository.outputFormat( new IList<MediaType> { MediaType.TEXT_HTML_TYPE }, new URI( "http://some.host" ), StreamingHeader() );
			  assertNotNull( format );
			  format.Response( responseBuilder, new ExceptionRepresentation( new Exception() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canProvideStreamingJsonOutputFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanProvideStreamingJsonOutputFormat()
		 {
			  Response response = mock( typeof( Response ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<javax.ws.rs.core.StreamingOutput> ref = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<StreamingOutput> @ref = new AtomicReference<StreamingOutput>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.core.Response.ResponseBuilder responseBuilder = mockResponseBuilder(response, ref);
			  Response.ResponseBuilder responseBuilder = MockResponseBuilder( response, @ref );
			  OutputFormat format = _repository.outputFormat( new IList<MediaType> { MediaType.APPLICATION_JSON_TYPE }, null, StreamingHeader() );
			  assertNotNull( format );
			  Response returnedResponse = format.Response( responseBuilder, new MapRepresentation( map( "a", "test" ) ) );
			  assertSame( response, returnedResponse );
			  StreamingOutput streamingOutput = @ref.get();
			  MemoryStream baos = new MemoryStream();
			  streamingOutput.write( baos );
			  assertEquals( "{\"a\":\"test\"}", baos.ToString() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.ws.rs.core.Response.ResponseBuilder mockResponseBuilder(javax.ws.rs.core.Response response, final java.util.concurrent.atomic.AtomicReference<javax.ws.rs.core.StreamingOutput> ref)
		 private Response.ResponseBuilder MockResponseBuilder( Response response, AtomicReference<StreamingOutput> @ref )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.core.Response.ResponseBuilder responseBuilder = mock(javax.ws.rs.core.Response.ResponseBuilder.class);
			  Response.ResponseBuilder responseBuilder = mock( typeof( Response.ResponseBuilder ) );
			  when( responseBuilder.entity( ArgumentMatchers.isA( typeof( StreamingOutput ) ) ) ).thenAnswer(invocationOnMock =>
			  {
						  @ref.set( invocationOnMock.getArgument( 0 ) );
						  return responseBuilder;
			  });
			  when( responseBuilder.type( ArgumentMatchers.any<MediaType>() ) ).thenReturn(responseBuilder);
			  when( responseBuilder.build() ).thenReturn(response);
			  return responseBuilder;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private javax.ws.rs.core.MultivaluedMap<String, String> streamingHeader()
		 private MultivaluedMap<string, string> StreamingHeader()
		 {
			  MultivaluedMap<string, string> headers = mock( typeof( MultivaluedMap ) );
			  when( headers.getFirst( StreamingFormat_Fields.STREAM_HEADER ) ).thenReturn( "true" );
			  return headers;
		 }
	}

}