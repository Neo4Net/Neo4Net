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

	public class TraverseToBottomConfiguration : SpaceFillingCurveConfiguration
	{
		 public override bool StopAtThisDepth( double overlap, int depth, int maxDepth )
		 {
			  return false;
		 }

		 public override int MaxDepth( Envelope referenceEnvelope, Envelope range, int nbrDim, int maxLevel )
		 {
			  return maxLevel;
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }

		 public override int InitialRangesListCapacity()
		 {
			  // When traversing to bottom, we can get extremely large lists and can't estimate the length.
			  // Thus, we can just as well start with a short list.
			  return 10;
		 }
	}

}