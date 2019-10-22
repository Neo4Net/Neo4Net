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
namespace Neo4Net.Kernel.impl.store.format.standard
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store.format;
	using Neo4Net.Kernel.impl.store.format;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.DynamicRecord.NO_DATA;

	public class DynamicRecordFormat : BaseOneByteHeaderRecordFormat<DynamicRecord>
	{
		 // (in_use+next high)(1 byte)+nr_of_bytes(3 bytes)+next_block(int)
		 public const int RECORD_HEADER_SIZE = 1 + 3 + 4; // = 8

		 public DynamicRecordFormat() : base(INT_STORE_HEADER_READER, RECORD_HEADER_SIZE, 0x10, StandardFormatSettings.DYNAMIC_MAXIMUM_ID_BITS)
		 {
		 }

		 public override DynamicRecord NewRecord()
		 {
			  return new DynamicRecord( -1 );
		 }

		 public override void Read( DynamicRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  /*
			   * First 4b
			   * [x   ,    ][    ,    ][    ,    ][    ,    ] 0: start record, 1: linked record
			   * [   x,    ][    ,    ][    ,    ][    ,    ] inUse
			   * [    ,xxxx][    ,    ][    ,    ][    ,    ] high next block bits
			   * [    ,    ][xxxx,xxxx][xxxx,xxxx][xxxx,xxxx] nr of bytes in the data field in this record
			   *
			   */
			  long firstInteger = cursor.Int & 0xFFFFFFFFL;
			  bool isStartRecord = ( firstInteger & 0x80000000 ) == 0;
			  bool inUse = ( firstInteger & 0x10000000 ) != 0;
			  if ( mode.shouldLoad( inUse ) )
			  {
					int dataSize = recordSize - RecordHeaderSize;
					int nrOfBytes = ( int )( firstInteger & 0xFFFFFF );
					if ( nrOfBytes > recordSize )
					{
						 // We must have performed an inconsistent read,
						 // because this many bytes cannot possibly fit in a record!
						 cursor.CursorException = PayloadTooBigErrorMessage( record, recordSize, nrOfBytes );
						 return;
					}

					/*
					 * Pointer to next block 4b (low bits of the pointer)
					 */
					long nextBlock = cursor.Int & 0xFFFFFFFFL;
					long nextModifier = ( firstInteger & 0xF000000L ) << 8;

					long longNextBlock = BaseRecordFormat.longFromIntAndMod( nextBlock, nextModifier );
					record.Initialize( inUse, isStartRecord, longNextBlock, -1, nrOfBytes );
					if ( longNextBlock != Record.NO_NEXT_BLOCK.intValue() && nrOfBytes < dataSize || nrOfBytes > dataSize )
					{
						 cursor.CursorException = IllegalBlockSizeMessage( record, dataSize );
						 return;
					}

					ReadData( record, cursor );
			  }
			  else
			  {
					record.InUse = inUse;
			  }
		 }

		 public static string PayloadTooBigErrorMessage( DynamicRecord record, int recordSize, int nrOfBytes )
		 {
			  return format( "DynamicRecord[%s] claims to have a payload of %s bytes, " + "which is larger than the record size of %s bytes.", record.Id, nrOfBytes, recordSize );
		 }

		 private string IllegalBlockSizeMessage( DynamicRecord record, int dataSize )
		 {
			  return format( "Next block set[%d] current block illegal size[%d/%d]", record.NextBlock, record.Length, dataSize );
		 }

		 public static void ReadData( DynamicRecord record, PageCursor cursor )
		 {
			  int len = record.Length;
			  if ( len == 0 ) // don't go though the trouble of acquiring the window if we would read nothing
			  {
					record.Data = NO_DATA;
					return;
			  }

			  sbyte[] data = record.Data;
			  if ( data == null || data.Length != len )
			  {
					data = new sbyte[len];
			  }
			  cursor.GetBytes( data );
			  record.Data = data;
		 }

		 public override void Write( DynamicRecord record, PageCursor cursor, int recordSize )
		 {
			  if ( record.InUse() )
			  {
					long nextBlock = record.NextBlock;
					int highByteInFirstInteger = nextBlock == Record.NO_NEXT_BLOCK.intValue() ? 0 : (int)((nextBlock & 0xF00000000L) >> 8);
					highByteInFirstInteger |= Record.IN_USE.byteValue() << 28;
					highByteInFirstInteger |= ( record.StartRecord ? 0 : 1 ) << 31;

					/*
					 * First 4b
					 * [x   ,    ][    ,    ][    ,    ][    ,    ] 0: start record, 1: linked record
					 * [   x,    ][    ,    ][    ,    ][    ,    ] inUse
					 * [    ,xxxx][    ,    ][    ,    ][    ,    ] high next block bits
					 * [    ,    ][xxxx,xxxx][xxxx,xxxx][xxxx,xxxx] nr of bytes in the data field in this record
					 *
					 */
					int firstInteger = record.Length;
					Debug.Assert( firstInteger < ( 1 << 24 ) - 1 );

					firstInteger |= highByteInFirstInteger;

					cursor.PutInt( firstInteger );
					cursor.PutInt( ( int ) nextBlock );
					cursor.PutBytes( record.Data );
			  }
			  else
			  {
					cursor.PutByte( Record.NOT_IN_USE.byteValue() );
			  }
		 }

		 public override long GetNextRecordReference( DynamicRecord record )
		 {
			  return record.NextBlock;
		 }
	}

}