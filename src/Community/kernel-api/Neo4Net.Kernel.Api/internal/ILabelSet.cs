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
   /// Set of label ids.
   ///
   /// Modifications are not reflected in the LabelSet and there is no guaranteed
   /// order.
   /// </summary>
   public interface ILabelSet
   {
      int NumberOfLabels();

      int Label(int offset);

      bool Contains(int labelToken);

      long[] All();

      //JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
      //	 LabelSet NONE = new LabelSet()
      //	 {
      //		  private final long[] EMPTY = new long[0];
      //
      //		  @@Override public int numberOfLabels()
      //		  {
      //				return 0;
      //		  }
      //
      //		  @@Override public int label(int offset)
      //		  {
      //				throw new NoSuchElementException();
      //		  }
      //
      //		  @@Override public boolean contains(int labelToken)
      //		  {
      //				return false;
      //		  }
      //
      //		  @@Override public long[] all()
      //		  {
      //				return EMPTY;
      //		  }
      //	 };
   }
}