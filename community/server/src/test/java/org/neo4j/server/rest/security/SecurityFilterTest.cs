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
namespace Org.Neo4j.Server.rest.security
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SecurityFilterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassThroughRequestToAnUnsecuredPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPassThroughRequestToAnUnsecuredPath()
		 {
			  // given
			  SecurityRule rule = mock( typeof( SecurityRule ) );
			  when( rule.ForUriPath() ).thenReturn("/some-path");

			  FilterChain filterChain = mock( typeof( FilterChain ) );

			  SecurityFilter securityFilter = new SecurityFilter( rule );

			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.ContextPath ).thenReturn( "/some-other-path" );

			  // when
			  securityFilter.DoFilter( request, mock( typeof( HttpServletResponse ) ), filterChain );

			  // then
			  verify( filterChain ).doFilter( any( typeof( HttpServletRequest ) ), any( typeof( HttpServletResponse ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActivateRuleThatRejectsTheRequestForAMatchingPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActivateRuleThatRejectsTheRequestForAMatchingPath()
		 {
			  // given
			  SecurityRule rule = mock( typeof( SecurityRule ) );
			  when( rule.ForUriPath() ).thenReturn("/some-path");
			  when( rule.IsAuthorized( any( typeof( HttpServletRequest ) ) ) ).thenReturn( false );

			  FilterChain filterChain = mock( typeof( FilterChain ) );

			  SecurityFilter securityFilter = new SecurityFilter( rule );

			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.ContextPath ).thenReturn( "/some-path" );

			  // when
			  securityFilter.DoFilter( request, mock( typeof( HttpServletResponse ) ), filterChain );

			  // then
			  verify( filterChain, never() ).doFilter(any(typeof(HttpServletRequest)), any(typeof(HttpServletResponse)));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActivateRuleThatAcceptsTheRequestForAMatchingPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActivateRuleThatAcceptsTheRequestForAMatchingPath()
		 {
			  // given
			  SecurityRule rule = mock( typeof( SecurityRule ) );
			  when( rule.ForUriPath() ).thenReturn("/some-path");
			  when( rule.IsAuthorized( any( typeof( HttpServletRequest ) ) ) ).thenReturn( true );

			  FilterChain filterChain = mock( typeof( FilterChain ) );

			  SecurityFilter securityFilter = new SecurityFilter( rule );

			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.ContextPath ).thenReturn( "/some-path" );

			  HttpServletResponse response = mock( typeof( HttpServletResponse ) );

			  // when
			  securityFilter.DoFilter( request, response, filterChain );

			  // then
			  verify( filterChain ).doFilter( request, response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveRules()
		 {
			  // given
			  SecurityRule securityRule1 = mock( typeof( SecurityRule ) );
			  when( securityRule1.ForUriPath() ).thenReturn("/securityRule1");

			  SecurityRule securityRule2 = mock( typeof( SecurityRule ) );
			  when( securityRule2.ForUriPath() ).thenReturn("/securityRule2");

			  SecurityFilter securityFilter = new SecurityFilter( securityRule1, securityRule2 );

			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  HttpServletResponse response = mock( typeof( HttpServletResponse ) );
			  FilterChain filterChain = mock( typeof( FilterChain ) );

			  // when
			  securityFilter.Destroy();
			  securityFilter.DoFilter( request, response, filterChain );

			  // then
			  verify( filterChain ).doFilter( request, response );
		 }
	}

}