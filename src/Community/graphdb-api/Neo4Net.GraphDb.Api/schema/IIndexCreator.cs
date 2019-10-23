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

namespace Neo4Net.GraphDb.Schema
{
   /// <summary>
   /// A builder for entering details about an index to create. After all details have been entered
   /// <seealso cref="create()"/> must be called for the index to actually be created. An index creator knows
   /// which <seealso cref="ILabel label"/> it is to be created for.
   /// <para>
   /// All methods except <seealso cref="create()"/> will return an <seealso cref="IIndexCreator"/> which should be
   /// used for further interaction.
   ///
   /// </para>
   /// </summary>
   /// <seealso cref= ISchema </seealso>
   public interface IIndexCreator
   {
      /// <summary>
      /// Includes the given {@code propertyKey} in this index, such that <seealso cref="INode nodes"/> with
      /// the assigned <seealso cref="ILabel label"/> and this property key will have its values indexed.
      /// <para>
      /// NOTE: currently only a single property key per index is supported.
      ///
      /// </para>
      /// </summary>
      /// <param name="propertyKey"> the property key to include in this index to be created. </param>
      /// <returns> an <seealso cref="IIndexCreator"/> instance to be used for further interaction. </returns>
      IIndexCreator On(string propertyKey);

      /// <summary>
      /// Assign a name to the index, which will then be returned from <seealso cref="IIndexDefinition.getName()"/>, and can be used for finding the index with
      /// <seealso cref="ISchema.getIndexByName(string)"/>.
      /// </summary>
      /// <param name="indexName"> the name to give the index. </param>
      /// <returns> an <seealso cref="IIndexCreator"/> instance to be used for further interaction. </returns>
      IIndexCreator WithName(string indexName);

      /// <summary>
      /// Creates an index with the details specified by the other methods in this interface.
      /// </summary>
      /// <returns> the created <seealso cref="IIndexDefinition index"/>. </returns>
      /// <exception cref="ConstraintViolationException"> if creating this index would violate one or more constraints. </exception>

      IIIndexDefinition Create();
   }
}