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
namespace Neo4Net.Server.rest.web
{
	using Test = org.junit.Test;


	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_ALLOW_HEADERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_ALLOW_METHODS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_ALLOW_ORIGIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_REQUEST_HEADERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_REQUEST_METHOD;

	public class CorsFilterTest
	{
		 private readonly HttpServletRequest _emptyRequest = RequestMock( emptyList(), emptyList() );
		 private readonly HttpServletResponse _response = ResponseMock();
		 private readonly FilterChain _chain = FilterChainMock();

		 private readonly CorsFilter _filter = new CorsFilter( NullLogProvider.Instance, "*" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallChainDoFilter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallChainDoFilter()
		 {
			  _filter.doFilter( _emptyRequest, _response, _chain );

			  verify( _chain ).doFilter( _emptyRequest, _response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAccessControlAllowOrigin() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAccessControlAllowOrigin()
		 {
			  _filter.doFilter( _emptyRequest, _response, FilterChainMock() );

			  verify( _response ).setHeader( ACCESS_CONTROL_ALLOW_ORIGIN, "*" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAttachNoHttpMethodsToAccessControlAllowMethodsWhenHeaderIsEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAttachNoHttpMethodsToAccessControlAllowMethodsWhenHeaderIsEmpty()
		 {
			  _filter.doFilter( _emptyRequest, _response, _chain );

			  verify( _response, never() ).addHeader(eq(ACCESS_CONTROL_ALLOW_METHODS), anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAttachNoHttpMethodsToAccessControlAllowMethodsWhenHeaderIsNull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAttachNoHttpMethodsToAccessControlAllowMethodsWhenHeaderIsNull()
		 {
			  HttpServletRequest request = RequestMock();
			  when( request.getHeaders( ACCESS_CONTROL_REQUEST_METHOD ) ).thenReturn( null );

			  _filter.doFilter( request, _response, _chain );

			  verify( _response, never() ).addHeader(eq(ACCESS_CONTROL_ALLOW_METHODS), anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAttachValidHttpMethodsToAccessControlAllowMethods() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAttachValidHttpMethodsToAccessControlAllowMethods()
		 {
			  IList<string> accessControlRequestMethods = new IList<string> { "GET", "WRONG", "POST", "TAKE", "CONNECT" };
			  HttpServletRequest request = RequestMock( accessControlRequestMethods, emptyList() );

			  _filter.doFilter( request, _response, _chain );

			  verify( _response ).addHeader( ACCESS_CONTROL_ALLOW_METHODS, "GET" );
			  verify( _response ).addHeader( ACCESS_CONTROL_ALLOW_METHODS, "POST" );
			  verify( _response ).addHeader( ACCESS_CONTROL_ALLOW_METHODS, "CONNECT" );

			  verify( _response, never() ).addHeader(ACCESS_CONTROL_ALLOW_METHODS, "TAKE");
			  verify( _response, never() ).addHeader(ACCESS_CONTROL_ALLOW_METHODS, "WRONG");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAttachNoRequestHeadersToAccessControlAllowHeadersWhenHeaderIsEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAttachNoRequestHeadersToAccessControlAllowHeadersWhenHeaderIsEmpty()
		 {
			  _filter.doFilter( _emptyRequest, _response, _chain );

			  verify( _response, never() ).addHeader(eq(ACCESS_CONTROL_ALLOW_HEADERS), anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAttachNoRequestHeadersToAccessControlAllowHeadersWhenHeaderIsNull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAttachNoRequestHeadersToAccessControlAllowHeadersWhenHeaderIsNull()
		 {
			  HttpServletRequest request = RequestMock();
			  when( request.getHeaders( ACCESS_CONTROL_REQUEST_HEADERS ) ).thenReturn( null );

			  _filter.doFilter( request, _response, _chain );

			  verify( _response, never() ).addHeader(eq(ACCESS_CONTROL_ALLOW_HEADERS), anyString());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAttachValidRequestHeadersToAccessControlAllowHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAttachValidRequestHeadersToAccessControlAllowHeaders()
		 {
			  IList<string> accessControlRequestHeaders = new IList<string> { "Accept", "X-Wrong\nHeader", "Content-Type", "Accept\r", "Illegal\r\nHeader", "", null, "   " };
			  HttpServletRequest request = RequestMock( emptyList(), accessControlRequestHeaders );

			  _filter.doFilter( request, _response, _chain );

			  verify( _response ).addHeader( ACCESS_CONTROL_ALLOW_HEADERS, "Accept" );
			  verify( _response ).addHeader( ACCESS_CONTROL_ALLOW_HEADERS, "Content-Type" );

			  verify( _response, never() ).addHeader(ACCESS_CONTROL_ALLOW_HEADERS, null);
			  verify( _response, never() ).addHeader(ACCESS_CONTROL_ALLOW_HEADERS, "");
			  verify( _response, never() ).addHeader(ACCESS_CONTROL_ALLOW_HEADERS, "   ");
			  verify( _response, never() ).addHeader(ACCESS_CONTROL_ALLOW_HEADERS, "X-Wrong\nHeader");
			  verify( _response, never() ).addHeader(ACCESS_CONTROL_ALLOW_HEADERS, "Accept\r");
			  verify( _response, never() ).addHeader(ACCESS_CONTROL_ALLOW_HEADERS, "Illegal\r\nHeader");
		 }

		 private static HttpServletRequest RequestMock()
		 {
			  return RequestMock( emptyList(), emptyList() );
		 }

		 private static HttpServletRequest RequestMock( IList<string> accessControlRequestMethods, IList<string> accessControlRequestHeaders )
		 {
			  HttpServletRequest mock = mock( typeof( HttpServletRequest ) );
			  when( mock.getHeaders( ACCESS_CONTROL_REQUEST_METHOD ) ).thenReturn( enumeration( accessControlRequestMethods ) );
			  when( mock.getHeaders( ACCESS_CONTROL_REQUEST_HEADERS ) ).thenReturn( enumeration( accessControlRequestHeaders ) );
			  return mock;
		 }

		 private static HttpServletResponse ResponseMock()
		 {
			  return mock( typeof( HttpServletResponse ) );
		 }

		 private static FilterChain FilterChainMock()
		 {
			  return mock( typeof( FilterChain ) );
		 }
	}

}