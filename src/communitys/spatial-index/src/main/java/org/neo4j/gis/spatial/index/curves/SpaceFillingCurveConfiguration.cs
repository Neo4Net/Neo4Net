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

	/// <summary>
	/// These settings define how to optimize the 2D (or 3D) to 1D mapping of the space filling curve.
	/// They will affect the number of 1D ranges produced as well as the number of false positives expected from the 1D index.
	/// The ideal performance depends on the behaviour of the underlying 1D index, whether it costs more to have more 1D searches,
	/// or have more false positives for post filtering.
	/// </summary>
	public interface SpaceFillingCurveConfiguration
	{
		 /// <summary>
		 /// Decides whether to stop at this depth or recurse deeper.
		 /// </summary>
		 /// <param name="overlap"> the overlap between search space and the current extent </param>
		 /// <param name="depth"> the current recursion depth </param>
		 /// <param name="maxDepth"> the maximum depth that was calculated to recurse to, </param>
		 /// <returns> if the algorithm should recurse deeper, returns {@code false}; if the algorithm
		 /// should stop at this depth, returns {@code true} </returns>
		 bool StopAtThisDepth( double overlap, int depth, int maxDepth );

		 /// <summary>
		 /// Decide how deep to recurse at max.
		 /// </summary>
		 /// <param name="referenceEnvelope"> the envelope describing the search area </param>
		 /// <param name="range"> the envelope describing the indexed area </param>
		 /// <param name="nbrDim"> the number of dimensions </param>
		 /// <param name="maxLevel"> the depth of the spaceFillingCurve </param>
		 /// <returns> the maximum depth to which the algorithm should recurse in the space filling curve. </returns>
		 int MaxDepth( Envelope referenceEnvelope, Envelope range, int nbrDim, int maxLevel );

		 /// <returns> the size to use when initializing the ArrayList to store ranges. </returns>
		 int InitialRangesListCapacity();
	}

}