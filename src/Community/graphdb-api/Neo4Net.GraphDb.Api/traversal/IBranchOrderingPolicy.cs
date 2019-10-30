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
    /// Creator of <seealso cref="IBranchSelector"/> instances with a starting point to base
    /// the first decision on.
    /// </summary>
    public interface IBranchOrderingPolicy
    {
        /// <summary>
        /// Instantiates a <seealso cref="IBranchSelector"/> with {@code startBranch} as the
        /// first branch to base a decision on "where to go next".
        /// </summary>
        /// <param name="startBranch"> the <seealso cref="ITraversalBranch"/> to start from. </param>
        /// <param name="expander"> <seealso cref="PathExpander"/> to use for expanding the branch. </param>
        /// <returns> a new <seealso cref="IBranchSelector"/> used to decide "where to go next" in
        ///         the traversal. </returns>
        IBranchSelector Create(ITraversalBranch startBranch, IPathExpander expander);
    }
}