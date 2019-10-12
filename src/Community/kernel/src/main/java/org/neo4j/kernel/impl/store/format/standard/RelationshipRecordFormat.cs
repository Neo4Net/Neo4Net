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
namespace Neo4Net.Kernel.impl.store.format.standard
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store.format;
	using Neo4Net.Kernel.impl.store.format;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;

	public class RelationshipRecordFormat : BaseOneByteHeaderRecordFormat<RelationshipRecord>
	{
		 // record header size
		 // directed|in_use(byte)+first_node(int)+second_node(int)+rel_type(int)+
		 // first_prev_rel_id(int)+first_next_rel_id+second_prev_rel_id(int)+
		 // second_next_rel_id+next_prop_id(int)+first-in-chain-markers(1)
		 public const int RECORD_SIZE = 34;

		 public RelationshipRecordFormat() : base(fixedRecordSize(RECORD_SIZE), 0, IN_USE_BIT, StandardFormatSettings.RELATIONSHIP_MAXIMUM_ID_BITS)
		 {
		 }

		 public override RelationshipRecord NewRecord()
		 {
			  return new RelationshipRecord( -1 );
		 }

		 public override void Read( RelationshipRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  sbyte headerByte = cursor.Byte;
			  bool inUse = IsInUse( headerByte );
			  record.InUse = inUse;
			  if ( mode.shouldLoad( inUse ) )
			  {
					// [    ,   x] in use flag
					// [    ,xxx ] first node high order bits
					// [xxxx,    ] next prop high order bits
					long firstNode = cursor.Int & 0xFFFFFFFFL;
					long firstNodeMod = ( headerByte & 0xEL ) << 31;

					long secondNode = cursor.Int & 0xFFFFFFFFL;

					// [ xxx,    ][    ,    ][    ,    ][    ,    ] second node high order bits,     0x70000000
					// [    ,xxx ][    ,    ][    ,    ][    ,    ] first prev rel high order bits,  0xE000000
					// [    ,   x][xx  ,    ][    ,    ][    ,    ] first next rel high order bits,  0x1C00000
					// [    ,    ][  xx,x   ][    ,    ][    ,    ] second prev rel high order bits, 0x380000
					// [    ,    ][    , xxx][    ,    ][    ,    ] second next rel high order bits, 0x70000
					// [    ,    ][    ,    ][xxxx,xxxx][xxxx,xxxx] type
					long typeInt = cursor.Int;
					long secondNodeMod = ( typeInt & 0x70000000L ) << 4;
					int type = ( int )( typeInt & 0xFFFF );

					long firstPrevRel = cursor.Int & 0xFFFFFFFFL;
					long firstPrevRelMod = ( typeInt & 0xE000000L ) << 7;

					long firstNextRel = cursor.Int & 0xFFFFFFFFL;
					long firstNextRelMod = ( typeInt & 0x1C00000L ) << 10;

					long secondPrevRel = cursor.Int & 0xFFFFFFFFL;
					long secondPrevRelMod = ( typeInt & 0x380000L ) << 13;

					long secondNextRel = cursor.Int & 0xFFFFFFFFL;
					long secondNextRelMod = ( typeInt & 0x70000L ) << 16;

					long nextProp = cursor.Int & 0xFFFFFFFFL;
					long nextPropMod = ( headerByte & 0xF0L ) << 28;

					sbyte extraByte = cursor.Byte;

					record.Initialize( inUse, BaseRecordFormat.longFromIntAndMod( nextProp, nextPropMod ), BaseRecordFormat.longFromIntAndMod( firstNode, firstNodeMod ), BaseRecordFormat.longFromIntAndMod( secondNode, secondNodeMod ), type, BaseRecordFormat.longFromIntAndMod( firstPrevRel, firstPrevRelMod ), BaseRecordFormat.longFromIntAndMod( firstNextRel, firstNextRelMod ), BaseRecordFormat.longFromIntAndMod( secondPrevRel, secondPrevRelMod ), BaseRecordFormat.longFromIntAndMod( secondNextRel, secondNextRelMod ), ( extraByte & 0x1 ) != 0, ( extraByte & 0x2 ) != 0 );
			  }
			  else
			  {
					int nextOffset = cursor.Offset + recordSize - HEADER_SIZE;
					cursor.Offset = nextOffset;
			  }
		 }

		 public override void Write( RelationshipRecord record, PageCursor cursor, int recordSize )
		 {
			  if ( record.InUse() )
			  {
					long firstNode = record.FirstNode;
					short firstNodeMod = ( short )( ( firstNode & 0x700000000L ) >> 31 );

					long secondNode = record.SecondNode;
					long secondNodeMod = ( secondNode & 0x700000000L ) >> 4;

					long firstPrevRel = record.FirstPrevRel;
					long firstPrevRelMod = firstPrevRel == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (firstPrevRel & 0x700000000L) >> 7;

					long firstNextRel = record.FirstNextRel;
					long firstNextRelMod = firstNextRel == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (firstNextRel & 0x700000000L) >> 10;

					long secondPrevRel = record.SecondPrevRel;
					long secondPrevRelMod = secondPrevRel == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (secondPrevRel & 0x700000000L) >> 13;

					long secondNextRel = record.SecondNextRel;
					long secondNextRelMod = secondNextRel == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (secondNextRel & 0x700000000L) >> 16;

					long nextProp = record.NextProp;
					long nextPropMod = nextProp == Record.NO_NEXT_PROPERTY.intValue() ? 0 : (nextProp & 0xF00000000L) >> 28;

					// [    ,   x] in use flag
					// [    ,xxx ] first node high order bits
					// [xxxx,    ] next prop high order bits
					short inUseUnsignedByte = ( short )( ( record.InUse() ? Record.IN_USE : Record.NOT_IN_USE ).byteValue() | firstNodeMod | nextPropMod );

					// [ xxx,    ][    ,    ][    ,    ][    ,    ] second node high order bits,     0x70000000
					// [    ,xxx ][    ,    ][    ,    ][    ,    ] first prev rel high order bits,  0xE000000
					// [    ,   x][xx  ,    ][    ,    ][    ,    ] first next rel high order bits,  0x1C00000
					// [    ,    ][  xx,x   ][    ,    ][    ,    ] second prev rel high order bits, 0x380000
					// [    ,    ][    , xxx][    ,    ][    ,    ] second next rel high order bits, 0x70000
					// [    ,    ][    ,    ][xxxx,xxxx][xxxx,xxxx] type
					int typeInt = ( int )( record.Type | secondNodeMod | firstPrevRelMod | firstNextRelMod | secondPrevRelMod | secondNextRelMod );

					// [    ,   x] 1:st in start node chain, 0x1
					// [    ,  x ] 1:st in end node chain,   0x2
					long firstInStartNodeChain = record.FirstInFirstChain ? 0x1 : 0;
					long firstInEndNodeChain = record.FirstInSecondChain ? 0x2 : 0;
					sbyte extraByte = ( sbyte )( firstInEndNodeChain | firstInStartNodeChain );

					cursor.PutByte( ( sbyte )inUseUnsignedByte );
					cursor.PutInt( ( int ) firstNode );
					cursor.PutInt( ( int ) secondNode );
					cursor.PutInt( typeInt );
					cursor.PutInt( ( int ) firstPrevRel );
					cursor.PutInt( ( int ) firstNextRel );
					cursor.PutInt( ( int ) secondPrevRel );
					cursor.PutInt( ( int ) secondNextRel );
					cursor.PutInt( ( int ) nextProp );
					cursor.PutByte( extraByte );
			  }
			  else
			  {
					MarkAsUnused( cursor );
			  }
		 }
	}

}