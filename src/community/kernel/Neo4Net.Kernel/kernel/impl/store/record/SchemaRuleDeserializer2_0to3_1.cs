using System;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Store.Records
{

	using MalformedSchemaRuleException = Neo4Net.Internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Neo4Net.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using SchemaRule_Kind = Neo4Net.Storageengine.Api.schema.SchemaRule_Kind;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@string.UTF8.getDecodedStringFrom;

	/// <summary>
	/// Deserializes SchemaRules from a ByteBuffer.
	/// </summary>
	public class SchemaRuleDeserializer2_0to3_1
	{
		 private const long? NO_OWNED_INDEX_RULE = null;

		 private SchemaRuleDeserializer2_0to3_1()
		 {
		 }

		 internal static bool IsLegacySchemaRule( sbyte schemaRuleType )
		 {
			  return schemaRuleType >= 1 && schemaRuleType <= SchemaRule_Kind.values().length;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static org.neo4j.storageengine.api.schema.SchemaRule deserialize(long id, int labelId, byte kindByte, ByteBuffer buffer) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 internal static SchemaRule Deserialize( long id, int labelId, sbyte kindByte, ByteBuffer buffer )
		 {
			  SchemaRule_Kind kind = SchemaRule_Kind.forId( kindByte );
			  try
			  {
					SchemaRule rule = NewRule( kind, id, labelId, buffer );
					if ( null == rule )
					{
						 throw new MalformedSchemaRuleException( null, "Deserialized null schema rule for id %d with kind %s", id, kind.name() );
					}
					return rule;
			  }
			  catch ( Exception e )
			  {
					throw new MalformedSchemaRuleException( e, "Could not deserialize schema rule for id %d with kind %s", id, kind.name() );
			  }
		 }

		 private static SchemaRule NewRule( SchemaRule_Kind kind, long id, int labelId, ByteBuffer buffer )
		 {
			  switch ( kind.innerEnumValue )
			  {
			  case SchemaRule_Kind.InnerEnum.INDEX_RULE:
					return ReadIndexRule( id, false, labelId, buffer );
			  case SchemaRule_Kind.InnerEnum.CONSTRAINT_INDEX_RULE:
					return ReadIndexRule( id, true, labelId, buffer );
			  case SchemaRule_Kind.InnerEnum.UNIQUENESS_CONSTRAINT:
					return ReadUniquenessConstraintRule( id, labelId, buffer );
			  case SchemaRule_Kind.InnerEnum.NODE_PROPERTY_EXISTENCE_CONSTRAINT:
					return ReadNodePropertyExistenceConstraintRule( id, labelId, buffer );
			  case SchemaRule_Kind.InnerEnum.RELATIONSHIP_PROPERTY_EXISTENCE_CONSTRAINT:
					return ReadRelPropertyExistenceConstraintRule( id, labelId, buffer );
			  default:
					throw new System.ArgumentException( kind.name() );
			  }
		 }

		 // === INDEX RULES ===

		 private static StoreIndexDescriptor ReadIndexRule( long id, bool constraintIndex, int label, ByteBuffer serialized )
		 {
			  IndexProviderDescriptor providerDescriptor = ReadIndexProviderDescriptor( serialized );
			  int[] propertyKeyIds = ReadIndexPropertyKeys( serialized );
			  LabelSchemaDescriptor schema = SchemaDescriptorFactory.forLabel( label, propertyKeyIds );
			  Optional<string> name = null;
			  IndexDescriptor descriptor = constraintIndex ? IndexDescriptorFactory.uniqueForSchema( schema, name, providerDescriptor ) : IndexDescriptorFactory.forSchema( schema, name, providerDescriptor );
			  StoreIndexDescriptor storeIndexDescriptor = constraintIndex ? descriptor.WithIds( id, ReadOwningConstraint( serialized ) ) : descriptor.WithId( id );
			  return storeIndexDescriptor;
		 }

		 private static IndexProviderDescriptor ReadIndexProviderDescriptor( ByteBuffer serialized )
		 {
			  string providerKey = getDecodedStringFrom( serialized );
			  string providerVersion = getDecodedStringFrom( serialized );
			  return new IndexProviderDescriptor( providerKey, providerVersion );
		 }

		 private static int[] ReadIndexPropertyKeys( ByteBuffer serialized )
		 {
			  // Currently only one key is supported although the data format supports multiple
			  int count = serialized.Short;
			  Debug.Assert( count >= 1 );

			  // Changed from being a long to an int 2013-09-10, but keeps reading a long to not change the store format.
			  int[] props = new int[count];
			  for ( int i = 0; i < count; i++ )
			  {
					props[i] = safeCastLongToInt( serialized.Long );
			  }
			  return props;
		 }

		 private static long ReadOwningConstraint( ByteBuffer serialized )
		 {
			  return serialized.Long;
		 }

		 // === CONSTRAINT RULES ===

		 public static ConstraintRule ReadUniquenessConstraintRule( long id, int labelId, ByteBuffer buffer )
		 {
			  return new ConstraintRule( id, ConstraintDescriptorFactory.uniqueForLabel( labelId, ReadConstraintPropertyKeys( buffer ) ), ReadOwnedIndexRule( buffer ) );
		 }

		 public static ConstraintRule ReadNodePropertyExistenceConstraintRule( long id, int labelId, ByteBuffer buffer )
		 {
			  return new ConstraintRule( id, ConstraintDescriptorFactory.existsForLabel( labelId, ReadPropertyKey( buffer ) ), NO_OWNED_INDEX_RULE );
		 }

		 public static ConstraintRule ReadRelPropertyExistenceConstraintRule( long id, int relTypeId, ByteBuffer buffer )
		 {
			  return new ConstraintRule( id, ConstraintDescriptorFactory.existsForRelType( relTypeId, ReadPropertyKey( buffer ) ), NO_OWNED_INDEX_RULE );
		 }

		 private static int ReadPropertyKey( ByteBuffer buffer )
		 {
			  return buffer.Int;
		 }

		 private static int[] ReadConstraintPropertyKeys( ByteBuffer buffer )
		 {
			  int[] keys = new int[buffer.get()];
			  for ( int i = 0; i < keys.Length; i++ )
			  {
					keys[i] = safeCastLongToInt( buffer.Long );
			  }
			  return keys;
		 }

		 private static long? ReadOwnedIndexRule( ByteBuffer buffer )
		 {
			  return buffer.Long;
		 }
	}

}