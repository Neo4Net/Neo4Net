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
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using RelationshipDirection = Neo4Net.Server.rest.domain.RelationshipDirection;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class HtmlIT : AbstractRestFunctionalTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;
		 private long _thomasAnderson;
		 private long _trinity;
		 private long _thomasAndersonLovesTrinity;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTheDatabase()
		 public virtual void SetupTheDatabase()
		 {
			  // Create the matrix example
			  _thomasAnderson = CreateAndIndexNode( "Thomas Anderson" );
			  _trinity = CreateAndIndexNode( "Trinity" );
			  long tank = CreateAndIndexNode( "Tank" );

			  long knowsRelationshipId = _helper.createRelationship( "KNOWS", _thomasAnderson, _trinity );
			  _thomasAndersonLovesTrinity = _helper.createRelationship( "LOVES", _thomasAnderson, _trinity );
			  _helper.setRelationshipProperties( _thomasAndersonLovesTrinity, Collections.singletonMap( "strength", 100 ) );
			  _helper.createRelationship( "KNOWS", _thomasAnderson, tank );
			  _helper.createRelationship( "KNOWS", _trinity, tank );

			  // index a relationship
			  _helper.createRelationshipIndex( "relationships" );
			  _helper.addRelationshipToIndex( "relationships", "key", "value", knowsRelationshipId );

			  // index a relationship
			  _helper.createRelationshipIndex( "relationships2" );
			  _helper.addRelationshipToIndex( "relationships2", "key2", "value2", knowsRelationshipId );
		 }

		 private long CreateAndIndexNode( string name )
		 {
			  long id = _helper.createNode();
			  _helper.setNodeProperties( id, Collections.singletonMap( "name", name ) );
			  _helper.addNodeToIndex( "node", "name", name, id );
			  return id;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRoot()
		 public virtual void ShouldGetRoot()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.dataUri(), MediaType.TEXT_HTML_TYPE);
			  assertEquals( Status.OK.StatusCode, response.Status );
			  AssertValidHtml( response.Entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRootWithHTTP()
		 public virtual void ShouldGetRootWithHTTP()
		 {
			  HTTP.Response response = HTTP.withHeaders( "Accept", MediaType.TEXT_HTML ).GET( _functionalTestHelper.dataUri() );
			  assertEquals( Status.OK.StatusCode, response.Status() );
			  AssertValidHtml( response.RawContent() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNodeIndexRoot()
		 public virtual void ShouldGetNodeIndexRoot()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.nodeIndexUri(), MediaType.TEXT_HTML_TYPE);
			  assertEquals( Status.OK.StatusCode, response.Status );
			  AssertValidHtml( response.Entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelationshipIndexRoot()
		 public virtual void ShouldGetRelationshipIndexRoot()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.relationshipIndexUri(), MediaType.TEXT_HTML_TYPE);
			  assertEquals( Status.OK.StatusCode, response.Status );
			  AssertValidHtml( response.Entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetTrinityWhenSearchingForHer()
		 public virtual void ShouldGetTrinityWhenSearchingForHer()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.indexNodeUri("node", "name", "Trinity"), MediaType.TEXT_HTML_TYPE);
			  assertEquals( Status.OK.StatusCode, response.Status );
			  string entity = response.Entity;
			  assertTrue( entity.Contains( "Trinity" ) );
			  AssertValidHtml( entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetThomasAndersonDirectly()
		 public virtual void ShouldGetThomasAndersonDirectly()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.nodeUri(_thomasAnderson), MediaType.TEXT_HTML_TYPE);
			  assertEquals( Status.OK.StatusCode, response.Status );
			  string entity = response.Entity;
			  assertTrue( entity.Contains( "Thomas Anderson" ) );
			  AssertValidHtml( entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetSomeRelationships()
		 public virtual void ShouldGetSomeRelationships()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RestRequest request = RestRequest.req();
			  RestRequest request = RestRequest.Req();
			  JaxRsResponse response = request.Get( _functionalTestHelper.relationshipsUri( _thomasAnderson, RelationshipDirection.all.name(), "KNOWS" ), MediaType.TEXT_HTML_TYPE );
			  assertEquals( Status.OK.StatusCode, response.Status );
			  string entity = response.Entity;
			  assertTrue( entity.Contains( "KNOWS" ) );
			  assertFalse( entity.Contains( "LOVES" ) );
			  AssertValidHtml( entity );
			  response.Close();

			  response = request.Get( _functionalTestHelper.relationshipsUri( _thomasAnderson, RelationshipDirection.all.name(), "LOVES" ), MediaType.TEXT_HTML_TYPE );

			  entity = response.Entity;
			  assertFalse( entity.Contains( "KNOWS" ) );
			  assertTrue( entity.Contains( "LOVES" ) );
			  AssertValidHtml( entity );
			  response.Close();

			  response = request.Get( _functionalTestHelper.relationshipsUri( _thomasAnderson, RelationshipDirection.all.name(), "LOVES", "KNOWS" ), MediaType.TEXT_HTML_TYPE );
			  entity = response.Entity;
			  assertTrue( entity.Contains( "KNOWS" ) );
			  assertTrue( entity.Contains( "LOVES" ) );
			  AssertValidHtml( entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetThomasAndersonLovesTrinityRelationship()
		 public virtual void ShouldGetThomasAndersonLovesTrinityRelationship()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.relationshipUri(_thomasAndersonLovesTrinity), MediaType.TEXT_HTML_TYPE);
			  assertEquals( Status.OK.StatusCode, response.Status );
			  string entity = response.Entity;
			  assertTrue( entity.Contains( "strength" ) );
			  assertTrue( entity.Contains( "100" ) );
			  assertTrue( entity.Contains( "LOVES" ) );
			  AssertValidHtml( entity );
			  response.Close();
		 }

		 private void AssertValidHtml( string entity )
		 {
			  assertTrue( entity.Contains( "<html>" ) );
			  assertTrue( entity.Contains( "</html>" ) );
		 }
	}

}