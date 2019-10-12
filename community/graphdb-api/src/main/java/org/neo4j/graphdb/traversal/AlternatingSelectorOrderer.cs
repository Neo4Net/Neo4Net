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
namespace Org.Neo4j.Graphdb.traversal
{
	using Org.Neo4j.Graphdb.impl.traversal;

	public class AlternatingSelectorOrderer : AbstractSelectorOrderer<int>
	{
		 public AlternatingSelectorOrderer( BranchSelector startSelector, BranchSelector endSelector ) : base( startSelector, endSelector )
		 {
		 }

		 protected internal override int? InitialState()
		 {
			  return 0;
		 }

		 public override TraversalBranch Next( TraversalContext metadata )
		 {
			  TraversalBranch branch = NextBranchFromNextSelector( metadata, true );
			  int? previousDepth = StateForCurrentSelector;
			  if ( branch != null && branch.Length() == previousDepth.Value )
			  {
					return branch;
			  }
			  else
			  {
					if ( branch != null )
					{
						 StateForCurrentSelector = branch.Length();
					}
			  }
			  return branch;
		 }
	}

}