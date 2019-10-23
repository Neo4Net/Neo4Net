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
namespace Neo4Net.GraphDb.Traversal
{
    /// <summary>
    /// Interface for filters preventing the traversal from visiting already seen parts of the graph. Implementations can
    /// allow for different heuristics to use to determine what may be re-visited.
    /// </summary>
    public interface IUniquenessFilter
    {
        /// <summary>
        /// The check whether or not to expand the first branch is a separate
        /// method because it may contain checks which would be unnecessary for
        /// all other checks. So it's purely an optimization.
        /// </summary>
        /// <param name="branch"> the first branch to check, i.e. the branch representing
        ///               the start node in the traversal. </param>
        /// <returns> whether or not {@code branch} is unique, and hence can be
        ///         visited in this traversal. </returns>
        bool CheckFirst(TraversalBranch branch);

        /// <summary>
        /// Checks whether or not {@code branch} is unique, and hence can be
        /// visited in this traversal.
        /// </summary>
        /// <param name="branch"> the <seealso cref="TraversalBranch"/> to check for uniqueness. </param>
        /// <returns> whether or not {@code branch} is unique, and hence can be
        ///         visited in this traversal. </returns>
        bool Check(TraversalBranch branch);
    }
}