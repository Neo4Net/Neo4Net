using System;
using System.Collections.Generic;
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
namespace Neo4Net.Kernel.impl.transaction.command
{
	using MutableObjectIntMap = org.eclipse.collections.api.map.primitive.MutableObjectIntMap;
	using ObjectIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using MalformedSchemaRuleException = Neo4Net.Internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using IndexCommand = Neo4Net.Kernel.impl.index.IndexCommand;
	using AddNodeCommand = Neo4Net.Kernel.impl.index.IndexCommand.AddNodeCommand;
	using AddRelationshipCommand = Neo4Net.Kernel.impl.index.IndexCommand.AddRelationshipCommand;
	using CreateCommand = Neo4Net.Kernel.impl.index.IndexCommand.CreateCommand;
	using DeleteCommand = Neo4Net.Kernel.impl.index.IndexCommand.DeleteCommand;
	using RemoveCommand = Neo4Net.Kernel.impl.index.IndexCommand.RemoveCommand;
	using IndexDefineCommand = Neo4Net.Kernel.impl.index.IndexDefineCommand;
	using AbstractDynamicStore = Neo4Net.Kernel.impl.store.AbstractDynamicStore;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using SchemaRuleSerialization = Neo4Net.Kernel.Impl.Store.Records.SchemaRuleSerialization;
	using Neo4Net.Kernel.impl.transaction.command.CommandReading;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.unsignedShortToInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.CommandReading.COLLECTION_DYNAMIC_RECORD_ADDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.CommandReading.PROPERTY_BLOCK_DYNAMIC_RECORD_ADDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.CommandReading.PROPERTY_DELETED_DYNAMIC_RECORD_ADDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.CommandReading.PROPERTY_INDEX_DYNAMIC_RECORD_ADDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.bitFlag;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.notFlag;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IoPrimitiveUtils.read2bLengthAndString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IoPrimitiveUtils.read2bMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IoPrimitiveUtils.read3bLengthAndString;

	public class PhysicalLogCommandReaderV2_2_10 : BaseCommandReader
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Command read(byte commandType, org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 protected internal override Command Read( sbyte commandType, ReadableChannel channel )
		 {
			  switch ( commandType )
			  {
			  case NeoCommandType_Fields.NodeCommand:
					return VisitNodeCommand( channel );
			  case NeoCommandType_Fields.PropCommand:
					return VisitPropertyCommand( channel );
			  case NeoCommandType_Fields.PropIndexCommand:
					return VisitPropertyKeyTokenCommand( channel );
			  case NeoCommandType_Fields.RelCommand:
					return VisitRelationshipCommand( channel );
			  case NeoCommandType_Fields.RelTypeCommand:
					return VisitRelationshipTypeTokenCommand( channel );
			  case NeoCommandType_Fields.LabelKeyCommand:
					return VisitLabelTokenCommand( channel );
			  case NeoCommandType_Fields.NeostoreCommand:
					return VisitNeoStoreCommand( channel );
			  case NeoCommandType_Fields.SchemaRuleCommand:
					return VisitSchemaRuleCommand( channel );
			  case NeoCommandType_Fields.RelGroupCommand:
					return VisitRelationshipGroupCommand( channel );
			  case NeoCommandType_Fields.IndexDefineCommand:
					return VisitIndexDefineCommand( channel );
			  case NeoCommandType_Fields.IndexAddCommand:
					return VisitIndexAddNodeCommand( channel );
			  case NeoCommandType_Fields.IndexAddRelationshipCommand:
					return VisitIndexAddRelationshipCommand( channel );
			  case NeoCommandType_Fields.IndexRemoveCommand:
					return VisitIndexRemoveCommand( channel );
			  case NeoCommandType_Fields.IndexDeleteCommand:
					return VisitIndexDeleteCommand( channel );
			  case NeoCommandType_Fields.IndexCreateCommand:
					return VisitIndexCreateCommand( channel );
			  case NeoCommandType_Fields.UpdateRelationshipCountsCommand:
					return VisitRelationshipCountsCommand( channel );
			  case NeoCommandType_Fields.UpdateNodeCountsCommand:
					return VisitNodeCountsCommand( channel );
			  default:
					throw UnknownCommandType( commandType, channel );
			  }
		 }

		 private sealed class IndexCommandHeader
		 {
			  internal sbyte ValueType;
			  internal sbyte EntityType;
			  internal bool EntityIdNeedsLong;
			  internal int IndexNameId;
			  internal bool StartNodeNeedsLong;
			  internal bool EndNodeNeedsLong;
			  internal int KeyId;

			  internal IndexCommandHeader( sbyte valueType, sbyte entityType, bool entityIdNeedsLong, int indexNameId, bool startNodeNeedsLong, bool endNodeNeedsLong, int keyId )
			  {
					this.ValueType = valueType;
					this.EntityType = entityType;
					this.EntityIdNeedsLong = entityIdNeedsLong;
					this.IndexNameId = indexNameId;
					this.StartNodeNeedsLong = startNodeNeedsLong;
					this.EndNodeNeedsLong = endNodeNeedsLong;
					this.KeyId = keyId;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitNodeCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitNodeCommand( ReadableChannel channel )
		 {
			  long id = channel.Long;
			  NodeRecord before = ReadNodeRecord( id, channel );
			  if ( before == null )
			  {
					return null;
			  }
			  NodeRecord after = ReadNodeRecord( id, channel );
			  if ( after == null )
			  {
					return null;
			  }
			  if ( !before.InUse() && after.InUse() )
			  {
					after.SetCreated();
			  }
			  return new Command.NodeCommand( before, after );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitRelationshipCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitRelationshipCommand( ReadableChannel channel )
		 {
			  long id = channel.Long;
			  sbyte flags = channel.Get();
			  bool inUse = false;
			  if ( notFlag( notFlag( flags, Record.IN_USE.byteValue() ), Record.CREATED_IN_TX ) != 0 )
			  {
					throw new IOException( "Illegal in use flag: " + flags );
			  }
			  if ( bitFlag( flags, Record.IN_USE.byteValue() ) )
			  {
					inUse = true;
			  }
			  RelationshipRecord record;
			  if ( inUse )
			  {
					record = new RelationshipRecord( id, channel.Long, channel.Long, channel.Int );
					record.InUse = true;
					record.FirstPrevRel = channel.Long;
					record.FirstNextRel = channel.Long;
					record.SecondPrevRel = channel.Long;
					record.SecondNextRel = channel.Long;
					record.NextProp = channel.Long;
					sbyte extraByte = channel.Get();
					record.FirstInFirstChain = ( extraByte & 0x1 ) > 0;
					record.FirstInSecondChain = ( extraByte & 0x2 ) > 0;
			  }
			  else
			  {
					record = new RelationshipRecord( id, -1, -1, channel.Int );
					record.InUse = false;
			  }
			  if ( bitFlag( flags, Record.CREATED_IN_TX ) )
			  {
					record.SetCreated();
			  }
			  return new Command.RelationshipCommand( null, record );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitPropertyCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitPropertyCommand( ReadableChannel channel )
		 {
			  // ID
			  long id = channel.Long; // 8
			  // BEFORE
			  PropertyRecord before = ReadPropertyRecord( id, channel );
			  if ( before == null )
			  {
					return null;
			  }
			  // AFTER
			  PropertyRecord after = ReadPropertyRecord( id, channel );
			  if ( after == null )
			  {
					return null;
			  }
			  return new Command.PropertyCommand( before, after );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitRelationshipGroupCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitRelationshipGroupCommand( ReadableChannel channel )
		 {
			  long id = channel.Long;
			  sbyte inUseByte = channel.Get();
			  bool inUse = inUseByte == Record.IN_USE.byteValue();
			  if ( inUseByte != Record.IN_USE.byteValue() && inUseByte != Record.NOT_IN_USE.byteValue() )
			  {
					throw new IOException( "Illegal in use flag: " + inUseByte );
			  }
			  int type = unsignedShortToInt( channel.Short );
			  RelationshipGroupRecord record = new RelationshipGroupRecord( id, type );
			  record.InUse = inUse;
			  record.Next = channel.Long;
			  record.FirstOut = channel.Long;
			  record.FirstIn = channel.Long;
			  record.FirstLoop = channel.Long;
			  record.OwningNode = channel.Long;

			  return new Command.RelationshipGroupCommand( null, record );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitRelationshipTypeTokenCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitRelationshipTypeTokenCommand( ReadableChannel channel )
		 {
			  // id+in_use(byte)+type_blockId(int)+nr_type_records(int)
			  int id = channel.Int;
			  sbyte inUseFlag = channel.Get();
			  bool inUse = false;
			  if ( ( inUseFlag & Record.IN_USE.byteValue() ) == Record.IN_USE.byteValue() )
			  {
					inUse = true;
			  }
			  else if ( inUseFlag != Record.NOT_IN_USE.byteValue() )
			  {
					throw new IOException( "Illegal in use flag: " + inUseFlag );
			  }
			  RelationshipTypeTokenRecord record = new RelationshipTypeTokenRecord( id );
			  record.InUse = inUse;
			  record.NameId = channel.Int;
			  int nrTypeRecords = channel.Int;
			  for ( int i = 0; i < nrTypeRecords; i++ )
			  {
					DynamicRecord dr = ReadDynamicRecord( channel );
					if ( dr == null )
					{
						 return null;
					}
					record.AddNameRecord( dr );
			  }
			  return new Command.RelationshipTypeTokenCommand( null, record );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitLabelTokenCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitLabelTokenCommand( ReadableChannel channel )
		 {
			  // id+in_use(byte)+type_blockId(int)+nr_type_records(int)
			  int id = channel.Int;
			  sbyte inUseFlag = channel.Get();
			  bool inUse = false;
			  if ( ( inUseFlag & Record.IN_USE.byteValue() ) == Record.IN_USE.byteValue() )
			  {
					inUse = true;
			  }
			  else if ( inUseFlag != Record.NOT_IN_USE.byteValue() )
			  {
					throw new IOException( "Illegal in use flag: " + inUseFlag );
			  }
			  LabelTokenRecord record = new LabelTokenRecord( id );
			  record.InUse = inUse;
			  record.NameId = channel.Int;
			  int nrTypeRecords = channel.Int;
			  for ( int i = 0; i < nrTypeRecords; i++ )
			  {
					DynamicRecord dr = ReadDynamicRecord( channel );
					if ( dr == null )
					{
						 return null;
					}
					record.AddNameRecord( dr );
			  }
			  return new Command.LabelTokenCommand( null, record );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitPropertyKeyTokenCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitPropertyKeyTokenCommand( ReadableChannel channel )
		 {
			  // id+in_use(byte)+count(int)+key_blockId(int)
			  int id = channel.Int;
			  sbyte inUseFlag = channel.Get();
			  bool inUse = false;
			  if ( ( inUseFlag & Record.IN_USE.byteValue() ) == Record.IN_USE.byteValue() )
			  {
					inUse = true;
			  }
			  else if ( inUseFlag != Record.NOT_IN_USE.byteValue() )
			  {
					throw new IOException( "Illegal in use flag: " + inUseFlag );
			  }
			  PropertyKeyTokenRecord record = new PropertyKeyTokenRecord( id );
			  record.InUse = inUse;
			  record.PropertyCount = channel.Int;
			  record.NameId = channel.Int;
			  if ( ReadDynamicRecords( channel, record, PROPERTY_INDEX_DYNAMIC_RECORD_ADDER ) == -1 )
			  {
					return null;
			  }
			  return new Command.PropertyKeyTokenCommand( null, record );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitSchemaRuleCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitSchemaRuleCommand( ReadableChannel channel )
		 {
			  ICollection<DynamicRecord> recordsBefore = new List<DynamicRecord>();
			  ReadDynamicRecords( channel, recordsBefore, COLLECTION_DYNAMIC_RECORD_ADDER );
			  ICollection<DynamicRecord> recordsAfter = new List<DynamicRecord>();
			  ReadDynamicRecords( channel, recordsAfter, COLLECTION_DYNAMIC_RECORD_ADDER );
			  sbyte isCreated = channel.Get();
			  if ( 1 == isCreated )
			  {
					foreach ( DynamicRecord record in recordsAfter )
					{
						 record.SetCreated();
					}
			  }
			  SchemaRule rule = Iterables.first( recordsAfter ).inUse() ? ReadSchemaRule(recordsAfter) : ReadSchemaRule(recordsBefore);
			  return new Command.SchemaRuleCommand( recordsBefore, recordsAfter, rule );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitNeoStoreCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitNeoStoreCommand( ReadableChannel channel )
		 {
			  long nextProp = channel.Long;
			  NeoStoreRecord record = new NeoStoreRecord();
			  record.NextProp = nextProp;
			  return new Command.NeoStoreCommand( null, record );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.store.record.NodeRecord readNodeRecord(long id, org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private NodeRecord ReadNodeRecord( long id, ReadableChannel channel )
		 {
			  sbyte inUseFlag = channel.Get();
			  bool inUse = false;
			  if ( inUseFlag == Record.IN_USE.byteValue() )
			  {
					inUse = true;
			  }
			  else if ( inUseFlag != Record.NOT_IN_USE.byteValue() )
			  {
					throw new IOException( "Illegal in use flag: " + inUseFlag );
			  }
			  NodeRecord record;
			  ICollection<DynamicRecord> dynamicLabelRecords = new List<DynamicRecord>();
			  long labelField = Record.NO_LABELS_FIELD.intValue();
			  if ( inUse )
			  {
					bool dense = channel.Get() == 1;
					record = new NodeRecord( id, dense, channel.Long, channel.Long );
					// labels
					labelField = channel.Long;
			  }
			  else
			  {
					record = new NodeRecord( id );
			  }
			  ReadDynamicRecords( channel, dynamicLabelRecords, COLLECTION_DYNAMIC_RECORD_ADDER );
			  record.SetLabelField( labelField, dynamicLabelRecords );
			  record.InUse = inUse;
			  return record;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.store.record.DynamicRecord readDynamicRecord(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private DynamicRecord ReadDynamicRecord( ReadableChannel channel )
		 {
			  // id+type+in_use(byte)+nr_of_bytes(int)+next_block(long)
			  long id = channel.Long;
			  Debug.Assert( id >= 0 && id <= ( 1L << 36 ) - 1, id + " is not a valid dynamic record id" );
			  int type = channel.Int;
			  sbyte inUseFlag = channel.Get();
			  bool inUse = ( inUseFlag & Record.IN_USE.byteValue() ) != 0;
			  DynamicRecord record = new DynamicRecord( id );
			  record.SetInUse( inUse, type );
			  if ( inUse )
			  {
					record.StartRecord = ( inUseFlag & Record.FIRST_IN_CHAIN.byteValue() ) != 0;
					int nrOfBytes = channel.Int;
					Debug.Assert( nrOfBytes >= 0 && nrOfBytes < ( ( 1 << 24 ) - 1 ), nrOfBytes + " is not valid for a number of bytes field of " + "a dynamic record" );
					long nextBlock = channel.Long;
					assert( nextBlock >= 0 && nextBlock <= ( 1L << 36 - 1 ) ) || ( nextBlock == Record.NO_NEXT_BLOCK.intValue() ) : nextBlock + " is not valid for a next record field of " + "a dynamic record";
					record.NextBlock = nextBlock;
					sbyte[] data = new sbyte[nrOfBytes];
					channel.Get( data, nrOfBytes );
					record.Data = data;
			  }
			  return record;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T> int readDynamicRecords(org.neo4j.storageengine.api.ReadableChannel channel, T target, org.neo4j.kernel.impl.transaction.command.CommandReading.DynamicRecordAdder<T> adder) throws java.io.IOException
		 private int ReadDynamicRecords<T>( ReadableChannel channel, T target, DynamicRecordAdder<T> adder )
		 {
			  int numberOfRecords = channel.Int;
			  Debug.Assert( numberOfRecords >= 0 );
			  while ( numberOfRecords > 0 )
			  {
					DynamicRecord read = ReadDynamicRecord( channel );
					if ( read == null )
					{
						 return -1;
					}
					adder.Add( target, read );
					numberOfRecords--;
			  }
			  return numberOfRecords;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.store.record.PropertyRecord readPropertyRecord(long id, org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private PropertyRecord ReadPropertyRecord( long id, ReadableChannel channel )
		 {
			  // in_use(byte)+type(int)+key_indexId(int)+prop_blockId(long)+
			  // prev_prop_id(long)+next_prop_id(long)
			  PropertyRecord record = new PropertyRecord( id );
			  sbyte inUseFlag = channel.Get(); // 1
			  long nextProp = channel.Long; // 8
			  long prevProp = channel.Long; // 8
			  record.NextProp = nextProp;
			  record.PrevProp = prevProp;
			  bool inUse = false;
			  if ( ( inUseFlag & Record.IN_USE.byteValue() ) == Record.IN_USE.byteValue() )
			  {
					inUse = true;
			  }
			  bool nodeProperty = true;
			  if ( ( inUseFlag & Record.REL_PROPERTY.byteValue() ) == Record.REL_PROPERTY.byteValue() )
			  {
					nodeProperty = false;
			  }
			  long primitiveId = channel.Long; // 8
			  if ( primitiveId != -1 && nodeProperty )
			  {
					record.NodeId = primitiveId;
			  }
			  else if ( primitiveId != -1 )
			  {
					record.RelId = primitiveId;
			  }
			  int nrPropBlocks = channel.Get();
			  Debug.Assert( nrPropBlocks >= 0 );
			  if ( nrPropBlocks > 0 )
			  {
					record.InUse = true;
			  }
			  while ( nrPropBlocks-- > 0 )
			  {
					PropertyBlock block = ReadPropertyBlock( channel );
					if ( block == null )
					{
						 return null;
					}
					record.AddPropertyBlock( block );
			  }
			  int deletedRecords = ReadDynamicRecords( channel, record, PROPERTY_DELETED_DYNAMIC_RECORD_ADDER );
			  if ( deletedRecords == -1 )
			  {
					return null;
			  }
			  Debug.Assert( deletedRecords >= 0 );
			  while ( deletedRecords-- > 0 )
			  {
					DynamicRecord read = ReadDynamicRecord( channel );
					if ( read == null )
					{
						 return null;
					}
					record.AddDeletedRecord( read );
			  }
			  if ( ( inUse && !record.InUse() ) || (!inUse && record.InUse()) )
			  {
					throw new System.InvalidOperationException( "Weird, inUse was read in as " + inUse + " but the record is " + record );
			  }
			  return record;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.store.record.PropertyBlock readPropertyBlock(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private PropertyBlock ReadPropertyBlock( ReadableChannel channel )
		 {
			  PropertyBlock toReturn = new PropertyBlock();
			  sbyte blockSize = channel.Get(); // the size is stored in bytes // 1
			  Debug.Assert( blockSize > 0 && blockSize % 8 == 0, blockSize + " is not a valid block size value" );
			  // Read in blocks
			  long[] blocks = ReadLongs( channel, blockSize / 8 );
			  Debug.Assert( blocks.Length == blockSize / 8, blocks.Length + " longs were read in while i asked for what corresponds to " + blockSize );
			  Debug.Assert( PropertyType.getPropertyTypeOrThrow( blocks[0] ).calculateNumberOfBlocksUsed( blocks[0] ) == blocks.Length, blocks.Length + " is not a valid number of blocks for type " );
																  + PropertyType.getPropertyTypeOrThrow( blocks[0] );
			  /*
			   *  Ok, now we may be ready to return, if there are no DynamicRecords. So
			   *  we start building the Object
			   */
			  toReturn.ValueBlocks = blocks;
			  /*
			   * Read in existence of DynamicRecords. Remember, this has already been
			   * read in the buffer with the blocks, above.
			   */
			  if ( ReadDynamicRecords( channel, toReturn, PROPERTY_BLOCK_DYNAMIC_RECORD_ADDER ) == -1 )
			  {
					return null;
			  }
			  return toReturn;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long[] readLongs(org.neo4j.storageengine.api.ReadableChannel channel, int count) throws java.io.IOException
		 private long[] ReadLongs( ReadableChannel channel, int count )
		 {
			  long[] result = new long[count];
			  for ( int i = 0; i < count; i++ )
			  {
					result[i] = channel.Long;
			  }
			  return result;
		 }

		 private SchemaRule ReadSchemaRule( ICollection<DynamicRecord> recordsBefore )
		 {
			  // TODO: Why was this assertion here?
			  //            assert first(recordsBefore).inUse() : "Asked to deserialize schema records that were not in
			  // use.";
			  SchemaRule rule;
			  ByteBuffer deserialized = AbstractDynamicStore.concatData( recordsBefore, new sbyte[100] );
			  try
			  {
					rule = SchemaRuleSerialization.deserialize( Iterables.first( recordsBefore ).Id, deserialized );
			  }
			  catch ( MalformedSchemaRuleException )
			  {
					return null;
			  }
			  return rule;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitIndexAddNodeCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitIndexAddNodeCommand( ReadableChannel channel )
		 {
			  IndexCommandHeader header = ReadIndexCommandHeader( channel );
			  Number entityId = header.EntityIdNeedsLong ? channel.Long : channel.Int;
			  object value = ReadIndexValue( header.ValueType, channel );
			  IndexCommand.AddNodeCommand command = new IndexCommand.AddNodeCommand();
			  command.Init( header.IndexNameId, entityId.longValue(), header.KeyId, value );
			  return command;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitIndexAddRelationshipCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitIndexAddRelationshipCommand( ReadableChannel channel )
		 {
			  IndexCommandHeader header = ReadIndexCommandHeader( channel );
			  Number entityId = header.EntityIdNeedsLong ? channel.Long : channel.Int;
			  object value = ReadIndexValue( header.ValueType, channel );
			  Number startNode = header.StartNodeNeedsLong ? channel.Long : channel.Int;
			  Number endNode = header.EndNodeNeedsLong ? channel.Long : channel.Int;
			  IndexCommand.AddRelationshipCommand command = new IndexCommand.AddRelationshipCommand();
			  command.Init( header.IndexNameId, entityId.longValue(), header.KeyId, value, startNode.longValue(), endNode.longValue() );
			  return command;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitIndexRemoveCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitIndexRemoveCommand( ReadableChannel channel )
		 {
			  IndexCommandHeader header = ReadIndexCommandHeader( channel );
			  Number entityId = header.EntityIdNeedsLong ? channel.Long : channel.Int;
			  object value = ReadIndexValue( header.ValueType, channel );
			  IndexCommand.RemoveCommand command = new IndexCommand.RemoveCommand();
			  command.Init( header.IndexNameId, header.EntityType, entityId.longValue(), header.KeyId, value );
			  return command;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitIndexDeleteCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitIndexDeleteCommand( ReadableChannel channel )
		 {
			  IndexCommandHeader header = ReadIndexCommandHeader( channel );
			  IndexCommand.DeleteCommand command = new IndexCommand.DeleteCommand();
			  command.Init( header.IndexNameId, header.EntityType );
			  return command;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitIndexCreateCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitIndexCreateCommand( ReadableChannel channel )
		 {
			  IndexCommandHeader header = ReadIndexCommandHeader( channel );
			  IDictionary<string, string> config = read2bMap( channel );
			  IndexCommand.CreateCommand command = new IndexCommand.CreateCommand();
			  command.Init( header.IndexNameId, header.EntityType, config );
			  return command;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitIndexDefineCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitIndexDefineCommand( ReadableChannel channel )
		 {
			  ReadIndexCommandHeader( channel );
			  MutableObjectIntMap<string> indexNames = ReadMap( channel );
			  MutableObjectIntMap<string> keys = ReadMap( channel );
			  IndexDefineCommand command = new IndexDefineCommand();
			  command.Init( indexNames, keys );
			  return command;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitNodeCountsCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitNodeCountsCommand( ReadableChannel channel )
		 {
			  int labelId = channel.Int;
			  long delta = channel.Long;
			  return new Command.NodeCountsCommand( labelId, delta );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Command visitRelationshipCountsCommand(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private Command VisitRelationshipCountsCommand( ReadableChannel channel )
		 {
			  int startLabelId = channel.Int;
			  int typeId = channel.Int;
			  int endLabelId = channel.Int;
			  long delta = channel.Long;
			  return new Command.RelationshipCountsCommand( startLabelId, typeId, endLabelId, delta );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.eclipse.collections.api.map.primitive.MutableObjectIntMap<String> readMap(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private MutableObjectIntMap<string> ReadMap( ReadableChannel channel )
		 {
			  int size = GetUnsignedShort( channel );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableObjectIntMap<String> result = new org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap<>(size);
			  MutableObjectIntMap<string> result = new ObjectIntHashMap<string>( size );
			  for ( int i = 0; i < size; i++ )
			  {
					string key = read2bLengthAndString( channel );
					int id = GetUnsignedShort( channel );
					if ( string.ReferenceEquals( key, null ) )
					{
						 return null;
					}
					result.put( key, id );
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getUnsignedShort(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private int GetUnsignedShort( ReadableChannel channel )
		 {
			  int result = channel.Short & 0xFFFF;
			  return result == 0xFFFF ? -1 : result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private IndexCommandHeader readIndexCommandHeader(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private IndexCommandHeader ReadIndexCommandHeader( ReadableChannel channel )
		 {
			  sbyte firstHeaderByte = channel.Get();
			  sbyte valueType = ( sbyte )( ( firstHeaderByte & 0x1C ) >> 2 );
			  sbyte entityType = ( sbyte )( ( firstHeaderByte & 0x2 ) >> 1 );
			  bool entityIdNeedsLong = ( firstHeaderByte & 0x1 ) > 0;
			  sbyte secondHeaderByte = channel.Get();
			  bool startNodeNeedsLong = ( secondHeaderByte & 0x80 ) > 0;
			  bool endNodeNeedsLong = ( secondHeaderByte & 0x40 ) > 0;
			  int indexNameId = GetUnsignedShort( channel );
			  int keyId = GetUnsignedShort( channel );
			  return new IndexCommandHeader( valueType, entityType, entityIdNeedsLong, indexNameId, startNodeNeedsLong, endNodeNeedsLong, keyId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Object readIndexValue(byte valueType, org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 private object ReadIndexValue( sbyte valueType, ReadableChannel channel )
		 {
			  switch ( valueType )
			  {
			  case IndexCommand.VALUE_TYPE_NULL:
					return null;
			  case IndexCommand.VALUE_TYPE_SHORT:
					return channel.Short;
			  case IndexCommand.VALUE_TYPE_INT:
					return channel.Int;
			  case IndexCommand.VALUE_TYPE_LONG:
					return channel.Long;
			  case IndexCommand.VALUE_TYPE_FLOAT:
					return channel.Float;
			  case IndexCommand.VALUE_TYPE_DOUBLE:
					return channel.Double;
			  case IndexCommand.VALUE_TYPE_STRING:
					return read3bLengthAndString( channel );
			  default:
					throw new Exception( "Unknown value type " + valueType );
			  }
		 }
	}

}