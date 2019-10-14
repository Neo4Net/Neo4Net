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
namespace Neo4Net.Kernel.impl.store
{
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;
	using InOrder = org.mockito.InOrder;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using Neo4Net.Kernel.impl.store.format;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using IdCapacityExceededException = Neo4Net.Kernel.impl.store.id.validation.IdCapacityExceededException;
	using NegativeIdException = Neo4Net.Kernel.impl.store.id.validation.NegativeIdException;
	using ReservedIdException = Neo4Net.Kernel.impl.store.id.validation.ReservedIdException;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using ConfigurablePageCacheRule = Neo4Net.Test.rule.ConfigurablePageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.TestDirectory.testDirectory;

	public class CommonAbstractStoreTest
	{
		 private const int PAGE_SIZE = 32;
		 private const int RECORD_SIZE = 10;
		 private const int HIGH_ID = 42;

		 private readonly IdGenerator _idGenerator = mock( typeof( IdGenerator ) );
		 private readonly IdGeneratorFactory _idGeneratorFactory = mock( typeof( IdGeneratorFactory ) );
		 private readonly PageCursor _pageCursor = mock( typeof( PageCursor ) );
		 private readonly PagedFile _pageFile = mock( typeof( PagedFile ) );
		 private readonly PageCache _pageCache = mock( typeof( PageCache ) );
		 private readonly Config _config = Config.defaults();
		 private readonly File _storeFile = new File( "store" );
		 private readonly File _idStoreFile = new File( "isStore" );
		 private readonly RecordFormat<TheRecord> _recordFormat = mock( typeof( RecordFormat ) );
		 private readonly IdType _idType = IdType.RELATIONSHIP; // whatever

		 private static readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private static readonly TestDirectory _dir = testDirectory( _fileSystemRule.get() );
		 private static readonly ConfigurablePageCacheRule _pageCacheRule = new ConfigurablePageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(dir).around(pageCacheRule);
		 public static readonly RuleChain RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _dir ).around( _pageCacheRule );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpMocks() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUpMocks()
		 {
			  when( _idGeneratorFactory.open( any( typeof( File ) ), eq( _idType ), any( typeof( System.Func<long> ) ), anyLong() ) ).thenReturn(_idGenerator);

			  when( _pageFile.pageSize() ).thenReturn(PAGE_SIZE);
			  when( _pageFile.io( anyLong(), anyInt() ) ).thenReturn(_pageCursor);
			  when( _pageCache.map( eq( _storeFile ), anyInt() ) ).thenReturn(_pageFile);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseStoreFileFirstAndIdGeneratorAfter() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseStoreFileFirstAndIdGeneratorAfter()
		 {
			  // given
			  TheStore store = NewStore();
			  InOrder inOrder = inOrder( _pageFile, _idGenerator );

			  // when
			  store.Close();

			  // then
			  inOrder.verify( _pageFile, times( 1 ) ).close();
			  inOrder.verify( _idGenerator, times( 1 ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failStoreInitializationWhenHeaderRecordCantBeRead() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailStoreInitializationWhenHeaderRecordCantBeRead()
		 {
			  File storeFile = _dir.file( "a" );
			  File idFile = _dir.file( "idFile" );
			  PageCache pageCache = mock( typeof( PageCache ) );
			  PagedFile pagedFile = mock( typeof( PagedFile ) );
			  PageCursor pageCursor = mock( typeof( PageCursor ) );

			  when( pageCache.Map( eq( storeFile ), anyInt(), any(typeof(OpenOption)) ) ).thenReturn(pagedFile);
			  when( pagedFile.Io( 0L, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) ).thenReturn( pageCursor );
			  when( pageCursor.Next() ).thenReturn(false);

			  RecordFormats recordFormats = Standard.LATEST_RECORD_FORMATS;

			  ExpectedException.expect( typeof( StoreNotFoundException ) );
			  ExpectedException.expectMessage( "Fail to read header record of store file: " + storeFile.AbsolutePath );

			  using ( DynamicArrayStore dynamicArrayStore = new DynamicArrayStore( storeFile, idFile, _config, IdType.NODE_LABELS, _idGeneratorFactory, pageCache, NullLogProvider.Instance, Settings.INTEGER.apply( GraphDatabaseSettings.label_block_size.DefaultValue ), recordFormats ) )
			  {
					dynamicArrayStore.Initialize( false );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenRecordWithNegativeIdIsUpdated()
		 public virtual void ThrowsWhenRecordWithNegativeIdIsUpdated()
		 {
			  TheStore store = NewStore();
			  TheRecord record = NewRecord( -1 );

			  try
			  {
					store.UpdateRecord( record );
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( NegativeIdException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenRecordWithTooHighIdIsUpdated()
		 public virtual void ThrowsWhenRecordWithTooHighIdIsUpdated()
		 {
			  long maxFormatId = 42;
			  when( _recordFormat.MaxId ).thenReturn( maxFormatId );

			  TheStore store = NewStore();
			  TheRecord record = NewRecord( maxFormatId + 1 );

			  try
			  {
					store.UpdateRecord( record );
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( IdCapacityExceededException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenRecordWithReservedIdIsUpdated()
		 public virtual void ThrowsWhenRecordWithReservedIdIsUpdated()
		 {
			  TheStore store = NewStore();
			  TheRecord record = NewRecord( IdGeneratorImpl.INTEGER_MINUS_ONE );

			  try
			  {
					store.UpdateRecord( record );
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( ReservedIdException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteOnCloseIfOpenOptionsSaysSo()
		 public virtual void ShouldDeleteOnCloseIfOpenOptionsSaysSo()
		 {
			  // GIVEN
			  DatabaseLayout databaseLayout = _dir.databaseLayout();
			  File nodeStore = databaseLayout.NodeStore();
			  File idFile = databaseLayout.IdFile( DatabaseFile.NODE_STORE ).orElseThrow( () => new System.InvalidOperationException("Node store id file not found.") );
			  FileSystemAbstraction fs = _fileSystemRule.get();
			  PageCache pageCache = _pageCacheRule.getPageCache( fs, Config.defaults() );
			  TheStore store = new TheStore( nodeStore, databaseLayout.IdNodeStore(), _config, _idType, new DefaultIdGeneratorFactory(fs), pageCache, NullLogProvider.Instance, _recordFormat, DELETE_ON_CLOSE );
			  store.Initialize( true );
			  store.MakeStoreOk();
			  assertTrue( fs.FileExists( nodeStore ) );
			  assertTrue( fs.FileExists( idFile ) );

			  // WHEN
			  store.Close();

			  // THEN
			  assertFalse( fs.FileExists( nodeStore ) );
			  assertFalse( fs.FileExists( idFile ) );
		 }

		 private TheStore NewStore()
		 {
			  LogProvider log = NullLogProvider.Instance;
			  TheStore store = new TheStore( _storeFile, _idStoreFile, _config, _idType, _idGeneratorFactory, _pageCache, log, _recordFormat );
			  store.Initialize( false );
			  return store;
		 }

		 private TheRecord NewRecord( long id )
		 {
			  return new TheRecord( id );
		 }

		 private class TheStore : CommonAbstractStore<TheRecord, NoStoreHeader>
		 {
			  internal TheStore( File file, File idFile, Config configuration, IdType idType, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, RecordFormat<TheRecord> recordFormat, params OpenOption[] openOptions ) : base( file, idFile, configuration, idType, idGeneratorFactory, pageCache, logProvider, "TheType", recordFormat, NoStoreHeaderFormat.NoStoreHeaderFormatConflict, "v1", openOptions )
			  {
			  }

			  protected internal override void InitializeNewStoreFile( PagedFile file )
			  {
			  }

			  protected internal override int DetermineRecordSize()
			  {
					return RECORD_SIZE;
			  }

			  public override long ScanForHighId()
			  {
					return HIGH_ID;
			  }

			  public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, TheRecord record ) where FAILURE : Exception
			  {
			  }
		 }

		 private class TheRecord : AbstractBaseRecord
		 {
			  internal TheRecord( long id ) : base( id )
			  {
			  }

			  public override TheRecord Clone()
			  {
					return new TheRecord( Id );
			  }
		 }
	}

}