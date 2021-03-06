﻿/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.store.record
{

	using MalformedSchemaRuleException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using Org.Neo4j.@internal.Kernel.Api.schema;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaProcessor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaProcessor;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using UTF8 = Org.Neo4j.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@string.UTF8.getDecodedStringFrom;

	public class SchemaRuleSerialization
	{
		 // Schema rule type
		 // Legacy schema store reserves 1,2,3,4 and 5
		 private const sbyte INDEX_RULE = 11;
		 private const sbyte CONSTRAINT_RULE = 12;

		 // Index type
		 private const sbyte GENERAL_INDEX = 31;
		 private const sbyte UNIQUE_INDEX = 32;

		 // Constraint type
		 private const sbyte EXISTS_CONSTRAINT = 61;
		 private const sbyte UNIQUE_CONSTRAINT = 62;
		 private const sbyte UNIQUE_EXISTS_CONSTRAINT = 63;

		 // Schema type
		 private const sbyte SIMPLE_LABEL = 91;
		 private const sbyte SIMPLE_REL_TYPE = 92;
		 private const sbyte GENERIC_MULTI_TOKEN_TYPE = 93;

		 private const long NO_OWNING_CONSTRAINT_YET = -1;
		 private const int LEGACY_LABEL_OR_REL_TYPE_ID = -1;

		 private SchemaRuleSerialization()
		 {
		 }

		 // PUBLIC

		 /// <summary>
		 /// Serialize the provided SchemaRule onto the target buffer
		 /// </summary>
		 /// <param name="schemaRule"> the SchemaRule to serialize </param>
		 public static sbyte[] Serialize( SchemaRule schemaRule )
		 {
			  if ( schemaRule is StoreIndexDescriptor )
			  {
					return Serialize( ( StoreIndexDescriptor )schemaRule );
			  }
			  else if ( schemaRule is ConstraintRule )
			  {
					return Serialize( ( ConstraintRule )schemaRule );
			  }
			  throw new System.InvalidOperationException( "Unknown schema rule type: " + schemaRule.GetType() );
		 }

		 /// <summary>
		 /// Parse a SchemaRule from the provided buffer.
		 /// </summary>
		 /// <param name="id"> the id to give the returned Schema Rule </param>
		 /// <param name="source"> the buffer to parse from </param>
		 /// <returns> a SchemaRule </returns>
		 /// <exception cref="MalformedSchemaRuleException"> if bytes in the buffer do encode a valid SchemaRule </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.storageengine.api.schema.SchemaRule deserialize(long id, ByteBuffer source) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 public static SchemaRule Deserialize( long id, ByteBuffer source )
		 {
			  int legacyLabelOrRelTypeId = source.Int;
			  sbyte schemaRuleType = source.get();

			  switch ( schemaRuleType )
			  {
			  case INDEX_RULE:
					return ReadIndexRule( id, source );
			  case CONSTRAINT_RULE:
					return ReadConstraintRule( id, source );
			  default:
					if ( SchemaRuleDeserializer2_0to3_1.IsLegacySchemaRule( schemaRuleType ) )
					{
						 return SchemaRuleDeserializer2_0to3_1.Deserialize( id, legacyLabelOrRelTypeId, schemaRuleType, source );
					}
					throw new MalformedSchemaRuleException( format( "Got unknown schema rule type '%d'.", schemaRuleType ) );
			  }
		 }

		 /// <summary>
		 /// Serialize the provided IndexRule onto the target buffer
		 /// </summary>
		 /// <param name="indexDescriptor"> the StoreIndexDescriptor to serialize </param>
		 /// <exception cref="IllegalStateException"> if the StoreIndexDescriptor is of type unique, but the owning constrain has not been set </exception>
		 public static sbyte[] Serialize( StoreIndexDescriptor indexDescriptor )
		 {
			  ByteBuffer target = ByteBuffer.allocate( LengthOf( indexDescriptor ) );
			  target.putInt( LEGACY_LABEL_OR_REL_TYPE_ID );
			  target.put( INDEX_RULE );

			  IndexProviderDescriptor providerDescriptor = indexDescriptor.ProviderDescriptor();
			  UTF8.putEncodedStringInto( providerDescriptor.Key, target );
			  UTF8.putEncodedStringInto( providerDescriptor.Version, target );

			  switch ( indexDescriptor.Type() )
			  {
			  case GENERAL:
					target.put( GENERAL_INDEX );
					break;

			  case UNIQUE:
					target.put( UNIQUE_INDEX );

					// The owning constraint can be null. See IndexRule.getOwningConstraint()
					long? owningConstraint = indexDescriptor.OwningConstraint;
					target.putLong( owningConstraint == null ? NO_OWNING_CONSTRAINT_YET : owningConstraint );
					break;

			  default:
					throw new System.NotSupportedException( format( "Got unknown index descriptor type '%s'.", indexDescriptor.Type() ) );
			  }

			  indexDescriptor.Schema().processWith(new SchemaDescriptorSerializer(target));
			  UTF8.putEncodedStringInto( indexDescriptor.Name, target );
			  return target.array();
		 }

		 /// <summary>
		 /// Serialize the provided ConstraintRule onto the target buffer </summary>
		 /// <param name="constraintRule"> the ConstraintRule to serialize </param>
		 /// <exception cref="IllegalStateException"> if the ConstraintRule is of type unique, but the owned index has not been set </exception>
		 public static sbyte[] Serialize( ConstraintRule constraintRule )
		 {
			  ByteBuffer target = ByteBuffer.allocate( LengthOf( constraintRule ) );
			  target.putInt( LEGACY_LABEL_OR_REL_TYPE_ID );
			  target.put( CONSTRAINT_RULE );

			  ConstraintDescriptor constraintDescriptor = constraintRule.ConstraintDescriptor;
			  switch ( constraintDescriptor.Type() )
			  {
			  case EXISTS:
					target.put( EXISTS_CONSTRAINT );
					break;

			  case UNIQUE:
					target.put( UNIQUE_CONSTRAINT );
					target.putLong( constraintRule.OwnedIndex );
					break;

			  case UNIQUE_EXISTS:
					target.put( UNIQUE_EXISTS_CONSTRAINT );
					target.putLong( constraintRule.OwnedIndex );
					break;

			  default:
					throw new System.NotSupportedException( format( "Got unknown index descriptor type '%s'.", constraintDescriptor.Type() ) );
			  }

			  constraintDescriptor.Schema().processWith(new SchemaDescriptorSerializer(target));
			  UTF8.putEncodedStringInto( constraintRule.Name, target );
			  return target.array();
		 }

		 /// <summary>
		 /// Compute the byte size needed to serialize the provided IndexRule using serialize. </summary>
		 /// <param name="indexDescriptor"> the StoreIndexDescriptor </param>
		 /// <returns> the byte size of StoreIndexDescriptor </returns>
		 internal static int LengthOf( StoreIndexDescriptor indexDescriptor )
		 {
			  int length = 4; // legacy label or relType id
			  length += 1; // schema rule type

			  IndexProviderDescriptor providerDescriptor = indexDescriptor.ProviderDescriptor();
			  length += UTF8.computeRequiredByteBufferSize( providerDescriptor.Key );
			  length += UTF8.computeRequiredByteBufferSize( providerDescriptor.Version );

			  length += 1; // index type
			  if ( indexDescriptor.Type() == IndexDescriptor.Type.UNIQUE )
			  {
					length += 8; // owning constraint id
			  }

			  length += indexDescriptor.Schema().computeWith(schemaSizeComputer);
			  length += UTF8.computeRequiredByteBufferSize( indexDescriptor.Name );
			  return length;
		 }

		 /// <summary>
		 /// Compute the byte size needed to serialize the provided ConstraintRule using serialize. </summary>
		 /// <param name="constraintRule"> the ConstraintRule </param>
		 /// <returns> the byte size of ConstraintRule </returns>
		 internal static int LengthOf( ConstraintRule constraintRule )
		 {
			  int length = 4; // legacy label or relType id
			  length += 1; // schema rule type

			  length += 1; // constraint type
			  ConstraintDescriptor constraintDescriptor = constraintRule.ConstraintDescriptor;
			  if ( constraintDescriptor.EnforcesUniqueness() )
			  {
					length += 8; // owned index id
			  }

			  length += constraintDescriptor.Schema().computeWith(schemaSizeComputer);
			  length += UTF8.computeRequiredByteBufferSize( constraintRule.Name );
			  return length;
		 }

		 // PRIVATE

		 // READ INDEX

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.storageengine.api.schema.StoreIndexDescriptor readIndexRule(long id, ByteBuffer source) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private static StoreIndexDescriptor ReadIndexRule( long id, ByteBuffer source )
		 {
			  IndexProviderDescriptor indexProvider = ReadIndexProviderDescriptor( source );
			  sbyte indexRuleType = source.get();
			  Optional<string> name;
			  switch ( indexRuleType )
			  {
			  case GENERAL_INDEX:
			  {
					SchemaDescriptor schema = ReadSchema( source );
					name = ReadRuleName( source );
					return IndexDescriptorFactory.forSchema( schema, name, indexProvider ).withId( id );
			  }
			  case UNIQUE_INDEX:
			  {
					long owningConstraint = source.Long;
					SchemaDescriptor schema = ReadSchema( source );
					name = ReadRuleName( source );
					IndexDescriptor descriptor = IndexDescriptorFactory.uniqueForSchema( schema, name, indexProvider );
					return owningConstraint == NO_OWNING_CONSTRAINT_YET ? descriptor.WithId( id ) : descriptor.WithIds( id, owningConstraint );
			  }
			  default:
					throw new MalformedSchemaRuleException( format( "Got unknown index rule type '%d'.", indexRuleType ) );
			  }

		 }

		 private static IndexProviderDescriptor ReadIndexProviderDescriptor( ByteBuffer source )
		 {
			  string providerKey = getDecodedStringFrom( source );
			  string providerVersion = getDecodedStringFrom( source );
			  return new IndexProviderDescriptor( providerKey, providerVersion );
		 }

		 // READ CONSTRAINT

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static ConstraintRule readConstraintRule(long id, ByteBuffer source) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private static ConstraintRule ReadConstraintRule( long id, ByteBuffer source )
		 {
			  SchemaDescriptor schema;
			  sbyte constraintRuleType = source.get();
			  string name;
			  switch ( constraintRuleType )
			  {
			  case EXISTS_CONSTRAINT:
					schema = ReadSchema( source );
					name = ReadRuleName( source ).orElse( null );
					return ConstraintRule.ConstraintRuleConflict( id, ConstraintDescriptorFactory.existsForSchema( schema ), name );

			  case UNIQUE_CONSTRAINT:
					long ownedUniqueIndex = source.Long;
					schema = ReadSchema( source );
					UniquenessConstraintDescriptor descriptor = ConstraintDescriptorFactory.uniqueForSchema( schema );
					name = ReadRuleName( source ).orElse( null );
					return ConstraintRule.ConstraintRuleConflict( id, descriptor, ownedUniqueIndex, name );

			  case UNIQUE_EXISTS_CONSTRAINT:
					long ownedNodeKeyIndex = source.Long;
					schema = ReadSchema( source );
					NodeKeyConstraintDescriptor nodeKeyConstraintDescriptor = ConstraintDescriptorFactory.nodeKeyForSchema( schema );
					name = ReadRuleName( source ).orElse( null );
					return ConstraintRule.ConstraintRuleConflict( id, nodeKeyConstraintDescriptor, ownedNodeKeyIndex, name );

			  default:
					throw new MalformedSchemaRuleException( format( "Got unknown constraint rule type '%d'.", constraintRuleType ) );
			  }
		 }

		 private static Optional<string> ReadRuleName( ByteBuffer source )
		 {
			  if ( source.remaining() >= UTF8.MINIMUM_SERIALISED_LENGTH_BYTES )
			  {
					string ruleName = UTF8.getDecodedStringFrom( source );
					return ruleName.Length == 0 ? null : ruleName;
			  }
			  return null;
		 }

		 // READ HELP

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.internal.kernel.api.schema.SchemaDescriptor readSchema(ByteBuffer source) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private static SchemaDescriptor ReadSchema( ByteBuffer source )
		 {
			  int[] propertyIds;
			  sbyte schemaDescriptorType = source.get();
			  switch ( schemaDescriptorType )
			  {
			  case SIMPLE_LABEL:
					int labelId = source.Int;
					propertyIds = ReadTokenIdList( source );
					return SchemaDescriptorFactory.forLabel( labelId, propertyIds );
			  case SIMPLE_REL_TYPE:
					int relTypeId = source.Int;
					propertyIds = ReadTokenIdList( source );
					return SchemaDescriptorFactory.forRelType( relTypeId, propertyIds );
			  case GENERIC_MULTI_TOKEN_TYPE:
					return ReadMultiTokenSchema( source );
			  default:
					throw new MalformedSchemaRuleException( format( "Got unknown schema descriptor type '%d'.", schemaDescriptorType ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.internal.kernel.api.schema.SchemaDescriptor readMultiTokenSchema(ByteBuffer source) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 private static SchemaDescriptor ReadMultiTokenSchema( ByteBuffer source )
		 {
			  sbyte schemaDescriptorType = source.get();
			  EntityType type;
			  switch ( schemaDescriptorType )
			  {
			  case SIMPLE_LABEL:
					type = EntityType.NODE;
					break;
			  case SIMPLE_REL_TYPE:
					type = EntityType.RELATIONSHIP;
					break;
			  default:
					throw new MalformedSchemaRuleException( format( "Got unknown schema descriptor type '%d'.", schemaDescriptorType ) );
			  }
			  int[] entityTokenIds = ReadTokenIdList( source );
			  int[] propertyIds = ReadTokenIdList( source );
			  return SchemaDescriptorFactory.multiToken( entityTokenIds, type, propertyIds );
		 }

		 private static int[] ReadTokenIdList( ByteBuffer source )
		 {
			  short numProperties = source.Short;
			  int[] propertyIds = new int[numProperties];
			  for ( int i = 0; i < numProperties; i++ )
			  {
					propertyIds[i] = source.Int;
			  }
			  return propertyIds;
		 }

		 // WRITE

		 private class SchemaDescriptorSerializer : SchemaProcessor
		 {
			  internal readonly ByteBuffer Target;

			  internal SchemaDescriptorSerializer( ByteBuffer target )
			  {
					this.Target = target;
			  }

			  public override void ProcessSpecific( LabelSchemaDescriptor schema )
			  {
					Target.put( SIMPLE_LABEL );
					Target.putInt( Schema.LabelId );
					PutIds( Schema.PropertyIds );
			  }

			  public override void ProcessSpecific( RelationTypeSchemaDescriptor schema )
			  {
					Target.put( SIMPLE_REL_TYPE );
					Target.putInt( Schema.RelTypeId );
					PutIds( Schema.PropertyIds );
			  }

			  public override void ProcessSpecific( SchemaDescriptor schema )
			  {
					Target.put( GENERIC_MULTI_TOKEN_TYPE );
					if ( Schema.entityType() == EntityType.NODE )
					{
						 Target.put( SIMPLE_LABEL );
					}
					else
					{
						 Target.put( SIMPLE_REL_TYPE );
					}

					PutIds( Schema.EntityTokenIds );
					PutIds( Schema.PropertyIds );
			  }

			  internal virtual void PutIds( int[] ids )
			  {
					Target.putShort( ( short ) ids.Length );
					foreach ( int entityTokenId in ids )
					{
						 Target.putInt( entityTokenId );
					}
			  }
		 }

		 // LENGTH OF

		 private static SchemaComputer<int> schemaSizeComputer = new SchemaComputerAnonymousInnerClass();

		 private class SchemaComputerAnonymousInnerClass : SchemaComputer<int>
		 {
			 public int? computeSpecific( LabelSchemaDescriptor schema )
			 {
				  return 1 + 4 + 2 + 4 * Schema.PropertyIds.length; // the actual property ids
			 }

			 public int? computeSpecific( RelationTypeSchemaDescriptor schema )
			 {
				  return 1 + 4 + 2 + 4 * Schema.PropertyIds.length; // the actual property ids
			 }

			 public int? computeSpecific( SchemaDescriptor schema )
			 {
				  return 1 + 1 + 2 + 4 * Schema.EntityTokenIds.length + 2 + 4 * Schema.PropertyIds.length; // the actual property ids
			 }
		 }
	}

}