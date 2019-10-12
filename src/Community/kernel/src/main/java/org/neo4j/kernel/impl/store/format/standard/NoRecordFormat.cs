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
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;

	public class NoRecordFormat<RECORD> : RecordFormat<RECORD> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
	{
		 public override RECORD NewRecord()
		 {
			  return default( RECORD );
		 }

		 public override int GetRecordSize( StoreHeader storeHeader )
		 {
			  return Neo4Net.Kernel.impl.store.format.RecordFormat_Fields.NO_RECORD_SIZE;
		 }

		 public virtual int RecordHeaderSize
		 {
			 get
			 {
				  return 0;
			 }
		 }

		 public override bool IsInUse( PageCursor cursor )
		 {
			  return false;
		 }

		 public override void Read( RECORD record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  record.clear();
		 }

		 public override void Prepare( RECORD record, int recordSize, IdSequence idSequence )
		 {
		 }

		 public override void Write( RECORD record, PageCursor cursor, int recordSize )
		 {
		 }

		 public override long GetNextRecordReference( RECORD record )
		 {
			  return Record.NULL_REFERENCE.intValue();
		 }

		 public virtual long MaxId
		 {
			 get
			 {
				  return 0;
			 }
		 }
	}

}