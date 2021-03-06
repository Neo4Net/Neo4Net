﻿/*
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
namespace Org.Neo4j.Kernel.impl.store.format
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;

	public class ForcedSecondaryUnitRecordFormat<RECORD> : RecordFormat<RECORD> where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private readonly RecordFormat<RECORD> _actual;

		 public ForcedSecondaryUnitRecordFormat( RecordFormat<RECORD> actual )
		 {
			  this._actual = actual;
		 }

		 public override RECORD NewRecord()
		 {
			  return _actual.newRecord();
		 }

		 public override int GetRecordSize( StoreHeader storeHeader )
		 {
			  return _actual.getRecordSize( storeHeader );
		 }

		 public virtual int RecordHeaderSize
		 {
			 get
			 {
				  return _actual.RecordHeaderSize;
			 }
		 }

		 public override bool IsInUse( PageCursor cursor )
		 {
			  return _actual.isInUse( cursor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void read(RECORD record, org.neo4j.io.pagecache.PageCursor cursor, org.neo4j.kernel.impl.store.record.RecordLoad mode, int recordSize) throws java.io.IOException
		 public override void Read( RECORD record, PageCursor cursor, RecordLoad mode, int recordSize )
		 {
			  _actual.read( record, cursor, mode, recordSize );
		 }

		 public override void Prepare( RECORD record, int recordSize, IdSequence idSequence )
		 {
			  _actual.prepare( record, recordSize, idSequence );
			  if ( !record.hasSecondaryUnitId() )
			  {
					record.SecondaryUnitId = idSequence.NextId();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(RECORD record, org.neo4j.io.pagecache.PageCursor cursor, int recordSize) throws java.io.IOException
		 public override void Write( RECORD record, PageCursor cursor, int recordSize )
		 {
			  _actual.write( record, cursor, recordSize );
		 }

		 public override long GetNextRecordReference( RECORD record )
		 {
			  return _actual.getNextRecordReference( record );
		 }

		 public override bool Equals( object otherFormat )
		 {
			  return _actual.Equals( otherFormat );
		 }

		 public override int GetHashCode()
		 {
			  return _actual.GetHashCode();
		 }

		 public virtual long MaxId
		 {
			 get
			 {
				  return _actual.MaxId;
			 }
		 }
	}

}