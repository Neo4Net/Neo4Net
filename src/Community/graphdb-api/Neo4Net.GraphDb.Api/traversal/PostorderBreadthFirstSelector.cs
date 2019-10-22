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
namespace Neo4Net.GraphDb.traversal
{

	using Neo4Net.GraphDb;

	/// <summary>
	/// Selects <seealso cref="TraversalBranch"/>s according to postorder breadth first
	/// pattern which basically is a reverse to preorder breadth first in that
	/// deepest levels are returned first, see
	/// http://en.wikipedia.org/wiki/Breadth-first_search
	/// </summary>
	internal class PostorderBreadthFirstSelector : BranchSelector
	{
		 private IEnumerator<TraversalBranch> _sourceIterator;
		 private readonly TraversalBranch _current;
		 private readonly IPathExpander _expander;

		 internal PostorderBreadthFirstSelector( TraversalBranch startSource, IPathExpander expander )
		 {
			  this._current = startSource;
			  this._expander = expander;
		 }

		 public override TraversalBranch Next( TraversalContext metadata )
		 {
			  if ( _sourceIterator == null )
			  {
					_sourceIterator = GatherSourceIterator( metadata );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _sourceIterator.hasNext() ? _sourceIterator.next() : null;
		 }

		 private IEnumerator<TraversalBranch> GatherSourceIterator( TraversalContext metadata )
		 {
			  LinkedList<TraversalBranch> queue = new LinkedList<TraversalBranch>();
			  queue.AddLast( _current.next( _expander, metadata ) );
			  while ( true )
			  {
					IList<TraversalBranch> level = GatherOneLevel( queue, metadata );
					if ( level.Count == 0 )
					{
						 break;
					}
					queue.addAll( 0, level );
			  }
			  return queue.GetEnumerator();
		 }

		 private IList<TraversalBranch> GatherOneLevel( IList<TraversalBranch> queue, TraversalContext metadata )
		 {
			  IList<TraversalBranch> level = new LinkedList<TraversalBranch>();
			  int? depth = null;
			  foreach ( TraversalBranch source in queue )
			  {
					if ( depth == null )
					{
						 depth = source.Length();
					}
					else if ( source.Length() != depth.Value )
					{
						 break;
					}

					while ( true )
					{
						 TraversalBranch next = source.Next( _expander, metadata );
						 if ( next == null )
						 {
							  break;
						 }
						 level.Add( next );
					}
			  }
			  return level;
		 }
	}

}