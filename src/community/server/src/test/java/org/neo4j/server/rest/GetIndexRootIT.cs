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
namespace Neo4Net.Server.rest
{
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;

	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.jsonToMap;

	public class GetIndexRootIT : AbstractRestFunctionalTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
		 }

		 /// <summary>
		 /// /db/data/index is not itself a resource
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404ForNonResourceIndexPath()
		 public virtual void ShouldRespondWith404ForNonResourceIndexPath()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexUri());
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

		 /// <summary>
		 /// /db/data/index/node should be a resource with no content
		 /// </summary>
		 /// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithNodeIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWithNodeIndexes()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.nodeIndexUri());
			  AssertResponseContainsNoIndexesOtherThanAutoIndexes( response );
			  response.Close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertResponseContainsNoIndexesOtherThanAutoIndexes(JaxRsResponse response) throws Neo4Net.server.rest.domain.JsonParseException
		 private void AssertResponseContainsNoIndexesOtherThanAutoIndexes( JaxRsResponse response )
		 {
			  switch ( response.Status )
			  {
			  case 204:
					return; // OK no auto indices
			  case 200:
					assertEquals( 0, _functionalTestHelper.removeAnyAutoIndex( jsonToMap( response.Entity ) ).Count );
					break;
			  default:
					fail( "Invalid response code " + response.Status );
				break;
			  }
		 }

		 /// <summary>
		 /// /db/data/index/relationship should be a resource with no content
		 /// </summary>
		 /// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithRelationshipIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWithRelationshipIndexes()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.relationshipIndexUri());
			  AssertResponseContainsNoIndexesOtherThanAutoIndexes( response );
			  response.Close();
		 }

		 // TODO More tests...
	}

}