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
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using TokenRecord = Neo4Net.Kernel.impl.store.record.TokenRecord;

	public abstract class TokenRecordFormat<RECORD> : BaseOneByteHeaderRecordFormat<RECORD> where RECORD : Neo4Net.Kernel.impl.store.record.TokenRecord
	{
		 protected internal const int BASE_RECORD_SIZE = 1 + 4;

		 protected internal TokenRecordFormat( int recordSize, int idBits ) : base( fixedRecordSize( recordSize ), 0, IN_USE_BIT, idBits )
		 {
		 }

		 public override void Read( RECORD record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  sbyte inUseByte = cursor.Byte;
			  bool inUse = isInUse( inUseByte );
			  record.InUse = inUse;
			  if ( mode.shouldLoad( inUse ) )
			  {
					ReadRecordData( cursor, record, inUse );
			  }
		 }

		 protected internal virtual void ReadRecordData( PageCursor cursor, RECORD record, bool inUse )
		 {
			  record.initialize( inUse, cursor.Int );
		 }

		 public override void Write( RECORD record, PageCursor cursor, int recordSize )
		 {
			  if ( record.inUse() )
			  {
					cursor.PutByte( Record.IN_USE.byteValue() );
					WriteRecordData( record, cursor );
			  }
			  else
			  {
					cursor.PutByte( Record.NOT_IN_USE.byteValue() );
			  }
		 }

		 protected internal virtual void WriteRecordData( RECORD record, PageCursor cursor )
		 {
			  cursor.PutInt( record.NameId );
		 }
	}

}