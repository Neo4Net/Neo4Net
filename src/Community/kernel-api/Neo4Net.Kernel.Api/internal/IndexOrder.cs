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
   /// Enum used for two purposes:
   /// 1. As return value for <seealso cref="IIndexCapability.orderCapability(Neo4Net.values.storable.ValueCategory...)"/>.
   /// Only <seealso cref="ASCENDING"/> and <seealso cref="DESCENDING"/> is valid for this.
   /// 2. As parameter for <seealso cref="IRead.nodeIndexScan(IIndexReference, INodeValueIndexCursor, IndexOrder, bool)"/> and
   /// <seealso cref="IRead.nodeIndexSeek(IIndexReference, INodeValueIndexCursor, IndexOrder, bool, IndexQuery...)"/>. Where <seealso cref="NONE"/> is used when
   /// no ordering is available or required.
   /// </summary>
   public enum IndexOrder
   {
      Ascending,
      Descending,
      None
   }
}