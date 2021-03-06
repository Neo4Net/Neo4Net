﻿using System.Collections.Generic;

/*
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
namespace Org.Neo4j.Kernel.impl.transaction.command
{

	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using DynamicNodeLabels = Org.Neo4j.Kernel.impl.store.DynamicNodeLabels;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using PropertyType = Org.Neo4j.Kernel.impl.store.PropertyType;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using SchemaRuleSerialization = Org.Neo4j.Kernel.impl.store.record.SchemaRuleSerialization;
	using TokenRecord = Org.Neo4j.Kernel.impl.store.record.TokenRecord;
	using LabelTokenCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.LabelTokenCommand;
	using NodeCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.PropertyCommand;
	using PropertyKeyTokenCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.PropertyKeyTokenCommand;
	using RelationshipCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using RelationshipGroupCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.RelationshipGroupCommand;
	using RelationshipTypeTokenCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand;
	using SchemaRuleCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.SchemaRuleCommand;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using Values = Org.Neo4j.Values.Storable.Values;

	public class Commands
	{
		 private Commands()
		 {
		 }

		 public static NodeCommand CreateNode( long id, params long[] dynamicLabelRecordIds )
		 {
			  NodeRecord record = new NodeRecord( id );
			  record.InUse = true;
			  record.SetCreated();
			  if ( dynamicLabelRecordIds.Length > 0 )
			  {
					ICollection<DynamicRecord> dynamicRecords = dynamicRecords( dynamicLabelRecordIds );
					record.SetLabelField( DynamicNodeLabels.dynamicPointer( dynamicRecords ), dynamicRecords );
			  }
			  return new NodeCommand( new NodeRecord( id ), record );
		 }

		 private static IList<DynamicRecord> DynamicRecords( params long[] dynamicLabelRecordIds )
		 {
			  IList<DynamicRecord> dynamicRecords = new List<DynamicRecord>();
			  foreach ( long did in dynamicLabelRecordIds )
			  {
					DynamicRecord dynamicRecord = new DynamicRecord( did );
					dynamicRecord.InUse = true;
					dynamicRecords.Add( dynamicRecord );
			  }
			  return dynamicRecords;
		 }

		 public static RelationshipCommand CreateRelationship( long id, long startNode, long endNode, int type )
		 {
			  RelationshipRecord before = new RelationshipRecord( id );
			  before.InUse = false;
			  RelationshipRecord after = new RelationshipRecord( id, startNode, endNode, type );
			  after.InUse = true;
			  after.SetCreated();
			  return new RelationshipCommand( before, after );
		 }

		 public static LabelTokenCommand CreateLabelToken( int id, int nameId )
		 {
			  LabelTokenRecord before = new LabelTokenRecord( id );
			  LabelTokenRecord after = new LabelTokenRecord( id );
			  PopulateTokenRecord( after, nameId );
			  return new LabelTokenCommand( before, after );
		 }

		 private static void PopulateTokenRecord( TokenRecord record, int nameId )
		 {
			  record.InUse = true;
			  record.NameId = nameId;
			  record.SetCreated();
			  DynamicRecord dynamicRecord = new DynamicRecord( nameId );
			  dynamicRecord.InUse = true;
			  dynamicRecord.Data = new sbyte[10];
			  dynamicRecord.SetCreated();
			  record.AddNameRecord( dynamicRecord );
		 }

		 public static PropertyKeyTokenCommand CreatePropertyKeyToken( int id, int nameId )
		 {
			  PropertyKeyTokenRecord before = new PropertyKeyTokenRecord( id );
			  PropertyKeyTokenRecord after = new PropertyKeyTokenRecord( id );
			  PopulateTokenRecord( after, nameId );
			  return new PropertyKeyTokenCommand( before, after );
		 }

		 public static RelationshipTypeTokenCommand CreateRelationshipTypeToken( int id, int nameId )
		 {
			  RelationshipTypeTokenRecord before = new RelationshipTypeTokenRecord( id );
			  RelationshipTypeTokenRecord after = new RelationshipTypeTokenRecord( id );
			  PopulateTokenRecord( after, nameId );
			  return new RelationshipTypeTokenCommand( before, after );
		 }

		 public static RelationshipGroupCommand CreateRelationshipGroup( long id, int type )
		 {
			  RelationshipGroupRecord before = new RelationshipGroupRecord( id );
			  RelationshipGroupRecord after = new RelationshipGroupRecord( id, type );
			  after.InUse = true;
			  after.SetCreated();
			  return new RelationshipGroupCommand( before, after );
		 }

		 public static SchemaRuleCommand CreateIndexRule( IndexProviderDescriptor provider, long id, LabelSchemaDescriptor descriptor )
		 {
			  SchemaRule rule = IndexDescriptorFactory.forSchema( descriptor, provider ).withId( id );
			  DynamicRecord record = new DynamicRecord( id );
			  record.InUse = true;
			  record.SetCreated();
			  record.Data = SchemaRuleSerialization.serialize( rule );
			  return new SchemaRuleCommand( Collections.emptyList(), singletonList(record), rule );
		 }

		 public static PropertyCommand CreateProperty( long id, PropertyType type, int key, params long[] valueRecordIds )
		 {
			  PropertyRecord record = new PropertyRecord( id );
			  record.InUse = true;
			  record.SetCreated();
			  PropertyBlock block = new PropertyBlock();
			  if ( valueRecordIds.Length == 0 )
			  {
					PropertyStore.encodeValue( block, key, Values.of( 123 ), null, null, true );
			  }
			  else
			  {
					PropertyStore.setSingleBlockValue( block, key, type, valueRecordIds[0] );
					block.ValueRecords = DynamicRecords( valueRecordIds );
			  }
			  record.AddPropertyBlock( block );
			  return new PropertyCommand( new PropertyRecord( id ), record );
		 }

		 public static TransactionRepresentation TransactionRepresentation( params Command[] commands )
		 {
			  return TransactionRepresentation( Arrays.asList( commands ) );
		 }

		 public static TransactionRepresentation TransactionRepresentation( ICollection<StorageCommand> commands )
		 {
			  PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( commands );
			  tx.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return tx;
		 }
	}

}