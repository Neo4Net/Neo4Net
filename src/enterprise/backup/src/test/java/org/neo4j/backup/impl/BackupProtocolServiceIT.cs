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
namespace Neo4Net.backup.impl
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using Mockito = org.mockito.Mockito;


	using StoreCopyServer = Neo4Net.com.storecopy.StoreCopyServer;
	using StoreUtil = Neo4Net.com.storecopy.StoreUtil;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using Neo4Net.Cursors;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4Net.GraphDb.index;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using SimpleKernelContext = Neo4Net.Kernel.impl.spi.SimpleKernelContext;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using ReadOnlyTransactionStore = Neo4Net.Kernel.impl.transaction.log.ReadOnlyTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using LogHeaderReader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeaderReader;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using DependenciesProxy = Neo4Net.Kernel.impl.util.DependenciesProxy;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Logger = Neo4Net.Logging.Logger;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;
	using Barrier = Neo4Net.Test.Barrier;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static true;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.backup.impl.BackupProtocolServiceFactory.backupProtocolService;

	public class BackupProtocolServiceIT
	{
		private bool InstanceFieldsInitialized = false;

		public BackupProtocolServiceIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _target ).around( _dbRule ).around( _pageCacheRule ).around( _suppressOutput );
		}

		 private const string BACKUP_HOST = "localhost";
		 private static readonly Stream NULL_OUTPUT = new OutputStreamAnonymousInnerClass();

		 private class OutputStreamAnonymousInnerClass : Stream
		 {
			 public override void write( int b )
			 {
			 }
		 }
		 private const string PROP = "id";
		 private static readonly Label _label = Label.label( "LABEL" );

		 private readonly Monitors _monitors = new Monitors();
		 private readonly IOLimiter _limiter = Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited;
		 private FileSystemAbstraction _fileSystem;
		 private DatabaseLayout _databaseLayout;
		 private DatabaseLayout _backupDatabaseLayout;
		 private File _backupStoreDir;
		 private int _backupPort = -1;

		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly TestDirectory _target = TestDirectory.testDirectory();
		 private readonly EmbeddedDatabaseRule _dbRule = new EmbeddedDatabaseRule().startLazily();
		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(target).around(dbRule).around(pageCacheRule).around(suppressOutput);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fileSystem = _fileSystemRule.get();
			  _backupPort = PortAuthority.allocatePort();
			  _databaseLayout = _dbRule.databaseLayout();
			  _backupStoreDir = _target.storeDir( "backupStore" );
			  _backupDatabaseLayout = _target.databaseLayout( _backupStoreDir );
		 }

		 private BackupProtocolService BackupService()
		 {
			  return backupProtocolService( () => new UncloseableDelegatingFileSystemAbstraction(_fileSystemRule.get()), FormattedLogProvider.toOutputStream(NULL_OUTPUT), NULL_OUTPUT, new Monitors(), _pageCacheRule.getPageCache(_fileSystemRule.get()) );
		 }

		 private BackupProtocolService BackupService( LogProvider logProvider )
		 {
			  return backupProtocolService( () => new UncloseableDelegatingFileSystemAbstraction(_fileSystemRule.get()), logProvider, NULL_OUTPUT, new Monitors(), _pageCacheRule.getPageCache(_fileSystemRule.get()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void performConsistencyCheckAfterIncrementalBackup()
		 public virtual void PerformConsistencyCheckAfterIncrementalBackup()
		 {
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults( OnlineBackupSettings.online_backup_server, BACKUP_HOST + ":" + _backupPort );

			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );

			  BackupService().doFullBackup(BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false);

			  CreateAndIndexNode( db, 1 );
			  TestFullConsistencyCheck consistencyCheck = new TestFullConsistencyCheck();
			  BackupOutcome backupOutcome = BackupService().doIncrementalBackupOrFallbackToFull(BACKUP_HOST, _backupPort, _backupDatabaseLayout, consistencyCheck, defaultConfig, BackupClient.BigReadTimeout, false);
			  assertTrue( "Consistency check invoked for incremental backup, ", consistencyCheck.Checked );
			  assertTrue( backupOutcome.Consistent );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintThatFullBackupIsPerformed()
		 public virtual void ShouldPrintThatFullBackupIsPerformed()
		 {
			  DefaultBackupPortHostParams();
			  IGraphDatabaseService db = _dbRule.GraphDatabaseAPI;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.logging.Log log = mock(org.Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  LogProvider logProvider = new LogProviderAnonymousInnerClass( this, log );

			  BackupService( logProvider ).doIncrementalBackupOrFallbackToFull( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );

			  verify( log ).info( "Previous backup not found, a new full backup will be performed." );
		 }

		 private class LogProviderAnonymousInnerClass : LogProvider
		 {
			 private readonly BackupProtocolServiceIT _outerInstance;

			 private Log _log;

			 public LogProviderAnonymousInnerClass( BackupProtocolServiceIT outerInstance, Log log )
			 {
				 this.outerInstance = outerInstance;
				 this._log = log;
			 }

			 public Log getLog( Type loggingClass )
			 {
				  return _log;
			 }

			 public Log getLog( string name )
			 {
				  return _log;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintThatIncrementalBackupIsPerformedAndFallingBackToFull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintThatIncrementalBackupIsPerformedAndFallingBackToFull()
		 {
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults();
			  _dbRule.withSetting( GraphDatabaseSettings.keep_logical_logs, "false" );
			  // have logs rotated on every transaction
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );

			  CreateAndIndexNode( db, 1 );

			  // A full backup
			  BackupService().doFullBackup(BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false);

			  // And the log the backup uses is rotated out
			  CreateAndIndexNode( db, 2 );
			  RotateAndCheckPoint( db );
			  CreateAndIndexNode( db, 3 );
			  RotateAndCheckPoint( db );
			  CreateAndIndexNode( db, 4 );
			  RotateAndCheckPoint( db );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.logging.Log log = mock(org.Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  LogProvider logProvider = new LogProviderAnonymousInnerClass2( this, log );

			  BackupService( logProvider ).doIncrementalBackupOrFallbackToFull( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );

			  verify( log ).info( "Previous backup found, trying incremental backup." );
			  verify( log ).info( "Existing backup is too far out of date, a new full backup will be performed." );
		 }

		 private class LogProviderAnonymousInnerClass2 : LogProvider
		 {
			 private readonly BackupProtocolServiceIT _outerInstance;

			 private Log _log;

			 public LogProviderAnonymousInnerClass2( BackupProtocolServiceIT outerInstance, Log log )
			 {
				 this.outerInstance = outerInstance;
				 this._log = log;
			 }

			 public Log getLog( Type loggingClass )
			 {
				  return _log;
			 }

			 public Log getLog( string name )
			 {
				  return _log;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowUsefulMessageWhenCannotConnectDuringFullBackup()
		 public virtual void ShouldThrowUsefulMessageWhenCannotConnectDuringFullBackup()
		 {
			  try
			  {
					BackupService().doIncrementalBackupOrFallbackToFull(BACKUP_HOST, 56789, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false);
					fail( "No exception thrown" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.Message, containsString( "BackupClient could not connect" ) );
					assertThat( e.InnerException, instanceOf( typeof( ConnectException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowUsefulMessageWhenCannotConnectDuringIncrementalBackup()
		 public virtual void ShouldThrowUsefulMessageWhenCannotConnectDuringIncrementalBackup()
		 {
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  BackupProtocolService backupProtocolService = BackupService();

			  CreateAndIndexNode( db, 1 );

			  // A full backup
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );
			  try
			  {
					BackupService().doIncrementalBackupOrFallbackToFull(BACKUP_HOST, 56789, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false);
					fail( "No exception thrown" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.Message, containsString( "BackupClient could not connect" ) );
					assertThat( e.InnerException, instanceOf( typeof( ConnectException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionWhenDoingFullBackupWhenDirectoryHasSomeFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionWhenDoingFullBackupWhenDirectoryHasSomeFiles()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );

			  // Touch a random file
			  assertTrue( _backupDatabaseLayout.file( ".jibberishfile" ).createNewFile() );

			  try
			  {
					// when
					BackupService().doFullBackup(BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.FULL, Config.defaults(), BackupClient.BigReadTimeout, false);
					fail( "Should have thrown an exception" );
			  }
			  catch ( Exception ex )
			  {
					// then
					assertThat( ex.Message, containsString( "is not empty" ) );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionWhenDoingFullBackupWhenDirectoryHasSomeDirs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionWhenDoingFullBackupWhenDirectoryHasSomeDirs()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );

			  // Touch a random directory
			  Files.createDirectory( _backupDatabaseLayout.file( "jibberishfolder" ).toPath() );

			  try
			  {
					// when
					BackupService().doFullBackup(BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.FULL, Config.defaults(), BackupClient.BigReadTimeout, false);
					fail( "Should have thrown an exception" );
			  }
			  catch ( Exception ex )
			  {
					// then
					assertThat( ex.Message, containsString( "is not empty" ) );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveTempDirectory()
		 public virtual void ShouldRemoveTempDirectory()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );

			  // when
			  BackupProtocolService backupProtocolService = BackupService();
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );
			  Db.shutdown();

			  // then
			  assertFalse( "Temp directory was not removed as expected", Files.exists( _backupDatabaseLayout.file( StoreUtil.TEMP_COPY_DIRECTORY_NAME ).toPath() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopyStoreFiles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCopyStoreFiles()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );

			  // when
			  BackupProtocolService backupProtocolService = BackupService();
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );
			  Db.shutdown();

			  // then
			  File[] files = _backupDatabaseLayout.databaseDirectory().listFiles();
			  assertTrue( Files.Length > 0 );

			  foreach ( File storeFile in _backupDatabaseLayout.storeFiles() )
			  {
					if ( _backupDatabaseLayout.countStoreA().Equals(storeFile) || _backupDatabaseLayout.countStoreB().Equals(storeFile) )
					{
						 assertThat( files, anyOf( HasFile( _backupDatabaseLayout.countStoreA() ), HasFile(_backupDatabaseLayout.countStoreB()) ) );
					}
					else
					{
						 assertThat( files, HasFile( storeFile ) );
					}
			  }

			  assertEquals( DbRepresentation, BackupDbRepresentation );
		 }

		 /*
		  * During incremental backup destination db should not track free ids independently from source db
		  * for now we will always cleanup id files generated after incremental backup and will regenerate them afterwards
		  * This should prevent situation when destination db free id following master, but never allocates it from
		  * generator till some db will be started on top of it.
		  * That will cause all sorts of problems with several entities in a store with same id.
		  *
		  * As soon as backup will be able to align ids between participants please remove description and adapt test.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementallyBackupDatabaseShouldNotKeepGeneratedIdFiles()
		 public virtual void IncrementallyBackupDatabaseShouldNotKeepGeneratedIdFiles()
		 {
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI graphDatabase = _dbRule.GraphDatabaseAPI;
			  Label markerLabel = Label.label( "marker" );

			  using ( Transaction transaction = graphDatabase.BeginTx() )
			  {
					Node node = graphDatabase.CreateNode();
					node.AddLabel( markerLabel );
					transaction.Success();
			  }

			  using ( Transaction transaction = graphDatabase.BeginTx() )
			  {
					Node node = FindNodeByLabel( graphDatabase, markerLabel );
					for ( int i = 0; i < 10; i++ )
					{
						 node.SetProperty( "property" + i, "testValue" + i );
					}
					transaction.Success();
			  }
			  // propagate to backup node and properties
			  DoIncrementalBackupOrFallbackToFull();

			  // removing properties will free couple of ids that will be reused during next properties creation
			  using ( Transaction transaction = graphDatabase.BeginTx() )
			  {
					Node node = FindNodeByLabel( graphDatabase, markerLabel );
					for ( int i = 0; i < 6; i++ )
					{
						 node.RemoveProperty( "property" + i );
					}

					transaction.Success();
			  }

			  // propagate removed properties
			  DoIncrementalBackupOrFallbackToFull();

			  using ( Transaction transaction = graphDatabase.BeginTx() )
			  {
					Node node = FindNodeByLabel( graphDatabase, markerLabel );
					for ( int i = 10; i < 16; i++ )
					{
						 node.SetProperty( "property" + i, "updatedValue" + i );
					}

					transaction.Success();
			  }

			  // propagate to backup new properties with reclaimed ids
			  DoIncrementalBackupOrFallbackToFull();

			  // it should be possible to at this point to start db based on our backup and create couple of properties
			  // their ids should not clash with already existing
			  IGraphDatabaseService backupBasedDatabase = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_backupDatabaseLayout.databaseDirectory()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
			  try
			  {
					using ( Transaction transaction = backupBasedDatabase.BeginTx() )
					{
						 Node node = FindNodeByLabel( ( GraphDatabaseAPI ) backupBasedDatabase, markerLabel );
						 IEnumerable<string> propertyKeys = node.PropertyKeys;
						 foreach ( string propertyKey in propertyKeys )
						 {
							  node.SetProperty( propertyKey, "updatedClientValue" + propertyKey );
						 }
						 node.SetProperty( "newProperty", "updatedClientValue" );
						 transaction.Success();
					}

					using ( Transaction ignored = backupBasedDatabase.BeginTx() )
					{
						 Node node = FindNodeByLabel( ( GraphDatabaseAPI ) backupBasedDatabase, markerLabel );
						 // newProperty + 10 defined properties.
						 assertEquals( "We should be able to see all previously defined properties.", 11, Iterables.asList( node.PropertyKeys ).Count );
					}
			  }
			  finally
			  {
					backupBasedDatabase.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToBackupEvenIfTransactionLogsAreIncomplete() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToBackupEvenIfTransactionLogsAreIncomplete()
		 {
			  /*
			  * This test deletes the old persisted log file and expects backup to still be functional. It
			  * should not be assumed that the log files have any particular length of history. They could
			  * for example have been mangled during backups or removed during pruning.
			  */

			  // given
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );

			  for ( int i = 0; i < 100; i++ )
			  {
					CreateAndIndexNode( db, i );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File oldLog = db.getDependencyResolver().resolveDependency(org.Neo4Net.kernel.impl.transaction.log.files.LogFiles.class).getHighestLogFile();
			  File oldLog = Db.DependencyResolver.resolveDependency( typeof( LogFiles ) ).HighestLogFile;
			  RotateAndCheckPoint( db );

			  for ( int i = 0; i < 1; i++ )
			  {
					CreateAndIndexNode( db, i );
			  }
			  RotateAndCheckPoint( db );

			  long lastCommittedTxBefore = Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastCommittedTransactionId;

			  db = _dbRule.restartDatabase( ( fs, storeDirectory ) => FileUtils.deleteFile( oldLog ) );

			  long lastCommittedTxAfter = Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastCommittedTransactionId;

			  // when
			  BackupProtocolService backupProtocolService = BackupService();
			  BackupOutcome outcome = backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.FULL, Config.defaults(), BackupClient.BigReadTimeout, false );

			  Db.shutdown();

			  // then
			  assertEquals( lastCommittedTxBefore, lastCommittedTxAfter );
			  assertTrue( outcome.Consistent );
			  assertEquals( DbRepresentation, BackupDbRepresentation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindTransactionLogContainingLastNeoStoreTransactionInAnEmptyStore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindTransactionLogContainingLastNeoStoreTransactionInAnEmptyStore()
		 {
			  // This test highlights a special case where an empty store can return transaction metadata for transaction 0.

			  // given
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;

			  // when
			  BackupProtocolService backupProtocolService = BackupService();
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );
			  Db.shutdown();

			  // then
			  assertEquals( DbRepresentation, BackupDbRepresentation );

			  assertEquals( 0, GetLastTxChecksum( _pageCacheRule.getPageCache( _fileSystem ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindTransactionLogContainingLastNeoStoreTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindTransactionLogContainingLastNeoStoreTransaction()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );

			  // when
			  BackupProtocolService backupProtocolService = BackupService();
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );
			  Db.shutdown();

			  // then
			  assertEquals( DbRepresentation, BackupDbRepresentation );
			  assertNotEquals( 0, GetLastTxChecksum( _pageCacheRule.getPageCache( _fileSystem ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindValidPreviousCommittedTxIdInFirstNeoStoreLog() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindValidPreviousCommittedTxIdInFirstNeoStoreLog()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );
			  CreateAndIndexNode( db, 2 );
			  CreateAndIndexNode( db, 3 );
			  CreateAndIndexNode( db, 4 );

			  Db.DependencyResolver.resolveDependency( typeof( StorageEngine ) ).flushAndForce( _limiter );
			  long txId = Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastCommittedTransactionId;

			  // when
			  BackupProtocolService backupProtocolService = BackupService();
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );
			  Db.shutdown();

			  // then
			  CheckPreviousCommittedTxIdFromLog( 0, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindTransactionLogContainingLastLuceneTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindTransactionLogContainingLastLuceneTransaction()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );

			  // when
			  BackupProtocolService backupProtocolService = BackupService();
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false );
			  Db.shutdown();

			  // then
			  assertEquals( DbRepresentation, BackupDbRepresentation );
			  assertNotEquals( 0, GetLastTxChecksum( _pageCacheRule.getPageCache( _fileSystem ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorMessageIfLogsPrunedPastThePointOfNoReturn() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorMessageIfLogsPrunedPastThePointOfNoReturn()
		 {
			  // Given
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults();
			  _dbRule.withSetting( GraphDatabaseSettings.keep_logical_logs, "false" );
			  // have logs rotated on every transaction
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  BackupProtocolService backupProtocolService = BackupService();

			  CreateAndIndexNode( db, 1 );
			  RotateAndCheckPoint( db );

			  // A full backup
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false );

			  // And the log the backup uses is rotated out
			  CreateAndIndexNode( db, 2 );
			  RotateAndCheckPoint( db );
			  CreateAndIndexNode( db, 3 );
			  RotateAndCheckPoint( db );
			  CreateAndIndexNode( db, 4 );
			  RotateAndCheckPoint( db );
			  CreateAndIndexNode( db, 5 );
			  RotateAndCheckPoint( db );

			  // when
			  try
			  {
					backupProtocolService.DoIncrementalBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, BackupClient.BigReadTimeout, defaultConfig );
					fail( "Should have thrown exception." );
			  }
			  // Then
			  catch ( IncrementalBackupNotPossibleException e )
			  {
					assertThat( e.Message, equalTo( BackupProtocolService.TooOldBackup ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFallbackToFullBackupIfIncrementalFailsAndExplicitlyAskedToDoThis() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFallbackToFullBackupIfIncrementalFailsAndExplicitlyAskedToDoThis()
		 {
			  // Given
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults();
			  _dbRule.withSetting( GraphDatabaseSettings.keep_logical_logs, "false" );
			  // have logs rotated on every transaction
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  BackupProtocolService backupProtocolService = BackupService();

			  CreateAndIndexNode( db, 1 );

			  // A full backup
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false );

			  // And the log the backup uses is rotated out
			  CreateAndIndexNode( db, 2 );
			  RotateAndCheckPoint( db );
			  CreateAndIndexNode( db, 3 );
			  RotateAndCheckPoint( db );
			  CreateAndIndexNode( db, 4 );
			  RotateAndCheckPoint( db );

			  // when
			  backupProtocolService.DoIncrementalBackupOrFallbackToFull( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false );

			  // Then
			  Db.shutdown();
			  assertEquals( DbRepresentation, BackupDbRepresentation );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void rotateAndCheckPoint(org.Neo4Net.kernel.internal.GraphDatabaseAPI db) throws java.io.IOException
		 private static void RotateAndCheckPoint( GraphDatabaseAPI db )
		 {
			  Db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
			  Db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint(new SimpleTriggerInfo("test")
			 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBackupWhenLogFilesHaveBeenDeleted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleBackupWhenLogFilesHaveBeenDeleted()
		 {
			  // Given
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults();
			  _dbRule.withSetting( GraphDatabaseSettings.keep_logical_logs, "false" );
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  BackupProtocolService backupProtocolService = BackupService();

			  CreateAndIndexNode( db, 1 );

			  // A full backup
			  backupProtocolService.DoIncrementalBackupOrFallbackToFull( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false );

			  // And the log the backup uses is rotated out
			  CreateAndIndexNode( db, 2 );
			  db = DeleteLogFilesAndRestart();

			  CreateAndIndexNode( db, 3 );
			  db = DeleteLogFilesAndRestart();

			  // when
			  backupProtocolService.DoIncrementalBackupOrFallbackToFull( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false );

			  // Then
			  Db.shutdown();
			  assertEquals( DbRepresentation, BackupDbRepresentation );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.internal.GraphDatabaseAPI deleteLogFilesAndRestart() throws java.io.IOException
		 private GraphDatabaseAPI DeleteLogFilesAndRestart()
		 {
			  IList<File> logFiles = new List<File>();
			  NeoStoreDataSource dataSource = _dbRule.resolveDependency( typeof( NeoStoreDataSource ) );
			  using ( ResourceIterator<StoreFileMetadata> files = dataSource.ListStoreFiles( true ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					Files.Where( StoreFileMetadata::isLogFile ).Select( StoreFileMetadata::file ).ForEach( logFiles.add );
			  }
			  return _dbRule.restartDatabase((fs, storeDirectory) =>
			  {
				foreach ( File logFile in logFiles )
				{
					 fs.deleteFile( logFile );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupShouldWorkWithReadOnlySourceDatabases() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupShouldWorkWithReadOnlySourceDatabases()
		 {
			  // Create some data
			  DefaultBackupPortHostParams();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  CreateAndIndexNode( db, 1 );

			  // Make it read-only
			  db = _dbRule.restartDatabase( GraphDatabaseSettings.read_only.name(), TRUE.ToString() );

			  // Take a backup
			  Config defaultConfig = Config.defaults();
			  BackupProtocolService backupProtocolService = BackupService();
			  backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.FULL, defaultConfig, BackupClient.BigReadTimeout, false );

			  // Then
			  Db.shutdown();
			  assertEquals( DbRepresentation, BackupDbRepresentation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoFullBackupOnIncrementalFallbackToFullIfNoBackupFolderExists()
		 public virtual void ShouldDoFullBackupOnIncrementalFallbackToFullIfNoBackupFolderExists()
		 {
			  // Given
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults();
			  _dbRule.withSetting( GraphDatabaseSettings.keep_logical_logs, "false" );
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );
			  BackupProtocolService backupProtocolService = BackupService();

			  CreateAndIndexNode( db, 1 );

			  // when
			  backupProtocolService.DoIncrementalBackupOrFallbackToFull( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false );

			  // then
			  Db.shutdown();
			  assertEquals( DbRepresentation, BackupDbRepresentation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainTransactionsThatHappenDuringBackupProcess() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContainTransactionsThatHappenDuringBackupProcess()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults();
			  defaultConfig.Augment( OnlineBackupSettings.online_backup_server, BACKUP_HOST + ":" + _backupPort );
			  _dbRule.withSetting( OnlineBackupSettings.online_backup_enabled, "false" );
			  Config withOnlineBackupDisabled = Config.defaults();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.test.Barrier_Control barrier = new org.Neo4Net.test.Barrier_Control();
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.internal.GraphDatabaseAPI db = dbRule.getGraphDatabaseAPI();
			  GraphDatabaseAPI db = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db );

			  CreateAndIndexNode( db, 1 ); // create some data

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.DependencyResolver resolver = db.getDependencyResolver();
			  DependencyResolver resolver = Db.DependencyResolver;
			  long expectedLastTxId = resolver.ResolveDependency( typeof( TransactionIdStore ) ).LastClosedTransactionId;

			  // This monitor is added server-side...
			  _monitors.addMonitorListener( new StoreSnoopingMonitor( barrier, Db.databaseLayout() ) );

			  Dependencies dependencies = new Dependencies( resolver );
			  dependencies.SatisfyDependencies( defaultConfig, _monitors, NullLogProvider.Instance );

			  OnlineBackupKernelExtension backup = ( OnlineBackupKernelExtension ) ( new OnlineBackupExtensionFactory() ).newInstance(new SimpleKernelContext(_databaseLayout.databaseDirectory(), DatabaseInfo.UNKNOWN, dependencies), DependenciesProxy.dependencies(dependencies, typeof(OnlineBackupExtensionFactory.Dependencies)));
			  backup.Start();

			  // when
			  BackupProtocolService backupProtocolService = BackupService();
			  ExecutorService executor = Executors.newSingleThreadExecutor();
			  executor.execute(() =>
			  {
				barrier.AwaitUninterruptibly();

				CreateAndIndexNode( db, 1 );
				resolver.ResolveDependency( typeof( StorageEngine ) ).flushAndForce( _limiter );

				barrier.Release();
			  });

			  BackupOutcome backupOutcome = backupProtocolService.DoFullBackup( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.FULL, withOnlineBackupDisabled, BackupClient.BigReadTimeout, false );

			  backup.Stop();
			  executor.shutdown();
			  executor.awaitTermination( 30, TimeUnit.SECONDS );

			  // then
			  CheckPreviousCommittedTxIdFromLog( 0, expectedLastTxId );
			  Path neoStore = Db.databaseLayout().metadataStore().toPath();
			  PageCache pageCache = resolver.ResolveDependency( typeof( PageCache ) );
			  long txIdFromOrigin = MetaDataStore.getRecord( pageCache, neoStore.toFile(), MetaDataStore.Position.LAST_TRANSACTION_ID );
			  CheckLastCommittedTxIdInLogAndNeoStore( expectedLastTxId + 1, txIdFromOrigin );
			  assertEquals( DbRepresentation.of( db ), BackupDbRepresentation );
			  assertTrue( backupOutcome.Consistent );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupsShouldBeMentionedInServerConsoleLog() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupsShouldBeMentionedInServerConsoleLog()
		 {
			  // given
			  DefaultBackupPortHostParams();
			  Config config = Config.defaults();
			  config.Augment( OnlineBackupSettings.online_backup_server, BACKUP_HOST + ":" + _backupPort );
			  _dbRule.withSetting( OnlineBackupSettings.online_backup_enabled, "false" );
			  Config withOnlineBackupDisabled = Config.defaults();
			  CreateAndIndexNode( _dbRule, 1 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.logging.Log log = mock(org.Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  LogProvider logProvider = new LogProviderAnonymousInnerClass3( this, log );
			  Logger logger = mock( typeof( Logger ) );
			  when( log.InfoLogger() ).thenReturn(logger);
			  LogService logService = mock( typeof( LogService ) );
			  when( logService.InternalLogProvider ).thenReturn( logProvider );

			  Dependencies dependencies = new Dependencies( _dbRule.DependencyResolver );
			  dependencies.SatisfyDependencies( config, _monitors, logService );

			  OnlineBackupKernelExtension backup = ( OnlineBackupKernelExtension ) ( new OnlineBackupExtensionFactory() ).newInstance(new SimpleKernelContext(_databaseLayout.databaseDirectory(), DatabaseInfo.UNKNOWN, dependencies), DependenciesProxy.dependencies(dependencies, typeof(OnlineBackupExtensionFactory.Dependencies)));
			  try
			  {
					backup.Start();

					// when
					BackupService().doFullBackup(BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, withOnlineBackupDisabled, BackupClient.BigReadTimeout, false);

					// then
					verify( logger ).log( eq( "%s: Full backup started..." ), Mockito.StartsWith( "BackupServer" ) );
					verify( logger ).log( eq( "%s: Full backup finished." ), Mockito.StartsWith( "BackupServer" ) );

					// when
					CreateAndIndexNode( _dbRule, 2 );

					BackupService().doIncrementalBackupOrFallbackToFull(BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, withOnlineBackupDisabled, BackupClient.BigReadTimeout, false);

					// then
					verify( logger ).log( eq( "%s: Incremental backup started..." ), Mockito.StartsWith( "BackupServer" ) );
					verify( logger ).log( eq( "%s: Incremental backup finished." ), Mockito.StartsWith( "BackupServer" ) );
			  }
			  finally
			  {
					backup.Stop();
			  }
		 }

		 private class LogProviderAnonymousInnerClass3 : LogProvider
		 {
			 private readonly BackupProtocolServiceIT _outerInstance;

			 private Log _log;

			 public LogProviderAnonymousInnerClass3( BackupProtocolServiceIT outerInstance, Log log )
			 {
				 this.outerInstance = outerInstance;
				 this._log = log;
			 }

			 public Log getLog( Type loggingClass )
			 {
				  return _log;
			 }

			 public Log getLog( string name )
			 {
				  return _log;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementalBackupShouldFailWhenTargetDirContainsDifferentStore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncrementalBackupShouldFailWhenTargetDirContainsDifferentStore()
		 {
			  // Given
			  DefaultBackupPortHostParams();
			  Config defaultConfig = Config.defaults();
			  GraphDatabaseAPI db1 = _dbRule.GraphDatabaseAPI;
			  CreateSchemaIndex( db1 );
			  CreateAndIndexNode( db1, 1 );

			  BackupService().doFullBackup(BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false);

			  // When
			  GraphDatabaseAPI db2 = _dbRule.restartDatabase((fs, storeDirectory) =>
			  {
				DeleteAllBackedUpTransactionLogs();
				FileUtils.deletePathRecursively( storeDirectory.databaseDirectory().toPath() );
				Files.createDirectory( storeDirectory.databaseDirectory().toPath() );
			  });
			  CreateAndIndexNode( db2, 2 );

			  try
			  {
					BackupService().doIncrementalBackupOrFallbackToFull(BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, defaultConfig, BackupClient.BigReadTimeout, false);

					fail( "Should have thrown exception about mismatching store ids" );
			  }
			  catch ( Exception e )
			  {
					// Then
					assertThat( e.Message, equalTo( BackupProtocolService.DIFFERENT_STORE_MESSAGE ) );
					assertThat( e.InnerException, instanceOf( typeof( MismatchingStoreIdException ) ) );
			  }
		 }

		 private void DefaultBackupPortHostParams()
		 {
			  _dbRule.withSetting( OnlineBackupSettings.online_backup_server, BACKUP_HOST + ":" + _backupPort );
		 }

		 private static void CreateSchemaIndex( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(_label).on(PROP).create();
					tx.Success();
			  }
			  using ( Transaction ignore = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
			  }
		 }

		 private static void CreateAndIndexNode( IGraphDatabaseService db, int i )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> index = Db.index().forNodes("delete_me");
					Node node = Db.createNode();
					node.SetProperty( PROP, DateTimeHelper.CurrentUnixTimeMillis() + i );
					index.Add( node, "delete", "me" );
					tx.Success();
			  }
		 }

		 private static BaseMatcher<File[]> HasFile( File file )
		 {
			  return new BaseMatcherAnonymousInnerClass( file );
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<File[]>
		 {
			 private File _file;

			 public BaseMatcherAnonymousInnerClass( File file )
			 {
				 this._file = file;
			 }

			 public override bool matches( object o )
			 {
				  File[] files = ( File[] ) o;
				  if ( files == null )
				  {
						return false;
				  }
				  return ArrayUtils.contains( files, _file );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( string.Format( "[{0}] in list of copied files", _file.AbsolutePath ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkPreviousCommittedTxIdFromLog(long logVersion, long txId) throws java.io.IOException
		 private void CheckPreviousCommittedTxIdFromLog( long logVersion, long txId )
		 {
			  // Assert header of specified log version containing correct txId
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _backupDatabaseLayout.databaseDirectory(), _fileSystem ).build();
			  LogHeader logHeader = LogHeaderReader.readLogHeader( _fileSystem, logFiles.GetLogFileForVersion( logVersion ) );
			  assertEquals( txId, logHeader.LastCommittedTxId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkLastCommittedTxIdInLogAndNeoStore(long txId, long txIdFromOrigin) throws Exception
		 private void CheckLastCommittedTxIdInLogAndNeoStore( long txId, long txIdFromOrigin )
		 {
			  // Assert last committed transaction can be found in tx log and is the last tx in the log
			  LifeSupport life = new LifeSupport();
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  LogicalTransactionStore transactionStore = life.Add( new ReadOnlyTransactionStore( pageCache, _fileSystem, _backupDatabaseLayout, Config.defaults(), _monitors ) );
			  life.Start();
			  try
			  {
					  using ( IOCursor<CommittedTransactionRepresentation> cursor = transactionStore.GetTransactions( txId ) )
					  {
						assertTrue( cursor.next() );
						assertEquals( txId, cursor.get().CommitEntry.TxId );
						assertFalse( cursor.next() );
					  }
			  }
			  finally
			  {
					life.Shutdown();
			  }

			  // Assert last committed transaction is correct in neostore
			  assertEquals( txId, txIdFromOrigin );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long getLastTxChecksum(org.Neo4Net.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private long GetLastTxChecksum( PageCache pageCache )
		 {
			  Path neoStore = _backupDatabaseLayout.metadataStore().toPath();
			  return MetaDataStore.getRecord( pageCache, neoStore.toFile(), MetaDataStore.Position.LAST_TRANSACTION_CHECKSUM );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteAllBackedUpTransactionLogs() throws java.io.IOException
		 private void DeleteAllBackedUpTransactionLogs()
		 {
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _backupDatabaseLayout.databaseDirectory(), _fileSystem ).build();
			  foreach ( File log in logFiles.LogFilesConflict() )
			  {
					_fileSystem.deleteFile( log );
			  }
		 }

		 private void DoIncrementalBackupOrFallbackToFull()
		 {
			  BackupProtocolService backupProtocolService = BackupService();
			  backupProtocolService.DoIncrementalBackupOrFallbackToFull( BACKUP_HOST, _backupPort, _backupDatabaseLayout, ConsistencyCheck.NONE, Config.defaults(), BackupClient.BigReadTimeout, false );
		 }

		 private static Node FindNodeByLabel( GraphDatabaseAPI graphDatabase, Label label )
		 {
			  using ( ResourceIterator<Node> nodes = graphDatabase.FindNodes( label ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return nodes.next();
			  }
		 }

		 private DbRepresentation BackupDbRepresentation
		 {
			 get
			 {
				  Config config = Config.defaults( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
				  return DbRepresentation.of( _backupDatabaseLayout.databaseDirectory(), config );
			 }
		 }

		 private DbRepresentation DbRepresentation
		 {
			 get
			 {
				  Config config = Config.defaults( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
				  return DbRepresentation.of( _databaseLayout.databaseDirectory(), config );
			 }
		 }

		 private sealed class StoreSnoopingMonitor : StoreCopyServer.Monitor_Adapter
		 {
			  internal readonly Barrier Barrier;
			  internal readonly DatabaseLayout DatabaseLayout;

			  internal StoreSnoopingMonitor( Barrier barrier, DatabaseLayout databaseLayout )
			  {
					this.Barrier = barrier;
					this.DatabaseLayout = databaseLayout;
			  }

			  public override void FinishStreamingStoreFile( File storefile, string storeCopyIdentifier )
			  {
					if ( DatabaseLayout.nodeStore().Equals(storefile) || DatabaseLayout.relationshipStore().Equals(storefile) )
					{
						 Barrier.reached(); // multiple calls to this barrier will not block
					}
			  }
		 }

		 private class TestFullConsistencyCheck : ConsistencyCheck
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CheckedConflict;
			  public override string Name()
			  {
					return "testFull";
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean runFull(org.Neo4Net.io.layout.DatabaseLayout databaseLayout, org.Neo4Net.kernel.configuration.Config tuningConfiguration, org.Neo4Net.helpers.progress.ProgressMonitorFactory progressFactory, org.Neo4Net.logging.LogProvider logProvider, org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, org.Neo4Net.io.pagecache.PageCache pageCache, boolean verbose, org.Neo4Net.consistency.checking.full.ConsistencyFlags consistencyFlags) throws ConsistencyCheckFailedException
			  public override bool RunFull( DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, bool verbose, ConsistencyFlags consistencyFlags )
			  {
					MarkAsChecked();
					return ConsistencyCheck.FULL.runFull( databaseLayout, tuningConfiguration, progressFactory, logProvider, fileSystem, pageCache, verbose, consistencyFlags );
			  }

			  internal virtual void MarkAsChecked()
			  {
					CheckedConflict = true;
			  }

			  internal virtual bool Checked
			  {
				  get
				  {
						return CheckedConflict;
				  }
			  }
		 }
	}

}