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
	public class HistogramMonitor : SpaceFillingCurveMonitor
	{
		 private int[] _counts;
		 private int _highestDepth;
		 private long _searchArea;
		 private long _coveredArea;

		 internal HistogramMonitor( int maxLevel )
		 {
			  this._counts = new int[maxLevel + 1];
		 }

		 public override void AddRangeAtDepth( int depth )
		 {
			  this._counts[depth]++;
			  if ( depth > _highestDepth )
			  {
					_highestDepth = depth;
			  }
		 }

		 public override void RegisterSearchArea( long size )
		 {
			  this._searchArea = size;
		 }

		 public override void AddToCoveredArea( long size )
		 {
			  this._coveredArea += size;
		 }

		 internal virtual int[] Counts
		 {
			 get
			 {
				  return this._counts;
			 }
		 }

		 internal virtual long SearchArea
		 {
			 get
			 {
				  return _searchArea;
			 }
		 }

		 internal virtual long CoveredArea
		 {
			 get
			 {
				  return _coveredArea;
			 }
		 }

		 internal virtual int HighestDepth
		 {
			 get
			 {
				  return _highestDepth;
			 }
		 }
	}

}