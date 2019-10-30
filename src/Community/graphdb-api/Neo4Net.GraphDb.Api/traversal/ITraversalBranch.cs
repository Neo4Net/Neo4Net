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

//
namespace Neo4Net.GraphDb.Traversal
{
    /// <summary>
    /// Represents a <seealso cref="IPath position"/> and a <seealso cref="PathExpander"/> with a
    /// traversal context, for example parent and an iterator of relationships to go
    /// next. It's a base to write a <seealso cref="IBranchSelector"/> on top of.
    /// </summary>
    public interface ITraversalBranch : IPath
    {
        /// <summary>
        /// The parent expansion source which created this <seealso cref="ITraversalBranch"/>. </summary>
        /// <returns> the parent of this expansion source. </returns>
        ITraversalBranch Parent { get; }

        /// <summary>
        /// Returns the next expansion source from the expanded relationships
        /// from the current node.
        /// </summary>
        /// <param name="expander"> an expander to decide which relationships to follow </param>
        /// <param name="metadata"> the context of the traversal </param>
        /// <returns> the next expansion source from this expansion source. </returns>
        ITraversalBranch Next(IPathExpander expander, TraversalContext metadata);

        /// <summary>
        /// Returns the number of relationships this expansion source has expanded.
        /// In this count isn't included the relationship which led to coming here
        /// (since that could also be traversed, although skipped, when expanding
        /// this source).
        /// </summary>
        /// <returns> the number of relationships this expansion source has expanded. </returns>
        int Expanded { get; }

        /// <summary>
        /// Explicitly tell this branch to be pruned so that consecutive calls to
        /// <seealso cref="next(IPathExpander, TraversalContext)"/> is guaranteed to return
        /// {@code null}.
        /// </summary>
        void Prune();

        /// <returns> whether or not the traversal should continue further along this
        /// branch. </returns>
        bool Continues { get; }

        /// <returns> whether or not this branch (the <seealso cref="IPath"/> representation of
        /// this branch at least) should be included in the result of this
        /// traversal, i.e. returned as one of the <seealso cref="IPath"/>s from f.ex.
        /// <seealso cref="ITraversalDescription.traverse(org.Neo4Net.graphdb.Node...)"/> </returns>
        bool Includes { get; }

        /// <summary>
        /// Can change evaluation outcome in a negative direction. For example
        /// to force pruning. </summary>
        /// <param name="eval"> the <seealso cref="Evaluation"/> to AND with the current evaluation. </param>
        void Evaluation(Evaluation eval);

        /// <summary>
        /// Initializes this <seealso cref="ITraversalBranch"/>, the relationship iterator,
        /// <seealso cref="Evaluation"/> etc.
        /// </summary>
        /// <param name="expander"> <seealso cref="PathExpander"/> to use for getting relationships. </param>
        /// <param name="metadata"> <seealso cref="TraversalContext"/> to update on progress. </param>
        void Initialize(IPathExpander expander, TraversalContext metadata);

        /// <summary>
        /// Returns the state associated with this branch.
        ///
        /// Why is this of type <seealso cref="object"/>? The state object type only exists when
        /// specifying the expander in the <seealso cref="ITraversalDescription"/>, not anywhere
        /// else. So in the internals of the traversal the state type is unknown and ignored.
        /// </summary>
        /// <returns> the state associated with this branch. </returns>
        object State { get; }
    }
}