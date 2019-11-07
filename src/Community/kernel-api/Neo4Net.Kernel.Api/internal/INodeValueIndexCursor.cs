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
   using Value = Neo4Net.Values.Storable.Value;

   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.values.storable.Values.NO_VALUE;

   /// <summary>
   /// ICursor for scanning the property values of nodes in a schema index.
   /// <para>
   /// Usage pattern:
   /// <pre><code>
   ///     int nbrOfProps = cursor.numberOfProperties();
   ///
   ///     Value[] values = new Value[nbrOfProps];
   ///     while ( cursor.next() )
   ///     {
   ///         if ( cursor.hasValue() )
   ///         {
   ///             for ( int i = 0; i < nbrOfProps; i++ )
   ///             {
   ///                 values[i] = cursor.propertyValue( i );
   ///             }
   ///         }
   ///         else
   ///         {
   ///             values[i] = getPropertyValueFromStore( cursor.nodeReference(), cursor.propertyKey( i ) )
   ///         }
   ///
   ///         doWhatYouWantToDoWith( values );
   ///     }
   /// </code></pre>
   /// </para>
   /// </summary>
   public interface INodeValueIndexCursor : INodeIndexCursor
   {
      /// <returns> the number of properties accessible within the index, and thus from this cursor. </returns>
      int NumberOfProperties();

      int PropertyKey(int offset);

      /// <summary>
      /// Check before trying to access values with <seealso cref="propertyValue(int)"/>. Result can change with each call to <seealso cref="next()"/>.
      /// </summary>
      /// <returns> {@code true} if <seealso cref="propertyValue(int)"/> can be used to get property value on cursor's current location,
      /// else {@code false}. </returns>
      bool HasValue();

      Value PropertyValue(int offset);
   }

   public static class NodeValueIndexCursor_Fields
   {
      public static readonly INodeValueIndexCursor Empty = new Empty();
   }

   public class NodeValueIndexCursor_Empty : INodeValueIndexCursor
   {
      public void Node(INodeCursor ICursor)
      {
      }

      public long NodeReference()
      {
         return -1L;
      }

      public bool Next()
      {
         return false;
      }

      public void Close()
      {
      }

      public virtual bool Closed
      {
         get
         {
            return false;
         }
      }

      public int NumberOfProperties()
      {
         return 0;
      }

      public int PropertyKey(int offset)
      {
         return -1;
      }

      public bool HasValue()
      {
         return false;
      }

      public Value PropertyValue(int offset)
      {
         return NO_VALUE;
      }
   }
}