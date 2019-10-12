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
namespace Org.Neo4j.Gis.Spatial.Index
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.closeTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class EnvelopeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateBasic2DEnvelopes()
		 internal virtual void ShouldCreateBasic2DEnvelopes()
		 {
			  for ( double width = 0.0; width < 10.0; width += 2.5 )
			  {
					for ( double minx = -10.0; minx < 10.0; minx += 2.5 )
					{
						 for ( double miny = -10.0; miny < 10.0; miny += 2.5 )
						 {
							  double maxx = minx + width;
							  double maxy = miny + width;
							  MakeAndTestEnvelope( new double[]{ minx, miny }, new double[]{ maxx, maxy }, new double[]{ width, width } );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleIntersectionsIn1D()
		 internal virtual void ShouldHandleIntersectionsIn1D()
		 {
			  double widthX = 1.0;
			  double widthY = 1.0;
			  Envelope left = new Envelope( 0.0, widthX, 0.0, widthY );
			  for ( double minx = -10.0; minx < 10.0; minx += 0.2 )
			  {
					double maxx = minx + widthX;
					Envelope right = new Envelope( minx, maxx, 0.0, widthY );
					if ( maxx < left.MinX || minx > left.MaxX )
					{
						 TestDoesNotOverlap( left, right );
					}
					else
					{
						 double overlapX = ( maxx < left.MaxX ) ? maxx - left.MinX : left.MaxX - minx;
						 double overlap = overlapX * widthY;
						 TestOverlaps( left, right, true, overlap );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleIntersectionsIn2D()
		 internal virtual void ShouldHandleIntersectionsIn2D()
		 {
			  Envelope left = new Envelope( 0.0, 1.0, 0.0, 1.0 );
			  TestOverlaps( left, new Envelope( 0.0, 1.0, 0.0, 1.0 ), true, 1.0, 1.0 ); // copies
			  TestOverlaps( left, new Envelope( 0.5, 1.0, 0.5, 1.0 ), true, 1.0, 0.25 ); // top right quadrant
			  TestOverlaps( left, new Envelope( 0.25, 0.75, 0.25, 0.75 ), true, 1.0, 0.25 ); // centered
			  TestOverlaps( left, new Envelope( -0.5, 0.5, -0.5, 0.5 ), true, 0.25, 0.25 ); // overlaps bottom left quadrant
			  TestOverlaps( left, new Envelope( -0.5, 1.5, -0.5, 1.5 ), true, 1.0, 1.0 ); // encapsulates
			  TestOverlaps( left, new Envelope( -1.0, 0.0, 0.0, 1.0 ), true, 0.0, 0.0 ); // touches left edge
			  TestOverlaps( left, new Envelope( 0.5, 1.5, 1.0, 2.0 ), true, 0.0, 0.0 ); // touches top-right edge
			  TestOverlaps( left, new Envelope( 0.5, 1.5, 0.0, 1.0 ), true, 0.5, 0.5 ); // overlaps right half
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testWithSideRatioNotTooSmall2D()
		 internal virtual void TestWithSideRatioNotTooSmall2D()
		 {
			  // No change expected
			  double[] from = new double[]{ 0, 0 };
			  double[] to = new double[]{ 1, 1 };

			  Envelope envelope = ( new Envelope( from, to ) ).WithSideRatioNotTooSmall();
			  double[] expectedFrom = new double[]{ 0, 0 };
			  double[] expectedTo = new double[]{ 1, 1 };
			  assertArrayEquals( expectedFrom, envelope.MinConflict, 0.0001 );
			  assertArrayEquals( expectedTo, envelope.MaxConflict, 0.0001 );

			  // Expected to change
			  const double bigValue = 100;
			  const double smallValue = 0.000000000000000001;
			  to = new double[]{ bigValue, smallValue };
			  Envelope envelope2 = ( new Envelope( from, to ) ).WithSideRatioNotTooSmall();
			  double[] expectedTo2 = new double[]{ bigValue, bigValue / Envelope.MAXIMAL_ENVELOPE_SIDE_RATIO };
			  assertArrayEquals( expectedFrom, envelope2.MinConflict, 0.0001 );
			  assertArrayEquals( expectedTo2, envelope2.MaxConflict, 0.00001 );
		 }

		 // Works for any number of dimensions, and 4 is more interesting than 3
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testWithSideRatioNotTooSmall4D()
		 internal virtual void TestWithSideRatioNotTooSmall4D()
		 {
			  // No change expected
			  double[] from = new double[]{ 0, 0, 0, 0 };
			  double[] to = new double[]{ 1, 1, 1, 1 };

			  Envelope envelope = ( new Envelope( from, to ) ).WithSideRatioNotTooSmall();
			  double[] expectedFrom = new double[]{ 0, 0, 0, 0 };
			  double[] expectedTo = new double[]{ 1, 1, 1, 1 };
			  assertArrayEquals( expectedFrom, envelope.MinConflict, 0.0001 );
			  assertArrayEquals( expectedTo, envelope.MaxConflict, 0.0001 );

			  // Expected to change
			  const double bigValue = 107;
			  const double smallValue = 0.00000000000000000123;
			  to = new double[]{ bigValue, smallValue, 12, smallValue * 0.1 };
			  Envelope envelope2 = ( new Envelope( from, to ) ).WithSideRatioNotTooSmall();
			  double[] expectedTo2 = new double[]{ bigValue, bigValue / Envelope.MAXIMAL_ENVELOPE_SIDE_RATIO, 12, bigValue / Envelope.MAXIMAL_ENVELOPE_SIDE_RATIO };
			  assertArrayEquals( expectedFrom, envelope2.MinConflict, 0.00001 );
			  assertArrayEquals( expectedTo2, envelope2.MaxConflict, 0.00001 );
		 }

		 private static void MakeAndTestEnvelope( double[] min, double[] max, double[] width )
		 {
			  Envelope env = new Envelope( min, max );
			  assertThat( "Expected min-x to be correct", env.MinX, equalTo( min[0] ) );
			  assertThat( "Expected min-y to be correct", env.MinY, equalTo( min[1] ) );
			  assertThat( "Expected max-x to be correct", env.MaxX, equalTo( max[0] ) );
			  assertThat( "Expected max-y to be correct", env.MaxY, equalTo( max[1] ) );
			  assertThat( "Expected dimension to be same as min.length", env.Dimension, equalTo( min.Length ) );
			  assertThat( "Expected dimension to be same as max.length", env.Dimension, equalTo( max.Length ) );
			  for ( int i = 0; i < min.Length; i++ )
			  {
					assertThat( "Expected min[" + i + "] to be correct", env.GetMin( i ), equalTo( min[i] ) );
					assertThat( "Expected max[" + i + "] to be correct", env.GetMax( i ), equalTo( max[i] ) );
			  }
			  double area = 1.0;
			  Envelope copy = new Envelope( env );
			  Envelope intersection = env.Intersection( copy );
			  for ( int i = 0; i < min.Length; i++ )
			  {
					assertThat( "Expected width[" + i + "] to be correct", env.GetWidth( i ), equalTo( width[i] ) );
					assertThat( "Expected copied width[" + i + "] to be correct", copy.GetWidth( i ), equalTo( width[i] ) );
					assertThat( "Expected intersected width[" + i + "] to be correct", intersection.GetWidth( i ), equalTo( width[i] ) );
					area *= width[i];
			  }
			  assertThat( "Expected area to be correct", env.Area, equalTo( area ) );
			  assertThat( "Expected copied area to be correct", env.Area, equalTo( copy.Area ) );
			  assertThat( "Expected intersected area to be correct", env.Area, equalTo( intersection.Area ) );
			  assertTrue( env.Intersects( copy ), "Expected copied envelope to intersect" );
			  assertThat( "Expected copied envelope to intersect completely", env.Overlap( copy ), equalTo( 1.0 ) );
		 }

		 private static void TestDoesNotOverlap( Envelope left, Envelope right )
		 {
			  Envelope bbox = new Envelope( left );
			  bbox.ExpandToInclude( right );
			  TestOverlaps( left, right, false, 0.0 );
		 }

		 private static void TestOverlaps( Envelope left, Envelope right, bool intersects, double overlap )
		 {
			  string intersectMessage = intersects ? "Should intersect" : "Should not intersect";
			  string overlapMessage = intersects ? "Should overlap" : "Should not have overlap";
			  assertThat( intersectMessage, left.Intersects( right ), equalTo( intersects ) );
			  assertThat( intersectMessage, right.Intersects( left ), equalTo( intersects ) );
			  assertThat( overlapMessage, left.Overlap( right ), closeTo( overlap, 0.000001 ) );
			  assertThat( overlapMessage, right.Overlap( left ), closeTo( overlap, 0.000001 ) );
		 }

		 private static void TestOverlaps( Envelope left, Envelope right, bool intersects, double overlap, double overlapArea )
		 {
			  TestOverlaps( left, right, intersects, overlap );
			  assertThat( "Expected overlap area", left.Intersection( right ).Area, closeTo( overlapArea, 0.000001 ) );
			  assertThat( "Expected overlap area", right.Intersection( left ).Area, closeTo( overlapArea, 0.000001 ) );
		 }
	}

}