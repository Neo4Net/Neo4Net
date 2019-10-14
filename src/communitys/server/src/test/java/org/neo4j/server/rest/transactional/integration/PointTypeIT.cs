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
namespace Neo4Net.Server.rest.transactional.integration
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using After = org.junit.After;
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using CRS = Neo4Net.Graphdb.spatial.CRS;
	using Point = Neo4Net.Graphdb.spatial.Point;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using GeometryType = Neo4Net.Kernel.impl.store.GeometryType;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using HTTP = Neo4Net.Test.server.HTTP;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;

	public class PointTypeIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  // empty the database
			  Graphdb().execute("MATCH (n) DETACH DELETE n");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithPoint2DArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithPoint2DArrays()
		 {
			  HTTP.Response response = RunQuery( "create (:Node {points: [point({x:1, y:1}), point({x:2, y:2}), point({x: 3.0, y: 3.0})]})" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  GraphDatabaseFacade db = Server().Database.Graph;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 if ( node.HasLabel( label( "Node" ) ) && node.HasProperty( "points" ) )
						 {
							  Point[] points = ( Point[] ) node.GetProperty( "points" );

							  VerifyPoint( points[0], Cartesian, 1.0, 1.0 );
							  VerifyPoint( points[1], Cartesian, 2.0, 2.0 );
							  VerifyPoint( points[2], Cartesian, 3.0, 3.0 );
						 }
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnPoint2DWithXAndY() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnPoint2DWithXAndY()
		 {
			  TestPoint( "RETURN point({x: 42.05, y: 90.99})", new double[]{ 42.05, 90.99 }, Cartesian, "point" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnPoint2DWithLatitudeAndLongitude() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnPoint2DWithLatitudeAndLongitude()
		 {
			  TestPoint( "RETURN point({longitude: 56.7, latitude: 12.78})", new double[]{ 56.7, 12.78 }, WGS84, "point" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandlePointArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandlePointArrays()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "coordinates", new Point[]{ pointValue( WGS84, 30.655691, 104.081602 ) } );
					node.SetProperty( "location", "Shanghai" );
					node.SetProperty( "type", "gps" );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  //Then
			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "row" ).get( 0 ).get( "coordinates" ).get( 0 );
			  AssertGeometryTypeEqual( GeometryType.GEOMETRY_POINT, row );
			  AssertCoordinatesEqual( new double[]{ 30.655691, 104.081602 }, row );
			  AssertCrsEqual( WGS84, row );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandlePointsUsingRestResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandlePointsUsingRestResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "coordinates", pointValue( WGS84, 30.655691, 104.081602 ) );
					node.SetProperty( "location", "Shanghai" );
					node.SetProperty( "type", "gps" );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "rest" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  //Then
			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "rest" ).get( 0 ).get( "data" ).get( "coordinates" );
			  AssertGeometryTypeEqual( GeometryType.GEOMETRY_POINT, row );
			  AssertCoordinatesEqual( new double[]{ 30.655691, 104.081602 }, row );
			  AssertCrsEqual( WGS84, row );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandlePointsUsingGraphResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandlePointsUsingGraphResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "coordinates", pointValue( WGS84, 30.655691, 104.081602 ) );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "graph" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  //Then
			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "graph" ).get( "nodes" ).get( 0 ).get( "properties" ).get( "coordinates" );
			  AssertGeometryTypeEqual( GeometryType.GEOMETRY_POINT, row );
			  AssertCoordinatesEqual( new double[]{ 30.655691, 104.081602 }, row );
			  AssertCrsEqual( WGS84, row );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleArrayOfPointsUsingRestResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleArrayOfPointsUsingRestResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "coordinates", new Point[]{ pointValue( WGS84, 30.655691, 104.081602 ) } );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "rest" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  //Then
			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "rest" ).get( 0 ).get( "data" ).get( "coordinates" ).get( 0 );
			  AssertGeometryTypeEqual( GeometryType.GEOMETRY_POINT, row );
			  AssertCoordinatesEqual( new double[]{ 30.655691, 104.081602 }, row );
			  AssertCrsEqual( WGS84, row );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleArrayOfPointsUsingGraphResultDataContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleArrayOfPointsUsingGraphResultDataContent()
		 {
			  //Given
			  GraphDatabaseFacade db = Server().Database.Graph;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label( "N" ) );
					node.SetProperty( "coordinates", new Point[]{ pointValue( WGS84, 30.655691, 104.081602 ) } );
					tx.Success();
			  }

			  //When
			  HTTP.Response response = RunQuery( "MATCH (n:N) RETURN n", "graph" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  //Then
			  JsonNode row = response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( "graph" ).get( "nodes" ).get( 0 ).get( "properties" ).get( "coordinates" ).get( 0 );
			  AssertGeometryTypeEqual( GeometryType.GEOMETRY_POINT, row );
			  AssertCoordinatesEqual( new double[]{ 30.655691, 104.081602 }, row );
			  AssertCrsEqual( WGS84, row );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testPoint(String query, double[] expectedCoordinate, org.neo4j.values.storable.CoordinateReferenceSystem expectedCrs, String expectedType) throws Exception
		 private static void TestPoint( string query, double[] expectedCoordinate, CoordinateReferenceSystem expectedCrs, string expectedType )
		 {
			  HTTP.Response response = RunQuery( query );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );

			  JsonNode element = ExtractSingleElement( response );
			  AssertGeometryTypeEqual( GeometryType.GEOMETRY_POINT, element );
			  AssertCoordinatesEqual( expectedCoordinate, element );
			  AssertCrsEqual( expectedCrs, element );

			  AssertTypeEqual( expectedType, response );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertTypeEqual(String expectedType, org.neo4j.test.server.HTTP.Response response) throws org.neo4j.server.rest.domain.JsonParseException
		 private static void AssertTypeEqual( string expectedType, HTTP.Response response )
		 {
			  JsonNode data = response.Get( "results" ).get( 0 ).get( "data" );
			  JsonNode meta = data.get( 0 ).get( "meta" );
			  assertEquals( 1, meta.size() );
			  assertEquals( expectedType, meta.get( 0 ).get( "type" ).asText() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.codehaus.jackson.JsonNode extractSingleElement(org.neo4j.test.server.HTTP.Response response) throws org.neo4j.server.rest.domain.JsonParseException
		 private static JsonNode ExtractSingleElement( HTTP.Response response )
		 {
			  JsonNode data = response.Get( "results" ).get( 0 ).get( "data" );
			  assertEquals( 1, data.size() );
			  JsonNode row = data.get( 0 ).get( "row" );
			  assertEquals( 1, row.size() );
			  return row.get( 0 );
		 }

		 private static void AssertGeometryTypeEqual( GeometryType expected, JsonNode element )
		 {
			  assertEquals( expected.Name, element.get( "type" ).asText() );
		 }

		 private static void AssertCoordinatesEqual( double[] expected, JsonNode element )
		 {
			  assertArrayEquals( expected, CoordinatesAsArray( element ), 0.000001 );
		 }

		 private static double[] CoordinatesAsArray( JsonNode element )
		 {
			  return Iterables.stream( element.get( "coordinates" ) ).mapToDouble( JsonNode.asDouble ).toArray();
		 }

		 private static void AssertCrsEqual( CoordinateReferenceSystem crs, JsonNode element )
		 {
			  assertEquals( crs.Name, element.get( "crs" ).get( "name" ).asText() );
		 }

		 private static void VerifyPoint( Point point, CRS expectedCRS, params Double[] expectedCoordinate )
		 {
			  assertEquals( expectedCRS.Code, point.CRS.Code );
			  assertEquals( asList( expectedCoordinate ), point.Coordinate.Coordinate );
		 }
	}

}