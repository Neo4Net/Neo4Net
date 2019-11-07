using System;

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
namespace Neo4Net.Kernel.impl.store.format
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;

	/// <summary>
	/// Specifies a particular <seealso cref="AbstractBaseRecord record"/> format, used to read and write records in a
	/// <seealso cref="RecordStore"/> from and to a <seealso cref="PageCursor"/>.
	/// </summary>
	/// @param <RECORD> type of <seealso cref="AbstractBaseRecord"/> this format handles. </param>
	public interface RecordFormat<RECORD> where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{

		 /// <summary>
		 /// Instantiates a new record to use in <seealso cref="read(AbstractBaseRecord, PageCursor, RecordLoad, int)"/>
		 /// and <seealso cref="write(AbstractBaseRecord, PageCursor, int)"/>. Records may be reused, which is why the instantiation
		 /// is separated from reading and writing.
		 /// </summary>
		 /// <returns> a new record instance, usable in <seealso cref="read(AbstractBaseRecord, PageCursor, RecordLoad, int)"/>
		 /// and <seealso cref="write(AbstractBaseRecord, PageCursor, int)"/>. </returns>
		 RECORD NewRecord();

		 /// <summary>
		 /// Returns the record size for this format. Supplied here is the <seealso cref="StoreHeader store header"/> of the
		 /// owning store, which may contain data affecting the record size.
		 /// </summary>
		 /// <param name="storeHeader"> <seealso cref="StoreHeader"/> with header information from the store. </param>
		 /// <returns> record size of records of this format and store. </returns>
		 int GetRecordSize( StoreHeader storeHeader );

		 /// @deprecated since only being applicable to <seealso cref="DynamicRecord"/> formats. 
		 /// <returns> header size of records of this format. This is only applicable to <seealso cref="DynamicRecord"/>
		 /// format and may not need to be in this interface. </returns>
		 [Obsolete("since only being applicable to <seealso cref=\"DynamicRecord\"/> formats.")]
		 int RecordHeaderSize { get; }

		 /// <summary>
		 /// Quickly determines whether or not record starting right at where the {@code cursor} is placed
		 /// is in use or not.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read data from, placed at the start of record to determine
		 /// in use status of. </param>
		 /// <returns> whether or not the record at where the {@code cursor} is placed is in use. </returns>
		 bool IsInUse( PageCursor cursor );

		 /// <summary>
		 /// Reads data from {@code cursor} of the format specified by this implementation into {@code record}.
		 /// The cursor is placed at the beginning of the record id, which also {@code record}
		 /// <seealso cref="AbstractBaseRecord.getId() refers to"/>.
		 /// </summary>
		 /// <param name="record"> to put read data into, replacing any existing data in that record object. </param>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read data from. </param>
		 /// <param name="mode"> <seealso cref="RecordLoad"/> mode of reading.
		 /// See <seealso cref="RecordStore.getRecord(long, AbstractBaseRecord, RecordLoad)"/> for more information. </param>
		 /// <param name="recordSize"> size of records of this format. This is passed in like this since not all formats
		 /// know the record size in advance, but may be read from store header when opening the store. </param>
		 /// <exception cref="IOException"> on error reading. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void read(RECORD record, Neo4Net.io.pagecache.PageCursor cursor, Neo4Net.kernel.impl.store.record.RecordLoad mode, int recordSize) throws java.io.IOException;
		 void Read( RECORD record, PageCursor cursor, RecordLoad mode, int recordSize );

		 /// <summary>
		 /// Called when all changes about a record has been gathered
		 /// and before it's time to convert into a command. The original reason for introducing this is the
		 /// thing with record units, where we need to know whether or not a record will span two units
		 /// before even writing to the log as a command. The format is the pluggable IEntity which knows
		 /// about the format and therefore the potential length of it and can update the given record with
		 /// additional information which needs to be written to the command, carried back inside the record
		 /// itself.
		 /// </summary>
		 /// <param name="record"> record to prepare, potentially updating it with more information before converting
		 /// into a command. </param>
		 /// <param name="recordSize"> size of each record. </param>
		 /// <param name="idSequence"> source of new ids if such are required be generated. </param>
		 void Prepare( RECORD record, int recordSize, IdSequence idSequence );

		 /// <summary>
		 /// Writes record contents to the {@code cursor} in the format specified by this implementation.
		 /// </summary>
		 /// <param name="record"> containing data to write. </param>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to write the record data into. </param>
		 /// <param name="recordSize"> size of records of this format. This is passed in like this since not all formats
		 /// know the record size in advance, but may be read from store header when opening the store. </param>
		 /// <exception cref="IOException"> on error writing. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void write(RECORD record, Neo4Net.io.pagecache.PageCursor cursor, int recordSize) throws java.io.IOException;
		 void Write( RECORD record, PageCursor cursor, int recordSize );

		 /// <param name="record"> to obtain "next" reference from. </param>
		 /// <returns> "next" reference of records of this type. </returns>
		 /// <seealso cref= RecordStore#getNextRecordReference(AbstractBaseRecord) </seealso>
		 long GetNextRecordReference( RECORD record );

		 /// <summary>
		 /// Can be used to compare against another <seealso cref="RecordFormat"/>, returns {@code true} the format
		 /// represented by {@code obj} is the exact same as this format.
		 /// </summary>
		 /// <param name="otherFormat"> other format to compare with. </param>
		 /// <returns> whether or not the other format is the same as this one. </returns>
		 boolean ( object otherFormat );

		 /// <summary>
		 /// To match <seealso cref="equals(object)"/>.
		 /// </summary>
		 int ();

		 /// <summary>
		 /// Maximum number that can be used to as id in specified format </summary>
		 /// <returns> maximum possible id </returns>
		 long MaxId { get; }
	}

	public static class RecordFormat_Fields
	{
		 public const int NO_RECORD_SIZE = 1;
	}

}