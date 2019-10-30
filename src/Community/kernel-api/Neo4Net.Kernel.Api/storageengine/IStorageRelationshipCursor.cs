using System;

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

namespace Neo4Net.Kernel.Api.StorageEngine
{
    /// <summary>
    /// Shared interface between the two <seealso cref="StorageRelationshipScanCursor"/> and <seealso cref="StorageRelationshipTraversalCursor"/>.
    /// </summary>
    public interface IStorageRelationshipCursor : IRelationshipVisitor<Exception>, IStorageEntityCursor
    {
        /// <returns> relationship type of the relationship this ICursor is placed at. </returns>
        int Type { get; }

        /// <returns> source node of the relationship this ICursor is placed at. </returns>
        long SourceNodeReference { get; }

        /// <returns> target node of the relationship this ICursor is placed at. </returns>
        long TargetNodeReference { get; }

        /// <summary>
        /// Used to visit transaction state, for simplifying implementation of higher-level ICursor that consider transaction-state.
        /// </summary>
        void Visit(long relationshipId, int typeId, long startNodeId, long endNodeId);
    }
}