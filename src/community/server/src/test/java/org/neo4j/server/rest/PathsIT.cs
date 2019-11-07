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


	using Node = Neo4Net.GraphDb.Node;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using GraphDescription = Neo4Net.Test.GraphDescription;
	using Graph = Neo4Net.Test.GraphDescription.Graph;
	using NODE = Neo4Net.Test.GraphDescription.NODE;
	using PROP = Neo4Net.Test.GraphDescription.PROP;
	using REL = Neo4Net.Test.GraphDescription.REL;
	using Title = Neo4Net.Test.TestData.Title;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class PathsIT : AbstractRestFunctionalTestBase
	{
	//     Layout
	//
	//     (e)----------------
	//      |                 |
	//     (d)-------------   |
	//      |               \/
	//     (a)-(c)-(b)-(f)-(g)
	//          |\     /   /
	//          | ----    /
	//           --------
	//
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(value = { "a to c", "a to d", "c to b", "d to e", "b to f", "c to f", "f to g", "d to g", "e to g", "c to g" }) @Title("Find all shortest paths") @Documented("The +shortestPath+ algorithm can find multiple paths between the same nodes, like in this example.") public void shouldBeAbleToFindAllShortestPaths() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph(value : { "a to c", "a to d", "c to b", "d to e", "b to f", "c to f", "f to g", "d to g", "e to g", "c to g" }), Title("Find all shortest paths"), Documented("The +shortestPath+ algorithm can find multiple paths between the same nodes, like in this example.")]
		 public virtual void ShouldBeAbleToFindAllShortestPaths()
		 {

			  // Get all shortest paths
			  long a = NodeId( Data.get(), "a" );
			  long g = NodeId( Data.get(), "g" );
			  string response = Gen().expectedStatus(Status.OK.StatusCode).payload(GetAllShortestPathPayLoad(g)).post(ServerUri + "db/data/node/" + a + "/paths").entity();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> result = (java.util.Collection<?>) Neo4Net.server.rest.domain.JsonHelper.readJson(response);
			  ICollection<object> result = ( ICollection<object> ) JsonHelper.readJson( response );
			  assertEquals( 2, result.Count );
			  foreach ( object representation in result )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> path = (java.util.Map<?, ?>) representation;
					IDictionary<object, ?> path = ( IDictionary<object, ?> ) representation;

					AssertThatPathStartsWith( path, a );
					AssertThatPathEndsWith( path, g );
					AssertThatPathHasLength( path, 2 );
			  }
		 }

	//      Layout
	//
	//      (e)----------------
	//       |                 |
	//      (d)-------------   |
	//       |               \/
	//      (a)-(c)-(b)-(f)-(g)
	//           |\     /   /
	//           | ----    /
	//            --------
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Title("Find one of the shortest paths") @Test @Graph(value = { "a to c", "a to d", "c to b", "d to e", "b to f", "c to f", "f to g", "d to g", "e to g", "c to g" }) @Documented("If no path algorithm is specified, a +shortestPath+ algorithm with a max\n" + "depth of 1 will be chosen. In this example, the +max_depth+ is set to +3+\n" + "in order to find the shortest path between a maximum of 3 linked nodes.") public void shouldBeAbleToFetchSingleShortestPath() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Find one of the shortest paths"), Graph(value : { "a to c", "a to d", "c to b", "d to e", "b to f", "c to f", "f to g", "d to g", "e to g", "c to g" }), Documented("If no path algorithm is specified, a +shortestPath+ algorithm with a max\n" + "depth of 1 will be chosen. In this example, the +max_depth+ is set to +3+\n" + "in order to find the shortest path between a maximum of 3 linked nodes.")]
		 public virtual void ShouldBeAbleToFetchSingleShortestPath()
		 {
			  long a = NodeId( Data.get(), "a" );
			  long g = NodeId( Data.get(), "g" );
			  string response = Gen().expectedStatus(Status.OK.StatusCode).payload(GetAllShortestPathPayLoad(g)).post(ServerUri + "db/data/node/" + a + "/path").entity();
			  // Get single shortest path

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> path = Neo4Net.server.rest.domain.JsonHelper.jsonToMap(response);
			  IDictionary<object, ?> path = JsonHelper.jsonToMap( response );

			  AssertThatPathStartsWith( path, a );
			  AssertThatPathEndsWith( path, g );
			  AssertThatPathHasLength( path, 2 );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void assertThatPathStartsWith(final java.util.Map<?, ?> path, final long start)
		 private void AssertThatPathStartsWith<T1>( IDictionary<T1> path, long start )
		 {
			  assertTrue( "Path should start with " + start + "\nBut it was " + path, path["start"].ToString().EndsWith("/node/" + start, StringComparison.Ordinal) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void assertThatPathEndsWith(final java.util.Map<?, ?> path, final long start)
		 private void AssertThatPathEndsWith<T1>( IDictionary<T1> path, long start )
		 {
			  assertTrue( "Path should end with " + start + "\nBut it was " + path, path["end"].ToString().EndsWith("/node/" + start, StringComparison.Ordinal) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void assertThatPathHasLength(final java.util.Map<?, ?> path, final int length)
		 private void AssertThatPathHasLength<T1>( IDictionary<T1> path, int length )
		 {
			  object actual = path["length"];

			  assertEquals( "Expected path to have a length of " + length + "\nBut it was " + actual, length, actual );
		 }

	//      Layout
	//
	//         1.5------(b)--------0.5
	//        /                      \
	//      (a)-0.5-(c)-0.5-(d)-0.5-(e)
	//        \                     /
	//        0.5-------(f)------1.2
	//
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = { @NODE(name = "a", setNameProperty = true), @NODE(name = "b", setNameProperty = true), @NODE(name = "c", setNameProperty = true), @NODE(name = "d", setNameProperty = true), @NODE(name = "e", setNameProperty = true), @NODE(name = "f", setNameProperty = true) }, relationships = { @REL(start = "a", end = "b", type = "to", properties = { @PROP(key = "cost", value = "1.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "a", end = "c", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "a", end = "f", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "c", end = "d", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "d", end = "e", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "b", end = "e", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "f", end = "e", type = "to", properties = { @PROP(key = "cost", value = "1.2", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }) }) @Title("Execute a Dijkstra algorithm and get a single path") @Documented("This example is running a Dijkstra algorithm over a graph with different\n" + "cost properties on different relationships. Note that the request URI\n" + "ends with +/path+ which means a single path is what we want here.") public void shouldGetCorrectDijkstraPathWithWeights() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph(nodes : { @NODE(name : "a", setNameProperty : true), @NODE(name : "b", setNameProperty : true), @NODE(name : "c", setNameProperty : true), @NODE(name : "d", setNameProperty : true), @NODE(name : "e", setNameProperty : true), @NODE(name : "f", setNameProperty : true) }, relationships : { @REL(start : "a", end : "b", type : "to", properties : { @PROP(key : "cost", value : "1.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "a", end : "c", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "a", end : "f", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "c", end : "d", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "d", end : "e", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "b", end : "e", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "f", end : "e", type : "to", properties : { @PROP(key : "cost", value : "1.2", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }) }), Title("Execute a Dijkstra algorithm and get a single path"), Documented("This example is running a Dijkstra algorithm over a graph with different\n" + "cost properties on different relationships. Note that the request URI\n" + "ends with +/path+ which means a single path is what we want here.")]
		 public virtual void ShouldGetCorrectDijkstraPathWithWeights()
		 {
			  // Get cheapest paths using Dijkstra
			  long a = NodeId( Data.get(), "a" );
			  long e = NodeId( Data.get(), "e" );
			  string response = Gen().expectedStatus(Status.OK.StatusCode).payload(GetAllPathsUsingDijkstraPayLoad(e, false)).post(ServerUri + "db/data/node/" + a + "/path").entity();
			  //
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> path = Neo4Net.server.rest.domain.JsonHelper.jsonToMap(response);
			  IDictionary<object, ?> path = JsonHelper.jsonToMap( response );
			  AssertThatPathStartsWith( path, a );
			  AssertThatPathEndsWith( path, e );
			  AssertThatPathHasLength( path, 3 );
			  assertEquals( 1.5, path["weight"] );
		 }

	//      Layout
	//
	//         1.5------(b)--------0.5
	//        /                      \
	//      (a)-0.5-(c)-0.5-(d)-0.5-(e)
	//        \                     /
	//        0.5-------(f)------1.0
	//
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = { @NODE(name = "a", setNameProperty = true), @NODE(name = "b", setNameProperty = true), @NODE(name = "c", setNameProperty = true), @NODE(name = "d", setNameProperty = true), @NODE(name = "e", setNameProperty = true), @NODE(name = "f", setNameProperty = true) }, relationships = { @REL(start = "a", end = "b", type = "to", properties = { @PROP(key = "cost", value = "1.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "a", end = "c", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "a", end = "f", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "c", end = "d", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "d", end = "e", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "b", end = "e", type = "to", properties = { @PROP(key = "cost", value = "0.5", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }), @REL(start = "f", end = "e", type = "to", properties = { @PROP(key = "cost", value = "1.0", type = Neo4Net.test.GraphDescription.PropType.DOUBLE) }) }) @Title("Execute a Dijkstra algorithm and get multiple paths") @Documented("This example is running a Dijkstra algorithm over a graph with different\n" + "cost properties on different relationships. Note that the request URI\n" + "ends with +/paths+ which means we want multiple paths returned, in case\n" + "they exist.") public void shouldGetCorrectDijkstraPathsWithWeights() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph(nodes : { @NODE(name : "a", setNameProperty : true), @NODE(name : "b", setNameProperty : true), @NODE(name : "c", setNameProperty : true), @NODE(name : "d", setNameProperty : true), @NODE(name : "e", setNameProperty : true), @NODE(name : "f", setNameProperty : true) }, relationships : { @REL(start : "a", end : "b", type : "to", properties : { @PROP(key : "cost", value : "1.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "a", end : "c", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "a", end : "f", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "c", end : "d", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "d", end : "e", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "b", end : "e", type : "to", properties : { @PROP(key : "cost", value : "0.5", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }), @REL(start : "f", end : "e", type : "to", properties : { @PROP(key : "cost", value : "1.0", type : Neo4Net.Test.GraphDescription.PropType.DOUBLE) }) }), Title("Execute a Dijkstra algorithm and get multiple paths"), Documented("This example is running a Dijkstra algorithm over a graph with different\n" + "cost properties on different relationships. Note that the request URI\n" + "ends with +/paths+ which means we want multiple paths returned, in case\n" + "they exist.")]
		 public virtual void ShouldGetCorrectDijkstraPathsWithWeights()
		 {
			  // Get cheapest paths using Dijkstra
			  long a = NodeId( Data.get(), "a" );
			  long e = NodeId( Data.get(), "e" );
			  string response = Gen().expectedStatus(Status.OK.StatusCode).payload(GetAllPathsUsingDijkstraPayLoad(e, false)).post(ServerUri + "db/data/node/" + a + "/paths").entity();
			  //
			  IList<IDictionary<string, object>> list = JsonHelper.jsonToList( response );
			  assertEquals( 2, list.Count );
			  IDictionary<string, object> firstPath = list[0];
			  IDictionary<string, object> secondPath = list[1];
			  Console.WriteLine( firstPath );
			  Console.WriteLine( secondPath );
			  AssertThatPathStartsWith( firstPath, a );
			  AssertThatPathStartsWith( secondPath, a );
			  AssertThatPathEndsWith( firstPath, e );
			  AssertThatPathEndsWith( secondPath, e );
			  assertEquals( 1.5, firstPath["weight"] );
			  assertEquals( 1.5, secondPath["weight"] );
			  // 5 = 3 + 2
			  assertEquals( 5, ( int? ) firstPath["length"] + ( int? ) secondPath["length"] );
			  assertEquals( 1, Math.Abs( ( int? ) firstPath["length"] - ( int? ) secondPath["length"] ) );
		 }

	//      Layout
	//
	//         1------(b)-----1
	//        /                \
	//      (a)-1-(c)-1-(d)-1-(e)
	//        \                /
	//         1------(f)-----1
	//
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = { @NODE(name = "a", setNameProperty = true), @NODE(name = "b", setNameProperty = true), @NODE(name = "c", setNameProperty = true), @NODE(name = "d", setNameProperty = true), @NODE(name = "e", setNameProperty = true), @NODE(name = "f", setNameProperty = true) }, relationships = { @REL(start = "a", end = "b", type = "to", properties = { @PROP(key = "cost", value = "1", type = Neo4Net.test.GraphDescription.PropType.INTEGER) }), @REL(start = "a", end = "c", type = "to", properties = { @PROP(key = "cost", value = "1", type = Neo4Net.test.GraphDescription.PropType.INTEGER) }), @REL(start = "a", end = "f", type = "to", properties = { @PROP(key = "cost", value = "1", type = Neo4Net.test.GraphDescription.PropType.INTEGER) }), @REL(start = "c", end = "d", type = "to", properties = { @PROP(key = "cost", value = "1", type = Neo4Net.test.GraphDescription.PropType.INTEGER) }), @REL(start = "d", end = "e", type = "to", properties = { @PROP(key = "cost", value = "1", type = Neo4Net.test.GraphDescription.PropType.INTEGER) }), @REL(start = "b", end = "e", type = "to", properties = { @PROP(key = "cost", value = "1", type = Neo4Net.test.GraphDescription.PropType.INTEGER) }), @REL(start = "f", end = "e", type = "to", properties = { @PROP(key = "cost", value = "1", type = Neo4Net.test.GraphDescription.PropType.INTEGER) }) }) @Title("Execute a Dijkstra algorithm with equal weights on relationships") @Documented("The following is executing a Dijkstra search on a graph with equal\n" + "weights on all relationships. This example is included to show the\n" + "difference when the same graph structure is used, but the path weight is\n" + "equal to the number of hops.") public void shouldGetCorrectDijkstraPathsWithEqualWeightsWithDefaultCost() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Graph(nodes : { @NODE(name : "a", setNameProperty : true), @NODE(name : "b", setNameProperty : true), @NODE(name : "c", setNameProperty : true), @NODE(name : "d", setNameProperty : true), @NODE(name : "e", setNameProperty : true), @NODE(name : "f", setNameProperty : true) }, relationships : { @REL(start : "a", end : "b", type : "to", properties : { @PROP(key : "cost", value : "1", type : Neo4Net.Test.GraphDescription.PropType.INTEGER) }), @REL(start : "a", end : "c", type : "to", properties : { @PROP(key : "cost", value : "1", type : Neo4Net.Test.GraphDescription.PropType.INTEGER) }), @REL(start : "a", end : "f", type : "to", properties : { @PROP(key : "cost", value : "1", type : Neo4Net.Test.GraphDescription.PropType.INTEGER) }), @REL(start : "c", end : "d", type : "to", properties : { @PROP(key : "cost", value : "1", type : Neo4Net.Test.GraphDescription.PropType.INTEGER) }), @REL(start : "d", end : "e", type : "to", properties : { @PROP(key : "cost", value : "1", type : Neo4Net.Test.GraphDescription.PropType.INTEGER) }), @REL(start : "b", end : "e", type : "to", properties : { @PROP(key : "cost", value : "1", type : Neo4Net.Test.GraphDescription.PropType.INTEGER) }), @REL(start : "f", end : "e", type : "to", properties : { @PROP(key : "cost", value : "1", type : Neo4Net.Test.GraphDescription.PropType.INTEGER) }) }), Title("Execute a Dijkstra algorithm with equal weights on relationships"), Documented("The following is executing a Dijkstra search on a graph with equal\n" + "weights on all relationships. This example is included to show the\n" + "difference when the same graph structure is used, but the path weight is\n" + "equal to the number of hops.")]
		 public virtual void ShouldGetCorrectDijkstraPathsWithEqualWeightsWithDefaultCost()
		 {
			  // Get cheapest path using Dijkstra
			  long a = NodeId( Data.get(), "a" );
			  long e = NodeId( Data.get(), "e" );
			  string response = Gen().expectedStatus(Status.OK.StatusCode).payload(GetAllPathsUsingDijkstraPayLoad(e, false)).post(ServerUri + "db/data/node/" + a + "/path").entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?, ?> path = Neo4Net.server.rest.domain.JsonHelper.jsonToMap(response);
			  IDictionary<object, ?> path = JsonHelper.jsonToMap( response );
			  AssertThatPathStartsWith( path, a );
			  AssertThatPathEndsWith( path, e );
			  AssertThatPathHasLength( path, 2 );
			  assertEquals( 2.0, path["weight"] );
		 }

	//      Layout
	//
	//      (e)----------------
	//       |                 |
	//      (d)-------------   |
	//       |               \/
	//      (a)-(c)-(b)-(f)-(g)
	//           |\     /   /
	//           | ----    /
	//            --------
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(value = { "a to c", "a to d", "c to b", "d to e", "b to f", "c to f", "f to g", "d to g", "e to g", "c to g" }) public void shouldReturn404WhenFailingToFindASinglePath()
		 [Graph(value : { "a to c", "a to d", "c to b", "d to e", "b to f", "c to f", "f to g", "d to g", "e to g", "c to g" })]
		 public virtual void ShouldReturn404WhenFailingToFindASinglePath()
		 {
			  long a = NodeId( Data.get(), "a" );
			  long g = NodeId( Data.get(), "g" );
			  string noHitsJson = "{\"to\":\""
					+ NodeUri( g ) + "\", \"max_depth\":1, \"relationships\":{\"type\":\"dummy\", \"direction\":\"in\"}, \"algorithm\":\"shortestPath\"}";
			  string IEntity = Gen().expectedStatus(Status.NOT_FOUND.StatusCode).payload(noHitsJson).post(ServerUri + "db/data/node/" + a + "/path").entity();
			  Console.WriteLine( IEntity );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private long nodeId(final java.util.Map<String, Neo4Net.graphdb.Node> map, final String string)
		 private long NodeId( IDictionary<string, Node> map, string @string )
		 {
			  return map[@string].Id;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String nodeUri(final long l)
		 private string NodeUri( long l )
		 {
			  return ServerUri + "db/data/node/" + l;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getAllShortestPathPayLoad(final long to)
		 private string GetAllShortestPathPayLoad( long to )
		 {
			  string json = "{\"to\":\""
					+ NodeUri( to ) + "\", \"max_depth\":3, \"relationships\":{\"type\":\"to\", \"direction\":\"out\"}, \"algorithm\":\"shortestPath\"}";
			  return json;
		 }

		 //
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getAllPathsUsingDijkstraPayLoad(final long to, final boolean includeDefaultCost)
		 private string GetAllPathsUsingDijkstraPayLoad( long to, bool includeDefaultCost )
		 {
			  string json = "{\"to\":\"" + NodeUri( to ) + "\"" + ", \"cost_property\":\"cost\""
			  + ( includeDefaultCost ? ", \"default_cost\":1" : "" ) + ", \"relationships\":{\"type\":\"to\", \"direction\":\"out\"}, \"algorithm\":\"dijkstra\"}";
			  return json;
		 }

		 private string ServerUri
		 {
			 get
			 {
				  return Server().baseUri().ToString();
			 }
		 }

	}

}