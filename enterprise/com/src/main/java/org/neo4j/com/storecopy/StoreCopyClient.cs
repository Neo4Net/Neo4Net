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
namespace Org.Neo4j.com.storecopy
{

	using Org.Neo4j.com;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using CancellationRequest = Org.Neo4j.Helpers.CancellationRequest;
	using Org.Neo4j.Helpers.Collection;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using StoreLayout = Org.Neo4j.Io.layout.StoreLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using Org.Neo4j.Kernel.extension;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using FlushableChannel = Org.Neo4j.Kernel.impl.transaction.log.FlushableChannel;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using TransactionLogWriter = Org.Neo4j.Kernel.impl.transaction.log.TransactionLogWriter;
	using LogEntryWriter = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderWriter.writeLogHeader;

	/// <summary>
	/// Client-side store copier. Deals with issuing a request to a source of a database, which will
	/// reply with a <seealso cref="Response"/> containing the store files and transactions happening while streaming
	/// all the files. After the store files have been streamed, the transactions will be applied so that
	/// the store will end up in a consistent state.
	/// </summary>
	/// <seealso cref= StoreCopyServer </seealso>
	public class StoreCopyClient
	{

		 /// <summary>
		 /// This is built as a pluggable interface to allow backup and HA to use this code independently of each other,
		 /// each implements it's own version of how to copy a store from a remote location.
		 /// </summary>
		 public interface StoreCopyRequester
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.com.Response<?> copyStore(StoreWriter writer);
			  Response<object> CopyStore( StoreWriter writer );

			  void Done();
		 }

		 private readonly DatabaseLayout _databaseLayout;
		 private readonly Config _config;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions;
		 private readonly IEnumerable<KernelExtensionFactory<object>> _kernelExtensions;
		 private readonly Log _log;
		 private readonly FileSystemAbstraction _fs;
		 private readonly PageCache _pageCache;
		 private readonly StoreCopyClientMonitor _monitor;
		 private readonly bool _forensics;
		 private readonly FileMoveProvider _fileMoveProvider;

		 public StoreCopyClient<T1>( DatabaseLayout databaseLayout, Config config, IEnumerable<T1> kernelExtensions, LogProvider logProvider, FileSystemAbstraction fs, PageCache pageCache, StoreCopyClientMonitor monitor, bool forensics ) : this( databaseLayout, config, kernelExtensions, logProvider, fs, pageCache, monitor, forensics, new FileMoveProvider( fs ) )
		 {
		 }

		 public StoreCopyClient<T1>( DatabaseLayout databaseLayout, Config config, IEnumerable<T1> kernelExtensions, LogProvider logProvider, FileSystemAbstraction fs, PageCache pageCache, StoreCopyClientMonitor monitor, bool forensics, FileMoveProvider fileMoveProvider )
		 {
			  this._databaseLayout = databaseLayout;
			  this._config = config;
			  this._kernelExtensions = kernelExtensions;
			  this._log = logProvider.getLog( this.GetType() );
			  this._fs = fs;
			  this._pageCache = pageCache;
			  this._monitor = monitor;
			  this._forensics = forensics;
			  this._fileMoveProvider = fileMoveProvider;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyStore(StoreCopyRequester requester, org.neo4j.helpers.CancellationRequest cancellationRequest, MoveAfterCopy moveAfterCopy) throws Exception
		 public virtual void CopyStore( StoreCopyRequester requester, CancellationRequest cancellationRequest, MoveAfterCopy moveAfterCopy )
		 {
			  // Create a temp directory (or clean if present)
			  File tempDatabaseDirectory = _databaseLayout.file( StoreUtil.TEMP_COPY_DIRECTORY_NAME );
			  try
			  {
					CleanDirectory( tempDatabaseDirectory );

					// Request store files and transactions that will need recovery
					_monitor.startReceivingStoreFiles();
					ToFileStoreWriter storeWriter = new ToFileStoreWriter( tempDatabaseDirectory, _fs, _monitor );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: try (org.neo4j.com.Response<?> response = requester.copyStore(decorateWithProgressIndicator(storeWriter)))
					try
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: using (org.neo4j.com.Response<JavaToDotNetGenericWildcard> response = requester.copyStore(decorateWithProgressIndicator(storeWriter)))
							using ( Response<object> response = requester.CopyStore( DecorateWithProgressIndicator( storeWriter ) ) )
							{
							 _monitor.finishReceivingStoreFiles();
							 // Update highest archived log id
							 // Write transactions that happened during the copy to the currently active logical log
							 WriteTransactionsToActiveLogFile( DatabaseLayout.of( tempDatabaseDirectory ), response );
							}
					}
					finally
					{
						 requester.Done();
					}

					// This is a good place to check if the switch has been cancelled
					CheckCancellation( cancellationRequest, tempDatabaseDirectory );

					// Run recovery, so that the transactions we just wrote into the active log will be applied.
					RecoverDatabase( tempDatabaseDirectory );

					// All is well, move the streamed files to the real store directory.
					// Should only be record store files.
					// Note that the stream is lazy, so the file system traversal won't happen until *after* the store files
					// have been moved. Thus we ensure that we only attempt to move them once.
					MoveFromTemporaryLocationToCorrect( tempDatabaseDirectory, moveAfterCopy );
			  }
			  finally
			  {
					// All done, delete temp directory
					FileUtils.deleteRecursively( tempDatabaseDirectory );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void moveFromTemporaryLocationToCorrect(java.io.File tempStore, MoveAfterCopy moveAfterCopy) throws Exception
		 private void MoveFromTemporaryLocationToCorrect( File tempStore, MoveAfterCopy moveAfterCopy )
		 {
			  LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( _databaseLayout, _fs, _pageCache ).withConfig( _config ).build();

			  Stream<FileMoveAction> moveActionStream = _fileMoveProvider.traverseForMoving( tempStore );
			  System.Func<File, File> destinationMapper = file => logFiles.IsLogFile( file ) ? logFiles.LogFilesDirectory() : _databaseLayout.databaseDirectory();
			  moveAfterCopy.Move( moveActionStream, tempStore, destinationMapper );
		 }

		 private void RecoverDatabase( File tempStore )
		 {
			  _monitor.startRecoveringStore();
			  File storeDir = tempStore.ParentFile;
			  GraphDatabaseService graphDatabaseService = NewTempDatabase( tempStore );
			  graphDatabaseService.Shutdown();
			  // as soon as recovery will be extracted we will not gonna need this
			  File lockFile = StoreLayout.of( storeDir ).storeLockFile();
			  if ( lockFile.exists() )
			  {
					FileUtils.deleteFile( lockFile );
			  }
			  _monitor.finishRecoveringStore();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeTransactionsToActiveLogFile(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.com.Response<?> response) throws Exception
		 private void WriteTransactionsToActiveLogFile<T1>( DatabaseLayout databaseLayout, Response<T1> response )
		 {
			  LifeSupport life = new LifeSupport();
			  try
			  {
					// Start the log and appender
					LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( databaseLayout, _fs, _pageCache ).build();
					life.Add( logFiles );
					life.Start();

					// Just write all transactions to the active log version. Remember that this is after a store copy
					// where there are no logs, and the transaction stream we're about to write will probably contain
					// transactions that goes some time back, before the last committed transaction id. So we cannot
					// use a TransactionAppender, since it has checks for which transactions one can append.
					FlushableChannel channel = logFiles.LogFile.Writer;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.TransactionLogWriter writer = new org.neo4j.kernel.impl.transaction.log.TransactionLogWriter(new org.neo4j.kernel.impl.transaction.log.entry.LogEntryWriter(channel));
					TransactionLogWriter writer = new TransactionLogWriter( new LogEntryWriter( channel ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong firstTxId = new java.util.concurrent.atomic.AtomicLong(BASE_TX_ID);
					AtomicLong firstTxId = new AtomicLong( BASE_TX_ID );

					response.Accept( new HandlerAnonymousInnerClass( this, writer, firstTxId ) );

					long endTxId = firstTxId.get();
					if ( endTxId != BASE_TX_ID )
					{
						 _monitor.finishReceivingTransactions( endTxId );
					}

					long currentLogVersion = logFiles.HighestLogVersion;
					writer.CheckPoint( new LogPosition( currentLogVersion, LOG_HEADER_SIZE ) );

					// And since we write this manually we need to set the correct transaction id in the
					// header of the log that we just wrote.
					File currentLogFile = logFiles.GetLogFileForVersion( currentLogVersion );
					writeLogHeader( _fs, currentLogFile, currentLogVersion, max( BASE_TX_ID, endTxId - 1 ) );

					if ( !_forensics )
					{
						 // since we just create new log and put checkpoint into it with offset equals to
						 // LOG_HEADER_SIZE we need to update last transaction offset to be equal to this newly defined max
						 // offset otherwise next checkpoint that use last transaction offset will be created for non
						 // existing offset that is in most of the cases bigger than new log size.
						 // Recovery will treat that as last checkpoint and will not try to recover store till new
						 // last closed transaction offset will not overcome old one. Till that happens it will be
						 // impossible for recovery process to restore the store
						 File neoStore = databaseLayout.MetadataStore();
						 MetaDataStore.setRecord( _pageCache, neoStore, MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET, LOG_HEADER_SIZE );
					}
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

		 private class HandlerAnonymousInnerClass : Response.Handler
		 {
			 private readonly StoreCopyClient _outerInstance;

			 private TransactionLogWriter _writer;
			 private AtomicLong _firstTxId;

			 public HandlerAnonymousInnerClass( StoreCopyClient outerInstance, TransactionLogWriter writer, AtomicLong firstTxId )
			 {
				 this.outerInstance = outerInstance;
				 this._writer = writer;
				 this._firstTxId = firstTxId;
			 }

			 public void obligation( long txId )
			 {
				  throw new System.NotSupportedException( "Shouldn't be called" );
			 }

			 public Visitor<CommittedTransactionRepresentation, Exception> transactions()
			 {
				  return transaction =>
				  {
					long txId = transaction.CommitEntry.TxId;
					if ( _firstTxId.compareAndSet( BASE_TX_ID, txId ) )
					{
						 _outerInstance.monitor.startReceivingTransactions( txId );
					}
					_writer.append( transaction.TransactionRepresentation, txId );
					return false;
				  };
			 }
		 }

		 private GraphDatabaseService NewTempDatabase( File tempStore )
		 {
			  ExternallyManagedPageCache.GraphDatabaseFactoryWithPageCacheFactory factory = ExternallyManagedPageCache.GraphDatabaseFactoryWithPageCache( _pageCache );
			  return factory.setKernelExtensions( _kernelExtensions ).setUserLogProvider( NullLogProvider.Instance ).newEmbeddedDatabaseBuilder( tempStore.AbsoluteFile ).setConfig( GraphDatabaseSettings.active_database, tempStore.Name ).setConfig( "dbms.backup.enabled", Settings.FALSE ).setConfig( GraphDatabaseSettings.pagecache_warmup_enabled, Settings.FALSE ).setConfig( GraphDatabaseSettings.logs_directory, tempStore.AbsolutePath ).setConfig( GraphDatabaseSettings.keep_logical_logs, Settings.TRUE ).setConfig( GraphDatabaseSettings.logical_logs_location, tempStore.AbsolutePath ).setConfig( GraphDatabaseSettings.allow_upgrade, _config.get( GraphDatabaseSettings.allow_upgrade ).ToString() ).setConfig(GraphDatabaseSettings.default_schema_provider, _config.get(GraphDatabaseSettings.default_schema_provider)).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private StoreWriter decorateWithProgressIndicator(final StoreWriter actual)
		 private StoreWriter DecorateWithProgressIndicator( StoreWriter actual )
		 {
			  return new StoreWriterAnonymousInnerClass( this, actual );
		 }

		 private class StoreWriterAnonymousInnerClass : StoreWriter
		 {
			 private readonly StoreCopyClient _outerInstance;

			 private Org.Neo4j.com.storecopy.StoreWriter _actual;

			 public StoreWriterAnonymousInnerClass( StoreCopyClient outerInstance, Org.Neo4j.com.storecopy.StoreWriter actual )
			 {
				 this.outerInstance = outerInstance;
				 this._actual = actual;
			 }

			 private int totalFiles;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(String path, java.nio.channels.ReadableByteChannel data, ByteBuffer temporaryBuffer, boolean hasData, int requiredElementAlignment) throws java.io.IOException
			 public long write( string path, ReadableByteChannel data, ByteBuffer temporaryBuffer, bool hasData, int requiredElementAlignment )
			 {
				  _outerInstance.log.info( "Copying %s", path );
				  long written = _actual.write( path, data, temporaryBuffer, hasData, requiredElementAlignment );
				  _outerInstance.log.info( "Copied %s %s", path, bytes( written ) );
				  totalFiles++;
				  return written;
			 }

			 public void close()
			 {
				  _actual.Dispose();
				  _outerInstance.log.info( "Done, copied %s files", totalFiles );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void cleanDirectory(java.io.File directory) throws java.io.IOException
		 private static void CleanDirectory( File directory )
		 {
			  if ( !directory.mkdir() )
			  {
					FileUtils.deleteRecursively( directory );
					directory.mkdir();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkCancellation(org.neo4j.helpers.CancellationRequest cancellationRequest, java.io.File tempStore) throws java.io.IOException
		 private void CheckCancellation( CancellationRequest cancellationRequest, File tempStore )
		 {
			  if ( cancellationRequest.CancellationRequested() )
			  {
					_log.info( "Store copying was cancelled. Cleaning up temp-directories." );
					CleanDirectory( tempStore );
			  }
		 }
	}

}