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
   /// A position is immutable and re-usable indefinitely.
   /// </summary>
   /// @param <Position> the specific subtype used by a particular cursor. </param>
   internal abstract class CursorPosition<Position> where Position : CursorPosition<Position>
   {
      private Position _tail;

      public Position Push(Position head)
      {
         ((CursorPosition<Position>)head)._tail = this;
         return head;
      }

      public Position Tail()
      {
         return _tail;
      }
   }
}