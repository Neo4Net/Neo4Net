using System.Diagnostics;

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
	using Position = Org.Neo4j.Kernel.impl.store.MetaDataStore.Position;
	using Org.Neo4j.Kernel.impl.store.format;
	using MetaDataRecord = Org.Neo4j.Kernel.impl.store.record.MetaDataRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;

	public class MetaDataRecordFormat : BaseOneByteHeaderRecordFormat<MetaDataRecord>
	{
		 public const int RECORD_SIZE = 9;
		 public const long FIELD_NOT_PRESENT = -1;
		 private const int ID_BITS = 32;

		 public MetaDataRecordFormat() : base(fixedRecordSize(RECORD_SIZE), 0, IN_USE_BIT, ID_BITS)
		 {
		 }

		 public override MetaDataRecord NewRecord()
		 {
			  return new MetaDataRecord();
		 }

		 public override void Read( MetaDataRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  int id = record.IntId;
			  Position[] values = Position.values();
			  if ( id >= values.Length )
			  {
					record.Initialize( false, FIELD_NOT_PRESENT );
					return;
			  }

			  Position position = values[id];
			  int offset = position.id() * recordSize;
			  cursor.Offset = offset;
			  bool inUse = cursor.Byte == Record.IN_USE.byteValue();
			  long value = inUse ? cursor.Long : FIELD_NOT_PRESENT;
			  record.Initialize( inUse, value );
		 }

		 public override void Write( MetaDataRecord record, PageCursor cursor, int recordSize )
		 {
			  Debug.Assert( record.InUse() );
			  cursor.PutByte( Record.IN_USE.byteValue() );
			  cursor.PutLong( record.Value );
		 }
	}

}