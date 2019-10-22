using System;
using System.Collections.Generic;

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
namespace Neo4Net.com.storecopy
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using TestRule = org.junit.rules.TestRule;


	using Neo4Net.com;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using CancellationRequest = Neo4Net.Helpers.CancellationRequest;
	using Service = Neo4Net.Helpers.Service;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.extension;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.storecopy.StoreUtil.TEMP_COPY_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.logical_logs_location;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.record_format;

	public class StoreCopyClientTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreCopyClientTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _pageCacheRule ).around( _cleanup );
		}

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly CleanupRule _cleanup = new CleanupRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TestRule rules = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(pageCacheRule).around(cleanup);
		 public TestRule Rules;

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private readonly StoreCopyRequestFactory _requestFactory = LocalStoreCopyRequester::new;
		 private FileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _fileSystem = _fileSystemRule.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopyStoreFilesAcrossIfACancellationRequestHappensAfterTheTempStoreHasBeenRecovered() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCopyStoreFilesAcrossIfACancellationRequestHappensAfterTheTempStoreHasBeenRecovered()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.io.layout.DatabaseLayout copyLayout = directory.databaseLayout("copy");
			  DatabaseLayout copyLayout = _directory.databaseLayout( "copy" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File originalDir = directory.storeDir("original");
			  File originalDir = _directory.storeDir( "original" );
			  DatabaseLayout originalLayout = _directory.databaseLayout( originalDir );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean cancelStoreCopy = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean cancelStoreCopy = new AtomicBoolean( false );
			  StoreCopyClientMonitor storeCopyMonitor = new StoreCopyClientMonitor_AdapterAnonymousInnerClass( this, cancelStoreCopy );

			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  StoreCopyClient copier = new StoreCopyClient( copyLayout, Config.defaults(), LoadKernelExtensions(), NullLogProvider.Instance, _fileSystem, pageCache, storeCopyMonitor, false );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.internal.GraphDatabaseAPI original = (org.Neo4Net.kernel.internal.GraphDatabaseAPI) startDatabase(originalLayout.databaseDirectory());
			  GraphDatabaseAPI original = ( GraphDatabaseAPI ) StartDatabase( originalLayout.DatabaseDirectory() );

			  using ( Transaction tx = original.BeginTx() )
			  {
					original.CreateNode( label( "BeforeCopyBegins" ) );
					tx.Success();
			  }

			  StoreCopyClient.StoreCopyRequester storeCopyRequest = spy( _requestFactory.create( original, originalLayout.DatabaseDirectory(), _fileSystem, false ) );

			  // when
			  copier.CopyStore( storeCopyRequest, cancelStoreCopy.get, MoveAfterCopy.moveReplaceExisting() );

			  // Then
			  IGraphDatabaseService copy = StartDatabase( copyLayout.DatabaseDirectory() );

			  using ( Transaction tx = copy.BeginTx() )
			  {
					long nodesCount = Iterators.count( copy.FindNodes( label( "BeforeCopyBegins" ) ) );
					assertThat( nodesCount, equalTo( 1L ) );

					assertThat( Iterators.single( copy.FindNodes( label( "BeforeCopyBegins" ) ) ).Id, equalTo( 0L ) );

					tx.Success();
			  }

			  verify( storeCopyRequest, times( 1 ) ).done();
			  assertFalse( copyLayout.file( TEMP_COPY_DIRECTORY_NAME ).exists() );
		 }

		 private class StoreCopyClientMonitor_AdapterAnonymousInnerClass : StoreCopyClientMonitor_Adapter
		 {
			 private readonly StoreCopyClientTest _outerInstance;

			 private AtomicBoolean _cancelStoreCopy;

			 public StoreCopyClientMonitor_AdapterAnonymousInnerClass( StoreCopyClientTest outerInstance, AtomicBoolean cancelStoreCopy )
			 {
				 this.outerInstance = outerInstance;
				 this._cancelStoreCopy = cancelStoreCopy;
			 }

			 public override void finishRecoveringStore()
			 {
				  // simulate a cancellation request
				  _cancelStoreCopy.set( true );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeCopyClientUseCustomTransactionLogLocationWhenConfigured() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreCopyClientUseCustomTransactionLogLocationWhenConfigured()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.io.layout.DatabaseLayout copyLayout = directory.databaseLayout("copyCustomLocation");
			  DatabaseLayout copyLayout = _directory.databaseLayout( "copyCustomLocation" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File originalDir = new java.io.File(directory.directory(), "originalCustomLocation");
			  File originalDir = new File( _directory.directory(), "originalCustomLocation" );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  File copyCustomLogFilesLocation = copyLayout.File( "CopyCustomLogFilesLocation" );
			  File originalCustomLogFilesLocation = new File( originalDir, "originalCustomLogFilesLocation" );

			  Config config = Config.defaults( logical_logs_location, copyCustomLogFilesLocation.Name );
			  config.Augment( GraphDatabaseSettings.active_database, copyLayout.DatabaseName );
			  StoreCopyClient copier = new StoreCopyClient( copyLayout, config, LoadKernelExtensions(), NullLogProvider.Instance, _fileSystem, pageCache, new StoreCopyClientMonitor_Adapter(), false );

			  GraphDatabaseAPI original = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(originalDir).setConfig(logical_logs_location, originalCustomLogFilesLocation.Name).newGraphDatabase();
			  GenerateTransactions( original );
			  long logFileSize = original.DependencyResolver.resolveDependency( typeof( LogFiles ) ).getLogFileForVersion( 0 ).length();

			  StoreCopyClient.StoreCopyRequester storeCopyRequest = _requestFactory.create( original, originalDir, _fileSystem, true );

			  copier.CopyStore( storeCopyRequest, Neo4Net.Helpers.CancellationRequest_Fields.NeverCancelled, MoveAfterCopy.moveReplaceExisting() );
			  original.Shutdown();

			  assertFalse( copyLayout.file( TEMP_COPY_DIRECTORY_NAME ).exists() );

			  LogFiles customLogFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( copyCustomLogFilesLocation, _fileSystem ).build();
			  assertTrue( customLogFiles.VersionExists( 0 ) );
			  assertThat( customLogFiles.GetLogFileForVersion( 0 ).length(), greaterThanOrEqualTo(logFileSize) );

			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( copyLayout.DatabaseDirectory(), _fileSystem ).build();
			  assertFalse( logFiles.VersionExists( 0 ) );

			  ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(copyLayout.DatabaseDirectory()).setConfig(logical_logs_location, copyCustomLogFilesLocation.Name).newGraphDatabase().shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeCopyClientMustWorkWithStandardRecordFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreCopyClientMustWorkWithStandardRecordFormat()
		 {
			  CheckStoreCopyClientWithRecordFormats( Standard.LATEST_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeCopyClientMustWorkWithHighLimitRecordFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreCopyClientMustWorkWithHighLimitRecordFormat()
		 {
			  CheckStoreCopyClientWithRecordFormats( HighLimit.NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEndUpWithAnEmptyStoreIfCancellationRequestIssuedJustBeforeRecoveryTakesPlace() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEndUpWithAnEmptyStoreIfCancellationRequestIssuedJustBeforeRecoveryTakesPlace()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.io.layout.DatabaseLayout copyLayout = directory.databaseLayout("copy");
			  DatabaseLayout copyLayout = _directory.databaseLayout( "copy" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File originalDir = directory.storeDir("original");
			  File originalDir = _directory.storeDir( "original" );
			  DatabaseLayout originalLayout = _directory.databaseLayout( originalDir );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean cancelStoreCopy = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean cancelStoreCopy = new AtomicBoolean( false );
			  StoreCopyClientMonitor storeCopyMonitor = new StoreCopyClientMonitor_AdapterAnonymousInnerClass2( this, cancelStoreCopy );

			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  StoreCopyClient copier = new StoreCopyClient( copyLayout, Config.defaults(), LoadKernelExtensions(), NullLogProvider.Instance, _fileSystem, pageCache, storeCopyMonitor, false );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.internal.GraphDatabaseAPI original = (org.Neo4Net.kernel.internal.GraphDatabaseAPI) startDatabase(originalLayout.databaseDirectory());
			  GraphDatabaseAPI original = ( GraphDatabaseAPI ) StartDatabase( originalLayout.DatabaseDirectory() );

			  using ( Transaction tx = original.BeginTx() )
			  {
					original.CreateNode( label( "BeforeCopyBegins" ) );
					tx.Success();
			  }

			  StoreCopyClient.StoreCopyRequester storeCopyRequest = spy( _requestFactory.create( original, originalLayout.DatabaseDirectory(), _fileSystem, false ) );

			  // when
			  copier.CopyStore( storeCopyRequest, cancelStoreCopy.get, MoveAfterCopy.moveReplaceExisting() );

			  // Then
			  IGraphDatabaseService copy = StartDatabase( copyLayout.DatabaseDirectory() );

			  using ( Transaction tx = copy.BeginTx() )
			  {
					long nodesCount = Iterators.count( copy.FindNodes( label( "BeforeCopyBegins" ) ) );
					assertThat( nodesCount, equalTo( 0L ) );

					tx.Success();
			  }

			  verify( storeCopyRequest, times( 1 ) ).done();
			  assertFalse( copyLayout.file( TEMP_COPY_DIRECTORY_NAME ).exists() );
		 }

		 private class StoreCopyClientMonitor_AdapterAnonymousInnerClass2 : StoreCopyClientMonitor_Adapter
		 {
			 private readonly StoreCopyClientTest _outerInstance;

			 private AtomicBoolean _cancelStoreCopy;

			 public StoreCopyClientMonitor_AdapterAnonymousInnerClass2( StoreCopyClientTest outerInstance, AtomicBoolean cancelStoreCopy )
			 {
				 this.outerInstance = outerInstance;
				 this._cancelStoreCopy = cancelStoreCopy;
			 }

			 public override void finishReceivingStoreFiles()
			 {
				  // simulate a cancellation request
				  _cancelStoreCopy.set( true );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetNeoStoreLastTransactionOffsetForNonForensicCopy() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetNeoStoreLastTransactionOffsetForNonForensicCopy()
		 {
			  // GIVEN
			  DatabaseLayout initialLayout = _directory.databaseLayout( "initialStore" );
			  File backupStore = _directory.directory( "backupStore" );

			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  CreateInitialDatabase( initialLayout.DatabaseDirectory() );

			  long originalTransactionOffset = MetaDataStore.getRecord( pageCache, initialLayout.MetadataStore(), MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET );
			  IGraphDatabaseService initialDatabase = StartDatabase( initialLayout.DatabaseDirectory() );

			  DatabaseLayout backupLayout = _directory.databaseLayout( backupStore );
			  StoreCopyClient copier = new StoreCopyClient( backupLayout, Config.defaults(), LoadKernelExtensions(), NullLogProvider.Instance, _fileSystem, pageCache, new StoreCopyClientMonitor_Adapter(), false );
			  CancellationRequest falseCancellationRequest = () => false;
			  StoreCopyClient.StoreCopyRequester storeCopyRequest = _requestFactory.create( ( GraphDatabaseAPI ) initialDatabase, initialLayout.DatabaseDirectory(), _fileSystem, false );

			  // WHEN
			  copier.CopyStore( storeCopyRequest, falseCancellationRequest, MoveAfterCopy.moveReplaceExisting() );

			  // THEN
			  long updatedTransactionOffset = MetaDataStore.getRecord( pageCache, backupLayout.MetadataStore(), MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET );
			  assertNotEquals( originalTransactionOffset, updatedTransactionOffset );
			  assertEquals( LogHeader.LOG_HEADER_SIZE, updatedTransactionOffset );
			  assertFalse( ( new File( backupStore, TEMP_COPY_DIRECTORY_NAME ) ).exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteTempCopyFolderOnFailures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteTempCopyFolderOnFailures()
		 {
			  // GIVEN
			  File initialStore = _directory.directory( "initialStore" );
			  DatabaseLayout backupLayout = _directory.databaseLayout( "backupStore" );

			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  IGraphDatabaseService initialDatabase = CreateInitialDatabase( initialStore );
			  StoreCopyClient copier = new StoreCopyClient( backupLayout, Config.defaults(), LoadKernelExtensions(), NullLogProvider.Instance, _fileSystem, pageCache, new StoreCopyClientMonitor_Adapter(), false );
			  CancellationRequest falseCancellationRequest = () => false;

			  Exception exception = new Exception( "Boom!" );
			  StoreCopyClient.StoreCopyRequester storeCopyRequest = new LocalStoreCopyRequesterAnonymousInnerClass( this, initialStore, _fileSystem, exception );

			  // WHEN
			  try
			  {
					copier.CopyStore( storeCopyRequest, falseCancellationRequest, MoveAfterCopy.moveReplaceExisting() );
					fail( "should have thrown " );
			  }
			  catch ( Exception ex )
			  {
					assertEquals( exception, ex );
			  }

			  // THEN
			  assertFalse( backupLayout.file( TEMP_COPY_DIRECTORY_NAME ).exists() );
		 }

		 private class LocalStoreCopyRequesterAnonymousInnerClass : LocalStoreCopyRequester
		 {
			 private readonly StoreCopyClientTest _outerInstance;

			 private Exception _exception;

			 public LocalStoreCopyRequesterAnonymousInnerClass( StoreCopyClientTest outerInstance, File initialStore, FileSystemAbstraction fileSystem, Exception exception ) : base( ( GraphDatabaseAPI ) initialDatabase, initialStore, fileSystem, false )
			 {
				 this.outerInstance = outerInstance;
				 this._exception = exception;
			 }

			 public override void done()
			 {
				  throw _exception;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkStoreCopyClientWithRecordFormats(String recordFormatsName) throws Exception
		 private void CheckStoreCopyClientWithRecordFormats( string recordFormatsName )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.io.layout.DatabaseLayout copyLayout = directory.databaseLayout("copy");
			  DatabaseLayout copyLayout = _directory.databaseLayout( "copy" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File originalDir = directory.storeDir("original");
			  File originalDir = _directory.storeDir( "original" );
			  DatabaseLayout originalLayout = _directory.databaseLayout( originalDir );

			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  Config config = Config.defaults( record_format, recordFormatsName );
			  StoreCopyClient copier = new StoreCopyClient( copyLayout, config, LoadKernelExtensions(), NullLogProvider.Instance, _fileSystem, pageCache, new StoreCopyClientMonitor_Adapter(), false );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.internal.GraphDatabaseAPI original = (org.Neo4Net.kernel.internal.GraphDatabaseAPI) startDatabase(originalLayout.databaseDirectory(), recordFormatsName);
			  GraphDatabaseAPI original = ( GraphDatabaseAPI ) StartDatabase( originalLayout.DatabaseDirectory(), recordFormatsName );
			  StoreCopyClient.StoreCopyRequester storeCopyRequest = _requestFactory.create( original, originalLayout.DatabaseDirectory(), _fileSystem, false );

			  copier.CopyStore( storeCopyRequest, Neo4Net.Helpers.CancellationRequest_Fields.NeverCancelled, MoveAfterCopy.moveReplaceExisting() );

			  assertFalse( copyLayout.file( TEMP_COPY_DIRECTORY_NAME ).exists() );

			  // Must not throw
			  StartDatabase( copyLayout.DatabaseDirectory(), recordFormatsName ).shutdown();
		 }

		 private IGraphDatabaseService CreateInitialDatabase( File initialStore )
		 {
			  IGraphDatabaseService initialDatabase = StartDatabase( initialStore );
			  for ( int i = 0; i < 10; i++ )
			  {
					using ( Transaction tx = initialDatabase.BeginTx() )
					{
						 initialDatabase.CreateNode( label( "Neo" + i ) );
						 tx.Success();
					}
			  }
			  initialDatabase.Shutdown();
			  return initialDatabase;
		 }

		 private IGraphDatabaseService StartDatabase( File storeDir )
		 {
			  return StartDatabase( storeDir, Standard.LATEST_NAME );
		 }

		 private IGraphDatabaseService StartDatabase( File storeDir, string recordFormatName )
		 {
			  IGraphDatabaseService database = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(record_format, recordFormatName).newGraphDatabase();
			  return _cleanup.add( database );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.List<org.Neo4Net.kernel.extension.KernelExtensionFactory<?>> loadKernelExtensions()
		 private static IList<KernelExtensionFactory<object>> LoadKernelExtensions()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.Neo4Net.kernel.extension.KernelExtensionFactory<?>> kernelExtensions = new java.util.ArrayList<>();
			  IList<KernelExtensionFactory<object>> kernelExtensions = new List<KernelExtensionFactory<object>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.Neo4Net.kernel.extension.KernelExtensionFactory<?> factory : org.Neo4Net.helpers.Service.load(org.Neo4Net.kernel.extension.KernelExtensionFactory.class))
			  foreach ( KernelExtensionFactory<object> factory in Service.load( typeof( KernelExtensionFactory ) ) )
			  {
					kernelExtensions.Add( factory );
			  }
			  return kernelExtensions;
		 }

		 private static void GenerateTransactions( GraphDatabaseAPI original )
		 {
			  for ( int i = 0; i < 10; i++ )
			  {
					using ( Transaction transaction = original.BeginTx() )
					{
						 original.CreateNode();
						 transaction.Success();
					}
			  }
		 }

		 private interface StoreCopyRequestFactory
		 {
			  StoreCopyClient.StoreCopyRequester Create( GraphDatabaseAPI original, File originalDir, FileSystemAbstraction fs, bool includeLogs );
		 }

		 private class LocalStoreCopyRequester : StoreCopyClient.StoreCopyRequester
		 {
			  internal readonly GraphDatabaseAPI Original;
			  internal readonly File OriginalDir;
			  internal readonly FileSystemAbstraction Fs;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.com.Response<?> response;
			  internal Response<object> Response;
			  internal bool IncludeLogs;

			  internal LocalStoreCopyRequester( GraphDatabaseAPI original, File originalDir, FileSystemAbstraction fs, bool includeLogs )
			  {
					this.Original = original;
					this.OriginalDir = originalDir;
					this.Fs = fs;
					this.IncludeLogs = includeLogs;
			  }

			  protected internal virtual PageCache PageCache
			  {
				  get
				  {
						return Original.DependencyResolver.resolveDependency( typeof( PageCache ) );
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.Neo4Net.com.Response<?> copyStore(StoreWriter writer)
			  public override Response<object> CopyStore( StoreWriter writer )
			  {
					NeoStoreDataSource neoStoreDataSource = Original.DependencyResolver.resolveDependency( typeof( NeoStoreDataSource ) );

					TransactionIdStore transactionIdStore = Original.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );

					LogicalTransactionStore logicalTransactionStore = Original.DependencyResolver.resolveDependency( typeof( LogicalTransactionStore ) );

					CheckPointer checkPointer = Original.DependencyResolver.resolveDependency( typeof( CheckPointer ) );

					RequestContext requestContext = ( new StoreCopyServer( neoStoreDataSource, checkPointer, Fs, OriginalDir, ( new Monitors() ).newMonitor(typeof(StoreCopyServer.Monitor)) ) ).flushStoresAndStreamStoreFiles("test", writer, IncludeLogs);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.storageengine.api.StoreId storeId = original.getDependencyResolver().resolveDependency(org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine.class).getStoreId();
					StoreId storeId = Original.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).StoreId;

					ResponsePacker responsePacker = new ResponsePacker( logicalTransactionStore, transactionIdStore, () => storeId );

					Response = spy( responsePacker.PackTransactionStreamResponse( requestContext, null ) );
					return Response;
			  }

			  public override void Done()
			  {
					// Ensure response is closed before this method is called
					assertNotNull( Response );
					verify( Response, times( 1 ) ).close();
			  }
		 }
	}

}