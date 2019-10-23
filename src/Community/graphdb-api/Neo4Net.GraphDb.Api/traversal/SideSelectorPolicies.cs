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
	/// A catalogue of convenient side selector policies for use in bidirectional traversals.
	/// 
	/// Copied from kernel package so that we can hide kernel from the public API.
	/// </summary>
	public enum SideSelectorPolicies
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: LEVEL { @Override public SideSelector create(BranchSelector start, BranchSelector end, int maxDepth) { return new LevelSelectorOrderer(start, end, false, maxDepth); } },
		 LEVEL
		 {
			 public SideSelector create( BranchSelector start, BranchSelector end, int maxDepth ) { return new LevelSelectorOrderer( start, end, false, maxDepth ); }
		 },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: LEVEL_STOP_DESCENT_ON_RESULT { @Override public SideSelector create(BranchSelector start, BranchSelector end, int maxDepth) { return new LevelSelectorOrderer(start, end, true, maxDepth); } },
		 LEVEL_STOP_DESCENT_ON_RESULT
		 {
			 public SideSelector create( BranchSelector start, BranchSelector end, int maxDepth ) { return new LevelSelectorOrderer( start, end, true, maxDepth ); }
		 },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: ALTERNATING { @Override public SideSelector create(BranchSelector start, BranchSelector end, int maxDepth) { return new AlternatingSelectorOrderer(start, end); } }
		 ALTERNATING
		 {
			 public SideSelector create( BranchSelector start, BranchSelector end, int maxDepth ) { return new AlternatingSelectorOrderer( start, end ); }
		 }
	}

}