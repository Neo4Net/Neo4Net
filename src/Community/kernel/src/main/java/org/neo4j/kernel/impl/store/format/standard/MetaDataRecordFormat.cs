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
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using Neo4Net.Kernel.impl.store.format;
	using MetaDataRecord = Neo4Net.Kernel.impl.store.record.MetaDataRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;

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