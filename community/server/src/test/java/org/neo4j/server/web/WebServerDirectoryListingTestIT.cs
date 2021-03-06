﻿/*
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
namespace Org.Neo4j.Server.web
{
	using After = org.junit.After;
	using Test = org.junit.Test;

	using CommunityServerBuilder = Org.Neo4j.Server.helpers.CommunityServerBuilder;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;
	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class WebServerDirectoryListingTestIT : ExclusiveServerTestBase
	{
		 private CommunityNeoServer _server;

		 /*
		  * The default jetty behaviour serves an index page for static resources. The 'directories' exposed through this
		  * behaviour are not file system directories, but only a list of resources present on the classpath, so there is no
		  * security vulnerability. However, it might seem like a vulnerability to somebody without the context of how the
		  * whole stack works, so to avoid confusion we disable the jetty behaviour.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisallowDirectoryListings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisallowDirectoryListings()
		 {
			  // Given
			  _server = CommunityServerBuilder.serverOnRandomPorts().build();
			  _server.start();

			  // When
			  HTTP.Response okResource = HTTP.GET( _server.baseUri().resolve("/browser/index.html").ToString() );
			  HTTP.Response illegalResource = HTTP.GET( _server.baseUri().resolve("/browser/assets/").ToString() );

			  // Then
			  // Depends on specific resources exposed by the browser module; if this test starts to fail,
			  // check whether the structure of the browser module has changed and adjust accordingly.
			  assertEquals( 200, okResource.Status() );
			  assertEquals( 403, illegalResource.Status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

	}

}