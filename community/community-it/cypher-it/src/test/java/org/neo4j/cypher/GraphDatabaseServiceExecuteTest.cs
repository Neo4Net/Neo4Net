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
namespace Org.Neo4j.Cypher
{
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Result = Org.Neo4j.Graphdb.Result;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using CRS = Org.Neo4j.Graphdb.spatial.CRS;
	using Coordinate = Org.Neo4j.Graphdb.spatial.Coordinate;
	using Geometry = Org.Neo4j.Graphdb.spatial.Geometry;
	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using Name = Org.Neo4j.Procedure.Name;
	using Procedure = Org.Neo4j.Procedure.Procedure;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class GraphDatabaseServiceExecuteTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule graphDb = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule GraphDb = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteCypher()
		 public virtual void ShouldExecuteCypher()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long before;
			  long before;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long after;
			  long after;
			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					before = Iterables.count( GraphDb.AllNodes );
					tx.Success();
			  }

			  // when
			  GraphDb.execute( "CREATE (n:Foo{bar:\"baz\"})" );

			  // then
			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					after = Iterables.count( GraphDb.AllNodes );
					tx.Success();
			  }
			  assertEquals( before + 1, after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnInternalGeographicPointType()
		 public virtual void ShouldNotReturnInternalGeographicPointType()
		 {
			  // when
			  Result execute = GraphDb.execute( "RETURN point({longitude: 144.317718, latitude: -37.031738}) AS p" );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  object obj = execute.Next()["p"];
			  assertThat( obj, Matchers.instanceOf( typeof( Point ) ) );

			  Point point = ( Point ) obj;
			  assertThat( point.Coordinate.Coordinate[0], equalTo( 144.317718 ) );
			  assertThat( point.Coordinate.Coordinate[1], equalTo( -37.031738 ) );

			  CRS crs = point.CRS;
			  assertThat( crs.Code, equalTo( 4326 ) );
			  assertThat( crs.Type, equalTo( "wgs-84" ) );
			  assertThat( crs.Href, equalTo( "http://spatialreference.org/ref/epsg/4326/" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnInternalCartesianPointType()
		 public virtual void ShouldNotReturnInternalCartesianPointType()
		 {
			  // when
			  Result execute = GraphDb.execute( "RETURN point({x: 13.37, y: 13.37, crs:'cartesian'}) AS p" );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  object obj = execute.Next()["p"];
			  assertThat( obj, Matchers.instanceOf( typeof( Point ) ) );

			  Point point = ( Point ) obj;
			  assertThat( point.Coordinate, equalTo( new Coordinate( 13.37, 13.37 ) ) );

			  CRS crs = point.CRS;
			  assertThat( crs.Code, equalTo( 7203 ) );
			  assertThat( crs.Type, equalTo( "cartesian" ) );
			  assertThat( crs.Href, equalTo( "http://spatialreference.org/ref/sr-org/7203/" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldNotReturnInternalPointWhenInArray()
		 public virtual void ShouldNotReturnInternalPointWhenInArray()
		 {
			  // when
			  Result execute = GraphDb.execute( "RETURN [point({longitude: 144.317718, latitude: -37.031738})] AS ps" );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  IList<Point> points = ( IList<Point> )execute.Next()["ps"];
			  assertThat( points[0], Matchers.instanceOf( typeof( Point ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldNotReturnInternalPointWhenInMap()
		 public virtual void ShouldNotReturnInternalPointWhenInMap()
		 {
			  // when
			  Result execute = GraphDb.execute( "RETURN {p: point({longitude: 144.317718, latitude: -37.031738})} AS m" );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  IDictionary<string, object> points = ( IDictionary<string, object> )execute.Next()["m"];
			  assertThat( points["p"], Matchers.instanceOf( typeof( Point ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseResultingPointFromOneQueryAsParameterToNext()
		 public virtual void ShouldBeAbleToUseResultingPointFromOneQueryAsParameterToNext()
		 {
			  // given a point create by one cypher query
			  Result execute = GraphDb.execute( "RETURN point({longitude: 144.317718, latitude: -37.031738}) AS p" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Point point = ( Point ) execute.Next()["p"];

			  // when passing as params to a distance function
			  Result result = GraphDb.execute( "RETURN distance(point({longitude: 144.317718, latitude: -37.031738}),{previous}) AS dist", map( "previous", point ) );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  double? dist = ( double? ) result.Next()["dist"];
			  assertThat( dist, equalTo( 0.0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseExternalPointAsParameterToQuery()
		 public virtual void ShouldBeAbleToUseExternalPointAsParameterToQuery()
		 {
			  // given a point created from public interface
			  Point point = MakeFakePoint( 144.317718, -37.031738, MakeWGS84() );

			  // when passing as params to a distance function
			  Result result = GraphDb.execute( "RETURN distance(point({longitude: 144.317718, latitude: -37.031738}),{previous}) AS dist", map( "previous", point ) );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  double? dist = ( double? ) result.Next()["dist"];
			  assertThat( dist, equalTo( 0.0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseExternalGeometryAsParameterToQuery()
		 public virtual void ShouldBeAbleToUseExternalGeometryAsParameterToQuery()
		 {
			  // given a point created from public interface
			  Geometry geometry = MakeFakePointAsGeometry( 144.317718, -37.031738, MakeWGS84() );

			  // when passing as params to a distance function
			  Result result = GraphDb.execute( "RETURN distance(point({longitude: 144.317718, latitude: -37.031738}),{previous}) AS dist", map( "previous", geometry ) );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  double? dist = ( double? ) result.Next()["dist"];
			  assertThat( dist, equalTo( 0.0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseExternalPointArrayAsParameterToQuery()
		 public virtual void ShouldBeAbleToUseExternalPointArrayAsParameterToQuery()
		 {
			  // given a point created from public interface
			  Point point = MakeFakePoint( 144.317718, -37.031738, MakeWGS84() );
			  Point[] points = new Point[]{ point, point };

			  // when passing as params to a distance function
			  Result result = GraphDb.execute( "RETURN distance({points}[0],{points}[1]) AS dist", map( "points", points ) );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  double? dist = ( double? ) result.Next()["dist"];
			  assertThat( dist, equalTo( 0.0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseResultsOfPointProcedureAsInputToDistanceFunction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUseResultsOfPointProcedureAsInputToDistanceFunction()
		 {
			  // given procedure that produces a point
			  Procedures procedures = GraphDb.DependencyResolver.resolveDependency( typeof( Procedures ) );
			  procedures.RegisterProcedure( typeof( PointProcs ) );

			  // when calling procedure that produces a point
			  Result result = GraphDb.execute( "CALL spatial.point(144.317718, -37.031738) YIELD point " + "RETURN distance(point({longitude: 144.317718, latitude: -37.031738}), point) AS dist" );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  double? dist = ( double? ) result.Next()["dist"];
			  assertThat( dist, equalTo( 0.0 ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseResultsOfPointGeometryProcedureAsInputToDistanceFunction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUseResultsOfPointGeometryProcedureAsInputToDistanceFunction()
		 {
			  // given procedure that produces a point
			  Procedures procedures = GraphDb.DependencyResolver.resolveDependency( typeof( Procedures ) );
			  procedures.RegisterProcedure( typeof( PointProcs ) );

			  // when calling procedure that produces a point
			  Result result = GraphDb.execute( "CALL spatial.pointGeometry(144.317718, -37.031738) YIELD geometry " + "RETURN distance(point({longitude: 144.317718, latitude: -37.031738}), geometry) AS dist" );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  object dist1 = result.Next()["dist"];
			  double? dist = ( double? ) dist1;
			  assertThat( dist, equalTo( 0.0 ) );

		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.graphdb.spatial.Point makeFakePoint(double x, double y, final org.neo4j.graphdb.spatial.CRS crs)
		 private static Point MakeFakePoint( double x, double y, CRS crs )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.spatial.Coordinate coord = new org.neo4j.graphdb.spatial.Coordinate(x, y);
			  Coordinate coord = new Coordinate( x, y );
			  return new PointAnonymousInnerClass( crs, coord );
		 }

		 private class PointAnonymousInnerClass : Point
		 {
			 private CRS _crs;
			 private Coordinate _coord;

			 public PointAnonymousInnerClass( CRS crs, Coordinate coord )
			 {
				 this._crs = crs;
				 this._coord = coord;
			 }


			 public string GeometryType
			 {
				 get
				 {
					  return "Point";
				 }
			 }

			 public IList<Coordinate> Coordinates
			 {
				 get
				 {
					  return Arrays.asList( _coord );
				 }
			 }

			 public CRS CRS
			 {
				 get
				 {
					  return _crs;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.graphdb.spatial.Geometry makeFakePointAsGeometry(double x, double y, final org.neo4j.graphdb.spatial.CRS crs)
		 private static Geometry MakeFakePointAsGeometry( double x, double y, CRS crs )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.spatial.Coordinate coord = new org.neo4j.graphdb.spatial.Coordinate(x, y);
			  Coordinate coord = new Coordinate( x, y );
			  return new GeometryAnonymousInnerClass( crs, coord );
		 }

		 private class GeometryAnonymousInnerClass : Geometry
		 {
			 private CRS _crs;
			 private Coordinate _coord;

			 public GeometryAnonymousInnerClass( CRS crs, Coordinate coord )
			 {
				 this._crs = crs;
				 this._coord = coord;
			 }


			 public string GeometryType
			 {
				 get
				 {
					  return "Point";
				 }
			 }

			 public IList<Coordinate> Coordinates
			 {
				 get
				 {
					  return Arrays.asList( _coord );
				 }
			 }

			 public CRS CRS
			 {
				 get
				 {
					  return _crs;
				 }
			 }
		 }

		 private static CRS MakeWGS84()
		 {
			  // "WGS-84", 4326, "http://spatialreference.org/ref/epsg/4326/"
			  return new CRSAnonymousInnerClass();
		 }

		 private class CRSAnonymousInnerClass : CRS
		 {
			 public int Code
			 {
				 get
				 {
					  return 4326;
				 }
			 }

			 public string Type
			 {
				 get
				 {
					  return "WGS-84";
				 }
			 }

			 public string Href
			 {
				 get
				 {
					  return "http://spatialreference.org/ref/epsg/4326/";
				 }
			 }
		 }

		 public class PointProcs
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("spatial.point") public java.util.stream.Stream<PointResult> spatialPoint(@Name("longitude") double longitude, @Name("latitude") double latitude)
			  [Procedure("spatial.point")]
			  public virtual Stream<PointResult> SpatialPoint( double longitude, double latitude )
			  {
					Point point = MakeFakePoint( longitude, latitude, MakeWGS84() );
					return Stream.of( new PointResult( point ) );
			  }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("spatial.pointGeometry") public java.util.stream.Stream<GeometryResult> spatialPointGeometry(@Name("longitude") double longitude, @Name("latitude") double latitude)
			  [Procedure("spatial.pointGeometry")]
			  public virtual Stream<GeometryResult> SpatialPointGeometry( double longitude, double latitude )
			  {
					Geometry geometry = MakeFakePointAsGeometry( longitude, latitude, MakeWGS84() );
					return Stream.of( new GeometryResult( geometry ) );
			  }
		 }

		 public class PointResult
		 {
			  public Point Point;

			  public PointResult( Point point )
			  {
					this.Point = point;
			  }
		 }

		 public class GeometryResult
		 {
			  public Geometry Geometry;

			  public GeometryResult( Geometry geometry )
			  {
					this.Geometry = geometry;
			  }
		 }
	}

}