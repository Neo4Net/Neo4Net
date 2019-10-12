using System.Collections.Generic;

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
namespace Neo4Net.Graphdb.traversal
{

	using Neo4Net.Graphdb;

	/// <summary>
	/// Selects <seealso cref="TraversalBranch"/>s according to breadth first
	/// pattern, the most natural ordering in a breadth first search, see
	/// http://en.wikipedia.org/wiki/Breadth-first_search
	/// </summary>
	internal class PreorderBreadthFirstSelector : BranchSelector
	{
		 private readonly LinkedList<TraversalBranch> _queue = new LinkedList<TraversalBranch>();
		 private TraversalBranch _current;
		 private readonly PathExpander _expander;

		 internal PreorderBreadthFirstSelector( TraversalBranch startSource, PathExpander expander )
		 {
			  this._current = startSource;
			  this._expander = expander;
		 }

		 public override TraversalBranch Next( TraversalContext metadata )
		 {
			  TraversalBranch result = null;
			  while ( result == null )
			  {
					TraversalBranch next = _current.next( _expander, metadata );
					if ( next != null )
					{
						 _queue.AddLast( next );
						 result = next;
					}
					else
					{
						 _current = _queue.RemoveFirst();
						 if ( _current == null )
						 {
							  return null;
						 }
					}
			  }
			  return result;
		 }
	}

}