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
namespace Neo4Net.Kernel.Api.Internal
{
	/// <summary>
	/// ICursor for traversing the relationships of a single node.
	/// </summary>
	public interface IRelationshipTraversalCursor : IRelationshipDataAccessor, ISuspendableCursor<RelationshipTraversalCursor_Position>
	{

		 /// <summary>
		 /// Get the other node, the one that this ICursor was not initialized from.
		 /// <para>
		 /// Relationship cursors have context, and know which node they are traversing relationships for, making it
		 /// possible and convenient to access the other node.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> the ICursor to use for accessing the other node. </param>
		 void Neighbour( INodeCursor ICursor );

		 long NeighbourNodeReference();

		 long OriginNodeReference();
	}

	 public abstract class RelationshipTraversalCursor_Position : CursorPosition<RelationshipTraversalCursor_Position>
	 {
	 }

}