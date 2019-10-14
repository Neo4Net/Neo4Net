using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.store.format.highlimit
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store.format;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.DynamicRecordFormat.payloadTooBigErrorMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.DynamicRecordFormat.readData;

	/// <summary>
	/// LEGEND:
	/// V: variable between 3B-8B
	/// D: data size
	/// 
	/// Record format:
	/// 1B   header
	/// 3B   number of bytes data in this block
	/// 8B   next block
	/// DB   data (record size - (the above) header size)
	/// 
	/// => 12B + data size
	/// </summary>
	public class DynamicRecordFormat : BaseOneByteHeaderRecordFormat<DynamicRecord>
	{
		 private const int RECORD_HEADER_SIZE = 1 + 3 + 8;
															  // = 12
		 private const int START_RECORD_BIT = 0x8;

		 public DynamicRecordFormat() : base(INT_STORE_HEADER_READER, RECORD_HEADER_SIZE, IN_USE_BIT, HighLimitFormatSettings.DYNAMIC_MAXIMUM_ID_BITS)
		 {
		 }

		 public override DynamicRecord NewRecord()
		 {
			  return new DynamicRecord( -1 );
		 }

		 public override void Read( DynamicRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  sbyte headerByte = cursor.Byte;
			  bool inUse = IsInUse( headerByte );
			  if ( mode.shouldLoad( inUse ) )
			  {
					int length = cursor.Short | cursor.Byte << 16;
					if ( length > recordSize | length < 0 )
					{
						 cursor.CursorException = PayloadLengthErrorMessage( record, recordSize, length );
						 return;
					}
					long next = cursor.Long;
					bool isStartRecord = ( headerByte & START_RECORD_BIT ) != 0;
					record.Initialize( inUse, isStartRecord, next, -1, length );
					readData( record, cursor );
			  }
			  else
			  {
					record.InUse = inUse;
			  }
		 }

		 private string PayloadLengthErrorMessage( DynamicRecord record, int recordSize, int length )
		 {
			  return length < 0 ? NegativePayloadErrorMessage( record, length ) : payloadTooBigErrorMessage( record, recordSize, length );
		 }

		 private string NegativePayloadErrorMessage( DynamicRecord record, int length )
		 {
			  return format( "DynamicRecord[%s] claims to have a negative payload of %s bytes.", record.Id, length );
		 }

		 public override void Write( DynamicRecord record, PageCursor cursor, int recordSize )
		 {
			  if ( record.InUse() )
			  {
					Debug.Assert( record.Length < ( 1 << 24 ) - 1 );
					sbyte headerByte = ( sbyte )( ( record.InUse() ? IN_USE_BIT : 0 ) | (record.StartRecord ? START_RECORD_BIT : 0) );
					cursor.PutByte( headerByte );
					cursor.PutShort( ( short ) record.Length );
					cursor.PutByte( ( sbyte )( ( int )( ( uint )record.Length >> 16 ) ) );
					cursor.PutLong( record.NextBlock );
					cursor.PutBytes( record.Data );
			  }
			  else
			  {
					MarkAsUnused( cursor );
			  }
		 }

		 public override long GetNextRecordReference( DynamicRecord record )
		 {
			  return record.NextBlock;
		 }
	}

}