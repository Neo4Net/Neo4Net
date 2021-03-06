﻿using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.backup.impl
{

	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.com;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using ExternallyManagedPageCache = Org.Neo4j.com.storecopy.ExternallyManagedPageCache;
	using MoveAfterCopy = Org.Neo4j.com.storecopy.MoveAfterCopy;
	using ResponseUnpacker = Org.Neo4j.com.storecopy.ResponseUnpacker;
	using ResponseUnpacker_TxHandler = Org.Neo4j.com.storecopy.ResponseUnpacker_TxHandler;
	using StoreCopyClient = Org.Neo4j.com.storecopy.StoreCopyClient;
	using StoreCopyClientMonitor = Org.Neo4j.com.storecopy.StoreCopyClientMonitor;
	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using TransactionCommittingResponseUnpacker = Org.Neo4j.com.storecopy.TransactionCommittingResponseUnpacker;
	using ConsistencyFlags = Org.Neo4j.Consistency.checking.full.ConsistencyFlags;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using CancellationRequest = Org.Neo4j.Helpers.CancellationRequest;
	using Service = Org.Neo4j.Helpers.Service;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using Org.Neo4j.Kernel.extension;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using MismatchingStoreIdException = Org.Neo4j.Kernel.impl.store.MismatchingStoreIdException;
	using UnexpectedStoreVersionException = Org.Neo4j.Kernel.impl.store.UnexpectedStoreVersionException;
	using IdGeneratorImpl = Org.Neo4j.Kernel.impl.store.id.IdGeneratorImpl;
	using UpgradeNotAllowedByConfigurationException = Org.Neo4j.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using MissingLogDataException = Org.Neo4j.Kernel.impl.transaction.log.MissingLogDataException;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.RequestContext.anonymous;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.TransactionCommittingResponseUnpacker.DEFAULT_BATCH_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.rootCause;

	/// <summary>
	/// Client-side convenience service for doing backups from a running database instance.
	/// </summary>
	public class BackupProtocolService : AutoCloseable
	{
		 internal static readonly string TooOldBackup = "It's been too long since this backup was last updated, and it has " +
					"fallen too far behind the database transaction stream for incremental backup to be possible. You need to" +
					" perform a full backup at this point. You can modify this time interval by setting the '" +
					GraphDatabaseSettings.keep_logical_logs.name() + "' configuration on the database to a higher value.";

		 static final string DIFFERENT_STORE_MESSAGE = "Target directory contains full backup of a logically different store.";

		 private final System.Func<FileSystemAbstraction> _fileSystemSupplier;
		 private final LogProvider _logProvider;
		 private final Log _log;
		 private final Stream _logDestination;
		 private final Monitors _monitors;
		 private final BackupPageCacheContainer _pageCacheContianer;

		 BackupProtocolService( System.Func<FileSystemAbstraction> _fileSystemSupplier, LogProvider _logProvider, Stream _logDestination, Monitors _monitors, BackupPageCacheContainer pageCacheContainer )
		 {
			  this._fileSystemSupplier = _fileSystemSupplier;
			  this._logProvider = _logProvider;
			  this._log = _logProvider.getLog( this.GetType() );
			  this._logDestination = _logDestination;
			  this._monitors = _monitors;
			  this._pageCacheContianer = pageCacheContainer;
		 }

		 public BackupOutcome DoFullBackup( final string sourceHostNameOrIp, final int sourcePort, DatabaseLayout targetLayout, ConsistencyCheck consistencyCheck, Config tuningConfiguration, final long timeout, final bool forensics )
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = _fileSystemSupplier.get() )
					  {
						return FullBackup( fileSystem, sourceHostNameOrIp, sourcePort, targetLayout, consistencyCheck, tuningConfiguration, timeout, forensics );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private BackupOutcome FullBackup( FileSystemAbstraction fileSystem, string sourceHostNameOrIp, int sourcePort, DatabaseLayout targetLayout, ConsistencyCheck consistencyCheck, Config tuningConfiguration, long timeout, bool forensics )
		 {
			  try
			  {
					if ( !DirectoryIsEmpty( targetLayout ) )
					{
						 throw new Exception( "Can only perform a full backup into an empty directory but " + targetLayout + " is not empty" );
					}
					long timestamp = DateTimeHelper.CurrentUnixTimeMillis();
					long lastCommittedTx = -1;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					StoreCopyClient storeCopier = new StoreCopyClient( targetLayout, tuningConfiguration, LoadKernelExtensions(), _logProvider, fileSystem, _pageCacheContianer.PageCache, _monitors.newMonitor(typeof(StoreCopyClientMonitor), this.GetType().FullName), forensics );
					FullBackupStoreCopyRequester storeCopyRequester = new FullBackupStoreCopyRequester( sourceHostNameOrIp, sourcePort, timeout, forensics, _monitors );
					storeCopier.CopyStore( storeCopyRequester, Org.Neo4j.Helpers.CancellationRequest_Fields.NeverCancelled, MoveAfterCopy.moveReplaceExisting() );

					tuningConfiguration.augment( logs_directory, targetLayout.databaseDirectory().toPath().toRealPath().ToString() );
					File debugLogFile = tuningConfiguration.get( store_internal_log_path );
					BumpDebugDotLogFileVersion( debugLogFile, timestamp );
					bool consistent = CheckDbConsistency( fileSystem, targetLayout, consistencyCheck, tuningConfiguration, _pageCacheContianer.PageCache );
					ClearIdFiles( fileSystem, targetLayout );
					return new BackupOutcome( lastCommittedTx, consistent );
			  }
			  catch ( Exception e )
			  {
					throw e;
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 public BackupOutcome DoIncrementalBackup( string sourceHostNameOrIp, int sourcePort, DatabaseLayout databaseLayout, ConsistencyCheck consistencyCheck, long timeout, Config config ) throws IncrementalBackupNotPossibleException
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = _fileSystemSupplier.get() )
					  {
						return IncrementalBackup( fileSystem, sourceHostNameOrIp, sourcePort, databaseLayout, consistencyCheck, timeout, config );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private BackupOutcome IncrementalBackup( FileSystemAbstraction fileSystem, string sourceHostNameOrIp, int sourcePort, DatabaseLayout targetLayout, ConsistencyCheck consistencyCheck, long timeout, Config config )
		 {
			  try
			  {
					if ( !DirectoryContainsDb( targetLayout ) )
					{
						 throw new Exception( targetLayout + " doesn't contain a database" );
					}

					IDictionary<string, string> temporaryDbConfig = TemporaryDbConfig;
					config.augment( temporaryDbConfig );

					IDictionary<string, string> configParams = config.Raw;
					GraphDatabaseAPI targetDb = StartTemporaryDb( targetLayout.databaseDirectory(), _pageCacheContianer.PageCache, configParams );
					long backupStartTime = DateTimeHelper.CurrentUnixTimeMillis();
					long lastCommittedTx;
					try
					{
						 lastCommittedTx = IncrementalWithContext( sourceHostNameOrIp, sourcePort, targetDb, timeout, SlaveContextOf( targetDb ) );
					}
					finally
					{
						 targetDb.Shutdown();
						 // as soon as recovery will be extracted we will not gonna need this
						 File lockFile = targetLayout.StoreLayout.storeLockFile();
						 if ( lockFile.exists() )
						 {
							  FileUtils.deleteFile( lockFile );
						 }
					}
					config.augment( logs_directory, targetLayout.databaseDirectory().CanonicalPath );
					File debugLogFile = config.get( store_internal_log_path );
					BumpDebugDotLogFileVersion( debugLogFile, backupStartTime );
					bool consistent = CheckDbConsistency( fileSystem, targetLayout, consistencyCheck, config, _pageCacheContianer.PageCache );
					ClearIdFiles( fileSystem, targetLayout );
					return new BackupOutcome( lastCommittedTx, consistent );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private bool CheckDbConsistency( FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout, ConsistencyCheck consistencyCheck, Config tuningConfiguration, PageCache pageCache )
		 {
			  bool consistent = false;
			  try
			  {
					consistent = consistencyCheck.runFull( databaseLayout, tuningConfiguration, ProgressMonitorFactory.textual( _logDestination ), _logProvider, fileSystem, pageCache, false, new ConsistencyFlags( tuningConfiguration ) );
			  }
			  catch ( ConsistencyCheckFailedException e )
			  {
					_log.error( "Consistency check incomplete", e );
			  }
			  return consistent;
		 }

		 private static IDictionary<string, string> TemporaryDbConfig
		 {
			  IDictionary<string, string> tempDbConfig = new Dictionary<string, string>();
			  tempDbConfig[OnlineBackupSettings.online_backup_enabled.name()] = Settings.FALSE;
			  // In case someone deleted the logical log from a full backup
			  tempDbConfig[GraphDatabaseSettings.keep_logical_logs.name()] = Settings.TRUE;
			  tempDbConfig[GraphDatabaseSettings.pagecache_warmup_enabled.name()] = Settings.FALSE;
			  return tempDbConfig;
		 }

		 public BackupOutcome DoIncrementalBackupOrFallbackToFull( string sourceHostNameOrIp, int sourcePort, DatabaseLayout targetLayout, ConsistencyCheck consistencyCheck, Config config, long timeout, bool forensics )
		 {
			  try
			  {
					  using ( FileSystemAbstraction fileSystem = _fileSystemSupplier.get() )
					  {
						if ( DirectoryIsEmpty( targetLayout ) )
						{
							 _log.info( "Previous backup not found, a new full backup will be performed." );
							 return FullBackup( fileSystem, sourceHostNameOrIp, sourcePort, targetLayout, consistencyCheck, config, timeout, forensics );
						}
						try
						{
							 _log.info( "Previous backup found, trying incremental backup." );
							 return IncrementalBackup( fileSystem, sourceHostNameOrIp, sourcePort, targetLayout, consistencyCheck, timeout, config );
						}
						catch ( IncrementalBackupNotPossibleException e )
						{
							 try
							 {
								  _log.warn( "Attempt to do incremental backup failed.", e );
								  _log.info( "Existing backup is too far out of date, a new full backup will be performed." );
								  FileUtils.deletePathRecursively( targetLayout.databaseDirectory().toPath() );
								  return FullBackup( fileSystem, sourceHostNameOrIp, sourcePort, targetLayout, consistencyCheck, config, timeout, forensics );
							 }
							 catch ( Exception fullBackupFailure )
							 {
								  Exception exception = new Exception( "Failed to perform incremental backup, fell back to full backup, but that failed as " + "well: '" + fullBackupFailure.Message + "'.", fullBackupFailure );
								  exception.addSuppressed( e );
								  throw exception;
							 }
						}
					  }
			  }
			  catch ( Exception e )
			  {
					if ( rootCause( e ) is UpgradeNotAllowedByConfigurationException )
					{
						 throw new UnexpectedStoreVersionException( "Failed to perform backup because existing backup is from a different version.", e );
					}

					throw e;
			  }
			  catch ( IOException io )
			  {
					throw new Exception( io );
			  }
		 }

		 public static BackupOutcome DoIncrementalBackup( string sourceHostNameOrIp, int sourcePort, GraphDatabaseAPI targetDb, long timeout ) throws IncrementalBackupNotPossibleException
		 {
			  long lastCommittedTransaction = IncrementalWithContext( sourceHostNameOrIp, sourcePort, targetDb, timeout, SlaveContextOf( targetDb ) );
			  return new BackupOutcome( lastCommittedTransaction, true );
		 }

		 private static RequestContext SlaveContextOf( GraphDatabaseAPI graphDb )
		 {
			  TransactionIdStore transactionIdStore = graphDb.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
			  return anonymous( transactionIdStore.LastCommittedTransactionId );
		 }

		 private static bool DirectoryContainsDb( DatabaseLayout databaseLayout )
		 {
			  return Files.isRegularFile( databaseLayout.metadataStore().toPath() );
		 }

		 private static bool DirectoryIsEmpty( DatabaseLayout databaseLayout ) throws IOException
		 {
			  Path path = databaseLayout.databaseDirectory().toPath();
			  return Files.notExists( path ) || Files.isDirectory( path ) && FileUtils.countFilesInDirectoryPath( path ) == 0;
		 }

		 static GraphDatabaseAPI StartTemporaryDb( File storeDir, PageCache pageCache, IDictionary<string, string> config )
		 {
			  GraphDatabaseFactory factory = ExternallyManagedPageCache.graphDatabaseFactoryWithPageCache( pageCache );
			  return ( GraphDatabaseAPI ) factory.NewEmbeddedDatabaseBuilder( storeDir ).setConfig( config ).setConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ).newGraphDatabase();
		 }

		 /// <summary>
		 /// Performs an incremental backup based off the given context. This means
		 /// receiving and applying selectively (i.e. irrespective of the actual state
		 /// of the target db) a set of transactions starting at the desired txId and
		 /// spanning up to the latest of the master
		 /// </summary>
		 /// <param name="targetDb"> The database that contains a previous full copy </param>
		 /// <param name="context"> The context, containing transaction id to start streaming transaction from </param>
		 /// <returns> last committed transaction id </returns>
		 private static long IncrementalWithContext( string sourceHostNameOrIp, int sourcePort, GraphDatabaseAPI targetDb, long timeout, RequestContext context ) throws IncrementalBackupNotPossibleException
		 {
			  DependencyResolver resolver = targetDb.DependencyResolver;

			  ProgressTxHandler handler = new ProgressTxHandler();
			  TransactionCommittingResponseUnpacker unpacker = new TransactionCommittingResponseUnpacker( resolver, DEFAULT_BATCH_SIZE, 0 );

			  Monitors monitors = resolver.ResolveDependency( typeof( Monitors ) );
			  LogProvider logProvider = resolver.ResolveDependency( typeof( LogService ) ).InternalLogProvider;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  BackupClient client = new BackupClient( sourceHostNameOrIp, sourcePort, null, logProvider, targetDb.storeId(), timeout, unpacker, monitors.NewMonitor(typeof(ByteCounterMonitor), typeof(BackupClient).FullName), monitors.NewMonitor(typeof(RequestMonitor), typeof(BackupClient).FullName), new VersionAwareLogEntryReader<Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>() );

			  try
			  {
					  using ( Lifespan lifespan = new Lifespan( unpacker, client ) )
					  {
						using ( Response<Void> response = client.IncrementalBackup( context ) )
						{
							 unpacker.UnpackResponse( response, handler );
						}
					  }
			  }
			  catch ( MismatchingStoreIdException e )
			  {
					throw new Exception( DIFFERENT_STORE_MESSAGE, e );
			  }
			  catch ( Exception e ) when ( e is Exception || e is IOException )
			  {
					if ( e.InnerException != null && e.InnerException is MissingLogDataException )
					{
						 throw new IncrementalBackupNotPossibleException( TooOldBackup, e.InnerException );
					}
					if ( e.InnerException != null && e.InnerException is ConnectException )
					{
						 throw new Exception( e.Message, e.InnerException );
					}
					throw new Exception( "Failed to perform incremental backup.", e );
			  }
			  catch ( Exception throwable )
			  {
					throw new Exception( "Unexpected error", throwable );
			  }

			  return handler.LastSeenTransactionId;
		 }

		 private static void bumpDebugDotLogFileVersion( final File debugLogFile, final long toTimestamp )
		 {
			  if ( !debugLogFile.exists() )
			  {
					return;
			  }
			  // Build to, from existing parent + new filename
			  File to = new File( debugLogFile.ParentFile, debugLogFile.Name + "." + toTimestamp );
			  debugLogFile.renameTo( to );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.List<org.neo4j.kernel.extension.KernelExtensionFactory<?>> loadKernelExtensions()
		 private static IList<KernelExtensionFactory<object>> LoadKernelExtensions()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions = new java.util.ArrayList<>();
			  IList<KernelExtensionFactory<object>> kernelExtensions = new List<KernelExtensionFactory<object>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.extension.KernelExtensionFactory<?> factory : org.neo4j.helpers.Service.load(org.neo4j.kernel.extension.KernelExtensionFactory.class))
			  foreach ( KernelExtensionFactory<object> factory in Service.load( typeof( KernelExtensionFactory ) ) )
			  {
					kernelExtensions.Add( factory );
			  }
			  return kernelExtensions;
		 }

		 private static void clearIdFiles( FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout ) throws IOException
		 {
			  foreach ( File file in fileSystem.ListFiles( databaseLayout.databaseDirectory() ) )
			  {
					if ( !fileSystem.IsDirectory( file ) && file.Name.EndsWith( ".id" ) )
					{
						 long highId = IdGeneratorImpl.readHighId( fileSystem, file );
						 fileSystem.DeleteFile( file );
						 IdGeneratorImpl.createGenerator( fileSystem, file, highId, true );
					}
			  }
		 }

		 public void close() throws Exception
		 {
			  _pageCacheContianer.close();
		 }

		 private static class ProgressTxHandler implements ResponseUnpacker_TxHandler
		 {
			  private long lastSeenTransactionId;

			  public void accept( long transactionId )
			  {
					lastSeenTransactionId = transactionId;
			  }

			  long LastSeenTransactionId
			  {
					return lastSeenTransactionId;
			  }
		 }

		 private static class FullBackupStoreCopyRequester implements StoreCopyClient.StoreCopyRequester
		 {
			  private final string sourceHostNameOrIp;
			  private final int sourcePort;
			  private final long timeout;
			  private final bool forensics;
			  private final Monitors _monitors;

			  private BackupClient client;

			  private FullBackupStoreCopyRequester( string sourceHostNameOrIp, int sourcePort, long timeout, bool forensics, Monitors _monitors )
			  {
					this.sourceHostNameOrIp = sourceHostNameOrIp;
					this.sourcePort = sourcePort;
					this.timeout = timeout;
					this.forensics = forensics;
					this._monitors = _monitors;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.com.Response<?> copyStore(org.neo4j.com.storecopy.StoreWriter writer)
			  public Response<object> copyStore( StoreWriter writer )
			  {
					client = new BackupClient( sourceHostNameOrIp, sourcePort, null, NullLogProvider.Instance, StoreId.DEFAULT, timeout, Org.Neo4j.com.storecopy.ResponseUnpacker_Fields.NoOpResponseUnpacker, _monitors.newMonitor( typeof( ByteCounterMonitor ) ), _monitors.newMonitor( typeof( RequestMonitor ) ), new VersionAwareLogEntryReader<Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>() );
					client.start();
					return client.fullBackup( writer, forensics );
			  }

			  public void done()
			  {
					client.stop();
			  }
		 }
	}

}