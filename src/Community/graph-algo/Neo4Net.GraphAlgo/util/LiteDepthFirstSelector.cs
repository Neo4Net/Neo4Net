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
namespace Neo4Net.GraphAlgo.Utils
{

	using Neo4Net.GraphDb;
	using BranchSelector = Neo4Net.GraphDb.Traversal.BranchSelector;
	using TraversalBranch = Neo4Net.GraphDb.Traversal.TraversalBranch;
	using TraversalContext = Neo4Net.GraphDb.Traversal.TraversalContext;

	/// <summary>
	/// A preorder depth first selector which detects "super nodes", i.e. nodes
	/// which has many relationships. It delays traversing those super nodes until
	/// after all non-super nodes have been traversed.
	/// 
	/// @author Mattias Persson
	/// @author Tobias Ivarsson
	/// </summary>
	public class LiteDepthFirstSelector : BranchSelector
	{
		 private readonly LinkedList<TraversalBranch> _superNodes = new LinkedList<TraversalBranch>();
		 private TraversalBranch _current;
		 private readonly int _threshold;
		 private readonly PathExpander _expander;

		 public LiteDepthFirstSelector( TraversalBranch startSource, int startThreshold, PathExpander expander )
		 {
			  this._current = startSource;
			  this._threshold = startThreshold;
			  this._expander = expander;
		 }

		 public override TraversalBranch Next( TraversalContext metadata )
		 {
			  TraversalBranch result = null;
			  while ( result == null )
			  {
					if ( _current == null )
					{
						 _current = _superNodes.RemoveFirst();
						 if ( _current == null )
						 {
							  return null;
						 }
					}
					else if ( _current.expanded() > 0 && _current.expanded() % _threshold == 0 )
					{
						 _superNodes.AddLast( _current );
						 _current = _current.parent();
						 continue;
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