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
	/// <summary>
	/// A factory for <seealso cref="SideSelector"/>s. Used during bidirectional traversals.
	/// </summary>
	public interface SideSelectorPolicy
	{
		 /// <summary>
		 /// Creates a new <seealso cref="SideSelector"/> given the {@code start}/{@code end}
		 /// <seealso cref="BranchSelector"/>s and an optional {@code maxDepth}.
		 /// </summary>
		 /// <param name="start"> the start side <seealso cref="BranchSelector"/> of this
		 /// bidirectional traversal. </param>
		 /// <param name="end"> the end side <seealso cref="BranchSelector"/> of this
		 /// bidirectional traversal. </param>
		 /// <param name="maxDepth"> an optional max depth the combined traversal depth must
		 /// be kept within. Optional in the sense that only some implementations
		 /// honors it. </param>
		 /// <returns> a new <seealso cref="SideSelector"/> for {@code start} and {@code end}
		 /// <seealso cref="BranchSelector"/>s. </returns>
		 SideSelector Create( BranchSelector start, BranchSelector end, int maxDepth );
	}

}