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
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using IdValidator = Neo4Net.Kernel.impl.store.id.validation.IdValidator;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;

	/// <summary>
	/// Basic abstract implementation of a <seealso cref="RecordFormat"/> implementing most functionality except
	/// <seealso cref="RecordFormat.read(AbstractBaseRecord, PageCursor, org.neo4j.kernel.impl.store.record.RecordLoad, int)"/> and
	/// <seealso cref="RecordFormat.write(AbstractBaseRecord, PageCursor, int)"/>.
	/// </summary>
	/// @param <RECORD> type of record. </param>
	public abstract class BaseRecordFormat<RECORD> : RecordFormat<RECORD> where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		public abstract void Write( RECORD record, PageCursor cursor, int recordSize );
		public abstract void Read( RECORD record, PageCursor cursor, Neo4Net.Kernel.Impl.Store.Records.RecordLoad mode, int recordSize );
		public abstract bool IsInUse( PageCursor cursor );
		public abstract AbstractBaseRecord NewRecord();
		 public static readonly int InUseBit = 0b0000_0001;
		 public static readonly System.Func<StoreHeader, int> IntStoreHeaderReader = header => ( ( IntStoreHeader ) header ).value();

		 public static System.Func<StoreHeader, int> FixedRecordSize( int recordSize )
		 {
			  return header => recordSize;
		 }

		 private readonly System.Func<StoreHeader, int> _recordSize;
		 private readonly int _recordHeaderSize;
		 private readonly long _maxId;

		 protected internal BaseRecordFormat( System.Func<StoreHeader, int> recordSize, int recordHeaderSize, int idBits )
		 {
			  this._recordSize = recordSize;
			  this._recordHeaderSize = recordHeaderSize;
			  this._maxId = ( 1L << idBits ) - 1;
		 }

		 public override int GetRecordSize( StoreHeader header )
		 {
			  return _recordSize.apply( header );
		 }

		 public virtual int RecordHeaderSize
		 {
			 get
			 {
				  return _recordHeaderSize;
			 }
		 }

		 public override long GetNextRecordReference( RECORD record )
		 {
			  return Record.NULL_REFERENCE.intValue();
		 }

		 public static long LongFromIntAndMod( long @base, long modifier )
		 {
			  return modifier == 0 && IdValidator.isReservedId( @base ) ? -1 : @base | modifier;
		 }

		 public override void Prepare( RECORD record, int recordSize, IdSequence idSequence )
		 { // Do nothing by default
		 }

		 public override bool Equals( object obj )
		 {
			  return obj != null && this.GetType().Equals(obj.GetType());
		 }

		 public override int GetHashCode()
		 {
			  return this.GetType().GetHashCode();

		 }

		 public long MaxId
		 {
			 get
			 {
				  return _maxId;
			 }
		 }
	}

}