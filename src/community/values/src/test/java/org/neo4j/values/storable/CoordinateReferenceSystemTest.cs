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
namespace Neo4Net.Values.Storable
{
	using Test = org.junit.jupiter.api.Test;

	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.closeTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;

	internal class CoordinateReferenceSystemTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGetCrsByCode()
		 internal virtual void ShouldGetCrsByCode()
		 {
			  assertEquals( Cartesian, CoordinateReferenceSystem.get( Cartesian.Code ) );
			  assertEquals( WGS84, CoordinateReferenceSystem.get( WGS84.Code ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailToGetWithIncorrectCode()
		 internal virtual void ShouldFailToGetWithIncorrectCode()
		 {
			  InvalidValuesArgumentException exception = assertThrows( typeof( InvalidValuesArgumentException ), () => CoordinateReferenceSystem.Get(42) );
			  assertEquals( "Unknown coordinate reference system code: 42", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFindByTableAndCode()
		 internal virtual void ShouldFindByTableAndCode()
		 {
			  assertThat( CoordinateReferenceSystem.Get( 1, 4326 ), equalTo( CoordinateReferenceSystem.Wgs84 ) );
			  assertThat( CoordinateReferenceSystem.Get( 1, 4979 ), equalTo( CoordinateReferenceSystem.Wgs84_3d ) );
			  assertThat( CoordinateReferenceSystem.Get( 2, 7203 ), equalTo( CoordinateReferenceSystem.Cartesian ) );
			  assertThat( CoordinateReferenceSystem.Get( 2, 9157 ), equalTo( CoordinateReferenceSystem.Cartesian_3D ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateCartesianDistance()
		 internal virtual void ShouldCalculateCartesianDistance()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.Cartesian;
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0 ), Cart( 0.0, 1.0 ) ), equalTo( 1.0 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0 ), Cart( 1.0, 0.0 ) ), equalTo( 1.0 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0 ), Cart( 1.0, 1.0 ) ), closeTo( 1.4, 0.02 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0 ), Cart( 0.0, -1.0 ) ), equalTo( 1.0 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0 ), Cart( -1.0, 0.0 ) ), equalTo( 1.0 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0 ), Cart( -1.0, -1.0 ) ), closeTo( 1.4, 0.02 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 1.0, 0.0 ), Cart( 0.0, -1.0 ) ), closeTo( 1.4, 0.02 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 1.0, 0.0 ), Cart( -1.0, 0.0 ) ), equalTo( 2.0 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 1.0, 0.0 ), Cart( -1.0, -1.0 ) ), closeTo( 2.24, 0.01 ) );
			  assertThat( "", crs.Calculator.distance( Cart( -1000000.0, -1000000.0 ), Cart( 1000000.0, 1000000.0 ) ), closeTo( 2828427.0, 1.0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateCartesianDistance3D()
		 internal virtual void ShouldCalculateCartesianDistance3D()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.Cartesian_3D;
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0, 0.0 ), Cart( 1.0, 0.0, 0.0 ) ), equalTo( 1.0 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0, 0.0 ), Cart( 0.0, 1.0, 0.0 ) ), equalTo( 1.0 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0, 0.0 ), Cart( 0.0, 0.0, 1.0 ) ), equalTo( 1.0 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0, 0.0 ), Cart( 0.0, 1.0, 1.0 ) ), closeTo( 1.41, 0.01 ) );
			  assertThat( "", crs.Calculator.distance( Cart( 0.0, 0.0, 0.0 ), Cart( 1.0, 1.0, 1.0 ) ), closeTo( 1.73, 0.01 ) );
			  assertThat( "", crs.Calculator.distance( Cart( -1000000.0, -1000000.0, -1000000.0 ), Cart( 1000000.0, 1000000.0, 1000000.0 ) ), closeTo( 3464102.0, 1.0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateGeographicDistance()
		 internal virtual void ShouldCalculateGeographicDistance()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.Wgs84;
			  assertThat( "2D distance should match", crs.Calculator.distance( Geo( 0.0, 0.0 ), Geo( 0.0, 90.0 ) ), closeTo( 10000000.0, 20000.0 ) );
			  assertThat( "2D distance should match", crs.Calculator.distance( Geo( 0.0, 0.0 ), Geo( 0.0, -90.0 ) ), closeTo( 10000000.0, 20000.0 ) );
			  assertThat( "2D distance should match", crs.Calculator.distance( Geo( 0.0, -45.0 ), Geo( 0.0, 45.0 ) ), closeTo( 10000000.0, 20000.0 ) );
			  assertThat( "2D distance should match", crs.Calculator.distance( Geo( -45.0, 0.0 ), Geo( 45.0, 0.0 ) ), closeTo( 10000000.0, 20000.0 ) );
			  assertThat( "2D distance should match", crs.Calculator.distance( Geo( -45.0, 0.0 ), Geo( 45.0, 0.0 ) ), closeTo( 10000000.0, 20000.0 ) );
			  //"distance function should measure distance from Copenhagen train station to Neo4j in Malmö"
			  PointValue cph = Geo( 12.564590, 55.672874 );
			  PointValue malmo = Geo( 12.994341, 55.611784 );
			  double expected = 27842.0;
			  assertThat( "2D distance should match", crs.Calculator.distance( cph, malmo ), closeTo( expected, 0.1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateGeographicDistance3D()
		 internal virtual void ShouldCalculateGeographicDistance3D()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.Wgs84_3d;
			  //"distance function should measure distance from Copenhagen train station to Neo4j in Malmö"
			  PointValue cph = Geo( 12.564590, 55.672874, 0.0 );
			  PointValue cphHigh = Geo( 12.564590, 55.672874, 1000.0 );
			  PointValue malmo = Geo( 12.994341, 55.611784, 0.0 );
			  PointValue malmoHigh = Geo( 12.994341, 55.611784, 1000.0 );
			  double expected = 27842.0;
			  double expectedHigh = 27862.0;
			  assertThat( "3D distance should match", crs.Calculator.distance( cph, malmo ), closeTo( expected, 0.1 ) );
			  assertThat( "3D distance should match", crs.Calculator.distance( cph, malmoHigh ), closeTo( expectedHigh, 0.2 ) );
			  assertThat( "3D distance should match", crs.Calculator.distance( cphHigh, malmo ), closeTo( expectedHigh, 0.2 ) );
		 }

		 private PointValue Cart( params double[] coords )
		 {
			  CoordinateReferenceSystem crs = coords.Length == 3 ? CoordinateReferenceSystem.Cartesian_3D : CoordinateReferenceSystem.Cartesian;
			  return Values.PointValue( crs, coords );
		 }

		 private PointValue Geo( params double[] coords )
		 {
			  CoordinateReferenceSystem crs = coords.Length == 3 ? CoordinateReferenceSystem.Cartesian_3D : CoordinateReferenceSystem.Cartesian;
			  return Values.PointValue( crs, coords );
		 }
	}

}