﻿using System.Collections.Generic;

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
	using Test = org.junit.Test;

	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class DiscoveryServiceIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200WhenRetrievingDiscoveryDocument()
		 public virtual void ShouldRespondWith200WhenRetrievingDiscoveryDocument()
		 {
			  JaxRsResponse response = DiscoveryDocument;
			  assertEquals( 200, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetContentLengthHeaderWhenRetrievingDiscoveryDocument()
		 public virtual void ShouldGetContentLengthHeaderWhenRetrievingDiscoveryDocument()
		 {
			  JaxRsResponse response = DiscoveryDocument;
			  assertNotNull( response.Headers.get( "Content-Length" ) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveJsonMediaTypeWhenRetrievingDiscoveryDocument()
		 public virtual void ShouldHaveJsonMediaTypeWhenRetrievingDiscoveryDocument()
		 {
			  JaxRsResponse response = DiscoveryDocument;
			  assertThat( response.Type.ToString(), containsString(APPLICATION_JSON) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveJsonDataInResponse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveJsonDataInResponse()
		 {
			  JaxRsResponse response = DiscoveryDocument;

			  IDictionary<string, object> map = JsonHelper.jsonToMap( response.Entity );

			  string managementKey = "management";
			  assertTrue( map.ContainsKey( managementKey ) );
			  assertNotNull( map[managementKey] );

			  string dataKey = "data";
			  assertTrue( map.ContainsKey( dataKey ) );
			  assertNotNull( map[dataKey] );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRedirectOnHtmlRequest()
		 public virtual void ShouldRedirectOnHtmlRequest()
		 {
			  Client nonRedirectingClient = Client.create();
			  nonRedirectingClient.FollowRedirects = false;

			  JaxRsResponse clientResponse = ( new RestRequest( null, nonRedirectingClient ) ).Get( Server().baseUri().ToString(), TEXT_HTML_TYPE );

			  assertEquals( 303, clientResponse.Status );
		 }

		 private JaxRsResponse DiscoveryDocument
		 {
			 get
			 {
				  return ( new RestRequest( Server().baseUri() ) ).get();
			 }
		 }
	}

}