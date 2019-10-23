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
   using TooManyLabelsException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.TooManyLabelsException;

   public interface ITokenWrite
   {
      /// <summary>
      /// Returns a label id for a label name. If the label doesn't exist prior to
      /// this call it gets created.
      /// </summary>
      int LabelGetOrCreateForName(string labelName);

      /// <summary>
      /// Get or create the label token ids for each of the given {@code labelNames}, and store them at the corresponding
      /// index in the given {@code labelIds} array.
      ///
      /// This is effectively a batching version of <seealso cref="labelGetOrCreateForName(string)"/>.
      /// </summary>
      /// <param name="labelNames"> The array of label names for which to resolve or create their id. </param>
      /// <param name="labelIds"> The array into which the resulting token ids will be stored. </param>
      /// <exception cref="TooManyLabelsException"> if too many labels would bve created by this call, compared to the token id space
      /// available. </exception>
      void LabelGetOrCreateForNames(string[] labelNames, int[] labelIds);

      /// <summary>
      /// Creates a label with the given name.
      /// </summary>
      /// <param name="labelName"> the name of the label. </param>
      /// <returns> id of the created label. </returns>
      int LabelCreateForName(string labelName);

      /// <summary>
      /// Creates a property token with the given name.
      /// </summary>
      /// <param name="propertyKeyName"> the name of the property. </param>
      /// <returns> id of the created property key. </returns>
      int PropertyKeyCreateForName(string propertyKeyName);

      /// <summary>
      /// Creates a relationship type with the given name. </summary>
      /// <param name="relationshipTypeName"> the name of the relationship. </param>
      /// <returns> id of the created relationship type. </returns>
      int RelationshipTypeCreateForName(string relationshipTypeName);

      /// <summary>
      /// Returns a property key id for a property key. If the key doesn't exist prior to
      /// this call it gets created.
      /// </summary>
      int PropertyKeyGetOrCreateForName(string propertyKeyName);

      /// <summary>
      /// Get or create the property token ids for each of the given {@code propertyKeys}, and store them at the
      /// corresponding index in the given {@code ids} array.
      ///
      /// This is effectively a batching version of <seealso cref="propertyKeyGetOrCreateForName(string)"/>.
      /// </summary>
      /// <param name="propertyKeys"> The array of property names for which to resolve or create their id. </param>
      /// <param name="ids"> The array into which the resulting token ids will be stored. </param>
      void PropertyKeyGetOrCreateForNames(string[] propertyKeys, int[] ids);

      /// <summary>
      /// Returns the id associated with the relationship type or creates a new one. </summary>
      /// <param name="relationshipTypeName"> the name of the relationship </param>
      /// <returns> the id associated with the name </returns>
      int RelationshipTypeGetOrCreateForName(string relationshipTypeName);

      /// <summary>
      /// Get or create the relationship type token ids for each of the given {@code relationshipTypes}, and store them at
      /// the corresponding index in the given {@code ids} array.
      ///
      /// This is effectively a batching version of <seealso cref="relationshipTypeGetOrCreateForName(string)"/>.
      /// </summary>
      /// <param name="relationshipTypes"> The array of relationship type names for which to resolve or create their id. </param>
      /// <param name="ids"> The array into which the resulting token ids will be stored. </param>
      void RelationshipTypeGetOrCreateForNames(string[] relationshipTypes, int[] ids);
   }
}