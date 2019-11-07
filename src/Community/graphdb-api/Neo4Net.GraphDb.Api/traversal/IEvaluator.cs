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
    /// An Evaluator controls what's to be returned from a traversal and also how
    /// pruning is done. It looks at a <seealso cref="IPath"/> and decides whether or not it
    /// should be included in the traversal result. It also decides whether the traverser
    /// should continue down that path or if it should be pruned so that the traverser
    /// won't continue down that path.
    ///
    /// @author Mattias Persson </summary>
    /// <seealso cref= Evaluation </seealso>
    /// <seealso cref= Evaluators </seealso>
    /// <seealso cref= TraversalDescription#evaluator(Evaluator) </seealso>
    public interface IEvaluator
    {
        /// <summary>
        /// Evaluates a <seealso cref="IPath"/> and returns an <seealso cref="Evaluation"/> containing
        /// information about whether or not to include it in the traversal result,
        /// i.e return it from the <seealso cref="ITraverser"/>. And also whether or not to
        /// continue traversing down that {@code path} or if it instead should be
        /// pruned so that the traverser won't continue down that branch represented
        /// by {@code path}.
        /// </summary>
        /// <param name="path"> the <seealso cref="IPath"/> to evaluate. </param>
        /// <returns> an <seealso cref="Evaluation"/> containing information about whether or not
        /// to return it from the <seealso cref="ITraverser"/> and whether or not to continue
        /// down that path. </returns>
        Evaluation Evaluate(IPath path);

        /// <summary>
        /// Exposes an <seealso cref="IEvaluator"/> as a <seealso cref="PathEvaluator"/>. </summary>
        /// @param <STATE> the type of state passed into the evaluator. </param>
    }

    public class Evaluator_AsPathEvaluator<STATE> : PathEvaluator<STATE>
    {
        internal readonly IEvaluator Evaluator;

        public Evaluator_AsPathEvaluator(IEvaluator evaluator)
        {
            this.Evaluator = evaluator;
        }

        public override Evaluation Evaluate(IPath path, IBranchState<STATE> state)
        {
            return Evaluator.Evaluate(path);
        }

        public override Evaluation Evaluate(IPath path)
        {
            return Evaluator.Evaluate(path);
        }
    }
}