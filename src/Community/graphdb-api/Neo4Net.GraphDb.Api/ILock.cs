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

namespace Neo4Net.GraphDb
{
   /// <summary>
   /// An acquired lock on an IEntity for a transaction, acquired from
   /// <seealso cref="ITransaction.acquireWriteLock(PropertyContainer)"/> or
   /// <seealso cref="ITransaction.acquireReadLock(PropertyContainer)"/> this lock
   /// can be released manually using <seealso cref="release()"/>. If not released
   /// manually it will be automatically released when the transaction owning
   /// it finishes.
   ///
   /// @author Mattias Persson
   /// </summary>
   public interface ILock
   {
      /// <summary>
      /// Releases this lock before the transaction finishes. It is an optional
      /// operation and if not called, this lock will be released when the owning
      /// transaction finishes.
      /// </summary>
      /// <exception cref="IllegalStateException"> if this lock has already been released. </exception>
      void Release();
   }
}