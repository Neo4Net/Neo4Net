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
    using Neo4Net.GraphDb.Impl.Traversal;

    public class AlternatingSelectorOrderer : AbstractSelectorOrderer<int>
    {
        public AlternatingSelectorOrderer(IBranchSelector startSelector, IBranchSelector endSelector) : base(startSelector, endSelector)
        {
        }

        protected internal override int? InitialState()
        {
            return 0;
        }

        public override ITraversalBranch Next(TraversalContext metadata)
        {
            ITraversalBranch branch = NextBranchFromNextSelector(metadata, true);
            int? previousDepth = StateForCurrentSelector;
            if (branch != null && branch.Length == previousDepth.Value)
            {
                return branch;
            }
            else
            {
                if (branch != null)
                {
                    StateForCurrentSelector = branch.Length;
                }
            }
            return branch;
        }
    }
}