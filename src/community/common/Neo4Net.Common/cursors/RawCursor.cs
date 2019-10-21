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

namespace Neo4Net.Cursors
{
   /// <summary>
   /// A cursor is an object that moves to point to different locations in a data structure.
   /// The abstraction originally comes from mechanical slide rules, which have a "cursor" which
   /// slides to point to different positions on the ruler.
   /// <para>
   /// Each position a cursor points to is referred to as a "row".
   /// </para>
   /// <para>
   /// Access to the current row is done by subtyping this interface and adding accessor methods. If no call to
   /// <seealso cref="next()"/> has been done, or if it returned false, then such accessor methods throw {@link
   /// IllegalStateException}.
   /// </para>
   /// </summary>
   public interface IRawCursor<T, EXCEPTION> : System.Func<T>, IDisposable where EXCEPTION : Exception
   {
      /// <summary>
      /// Move the cursor to the next row.
      /// Return false if there are no more valid positions, generally indicating that the end of the data structure
      /// has been reached.
      /// </summary>

      bool Next();

      /// <summary>
      /// Signal that the cursor is no longer needed.
      /// </summary>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void close() throws EXCEPTION;
      void Close();

      //JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
      //		 default void forAll(System.Action<T> consumer) throws EXCEPTION
      //	 {
      //		  try
      //		  {
      //				while (next())
      //				{
      //					 consumer.accept(get());
      //				}
      //		  }
      //		  finally
      //		  {
      //				close();
      //		  }
      //	 }
   }
}