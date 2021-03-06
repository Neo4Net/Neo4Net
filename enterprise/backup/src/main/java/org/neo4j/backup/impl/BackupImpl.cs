﻿using System.Threading;

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
	using ResponsePacker = Org.Neo4j.com.storecopy.ResponsePacker;
	using StoreCopyServer = Org.Neo4j.com.storecopy.StoreCopyServer;
	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using LogFileInformation = Org.Neo4j.Kernel.impl.transaction.log.LogFileInformation;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Logger = Org.Neo4j.Logging.Logger;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

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