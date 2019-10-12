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
	using Org.Neo4j.Graphdb;

	/// <summary>
	/// Selects <seealso cref="TraversalBranch"/>s according to preorder depth first pattern,
	/// the most natural ordering in a depth first search, see
	/// http://en.wikipedia.org/wiki/Depth-first_search
	/// </summary>
	internal class PreorderDepthFirstSelector : BranchSelector
	{
		 private TraversalBranch _current;
		 private readonly PathExpander _expander;

		 internal PreorderDepthFirstSelector( TraversalBranch startSource, PathExpander expander )
		 {
			  this._current = startSource;
			  this._expander = expander;
		 }

		 public override TraversalBranch Next( TraversalContext metadata )
		 {
			  TraversalBranch result = null;
			  while ( result == null )
			  {
					if ( _current == null )
					{
						 return null;
					}
					TraversalBranch next = _current.next( _expander, metadata );
					if ( next == null )
					{
						 _current = _current.parent();
						 continue;
					}
					_current = next;
					result = _current;
			  }
			  return result;
		 }
	}

}