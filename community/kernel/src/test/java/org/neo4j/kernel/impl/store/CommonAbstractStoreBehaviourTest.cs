using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.store
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using TestRule = org.junit.rules.TestRule;


	using Org.Neo4j.Function;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.impl.store.format;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using ConfigurablePageCacheRule = Org.Neo4j.Test.rule.ConfigurablePageCacheRule;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PageCache_Fields.PAGE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.CHECK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	/// <summary>
	/// Test for <seealso cref="CommonAbstractStore"/>, but without using mocks. </summary>
	/// <seealso cref= CommonAbstractStoreTest for testing with mocks. </seealso>
	public class CommonAbstractStoreBehaviourTest
	{
		private bool InstanceFieldsInitialized = false;

		public CommonAbstractStoreBehaviourTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _fs ).around( _pageCacheRule );
		}

		 /// <summary>
		 /// Note that tests MUST use the non-modifying methods, to make alternate copies
		 /// of this settings class.
		 /// </summary>
		 private static readonly Config _config = Config.defaults( GraphDatabaseSettings.pagecache_memory, "8M" );

		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private readonly ConfigurablePageCacheRule _pageCacheRule = new ConfigurablePageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestRule rules = org.junit.rules.RuleChain.outerRule(fs).around(pageCacheRule);
		 public TestRule Rules;

		 private readonly LinkedList<long> _nextPageId = new ConcurrentLinkedQueue<long>();
		 private readonly LinkedList<int> _nextPageOffset = new ConcurrentLinkedQueue<int>();
		 private int _cursorErrorOnRecord;
		 private int _intsPerRecord = 1;

		 private MyStore _store;
		 private Config _config = _config;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _store != null )
			  {
					_store.close();
					_store = null;
			  }
			  _nextPageOffset.Clear();
			  _nextPageId.Clear();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertThrowsUnderlyingStorageException(org.neo4j.function.ThrowingAction<Exception> action) throws Exception
		 private void AssertThrowsUnderlyingStorageException( ThrowingAction<Exception> action )
		 {
			  try
			  {
					action.Apply();
					fail( "expected an UnderlyingStorageException exception" );
			  }
			  catch ( UnderlyingStorageException )
			  {
					// Good!
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertThrowsInvalidRecordException(org.neo4j.function.ThrowingAction<Exception> action) throws Exception
		 private void AssertThrowsInvalidRecordException( ThrowingAction<Exception> action )
		 {
			  try
			  {
					action.Apply();
					fail( "expected an InvalidRecordException exception" );
			  }
			  catch ( InvalidRecordException )
			  {
					// Good!
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyExceptionOnOutOfBoundsAccess(org.neo4j.function.ThrowingAction<Exception> access) throws Exception
		 private void VerifyExceptionOnOutOfBoundsAccess( ThrowingAction<Exception> access )
		 {
			  PrepareStoreForOutOfBoundsAccess();
			  AssertThrowsUnderlyingStorageException( access );
		 }

		 private void PrepareStoreForOutOfBoundsAccess()
		 {
			  CreateStore();
			  _nextPageOffset.AddLast( PAGE_SIZE - 2 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyExceptionOnCursorError(org.neo4j.function.ThrowingAction<Exception> access) throws Exception
		 private void VerifyExceptionOnCursorError( ThrowingAction<Exception> access )
		 {
			  PrepareStoreForCursorError();
			  AssertThrowsInvalidRecordException( access );
		 }

		 private void PrepareStoreForCursorError()
		 {
			  CreateStore();
			  _cursorErrorOnRecord = 5;
		 }

		 private void CreateStore()
		 {
			  _store = new MyStore( this, _config, _pageCacheRule.getPageCache( _fs.get(), _config ), 8 );
			  _store.initialise( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writingOfHeaderRecordDuringInitialiseNewStoreFileMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WritingOfHeaderRecordDuringInitialiseNewStoreFileMustThrowOnPageOverflow()
		 {
			  // 16-byte header will overflow an 8-byte page size
			  PageCacheRule.PageCacheConfig pageCacheConfig = PageCacheRule.config();
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs.get(), pageCacheConfig, _config );
			  MyStore store = new MyStore( this, _config, pageCache, PAGE_SIZE + 1 );
			  AssertThrowsUnderlyingStorageException( () => store.initialise(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void extractHeaderRecordDuringLoadStorageMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExtractHeaderRecordDuringLoadStorageMustThrowOnPageOverflow()
		 {
			  MyStore first = new MyStore( this, _config, _pageCacheRule.getPageCache( _fs.get(), _config ), 8 );
			  first.Initialise( true );
			  first.Close();

			  PageCacheRule.PageCacheConfig pageCacheConfig = PageCacheRule.config();
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs.get(), pageCacheConfig, _config );
			  MyStore second = new MyStore( this, _config, pageCache, PAGE_SIZE + 1 );
			  AssertThrowsUnderlyingStorageException( () => second.initialise(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getRawRecordDataMustNotThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getRawRecordDataMustNotThrowOnPageOverflow()
		 {
			  PrepareStoreForOutOfBoundsAccess();
			  _store.getRawRecordData( 5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isInUseMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void isInUseMustThrowOnPageOverflow()
		 {
			  VerifyExceptionOnOutOfBoundsAccess( () => _store.isInUse(5) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isInUseMustThrowOnCursorError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void isInUseMustThrowOnCursorError()
		 {
			  VerifyExceptionOnCursorError( () => _store.isInUse(5) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getRecordMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getRecordMustThrowOnPageOverflow()
		 {
			  VerifyExceptionOnOutOfBoundsAccess( () => _store.getRecord(5, new IntRecord(5), NORMAL) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getRecordMustNotThrowOnPageOverflowWithCheckLoadMode()
		 public virtual void getRecordMustNotThrowOnPageOverflowWithCheckLoadMode()
		 {
			  PrepareStoreForOutOfBoundsAccess();
			  _store.getRecord( 5, new IntRecord( 5 ), CHECK );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getRecordMustNotThrowOnPageOverflowWithForceLoadMode()
		 public virtual void getRecordMustNotThrowOnPageOverflowWithForceLoadMode()
		 {
			  PrepareStoreForOutOfBoundsAccess();
			  _store.getRecord( 5, new IntRecord( 5 ), FORCE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateRecordMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpdateRecordMustThrowOnPageOverflow()
		 {
			  VerifyExceptionOnOutOfBoundsAccess( () => _store.updateRecord(new IntRecord(5)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getRecordMustThrowOnCursorError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getRecordMustThrowOnCursorError()
		 {
			  VerifyExceptionOnCursorError( () => _store.getRecord(5, new IntRecord(5), NORMAL) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getRecordMustNotThrowOnCursorErrorWithCheckLoadMode()
		 public virtual void getRecordMustNotThrowOnCursorErrorWithCheckLoadMode()
		 {
			  PrepareStoreForCursorError();
			  _store.getRecord( 5, new IntRecord( 5 ), CHECK );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getRecordMustNotThrowOnCursorErrorWithForceLoadMode()
		 public virtual void getRecordMustNotThrowOnCursorErrorWithForceLoadMode()
		 {
			  PrepareStoreForCursorError();
			  _store.getRecord( 5, new IntRecord( 5 ), FORCE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rebuildIdGeneratorSlowMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RebuildIdGeneratorSlowMustThrowOnPageOverflow()
		 {
			  _config.augment( GraphDatabaseSettings.rebuild_idgenerators_fast, "false" );
			  CreateStore();
			  _store.StoreNotOk = new Exception();
			  IntRecord record = new IntRecord( 200 );
			  record.Value = unchecked( ( int )0xCAFEBABE );
			  _store.updateRecord( record );
			  _intsPerRecord = 8192;
			  AssertThrowsUnderlyingStorageException( () => _store.makeStoreOk() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void scanForHighIdMustThrowOnPageOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScanForHighIdMustThrowOnPageOverflow()
		 {
			  CreateStore();
			  _store.StoreNotOk = new Exception();
			  IntRecord record = new IntRecord( 200 );
			  record.Value = unchecked( ( int )0xCAFEBABE );
			  _store.updateRecord( record );
			  _intsPerRecord = 8192;
			  AssertThrowsUnderlyingStorageException( () => _store.makeStoreOk() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFinishInitialisationOfIncompleteStoreHeader() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustFinishInitialisationOfIncompleteStoreHeader()
		 {
			  CreateStore();
			  int headerSizeInRecords = _store.NumberOfReservedLowIds;
			  int headerSizeInBytes = headerSizeInRecords * _store.RecordSize;
			  using ( PageCursor cursor = _store.pagedFile.io( 0, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					assertTrue( cursor.Next() );
					for ( int i = 0; i < headerSizeInBytes; i++ )
					{
						 cursor.PutByte( ( sbyte ) 0 );
					}
			  }
			  int pageSize = _store.pagedFile.pageSize();
			  _store.close();
			  _store.pageCache.map( _store.StorageFile, pageSize, StandardOpenOption.TRUNCATE_EXISTING ).close();
			  CreateStore();
		 }

		 private class IntRecord : AbstractBaseRecord
		 {
			  public int Value;

			  internal IntRecord( long id ) : base( id )
			  {
					InUse = true;
			  }

			  public override string ToString()
			  {
					return "IntRecord[" + Id + "](" + Value + ")";
			  }
		 }

		 private class LongLongHeader : StoreHeader
		 {
		 }

		 private class MyFormat : BaseRecordFormat<IntRecord>, StoreHeaderFormat<LongLongHeader>
		 {
			 private readonly CommonAbstractStoreBehaviourTest _outerInstance;

			  internal MyFormat( CommonAbstractStoreBehaviourTest outerInstance, int recordHeaderSize ) : base( x -> 4, recordHeaderSize, 32 )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override IntRecord NewRecord()
			  {
					return new IntRecord( 0 );
			  }

			  public override bool IsInUse( PageCursor cursor )
			  {
					int offset = cursor.Offset;
					long pageId = cursor.CurrentPageId;
					long recordId = ( offset + pageId * cursor.CurrentPageSize ) / 4;
					bool inUse = false;
					for ( int i = 0; i < outerInstance.intsPerRecord; i++ )
					{
						 inUse |= cursor.Int != 0;
					}
					MaybeSetCursorError( cursor, recordId );
					return inUse;
			  }

			  public override void Read( IntRecord record, PageCursor cursor, RecordLoad mode, int recordSize )
			  {
					for ( int i = 0; i < outerInstance.intsPerRecord; i++ )
					{
						 record.Value = cursor.Int;
					}
					record.InUse = true;
					MaybeSetCursorError( cursor, record.Id );
			  }

			  internal virtual void MaybeSetCursorError( PageCursor cursor, long id )
			  {
					if ( outerInstance.cursorErrorOnRecord == id )
					{
						 cursor.CursorException = "boom";
					}
			  }

			  public override void Write( IntRecord record, PageCursor cursor, int recordSize )
			  {
					for ( int i = 0; i < outerInstance.intsPerRecord; i++ )
					{
						 cursor.PutInt( record.Value );
					}
			  }

			  public override int NumberOfReservedRecords()
			  {
					return 4; // 2 longs occupy 4 int records
			  }

			  public override void WriteHeader( PageCursor cursor )
			  {
					for ( int i = 0; i < RecordHeaderSize; i++ )
					{
						 cursor.PutByte( ( sbyte ) ThreadLocalRandom.current().Next() );
					}
			  }

			  public override LongLongHeader ReadHeader( PageCursor cursor )
			  {
					LongLongHeader header = new LongLongHeader();
					for ( int i = 0; i < RecordHeaderSize; i++ )
					{
						 // pretend to read fields into the header
						 cursor.Byte;
					}
					return header;
			  }
		 }

		 private class MyStore : CommonAbstractStore<IntRecord, LongLongHeader>
		 {
			 private readonly CommonAbstractStoreBehaviourTest _outerInstance;

			  internal MyStore( CommonAbstractStoreBehaviourTest outerInstance, Config config, PageCache pageCache, int recordHeaderSize ) : this( outerInstance, config, pageCache, new MyFormat( outerInstance, recordHeaderSize ) )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal MyStore( CommonAbstractStoreBehaviourTest outerInstance, Config config, PageCache pageCache, MyFormat format ) : base( new File( "store" ), new File( "idFile" ), config, IdType.NODE, new DefaultIdGeneratorFactory( outerInstance.fs.Get() ), pageCache, NullLogProvider.Instance, "T", format, format, "XYZ" )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, IntRecord record ) where FAILURE : Exception
			  {
					throw new System.NotSupportedException();
			  }

			  protected internal override long PageIdForRecord( long id )
			  {
					long? @override = outerInstance.nextPageId.RemoveFirst();
					return @override != null ? @override.Value : base.PageIdForRecord( id );
			  }

			  protected internal override int OffsetForId( long id )
			  {
					int? @override = outerInstance.nextPageOffset.RemoveFirst();
					return @override != null ? @override.Value : base.OffsetForId( id );
			  }
		 }
	}

}