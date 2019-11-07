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
	using Neo4Net.GraphDb;

	/// <summary>
	/// Selects <seealso cref="ITraversalBranch"/>s according to preorder depth first pattern,
	/// the most natural ordering in a depth first search, see
	/// http://en.wikipedia.org/wiki/Depth-first_search
	/// </summary>
	internal class PreorderDepthFirstSelector : IBranchSelector
	{
		 private ITraversalBranch _current;
		 private readonly IPathExpander _expander;

		 internal PreorderDepthFirstSelector( ITraversalBranch startSource, IPathExpander expander )
		 {
			  _current = startSource;
			  _expander = expander;
		 }

		 public override ITraversalBranch Next( ITraversalContext metadata )
		 {
			  ITraversalBranch result = null;
			  while ( result == null )
			  {
					if ( _current == null )
					{
						 return null;
					}
					ITraversalBranch next = _current.next( _expander, metadata );
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