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
namespace Neo4Net.causalclustering.catchup.tx
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using TransactionLogWriter = Neo4Net.Kernel.impl.transaction.log.TransactionLogWriter;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.MetaDataStore.Position.LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.StoreType.META_DATA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

	public class TransactionLogCatchUpWriter : TxPullResponseListener, IDisposable
	{
		 private readonly Lifespan _lifespan = new Lifespan();
		 private readonly PageCache _pageCache;
		 private readonly Log _log;
		 private readonly bool _asPartOfStoreCopy;
		 private readonly TransactionLogWriter _writer;
		 private readonly LogFiles _logFiles;
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly NeoStores _stores;
		 private readonly bool _rotateTransactionsManually;

		 private long _lastTxId = -1;
		 private long _expectedTxId;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionLogCatchUpWriter(org.Neo4Net.io.layout.DatabaseLayout databaseLayout, org.Neo4Net.io.fs.FileSystemAbstraction fs, org.Neo4Net.io.pagecache.PageCache pageCache, org.Neo4Net.kernel.configuration.Config config, org.Neo4Net.logging.LogProvider logProvider, long fromTxId, boolean asPartOfStoreCopy, boolean keepTxLogsInStoreDir, boolean forceTransactionRotations) throws java.io.IOException
		 internal TransactionLogCatchUpWriter( DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache, Config config, LogProvider logProvider, long fromTxId, bool asPartOfStoreCopy, bool keepTxLogsInStoreDir, bool forceTransactionRotations )
		 {
			  this._pageCache = pageCache;
			  this._log = logProvider.getLog( this.GetType() );
			  this._asPartOfStoreCopy = asPartOfStoreCopy;
			  this._rotateTransactionsManually = forceTransactionRotations;
			  RecordFormats recordFormats = RecordFormatSelector.selectForStoreOrConfig( Config.defaults(), databaseLayout, fs, pageCache, logProvider );
			  this._stores = ( new StoreFactory( databaseLayout, config, new DefaultIdGeneratorFactory( fs ), pageCache, fs, recordFormats, logProvider, EMPTY ) ).openNeoStores( META_DATA );
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( _stores.MetaDataStore );
			  LogFilesBuilder logFilesBuilder = LogFilesBuilder.builder( databaseLayout, fs ).withDependencies( dependencies ).withLastCommittedTransactionIdSupplier( () => fromTxId - 1 ).withConfig(CustomisedConfig(config, keepTxLogsInStoreDir, forceTransactionRotations)).withLogVersionRepository(_stores.MetaDataStore);
			  this._logFiles = logFilesBuilder.Build();
			  this._lifespan.add( _logFiles );
			  this._writer = new TransactionLogWriter( new LogEntryWriter( _logFiles.LogFile.Writer ) );
			  this._databaseLayout = databaseLayout;
			  this._expectedTxId = fromTxId;
		 }

		 private Config CustomisedConfig( Config original, bool keepTxLogsInStoreDir, bool forceTransactionRotations )
		 {
			  Config config = Config.builder().build();
			  if ( !keepTxLogsInStoreDir )
			  {
					original.GetRaw( GraphDatabaseSettings.logical_logs_location.name() ).ifPresent(v => config.augment(GraphDatabaseSettings.logical_logs_location, v));
			  }
			  if ( forceTransactionRotations )
			  {
					original.GetRaw( GraphDatabaseSettings.logical_log_rotation_threshold.name() ).ifPresent(v => config.augment(GraphDatabaseSettings.logical_log_rotation_threshold, v));
			  }
			  return config;
		 }

		 public override void OnTxReceived( TxPullResponse txPullResponse )
		 {
			 lock ( this )
			 {
				  CommittedTransactionRepresentation tx = txPullResponse.Tx();
				  long receivedTxId = tx.CommitEntry.TxId;
      
				  // Neo4Net admin backup clients pull transactions indefinitely and have no monitoring mechanism for tx log rotation
				  // Other cases, ex. Read Replicas have an external mechanism that rotates independently of this process and don't need to
				  // manually rotate while pulling
				  if ( _rotateTransactionsManually && _logFiles.LogFile.rotationNeeded() )
				  {
						RotateTransactionLogs( _logFiles );
				  }
      
				  if ( receivedTxId != _expectedTxId )
				  {
						throw new Exception( format( "Expected txId: %d but got: %d", _expectedTxId, receivedTxId ) );
				  }
      
				  _lastTxId = receivedTxId;
				  _expectedTxId++;
      
				  try
				  {
						_writer.append( tx.TransactionRepresentation, _lastTxId );
				  }
				  catch ( IOException e )
				  {
						_log.error( "Failed when appending to transaction log", e );
				  }
			 }
		 }

		 private static void RotateTransactionLogs( LogFiles logFiles )
		 {
			  try
			  {
					logFiles.LogFile.rotate();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void close() throws java.io.IOException
		 public override void Close()
		 {
			 lock ( this )
			 {
				  if ( _asPartOfStoreCopy )
				  {
						/* A checkpoint which points to the beginning of all the log files, meaning that
						all the streamed transactions will be applied as part of recovery. */
						long logVersion = _logFiles.LowestLogVersion;
						LogPosition checkPointPosition = new LogPosition( logVersion, LOG_HEADER_SIZE );
      
						_log.info( "Writing checkpoint as part of store copy: " + checkPointPosition );
						_writer.checkPoint( checkPointPosition );
      
						// * comment copied from old StoreCopyClient *
						// since we just create new log and put checkpoint into it with offset equals to
						// LOG_HEADER_SIZE we need to update last transaction offset to be equal to this newly defined max
						// offset otherwise next checkpoint that use last transaction offset will be created for non
						// existing offset that is in most of the cases bigger than new log size.
						// Recovery will treat that as last checkpoint and will not try to recover store till new
						// last closed transaction offset will not overcome old one. Till that happens it will be
						// impossible for recovery process to restore the store
						File neoStore = _databaseLayout.metadataStore();
						MetaDataStore.setRecord( _pageCache, neoStore, LAST_CLOSED_TRANSACTION_LOG_BYTE_OFFSET, checkPointPosition.ByteOffset );
				  }
      
				  _lifespan.close();
      
				  if ( _lastTxId != -1 )
				  {
						File neoStoreFile = _databaseLayout.metadataStore();
						MetaDataStore.setRecord( _pageCache, neoStoreFile, LAST_TRANSACTION_ID, _lastTxId );
				  }
				  _stores.close();
			 }
		 }
	}

}