using System.Collections.Generic;

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
    /// Selects <seealso cref="ITraversalBranch"/>s according to postorder breadth first
    /// pattern which basically is a reverse to preorder breadth first in that
    /// deepest levels are returned first, see
    /// http://en.wikipedia.org/wiki/Breadth-first_search
    /// </summary>
    internal class PostorderBreadthFirstSelector : IBranchSelector
    {
        private IEnumerator<ITraversalBranch> _sourceIterator;
        private readonly ITraversalBranch _current;
        private readonly IPathExpander _expander;

        internal PostorderBreadthFirstSelector(ITraversalBranch startSource, IPathExpander expander)
        {
            _current = startSource;
            _expander = expander;
        }

        public override ITraversalBranch Next(ITraversalContext metadata)
        {
            if (_sourceIterator == null)
            {
                _sourceIterator = GatherSourceIterator(metadata);
            }
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            return _sourceIterator.hasNext() ? _sourceIterator.next() : null;
        }

        private IEnumerator<ITraversalBranch> GatherSourceIterator(ITraversalContext metadata)
        {
            LinkedList<ITraversalBranch> queue = new LinkedList<ITraversalBranch>();
            queue.AddLast(_current.next(_expander, metadata));
            while (true)
            {
                IList<ITraversalBranch> level = GatherOneLevel(queue, metadata);
                if (level.Count == 0)
                {
                    break;
                }
                queue.addAll(0, level);
            }
            return queue.GetEnumerator();
        }

        private IList<ITraversalBranch> GatherOneLevel(IList<ITraversalBranch> queue, ITraversalContext metadata)
        {
            IList<ITraversalBranch> level = new LinkedList<ITraversalBranch>();
            int? depth = null;
            foreach (ITraversalBranch source in queue)
            {
                if (depth == null)
                {
                    depth = source.Length;
                }
                else if (source.Length != depth.Value)
                {
                    break;
                }

                while (true)
                {
                    ITraversalBranch next = source.Next(_expander, metadata);
                    if (next == null)
                    {
                        break;
                    }
                    level.Add(next);
                }
            }
            return level;
        }
    }
}