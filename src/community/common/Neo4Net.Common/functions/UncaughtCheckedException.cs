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

namespace Neo4Net.Functions
{
   /// <summary>
   /// Wrapper around checked exceptions for rethrowing them as runtime exceptions when the signature of the containing method
   /// cannot be changed to declare them.
   ///
   /// Thrown by <seealso cref="ThrowingFunction.catchThrown(System.Type, ThrowingFunction)"/>
   /// </summary>
   public class UncaughtCheckedException : Exception
   {
      private readonly object _source;

      public UncaughtCheckedException(object source, Exception cause) : base("Uncaught checked exception", cause)
      {
         if (cause == null)
         {
            throw new System.ArgumentException("Expected non-null cause");
         }
         this._source = source;
      }

      /// <summary>
      /// Check that the cause has the given type and if successful, return it.
      /// </summary>
      /// <param name="clazz"> class object for the desired type of the cause </param>
      /// @param <E> the desired type of the cause </param>
      /// <returns> the underlying cause of this exception but only if it is of desired type E, nothing otherwise </returns>
      public virtual Optional<E> GetCauseIfOfType<E>(Type clazz) where E : Exception
      {
         clazz = typeof(E);
         Exception cause = Cause;
         if (clazz.IsInstanceOfType(cause))
         {
            return clazz.cast(cause);
         }
         else
         {
            return null;
         }
      }

      public virtual object Source()
      {
         return _source;
      }
   }
}