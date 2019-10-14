using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Gis.Spatial.Index.curves
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Disabled = org.junit.jupiter.api.Disabled;
	using Test = org.junit.jupiter.api.Test;


	using FormattedLog = Neo4Net.Logging.FormattedLog;
	using Level = Neo4Net.Logging.Level;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve3D.BinaryCoordinateRotationUtils3D.rotateNPointLeft;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve3D.BinaryCoordinateRotationUtils3D.rotateNPointRight;

	public class SpaceFillingCurveTest
	{
		 private const bool DEBUG = false;

		 private FormattedLog _logger;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _logger = FormattedLog.withLogLevel( DEBUG ? Level.DEBUG : Level.NONE ).toOutputStream( System.out );
		 }

		 //
		 // Set of tests for 2D ZOrderCurve at various levels
		 //
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DZOrderCurveAtLevel1()
		 internal virtual void ShouldCreateSimple2DZOrderCurveAtLevel1()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  ZOrderSpaceFillingCurve2D curve = new ZOrderSpaceFillingCurve2D( envelope, 1 );
			  AssertAtLevel( curve, envelope );
			  assertRange( "Bottom-left should evaluate to zero", curve, GetTileEnvelope( envelope, 2, 0, 1 ), 0L );
			  assertRange( "Top-left should evaluate to one", curve, GetTileEnvelope( envelope, 2, 1, 1 ), 1L );
			  assertRange( "Top-right should evaluate to two", curve, GetTileEnvelope( envelope, 2, 0, 0 ), 2L );
			  assertRange( "Bottom-right should evaluate to three", curve, GetTileEnvelope( envelope, 2, 1, 0 ), 3L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DZOrderCurveAtLevel2()
		 internal virtual void ShouldCreateSimple2DZOrderCurveAtLevel2()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  ZOrderSpaceFillingCurve2D curve = new ZOrderSpaceFillingCurve2D( envelope, 2 );
			  AssertAtLevel( curve, envelope );
			  assertRange( "'00' should evaluate to 0", curve, GetTileEnvelope( envelope, 4, 0, 3 ), 0L );
			  assertRange( "'10' should evaluate to 1", curve, GetTileEnvelope( envelope, 4, 1, 3 ), 1L );
			  assertRange( "'11' should evaluate to 2", curve, GetTileEnvelope( envelope, 4, 0, 2 ), 2L );
			  assertRange( "'01' should evaluate to 3", curve, GetTileEnvelope( envelope, 4, 1, 2 ), 3L );
			  assertRange( "'02' should evaluate to 4", curve, GetTileEnvelope( envelope, 4, 2, 3 ), 4L );
			  assertRange( "'03' should evaluate to 5", curve, GetTileEnvelope( envelope, 4, 3, 3 ), 5L );
			  assertRange( "'13' should evaluate to 6", curve, GetTileEnvelope( envelope, 4, 2, 2 ), 6L );
			  assertRange( "'12' should evaluate to 7", curve, GetTileEnvelope( envelope, 4, 3, 2 ), 7L );
			  assertRange( "'22' should evaluate to 8", curve, GetTileEnvelope( envelope, 4, 0, 1 ), 8L );
			  assertRange( "'23' should evaluate to 9", curve, GetTileEnvelope( envelope, 4, 1, 1 ), 9L );
			  assertRange( "'33' should evaluate to 10", curve, GetTileEnvelope( envelope, 4, 0, 0 ), 10L );
			  assertRange( "'32' should evaluate to 11", curve, GetTileEnvelope( envelope, 4, 1, 0 ), 11L );
			  assertRange( "'31' should evaluate to 12", curve, GetTileEnvelope( envelope, 4, 2, 1 ), 12L );
			  assertRange( "'21' should evaluate to 13", curve, GetTileEnvelope( envelope, 4, 3, 1 ), 13L );
			  assertRange( "'20' should evaluate to 14", curve, GetTileEnvelope( envelope, 4, 2, 0 ), 14L );
			  assertRange( "'30' should evaluate to 15", curve, GetTileEnvelope( envelope, 4, 3, 0 ), 15L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DZOrderCurveAtLevel3()
		 internal virtual void ShouldCreateSimple2DZOrderCurveAtLevel3()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  AssertAtLevel( new ZOrderSpaceFillingCurve2D( envelope, 3 ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DZOrderCurveAtLevel4()
		 internal virtual void ShouldCreateSimple2DZOrderCurveAtLevel4()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  AssertAtLevel( new ZOrderSpaceFillingCurve2D( envelope, 4 ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DZOrderCurveAtLevel5()
		 internal virtual void ShouldCreateSimple2DZOrderCurveAtLevel5()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  AssertAtLevel( new ZOrderSpaceFillingCurve2D( envelope, 5 ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DZOrderCurveAtLevel24()
		 internal virtual void ShouldCreateSimple2DZOrderCurveAtLevel24()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  AssertAtLevel( new ZOrderSpaceFillingCurve2D( envelope, 24 ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DZOrderCurveAtManyLevels()
		 internal virtual void ShouldCreateSimple2DZOrderCurveAtManyLevels()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8 }, new double[]{ 8, 8 } );
			  for ( int level = 1; level <= ZOrderSpaceFillingCurve2D.MAX_LEVEL; level++ )
			  {
					ZOrderSpaceFillingCurve2D curve = new ZOrderSpaceFillingCurve2D( envelope, level );
					_logger.debug( "Max value at level " + level + ": " + curve.ValueWidth );
					AssertAtLevel( curve, envelope );
					assertRange( ( int ) curve.Width, 0, curve, 0, ( int ) curve.Width - 1 );
					assertRange( ( int ) curve.Width, curve.ValueWidth - 1, curve, ( int ) curve.Width - 1, 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DZOrderCurveAtLevelDefault()
		 internal virtual void ShouldCreateSimple2DZOrderCurveAtLevelDefault()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  AssertAtLevel( new ZOrderSpaceFillingCurve2D( envelope ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreate2DZOrderCurveWithRectangularEnvelope()
		 internal virtual void ShouldCreate2DZOrderCurveWithRectangularEnvelope()
		 {
			  Assert2DAtLevel( new Envelope( -8, 8, -20, 20 ), 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreate2DZOrderCurveWithNonCenteredEnvelope()
		 internal virtual void ShouldCreate2DZOrderCurveWithNonCenteredEnvelope()
		 {
			  Assert2DAtLevel( new Envelope( 2, 7, 2, 7 ), 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWorkWithNormalGPSCoordinatesZOrder()
		 internal virtual void ShouldWorkWithNormalGPSCoordinatesZOrder()
		 {
			  Envelope envelope = new Envelope( -180, 180, -90, 90 );
			  ZOrderSpaceFillingCurve2D curve = new ZOrderSpaceFillingCurve2D( envelope );
			  AssertAtLevel( curve, envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet2DZOrderSearchTilesForManyLevels()
		 internal virtual void ShouldGet2DZOrderSearchTilesForManyLevels()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  for ( int level = 1; level <= ZOrderSpaceFillingCurve2D.MAX_LEVEL; level++ )
			  {
					ZOrderSpaceFillingCurve2D curve = new ZOrderSpaceFillingCurve2D( envelope, level );
					double halfTile = curve.GetTileWidth( 0, level ) / 2.0;
					long start = DateTimeHelper.CurrentUnixTimeMillis();
					AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -8, -8 + halfTile, 8 - halfTile, 8 ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
					AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( 8 - halfTile, 8, -8, -8 + halfTile ) ), new SpaceFillingCurve.LongRange( curve.ValueWidth - 1, curve.ValueWidth - 1 ) );
					AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( 8 - halfTile, 8, 0, 0 + halfTile ) ), new SpaceFillingCurve.LongRange( curve.ValueWidth / 2 - 1, curve.ValueWidth / 2 - 1 ) );
					_logger.debug( "Hilbert query at level " + level + " took " + ( DateTimeHelper.CurrentUnixTimeMillis() - start ) + "ms" );
			  }
		 }

		 //
		 // Set of tests for 2D HilbertCurve at various levels
		 //

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DHilbertCurveAtLevel1()
		 internal virtual void ShouldCreateSimple2DHilbertCurveAtLevel1()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, 1 );
			  AssertAtLevel( curve, envelope );
			  assertRange( "Bottom-left should evaluate to zero", curve, GetTileEnvelope( envelope, 2, 0, 0 ), 0L );
			  assertRange( "Top-left should evaluate to one", curve, GetTileEnvelope( envelope, 2, 0, 1 ), 1L );
			  assertRange( "Top-right should evaluate to two", curve, GetTileEnvelope( envelope, 2, 1, 1 ), 2L );
			  assertRange( "Bottom-right should evaluate to three", curve, GetTileEnvelope( envelope, 2, 1, 0 ), 3L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DHilbertCurveAtLevel2()
		 internal virtual void ShouldCreateSimple2DHilbertCurveAtLevel2()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, 2 );
			  AssertAtLevel( curve, envelope );
			  assertRange( "'00' should evaluate to 0", curve, GetTileEnvelope( envelope, 4, 0, 0 ), 0L );
			  assertRange( "'10' should evaluate to 1", curve, GetTileEnvelope( envelope, 4, 1, 0 ), 1L );
			  assertRange( "'11' should evaluate to 2", curve, GetTileEnvelope( envelope, 4, 1, 1 ), 2L );
			  assertRange( "'01' should evaluate to 3", curve, GetTileEnvelope( envelope, 4, 0, 1 ), 3L );
			  assertRange( "'02' should evaluate to 4", curve, GetTileEnvelope( envelope, 4, 0, 2 ), 4L );
			  assertRange( "'03' should evaluate to 5", curve, GetTileEnvelope( envelope, 4, 0, 3 ), 5L );
			  assertRange( "'13' should evaluate to 6", curve, GetTileEnvelope( envelope, 4, 1, 3 ), 6L );
			  assertRange( "'12' should evaluate to 7", curve, GetTileEnvelope( envelope, 4, 1, 2 ), 7L );
			  assertRange( "'22' should evaluate to 8", curve, GetTileEnvelope( envelope, 4, 2, 2 ), 8L );
			  assertRange( "'23' should evaluate to 9", curve, GetTileEnvelope( envelope, 4, 2, 3 ), 9L );
			  assertRange( "'33' should evaluate to 10", curve, GetTileEnvelope( envelope, 4, 3, 3 ), 10L );
			  assertRange( "'32' should evaluate to 11", curve, GetTileEnvelope( envelope, 4, 3, 2 ), 11L );
			  assertRange( "'31' should evaluate to 12", curve, GetTileEnvelope( envelope, 4, 3, 1 ), 12L );
			  assertRange( "'21' should evaluate to 13", curve, GetTileEnvelope( envelope, 4, 2, 1 ), 13L );
			  assertRange( "'20' should evaluate to 14", curve, GetTileEnvelope( envelope, 4, 2, 0 ), 14L );
			  assertRange( "'30' should evaluate to 15", curve, GetTileEnvelope( envelope, 4, 3, 0 ), 15L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DHilbertCurveAtLevel3()
		 internal virtual void ShouldCreateSimple2DHilbertCurveAtLevel3()
		 {
			  Assert2DAtLevel( new Envelope( -8, 8, -8, 8 ), 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DHilbertCurveAtLevel4()
		 internal virtual void ShouldCreateSimple2DHilbertCurveAtLevel4()
		 {
			  Assert2DAtLevel( new Envelope( -8, 8, -8, 8 ), 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DHilbertCurveAtLevel5()
		 internal virtual void ShouldCreateSimple2DHilbertCurveAtLevel5()
		 {
			  Assert2DAtLevel( new Envelope( -8, 8, -8, 8 ), 5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DHilbertCurveAtLevel24()
		 internal virtual void ShouldCreateSimple2DHilbertCurveAtLevel24()
		 {
			  Assert2DAtLevel( new Envelope( -8, 8, -8, 8 ), 24 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DHilbertCurveAtManyLevels()
		 internal virtual void ShouldCreateSimple2DHilbertCurveAtManyLevels()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8 }, new double[]{ 8, 8 } );
			  for ( int level = 1; level <= HilbertSpaceFillingCurve2D.MAX_LEVEL; level++ )
			  {
					HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, level );
					_logger.debug( "Max value at level " + level + ": " + curve.ValueWidth );
					AssertAtLevel( curve, envelope );
					assertRange( ( int ) curve.Width, 0, curve, 0, 0 );
					assertRange( ( int ) curve.Width, curve.ValueWidth - 1, curve, ( int ) curve.Width - 1, 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple2DHilbertCurveAtLevelDefault()
		 internal virtual void ShouldCreateSimple2DHilbertCurveAtLevelDefault()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  AssertAtLevel( new HilbertSpaceFillingCurve2D( envelope ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreate2DHilbertCurveWithRectangularEnvelope()
		 internal virtual void ShouldCreate2DHilbertCurveWithRectangularEnvelope()
		 {
			  Assert2DAtLevel( new Envelope( -8, 8, -20, 20 ), 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreate2DHilbertCurveWithNonCenteredEnvelope()
		 internal virtual void ShouldCreate2DHilbertCurveWithNonCenteredEnvelope()
		 {
			  Assert2DAtLevel( new Envelope( 2, 7, 2, 7 ), 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreate2DHilbertCurveOfThreeLevelsFromExampleInThePaper()
		 internal virtual void ShouldCreate2DHilbertCurveOfThreeLevelsFromExampleInThePaper()
		 {
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( new Envelope( 0, 8, 0, 8 ), 3 );
			  assertThat( "Example should evaluate to 101110", curve.DerivedValueFor( new double[]{ 6, 4 } ), equalTo( 46L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWorkWithNormalGPSCoordinatesHilbert()
		 internal virtual void ShouldWorkWithNormalGPSCoordinatesHilbert()
		 {
			  Envelope envelope = new Envelope( -180, 180, -90, 90 );
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope );
			  AssertAtLevel( curve, envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet2DHilbertSearchTilesForLevel1()
		 internal virtual void ShouldGet2DHilbertSearchTilesForLevel1()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, 1 );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -6, -5, -6, -5 ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( 0, 6, -6, -5 ) ), new SpaceFillingCurve.LongRange( 3, 3 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -6, 4, -5, -2 ) ), new SpaceFillingCurve.LongRange( 0, 0 ), new SpaceFillingCurve.LongRange( 3, 3 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -2, -1, -6, 5 ) ), new SpaceFillingCurve.LongRange( 0, 1 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -2, 1, -6, 5 ) ), new SpaceFillingCurve.LongRange( 0, 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet2DHilbertSearchTilesForLevel2()
		 internal virtual void ShouldGet2DHilbertSearchTilesForLevel2()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, 2 );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -6, -5, -6, -5 ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( 0, 6, -6, -5 ) ), new SpaceFillingCurve.LongRange( 14, 15 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -6, 4, -5, -2 ) ), new SpaceFillingCurve.LongRange( 0, 3 ), new SpaceFillingCurve.LongRange( 12, 15 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -2, -1, -6, 5 ) ), new SpaceFillingCurve.LongRange( 1, 2 ), new SpaceFillingCurve.LongRange( 6, 7 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -2, 1, -6, 5 ) ), new SpaceFillingCurve.LongRange( 1, 2 ), new SpaceFillingCurve.LongRange( 6, 9 ), new SpaceFillingCurve.LongRange( 13, 14 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet2DHilbertSearchTilesForLevel3()
		 internal virtual void ShouldGet2DHilbertSearchTilesForLevel3()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, 3 );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -8, -7, -8, -7 ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( 0, 1, 0, 1 ) ), new SpaceFillingCurve.LongRange( 32, 32 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( 7, 8, -8, -1 ) ), new SpaceFillingCurve.LongRange( 48, 49 ), new SpaceFillingCurve.LongRange( 62, 63 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet2DHilbertSearchTilesForManyLevels()
		 internal virtual void ShouldGet2DHilbertSearchTilesForManyLevels()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  for ( int level = 1; level <= HilbertSpaceFillingCurve2D.MAX_LEVEL; level++ )
			  {
					HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, level );
					double halfTile = curve.GetTileWidth( 0, level ) / 2.0;
					long start = DateTimeHelper.CurrentUnixTimeMillis();
					AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( -8, -8 + halfTile, -8, -8 + halfTile ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
					AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( 8 - halfTile, 8, -8, -8 + halfTile ) ), new SpaceFillingCurve.LongRange( curve.ValueWidth - 1, curve.ValueWidth - 1 ) );
					AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( 0, halfTile, 0, halfTile ) ), new SpaceFillingCurve.LongRange( curve.ValueWidth / 2, curve.ValueWidth / 2 ) );
					_logger.debug( "Hilbert query at level " + level + " took " + ( DateTimeHelper.CurrentUnixTimeMillis() - start ) + "ms" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet2DHilbertSearchTilesForWideRangeAtManyLevels()
		 internal virtual void ShouldGet2DHilbertSearchTilesForWideRangeAtManyLevels()
		 {
			  const int xmin = -100;
			  const int xmax = 100;
			  const int ymin = -100;
			  const int ymax = 100;
			  Envelope envelope = new Envelope( xmin, xmax, ymin, ymax );
			  for ( int level = 1; level <= HilbertSpaceFillingCurve2D.MAX_LEVEL; level++ )
			  {
					HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, level );
					const int delta = 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int xhalf = xmin + (xmax - xmin) / 2;
					int xhalf = xmin + ( xmax - xmin ) / 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int yhalf = ymin + (ymax - ymin) / 2;
					int yhalf = ymin + ( ymax - ymin ) / 2;
					Envelope q1 = new Envelope( xmin, xhalf - delta, ymin, yhalf - delta );
					Envelope q2 = new Envelope( xmin, xhalf - delta, yhalf, ymax );
					Envelope q3 = new Envelope( xhalf, xmax, yhalf, ymax );
					Envelope q4 = new Envelope( xhalf, xmax, ymin, yhalf - delta );

					// Bottom left should give 1/4 of all tiles started at index 0
					AssertTiles( curve.GetTilesIntersectingEnvelope( q1 ), new SpaceFillingCurve.LongRange( 0, curve.ValueWidth / 4 - 1 ) );
					// Top left should give 1/4 of all tiles started at index 1/4
					AssertTiles( curve.GetTilesIntersectingEnvelope( q2 ), new SpaceFillingCurve.LongRange( curve.ValueWidth / 4, curve.ValueWidth / 2 - 1 ) );
					// Top right should give 1/4 of all tiles started at index 1/2
					AssertTiles( curve.GetTilesIntersectingEnvelope( q3 ), new SpaceFillingCurve.LongRange( curve.ValueWidth / 2, 3 * curve.ValueWidth / 4 - 1 ) );
					// Bottom right should give 1/4 of all tiles started at index 3/4
					AssertTiles( curve.GetTilesIntersectingEnvelope( q4 ), new SpaceFillingCurve.LongRange( 3 * curve.ValueWidth / 4, curve.ValueWidth - 1 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveReasonableCoveredArea()
		 internal virtual void ShouldHaveReasonableCoveredArea()
		 {
			  const double minExtent = 0.000001;
			  const double maxAspect = 100.0;
			  const int xmin = -100;
			  const int xmax = 100;
			  const int ymin = -100;
			  const int ymax = 100;
			  // Chosen to be smaller than 10, and "random" enough to not intersect with tile borders on higher levels.
			  const double rectangleStepsPerDimension = 9.789;
			  const double extensionFactor = 5;
			  string formatHeader1 = "Level  Depth Limitation Configuration                  Area Ratio              Ranges                  Depth";
			  string formatHeader2 = "                                                        avg    min    max       avg    min    max       avg    min    max";
			  string formatBody = "%5d  %-42s   %7.2f%7.2f%7.2f   %7.2f%7d%7d   %7.2f%7d%7d";

			  Envelope envelope = new Envelope( xmin, xmax, ymin, ymax );
			  // For all 2D levels
			  for ( int level = 1; level <= HilbertSpaceFillingCurve2D.MAX_LEVEL; level++ )
			  {
					if ( DEBUG )
					{
						 _logger.debug( "" );
						 _logger.debug( formatHeader1 );
						 _logger.debug( formatHeader2 );
					}
					foreach (SpaceFillingCurveConfiguration config in new SpaceFillingCurveConfiguration[]
					{
						new StandardConfiguration( 1 ),
						new StandardConfiguration( 2 ),
						new StandardConfiguration( 3 ),
						new StandardConfiguration( 4 ),
						new PartialOverlapConfiguration( 1, 0.99, 0.1 ),
						new PartialOverlapConfiguration( 1, 0.99, 0.5 ),
						new PartialOverlapConfiguration( 2, 0.99, 0.1 ),
						new PartialOverlapConfiguration( 2, 0.99, 0.5 ),
						new PartialOverlapConfiguration( 3, 0.99, 0.1 ),
						new PartialOverlapConfiguration( 3, 0.99, 0.5 ),
						new PartialOverlapConfiguration( 4, 0.99, 0.1 ),
						new PartialOverlapConfiguration( 4, 0.99, 0.5 )
					})
					{
						 MonitorDoubleStats areaStats = new MonitorDoubleStats();
						 MonitorStats rangeStats = new MonitorStats();
						 MonitorStats maxDepthStats = new MonitorStats();
						 HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, level );

						 // For differently shaped rectangles
						 for ( double xExtent = minExtent; xExtent <= xmax; xExtent *= extensionFactor )
						 {
							  for ( double yExtent = minExtent; yExtent <= ymax; yExtent *= extensionFactor )
							  {
									// Filter out very thin rectangles
									double aspect = xExtent > yExtent ? ( xExtent / yExtent ) : ( yExtent / xExtent );
									if ( aspect < maxAspect )
									{
										 // For different positions of the rectangle
										 for ( double xOffset = 0; xmin + xOffset + xExtent <= xmax; xOffset += ( xmax - xmin - xExtent ) / rectangleStepsPerDimension )
										 {
											  for ( double yOffset = 0; xmin + yOffset + yExtent <= ymax; yOffset += ( ymax - ymin - yExtent ) / rectangleStepsPerDimension )
											  {
													HistogramMonitor monitor = new HistogramMonitor( curve.MaxLevel );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double xStart = xmin + xOffset;
													double xStart = xmin + xOffset;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double xEnd = xStart + xExtent;
													double xEnd = xStart + xExtent;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double yStart = ymin + yOffset;
													double yStart = ymin + yOffset;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double yEnd = yStart + yExtent;
													double yEnd = yStart + yExtent;
													Envelope searchEnvelope = new Envelope( xStart, xEnd, yStart, yEnd );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long start = System.currentTimeMillis();
													long start = DateTimeHelper.CurrentUnixTimeMillis();
													IList<SpaceFillingCurve.LongRange> ranges = curve.GetTilesIntersectingEnvelope( searchEnvelope, config, monitor );
													if ( DEBUG )
													{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long end = System.currentTimeMillis();
														 long end = DateTimeHelper.CurrentUnixTimeMillis();
														 _logger.debug( string.Format( "Results for level {0:D}, with x=[{1:F},{2:F}] y=[{3:F},{4:F}]. " + "Search size vs covered size: {5:D} vs {6:D} ({7:F} x). Ranges: {8:D}. Took {9:D} ms\n", level, xStart, xEnd, yStart, yEnd, monitor.SearchArea, monitor.CoveredArea, ( double )( monitor.CoveredArea ) / monitor.SearchArea, ranges.Count, end - start ) );
														 int[] counts = monitor.Counts;
														 for ( int i = 0; i <= monitor.HighestDepth; i++ )
														 {
															  _logger.debug( "\t" + i + "\t" + counts[i] );
														 }

														 areaStats.Add( ( double )( monitor.CoveredArea ) / monitor.SearchArea );
														 rangeStats.Add( ranges.Count );
														 maxDepthStats.Add( monitor.HighestDepth );
													}

													assertThat( ranges, not( empty() ) );
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: assertThat(String.format("Search size was bigger than covered size for level %d, with x=[%a,%a] y=[%a,%a]", level, xStart, xEnd, yStart, yEnd), monitor.getSearchArea(), lessThanOrEqualTo(monitor.getCoveredArea()));
													assertThat( string.Format( "Search size was bigger than covered size for level %d, with x=[%a,%a] y=[%a,%a]", level, xStart, xEnd, yStart, yEnd ), monitor.SearchArea, lessThanOrEqualTo( monitor.CoveredArea ) );
											  }
										 }
									}
							  }
						 }

						 if ( DEBUG )
						 {
							  // Average over all runs on this level
							  _logger.debug( string.format( formatBody, level, config.ToString(), areaStats.Avg(), areaStats.Min, areaStats.Max, rangeStats.Avg(), rangeStats.Min, rangeStats.Max, maxDepthStats.Avg(), maxDepthStats.Min, maxDepthStats.Max ) );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveReasonableCoveredVolume()
		 internal virtual void ShouldHaveReasonableCoveredVolume()
		 {
			  const double minExtent = 0.000001;
			  const double maxAspect = 100.0;
			  Envelope envelope = new Envelope( new double[]{ -100, -100, -100 }, new double[]{ 100, 100, 100 } );
			  // Chosen to be smaller than 10, and "random" enough to not intersect with tile borders on higher levels.
			  const double rectangleStepsPerDimension = 3.789;
			  const double extensionFactor = 8;
			  string formatHeader1 = "Level  Depth Limitation Configuration                  Area Ratio              Ranges                  Depth";
			  string formatHeader2 = "                                                        avg    min    max       avg    min    max       avg    min    max";
			  string formatBody = "%5d  %-42s   %7.2f%7.2f%7.2f   %7.2f%7d%7d   %7.2f%7d%7d";

			  // For all 3D levels
			  for ( int level = 7; level <= HilbertSpaceFillingCurve3D.MAX_LEVEL; level += 3 )
			  {
					if ( DEBUG )
					{
						 _logger.debug( "" );
						 _logger.debug( formatHeader1 );
						 _logger.debug( formatHeader2 );
					}
					foreach (SpaceFillingCurveConfiguration config in new SpaceFillingCurveConfiguration[]
					{
						new StandardConfiguration( 1 ),
						new StandardConfiguration( 2 ),
						new StandardConfiguration( 3 ),
						new StandardConfiguration( 4 ),
						new PartialOverlapConfiguration( 1, 0.99, 0.1 ),
						new PartialOverlapConfiguration( 1, 0.99, 0.5 ),
						new PartialOverlapConfiguration( 2, 0.99, 0.1 ),
						new PartialOverlapConfiguration( 2, 0.99, 0.5 ),
						new PartialOverlapConfiguration( 3, 0.99, 0.1 ),
						new PartialOverlapConfiguration( 3, 0.99, 0.5 ),
						new PartialOverlapConfiguration( 4, 0.99, 0.1 ),
						new PartialOverlapConfiguration( 4, 0.99, 0.5 )
					})
					{
						 MonitorDoubleStats areaStats = new MonitorDoubleStats();
						 MonitorStats rangeStats = new MonitorStats();
						 MonitorStats maxDepthStats = new MonitorStats();
						 HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, level );

						 double[] extent = new double[3];
						 // For differently shaped rectangles
						 for ( extent[0] = minExtent; extent[0] <= envelope.GetMax( 0 ); extent[0] *= extensionFactor )
						 {
							  for ( extent[1] = minExtent; extent[1] <= envelope.GetMax( 1 ); extent[1] *= extensionFactor )
							  {
									for ( extent[2] = minExtent; extent[2] <= envelope.GetMax( 2 ); extent[2] *= extensionFactor )
									{
										 // Filter out very thin rectangles
										 double aspectXY = extent[0] > extent[1] ? ( extent[0] / extent[1] ) : ( extent[1] / extent[0] );
										 double aspectYZ = extent[1] > extent[2] ? ( extent[1] / extent[2] ) : ( extent[2] / extent[1] );
										 double aspectZX = extent[2] > extent[0] ? ( extent[2] / extent[0] ) : ( extent[0] / extent[2] );
										 if ( aspectXY < maxAspect && aspectYZ < maxAspect && aspectZX < maxAspect )
										 {
											  double[] offset = new double[3];
											  // For different positions of the rectangle
											  for ( offset[0] = 0; envelope.GetMin( 0 ) + offset[0] + extent[0] <= envelope.GetMax( 0 ); offset[0] += ( envelope.GetWidth( 0 ) - extent[0] ) / rectangleStepsPerDimension )
											  {
													for ( offset[1] = 0; envelope.GetMin( 1 ) + offset[1] + extent[1] <= envelope.GetMax( 1 ); offset[1] += ( envelope.GetWidth( 1 ) - extent[1] ) / rectangleStepsPerDimension )
													{
														 for ( offset[2] = 0; envelope.GetMin( 2 ) + offset[2] + extent[2] <= envelope.GetMax( 2 ); offset[2] += ( envelope.GetWidth( 2 ) - extent[2] ) / rectangleStepsPerDimension )
														 {
															  HistogramMonitor monitor = new HistogramMonitor( curve.MaxLevel );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double[] startPoint = java.util.Arrays.copyOf(envelope.getMin(), 3);
															  double[] startPoint = Arrays.copyOf( envelope.Min, 3 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double[] endPoint = java.util.Arrays.copyOf(extent, 3);
															  double[] endPoint = Arrays.copyOf( extent, 3 );
															  for ( int i = 0; i < 3; i++ )
															  {
																	startPoint[i] += offset[i];
																	endPoint[i] += startPoint[i];
															  }
															  Envelope searchEnvelope = new Envelope( startPoint, endPoint );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long start = System.currentTimeMillis();
															  long start = DateTimeHelper.CurrentUnixTimeMillis();
															  IList<SpaceFillingCurve.LongRange> ranges = curve.GetTilesIntersectingEnvelope( searchEnvelope, config, monitor );
															  if ( DEBUG )
															  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long end = System.currentTimeMillis();
																	long end = DateTimeHelper.CurrentUnixTimeMillis();
																	_logger.debug( string.Format( "Results for level {0:D}, with search {1}.", level, searchEnvelope.ToString() ) );
																	_logger.debug( string.Format( "Search size vs covered size: {0:D} vs {1:D} ({2:F} x). Ranges: {3:D}. Took {4:D} ms\n", monitor.SearchArea, monitor.CoveredArea, ( double )( monitor.CoveredArea ) / monitor.SearchArea, ranges.Count, end - start ) );
																	int[] counts = monitor.Counts;
																	for ( int i = 0; i <= monitor.HighestDepth; i++ )
																	{
																		 _logger.debug( "\t" + i + "\t" + counts[i] );
																	}

																	areaStats.Add( ( double )( monitor.CoveredArea ) / monitor.SearchArea );
																	rangeStats.Add( ranges.Count );
																	maxDepthStats.Add( monitor.HighestDepth );
															  }

															  assertThat( ranges, not( empty() ) );
															  assertThat( string.Format( "Search size was bigger than covered size for level {0:D}, with search {1}", level, searchEnvelope.ToString() ), monitor.SearchArea, lessThanOrEqualTo(monitor.CoveredArea) );
														 }
													}
											  }
										 }
									}
							  }
						 }

						 if ( DEBUG )
						 {
							  // Average over all runs on this level
							  _logger.debug( string.format( formatBody, level, config.ToString(), areaStats.Avg(), areaStats.Min, areaStats.Max, rangeStats.Avg(), rangeStats.Min, rangeStats.Max, maxDepthStats.Avg(), maxDepthStats.Min, maxDepthStats.Max ) );
						 }
					}
			  }
		 }

		 /// <summary>
		 /// This test can be uses to reproduce a bug with a single search envelope, if <seealso cref="shouldHaveReasonableCoveredArea()"/>
		 /// fails an assertion. It should be disabled by default.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled public void debugSingle()
		 public virtual void DebugSingle()
		 {
			  const int xmin = -100;
			  const int xmax = 100;
			  const int ymin = -100;
			  const int ymax = 100;

			  const int level = 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double xStart = -0x1.9p6;
			  double xStart = -0x1.9p6;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double xEnd = -0x1.8ffffd60e94eep6;
			  double xEnd = -0x1.8ffffd60e94eep6;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double yStart = 0x1.8ff5c28f5c28ep6;
			  double yStart = 0x1.8ff5c28f5c28ep6;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final double yEnd = 0x1.8ffffffffffffp6;
			  double yEnd = 0x1.8ffffffffffffp6;

			  Envelope envelope = new Envelope( xmin, xmax, ymin, ymax );
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, level );
			  Envelope searchEnvelope = new Envelope( xStart, xEnd, yStart, yEnd );
			  HistogramMonitor monitor = new HistogramMonitor( curve.MaxLevel );
			  IList<SpaceFillingCurve.LongRange> ranges = curve.GetTilesIntersectingEnvelope( searchEnvelope, new StandardConfiguration(), monitor );

			  _logger.debug( string.Format( "Results for level {0:D}, with x=[{1:F},{2:F}] y=[{3:F},{4:F}]\n", level, xStart, xEnd, yStart, yEnd ) );
			  _logger.debug( string.Format( "Search size vs covered size: {0:D} vs {1:D}\n", monitor.SearchArea, monitor.CoveredArea ) );
			  _logger.debug( "Ranges: " + ranges.Count );
			  int[] counts = monitor.Counts;
			  for ( int i = 0; i <= curve.MaxLevel; i++ )
			  {
					_logger.debug( "\t" + i + "\t" + counts[i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet2DHilbertSearchTilesForCenterRangeAndTraverseToBottom()
		 internal virtual void ShouldGet2DHilbertSearchTilesForCenterRangeAndTraverseToBottom()
		 {
			  TraverseToBottomConfiguration configuration = new TraverseToBottomConfiguration();
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  for ( int level = 2; level <= 11; level++ ) // 12 takes 6s, 13 takes 25s, 14 takes 100s, 15 takes over 400s
			  {
					HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, level );
					double fullTile = curve.GetTileWidth( 0, level );
					double halfTile = fullTile / 2.0;
					Envelope centerWithoutOuterRing = new Envelope( envelope.GetMin( 0 ) + fullTile + halfTile, envelope.GetMax( 0 ) - fullTile - halfTile, envelope.GetMin( 1 ) + fullTile + halfTile, envelope.GetMax( 1 ) - fullTile - halfTile );
					long start = DateTimeHelper.CurrentUnixTimeMillis();
					IList<SpaceFillingCurve.LongRange> result = curve.GetTilesIntersectingEnvelope( centerWithoutOuterRing, configuration, null );
					_logger.debug( "Hilbert query at level " + level + " took " + ( DateTimeHelper.CurrentUnixTimeMillis() - start ) + "ms to produce " + result.Count + " tiles" );
					AssertTiles( result, TilesNotTouchingOuterRing( curve ).ToArray() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveRangesWithinMaxValuesWhenMatchingWholeEnvelopeAtMaxLevel()
		 internal virtual void ShouldGiveRangesWithinMaxValuesWhenMatchingWholeEnvelopeAtMaxLevel()
		 {
			  Envelope envelope = new Envelope( -8, 8, -8, 8 );
			  HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope );
			  IList<SpaceFillingCurve.LongRange> ranges = curve.GetTilesIntersectingEnvelope( envelope );
			  assertThat( ranges.Count, equalTo( 1 ) );
			  assertThat( ranges[0].max, lessThan( long.MaxValue ) );
			  assertThat( ranges[0].min, greaterThan( long.MinValue ) );
		 }

		 //
		 // Set of tests for 3D HilbertCurve at various levels
		 //

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRotate3DNPointsLeft()
		 internal virtual void ShouldRotate3DNPointsLeft()
		 {
			  assertThat( rotateNPointLeft( 0b000 ), equalTo( 0b000 ) );
			  assertThat( rotateNPointLeft( 0b001 ), equalTo( 0b010 ) );
			  assertThat( rotateNPointLeft( 0b010 ), equalTo( 0b100 ) );
			  assertThat( rotateNPointLeft( 0b100 ), equalTo( 0b001 ) );
			  assertThat( rotateNPointLeft( 0b011 ), equalTo( 0b110 ) );
			  assertThat( rotateNPointLeft( 0b110 ), equalTo( 0b101 ) );
			  assertThat( rotateNPointLeft( 0b101 ), equalTo( 0b011 ) );
			  assertThat( rotateNPointLeft( 0b111 ), equalTo( 0b111 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRotate3DNPointsRight()
		 internal virtual void ShouldRotate3DNPointsRight()
		 {
			  assertThat( rotateNPointRight( 0b000 ), equalTo( 0b000 ) );
			  assertThat( rotateNPointRight( 0b001 ), equalTo( 0b100 ) );
			  assertThat( rotateNPointRight( 0b100 ), equalTo( 0b010 ) );
			  assertThat( rotateNPointRight( 0b010 ), equalTo( 0b001 ) );
			  assertThat( rotateNPointRight( 0b011 ), equalTo( 0b101 ) );
			  assertThat( rotateNPointRight( 0b101 ), equalTo( 0b110 ) );
			  assertThat( rotateNPointRight( 0b110 ), equalTo( 0b011 ) );
			  assertThat( rotateNPointRight( 0b111 ), equalTo( 0b111 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtLevel1()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtLevel1()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 1 );
			  AssertAtLevel( curve, envelope );
			  assertRange( 2, 0, curve, 0, 0, 0 );
			  assertRange( 2, 1, curve, 0, 1, 0 );
			  assertRange( 2, 2, curve, 0, 1, 1 );
			  assertRange( 2, 3, curve, 0, 0, 1 );
			  assertRange( 2, 4, curve, 1, 0, 1 );
			  assertRange( 2, 5, curve, 1, 1, 1 );
			  assertRange( 2, 6, curve, 1, 1, 0 );
			  assertRange( 2, 7, curve, 1, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtLevel2()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtLevel2()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 2 );
			  AssertAtLevel( curve, envelope );
			  assertRange( 4, 0, curve, 0, 0, 0 );
			  assertRange( 4, 1, curve, 0, 0, 1 );
			  assertRange( 4, 2, curve, 1, 0, 1 );
			  assertRange( 4, 3, curve, 1, 0, 0 );
			  assertRange( 4, 4, curve, 1, 1, 0 );
			  assertRange( 4, 5, curve, 1, 1, 1 );
			  assertRange( 4, 6, curve, 0, 1, 1 );
			  assertRange( 4, 7, curve, 0, 1, 0 );
			  assertRange( 4, 8, curve, 0, 2, 0 );
			  assertRange( 4, 9, curve, 1, 2, 0 );
			  assertRange( 4, 10, curve, 1, 3, 0 );
			  assertRange( 4, 11, curve, 0, 3, 0 );
			  assertRange( 4, 12, curve, 0, 3, 1 );
			  assertRange( 4, 13, curve, 1, 3, 1 );
			  assertRange( 4, 14, curve, 1, 2, 1 );
			  assertRange( 4, 15, curve, 0, 2, 1 );
			  assertRange( 4, 63, curve, 3, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtLevel3()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtLevel3()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 3 );
			  AssertAtLevel( curve, envelope );
			  assertRange( 8, 0, curve, 0, 0, 0 );
			  assertRange( 8, ( long ) Math.Pow( 8, 3 ) - 1, curve, 7, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtLevel4()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtLevel4()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 4 );
			  AssertAtLevel( curve, envelope );
			  assertRange( 16, 0, curve, 0, 0, 0 );
			  assertRange( 16, ( long ) Math.Pow( 8, 4 ) - 1, curve, 15, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtLevel5()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtLevel5()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 5 );
			  AssertAtLevel( curve, envelope );
			  assertRange( 32, 0, curve, 0, 0, 0 );
			  assertRange( 32, ( long ) Math.Pow( 8, 5 ) - 1, curve, 31, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtLevel6()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtLevel6()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 6 );
			  AssertAtLevel( curve, envelope );
			  assertRange( 64, 0, curve, 0, 0, 0 );
			  assertRange( 64, ( long ) Math.Pow( 8, 6 ) - 1, curve, 63, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtLevel7()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtLevel7()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 7 );
			  AssertAtLevel( curve, envelope );
			  assertRange( 128, 0, curve, 0, 0, 0 );
			  assertRange( 128, ( long ) Math.Pow( 8, 7 ) - 1, curve, 127, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtManyLevels()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtManyLevels()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  for ( int level = 1; level <= HilbertSpaceFillingCurve3D.MAX_LEVEL; level++ )
			  {
					HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, level );
					_logger.debug( "Max value at level " + level + ": " + curve.ValueWidth );
					AssertAtLevel( curve, envelope );
					assertRange( ( int ) curve.Width, 0, curve, 0, 0, 0 );
					assertRange( ( int ) curve.Width, curve.ValueWidth - 1, curve, ( int ) curve.Width - 1, 0, 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateSimple3DHilbertCurveAtLevelDefault()
		 internal virtual void ShouldCreateSimple3DHilbertCurveAtLevelDefault()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  AssertAtLevel( new HilbertSpaceFillingCurve3D( envelope ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreate3DHilbertCurveWithRectangularEnvelope()
		 internal virtual void ShouldCreate3DHilbertCurveWithRectangularEnvelope()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -20, -15 }, new double[]{ 8, 0, 15 } );
			  AssertAtLevel( new HilbertSpaceFillingCurve3D( envelope ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreate3DHilbertCurveWithNonCenteredEnvelope()
		 internal virtual void ShouldCreate3DHilbertCurveWithNonCenteredEnvelope()
		 {
			  Envelope envelope = new Envelope( new double[]{ 2, 2, 2 }, new double[]{ 7, 7, 7 } );
			  AssertAtLevel( new HilbertSpaceFillingCurve3D( envelope ), envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWorkWithNormalGPSCoordinatesAndHeight()
		 internal virtual void ShouldWorkWithNormalGPSCoordinatesAndHeight()
		 {
			  Envelope envelope = new Envelope( new double[]{ -180, -90, 0 }, new double[]{ 180, 90, 10000 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope );
			  AssertAtLevel( curve, envelope );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet3DSearchTilesForLevel1()
		 internal virtual void ShouldGet3DSearchTilesForLevel1()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 1 );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -6, -6, -6 }, new double[]{ -5, -5, -5 } ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ 0, -6, -6 }, new double[]{ 6, -5, -5 } ) ), new SpaceFillingCurve.LongRange( 7, 7 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -6, -5, -5 }, new double[]{ 4, -2, -2 } ) ), new SpaceFillingCurve.LongRange( 0, 0 ), new SpaceFillingCurve.LongRange( 7, 7 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -2, -6, -2 }, new double[]{ -1, 5, -1 } ) ), new SpaceFillingCurve.LongRange( 0, 1 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -2, -1, -1 }, new double[]{ -1, 1, 1 } ) ), new SpaceFillingCurve.LongRange( 0, 3 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -1, -1, -1 }, new double[]{ 1, 1, 1 } ) ), new SpaceFillingCurve.LongRange( 0, 7 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet3DSearchTilesForLevel2()
		 internal virtual void ShouldGet3DSearchTilesForLevel2()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 2 );
			  int[] mid = new int[]{ 5, 14, 17, 28, 35, 46, 49, 58 };
			  SpaceFillingCurve.LongRange[] midRanges = new SpaceFillingCurve.LongRange[mid.Length];
			  for ( int i = 0; i < mid.Length; i++ )
			  {
					midRanges[i] = new SpaceFillingCurve.LongRange( mid[i], mid[i] );
			  }
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -6, -6, -6 }, new double[]{ -5, -5, -5 } ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ 4, -6, -6 }, new double[]{ 6, -5, -5 } ) ), new SpaceFillingCurve.LongRange( 63, 63 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -6, -5, -5 }, new double[]{ 4, -2, -2 } ) ), new SpaceFillingCurve.LongRange( 0, 7 ), new SpaceFillingCurve.LongRange( 56, 63 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -2, -6, -2 }, new double[]{ -1, 5, -1 } ) ), new SpaceFillingCurve.LongRange( 2, 2 ), new SpaceFillingCurve.LongRange( 5, 5 ), new SpaceFillingCurve.LongRange( 13, 14 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -8, -3, -3 }, new double[]{ -1, 3, 3 } ) ), new SpaceFillingCurve.LongRange( 5, 6 ), new SpaceFillingCurve.LongRange( 14, 17 ), new SpaceFillingCurve.LongRange( 27, 28 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -1, -1, -1 }, new double[]{ 1, 1, 1 } ) ), midRanges );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet3DSearchTilesForLevel3()
		 internal virtual void ShouldGet3DSearchTilesForLevel3()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, 3 );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -8, -8, -8 }, new double[]{ -7, -7, -7 } ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ 7, -8, -8 }, new double[]{ 8, -7, -7 } ) ), new SpaceFillingCurve.LongRange( 511, 511 ) );
			  AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 7, 7, 7 } ) ), new SpaceFillingCurve.LongRange( 0, 511 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGet3DSearchTilesForManyLevels()
		 internal virtual void ShouldGet3DSearchTilesForManyLevels()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  for ( int level = 1; level <= HilbertSpaceFillingCurve3D.MAX_LEVEL; level++ )
			  {
					HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, level );
					double halfTile = curve.GetTileWidth( 0, level ) / 2.0;
					long start = DateTimeHelper.CurrentUnixTimeMillis();
					AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ -8, -8, -8 }, new double[]{ -8 + halfTile, -8 + halfTile, -8 + halfTile } ) ), new SpaceFillingCurve.LongRange( 0, 0 ) );
					AssertTiles( curve.GetTilesIntersectingEnvelope( new Envelope( new double[]{ 8 - halfTile, -8, -8 }, new double[]{ 8, -8 + halfTile, -8 + halfTile } ) ), new SpaceFillingCurve.LongRange( curve.ValueWidth - 1, curve.ValueWidth - 1 ) );
					//TODO: There is a performance issue building the full range when the search envelope hits a very wide part of the extent
					// Suggestion to fix this with shallower traversals
					//assertTiles(curve.getTilesIntersectingEnvelope(new Envelope(new double[]{-8, -8, -8}, new double[]{8, 8, 8})),
					// new HilbertSpaceFillingCurve.LongRange(0, curve.getValueWidth() - 1));
					_logger.debug( "Hilbert query at level " + level + " took " + ( DateTimeHelper.CurrentUnixTimeMillis() - start ) + "ms" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStepMoreThanDistanceOneForZOrderOnlyHalfTime()
		 internal virtual void ShouldStepMoreThanDistanceOneForZOrderOnlyHalfTime()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8 }, new double[]{ 8, 8 } );
			  for ( int level = 1; level < 8; level++ )
			  { // more than 8 takes way too long
					ZOrderSpaceFillingCurve2D curve = new ZOrderSpaceFillingCurve2D( envelope, level );
					ShouldNeverStepMoreThanDistanceOne( curve, level, 50 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNeverStepMoreThanDistanceOneForHilbert2D()
		 internal virtual void ShouldNeverStepMoreThanDistanceOneForHilbert2D()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8 }, new double[]{ 8, 8 } );
			  for ( int level = 1; level < 8; level++ )
			  { // more than 8 takes way too long
					HilbertSpaceFillingCurve2D curve = new HilbertSpaceFillingCurve2D( envelope, level );
					ShouldNeverStepMoreThanDistanceOne( curve, level, 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotStepMoreThanDistanceOneMoreThan10Percent()
		 internal virtual void ShouldNotStepMoreThanDistanceOneMoreThan10Percent()
		 {
			  Envelope envelope = new Envelope( new double[]{ -8, -8, -8 }, new double[]{ 8, 8, 8 } );
			  for ( int level = 1; level < 8; level++ )
			  { // more than 8 takes way too long
					HilbertSpaceFillingCurve3D curve = new HilbertSpaceFillingCurve3D( envelope, level );
					ShouldNeverStepMoreThanDistanceOne( curve, level, 10 );
			  }
		 }

		 private void ShouldNeverStepMoreThanDistanceOne( SpaceFillingCurve curve, int level, int badnessThresholdPercentage )
		 {
			  int badCount = 0;
			  long[] previous = null;
			  for ( long derivedValue = 0; derivedValue < curve.ValueWidth; derivedValue++ )
			  {
					long[] point = curve.NormalizedCoordinateFor( derivedValue, level );
					if ( previous != null )
					{
						 double distance = 0;
						 for ( int i = 0; i < point.Length; i++ )
						 {
							  distance += Math.Pow( point[i] - previous[i], 2 );
						 }
						 distance = Math.Sqrt( distance );
						 if ( distance > 1.0 )
						 {
							  badCount++;
						 }
					}
					previous = point;
			  }
			  int badness = ( int )( 100 * badCount / ( curve.ValueWidth - 1 ) );
			  assertThat( "Bad distance percentage should never be greater than " + badnessThresholdPercentage + "%", badness, lessThanOrEqualTo( badnessThresholdPercentage ) );
			  _logger.debug( string.Format( "Bad distance count for level: {0:D} ({1:D}/{2:D} = {3:D}%)", level, badCount, curve.ValueWidth - 1, badness ) );
		 }

		 //
		 // Test utilities and grouped/complex assertions for 2D and 3D Hilbert Curves
		 //
		 private static IList<SpaceFillingCurve.LongRange> TilesNotTouchingOuterRing( SpaceFillingCurve curve )
		 {
			  List<SpaceFillingCurve.LongRange> expected = new List<SpaceFillingCurve.LongRange>();
			  HashSet<long> outerRing = new HashSet<long>();
			  for ( int x = 0; x < curve.Width; x++ )
			  {
					// Adding top and bottom rows
					outerRing.Add( curve.DerivedValueFor( new long[]{ x, 0 } ) );
					outerRing.Add( curve.DerivedValueFor( new long[]{ x, curve.Width - 1 } ) );
			  }
			  for ( int y = 0; y < curve.Width; y++ )
			  {
					// adding left and right rows
					outerRing.Add( curve.DerivedValueFor( new long[]{ 0, y } ) );
					outerRing.Add( curve.DerivedValueFor( new long[]{ curve.Width - 1, y } ) );
			  }
			  for ( long derivedValue = 0; derivedValue < curve.ValueWidth; derivedValue++ )
			  {
					if ( !outerRing.Contains( derivedValue ) )
					{
						 SpaceFillingCurve.LongRange current = ( expected.Count > 0 ) ? expected[expected.Count - 1] : null;
						 if ( current != null && current.Max == derivedValue - 1 )
						 {
							  current.ExpandToMax( derivedValue );
						 }
						 else
						 {
							  current = new SpaceFillingCurve.LongRange( derivedValue );
							  expected.Add( current );
						 }
					}
			  }
			  return expected;
		 }

		 private static void AssertTiles( IList<SpaceFillingCurve.LongRange> results, params SpaceFillingCurve.LongRange[] expected )
		 {
			  assertThat( "Result differ: " + results + " != " + Arrays.ToString( expected ), results.Count, equalTo( expected.Length ) );
			  for ( int i = 0; i < results.Count; i++ )
			  {
					assertThat( "Result at " + i + " should be the same", results[i], equalTo( expected[i] ) );
			  }
		 }

		 private static Envelope GetTileEnvelope( Envelope envelope, int divisor, params int[] index )
		 {
			  double[] widths = envelope.GetWidths( divisor );
			  double[] min = Arrays.copyOf( envelope.Min, envelope.Dimension );
			  double[] max = Arrays.copyOf( envelope.Min, envelope.Dimension );
			  for ( int i = 0; i < min.Length; i++ )
			  {
					min[i] += index[i] * widths[i];
					max[i] += ( index[i] + 1 ) * widths[i];
			  }
			  return new Envelope( min, max );
		 }

		 private static void AssertRange( string message, ZOrderSpaceFillingCurve2D curve, Envelope range, long value )
		 {
			  for ( double x = range.MinX; x < range.MaxX; x += 1.0 )
			  {
					for ( double y = range.MinY; y < range.MaxY; y += 1.0 )
					{
						 AssertCurveAt( message, curve, value, x, y );
					}
			  }
		 }

		 private static void AssertRange( string message, HilbertSpaceFillingCurve2D curve, Envelope range, long value )
		 {
			  for ( double x = range.MinX; x < range.MaxX; x += 1.0 )
			  {
					for ( double y = range.MinY; y < range.MaxY; y += 1.0 )
					{
						 AssertCurveAt( message, curve, value, x, y );
					}
			  }
		 }

		 private static void AssertRange( string message, HilbertSpaceFillingCurve3D curve, Envelope range, long value )
		 {
			  for ( double x = range.GetMin( 0 ); x < range.GetMax( 0 ); x += 1.0 )
			  {
					for ( double y = range.GetMin( 1 ); y < range.GetMax( 1 ); y += 1.0 )
					{
						 for ( double z = range.GetMin( 2 ); z < range.GetMax( 2 ); z += 1.0 )
						 {
							  AssertCurveAt( message, curve, value, x, y, z );
						 }
					}
			  }
		 }

		 private static void AssertRange( int divisor, long value, ZOrderSpaceFillingCurve2D curve, params int[] index )
		 {
			  Envelope range = GetTileEnvelope( curve.Range, divisor, index );
			  string message = Arrays.ToString( index ) + " should evaluate to " + value;
			  AssertRange( message, curve, range, value );
		 }

		 private static void AssertRange( int divisor, long value, HilbertSpaceFillingCurve2D curve, params int[] index )
		 {
			  Envelope range = GetTileEnvelope( curve.Range, divisor, index );
			  string message = Arrays.ToString( index ) + " should evaluate to " + value;
			  AssertRange( message, curve, range, value );
		 }

		 private static void AssertRange( int divisor, long value, HilbertSpaceFillingCurve3D curve, params int[] index )
		 {
			  Envelope range = GetTileEnvelope( curve.Range, divisor, index );
			  string message = Arrays.ToString( index ) + " should evaluate to " + value;
			  AssertRange( message, curve, range, value );
		 }

		 private static void AssertCurveAt( string message, SpaceFillingCurve curve, long value, params double[] coord )
		 {
			  double[] halfTileWidths = new double[coord.Length];
			  for ( int i = 0; i < coord.Length; i++ )
			  {
					halfTileWidths[i] = curve.GetTileWidth( i, curve.MaxLevel ) / 2.0;
			  }
			  long result = curve.DerivedValueFor( coord ).Value;
			  double[] coordinate = curve.CenterPointFor( result );
			  assertThat( message + ": " + Arrays.ToString( coord ), result, equalTo( value ) );
			  for ( int i = 0; i < coord.Length; i++ )
			  {
					assertThat( message + ": " + Arrays.ToString( coord ), Math.Abs( coordinate[i] - coord[i] ), lessThanOrEqualTo( halfTileWidths[i] ) );
			  }
		 }

		 private static void Assert2DAtLevel( Envelope envelope, int level )
		 {
			  AssertAtLevel( new HilbertSpaceFillingCurve2D( envelope, level ), envelope );
		 }

		 private static void AssertAtLevel( ZOrderSpaceFillingCurve2D curve, Envelope envelope )
		 {
			  int level = curve.MaxLevel;
			  long width = ( long ) Math.Pow( 2, level );
			  long valueWidth = width * width;
			  double justInsideMaxX = envelope.MaxX - curve.GetTileWidth( 0, level ) / 2.0;
			  double justInsideMaxY = envelope.MaxY - curve.GetTileWidth( 1, level ) / 2.0;
			  double midX = ( envelope.MinX + envelope.MaxX ) / 2.0;
			  double midY = ( envelope.MinY + envelope.MaxY ) / 2.0;

			  long topRight = 1L;
			  long topRightFactor = 2L;
			  long topRightDiff = 1;
			  string topRightDescription = "1";
			  for ( int l = 0; l < level; l++ )
			  {
					topRight = topRightFactor - topRightDiff;
					topRightDescription = topRightFactor.ToString() + " - " + topRightDescription;
					topRightDiff = topRightFactor + topRightDiff;
					topRightFactor *= 4;
			  }

			  assertThat( "Level " + level + " should have width of " + width, curve.Width, equalTo( width ) );
			  assertThat( "Level " + level + " should have max value of " + valueWidth, curve.ValueWidth, equalTo( valueWidth ) );

			  AssertCurveAt( "Top-left should evaluate to zero", curve, 0, envelope.MinX, envelope.MaxY );
			  AssertCurveAt( "Just inside right edge on the bottom should evaluate to max-value", curve, curve.ValueWidth - 1, justInsideMaxX, envelope.MinY );
			  AssertCurveAt( "Just inside top-right corner should evaluate to " + topRightDescription, curve, topRight, justInsideMaxX, justInsideMaxY );
			  AssertCurveAt( "Right on top-right corner should evaluate to " + topRightDescription, curve, topRight, envelope.MaxX, envelope.MaxY );
			  AssertCurveAt( "Bottom-right should evaluate to max-value", curve, curve.ValueWidth - 1, envelope.MaxX, envelope.MinY );
			  AssertCurveAt( "Max x-value, middle y-value should evaluate to (maxValue-1) / 2", curve, ( curve.ValueWidth - 1 ) / 2, envelope.MaxX, midY );
		 }

		 private static void AssertAtLevel( HilbertSpaceFillingCurve2D curve, Envelope envelope )
		 {
			  int level = curve.MaxLevel;
			  long width = ( long ) Math.Pow( 2, level );
			  long valueWidth = width * width;
			  double justInsideMaxX = envelope.MaxX - curve.GetTileWidth( 0, level ) / 2.0;
			  double justInsideMaxY = envelope.MaxY - curve.GetTileWidth( 1, level ) / 2.0;
			  double midX = ( envelope.MinX + envelope.MaxX ) / 2.0;
			  double midY = ( envelope.MinY + envelope.MaxY ) / 2.0;

			  long topRight = 0L;
			  long topRightFactor = 2L;
			  StringBuilder topRightDescription = new StringBuilder();
			  for ( int l = 0; l < level; l++ )
			  {
					topRight += topRightFactor;
					if ( topRightDescription.Length == 0 )
					{
						 topRightDescription.Append( topRightFactor );
					}
					else
					{
						 topRightDescription.Append( " + " ).Append( topRightFactor );
					}
					topRightFactor *= 4;
			  }

			  assertThat( "Level " + level + " should have width of " + width, curve.Width, equalTo( width ) );
			  assertThat( "Level " + level + " should have max value of " + valueWidth, curve.ValueWidth, equalTo( valueWidth ) );

			  AssertCurveAt( "Bottom-left should evaluate to zero", curve, 0, envelope.MinX, envelope.MinY );
			  AssertCurveAt( "Just inside right edge on the bottom should evaluate to max-value", curve, curve.ValueWidth - 1, justInsideMaxX, envelope.MinY );
			  AssertCurveAt( "Just inside top-right corner should evaluate to " + topRightDescription, curve, topRight, justInsideMaxX, justInsideMaxY );
			  AssertCurveAt( "Right on top-right corner should evaluate to " + topRightDescription, curve, topRight, envelope.MaxX, envelope.MaxY );
			  AssertCurveAt( "Bottom-right should evaluate to max-value", curve, curve.ValueWidth - 1, envelope.MaxX, envelope.MinY );
			  AssertCurveAt( "Middle value should evaluate to (max-value+1) / 2", curve, curve.ValueWidth / 2, midX, midY );
		 }

		 private static void AssertAtLevel( HilbertSpaceFillingCurve3D curve, Envelope envelope )
		 {
			  int level = curve.MaxLevel;
			  int dimension = curve.RootCurve().Dimension;
			  long width = ( long ) Math.Pow( 2, level );
			  long valueWidth = ( long ) Math.Pow( width, dimension );
			  double midY = ( envelope.GetMax( 1 ) + envelope.GetMin( 1 ) ) / 2.0;
			  double[] justInsideMax = new double[dimension];
			  double[] midValY = new double[]{ envelope.GetMin( 1 ), envelope.GetMax( 1 ) };
			  for ( int i = 0; i < level; i++ )
			  {
					if ( i % 2 == 0 )
					{
						 midValY[1] = ( midValY[0] + midValY[1] ) / 2.0;
					}
					else
					{
						 midValY[0] = ( midValY[0] + midValY[1] ) / 2.0;
					}
			  }
			  double[] locationOfHalfCurve = new double[]{ ( envelope.GetMin( 0 ) + envelope.GetMax( 0 ) ) / 2.0, ( midValY[0] + midValY[1] ) / 2.0, envelope.GetMax( 1 ) - curve.GetTileWidth( 2, level ) / 2.0 };
			  for ( int i = 0; i < dimension; i++ )
			  {
					justInsideMax[i] = envelope.GetMax( i ) - curve.GetTileWidth( i, level ) / 2.0;
			  }

			  long frontRightMid = valueWidth / 2 + valueWidth / 8 + valueWidth / 256;
			  string fromRightMidDescription = valueWidth.ToString() + "/2 + " + valueWidth + "/8";

			  assertThat( "Level " + level + " should have width of " + width, curve.Width, equalTo( width ) );
			  assertThat( "Level " + level + " should have max value of " + valueWidth, curve.ValueWidth, equalTo( valueWidth ) );

			  AssertCurveAt( "Bottom-left should evaluate to zero", curve, 0, envelope.Min );
			  AssertCurveAt( "Just inside right edge on the bottom back should evaluate to max-value", curve, curve.ValueWidth - 1, ReplaceOne( envelope.Min, justInsideMax[0], 0 ) );
			  if ( curve.MaxLevel < 5 )
			  {
					AssertCurveAt( "Just above front-right-mid edge should evaluate to " + fromRightMidDescription, curve, frontRightMid, ReplaceOne( justInsideMax, midY, 1 ) );
					AssertCurveAt( "Right on top-right-front corner should evaluate to " + fromRightMidDescription, curve, frontRightMid, ReplaceOne( envelope.Max, midY, 1 ) );
			  }
			  AssertCurveAt( "Bottom-right-back should evaluate to max-value", curve, curve.ValueWidth - 1, ReplaceOne( envelope.Min, envelope.GetMax( 0 ), 0 ) );
			  if ( curve.MaxLevel < 3 )
			  {
					AssertCurveAt( "Middle value should evaluate to (max-value+1) / 2", curve, curve.ValueWidth / 2, locationOfHalfCurve );
			  }
		 }

		 private static double[] ReplaceOne( double[] values, double value, int index )
		 {
			  double[] newValues = Arrays.copyOf( values, values.Length );
			  newValues[index] = value;
			  return newValues;
		 }

		 private class MonitorStats
		 {
			  internal long Total;
			  internal long Min = long.MaxValue;
			  internal long Max;
			  internal int Count;

			  internal virtual void Add( long value )
			  {
					Total += value;
					Min = Math.Min( Min, value );
					Max = Math.Max( Max, value );
					Count++;
			  }

			  internal virtual double Avg()
			  {
					return ( double ) Total / ( double ) Count;
			  }
		 }

		 private class MonitorDoubleStats
		 {
			  internal double Total;
			  internal double Min = double.MaxValue;
			  internal double Max;
			  internal int Count;

			  internal virtual void Add( double value )
			  {
					Total += value;
					Min = Math.Min( Min, value );
					Max = Math.Max( Max, value );
					Count++;
			  }

			  internal virtual double Avg()
			  {
					return Total / Count;
			  }
		 }
	}

}