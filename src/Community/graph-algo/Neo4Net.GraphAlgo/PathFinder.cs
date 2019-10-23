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
namespace Neo4Net.GraphAlgo
{
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using TraversalMetadata = Neo4Net.GraphDb.Traversal.TraversalMetadata;

	/// <summary>
	/// Interface of algorithms that finds paths in between two nodes.
	/// 
	/// @author Tobias Ivarsson
	/// </summary>
	/// @param <P> the path type that the algorithm produces </param>
	public interface PathFinder<P> where P : Neo4Net.GraphDb.Path
	{
		 /// <summary>
		 /// Tries to find a single path between {@code start} and {@code end}
		 /// nodes. If a path is found a <seealso cref="Path"/> is returned with that path
		 /// information, else {@code null} is returned. If more than one path is
		 /// found, the implementation can decide itself upon which of those to return.
		 /// </summary>
		 /// <param name="start"> the start <seealso cref="Node"/> which defines the start of the path. </param>
		 /// <param name="end"> the end <seealso cref="Node"/> which defines the end of the path. </param>
		 /// <returns> a single <seealso cref="Path"/> between {@code start} and {@code end},
		 /// or {@code null} if no path was found. </returns>
		 P FindSinglePath( Node start, Node end );

		 /// <summary>
		 /// Tries to find all paths between {@code start} and {@code end} nodes.
		 /// A collection of <seealso cref="Path"/>s is returned with all the found paths.
		 /// If no paths are found an empty collection is returned.
		 /// </summary>
		 /// <param name="start"> the start <seealso cref="Node"/> which defines the start of the path. </param>
		 /// <param name="end"> the end <seealso cref="Node"/> which defines the end of the path. </param>
		 /// <returns> all <seealso cref="Path"/>s between {@code start} and {@code end}. </returns>
		 IEnumerable<P> FindAllPaths( Node start, Node end );

		 TraversalMetadata Metadata();
	}

}