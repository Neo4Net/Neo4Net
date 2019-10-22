using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.store.format
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using HighLimitV3_0_0 = Neo4Net.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using HighLimitV3_0_6 = Neo4Net.Kernel.impl.store.format.highlimit.v306.HighLimitV3_0_6;
	using HighLimitV3_1_0 = Neo4Net.Kernel.impl.store.format.highlimit.v310.HighLimitV3_1_0;
	using HighLimitV3_2_0 = Neo4Net.Kernel.impl.store.format.highlimit.v320.HighLimitV3_2_0;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_0 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_0;
	using StandardV3_2 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_4;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.defaultFormat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.findSuccessor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.selectForConfig;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.selectForStore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.selectForStoreOrConfig;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.selectForVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.selectNewestFormat;

	public class RecordFormatSelectorTest
	{
		private bool InstanceFieldsInitialized = false;

		public RecordFormatSelectorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fileSystemRule );
			RuleChain = RuleChain.outerRule( _pageCacheRule ).around( _fileSystemRule ).around( _testDirectory );
			_fs = _fileSystemRule.get();
		}

		 private static readonly LogProvider _log = NullLogProvider.Instance;

		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly EphemeralFileSystemRule _fileSystemRule = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(pageCacheRule).around(fileSystemRule).around(testDirectory);
		 public RuleChain RuleChain;

		 private FileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void defaultFormatTest()
		 public virtual void DefaultFormatTest()
		 {
			  assertSame( Standard.LATEST_RECORD_FORMATS, defaultFormat() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForVersionTest()
		 public virtual void SelectForVersionTest()
		 {
			  assertSame( StandardV2_3.RECORD_FORMATS, selectForVersion( StandardV2_3.STORE_VERSION ) );
			  assertSame( StandardV3_0.RECORD_FORMATS, selectForVersion( StandardV3_0.STORE_VERSION ) );
			  assertSame( StandardV3_2.RECORD_FORMATS, selectForVersion( StandardV3_2.STORE_VERSION ) );
			  assertSame( StandardV3_4.RECORD_FORMATS, selectForVersion( StandardV3_4.STORE_VERSION ) );
			  assertSame( HighLimitV3_0_0.RECORD_FORMATS, selectForVersion( HighLimitV3_0_0.STORE_VERSION ) );
			  assertSame( HighLimitV3_1_0.RECORD_FORMATS, selectForVersion( HighLimitV3_1_0.STORE_VERSION ) );
			  assertSame( HighLimit.RECORD_FORMATS, selectForVersion( HighLimit.STORE_VERSION ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForWrongVersionTest()
		 public virtual void SelectForWrongVersionTest()
		 {
			  try
			  {
					selectForVersion( "vA.B.9" );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.ArgumentException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForConfigWithRecordFormatParameter()
		 public virtual void SelectForConfigWithRecordFormatParameter()
		 {
			  assertSame( Standard.LATEST_RECORD_FORMATS, selectForConfig( Config( Standard.LATEST_NAME ), _log ) );
			  assertSame( HighLimit.RECORD_FORMATS, selectForConfig( Config( HighLimit.NAME ), _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForConfigWithoutRecordFormatParameter()
		 public virtual void SelectForConfigWithoutRecordFormatParameter()
		 {
			  assertSame( defaultFormat(), selectForConfig(Config.defaults(), _log) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForConfigWithWrongRecordFormatParameter()
		 public virtual void SelectForConfigWithWrongRecordFormatParameter()
		 {
			  try
			  {
					selectForConfig( Config( "unknown_format" ), _log );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.ArgumentException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreWithValidStore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectForStoreWithValidStore()
		 {
			  PageCache pageCache = PageCache;
			  VerifySelectForStore( pageCache, StandardV2_3.RECORD_FORMATS );
			  VerifySelectForStore( pageCache, StandardV3_0.RECORD_FORMATS );
			  VerifySelectForStore( pageCache, HighLimitV3_0_0.RECORD_FORMATS );
			  VerifySelectForStore( pageCache, HighLimit.RECORD_FORMATS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreWithNoStore()
		 public virtual void SelectForStoreWithNoStore()
		 {
			  assertNull( selectForStore( _testDirectory.databaseLayout(), _fs, PageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreWithThrowingPageCache() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectForStoreWithThrowingPageCache()
		 {
			  CreateNeoStoreFile();
			  PageCache pageCache = mock( typeof( PageCache ) );
			  when( pageCache.PageSize() ).thenReturn(Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE);
			  when( pageCache.Map( any(), anyInt(), any() ) ).thenThrow(new IOException("No reading..."));
			  assertNull( selectForStore( _testDirectory.databaseLayout(), _fs, pageCache, _log ) );
			  verify( pageCache ).map( any(), anyInt(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreWithInvalidStoreVersion() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectForStoreWithInvalidStoreVersion()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( "v9.Z.9", pageCache );
			  assertNull( selectForStore( _testDirectory.databaseLayout(), _fs, PageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithSameStandardConfiguredAndStoredFormat() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectForStoreOrConfigWithSameStandardConfiguredAndStoredFormat()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( Standard.LATEST_STORE_VERSION, pageCache );

			  Config config = config( Standard.LATEST_NAME );

			  assertSame( Standard.LATEST_RECORD_FORMATS, selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fs, pageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithSameHighLimitConfiguredAndStoredFormat() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectForStoreOrConfigWithSameHighLimitConfiguredAndStoredFormat()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( HighLimit.STORE_VERSION, pageCache );

			  Config config = config( HighLimit.NAME );

			  assertSame( HighLimit.RECORD_FORMATS, selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fs, pageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithDifferentlyConfiguredAndStoredFormat() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectForStoreOrConfigWithDifferentlyConfiguredAndStoredFormat()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( Standard.LATEST_STORE_VERSION, pageCache );

			  Config config = config( HighLimit.NAME );

			  try
			  {
					selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fs, pageCache, _log );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.ArgumentException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithOnlyStandardStoredFormat() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectForStoreOrConfigWithOnlyStandardStoredFormat()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( Standard.LATEST_STORE_VERSION, pageCache );

			  Config config = Config.defaults();

			  assertSame( Standard.LATEST_RECORD_FORMATS, selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fs, pageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithOnlyHighLimitStoredFormat() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectForStoreOrConfigWithOnlyHighLimitStoredFormat()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( HighLimit.STORE_VERSION, pageCache );

			  Config config = Config.defaults();

			  assertSame( HighLimit.RECORD_FORMATS, selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fs, pageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithOnlyStandardConfiguredFormat()
		 public virtual void SelectForStoreOrConfigWithOnlyStandardConfiguredFormat()
		 {
			  PageCache pageCache = PageCache;

			  Config config = config( Standard.LATEST_NAME );

			  assertSame( Standard.LATEST_RECORD_FORMATS, selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fs, pageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithOnlyHighLimitConfiguredFormat()
		 public virtual void SelectForStoreOrConfigWithOnlyHighLimitConfiguredFormat()
		 {
			  PageCache pageCache = PageCache;

			  Config config = config( HighLimit.NAME );

			  assertSame( HighLimit.RECORD_FORMATS, selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fs, pageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithWrongConfiguredFormat()
		 public virtual void SelectForStoreOrConfigWithWrongConfiguredFormat()
		 {
			  PageCache pageCache = PageCache;

			  Config config = config( "unknown_format" );

			  try
			  {
					selectForStoreOrConfig( config, _testDirectory.databaseLayout(), _fs, pageCache, _log );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.ArgumentException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectForStoreOrConfigWithoutConfiguredAndStoredFormats()
		 public virtual void SelectForStoreOrConfigWithoutConfiguredAndStoredFormats()
		 {
			  assertSame( defaultFormat(), selectForStoreOrConfig(Config.defaults(), _testDirectory.databaseLayout(), _fs, PageCache, _log) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectNewestFormatWithConfiguredStandardFormat()
		 public virtual void SelectNewestFormatWithConfiguredStandardFormat()
		 {
			  assertSame( Standard.LATEST_RECORD_FORMATS, selectNewestFormat( Config( Standard.LATEST_NAME ), _testDirectory.databaseLayout(), _fs, PageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectNewestFormatWithConfiguredHighLimitFormat()
		 public virtual void SelectNewestFormatWithConfiguredHighLimitFormat()
		 {
			  assertSame( HighLimit.RECORD_FORMATS, selectNewestFormat( Config( HighLimit.NAME ), _testDirectory.databaseLayout(), _fs, PageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectNewestFormatWithWrongConfiguredFormat()
		 public virtual void SelectNewestFormatWithWrongConfiguredFormat()
		 {
			  try
			  {
					selectNewestFormat( Config( "unknown_format" ), _testDirectory.databaseLayout(), _fs, PageCache, _log );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.ArgumentException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectNewestFormatWithoutConfigAndStore()
		 public virtual void SelectNewestFormatWithoutConfigAndStore()
		 {
			  assertSame( defaultFormat(), selectNewestFormat(Config.defaults(), _testDirectory.databaseLayout(), _fs, PageCache, _log) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectNewestFormatForExistingStandardStore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectNewestFormatForExistingStandardStore()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( Standard.LATEST_STORE_VERSION, pageCache );

			  Config config = Config.defaults();

			  assertSame( Standard.LATEST_RECORD_FORMATS, selectNewestFormat( config, _testDirectory.databaseLayout(), _fs, PageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectNewestFormatForExistingHighLimitStore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectNewestFormatForExistingHighLimitStore()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( HighLimit.STORE_VERSION, pageCache );

			  Config config = Config.defaults();

			  assertSame( HighLimit.RECORD_FORMATS, selectNewestFormat( config, _testDirectory.databaseLayout(), _fs, PageCache, _log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selectNewestFormatForExistingStoreWithLegacyFormat() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelectNewestFormatForExistingStoreWithLegacyFormat()
		 {
			  PageCache pageCache = PageCache;
			  PrepareNeoStoreFile( StandardV2_3.STORE_VERSION, pageCache );

			  Config config = Config.defaults();

			  assertSame( defaultFormat(), selectNewestFormat(config, _testDirectory.databaseLayout(), _fs, PageCache, _log) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void findSuccessorLatestVersion()
		 public virtual void FindSuccessorLatestVersion()
		 {
			  assertFalse( findSuccessor( defaultFormat() ).Present );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void findSuccessorToOlderVersion()
		 public virtual void FindSuccessorToOlderVersion()
		 {
			  assertEquals( StandardV3_0.RECORD_FORMATS, findSuccessor( StandardV2_3.RECORD_FORMATS ).get() );
			  assertEquals( StandardV3_2.RECORD_FORMATS, findSuccessor( StandardV3_0.RECORD_FORMATS ).get() );
			  assertEquals( StandardV3_4.RECORD_FORMATS, findSuccessor( StandardV3_2.RECORD_FORMATS ).get() );

			  assertEquals( HighLimitV3_0_6.RECORD_FORMATS, findSuccessor( HighLimitV3_0_0.RECORD_FORMATS ).get() );
			  assertEquals( HighLimitV3_1_0.RECORD_FORMATS, findSuccessor( HighLimitV3_0_6.RECORD_FORMATS ).get() );
			  assertEquals( HighLimitV3_2_0.RECORD_FORMATS, findSuccessor( HighLimitV3_1_0.RECORD_FORMATS ).get() );
			  assertEquals( HighLimit.RECORD_FORMATS, findSuccessor( HighLimitV3_2_0.RECORD_FORMATS ).get() );
		 }

		 private PageCache PageCache
		 {
			 get
			 {
				  return _pageCacheRule.getPageCache( _fs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifySelectForStore(org.Neo4Net.io.pagecache.PageCache pageCache, RecordFormats format) throws java.io.IOException
		 private void VerifySelectForStore( PageCache pageCache, RecordFormats format )
		 {
			  PrepareNeoStoreFile( format.StoreVersion(), pageCache );
			  assertSame( format, selectForStore( _testDirectory.databaseLayout(), _fs, pageCache, _log ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareNeoStoreFile(String storeVersion, org.Neo4Net.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private void PrepareNeoStoreFile( string storeVersion, PageCache pageCache )
		 {
			  File neoStoreFile = CreateNeoStoreFile();
			  long value = MetaDataStore.versionStringToLong( storeVersion );
			  MetaDataStore.setRecord( pageCache, neoStoreFile, STORE_VERSION, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createNeoStoreFile() throws java.io.IOException
		 private File CreateNeoStoreFile()
		 {
			  File neoStoreFile = _testDirectory.databaseLayout().metadataStore();
			  _fs.create( neoStoreFile ).close();
			  return neoStoreFile;
		 }

		 private static Config Config( string recordFormatName )
		 {
			  return Config.defaults( GraphDatabaseSettings.record_format, recordFormatName );
		 }
	}

}