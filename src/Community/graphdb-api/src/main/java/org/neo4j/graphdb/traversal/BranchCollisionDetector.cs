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

	/// <summary>
	/// In a bidirectional traversal there's one traversal from each start/end side and
	/// they will probably meet somewhere in the middle and the full paths are formed.
	/// This is where that detection and path generation takes place.
	/// </summary>
	public interface BranchCollisionDetector
	{
		 /// <summary>
		 /// Evaluate the given {@code branch} coming from either the start side or the
		 /// end side. Which side the branch represents is controlled by the {@code direction}
		 /// argument, <seealso cref="Direction.OUTGOING"/> means the start side and <seealso cref="Direction.INCOMING"/>
		 /// means the end side. Returns an <seealso cref="System.Collections.IEnumerable"/> of new unique <seealso cref="Path"/>s if
		 /// this branch resulted in a collision with other previously registered branches,
		 /// or {@code null} if this branch didn't result in any collision.
		 /// </summary>
		 /// <param name="branch"> the <seealso cref="TraversalBranch"/> to check for collision with other
		 /// previously registered branches. </param>
		 /// <param name="direction"> <seealso cref="Direction.OUTGOING"/> if this branch represents a branch
		 /// from the start side of this bidirectional traversal, or <seealso cref="Direction.INCOMING"/>
		 /// for the end side. </param>
		 /// <returns> new paths formed if this branch collided with other branches,
		 /// or {@code null} if no collision occurred. </returns>
		 IEnumerable<Path> Evaluate( TraversalBranch branch, Direction direction );
	}

}