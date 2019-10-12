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
	/// <summary>
	/// Provides a context for <seealso cref="TraversalBranch"/>es which they need to
	/// move further and report their progress.
	/// </summary>
	public interface TraversalContext : TraversalMetadata
	{
		 /// <summary>
		 /// Reports that one more relationship has been traversed in this
		 /// traversal.
		 /// </summary>
		 void RelationshipTraversed();

		 /// <summary>
		 /// Reports that one more relationship has been traversed, albeit
		 /// a relationship that hasn't provided any benefit to the traversal.
		 /// </summary>
		 void UnnecessaryRelationshipTraversed();

		 /// <summary>
		 /// Used for start branches to check adherence to the traversal uniqueness.
		 /// </summary>
		 /// <param name="branch"> the <seealso cref="TraversalBranch"/> to check for uniqueness. </param>
		 /// <returns> {@code true} if the branch is considered unique and is
		 /// allowed to progress in this traversal. </returns>
		 bool IsUniqueFirst( TraversalBranch branch );

		 /// <summary>
		 /// Used for all except branches to check adherence to the traversal
		 /// uniqueness.
		 /// </summary>
		 /// <param name="branch"> the <seealso cref="TraversalBranch"/> to check for uniqueness. </param>
		 /// <returns> {@code true} if the branch is considered unique and is
		 /// allowed to progress in this traversal. </returns>
		 bool IsUnique( TraversalBranch branch );

		 /// <summary>
		 /// Evaluates a <seealso cref="TraversalBranch"/> whether or not to include it in the
		 /// result and whether or not to continue further down this branch or not.
		 /// </summary>
		 /// <param name="branch"> the <seealso cref="TraversalBranch"/> to evaluate. </param>
		 /// <param name="state"> the <seealso cref="BranchState"/> for the branch. </param>
		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> an <seealso cref="Evaluation"/> of the branch in this traversal. </returns>
		 Evaluation evaluate<STATE>( TraversalBranch branch, BranchState<STATE> state );
	}

}