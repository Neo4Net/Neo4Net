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
namespace Org.Neo4j.@internal.Kernel.Api
{
	/// <summary>
	/// Cursor for traversing the relationships of a single node.
	/// </summary>
	public interface RelationshipTraversalCursor : RelationshipDataAccessor, SuspendableCursor<RelationshipTraversalCursor_Position>
	{

		 /// <summary>
		 /// Get the other node, the one that this cursor was not initialized from.
		 /// <para>
		 /// Relationship cursors have context, and know which node they are traversing relationships for, making it
		 /// possible and convenient to access the other node.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> the cursor to use for accessing the other node. </param>
		 void Neighbour( NodeCursor cursor );

		 long NeighbourNodeReference();

		 long OriginNodeReference();
	}

	 public abstract class RelationshipTraversalCursor_Position : CursorPosition<RelationshipTraversalCursor_Position>
	 {
	 }

}