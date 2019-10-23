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
namespace Neo4Net.GraphDb
{
	using Neo4Net.GraphDb.Traversal;

	/// <summary>
	/// An expander of relationships. It's a flexible way of getting relationships
	/// from a <seealso cref="IPath"/>. Given a path, which relationships should be expanded
	/// from it to traverse further.
	/// </summary>
	public interface IPathExpander<STATE>
	{
		 /// <summary>
		 /// Returns relationships for a <seealso cref="IPath"/>, most commonly from the
		 /// <seealso cref="IPath.endNode()"/>.
		 /// </summary>
		 /// <param name="path"> the path to expand (most commonly the end node). </param>
		 /// <param name="state"> the state of this branch in the current traversal.
		 /// <seealso cref="BranchState.getState()"/> returns the state and
		 /// <seealso cref="BranchState.setState(object)"/> optionally sets the state for
		 /// the children of this branch. If state isn't altered the children
		 /// of this path will see the state of the parent. </param>
		 /// <returns> the relationships to return for the {@code path}. </returns>
		 IEnumerable<IRelationship> Expand( IPath path, BranchState<STATE> state );

		 /// <summary>
		 /// Returns a new instance with the exact expansion logic, but reversed.
		 /// </summary>
		 /// <returns> a reversed <seealso cref="PathExpander"/>. </returns>
		 IPathExpander<STATE> Reverse();
	}

}