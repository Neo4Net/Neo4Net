﻿/*
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
    using Value = Neo4Net.Values.Storable.Value;
    using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

    /// <summary>
    /// ICursor that can read property data.
    /// </summary>
    public interface IStoragePropertyCursor : IStorageCursor
    {
        /// <summary>
        /// Initializes this Cursor to that reading property data at the given {@code reference}.
        /// </summary>
        /// <param name="reference"> reference to start reading properties at. </param>
        void Init(long reference);

        /// <returns> property key of the property this ICursor currently is placed at. </returns>
        int PropertyKey();

        /// <returns> value group of the property this ICursor currently is placed at. </returns>
        ValueGroup PropertyType { get; }

        /// <returns> value of the property this ICursor currently is placed at. </returns>
        Value PropertyValue { get; }
    }
}