using System;
using System.Collections.Generic;

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
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Collections.Helpers;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using GraphDescription = Neo4Net.Test.GraphDescription;
	using Graph = Neo4Net.Test.GraphDescription.Graph;
	using LABEL = Neo4Net.Test.GraphDescription.LABEL;
	using NODE = Neo4Net.Test.GraphDescription.NODE;
	using PROP = Neo4Net.Test.GraphDescription.PROP;
	using REL = Neo4Net.Test.GraphDescription.REL;
	using Title = Neo4Net.Test.TestData.Title;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.isA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNot.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.jsonToMap;

	public class CypherIT : AbstractRestFunctionalTestBase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Send a query") @Documented("A simple query returning all nodes connected to some node, returning the node and the name " + "property, if it exists, otherwise `NULL`:") @Graph(nodes = {@NODE(name = "I", setNameProperty = true), @NODE(name = "you", setNameProperty = true), @NODE(name = "him", setNameProperty = true, properties = { @PROP(key = "age", value = "25", type = Neo4Net.test.GraphDescription.PropType.INTEGER)})}, relationships = { @REL(start = "I", end = "him", type = "know", properties = {}), @REL(start = "I", end = "you", type = "know", properties = {})}) public void testPropertyColumn()
		 [Title("Send a query"), Documented("A simple query returning all nodes connected to some node, returning the node and the name " + "property, if it exists, otherwise `NULL`:"), Graph(nodes : {@NODE(name : "I", setNameProperty : true), @NODE(name : "you", setNameProperty : true), @NODE(name : "him", setNameProperty : true, properties : { @PROP(key : "age", value : "25", type : Neo4Net.Test.GraphDescription.PropType.INTEGER)})}, relationships : { @REL(start : "I", end : "him", type : "know", properties : {}), @REL(start : "I", end : "you", type : "know", properties : {})})]
		 public virtual void TestPropertyColumn()
		 {
			  string script = CreateScript( "MATCH (x {name: 'I'})-[r]->(n) RETURN type(r), n.name, n.age" );

			  string response = cypherRestCall( script, Status.OK );

			  assertThat( response, containsString( "you" ) );
			  assertThat( response, containsString( "him" ) );
			  assertThat( response, containsString( "25" ) );
			  assertThat( response, not( containsString( "\"x\"" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Retrieve query metadata") @Documented("By passing in an additional GET parameter when you execute Cypher queries, metadata about the " + "query will be returned, such as how many labels were added or removed by the query.") @Graph(nodes = {@NODE(name = "I", setNameProperty = true, labels = {@LABEL("Director")})}) public void testQueryStatistics() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Retrieve query metadata"), Documented("By passing in an additional GET parameter when you execute Cypher queries, metadata about the " + "query will be returned, such as how many labels were added or removed by the query."), Graph(nodes : {@NODE(name : "I", setNameProperty : true, labels : {@LABEL("Director")})})]
		 public virtual void TestQueryStatistics()
		 {
			  // Given
			  string script = CreateScript( "MATCH (n {name: 'I'}) SET n:Actor REMOVE n:Director RETURN labels(n)" );

			  // When
			  IDictionary<string, object> output = jsonToMap( doCypherRestCall( CypherUri() + "?includeStats=true", script, Status.OK ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String,Object> stats = (java.util.Map<String,Object>) output.get("stats");
			  IDictionary<string, object> stats = ( IDictionary<string, object> ) output["stats"];

			  assertThat( stats, isA( typeof( System.Collections.IDictionary ) ) );
			  assertThat( stats["contains_updates"], @is( true ) );
			  assertThat( stats["labels_added"], @is( 1 ) );
			  assertThat( stats["labels_removed"], @is( 1 ) );
			  assertThat( stats["nodes_created"], @is( 0 ) );
			  assertThat( stats["nodes_deleted"], @is( 0 ) );
			  assertThat( stats["properties_set"], @is( 0 ) );
			  assertThat( stats["relationships_created"], @is( 0 ) );
			  assertThat( stats["relationship_deleted"], @is( 0 ) );
		 }

		 /// <summary>
		 /// Ensure that order of data and column is ok.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", setNameProperty = true), @NODE(name = "you", setNameProperty = true), @NODE(name = "him", setNameProperty = true, properties = { @PROP(key = "age", value = "25", type = Neo4Net.test.GraphDescription.PropType.INTEGER)})}, relationships = { @REL(start = "I", end = "him", type = "know", properties = {}), @REL(start = "I", end = "you", type = "know", properties = {})}) public void testDataColumnOrder()
		 [Graph(nodes : {@NODE(name : "I", setNameProperty : true), @NODE(name : "you", setNameProperty : true), @NODE(name : "him", setNameProperty : true, properties : { @PROP(key : "age", value : "25", type : Neo4Net.Test.GraphDescription.PropType.INTEGER)})}, relationships : { @REL(start : "I", end : "him", type : "know", properties : {}), @REL(start : "I", end : "you", type : "know", properties : {})})]
		 public virtual void TestDataColumnOrder()
		 {
			  string script = CreateScript( "MATCH (x)-[r]->(n) WHERE id(x) = %I% RETURN type(r), n.name, n.age" );

			  string response = cypherRestCall( script, Status.OK );

			  assertThat( response.IndexOf( "columns", StringComparison.Ordinal ) < response.IndexOf( "data", StringComparison.Ordinal ), @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Errors") @Documented("Errors on the server will be reported as a JSON-formatted message, exception name and stacktrace.") @Graph("I know you") public void error_gets_returned_as_json() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Errors"), Documented("Errors on the server will be reported as a JSON-formatted message, exception name and stacktrace."), Graph("I know you")]
		 public virtual void ErrorGetsReturnedAsJson()
		 {
			  string response = cypherRestCall( "MATCH (x {name: 'I'}) RETURN x.dummy/0", Status.BAD_REQUEST );
			  IDictionary<string, object> output = jsonToMap( response );
			  assertTrue( output.ToString(), output.ContainsKey("message") );
			  assertTrue( output.ContainsKey( "exception" ) );
			  assertTrue( output.ContainsKey( "stackTrace" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Return paths") @Documented("Paths can be returned just like other return types.") @Graph("I know you") public void return_paths() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Return paths"), Documented("Paths can be returned just like other return types."), Graph("I know you")]
		 public virtual void ReturnPaths()
		 {
			  string script = "MATCH path = (x {name: 'I'})--(friend) RETURN path, friend.name";
			  string response = cypherRestCall( script, Status.OK );

			  assertEquals( 2, ( jsonToMap( response ) ).size() );
			  assertThat( response, containsString( "data" ) );
			  assertThat( response, containsString( "you" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Use parameters") @Documented("Cypher supports queries with parameters which are submitted as JSON.") @Graph(value = {"I know you"}, autoIndexNodes = true) public void send_queries_with_parameters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Use parameters"), Documented("Cypher supports queries with parameters which are submitted as JSON."), Graph(value : {"I know you"}, autoIndexNodes : true)]
		 public virtual void SendQueriesWithParameters()
		 {
			  Data.get();
			  string script = "MATCH (x {name: {startName}})-[r]-(friend) WHERE friend" + ".name = {name} RETURN TYPE(r)";
			  string response = cypherRestCall( script, Status.OK, Pair.of( "startName", "I" ), Pair.of( "name", "you" ) );

			  assertEquals( 2, ( jsonToMap( response ) ).size() );
			  assertTrue( response.Contains( "know" ) );
			  assertTrue( response.Contains( "data" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Create a node with a label and a property using Cypher. See the request for the parameter " + "sent with the query.") @Title("Create a node") @Graph public void send_query_to_create_a_node()
		 [Documented("Create a node with a label and a property using Cypher. See the request for the parameter " + "sent with the query."), Title("Create a node")]
		 public virtual void SendQueryToCreateANode()
		 {
			  Data.get();
			  string script = "CREATE (n:Person { name : {name} }) RETURN n";
			  string response = cypherRestCall( script, Status.OK, Pair.of( "name", "Andres" ) );

			  assertTrue( response.Contains( "name" ) );
			  assertTrue( response.Contains( "Andres" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Create a node with multiple properties") @Documented("Create a node with a label and multiple properties using Cypher. See the request for the parameter " + "sent with the query.") @Graph public void send_query_to_create_a_node_from_a_map()
		 [Title("Create a node with multiple properties"), Documented("Create a node with a label and multiple properties using Cypher. See the request for the parameter " + "sent with the query.")]
		 public virtual void SendQueryToCreateANodeFromAMap()
		 {
			  Data.get();
			  string script = "CREATE (n:Person { props } ) RETURN n";
			  string @params = "\"props\" : { \"position\" : \"Developer\", \"name\" : \"Michael\", \"awesome\" : true, \"children\" : 3 }";
			  string response = cypherRestCall( script, Status.OK, @params );

			  assertTrue( response.Contains( "name" ) );
			  assertTrue( response.Contains( "Michael" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Create multiple nodes with properties using Cypher. See the request for the parameter sent " + "with the query.") @Title("Create multiple nodes with properties") @Graph public void send_query_to_create_multiple_nodes_from_a_map()
		 [Documented("Create multiple nodes with properties using Cypher. See the request for the parameter sent " + "with the query."), Title("Create multiple nodes with properties")]
		 public virtual void SendQueryToCreateMultipleNodesFromAMap()
		 {
			  Data.get();
			  string script = "UNWIND {props} AS properties CREATE (n:Person) SET n = properties RETURN n";
			  string @params = "\"props\" : [ { \"name\" : \"Andres\", \"position\" : \"Developer\" }, " +
						 "{ \"name\" : \"Michael\", \"position\" : \"Developer\" } ]";
			  string response = cypherRestCall( script, Status.OK, @params );

			  assertTrue( response.Contains( "name" ) );
			  assertTrue( response.Contains( "Michael" ) );
			  assertTrue( response.Contains( "Andres" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Set all properties on a node using Cypher") @Documented("Set all properties on a node.") @Graph public void setAllPropertiesUsingMap()
		 [Title("Set all properties on a node using Cypher"), Documented("Set all properties on a node.")]
		 public virtual void SetAllPropertiesUsingMap()
		 {
			  Data.get();
			  string script = "CREATE (n:Person { name: 'this property is to be deleted' } ) SET n = { props } RETURN n";
			  string @params = "\"props\" : { \"position\" : \"Developer\", \"firstName\" : \"Michael\", \"awesome\" : true, \"children\" : 3 }";
			  string response = cypherRestCall( script, Status.OK, @params );

			  assertTrue( response.Contains( "firstName" ) );
			  assertTrue( response.Contains( "Michael" ) );
			  assertTrue( !response.Contains( "name" ) );
			  assertTrue( !response.Contains( "deleted" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", properties = { @PROP(key = "prop", value = "Hello", type = Neo4Net.test.GraphDescription.PropType.STRING)}), @NODE(name = "you")}, relationships = {@REL(start = "I", end = "him", type = "know", properties = { @PROP(key = "prop", value = "World", type = Neo4Net.test.GraphDescription.PropType.STRING)})}) public void nodes_are_represented_as_nodes()
		 [Graph(nodes : {@NODE(name : "I", properties : { @PROP(key : "prop", value : "Hello", type : Neo4Net.Test.GraphDescription.PropType.STRING)}), @NODE(name : "you")}, relationships : {@REL(start : "I", end : "him", type : "know", properties : { @PROP(key : "prop", value : "World", type : Neo4Net.Test.GraphDescription.PropType.STRING)})})]
		 public virtual void NodesAreRepresentedAsNodes()
		 {
			  Data.get();
			  string script = "MATCH (n)-[r]->() WHERE id(n) = %I% RETURN n, r";

			  string response = cypherRestCall( script, Status.OK );

			  assertThat( response, containsString( "Hello" ) );
			  assertThat( response, containsString( "World" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Syntax errors") @Documented("Sending a query with syntax errors will give a bad request (HTTP 400) response together with " + "an error message.") @Graph(value = {"I know you"}, autoIndexNodes = true) public void send_queries_with_syntax_errors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Syntax errors"), Documented("Sending a query with syntax errors will give a bad request (HTTP 400) response together with " + "an error message."), Graph(value : {"I know you"}, autoIndexNodes : true)]
		 public virtual void SendQueriesWithSyntaxErrors()
		 {
			  Data.get();
			  string script = "START x  = node:node_auto_index(name={startName}) MATC path = (x-[r]-friend) WHERE friend" +
						 ".name = {name} RETURN TYPE(r)";
			  string response = cypherRestCall( script, Status.BAD_REQUEST, Pair.of( "startName", "I" ), Pair.of( "name", "you" ) );

			  IDictionary<string, object> output = jsonToMap( response );
			  assertTrue( output.ContainsKey( "message" ) );
			  assertTrue( output.ContainsKey( "stackTrace" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("When sending queries that\n" + "return nested results like list and maps,\n" + "these will get serialized into nested JSON representations\n" + "according to their types.") @Graph(value = {"I know you"}, autoIndexNodes = true) public void nested_results() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("When sending queries that\n" + "return nested results like list and maps,\n" + "these will get serialized into nested JSON representations\n" + "according to their types."), Graph(value : {"I know you"}, autoIndexNodes : true)]
		 public virtual void NestedResults()
		 {
			  Data.get();
			  string script = "MATCH (n) WHERE n.name in ['I', 'you'] RETURN collect(n.name)";
			  string response = cypherRestCall( script, Status.OK );

			  IDictionary<string, object> resultMap = jsonToMap( response );
			  assertEquals( 2, resultMap.Count );
			  assertThat( response, anyOf( containsString( "\"I\",\"you\"" ), containsString( "\"you\",\"I\"" ), containsString( "\"I\", \"you\"" ), containsString( "\"you\", \"I\"" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Profile a query") @Documented("By passing in an extra parameter, you can ask the cypher executor to return a profile of the " + "query as it is executed. This can help in locating bottlenecks.") @Graph(nodes = {@NODE(name = "I", setNameProperty = true), @NODE(name = "you", setNameProperty = true), @NODE(name = "him", setNameProperty = true, properties = { @PROP(key = "age", value = "25", type = Neo4Net.test.GraphDescription.PropType.INTEGER)})}, relationships = { @REL(start = "I", end = "him", type = "know", properties = {}), @REL(start = "I", end = "you", type = "know", properties = {})}) public void testProfiling() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Profile a query"), Documented("By passing in an extra parameter, you can ask the cypher executor to return a profile of the " + "query as it is executed. This can help in locating bottlenecks."), Graph(nodes : {@NODE(name : "I", setNameProperty : true), @NODE(name : "you", setNameProperty : true), @NODE(name : "him", setNameProperty : true, properties : { @PROP(key : "age", value : "25", type : Neo4Net.Test.GraphDescription.PropType.INTEGER)})}, relationships : { @REL(start : "I", end : "him", type : "know", properties : {}), @REL(start : "I", end : "you", type : "know", properties : {})})]
		 public virtual void TestProfiling()
		 {
			  string script = CreateScript( "MATCH (x)-[r]->(n) WHERE id(x) = %I% RETURN type(r), n.name, n.age" );

			  // WHEN
			  string response = doCypherRestCall( CypherUri() + "?profile=true", script, Status.OK );

			  // THEN
			  IDictionary<string, object> des = jsonToMap( response );
			  assertThat( des["plan"], instanceOf( typeof( System.Collections.IDictionary ) ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String,Object> plan = (java.util.Map<String,Object>) des.get("plan");
			  IDictionary<string, object> plan = ( IDictionary<string, object> ) des["plan"];
			  assertThat( plan["name"], instanceOf( typeof( string ) ) );
			  assertThat( plan["children"], instanceOf( typeof( System.Collections.ICollection ) ) );
			  assertThat( plan["rows"], instanceOf( typeof( Number ) ) );
			  assertThat( plan["dbHits"], instanceOf( typeof( Number ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(value = {"I know you"}, autoIndexNodes = false) public void array_property()
		 [Graph(value : {"I know you"}, autoIndexNodes : false)]
		 public virtual void ArrayProperty()
		 {
			  SetProperty( "I", "array1", new int[]{ 1, 2, 3 } );
			  SetProperty( "I", "array2", new string[]{ "a", "b", "c" } );

			  string script = "MATCH (n) WHERE id(n) = %I% RETURN n.array1, n.array2";
			  string response = cypherRestCall( script, Status.OK );

			  assertThat( response, anyOf( containsString( "[ 1, 2, 3 ]" ), containsString( "[1,2,3]" ) ) );
			  assertThat( response, anyOf( containsString( "[ \"a\", \"b\", \"c\" ]" ), containsString( "[\"a\",\"b\",\"c\"]" ) ) );
		 }

		 internal virtual void SetProperty( string nodeName, string propertyName, object propertyValue )
		 {
			  Node i = this.GetNode( nodeName );
			  IGraphDatabaseService db = i.GraphDatabase;

			  using ( Transaction tx = Db.beginTx() )
			  {
					i.SetProperty( propertyName, propertyValue );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Send queries with errors") @Documented("This example shows what happens if you misspell an identifier.") @Graph(value = {"I know you"}, autoIndexNodes = true) public void send_queries_with_errors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Send queries with errors"), Documented("This example shows what happens if you misspell an identifier."), Graph(value : {"I know you"}, autoIndexNodes : true)]
		 public virtual void SendQueriesWithErrors()
		 {
			  Data.get();
			  string script = "START x = node:node_auto_index(name={startName}) MATCH path = (x)-[r]-(friend) WHERE frien" +
						 ".name = {name} RETURN type(r)";
			  string response = cypherRestCall( script, Status.BAD_REQUEST, Pair.of( "startName", "I" ), Pair.of( "name", "you" ) );

			  IDictionary<string, object> responseMap = jsonToMap( response );
			  assertThat( responseMap.Keys, containsInAnyOrder( "message", "exception", "fullname", "stackTrace", "cause", "errors" ) );
			  assertThat( response, containsString( "message" ) );
			  assertThat( ( string ) responseMap["message"], containsString( "Variable `frien` not defined" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final String cypherRestCall(String script, javax.ws.rs.core.Response.Status status, Neo4Net.helpers.collection.Pair<String,String>... params)
		 private string CypherRestCall( string script, Status status, params Pair<string, string>[] @params )
		 {
			  return DoCypherRestCall( CypherUri(), script, status, @params );
		 }

		 private string CypherRestCall( string script, Status status, string paramString )
		 {
			  return DoCypherRestCall( CypherUri(), script, status, paramString );
		 }

		 private string CypherUri()
		 {
			  return DataUri + "cypher";
		 }
	}

}