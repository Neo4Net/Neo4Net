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
   using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;

   /// <summary>
   /// Reference to a specific index together with it's capabilities. This reference is valid until the schema of the database changes
   /// (that is a create/drop of an index or constraint occurs).
   /// </summary>
   public interface IIndexReference : IIndexCapability
   {
      /// <summary>
      /// Returns true if this index only allows one value per key.
      /// </summary>
      bool Unique { get; }

      /// <summary>
      /// Returns the propertyKeyIds associated with this index.
      /// </summary>
      int[] Properties();

      /// <summary>
      /// Returns the schema of this index.
      /// </summary>
      SchemaDescriptor Schema();

      /// <summary>
      /// Returns the key (or name) of the index provider that backs this index.
      /// </summary>
      string ProviderKey();

      /// <summary>
      /// Returns the version of the index provider that backs this index.
      /// </summary>
      string ProviderVersion();

      /// <summary>
      /// The unique name for this index - either automatically generated or user supplied - or the <seealso cref="UNNAMED_INDEX"/> constant.
      /// </summary>
      string Name();

      /// <param name="tokenNameLookup"> used for looking up names for token ids. </param>
      /// <returns> a user friendly description of what this index indexes. </returns>
      string UserDescription(ITokenNameLookup tokenNameLookup);

      /// <summary>
      /// Sorts indexes by type, returning first GENERAL indexes, followed by UNIQUE. Implementation is not suitable in
      /// hot path.
      /// </summary>
      /// <param name="indexes"> Indexes to sort </param>
      /// <returns> sorted indexes </returns>
      //JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
      //		 static java.util.Iterator<IndexReference> sortByType(java.util.Iterator<IndexReference> indexes)
      //	 {
      //		  List<IndexReference> materialized = Iterators.asList(indexes);
      //		  return Iterators.concat(Iterators.filter(i -> !i.isUnique(), materialized.iterator()), Iterators.filter(IndexReference::isUnique, materialized.iterator()));
      //
      //	 }

      //JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
      //	 IndexReference NO_INDEX = new IndexReference()
      //	 {
      //		  @@Override public IndexOrder[] orderCapability(ValueCategory... valueCategories)
      //		  {
      //				return NO_CAPABILITY.orderCapability(valueCategories);
      //		  }
      //
      //		  @@Override public IndexValueCapability valueCapability(ValueCategory... valueCategories)
      //		  {
      //				return NO_CAPABILITY.valueCapability(valueCategories);
      //		  }
      //
      //		  @@Override public boolean isFulltextIndex()
      //		  {
      //				return false;
      //		  }
      //
      //		  @@Override public boolean isEventuallyConsistent()
      //		  {
      //				return false;
      //		  }
      //
      //		  @@Override public boolean isUnique()
      //		  {
      //				return false;
      //		  }
      //
      //		  @@Override public int[] properties()
      //		  {
      //				return new int[0];
      //		  }
      //
      //		  @@Override public SchemaDescriptor schema()
      //		  {
      //				return SchemaDescriptor.NO_SCHEMA;
      //		  }
      //
      //		  @@Override public String providerKey()
      //		  {
      //				return null;
      //		  }
      //
      //		  @@Override public String providerVersion()
      //		  {
      //				return null;
      //		  }
      //
      //		  @@Override public String name()
      //		  {
      //				return UNNAMED_INDEX;
      //		  }
      //
      //		  @@Override public String userDescription(TokenNameLookup tokenNameLookup)
      //		  {
      //				return SchemaDescriptor.NO_SCHEMA.userDescription(tokenNameLookup);
      //		  }
      //	 };
   }

   public static class IndexReference_Fields
   {
      public const string UNNAMED_INDEX = "Unnamed index";
   }
}