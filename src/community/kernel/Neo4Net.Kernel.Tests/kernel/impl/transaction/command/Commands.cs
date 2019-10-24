using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using DynamicNodeLabels = Neo4Net.Kernel.impl.store.DynamicNodeLabels;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using SchemaRuleSerialization = Neo4Net.Kernel.Impl.Store.Records.SchemaRuleSerialization;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;
	using LabelTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.LabelTokenCommand;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using PropertyKeyTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyKeyTokenCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using RelationshipGroupCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipGroupCommand;
	using RelationshipTypeTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand;
	using SchemaRuleCommand = Neo4Net.Kernel.impl.transaction.command.Command.SchemaRuleCommand;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;
	using Values = Neo4Net.Values.Storable.Values;

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