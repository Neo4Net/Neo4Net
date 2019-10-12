﻿using System;

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
namespace Neo4Net.Storageengine.Api
{
	/// <summary>
	/// Shared interface between the two <seealso cref="StorageRelationshipScanCursor"/> and <seealso cref="StorageRelationshipTraversalCursor"/>.
	/// </summary>
	public interface StorageRelationshipCursor : RelationshipVisitor<Exception>, StorageEntityCursor
	{
		 /// <returns> relationship type of the relationship this cursor is placed at. </returns>
		 int Type();

		 /// <returns> source node of the relationship this cursor is placed at. </returns>
		 long SourceNodeReference();

		 /// <returns> target node of the relationship this cursor is placed at. </returns>
		 long TargetNodeReference();

		 /// <summary>
		 /// Used to visit transaction state, for simplifying implementation of higher-level cursor that consider transaction-state.
		 /// </summary>
		 void Visit( long relationshipId, int typeId, long startNodeId, long endNodeId );
	}

}