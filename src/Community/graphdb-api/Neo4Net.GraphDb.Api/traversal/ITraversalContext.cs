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
	/// Provides a context for <seealso cref="ITraversalBranch"/>es which they need to
	/// move further and report their progress.
	/// </summary>
	public interface ITraversalContext : ITraversalMetadata
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
		 /// <param name="branch"> the <seealso cref="ITraversalBranch"/> to check for uniqueness. </param>
		 /// <returns> {@code true} if the branch is considered unique and is
		 /// allowed to progress in this traversal. </returns>
		 bool IsUniqueFirst( ITraversalBranch branch );

		 /// <summary>
		 /// Used for all except branches to check adherence to the traversal
		 /// uniqueness.
		 /// </summary>
		 /// <param name="branch"> the <seealso cref="ITraversalBranch"/> to check for uniqueness. </param>
		 /// <returns> {@code true} if the branch is considered unique and is
		 /// allowed to progress in this traversal. </returns>
		 bool IsUnique( ITraversalBranch branch );

		 /// <summary>
		 /// Evaluates a <seealso cref="ITraversalBranch"/> whether or not to include it in the
		 /// result and whether or not to continue further down this branch or not.
		 /// </summary>
		 /// <param name="branch"> the <seealso cref="ITraversalBranch"/> to evaluate. </param>
		 /// <param name="state"> the <seealso cref="BranchState"/> for the branch. </param>
		 /// @param <STATE> the type of the state object. </param>
		 /// <returns> an <seealso cref="Evaluation"/> of the branch in this traversal. </returns>
		 Evaluation Evaluate<STATE>( ITraversalBranch branch, IBranchState<STATE> state );
	}

}