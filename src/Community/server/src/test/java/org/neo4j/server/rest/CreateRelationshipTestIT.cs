using System.Collections.Generic;

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
namespace Neo4Net.Server.rest
{
	using Status = com.sun.jersey.api.client.ClientResponse.Status;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using RelationshipRepresentationTest = Neo4Net.Server.rest.repr.RelationshipRepresentationTest;
	using Graph = Neo4Net.Test.GraphDescription.Graph;
	using Title = Neo4Net.Test.TestData.Title;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CreateRelationshipTestIT : AbstractRestFunctionalDocTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("Joe knows Sara") @Documented("Upon successful creation of a relationship, the new relationship is returned.") @Title("Create a relationship with properties") public void create_a_relationship_with_properties()
		 [Graph("Joe knows Sara"), Documented("Upon successful creation of a relationship, the new relationship is returned."), Title("Create a relationship with properties")]
		 public virtual void CreateARelationshipWithProperties()
		 {
			  string jsonString = "{\"to\" : \""
										 + DataUri + "node/"
										 + GetNode( "Sara" ).Id + "\", \"type\" : \"LOVES\", \"data\" : {\"foo\" : \"bar\"}}";
			  Node i = GetNode( "Joe" );
			  GenConflict.get().expectedStatus(Status.CREATED.StatusCode).payload(jsonString).post(GetNodeUri(i) + "/relationships");
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					assertTrue( i.HasRelationship( RelationshipType.withName( "LOVES" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Upon successful creation of a relationship, the new relationship is returned.") @Title("Create relationship") @Graph("Joe knows Sara") public void create_relationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Upon successful creation of a relationship, the new relationship is returned."), Title("Create relationship"), Graph("Joe knows Sara")]
		 public virtual void CreateRelationship()
		 {
			  string jsonString = "{\"to\" : \""
										 + DataUri + "node/"
										 + GetNode( "Sara" ).Id + "\", \"type\" : \"LOVES\"}";
			  Node i = GetNode( "Joe" );
			  string entity = GenConflict.get().expectedStatus(Status.CREATED.StatusCode).payload(jsonString).post(GetNodeUri(i) + "/relationships").entity();
			  using ( Transaction tx = Graphdb().beginTx() )
			  {
					assertTrue( i.HasRelationship( RelationshipType.withName( "LOVES" ) ) );
			  }
			  AssertProperRelationshipRepresentation( JsonHelper.jsonToMap( entity ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("Joe knows Sara") public void shouldRespondWith404WhenStartNodeDoesNotExist()
		 [Graph("Joe knows Sara")]
		 public virtual void ShouldRespondWith404WhenStartNodeDoesNotExist()
		 {
			  string jsonString = "{\"to\" : \""
										 + DataUri + "node/"
										 + GetNode( "Joe" ) + "\", \"type\" : \"LOVES\", \"data\" : {\"foo\" : \"bar\"}}";
			  GenConflict.get().expectedStatus(Status.NOT_FOUND.StatusCode).expectedType(MediaType.TEXT_HTML_TYPE).payload(jsonString).post(DataUri + "/node/12345/relationships").entity();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("Joe knows Sara") public void creating_a_relationship_to_a_nonexisting_end_node()
		 [Graph("Joe knows Sara")]
		 public virtual void CreatingARelationshipToANonexistingEndNode()
		 {
			  string jsonString = "{\"to\" : \""
										 + DataUri + "node/"
										 + "999999\", \"type\" : \"LOVES\", \"data\" : {\"foo\" : \"bar\"}}";
			  GenConflict.get().expectedStatus(Status.BAD_REQUEST.StatusCode).payload(jsonString).post(GetNodeUri(GetNode("Joe")) + "/relationships").entity();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("Joe knows Sara") public void creating_a_loop_relationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph("Joe knows Sara")]
		 public virtual void CreatingALoopRelationship()
		 {

			  Node joe = GetNode( "Joe" );
			  string jsonString = "{\"to\" : \"" + GetNodeUri( joe ) + "\", \"type\" : \"LOVES\"}";
			  string entity = GenConflict.get().expectedStatus(Status.CREATED.StatusCode).payload(jsonString).post(GetNodeUri(GetNode("Joe")) + "/relationships").entity();
			  AssertProperRelationshipRepresentation( JsonHelper.jsonToMap( entity ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("Joe knows Sara") public void providing_bad_JSON()
		 [Graph("Joe knows Sara")]
		 public virtual void ProvidingBadJSON()
		 {
			  string jsonString = "{\"to\" : \""
										 + getNodeUri( Data.get()["Joe"] ) + "\", \"type\" : \"LOVES\", \"data\" : {\"foo\" : **BAD JSON HERE*** \"bar\"}}";
			  GenConflict.get().expectedStatus(Status.BAD_REQUEST.StatusCode).payload(jsonString).post(GetNodeUri(GetNode("Joe")) + "/relationships").entity();
		 }

		 private void AssertProperRelationshipRepresentation( IDictionary<string, object> relrep )
		 {
			  RelationshipRepresentationTest.verifySerialisation( relrep );
		 }
	}

}