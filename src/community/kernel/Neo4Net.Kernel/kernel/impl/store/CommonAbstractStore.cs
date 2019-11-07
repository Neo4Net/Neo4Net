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

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4Net.Collections.Helpers;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.impl.store.format;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using IdValidator = Neo4Net.Kernel.impl.store.id.validation.IdValidator;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Logger = Neo4Net.Logging.Logger;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.ArrayUtil.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.throwIfUnchecked;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PageCacheOpenOptions.ANY_PAGE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_READ_AHEAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.RecordLoad.CHECK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.RecordLoad.NORMAL;

	/// <summary>
	/// Contains common implementation of <seealso cref="RecordStore"/>.
	/// </summary>
	public abstract class CommonAbstractStore<RECORD, HEADER> : RecordStore<RECORD>, IDisposable where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord where HEADER : StoreHeader
	{
		public abstract R GetRecord( RecordStore<R> store, long id );
		public abstract R GetRecord( RecordStore<R> store, long id, record.RecordLoad mode );
		public abstract void Accept( RecordStore_Processor<FAILURE> processor, RECORD record );
		 internal const string UNKNOWN_VERSION = "Unknown";

		 protected internal readonly Config Configuration;
		 protected internal readonly PageCache PageCache;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly IdType IdTypeConflict;
		 protected internal readonly IdGeneratorFactory IdGeneratorFactory;
		 protected internal readonly Log Log;
		 protected internal readonly string StoreVersion;
		 protected internal readonly RecordFormat<RECORD> RecordFormat;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly File StorageFileConflict;
		 private readonly File _idFile;
		 private readonly string _typeDescriptor;
		 protected internal PagedFile PagedFile;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal int RecordSizeConflict;
		 private IdGenerator _idGenerator;
		 private bool _storeOk = true;
		 private Exception _causeOfStoreNotOk;

		 private readonly StoreHeaderFormat<HEADER> _storeHeaderFormat;
		 private HEADER _storeHeader;

		 private readonly OpenOption[] _openOptions;

		 /// <summary>
		 /// Opens and validates the store contained in <CODE>file</CODE>
		 /// loading any configuration defined in <CODE>config</CODE>. After
		 /// validation the <CODE>initStorage</CODE> method is called.
		 /// <para>
		 /// If the store had a clean shutdown it will be marked as <CODE>ok</CODE>
		 /// and the <seealso cref="getStoreOk()"/> method will return true.
		 /// If a problem was found when opening the store the <seealso cref="makeStoreOk()"/>
		 /// must be invoked.
		 /// </para>
		 /// <para>
		 /// throws IOException if the unable to open the storage or if the
		 /// <CODE>initStorage</CODE> method fails
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="databaseName"> database name </param>
		 /// <param name="idType"> The Id used to index into this store </param>
		 public CommonAbstractStore( File file, File idFile, Config configuration, IdType idType, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, string typeDescriptor, RecordFormat<RECORD> recordFormat, StoreHeaderFormat<HEADER> storeHeaderFormat, string storeVersion, params OpenOption[] openOptions )
		 {
			  this.StorageFileConflict = file;
			  this._idFile = idFile;
			  this.Configuration = configuration;
			  this.IdGeneratorFactory = idGeneratorFactory;
			  this.PageCache = pageCache;
			  this.IdTypeConflict = idType;
			  this._typeDescriptor = typeDescriptor;
			  this.RecordFormat = recordFormat;
			  this._storeHeaderFormat = storeHeaderFormat;
			  this.StoreVersion = storeVersion;
			  this._openOptions = openOptions;
			  this.Log = logProvider.getLog( this.GetType() );
		 }

		 internal virtual void Initialize( bool createIfNotExists )
		 {
			  try
			  {
					CheckAndLoadStorage( createIfNotExists );
			  }
			  catch ( Exception e )
			  {
					CloseAndThrow( e );
			  }
		 }

		 private void CloseAndThrow( Exception e )
		 {
			  if ( PagedFile != null )
			  {
					try
					{
						 CloseStoreFile();
					}
					catch ( IOException failureToClose )
					{
						 // Not really a suppressed exception, but we still want to throw the real exception, e,
						 // but perhaps also throw this in there or convenience.
						 e.addSuppressed( failureToClose );
					}
			  }
			  throwIfUnchecked( e );
			  throw new Exception( e );
		 }

		 /// <summary>
		 /// Returns the type and version that identifies this store.
		 /// </summary>
		 /// <returns> This store's implementation type and version identifier </returns>
		 public virtual string TypeDescriptor
		 {
			 get
			 {
				  return _typeDescriptor;
			 }
		 }

		 /// <summary>
		 /// This method is called by constructors. Checks the header record and loads the store.
		 /// <para>
		 /// Note: This method will map the file with the page cache. The store file must not
		 /// be accessed directly until it has been unmapped - the store file must only be
		 /// accessed through the page cache.
		 /// </para>
		 /// </summary>
		 /// <param name="createIfNotExists"> If true, creates and initialises the store file if it does not exist already. If false,
		 /// this method will instead throw an exception in that situation. </param>
		 protected internal virtual void CheckAndLoadStorage( bool createIfNotExists )
		 {
			  int pageSize = PageCache.pageSize();
			  int filePageSize;
			  try
			  {
					  using ( PagedFile pagedFile = PageCache.map( StorageFileConflict, pageSize, ANY_PAGE_SIZE ) )
					  {
						ExtractHeaderRecord( pagedFile );
						filePageSize = PageCache.pageSize() - PageCache.pageSize() % RecordSize;
					  }
			  }
			  catch ( Exception e ) when ( e is NoSuchFileException || e is StoreNotFoundException )
			  {
					if ( createIfNotExists )
					{
						 try
						 {
							  CreateStore( pageSize );
							  return;
						 }
						 catch ( IOException e1 )
						 {
							  e.addSuppressed( e1 );
						 }
					}
					if ( e is StoreNotFoundException )
					{
						 throw ( StoreNotFoundException ) e;
					}
					throw new StoreNotFoundException( "Store file not found: " + StorageFileConflict, e );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Unable to open store file: " + StorageFileConflict, e );
			  }
			  LoadStorage( filePageSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createStore(int pageSize) throws java.io.IOException
		 private void CreateStore( int pageSize )
		 {
			  using ( PagedFile file = PageCache.map( StorageFileConflict, pageSize, StandardOpenOption.CREATE ) )
			  {
					InitializeNewStoreFile( file );
			  }
			  CheckAndLoadStorage( false );
		 }

		 private void LoadStorage( int filePageSize )
		 {
			  try
			  {
					PagedFile = PageCache.map( StorageFileConflict, filePageSize, _openOptions );
					LoadIdGenerator();
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Unable to open store file: " + StorageFileConflict, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void initialiseNewStoreFile(Neo4Net.io.pagecache.PagedFile file) throws java.io.IOException
		 protected internal virtual void InitializeNewStoreFile( PagedFile file )
		 {
			  if ( NumberOfReservedLowIds > 0 )
			  {
					using ( PageCursor pageCursor = file.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 if ( pageCursor.Next() )
						 {
							  pageCursor.Offset = 0;
							  CreateHeaderRecord( pageCursor );
							  if ( pageCursor.CheckAndClearBoundsFlag() )
							  {
									throw new UnderlyingStorageException( "Out of page bounds when writing header; page size too small: " + PageCache.pageSize() + " bytes." );
							  }
						 }
					}
			  }

			  // Determine record size right after writing the header since some stores
			  // use it when initializing their stores to write some records.
			  RecordSizeConflict = DetermineRecordSize();

			  IdGeneratorFactory.create( _idFile, NumberOfReservedLowIds, false );
		 }

		 private void CreateHeaderRecord( PageCursor cursor )
		 {
			  int offset = cursor.Offset;
			  _storeHeaderFormat.writeHeader( cursor );
			  cursor.Offset = offset;
			  ReadHeaderAndInitializeRecordFormat( cursor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void extractHeaderRecord(Neo4Net.io.pagecache.PagedFile pagedFile) throws java.io.IOException
		 private void ExtractHeaderRecord( PagedFile pagedFile )
		 {
			  if ( NumberOfReservedLowIds > 0 )
			  {
					using ( PageCursor pageCursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{

						 if ( pageCursor.Next() )
						 {
							  do
							  {
									pageCursor.Offset = 0;
									ReadHeaderAndInitializeRecordFormat( pageCursor );
							  } while ( pageCursor.ShouldRetry() );
							  if ( pageCursor.CheckAndClearBoundsFlag() )
							  {
									throw new UnderlyingStorageException( "Out of page bounds when reading header; page size too small: " + PageCache.pageSize() + " bytes." );
							  }
						 }
						 else
						 {
							  throw new StoreNotFoundException( "Fail to read header record of store file: " + StorageFileConflict );
						 }
					}
			  }
			  else
			  {
					ReadHeaderAndInitializeRecordFormat( null );
			  }
			  RecordSizeConflict = DetermineRecordSize();
		 }

		 protected internal virtual long PageIdForRecord( long id )
		 {
			  return RecordPageLocationCalculator.PageIdForRecord( id, PagedFile.pageSize(), RecordSizeConflict );
		 }

		 protected internal virtual int OffsetForId( long id )
		 {
			  return RecordPageLocationCalculator.OffsetForId( id, PagedFile.pageSize(), RecordSizeConflict );
		 }

		 public virtual int RecordsPerPage
		 {
			 get
			 {
				  return PagedFile.pageSize() / RecordSizeConflict;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] getRawRecordData(long id) throws java.io.IOException
		 public virtual sbyte[] GetRawRecordData( long id )
		 {
			  sbyte[] data = new sbyte[RecordSizeConflict];
			  long pageId = PageIdForRecord( id );
			  int offset = OffsetForId( id );
			  using ( PageCursor cursor = PagedFile.io( pageId, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					if ( cursor.Next() )
					{
						 cursor.Offset = offset;
						 cursor.Mark();
						 do
						 {
							  cursor.SetOffsetToMark();
							  cursor.GetBytes( data );
						 } while ( cursor.ShouldRetry() );
						 CheckForDecodingErrors( cursor, id, CHECK );
					}
			  }
			  return data;
		 }

		 /// <summary>
		 /// This method is called when opening the store to extract header data and determine things like
		 /// record size of the specific record format for this store. Some formats rely on information
		 /// in the store header, that's why it happens at this stage.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> initialized at the start of the store header where header information
		 /// can be read if need be. This can be {@code null} if this store has no store header. The initialization
		 /// of the record format still happens in here. </param>
		 /// <exception cref="IOException"> if there were problems reading header information. </exception>
		 private void ReadHeaderAndInitializeRecordFormat( PageCursor cursor )
		 {
			  _storeHeader = _storeHeaderFormat.readHeader( cursor );
		 }

		 private void LoadIdGenerator()
		 {
			  try
			  {
					if ( _storeOk )
					{
						 OpenIdGenerator();
					}
					// else we will rebuild the id generator after recovery, and we don't want to have the id generator
					// picking up calls to freeId during recovery.
			  }
			  catch ( InvalidIdGeneratorException e )
			  {
					StoreNotOk = e;
			  }
			  finally
			  {
					if ( !StoreOk )
					{
						 Log.debug( StorageFileConflict + " non clean shutdown detected" );
					}
			  }
		 }

		 public virtual bool IsInUse( long id )
		 {
			  long pageId = PageIdForRecord( id );
			  int offset = OffsetForId( id );

			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_READ_LOCK ) )
					  {
						bool recordIsInUse = false;
						if ( cursor.Next() )
						{
							 cursor.Offset = offset;
							 cursor.Mark();
							 do
							 {
								  cursor.SetOffsetToMark();
								  recordIsInUse = IsInUse( cursor );
							 } while ( cursor.ShouldRetry() );
							 CheckForDecodingErrors( cursor, id, NORMAL );
						}
						return recordIsInUse;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 /// <summary>
		 /// DANGER: make sure to always close this cursor.
		 /// </summary>
		 public override PageCursor OpenPageCursorForReading( long id )
		 {
			  try
			  {
					long pageId = PageIdForRecord( id );
					return PagedFile.io( pageId, PF_SHARED_READ_LOCK );
			  }
			  catch ( IOException e )
			  {
					// TODO: think about what we really should be doing with the exception handling here...
					throw new UnderlyingStorageException( e );
			  }
		 }

		 /// <summary>
		 /// Should rebuild the id generator from scratch.
		 /// <para>
		 /// Note: This method may be called both while the store has the store file mapped in the
		 /// page cache, and while the store file is not mapped. Implementers must therefore
		 /// map their own temporary PagedFile for the store file, and do their file IO through that,
		 /// if they need to access the data in the store file.
		 /// </para>
		 /// </summary>
		 internal void RebuildIdGenerator()
		 {
			  int blockSize = RecordSize;
			  if ( blockSize <= 0 )
			  {
					throw new InvalidRecordException( "Illegal blockSize: " + blockSize );
			  }

			  Log.info( "Rebuilding id generator for[" + StorageFile + "] ..." );
			  CloseIdGenerator();
			  CreateIdGenerator( _idFile );
			  OpenIdGenerator();

			  long defraggedCount = 0;
			  bool fastRebuild = IsOnlyFastIdGeneratorRebuildEnabled( Configuration );

			  try
			  {
					long foundHighId = ScanForHighId();
					HighId = foundHighId;
					if ( !fastRebuild )
					{
						 using ( PageCursor cursor = PagedFile.io( 0, PF_SHARED_WRITE_LOCK | PF_READ_AHEAD ) )
						 {
							  defraggedCount = RebuildIdGeneratorSlow( cursor, RecordsPerPage, blockSize, foundHighId );
						 }
					}
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Unable to rebuild id generator " + StorageFile, e );
			  }

			  Log.info( "[" + StorageFile + "] high id=" + HighId + " (defragged=" + defraggedCount + ")" );
			  Log.info( StorageFile + " rebuild id generator, highId=" + HighId + " defragged count=" + defraggedCount );

			  if ( !fastRebuild )
			  {
					CloseIdGenerator();
					OpenIdGenerator();
			  }
		 }

		 protected internal virtual bool IsOnlyFastIdGeneratorRebuildEnabled( Config config )
		 {
			  return config.Get( GraphDatabaseSettings.rebuild_idgenerators_fast );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long rebuildIdGeneratorSlow(Neo4Net.io.pagecache.PageCursor cursor, int recordsPerPage, int blockSize, long foundHighId) throws java.io.IOException
		 private long RebuildIdGeneratorSlow( PageCursor cursor, int recordsPerPage, int blockSize, long foundHighId )
		 {
			  if ( !cursor.WriteLocked )
			  {
					throw new System.ArgumentException( "The store scanning id generator rebuild process requires a page cursor that is write-locked" );
			  }
			  long defragCount = 0;
			  long[] freedBatch = new long[recordsPerPage]; // we process in batches of one page worth of records
			  int startingId = NumberOfReservedLowIds;
			  int defragged;

			  bool done = false;
			  while ( !done && cursor.Next() )
			  {
					long idPageOffset = cursor.CurrentPageId * recordsPerPage;

					defragged = 0;
					for ( int i = startingId; i < recordsPerPage; i++ )
					{
						 int offset = i * blockSize;
						 cursor.Offset = offset;
						 long recordId = idPageOffset + i;
						 if ( recordId >= foundHighId )
						 { // We don't have to go further than the high id we found earlier
							  done = true;
							  break;
						 }

						 if ( !IsInUse( cursor ) )
						 {
							  freedBatch[defragged++] = recordId;
						 }
						 else if ( IsRecordReserved( cursor ) )
						 {
							  cursor.Offset = offset;
							  cursor.PutByte( Record.NOT_IN_USE.byteValue() );
							  cursor.PutInt( 0 );
							  freedBatch[defragged++] = recordId;
						 }
					}
					CheckIdScanCursorBounds( cursor );

					for ( int i = 0; i < defragged; i++ )
					{
						 FreeId( freedBatch[i] );
					}
					defragCount += defragged;
					startingId = 0;
			  }
			  return defragCount;
		 }

		 private void CheckIdScanCursorBounds( PageCursor cursor )
		 {
			  if ( cursor.CheckAndClearBoundsFlag() )
			  {
					throw new UnderlyingStorageException( "Out of bounds access on page " + cursor.CurrentPageId + " detected while scanning the " + StorageFileConflict + " file for deleted records" );
			  }
		 }

		 /// <summary>
		 /// Marks this store as "not ok".
		 /// </summary>
		 internal virtual Exception StoreNotOk
		 {
			 set
			 {
				  _storeOk = false;
				  _causeOfStoreNotOk = value;
				  _idGenerator = null; // since we will rebuild it later
			 }
		 }

		 /// <summary>
		 /// If store is "not ok" <CODE>false</CODE> is returned.
		 /// </summary>
		 /// <returns> True if this store is ok </returns>
		 internal virtual bool StoreOk
		 {
			 get
			 {
				  return _storeOk;
			 }
		 }

		 /// <summary>
		 /// Throws cause of not being OK if <seealso cref="getStoreOk()"/> returns {@code false}.
		 /// </summary>
		 internal virtual void CheckStoreOk()
		 {
			  if ( !_storeOk )
			  {
					throw _causeOfStoreNotOk;
			  }
		 }

		 /// <summary>
		 /// Returns the next id for this store's <seealso cref="IdGenerator"/>.
		 /// </summary>
		 /// <returns> The next free id </returns>
		 public override long NextId()
		 {
			  AssertIdGeneratorInitialized();
			  return _idGenerator.nextId();
		 }

		 private void AssertIdGeneratorInitialized()
		 {
			  if ( _idGenerator == null )
			  {
					throw new System.InvalidOperationException( "IdGenerator is not initialized" );
			  }
		 }

		 public override IdRange NextIdBatch( int size )
		 {
			  AssertIdGeneratorInitialized();
			  return _idGenerator.nextIdBatch( size );
		 }

		 /// <summary>
		 /// Frees an id for this store's <seealso cref="IdGenerator"/>.
		 /// </summary>
		 /// <param name="id"> The id to free </param>
		 public override void FreeId( long id )
		 {
			  IdGenerator generator = this._idGenerator;
			  if ( generator != null )
			  {
					generator.FreeId( id );
			  }
			  // else we're deleting records as part of applying transactions during recovery, and that's fine
		 }

		 /// <summary>
		 /// Return the highest id in use. If this store is not OK yet, the high id is calculated from the highest
		 /// in use record on the store, using <seealso cref="scanForHighId()"/>.
		 /// </summary>
		 /// <returns> The high id, i.e. highest id in use + 1. </returns>
		 public virtual long HighId
		 {
			 get
			 {
				  return _idGenerator != null ? _idGenerator.HighId : ScanForHighId();
			 }
			 set
			 {
				  // This method might get called during recovery, where we don't have a reliable id generator yet,
				  // so ignore these calls and let rebuildIdGenerators() figure out the high id after recovery.
				  IdGenerator generator = this._idGenerator;
				  if ( generator != null )
				  {
						//noinspection SynchronizationOnLocalVariableOrMethodParameter
						lock ( generator )
						{
							 if ( value > generator.HighId )
							 {
								  generator.HighId = value;
							 }
						}
				  }
			 }
		 }


		 /// <summary>
		 /// If store is not ok a call to this method will rebuild the {@link
		 /// IdGenerator} used by this store and if successful mark it as OK.
		 /// 
		 /// WARNING: this method must NOT be called if recovery is required, but hasn't performed.
		 /// To remove all negations from the above statement: Only call this method if store is in need of
		 /// recovery and recovery has been performed.
		 /// </summary>
		 internal virtual void MakeStoreOk()
		 {
			  if ( !_storeOk )
			  {
					RebuildIdGenerator();
					_storeOk = true;
					_causeOfStoreNotOk = null;
			  }
		 }

		 /// <summary>
		 /// Returns the name of this store.
		 /// </summary>
		 /// <returns> The name of this store </returns>
		 public virtual File StorageFile
		 {
			 get
			 {
				  return StorageFileConflict;
			 }
		 }

		 /// <summary>
		 /// Opens the <seealso cref="IdGenerator"/> used by this store.
		 /// <para>
		 /// Note: This method may be called both while the store has the store file mapped in the
		 /// page cache, and while the store file is not mapped. Implementers must therefore
		 /// map their own temporary PagedFile for the store file, and do their file IO through that,
		 /// if they need to access the data in the store file.
		 /// </para>
		 /// </summary>
		 internal virtual void OpenIdGenerator()
		 {
			  _idGenerator = IdGeneratorFactory.open( _idFile, IdType, this.scanForHighId, RecordFormat.MaxId );
		 }

		 /// <summary>
		 /// Starts from the end of the file and scans backwards to find the highest in use record.
		 /// Can be used even if <seealso cref="makeStoreOk()"/> hasn't been called. Basically this method should be used
		 /// over <seealso cref="getHighestPossibleIdInUse()"/> and <seealso cref="getHighId()"/> in cases where a store has been opened
		 /// but is in a scenario where recovery isn't possible, like some tooling or migration.
		 /// </summary>
		 /// <returns> the id of the highest in use record + 1, i.e. highId. </returns>
		 protected internal virtual long ScanForHighId()
		 {
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( 0, PF_SHARED_READ_LOCK ) )
					  {
						int recordsPerPage = RecordsPerPage;
						int recordSize = RecordSize;
						long highestId = NumberOfReservedLowIds;
						bool found;
						/*
						 * We do this in chunks of pages instead of one page at a time, the performance impact is significant.
						 * We first pre-fetch a large chunk sequentially, which is then scanned backwards for used records.
						 */
						const long chunkSizeInPages = 256; // 2MiB (8192 bytes/page * 256 pages/chunk)
      
						long chunkEndId = PagedFile.LastPageId;
						while ( chunkEndId >= 0 )
						{
							 // Do pre-fetch of the chunk
							 long chunkStartId = max( chunkEndId - chunkSizeInPages, 0 );
							 PreFetchChunk( cursor, chunkStartId, chunkEndId );
      
							 // Scan pages backwards in the chunk
							 for ( long currentId = chunkEndId; currentId >= chunkStartId && cursor.Next( currentId ); currentId-- )
							 {
								  do
								  {
										found = false;
										// Scan record backwards in the page
										for ( int offset = recordsPerPage * recordSize - recordSize; offset >= 0; offset -= recordSize )
										{
											 cursor.Offset = offset;
											 if ( IsInUse( cursor ) )
											 {
												  // We've found the highest id in use
												  highestId = ( cursor.CurrentPageId * recordsPerPage ) + offset / recordSize + 1;
												  found = true;
												  break;
											 }
										}
								  } while ( cursor.ShouldRetry() );
      
								  CheckIdScanCursorBounds( cursor );
								  if ( found )
								  {
										return highestId;
								  }
							 }
							 chunkEndId = chunkStartId - 1;
						}
      
						return NumberOfReservedLowIds;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Unable to find high id by scanning backwards " + StorageFile, e );
			  }
		 }

		 /// <summary>
		 /// Do a pre-fetch of pages in sequential order on the range [{@code pageIdStart},{@code pageIdEnd}].
		 /// </summary>
		 /// <param name="cursor"> Cursor to pre-fetch on. </param>
		 /// <param name="pageIdStart"> Page id to start pre-fetching from. </param>
		 /// <param name="pageIdEnd"> Page id to end pre-fetching on, inclusive {@code pageIdEnd}. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void preFetchChunk(Neo4Net.io.pagecache.PageCursor cursor, long pageIdStart, long pageIdEnd) throws java.io.IOException
		 private static void PreFetchChunk( PageCursor cursor, long pageIdStart, long pageIdEnd )
		 {
			  for ( long currentPageId = pageIdStart; currentPageId <= pageIdEnd; currentPageId++ )
			  {
					cursor.Next( currentPageId );
			  }
		 }

		 protected internal virtual int DetermineRecordSize()
		 {
			  return RecordFormat.getRecordSize( _storeHeader );
		 }

		 public int RecordSize
		 {
			 get
			 {
				  return RecordSizeConflict;
			 }
		 }

		 public virtual int RecordDataSize
		 {
			 get
			 {
				  return RecordSizeConflict - RecordFormat.RecordHeaderSize;
			 }
		 }

		 private bool IsInUse( PageCursor cursor )
		 {
			  return RecordFormat.isInUse( cursor );
		 }

		 protected internal virtual bool IsRecordReserved( PageCursor cursor )
		 {
			  return false;
		 }

		 private void CreateIdGenerator( File fileName )
		 {
			  IdGeneratorFactory.create( fileName, 0, false );
		 }

		 /// <summary>
		 /// Closed the <seealso cref="IdGenerator"/> used by this store </summary>
		 internal virtual void CloseIdGenerator()
		 {
			  if ( _idGenerator != null )
			  {
					_idGenerator.Dispose();
			  }
		 }

		 public override void Flush()
		 {
			  try
			  {
					PagedFile.flushAndForce();
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Failed to flush", e );
			  }
		 }

		 /// <summary>
		 /// Checks if this store is closed and throws exception if it is.
		 /// </summary>
		 /// <exception cref="IllegalStateException"> if the store is closed </exception>
		 internal virtual void AssertNotClosed()
		 {
			  if ( PagedFile == null )
			  {
					throw new System.InvalidOperationException( this + " for file '" + StorageFileConflict + "' is closed" );
			  }
		 }

		 /// <summary>
		 /// Closes this store. This will cause all buffers and channels to be closed.
		 /// Requesting an operation from after this method has been invoked is
		 /// illegal and an exception will be thrown.
		 /// <para>
		 /// This method will start by invoking the <seealso cref="closeStoreFile()"/> method
		 /// giving the implementing store way to do anything that it needs to do
		 /// before the pagedFile is closed.
		 /// </para>
		 /// </summary>
		 public override void Close()
		 {
			  try
			  {
					CloseStoreFile();
			  }
			  catch ( Exception e ) when ( e is IOException || e is System.InvalidOperationException )
			  {
					throw new UnderlyingStorageException( "Failed to close store file: " + StorageFile, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeStoreFile() throws java.io.IOException
		 private void CloseStoreFile()
		 {
			  try
			  {
					/*
					 * Note: the closing ordering here is important!
					 * It is the case since we wand to mark the id generator as closed cleanly ONLY IF
					 * also the store file is cleanly shutdown.
					 */
					if ( PagedFile != null )
					{
						 PagedFile.close();
					}
					if ( _idGenerator != null )
					{
						 if ( contains( _openOptions, DELETE_ON_CLOSE ) )
						 {
							  _idGenerator.delete();
						 }
						 else
						 {
							  _idGenerator.Dispose();
						 }
					}
			  }
			  finally
			  {
					PagedFile = null;
			  }
		 }

		 /// <returns> The highest possible id in use, -1 if no id in use. </returns>
		 public virtual long HighestPossibleIdInUse
		 {
			 get
			 {
				  return _idGenerator != null ? _idGenerator.HighestPossibleIdInUse : ScanForHighId() - 1;
			 }
			 set
			 {
				  HighId = value + 1;
			 }
		 }


		 /// <returns> The total number of ids in use. </returns>
		 public virtual long NumberOfIdsInUse
		 {
			 get
			 {
				  AssertIdGeneratorInitialized();
				  return _idGenerator.NumberOfIdsInUse;
			 }
		 }

		 /// <returns> the number of records at the beginning of the store file that are reserved for other things
		 /// than actual records. Stuff like permanent configuration data. </returns>
		 public virtual int NumberOfReservedLowIds
		 {
			 get
			 {
				  return _storeHeaderFormat.numberOfReservedRecords();
			 }
		 }

		 public virtual IdType IdType
		 {
			 get
			 {
				  return IdTypeConflict;
			 }
		 }

		 internal virtual void LogVersions( Logger logger )
		 {
			  logger.Log( "  " + TypeDescriptor + " " + StoreVersion );
		 }

		 internal virtual void LogIdUsage( Logger logger )
		 {
			  logger.Log( string.Format( "  {0}: used={1} high={2}", TypeDescriptor, NumberOfIdsInUse, HighestPossibleIdInUse ) );
		 }

		 /// <summary>
		 /// Visits this store, and any other store managed by this store.
		 /// TODO this could, and probably should, replace all override-and-do-the-same-thing-to-all-my-managed-stores
		 /// methods like:
		 /// <seealso cref="makeStoreOk()"/>,
		 /// <seealso cref="close()"/> (where that method could be deleted all together and do a visit in <seealso cref="close()"/>),
		 /// <seealso cref="logIdUsage(Logger)"/>,
		 /// <seealso cref="logVersions(Logger)"/>
		 /// For a good samaritan to pick up later.
		 /// </summary>
		 internal virtual void VisitStore( Visitor<CommonAbstractStore<RECORD, HEADER>, Exception> visitor )
		 {
			  visitor.Visit( this );
		 }

		 /// <summary>
		 /// Called from the part of the code that starts the <seealso cref="MetaDataStore"/> and friends, together with any
		 /// existing transaction log, seeing that there are transactions to recover. Now, this shouldn't be
		 /// needed because the state of the id generator _should_ reflect this fact, but turns out that,
		 /// given HA and the nature of the .id files being like orphans to the rest of the store, we just
		 /// can't trust that to be true. If we happen to have id generators open during recovery we delegate
		 /// <seealso cref="freeId(long)"/> calls to <seealso cref="IdGenerator.freeId(long)"/> and since the id generator is most likely
		 /// out of date w/ regards to high id, it may very well blow up.
		 /// 
		 /// This also marks the store as not OK. A call to <seealso cref="makeStoreOk()"/> is needed once recovery is complete.
		 /// </summary>
		 internal void DeleteIdGenerator()
		 {
			  if ( _idGenerator != null )
			  {
					_idGenerator.delete();
					_idGenerator = null;
					StoreNotOk = new System.InvalidOperationException( "IdGenerator is not initialized" );
			  }
		 }

		 public override long GetNextRecordReference( RECORD record )
		 {
			  return RecordFormat.getNextRecordReference( record );
		 }

		 public override RECORD NewRecord()
		 {
			  return RecordFormat.newRecord();
		 }

		 /// <summary>
		 /// Acquires a <seealso cref="PageCursor"/> from the <seealso cref="PagedFile store file"/> and reads the requested record
		 /// in the correct page and offset.
		 /// </summary>
		 /// <param name="id"> the record id. </param>
		 /// <param name="record"> the record instance to load the data into. </param>
		 /// <param name="mode"> how strict to be when loading, f.ex <seealso cref="RecordLoad.FORCE"/> will always read what's there
		 /// and load into the record, whereas <seealso cref="RecordLoad.NORMAL"/> will throw <seealso cref="InvalidRecordException"/>
		 /// if not in use. </param>
		 public override RECORD GetRecord( long id, RECORD record, RecordLoad mode )
		 {
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( NumberOfReservedLowIds, PF_SHARED_READ_LOCK ) )
					  {
						ReadIntoRecord( id, record, mode, cursor );
						return record;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void getRecordByCursor(long id, RECORD record, Neo4Net.kernel.impl.store.record.RecordLoad mode, Neo4Net.io.pagecache.PageCursor cursor) throws UnderlyingStorageException
		 public override void GetRecordByCursor( long id, RECORD record, RecordLoad mode, PageCursor cursor )
		 {
			  try
			  {
					ReadIntoRecord( id, record, mode, cursor );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readIntoRecord(long id, RECORD record, Neo4Net.kernel.impl.store.record.RecordLoad mode, Neo4Net.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private void ReadIntoRecord( long id, RECORD record, RecordLoad mode, PageCursor cursor )
		 {
			  // Mark the record with this id regardless of whether or not we load the contents of it.
			  // This is done in this method since there are multiple call sites and they all want the id
			  // on that record, so it's to ensure it isn't forgotten.
			  record.Id = id;
			  long pageId = PageIdForRecord( id );
			  int offset = OffsetForId( id );
			  if ( cursor.Next( pageId ) )
			  {
					cursor.Offset = offset;
					ReadRecordFromPage( id, record, mode, cursor );
			  }
			  else
			  {
					VerifyAfterNotRead( record, mode );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void nextRecordByCursor(RECORD record, Neo4Net.kernel.impl.store.record.RecordLoad mode, Neo4Net.io.pagecache.PageCursor cursor) throws UnderlyingStorageException
		 public override void NextRecordByCursor( RECORD record, RecordLoad mode, PageCursor cursor )
		 {
			  if ( cursor.CurrentPageId < -1 )
			  {
					throw new System.ArgumentException( "Pages are assumed to be positive or -1 if not initialized" );
			  }

			  try
			  {
					int offset = cursor.Offset;
					long id = record.Id + 1;
					record.Id = id;
					long pageId = cursor.CurrentPageId;
					if ( offset >= PagedFile.pageSize() || pageId < 0 )
					{
						 if ( !cursor.Next() )
						 {
							  VerifyAfterNotRead( record, mode );
							  return;
						 }
						 cursor.Offset = 0;
					}
					ReadRecordFromPage( id, record, mode, cursor );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readRecordFromPage(long id, RECORD record, Neo4Net.kernel.impl.store.record.RecordLoad mode, Neo4Net.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private void ReadRecordFromPage( long id, RECORD record, RecordLoad mode, PageCursor cursor )
		 {
			  cursor.Mark();
			  do
			  {
					PrepareForReading( cursor, record );
					RecordFormat.read( record, cursor, mode, RecordSizeConflict );
			  } while ( cursor.ShouldRetry() );
			  CheckForDecodingErrors( cursor, id, mode );
			  VerifyAfterReading( record, mode );
		 }

		 public override void UpdateRecord( RECORD record )
		 {
			  long id = record.Id;
			  IdValidator.assertValidId( IdType, id, RecordFormat.MaxId );

			  long pageId = PageIdForRecord( id );
			  int offset = OffsetForId( id );
			  try
			  {
					  using ( PageCursor cursor = PagedFile.io( pageId, PF_SHARED_WRITE_LOCK ) )
					  {
						if ( cursor.Next() )
						{
							 cursor.Offset = offset;
							 RecordFormat.write( record, cursor, RecordSizeConflict );
							 CheckForDecodingErrors( cursor, id, NORMAL ); // We don't free ids if something weird goes wrong
							 if ( !record.inUse() )
							 {
								  FreeId( id );
							 }
							 if ( ( !record.inUse() || !record.requiresSecondaryUnit() ) && record.hasSecondaryUnitId() )
							 {
								  // If record was just now deleted, or if the record used a secondary unit, but not anymore
								  // then free the id of that secondary unit.
								  FreeId( record.SecondaryUnitId );
							 }
						}
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 public override void PrepareForCommit( RECORD record )
		 {
			  PrepareForCommit( record, this );
		 }

		 public override void PrepareForCommit( RECORD record, IdSequence idSequence )
		 {
			  if ( record.inUse() )
			  {
					RecordFormat.prepare( record, RecordSizeConflict, idSequence );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <EXCEPTION extends Exception> void scanAllRecords(Neo4Net.helpers.collection.Visitor<RECORD,EXCEPTION> visitor) throws EXCEPTION
		 public override void ScanAllRecords<EXCEPTION>( Visitor<RECORD, EXCEPTION> visitor ) where EXCEPTION : Exception
		 {
			  using ( PageCursor cursor = OpenPageCursorForReading( 0 ) )
			  {
					RECORD record = NewRecord();
					long highId = HighId;
					for ( long id = NumberOfReservedLowIds; id < highId; id++ )
					{
						 GetRecordByCursor( id, record, CHECK, cursor );
						 if ( record.inUse() )
						 {
							  visitor.Visit( record );
						 }
					}
			  }
		 }

		 public override IList<RECORD> GetRecords( long firstId, RecordLoad mode )
		 {
			  if ( Record.NULL_REFERENCE.@is( firstId ) )
			  {
					return Collections.emptyList();
			  }

			  IList<RECORD> records = new List<RECORD>();
			  long id = firstId;
			  using ( PageCursor cursor = OpenPageCursorForReading( firstId ) )
			  {
					RECORD record;
					do
					{
						 record = NewRecord();
						 GetRecordByCursor( id, record, mode, cursor );
						 // Even unused records gets added and returned
						 records.Add( record );
						 id = GetNextRecordReference( record );
					} while ( !Record.NULL_REFERENCE.@is( id ) );
			  }
			  return records;
		 }

		 private void VerifyAfterNotRead( RECORD record, RecordLoad mode )
		 {
			  record.clear();
			  mode.verify( record );
		 }

		 internal void CheckForDecodingErrors( PageCursor cursor, long recordId, RecordLoad mode )
		 {
			  if ( mode.checkForOutOfBounds( cursor ) )
			  {
					ThrowOutOfBoundsException( recordId );
			  }
			  mode.clearOrThrowCursorError( cursor );
		 }

		 private void ThrowOutOfBoundsException( long recordId )
		 {
			  RECORD record = NewRecord();
			  record.Id = recordId;
			  long pageId = PageIdForRecord( recordId );
			  int offset = OffsetForId( recordId );
			  throw new UnderlyingStorageException( BuildOutOfBoundsExceptionMessage( record, pageId, offset, RecordSizeConflict, PagedFile.pageSize(), StorageFileConflict.AbsolutePath ) );
		 }

		 internal static string BuildOutOfBoundsExceptionMessage( AbstractBaseRecord record, long pageId, int offset, int recordSize, int pageSize, string filename )
		 {
			  return "Access to record " + record + " went out of bounds of the page. The record size is " + recordSize + " bytes, and the access was at offset " + offset + " bytes into page " + pageId + ", and the pages have a capacity of " + pageSize + " bytes. " +
						"The mapped store file in question is " + filename;
		 }

		 private void VerifyAfterReading( RECORD record, RecordLoad mode )
		 {
			  if ( !mode.verify( record ) )
			  {
					record.clear();
			  }
		 }

		 private void PrepareForReading( PageCursor cursor, RECORD record )
		 {
			  // Mark this record as unused. This to simplify implementations of readRecord.
			  // readRecord can behave differently depending on RecordLoad argument and so it may be that
			  // contents of a record may be loaded even if that record is unused, where the contents
			  // can still be initialized data. Know that for many record stores, deleting a record means
			  // just setting one byte or bit in that record.
			  record.InUse = false;
			  cursor.SetOffsetToMark();
		 }

		 public override void EnsureHeavy( RECORD record )
		 {
			  // Do nothing by default. Some record stores have this.
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }

		 public virtual int StoreHeaderInt
		 {
			 get
			 {
				  return ( ( IntStoreHeader ) _storeHeader ).value();
			 }
		 }
	}

}