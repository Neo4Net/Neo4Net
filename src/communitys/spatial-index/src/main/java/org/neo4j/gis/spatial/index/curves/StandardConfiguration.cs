using System;

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

	public class StandardConfiguration : SpaceFillingCurveConfiguration
	{
		 public const int DEFAULT_EXTRA_LEVELS = 1;

		 /// <summary>
		 /// After estimating the search ratio, we know the level at which tiles have approximately the same size as
		 /// our search area. This number dictates the amount of levels we go deeper than that, to trim down the amount
		 /// of false positives.
		 /// </summary>
		 protected internal int ExtraLevels;

		 public StandardConfiguration() : this(DEFAULT_EXTRA_LEVELS)
		 {
		 }

		 public StandardConfiguration( int extraLevels )
		 {
			  this.ExtraLevels = extraLevels;
		 }

		 /// <summary>
		 /// This simply stops at the maxDepth calculated in the maxDepth() function, or
		 /// if the overlap is over 99%.
		 /// <para>
		 /// {@inheritDoc}
		 /// </para>
		 /// </summary>
		 public override bool StopAtThisDepth( double overlap, int depth, int maxDepth )
		 {
			  return overlap >= 0.99 || depth >= maxDepth;
		 }

		 /// <summary>
		 /// If the search area is exactly one of the finest grained tiles (tile at maxLevel), then
		 /// we want the search to traverse to maxLevel, however, for each area that is 4x larger, we would
		 /// traverse one level shallower. This is achieved by a log (base 4 for 2D, base 8 for 3D) of the ratio of areas.
		 /// <para>
		 /// {@inheritDoc}
		 /// </para>
		 /// </summary>
		 public override int MaxDepth( Envelope referenceEnvelope, Envelope range, int nbrDim, int maxLevel )
		 {
			  Envelope paddedEnvelope = referenceEnvelope.WithSideRatioNotTooSmall();
			  double searchRatio = range.Area / paddedEnvelope.Area;
			  if ( double.IsInfinity( searchRatio ) )
			  {
					return maxLevel;
			  }
			  return Math.Min( maxLevel, ( int )( Math.Log( searchRatio ) / Math.Log( Math.Pow( 2, nbrDim ) ) ) + ExtraLevels );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "(" + ExtraLevels + ")";
		 }

		 public override int InitialRangesListCapacity()
		 {
			  // Probably big enough to for the majority of index queries.
			  return 1000;
		 }
	}

}