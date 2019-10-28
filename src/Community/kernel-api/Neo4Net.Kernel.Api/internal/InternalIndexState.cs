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

namespace Neo4Net.Kernel.Api.Internal
{
   /// <summary>
   /// Defines the state of a given index.
   /// </summary>
   public enum InternalIndexState
   {
      /// <summary>
      /// Denotes that an index is in the process of being created.
      /// </summary>
      Populating,

      /// <summary>
      /// Given after the database has populated the index, and notified the index provider that the index is in
      /// fact populated.
      /// </summary>
      Online,

      /// <summary>
      /// Denotes that the index, for one reason or another, is broken. Information about the
      /// failure is expected to have been logged.
      ///
      /// Dropping a failed index should be possible, as long as the failure is not caused by eg. out of memory.
      /// </summary>
      Failed
   }
}