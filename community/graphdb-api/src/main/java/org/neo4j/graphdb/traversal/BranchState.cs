﻿/*
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
	/// <summary>
	/// Accessor for a state associated with a <seealso cref="TraversalBranch"/> during a
	/// traversal. A <seealso cref="TraversalBranch"/> can have an associated state which
	/// follows down the branch as the traversal goes. If the state is modified
	/// with <seealso cref="setState(object)"/> it means that branches further down
	/// will have the newly set state, until it potentially gets overridden
	/// again. The state returned from <seealso cref="getState()"/> represents the state
	/// associated with the parent branch, which by this point has followed down
	/// to the branch calling <seealso cref="getState()"/>.
	/// </summary>
	/// @param <STATE> the type of object the state is. </param>
	public interface BranchState<STATE>
	{
		 /// <returns> the associated state for a <seealso cref="TraversalBranch"/>. </returns>
		 STATE State { get;set; }


		 /// <summary>
		 /// Instance representing no state, usage resulting in
		 /// <seealso cref="System.InvalidOperationException"/> being thrown.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 BranchState NO_STATE = new BranchState()
	//	 {
	//		  @@Override public Object getState()
	//		  {
	//				throw new IllegalStateException("Branch state disabled, pass in an initial state to enable it");
	//		  }
	//
	//		  @@Override public void setState(Object state)
	//		  {
	//				throw new IllegalStateException("Branch state disabled, pass in an initial state to enable it");
	//		  }
	//	 };
	}

}