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
namespace Neo4Net.Server.rest.web
{
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;

	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using UsageData = Neo4Net.Udc.UsageData;
	using UsageDataKeys = Neo4Net.Udc.UsageDataKeys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class CollectUserAgentFilterIT : AbstractRestFunctionalTestBase
	{
		 public const string USER_AGENT = "test/1.0";
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecordUserAgent()
		 public virtual void ShouldRecordUserAgent()
		 {
			  // When
			  SendRequest( "test/1.0" );

			  // Then
			  assertThat( ResolveDependency( typeof( UsageData ) ).get( UsageDataKeys.clientNames ), hasItem( USER_AGENT ) );
		 }

		 private void SendRequest( string userAgent )
		 {
			  string url = _functionalTestHelper.baseUri().ToString();
			  JaxRsResponse resp = RestRequest.req().header("User-Agent", userAgent).get(url);
			  string json = resp.Entity;
			  resp.Close();
			  assertEquals( json, 200, resp.Status );
		 }
	}

}