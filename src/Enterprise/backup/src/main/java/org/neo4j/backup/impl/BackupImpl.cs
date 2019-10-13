using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.backup.impl
{

	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using ResponsePacker = Neo4Net.com.storecopy.ResponsePacker;
	using StoreCopyServer = Neo4Net.com.storecopy.StoreCopyServer;
	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using LogFileInformation = Neo4Net.Kernel.impl.transaction.log.LogFileInformation;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Logger = Neo4Net.Logging.Logger;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.RequestContext.anonymous;

	public class BackupImpl : TheBackupInterface
	{
		 internal const string FULL_BACKUP_CHECKPOINT_TRIGGER = "full backup";

		 private readonly StoreCopyServer _storeCopyServer;
		 private readonly ResponsePacker _incrementalResponsePacker;
		 private readonly LogicalTransactionStore _logicalTransactionStore;
		 private readonly System.Func<StoreId> _storeId;
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly LogFileInformation _logFileInformation;
		 private readonly Logger _logger;

		 public BackupImpl( StoreCopyServer storeCopyServer, LogicalTransactionStore logicalTransactionStore, TransactionIdStore transactionIdStore, LogFileInformation logFileInformation, System.Func<StoreId> storeId, LogProvider logProvider )
		 {
			  this._storeCopyServer = storeCopyServer;
			  this._logicalTransactionStore = logicalTransactionStore;
			  this._transactionIdStore = transactionIdStore;
			  this._logFileInformation = logFileInformation;
			  this._storeId = storeId;
			  this._logger = logProvider.getLog( this.GetType() ).InfoLogger();
			  this._incrementalResponsePacker = new ResponsePacker( logicalTransactionStore, transactionIdStore, storeId );
		 }

		 public override Response<Void> FullBackup( StoreWriter writer, bool forensics )
		 {
			  string backupIdentifier = BackupIdentifier;
			  try
			  {
					  using ( StoreWriter storeWriter = writer )
					  {
						_logger.log( "%s: Full backup started...", backupIdentifier );
						RequestContext copyStartContext = _storeCopyServer.flushStoresAndStreamStoreFiles( FULL_BACKUP_CHECKPOINT_TRIGGER, storeWriter, forensics );
						ResponsePacker responsePacker = new StoreCopyResponsePacker( _logicalTransactionStore, _transactionIdStore, _logFileInformation, _storeId, copyStartContext.LastAppliedTransaction() + 1, _storeCopyServer.monitor() );
						long optionalTransactionId = copyStartContext.LastAppliedTransaction();
						return responsePacker.PackTransactionStreamResponse( anonymous( optionalTransactionId ), null );
					  }
			  }
			  finally
			  {
					_logger.log( "%s: Full backup finished.", backupIdentifier );
			  }
		 }

		 public override Response<Void> IncrementalBackup( RequestContext context )
		 {
			  string backupIdentifier = BackupIdentifier;
			  try
			  {
					_logger.log( "%s: Incremental backup started...", backupIdentifier );
					return _incrementalResponsePacker.packTransactionStreamResponse( context, null );
			  }
			  finally
			  {
					_logger.log( "%s: Incremental backup finished.", backupIdentifier );
			  }
		 }

		 private static string BackupIdentifier
		 {
			 get
			 {
				  return Thread.CurrentThread.Name;
			 }
		 }
	}

}