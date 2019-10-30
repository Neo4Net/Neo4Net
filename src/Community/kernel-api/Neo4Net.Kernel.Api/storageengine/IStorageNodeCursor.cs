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
    /// ICursor over nodes and its data.
    /// </summary>
    public interface IStorageNodeCursor : IStorageEntityScanCursor
    {
        /// <returns> label ids of the node this Cursor currently is placed at. </returns>
        long[] Labels { get; }

        /// <returns> {@code true} if the node this ICursor is placed at has the given {@code label}, otherwise {@code false}. </returns>
        bool HasLabel(int label);

        /// <summary>
        /// NOTE the fact that this method is here means physical details about underlying storage leaks into this API.
        /// However this method has to exist as long as the kernel API also exposes this. This needs to change at some point.
        /// </summary>
        /// <returns> reference for reading relationship groups, i.e. relationships split up by direction/type of the node this ICursor currently is placed at. </returns>
        long RelationshipGroupReference { get; }

        /// <returns> reference for reading all relationships of the node this ICursor currently is placed at. </returns>
        long AllRelationshipsReference { get; }

        /// <summary>
        /// A means of simplifying higher-level cursors which takes into consideration transaction-state.
        /// This basically tells this ICursor to be placed at nodeReference, even if it doesn't exist, such that
        /// <seealso cref="entityReference()"/> will return this reference on the next call.
        /// </summary>
        /// <param name="nodeReference"> the reference to be returned on the next <seealso cref="entityReference()"/> call. </param>
        long Current { set; }

        /// <summary>
        /// NOTE the fact that this method is here means physical details about underlying storage leaks into this API.
        /// However this method has to exist as long as the kernel API also exposes this. This needs to change at some point.
        /// </summary>
        /// <returns> whether or not this node is dense. </returns>
        bool Dense { get; }
    }
}