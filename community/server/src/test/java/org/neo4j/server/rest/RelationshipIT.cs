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
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;
	using JsonParseException = Org.Neo4j.Server.rest.domain.JsonParseException;
	using StreamingFormat = Org.Neo4j.Server.rest.repr.StreamingFormat;
	using GraphDescription = Org.Neo4j.Test.GraphDescription;
	using Graph = Org.Neo4j.Test.GraphDescription.Graph;
	using NODE = Org.Neo4j.Test.GraphDescription.NODE;
	using PROP = Org.Neo4j.Test.GraphDescription.PROP;
	using REL = Org.Neo4j.Test.GraphDescription.REL;
	using Title = Org.Neo4j.Test.TestData.Title;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNot.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class RelationshipIT : AbstractRestFunctionalDocTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Remove properties from a relationship") @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) public void shouldReturn204WhenPropertiesAreRemovedFromRelationship()
		 [Title("Remove properties from a relationship"), Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void ShouldReturn204WhenPropertiesAreRemovedFromRelationship()
		 {
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  Gen().expectedStatus(Status.NO_CONTENT.StatusCode).delete(_functionalTestHelper.relationshipPropertiesUri(loves.Id)).entity();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("I know you") public void get_Relationship_by_ID() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph("I know you")]
		 public virtual void GetRelationshipByID()
		 {
			  Node node = Data.get()["I"];
			  Relationship relationship;
			  using ( Transaction transaction = node.GraphDatabase.beginTx() )
			  {
					relationship = node.GetSingleRelationship( RelationshipType.withName( "know" ), Direction.OUTGOING );
			  }
			  string response = Gen().expectedStatus(ClientResponse.Status.OK.StatusCode).get(GetRelationshipUri(relationship)).entity();
			  assertTrue( JsonHelper.jsonToMap( response ).ContainsKey( "start" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Remove property from a relationship") @Documented("See the example request below.") @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) public void shouldReturn204WhenPropertyIsRemovedFromRelationship()
		 [Title("Remove property from a relationship"), Documented("See the example request below."), Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void ShouldReturn204WhenPropertyIsRemovedFromRelationship()
		 {
			  Data.get();
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  Gen().expectedStatus(Status.NO_CONTENT.StatusCode).delete(GetPropertiesUri(loves) + "/cost").entity();

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Remove non-existent property from a relationship") @Documented("Attempting to remove a property that doesn't exist results in an error.") @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) public void shouldReturn404WhenPropertyWhichDoesNotExistRemovedFromRelationship()
		 [Title("Remove non-existent property from a relationship"), Documented("Attempting to remove a property that doesn't exist results in an error."), Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void ShouldReturn404WhenPropertyWhichDoesNotExistRemovedFromRelationship()
		 {
			  Data.get();
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  Gen().expectedStatus(Status.NOT_FOUND.StatusCode).delete(GetPropertiesUri(loves) + "/non-existent").entity();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) public void shouldReturn404WhenPropertyWhichDoesNotExistRemovedFromRelationshipStreaming()
		 [Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void ShouldReturn404WhenPropertyWhichDoesNotExistRemovedFromRelationshipStreaming()
		 {
			  Data.get();
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  Gen().withHeader(Org.Neo4j.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER, "true").expectedStatus(Status.NOT_FOUND.StatusCode).delete(GetPropertiesUri(loves) + "/non-existent").entity();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("I know you") @Title("Remove properties from a non-existing relationship") @Documented("Attempting to remove all properties from a relationship which doesn't exist results in an error.") public void shouldReturn404WhenPropertiesRemovedFromARelationshipWhichDoesNotExist()
		 [Graph("I know you"), Title("Remove properties from a non-existing relationship"), Documented("Attempting to remove all properties from a relationship which doesn't exist results in an error.")]
		 public virtual void ShouldReturn404WhenPropertiesRemovedFromARelationshipWhichDoesNotExist()
		 {
			  Data.get();
			  Gen().expectedStatus(Status.NOT_FOUND.StatusCode).delete(_functionalTestHelper.relationshipPropertiesUri(1234L)).entity();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("I know you") @Title("Remove property from a non-existing relationship") @Documented("Attempting to remove a property from a relationship which doesn't exist results in an error.") public void shouldReturn404WhenPropertyRemovedFromARelationshipWhichDoesNotExist()
		 [Graph("I know you"), Title("Remove property from a non-existing relationship"), Documented("Attempting to remove a property from a relationship which doesn't exist results in an error.")]
		 public virtual void ShouldReturn404WhenPropertyRemovedFromARelationshipWhichDoesNotExist()
		 {
			  Data.get();
			  Gen().expectedStatus(Status.NOT_FOUND.StatusCode).delete(_functionalTestHelper.relationshipPropertiesUri(1234L) + "/cost").entity();

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) @Title("Delete relationship") public void removeRelationship()
		 [Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})}), Title("Delete relationship")]
		 public virtual void RemoveRelationship()
		 {
			  Data.get();
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  Gen().expectedStatus(Status.NO_CONTENT.StatusCode).delete(GetRelationshipUri(loves)).entity();

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) public void get_single_property_on_a_relationship()
		 [Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void GetSinglePropertyOnARelationship()
		 {
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  string response = Gen().expectedStatus(ClientResponse.Status.OK).get(GetRelPropURI(loves, "cost")).entity();
			  assertTrue( response.Contains( "high" ) );
		 }

		 private string GetRelPropURI( Relationship loves, string propertyKey )
		 {
			  return GetRelationshipUri( loves ) + "/properties/" + propertyKey;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) public void set_single_property_on_a_relationship()
		 [Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void SetSinglePropertyOnARelationship()
		 {
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  assertThat( loves, inTx( Graphdb(), hasProperty("cost").withValue("high") ) );
			  Gen().expectedStatus(ClientResponse.Status.NO_CONTENT).payload("\"deadly\"").put(GetRelPropURI(loves, "cost")).entity();
			  assertThat( loves, inTx( Graphdb(), hasProperty("cost").withValue("deadly") ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING), @PROP(key = "since", value = "1day", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) public void set_all_properties_on_a_relationship()
		 [Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING), @PROP(key : "since", value : "1day", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void SetAllPropertiesOnARelationship()
		 {
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  assertThat( loves, inTx( Graphdb(), hasProperty("cost").withValue("high") ) );
			  Gen().expectedStatus(ClientResponse.Status.NO_CONTENT).payload(JsonHelper.createJsonFrom(MapUtil.map("happy", false))).put(GetRelPropsURI(loves)).entity();
			  assertThat( loves, inTx( Graphdb(), hasProperty("happy").withValue(false) ) );
			  assertThat( loves, inTx( Graphdb(), not(hasProperty("cost")) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "Romeo", setNameProperty = true), @NODE(name = "Juliet", setNameProperty = true)}, relationships = { @REL(start = "Romeo", end = "Juliet", type = "LOVES", properties = { @PROP(key = "cost", value = "high", type = org.neo4j.test.GraphDescription.PropType.STRING), @PROP(key = "since", value = "1day", type = org.neo4j.test.GraphDescription.PropType.STRING)})}) public void get_all_properties_on_a_relationship()
		 [Graph(nodes : {@NODE(name : "Romeo", setNameProperty : true), @NODE(name : "Juliet", setNameProperty : true)}, relationships : { @REL(start : "Romeo", end : "Juliet", type : "LOVES", properties : { @PROP(key : "cost", value : "high", type : Org.Neo4j.Test.GraphDescription.PropType.STRING), @PROP(key : "since", value : "1day", type : Org.Neo4j.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void GetAllPropertiesOnARelationship()
		 {
			  Relationship loves = FirstRelationshipFromRomeoNode;
			  string response = Gen().expectedStatus(ClientResponse.Status.OK).get(GetRelPropsURI(loves)).entity();
			  assertTrue( response.Contains( "high" ) );
		 }

		 private Relationship FirstRelationshipFromRomeoNode
		 {
			 get
			 {
				  Node romeo = GetNode( "Romeo" );
   
				  using ( Transaction transaction = romeo.GraphDatabase.beginTx() )
				  {
						ResourceIterable<Relationship> relationships = ( ResourceIterable<Relationship> ) romeo.Relationships;
						using ( ResourceIterator<Relationship> iterator = relationships.GetEnumerator() )
						{
	//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 return iterator.next();
						}
				  }
			 }
		 }

		 private string GetRelPropsURI( Relationship rel )
		 {
			  return GetRelationshipUri( rel ) + "/properties";
		 }
	}

}