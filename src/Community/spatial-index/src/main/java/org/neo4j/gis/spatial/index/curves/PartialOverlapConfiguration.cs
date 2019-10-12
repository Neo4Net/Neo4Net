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
namespace Neo4Net.Gis.Spatial.Index.curves
{
	public class PartialOverlapConfiguration : StandardConfiguration
	{
		 private static double _topThreshold = 0.99;
		 private static double _bottomThreshold = 0.5;
		 private double _topThreshold;
		 private double _bottomThreshold;

		 public PartialOverlapConfiguration() : this(StandardConfiguration.DEFAULT_EXTRA_LEVELS, _topThreshold, _bottomThreshold)
		 {
		 }

		 public PartialOverlapConfiguration( int extraLevels, double topThreshold, double bottomThreshold ) : base( extraLevels )
		 {
			  this._topThreshold = topThreshold;
			  this._bottomThreshold = bottomThreshold;
		 }

		 /// <summary>
		 /// This simply stops at the maxDepth calculated in the maxDepth() function, or
		 /// if the overlap is over some fraction 99% (by default) at the top levels, but reduces
		 /// linearly to 0.5 (by default) when we get to maxDepth.
		 /// <para>
		 /// {@inheritDoc}
		 /// </para>
		 /// </summary>
		 public override bool StopAtThisDepth( double overlap, int depth, int maxDepth )
		 {
			  double slope = ( _bottomThreshold - _topThreshold ) / maxDepth;
			  double threshold = slope * depth + _topThreshold;
			  return overlap >= threshold || depth >= maxDepth;
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "(" + ExtraLevels + "," + _topThreshold + "," + _bottomThreshold + ")";
		 }
	}

}