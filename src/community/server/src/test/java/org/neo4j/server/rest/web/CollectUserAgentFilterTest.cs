using System;

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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.SuppressOutput.suppressAll;

	public class CollectUserAgentFilterTest
	{
		private bool InstanceFieldsInitialized = false;

		public CollectUserAgentFilterTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_filter = new CollectUserAgentFilter( _agents );
		}

		 private readonly RecentK<string> _agents = new RecentK<string>( 10 );
		 private readonly FilterChain _filterChain = mock( typeof( FilterChain ) );
		 private CollectUserAgentFilter _filter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public SuppressOutput SuppressOutput = suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecordASingleUserAgent() throws java.io.IOException, javax.servlet.ServletException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecordASingleUserAgent()
		 {
			  _filter.doFilter( Request( "the-agent" ), null, _filterChain );
			  assertThat( _agents.recentItems(), hasItem("the-agent") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyRecordTheFirstFieldOfTheUserAgentString() throws java.io.IOException, javax.servlet.ServletException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyRecordTheFirstFieldOfTheUserAgentString()
		 {
			  _filter.doFilter( Request( "the-agent other-info" ), null, _filterChain );
			  assertThat( _agents.recentItems(), hasItem("the-agent") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecordMultipleUserAgents() throws java.io.IOException, javax.servlet.ServletException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecordMultipleUserAgents()
		 {
			  _filter.doFilter( Request( "agent1" ), null, _filterChain );
			  _filter.doFilter( Request( "agent2" ), null, _filterChain );
			  assertThat( _agents.recentItems(), hasItems("agent1", "agent2") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportDuplicates() throws java.io.IOException, javax.servlet.ServletException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReportDuplicates()
		 {
			  _filter.doFilter( Request( "the-agent" ), null, _filterChain );
			  _filter.doFilter( Request( "the-agent" ), null, _filterChain );
			  assertThat( _agents.recentItems(), hasSize(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopeIfThereIsNoUserAgentHeader() throws java.io.IOException, javax.servlet.ServletException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCopeIfThereIsNoUserAgentHeader()
		 {
			  _filter.doFilter( Request( null ), null, _filterChain );
			  assertThat( _agents.recentItems(), hasSize(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSwallowAnyExceptionsThrownByTheRequest() throws java.io.IOException, javax.servlet.ServletException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSwallowAnyExceptionsThrownByTheRequest()
		 {
			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.getHeader( anyString() ) ).thenThrow(new Exception());
			  _filter.doFilter( request, null, _filterChain );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheRequest() throws java.io.IOException, javax.servlet.ServletException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTheRequest()
		 {
			  ServletRequest original = Request( "the-agent" );
			  _filter.doFilter( original, null, _filterChain );
			  verify( _filterChain ).doFilter( original, null );
		 }

		 private static ServletRequest Request( string userAgent )
		 {
			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.getHeader( "User-Agent" ) ).thenReturn( userAgent );
			  return request;
		 }
	}

}