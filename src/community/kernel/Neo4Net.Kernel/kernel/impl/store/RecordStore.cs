using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store
{

	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Neo4Net.Collections.Helpers;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

	/// <summary>
	/// A store for <seealso cref="updateRecord(AbstractBaseRecord) updating"/> and
	/// <seealso cref="getRecord(long, AbstractBaseRecord, RecordLoad) getting"/> records.
	/// 
	/// There are two ways of getting records, either one-by-one using
	/// <seealso cref="getRecord(long, AbstractBaseRecord, RecordLoad)"/>, passing in record retrieved from <seealso cref="newRecord()"/>.
	/// This to make a conscious decision about who will create the record instance and in that process figure out
	/// ways to reduce number of record instances created.
	/// <para>
	/// The other way is to use <seealso cref="openPageCursorForReading(long)"/> to open a cursor and use it to read records using
	/// <seealso cref="getRecordByCursor(long, AbstractBaseRecord, RecordLoad, PageCursor)"/>. A <seealso cref="PageCursor"/> can be ket open
	/// to read multiple records before closing it.
	/// 
	/// </para>
	/// </summary>
	/// @param <RECORD> type of <seealso cref="AbstractBaseRecord"/>. </param>
	public interface RecordStore<RECORD> : IdSequence where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		 /// <returns> the <seealso cref="File"/> that backs this store. </returns>
		 File StorageFile { get; }

		 /// <returns> high id of this store, i.e an id higher than any in use record. </returns>
		 long HighId { get; }

		 /// <returns> highest id in use in this store. </returns>
		 long HighestPossibleIdInUse { get;set; }


		 /// <returns> a new record instance for receiving data by <seealso cref="getRecord(long, AbstractBaseRecord, RecordLoad)"/>. </returns>
		 RECORD NewRecord();

		 /// <summary>
		 /// Reads a record from the store into {@code target}. Depending on <seealso cref="RecordLoad"/> given there will
		 /// be different behavior, although the {@code target} record will be marked with the specified
		 /// {@code id} after participating in this method call.
		 /// <ul>
		 /// <li><seealso cref="RecordLoad.CHECK"/>: As little data as possible is read to determine whether or not the record
		 ///     is in use. If not in use then no more data will be loaded into the target record and
		 ///     the the data of the record will be <seealso cref="AbstractBaseRecord.clear() cleared"/>.</li>
		 /// <li><seealso cref="RecordLoad.NORMAL"/>: Just like <seealso cref="RecordLoad.CHECK"/>, but with the difference that
		 ///     an <seealso cref="InvalidRecordException"/> will be thrown if the record isn't in use.</li>
		 /// <li><seealso cref="RecordLoad.FORCE"/>: The entire contents of the record will be loaded into the target record
		 ///     regardless if the record is in use or not. This leaves no guarantees about the data in the record
		 ///     after this method call, except that the id will be the specified {@code id}.
		 /// </summary>
		 /// <param name="id"> the id of the record to load. </param>
		 /// <param name="target"> record where data will be loaded into. This record will have its id set to the specified
		 /// {@code id} as part of this method call. </param>
		 /// <param name="mode"> loading behaviour, read more in method description. </param>
		 /// <returns> the record that was passed in, for convenience. </returns>
		 /// <exception cref="InvalidRecordException"> if record not in use and the {@code mode} allows for throwing. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: RECORD getRecord(long id, RECORD target, org.Neo4Net.kernel.impl.store.record.RecordLoad mode) throws InvalidRecordException;
		 RECORD GetRecord( long id, RECORD target, RecordLoad mode );

		 /// <summary>
		 /// Opens a <seealso cref="PageCursor"/> on this store, capable of reading records using
		 /// <seealso cref="getRecordByCursor(long, AbstractBaseRecord, RecordLoad, PageCursor)"/>.
		 /// The caller is responsible for closing it when done with it.
		 /// </summary>
		 /// <param name="id"> cursor will initially be placed at the page containing this record id. </param>
		 /// <returns> PageCursor for reading records. </returns>
		 PageCursor OpenPageCursorForReading( long id );

		 /// <summary>
		 /// Reads a record from the store into {@code target}, see
		 /// <seealso cref="RecordStore.getRecord(long, AbstractBaseRecord, RecordLoad)"/>.
		 /// <para>
		 /// The provided page cursor will be used to get the record, and in doing this it will be redirected to the
		 /// correct page if needed.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="id"> the record id, understood to be the absolute reference to the store. </param>
		 /// <param name="target"> the record to fill. </param>
		 /// <param name="mode"> loading behaviour, read more in <seealso cref="RecordStore.getRecord(long, AbstractBaseRecord, RecordLoad)"/>. </param>
		 /// <param name="cursor"> the PageCursor to use for record loading. </param>
		 /// <exception cref="InvalidRecordException"> if record not in use and the {@code mode} allows for throwing. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void getRecordByCursor(long id, RECORD target, org.Neo4Net.kernel.impl.store.record.RecordLoad mode, org.Neo4Net.io.pagecache.PageCursor cursor) throws InvalidRecordException;
		 void GetRecordByCursor( long id, RECORD target, RecordLoad mode, PageCursor cursor );

		 /// <summary>
		 /// Reads a record from the store into {@code target}, see
		 /// <seealso cref="RecordStore.getRecord(long, AbstractBaseRecord, RecordLoad)"/>.
		 /// <para>
		 /// This method requires that the cursor page and offset point to the first byte of the record in target on calling.
		 /// The provided page cursor will be used to get the record, and in doing this it will be redirected to the
		 /// next page if the input record was the last on it's page.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="target"> the record to fill. </param>
		 /// <param name="mode"> loading behaviour, read more in <seealso cref="RecordStore.getRecord(long, AbstractBaseRecord, RecordLoad)"/>. </param>
		 /// <param name="cursor"> the PageCursor to use for record loading. </param>
		 /// <exception cref="InvalidRecordException"> if record not in use and the {@code mode} allows for throwing. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nextRecordByCursor(RECORD target, org.Neo4Net.kernel.impl.store.record.RecordLoad mode, org.Neo4Net.io.pagecache.PageCursor cursor) throws InvalidRecordException;
		 void NextRecordByCursor( RECORD target, RecordLoad mode, PageCursor cursor );

		 /// <summary>
		 /// For stores that have other stores coupled underneath, the "top level" record will have a flag
		 /// saying whether or not it's light. Light means that no records from the coupled store have been loaded yet.
		 /// This method can load those records and enrich the target record with those, marking it as heavy.
		 /// </summary>
		 /// <param name="record"> record to make heavy, if not already. </param>
		 void EnsureHeavy( RECORD record );

		 /// <summary>
		 /// Reads records that belong together, a chain of records that as a whole forms the entirety of a data item.
		 /// </summary>
		 /// <param name="firstId"> record id of the first record to start loading from. </param>
		 /// <param name="mode"> <seealso cref="RecordLoad"/> mode. </param>
		 /// <returns> <seealso cref="System.Collections.ICollection"/> of records in the loaded chain. </returns>
		 /// <exception cref="InvalidRecordException"> if some record not in use and the {@code mode} is allows for throwing. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<RECORD> getRecords(long firstId, org.Neo4Net.kernel.impl.store.record.RecordLoad mode) throws InvalidRecordException;
		 IList<RECORD> GetRecords( long firstId, RecordLoad mode );

		 /// <summary>
		 /// Returns another record id which the given {@code record} references, if it exists in a chain of records.
		 /// </summary>
		 /// <param name="record"> to read the "next" reference from. </param>
		 /// <returns> record id of "next" record that the given {@code record} references, or <seealso cref="Record.NULL_REFERENCE"/>
		 /// if the record doesn't reference a next record. </returns>
		 long GetNextRecordReference( RECORD record );

		 /// <summary>
		 /// Updates this store with the contents of {@code record} at the record id
		 /// <seealso cref="AbstractBaseRecord.getId() specified"/> by the record. The whole record will be written if
		 /// the given record is <seealso cref="AbstractBaseRecord.inUse() in use"/>, not necessarily so if it's not in use.
		 /// </summary>
		 /// <param name="record"> containing data to write to this store at the <seealso cref="AbstractBaseRecord.getId() id"/>
		 /// specified by the record. </param>
		 void UpdateRecord( RECORD record );

		 /// <summary>
		 /// Lets {@code record} be processed by <seealso cref="Processor"/>.
		 /// </summary>
		 /// <param name="processor"> <seealso cref="Processor"/> of records. </param>
		 /// <param name="record"> to process. </param>
		 /// <exception cref="FAILURE"> if the processor fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, RECORD record) throws FAILURE;
		 void accept<FAILURE>( RecordStore_Processor<FAILURE> processor, RECORD record );

		 /// <returns> number of bytes each record in this store occupies. All records in a store is of the same size. </returns>
		 int RecordSize { get; }

		 /// @deprecated since it's exposed through the generic <seealso cref="RecordStore"/> interface although only
		 /// applicable to one particular type of of implementation of it. 
		 /// <returns> record "data" size, only applicable to dynamic record stores where record size may be specified
		 /// at creation time and later used every time the store is opened. Data size refers to number of bytes
		 /// of a record without header information, such as "inUse" and "next". </returns>
		 [Obsolete("since it's exposed through the generic <seealso cref=\"RecordStore\"/> interface although only")]
		 int RecordDataSize { get; }

		 /// <returns> underlying storage is assumed to work with pages. This method returns number of records that
		 /// will fit into each page. </returns>
		 int RecordsPerPage { get; }

		 /// <summary>
		 /// Closes this store and releases any resource attached to it.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Flushes all pending <seealso cref="updateRecord(AbstractBaseRecord) updates"/> to underlying storage.
		 /// This call is blocking and will ensure all updates since last call to this method are durable
		 /// once the call returns.
		 /// </summary>
		 void Flush();

		 /// <summary>
		 /// Some stores may have meta data stored in the header of the store file. Since all records in a store
		 /// are of the same size the means of storing that meta data is to occupy one or more records at the
		 /// beginning of the store (0...).
		 /// </summary>
		 /// <returns> the number of records in the beginning of the file that are reserved for header meta data. </returns>
		 int NumberOfReservedLowIds { get; }

		 /// <summary>
		 /// Returns store header (see <seealso cref="getNumberOfReservedLowIds()"/>) as {@code int}. Exposed like this
		 /// for convenience since all known store headers are ints.
		 /// </summary>
		 /// <returns> store header as an int value, e.g the first 4 bytes of the first (reserved) record in this store. </returns>
		 int StoreHeaderInt { get; }

		 /// <summary>
		 /// Called once all changes to a record is ready to be converted into a command.
		 /// </summary>
		 /// <param name="record"> record to prepare, potentially updating it with more information before converting into a command. </param>
		 void PrepareForCommit( RECORD record );

		 /// <summary>
		 /// Called once all changes to a record is ready to be converted into a command.
		 /// WARNING this is for advanced use, please consider using <seealso cref="prepareForCommit(AbstractBaseRecord)"/> instead.
		 /// </summary>
		 /// <param name="record"> record to prepare, potentially updating it with more information before converting into a command. </param>
		 /// <param name="idSequence"> <seealso cref="IdSequence"/> to use for potentially generating additional ids required by this record. </param>
		 void PrepareForCommit( RECORD record, IdSequence idSequence );

		 /// <summary>
		 /// Scan the given range of records both inclusive, and pass all the in-use ones to the given processor, one by one.
		 /// 
		 /// The record passed to the NodeRecordScanner is reused instead of reallocated for every record, so it must be
		 /// cloned if you want to save it for later. </summary>
		 /// <param name="visitor"> <seealso cref="Visitor"/> notified about all records. </param>
		 /// <exception cref="Exception"> on error reading from store. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <EXCEPTION extends Exception> void scanAllRecords(org.Neo4Net.helpers.collection.Visitor<RECORD,EXCEPTION> visitor) throws EXCEPTION;
		 void scanAllRecords<EXCEPTION>( Visitor<RECORD, EXCEPTION> visitor );

		 void FreeId( long id );

		 /// <summary>
		 /// Utility methods for reading records. These are not on the interface itself since it should be
		 /// an explicit choice when to create the record instances passed into it.
		 /// Also for mocking purposes it's less confusing and error prone having only a single method.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <R> R getRecord(RecordStore<R> store, long id, org.Neo4Net.kernel.impl.store.record.RecordLoad mode)
	//	 {
	//		  R record = store.newRecord();
	//		  store.getRecord(id, record, mode);
	//		  return record;
	//	 }

		 /// <summary>
		 /// Utility methods for reading records. These are not on the interface itself since it should be
		 /// an explicit choice when to create the record instances passed into it.
		 /// Also for mocking purposes it's less confusing and error prone having only a single method.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <R> R getRecord(RecordStore<R> store, long id)
	//	 {
	//		  return getRecord(store, id, RecordLoad.NORMAL);
	//	 }
	}

	public static class RecordStore_Fields
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 public static readonly System.Predicate<AbstractBaseRecord> InUse = AbstractBaseRecord::inUse;
	}

	 public class RecordStore_Delegator<R> : RecordStore<R> where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	 {
		  internal readonly RecordStore<R> Actual;

		  public virtual long HighestPossibleIdInUse
		  {
			  set
			  {
					Actual.HighestPossibleIdInUse = value;
			  }
			  get
			  {
					return Actual.HighestPossibleIdInUse;
			  }
		  }

		  public override R NewRecord()
		  {
				return Actual.newRecord();
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public R getRecord(long id, R target, org.Neo4Net.kernel.impl.store.record.RecordLoad mode) throws InvalidRecordException
		  public override R GetRecord( long id, R target, RecordLoad mode )
		  {
				return Actual.getRecord( id, target, mode );
		  }

		  public override PageCursor OpenPageCursorForReading( long id )
		  {
				return Actual.openPageCursorForReading( id );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void getRecordByCursor(long id, R target, org.Neo4Net.kernel.impl.store.record.RecordLoad mode, org.Neo4Net.io.pagecache.PageCursor cursor) throws InvalidRecordException
		  public override void GetRecordByCursor( long id, R target, RecordLoad mode, PageCursor cursor )
		  {
				Actual.getRecordByCursor( id, target, mode, cursor );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nextRecordByCursor(R target, org.Neo4Net.kernel.impl.store.record.RecordLoad mode, org.Neo4Net.io.pagecache.PageCursor cursor) throws InvalidRecordException
		  public override void NextRecordByCursor( R target, RecordLoad mode, PageCursor cursor )
		  {
				Actual.nextRecordByCursor( target, mode, cursor );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<R> getRecords(long firstId, org.Neo4Net.kernel.impl.store.record.RecordLoad mode) throws InvalidRecordException
		  public override IList<R> GetRecords( long firstId, RecordLoad mode )
		  {
				return Actual.getRecords( firstId, mode );
		  }

		  public override long GetNextRecordReference( R record )
		  {
				return Actual.getNextRecordReference( record );
		  }

		  public RecordStore_Delegator( RecordStore<R> actual )
		  {
				this.Actual = actual;
		  }

		  public override long NextId()
		  {
				return Actual.nextId();
		  }

		  public override IdRange NextIdBatch( int size )
		  {
				return Actual.nextIdBatch( size );
		  }

		  public virtual File StorageFile
		  {
			  get
			  {
					return Actual.StorageFile;
			  }
		  }

		  public virtual long HighId
		  {
			  get
			  {
					return Actual.HighId;
			  }
		  }


		  public override void UpdateRecord( R record )
		  {
				Actual.updateRecord( record );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, R record) throws FAILURE
		  public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, R record ) where FAILURE : Exception
		  {
				Actual.accept( processor, record );
		  }

		  public virtual int RecordSize
		  {
			  get
			  {
					return Actual.RecordSize;
			  }
		  }

		  public virtual int RecordDataSize
		  {
			  get
			  {
					return Actual.RecordDataSize;
			  }
		  }

		  public virtual int RecordsPerPage
		  {
			  get
			  {
					return Actual.RecordsPerPage;
			  }
		  }

		  public virtual int StoreHeaderInt
		  {
			  get
			  {
					return Actual.StoreHeaderInt;
			  }
		  }

		  public override void Close()
		  {
				Actual.close();
		  }

		  public virtual int NumberOfReservedLowIds
		  {
			  get
			  {
					return Actual.NumberOfReservedLowIds;
			  }
		  }

		  public override void Flush()
		  {
				Actual.flush();
		  }

		  public override void EnsureHeavy( R record )
		  {
				Actual.ensureHeavy( record );
		  }

		  public override void PrepareForCommit( R record )
		  {
				Actual.prepareForCommit( record );
		  }

		  public override void PrepareForCommit( R record, IdSequence idSequence )
		  {
				Actual.prepareForCommit( record, idSequence );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <EXCEPTION extends Exception> void scanAllRecords(org.Neo4Net.helpers.collection.Visitor<R,EXCEPTION> visitor) throws EXCEPTION
		  public override void ScanAllRecords<EXCEPTION>( Visitor<R, EXCEPTION> visitor ) where EXCEPTION : Exception
		  {
				Actual.scanAllRecords( visitor );
		  }

		  public override void FreeId( long id )
		  {
				Actual.freeId( id );
		  }
	 }

	 public abstract class RecordStore_Processor<FAILURE> where FAILURE : Exception
	 {
		  // Have it volatile so that it can be stopped from a different thread.
		  internal volatile bool ShouldStop;

		  public virtual void Stop()
		  {
				ShouldStop = true;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processSchema(RecordStore<org.Neo4Net.kernel.impl.store.record.DynamicRecord> store, org.Neo4Net.kernel.impl.store.record.DynamicRecord schema) throws FAILURE;
		  public abstract void ProcessSchema( RecordStore<DynamicRecord> store, DynamicRecord schema );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processNode(RecordStore<org.Neo4Net.kernel.impl.store.record.NodeRecord> store, org.Neo4Net.kernel.impl.store.record.NodeRecord node) throws FAILURE;
		  public abstract void ProcessNode( RecordStore<NodeRecord> store, NodeRecord node );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processRelationship(RecordStore<org.Neo4Net.kernel.impl.store.record.RelationshipRecord> store, org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) throws FAILURE;
		  public abstract void ProcessRelationship( RecordStore<RelationshipRecord> store, RelationshipRecord rel );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processProperty(RecordStore<org.Neo4Net.kernel.impl.store.record.PropertyRecord> store, org.Neo4Net.kernel.impl.store.record.PropertyRecord property) throws FAILURE;
		  public abstract void ProcessProperty( RecordStore<PropertyRecord> store, PropertyRecord property );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processString(RecordStore<org.Neo4Net.kernel.impl.store.record.DynamicRecord> store, org.Neo4Net.kernel.impl.store.record.DynamicRecord string, org.Neo4Net.kernel.impl.store.id.IdType idType) throws FAILURE;
		  public abstract void ProcessString( RecordStore<DynamicRecord> store, DynamicRecord @string, IdType idType );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processArray(RecordStore<org.Neo4Net.kernel.impl.store.record.DynamicRecord> store, org.Neo4Net.kernel.impl.store.record.DynamicRecord array) throws FAILURE;
		  public abstract void ProcessArray( RecordStore<DynamicRecord> store, DynamicRecord array );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processLabelArrayWithOwner(RecordStore<org.Neo4Net.kernel.impl.store.record.DynamicRecord> store, org.Neo4Net.kernel.impl.store.record.DynamicRecord labelArray) throws FAILURE;
		  public abstract void ProcessLabelArrayWithOwner( RecordStore<DynamicRecord> store, DynamicRecord labelArray );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processRelationshipTypeToken(RecordStore<org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord> store, org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord record) throws FAILURE;
		  public abstract void ProcessRelationshipTypeToken( RecordStore<RelationshipTypeTokenRecord> store, RelationshipTypeTokenRecord record );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processPropertyKeyToken(RecordStore<org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord> store, org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord record) throws FAILURE;
		  public abstract void ProcessPropertyKeyToken( RecordStore<PropertyKeyTokenRecord> store, PropertyKeyTokenRecord record );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processLabelToken(RecordStore<org.Neo4Net.kernel.impl.store.record.LabelTokenRecord> store, org.Neo4Net.kernel.impl.store.record.LabelTokenRecord record) throws FAILURE;
		  public abstract void ProcessLabelToken( RecordStore<LabelTokenRecord> store, LabelTokenRecord record );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void processRelationshipGroup(RecordStore<org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord> store, org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord record) throws FAILURE;
		  public abstract void ProcessRelationshipGroup( RecordStore<RelationshipGroupRecord> store, RelationshipGroupRecord record );

		  protected internal virtual R GetRecord<R>( RecordStore<R> store, long id, R into ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		  {
				store.GetRecord( id, into, RecordLoad.FORCE );
				return into;
		  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord> void applyFiltered(RecordStore<R> store, System.Predicate<? super R>... filters) throws FAILURE
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		  public virtual void ApplyFiltered<R>( RecordStore<R> store, params System.Predicate<object>[] filters ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		  {
				Apply( store, Neo4Net.Helpers.progress.ProgressListener_Fields.None, filters );
		  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord> void applyFiltered(RecordStore<R> store, org.Neo4Net.helpers.progress.ProgressListener progressListener, System.Predicate<? super R>... filters) throws FAILURE
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		  public virtual void ApplyFiltered<R>( RecordStore<R> store, ProgressListener progressListener, params System.Predicate<object>[] filters ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		  {
				Apply( store, progressListener, filters );
		  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: <R extends org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord> void apply(RecordStore<R> store, org.Neo4Net.helpers.progress.ProgressListener progressListener, System.Predicate<? super R>... filters) throws FAILURE
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		  internal virtual void Apply<R>( RecordStore<R> store, ProgressListener progressListener, params System.Predicate<object>[] filters ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		  {
				ResourceIterable<R> iterable = Scanner.Scan( store, true, filters );
				using ( ResourceIterator<R> scan = iterable.GetEnumerator() )
				{
					 while ( scan.MoveNext() )
					 {
						  R record = scan.Current;
						  if ( ShouldStop )
						  {
								break;
						  }

						  store.Accept( this, record );
						  progressListener.Set( record.Id );
					 }
					 progressListener.Done();
				}
		  }
	 }

}