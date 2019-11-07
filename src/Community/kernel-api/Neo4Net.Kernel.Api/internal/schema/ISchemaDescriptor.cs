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
namespace Neo4Net.Kernel.Api.Internal.Schema
{

	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

	/// <summary>
	/// Internal representation of one schema unit, for example a label-property pair.
	/// 
	/// Even when only supporting a small set of different schemas, the number of common methods is very small. This
	/// interface therefore supports a visitor type access pattern, results can be computed using the {#compute} method, and
	/// side-effect type logic performed using the processWith method. This means that when implementing this interface
	/// with a new concrete type, the compute and processWith method implementations need to be added similarly to
	/// how this is done in eg. LabelSchemaDescriptor, and the SchemaProcessor and SchemaComputer interfaces need to be
	/// extended with methods taking the new concrete type as argument.
	/// </summary>
	public interface ISchemaDescriptor : ISchemaDescriptorSupplier
	{
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 SchemaDescriptor NO_SCHEMA = new SchemaDescriptor()
	//	 {
	//		  @@Override public boolean isAffected(long[] IEntityIds)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public <R> R computeWith(SchemaComputer<R> computer)
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public void processWith(SchemaProcessor processor)
	//		  {
	//
	//		  }
	//
	//		  @@Override public String userDescription(TokenNameLookup tokenNameLookup)
	//		  {
	//				return "NO_SCHEMA";
	//		  }
	//
	//		  @@Override public int[] getPropertyIds()
	//		  {
	//				return new int[0];
	//		  }
	//
	//		  @@Override public int[] getEntityTokenIds()
	//		  {
	//				return new int[0];
	//		  }
	//
	//		  @@Override public int keyId()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public ResourceType keyType()
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public EntityType EntityType()
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public PropertySchemaType propertySchemaType()
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public SchemaDescriptor schema()
	//		  {
	//				return null;
	//		  }
	//	 };

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static long[] schemaTokenLockingIds(SchemaDescriptor schema)
	//	 {
	//		  // TODO make getEntityTokenIds produce a long array directly, and avoid this extra copying.
	//		  return schemaTokenLockingIds(schema.getEntityTokenIds());
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static long[] schemaTokenLockingIds(int[] tokenIds)
	//	 {
	//		  long[] lockingIds = new long[tokenIds.length];
	//		  for (int i = 0; i < lockingIds.length; i++)
	//		  {
	//				lockingIds[i] = tokenIds[i];
	//		  }
	//		  return lockingIds;
	//	 }

		 /// <summary>
		 /// Returns true if any of the given IEntity token ids are part of this schema unit. </summary>
		 /// <param name="entityTokenIds"> IEntity token ids to check against. </param>
		 /// <returns> true if the supplied ids are relevant to this schema unit. </returns>
		 bool IsAffected( long[] IEntityTokenIds );

		 /// <summary>
		 /// This enum signifies how this schema should behave in regards to updates.
		 /// <seealso cref="PropertySchemaType.COMPLETE_ALL_TOKENS"/> signifies that this schema unit only should be affected by updates that match the entire schema,
		 /// i.e. when all properties are present. If you are unsure then this is probably what you want.
		 /// <seealso cref="PropertySchemaType.PARTIAL_ANY_TOKEN"/> signifies that this schema unit should be affected by any update that is partial match of the schema,
		 ///  i.e. at least one of the properties of this schema unit is present.
		 /// </summary>

		 /// <summary>
		 /// Computes some value by feeding this object into the given SchemaComputer.
		 /// 
		 /// Note that implementers of this method just need to call `return computer.compute( this );`.
		 /// </summary>
		 /// <param name="computer"> The SchemaComputer that hold the logic for the computation </param>
		 /// @param <R> The return type </param>
		 /// <returns> The result of the computation </returns>
		 R computeWith<R>( SchemaComputer<R> computer );

		 /// <summary>
		 /// Performs some side-effect type logic by processing this object using the given SchemaProcessor.
		 /// 
		 /// Note that implementers of this method just need to call `return processor.process( this );`.
		 /// </summary>
		 /// <param name="processor"> The SchemaProcessor that hold the logic for the computation </param>
		 void ProcessWith( SchemaProcessor processor );

		 /// <param name="tokenNameLookup"> used for looking up names for token ids. </param>
		 /// <returns> a user friendly description of what this index indexes. </returns>
		 string UserDescription( ITokenNameLookup tokenNameLookup );

		 /// <summary>
		 /// This method return the property ids that are relevant to this Schema Descriptor.
		 /// 
		 /// Putting this method here is a convenience that will break if/when we introduce more complicated schema
		 /// descriptors like paths, but until that point it is very useful.
		 /// </summary>
		 /// <returns> the property ids </returns>
		 int[] PropertyIds { get; }

		 /// <summary>
		 /// Assume that this schema descriptor describes a schema that includes a single property id, and return that id.
		 /// </summary>
		 /// <returns> The presumed single property id of this schema. </returns>
		 /// <exception cref="IllegalStateException"> if this schema does not have exactly one property. </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int getPropertyId()
	//	 {
	//		  int[] propertyIds = getPropertyIds();
	//		  if (propertyIds.length != 1)
	//		  {
	//				throw new IllegalStateException("Single property schema requires one property but had " + propertyIds.length);
	//		  }
	//		  return propertyIds[0];
	//	 }

		 /// <summary>
		 /// This method returns the IEntity token ids handled by this descriptor. </summary>
		 /// <returns> the IEntity token ids that this schema descriptor represents </returns>
		 int[] IEntityTokenIds { get; }

		 /// <summary>
		 /// Id of underlying schema descriptor key.
		 /// Key is part of schema unit that determines which resources with specified properties are applicable. </summary>
		 /// <returns> id of underlying key </returns>
		 int KeyId();

		 /// <summary>
		 /// Type of underlying schema descriptor key.
		 /// Key is part of schema unit that determines which resources with specified properties are applicable. </summary>
		 /// <returns> type of underlying key </returns>
		 ResourceType KeyType();

		 /// <summary>
		 /// Type of entities this schema represents. </summary>
		 /// <returns> IEntity type </returns>
		 EntityType EntityType();

		 /// <summary>
		 /// Returns the type of this schema. See <seealso cref="PropertySchemaType"/>. </summary>
		 /// <returns> PropertySchemaType of this schema unit. </returns>
		 SchemaDescriptor_PropertySchemaType PropertySchemaType();

		 /// <summary>
		 /// Create a predicate that checks whether a schema descriptor Supplier supplies the given schema descriptor. </summary>
		 /// <param name="descriptor"> The schema descriptor to check equality with. </param>
		 /// <returns> A predicate that returns {@code true} if it is given a schema descriptor supplier that supplies the
		 /// same schema descriptor as the given schema descriptor. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T> System.Predicate<T> equalTo(SchemaDescriptor descriptor)
	//	 {
	//		  return supplier -> descriptor.equals(supplier.schema());
	//	 }
	}

	 public enum SchemaDescriptor_PropertySchemaType
	 {
		  CompleteAllTokens,
		  PartialAnyToken
	 }

}