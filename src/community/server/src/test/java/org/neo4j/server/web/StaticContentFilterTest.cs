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
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class StaticContentFilterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddStaticContentHeadersToHtmlResponses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddStaticContentHeadersToHtmlResponses()
		 {
			  // given
			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.ServletPath ).thenReturn( "index.html" );
			  HttpServletResponse response = mock( typeof( HttpServletResponse ) );
			  FilterChain filterChain = mock( typeof( FilterChain ) );

			  // when
			  ( new StaticContentFilter() ).DoFilter(request, response, filterChain);

			  // then
			  verify( response ).addHeader( "Cache-Control", "private, no-cache, no-store, proxy-revalidate, no-transform" );
			  verify( response ).addHeader( "Pragma", "no-cache" );
			  verify( response ).addHeader( "Content-Security-Policy", "frame-ancestors 'none'" );
			  verify( response ).addHeader( "X-Frame-Options", "DENY" );
			  verify( response ).addHeader( "X-Content-Type-Options", "nosniff" );
			  verify( response ).addHeader( "X-XSS-Protection", "1; mode=block" );
			  verify( filterChain ).doFilter( request, response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassThroughRequestsForNonHtmlResources() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPassThroughRequestsForNonHtmlResources()
		 {
			  // given
			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.ServletPath ).thenReturn( "index.js" );
			  HttpServletResponse response = mock( typeof( HttpServletResponse ) );
			  FilterChain filterChain = mock( typeof( FilterChain ) );

			  // when
			  ( new StaticContentFilter() ).DoFilter(request, response, filterChain);

			  // then
			  verifyZeroInteractions( response );
			  verify( filterChain ).doFilter( request, response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassThroughRequestsWithNullServletPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPassThroughRequestsWithNullServletPath()
		 {
			  // given
			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.ServletPath ).thenReturn( null );
			  HttpServletResponse response = mock( typeof( HttpServletResponse ) );
			  FilterChain filterChain = mock( typeof( FilterChain ) );

			  // when
			  ( new StaticContentFilter() ).DoFilter(request, response, filterChain);

			  // then
			  verifyZeroInteractions( response );
			  verify( filterChain ).doFilter( request, response );
		 }
	}

}