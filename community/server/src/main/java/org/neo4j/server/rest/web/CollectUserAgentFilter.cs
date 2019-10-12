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
namespace Org.Neo4j.Server.rest.web
{

	using Org.Neo4j.Util.concurrent;

	/// <summary>
	/// Collects user agent information and publishes it to a tracker of most recently seen user agents.
	/// </summary>
	public class CollectUserAgentFilter : Filter
	{
		 private readonly RecentK<string> _output;

		 public CollectUserAgentFilter( RecentK<string> output )
		 {
			  this._output = output;
		 }

		 public override void Init( FilterConfig filterConfig )
		 {

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doFilter(javax.servlet.ServletRequest servletRequest, javax.servlet.ServletResponse servletResponse, javax.servlet.FilterChain filterChain) throws java.io.IOException, javax.servlet.ServletException
		 public override void DoFilter( ServletRequest servletRequest, ServletResponse servletResponse, FilterChain filterChain )
		 {
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.servlet.http.HttpServletRequest request = (javax.servlet.http.HttpServletRequest) servletRequest;
					HttpServletRequest request = ( HttpServletRequest ) servletRequest;
					string ua = request.getHeader( "User-Agent" );
					if ( !string.ReferenceEquals( ua, null ) && ua.Length > 0 )
					{
						 _output.add( ua.Split( " ", true )[0] );
					}
			  }
			  catch ( Exception )
			  {
					// We're fine with that
			  }

			  filterChain.doFilter( servletRequest, servletResponse );
		 }

		 public override void Destroy()
		 {

		 }
	}

}