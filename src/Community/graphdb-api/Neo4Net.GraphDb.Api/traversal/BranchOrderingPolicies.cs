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
    /// A catalog of convenient branch ordering policies.
    ///
    /// Copied from kernel package so that we can hide kernel from the public API.
    /// </summary>
    public enum BranchOrderingPolicies
    {
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: PREORDER_DEPTH_FIRST { @Override public BranchSelector create(TraversalBranch startSource, Neo4Net.GraphDb.PathExpander expander) { return new PreorderDepthFirstSelector(startSource, expander); } },
        PREORDER_DEPTH_FIRST

		 {
             public IBranchSelector create(ITraversalBranch startSource, IPathExpander expander) { return new PreorderDepthFirstSelector(startSource, expander); }
},

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: POSTORDER_DEPTH_FIRST { @Override public BranchSelector create(TraversalBranch startSource, Neo4Net.GraphDb.PathExpander expander) { return new PostorderDepthFirstSelector(startSource, expander); } },
		 POSTORDER_DEPTH_FIRST
		 {
			 public BranchSelector create(TraversalBranch startSource, PathExpander expander) { return new PostorderDepthFirstSelector(startSource, expander); }
		 },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: PREORDER_BREADTH_FIRST { @Override public BranchSelector create(TraversalBranch startSource, Neo4Net.GraphDb.PathExpander expander) { return new PreorderBreadthFirstSelector(startSource, expander); } },
		 PREORDER_BREADTH_FIRST
		 {
			 public BranchSelector create(TraversalBranch startSource, PathExpander expander) { return new PreorderBreadthFirstSelector(startSource, expander); }
		 },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: POSTORDER_BREADTH_FIRST { @Override public BranchSelector create(TraversalBranch startSource, Neo4Net.GraphDb.PathExpander expander) { return new PostorderBreadthFirstSelector(startSource, expander); } }
		 POSTORDER_BREADTH_FIRST
		 {
			 public BranchSelector create(TraversalBranch startSource, PathExpander expander) { return new PostorderBreadthFirstSelector(startSource, expander); }
		 }
	}
}