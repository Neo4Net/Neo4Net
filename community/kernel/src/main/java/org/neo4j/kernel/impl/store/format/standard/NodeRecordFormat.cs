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
namespace Org.Neo4j.Kernel.impl.store.format.standard
{
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Org.Neo4j.Kernel.impl.store.format;
	using Org.Neo4j.Kernel.impl.store.format;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;

	public class NodeRecordFormat : BaseOneByteHeaderRecordFormat<NodeRecord>
	{
		 // in_use(byte)+next_rel_id(int)+next_prop_id(int)+labels(5)+extra(byte)
		 public const int RECORD_SIZE = 15;

		 public NodeRecordFormat() : base(fixedRecordSize(RECORD_SIZE), 0, IN_USE_BIT, StandardFormatSettings.NODE_MAXIMUM_ID_BITS)
		 {
		 }

		 public override NodeRecord NewRecord()
		 {
			  return new NodeRecord( -1 );
		 }

		 public override void Read( NodeRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  sbyte headerByte = cursor.Byte;
			  bool inUse = IsInUse( headerByte );
			  record.InUse = inUse;
			  if ( mode.shouldLoad( inUse ) )
			  {
					long nextRel = cursor.Int & 0xFFFFFFFFL;
					long nextProp = cursor.Int & 0xFFFFFFFFL;

					long relModifier = ( headerByte & 0xEL ) << 31;
					long propModifier = ( headerByte & 0xF0L ) << 28;

					long lsbLabels = cursor.Int & 0xFFFFFFFFL;
					long hsbLabels = cursor.Byte & 0xFF; // so that a negative byte won't fill the "extended" bits with ones.
					long labels = lsbLabels | ( hsbLabels << 32 );
					sbyte extra = cursor.Byte;
					bool dense = ( extra & 0x1 ) > 0;

					record.Initialize( inUse, BaseRecordFormat.longFromIntAndMod( nextProp, propModifier ), dense, BaseRecordFormat.longFromIntAndMod( nextRel, relModifier ), labels );
			  }
			  else
			  {
					int nextOffset = cursor.Offset + recordSize - HEADER_SIZE;
					cursor.Offset = nextOffset;
			  }
		 }

		 public override void Write( NodeRecord record, PageCursor cursor, int recordSize )
		 {
			  if ( record.InUse() )
			  {
					long nextRel = record.NextRel;
					long nextProp = record.NextProp;

					short relModifier = nextRel == Record.NO_NEXT_RELATIONSHIP.intValue() ? 0 : (short)((nextRel & 0x700000000L) >> 31);
					short propModifier = nextProp == Record.NO_NEXT_PROPERTY.intValue() ? 0 : (short)((nextProp & 0xF00000000L) >> 28);

					// [    ,   x] in use bit
					// [    ,xxx ] higher bits for rel id
					// [xxxx,    ] higher bits for prop id
					short inUseUnsignedByte = ( record.InUse() ? Record.IN_USE : Record.NOT_IN_USE ).byteValue();
					inUseUnsignedByte = ( short )( inUseUnsignedByte | relModifier | propModifier );

					cursor.PutByte( ( sbyte ) inUseUnsignedByte );
					cursor.PutInt( ( int ) nextRel );
					cursor.PutInt( ( int ) nextProp );

					// lsb of labels
					long labelField = record.LabelField;
					cursor.PutInt( ( int ) labelField );
					// msb of labels
					cursor.PutByte( ( sbyte )( ( labelField & 0xFF00000000L ) >> 32 ) );

					sbyte extra = record.Dense ? ( sbyte )1 : ( sbyte )0;
					cursor.PutByte( extra );
			  }
			  else
			  {
					MarkAsUnused( cursor );
			  }
		 }
	}

}