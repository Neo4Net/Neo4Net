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
namespace Neo4Net.Kernel.impl.store.format.standard
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store.format;
	using Neo4Net.Kernel.impl.store.format;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;

	public class RelationshipGroupRecordFormat : BaseOneByteHeaderRecordFormat<RelationshipGroupRecord>
	{
		/* Record layout
		 *
		 * [type+inUse+highbits,next,firstOut,firstIn,firstLoop,owningNode] = 25B
		 *
		 * One record holds first relationship links (out,in,loop) to relationships for one type for one entity.
		 */

		 public const int RECORD_SIZE = 25;

		 public RelationshipGroupRecordFormat() : base(fixedRecordSize(RECORD_SIZE), 0, IN_USE_BIT, StandardFormatSettings.RELATIONSHIP_GROUP_MAXIMUM_ID_BITS)
		 {
		 }

		 public override void Read( RelationshipGroupRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  // [    ,   x] in use
			  // [    ,xxx ] high next id bits
			  // [ xxx,    ] high firstOut bits
			  long headerByte = cursor.Byte;
			  bool inUse = isInUse( ( sbyte ) headerByte );
			  record.InUse = inUse;
			  if ( mode.shouldLoad( inUse ) )
			  {
					// [    ,xxx ] high firstIn bits
					// [ xxx,    ] high firstLoop bits
					long highByte = cursor.Byte;

					int type = cursor.Short & 0xFFFF;
					long nextLowBits = cursor.Int & 0xFFFFFFFFL;
					long nextOutLowBits = cursor.Int & 0xFFFFFFFFL;
					long nextInLowBits = cursor.Int & 0xFFFFFFFFL;
					long nextLoopLowBits = cursor.Int & 0xFFFFFFFFL;
					long owningNode = ( cursor.Int & 0xFFFFFFFFL ) | ( ( ( long )cursor.Byte ) << 32 );

					long nextMod = ( headerByte & 0xE ) << 31;
					long nextOutMod = ( headerByte & 0x70 ) << 28;
					long nextInMod = ( highByte & 0xE ) << 31;
					long nextLoopMod = ( highByte & 0x70 ) << 28;

					record.Initialize( inUse, type, BaseRecordFormat.longFromIntAndMod( nextOutLowBits, nextOutMod ), BaseRecordFormat.longFromIntAndMod( nextInLowBits, nextInMod ), BaseRecordFormat.longFromIntAndMod( nextLoopLowBits, nextLoopMod ), owningNode, BaseRecordFormat.longFromIntAndMod( nextLowBits, nextMod ) );
			  }
		 }

		 public override void Write( RelationshipGroupRecord record, PageCursor cursor, int recordSize )
		 {
			  if ( record.InUse() )
			  {
					long nextMod = record.Next == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (record.Next & 0x700000000L) >> 31;
					long nextOutMod = record.FirstOut == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (record.FirstOut & 0x700000000L) >> 28;
					long nextInMod = record.FirstIn == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (record.FirstIn & 0x700000000L) >> 31;
					long nextLoopMod = record.FirstLoop == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (record.FirstLoop & 0x700000000L) >> 28;

					// [    ,   x] in use
					// [    ,xxx ] high next id bits
					// [ xxx,    ] high firstOut bits
					cursor.PutByte( ( sbyte )( nextOutMod | nextMod | 1 ) );

					// [    ,xxx ] high firstIn bits
					// [ xxx,    ] high firstLoop bits
					cursor.PutByte( ( sbyte )( nextLoopMod | nextInMod ) );

					cursor.PutShort( ( short ) record.Type );
					cursor.PutInt( ( int ) record.Next );
					cursor.PutInt( ( int ) record.FirstOut );
					cursor.PutInt( ( int ) record.FirstIn );
					cursor.PutInt( ( int ) record.FirstLoop );
					cursor.PutInt( ( int ) record.OwningNode );
					cursor.PutByte( ( sbyte )( record.OwningNode >> 32 ) );
			  }
			  else
			  {
					MarkAsUnused( cursor );
			  }
		 }

		 public override RelationshipGroupRecord NewRecord()
		 {
			  return new RelationshipGroupRecord( -1 );
		 }

		 public override long GetNextRecordReference( RelationshipGroupRecord record )
		 {
			  return record.Next;
		 }
	}

}