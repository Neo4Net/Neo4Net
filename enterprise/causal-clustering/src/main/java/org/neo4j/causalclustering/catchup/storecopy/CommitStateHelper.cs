﻿/*
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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using NoSuchTransactionException = Org.Neo4j.Kernel.impl.transaction.log.NoSuchTransactionException;
	using ReadOnlyTransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.ReadOnlyTransactionIdStore;
	using ReadOnlyTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.ReadOnlyTransactionStore;
	using TransactionCursor = Org.Neo4j.Kernel.impl.transaction.log.TransactionCursor;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class CommitStateHelper
	{
		 private PageCache _pageCache;
		 private FileSystemAbstraction _fs;
		 private Config _config;

		 public CommitStateHelper( PageCache pageCache, FileSystemAbstraction fs, Config config )
		 {
			  this._pageCache = pageCache;
			  this._fs = fs;
			  this._config = config;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CommitState getStoreState(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 internal virtual CommitState GetStoreState( DatabaseLayout databaseLayout )
		 {
			  ReadOnlyTransactionIdStore metaDataStore = new ReadOnlyTransactionIdStore( _pageCache, databaseLayout );
			  long metaDataStoreTxId = metaDataStore.LastCommittedTransactionId;

			  long? latestTransactionLogIndex = GetLatestTransactionLogIndex( metaDataStoreTxId, databaseLayout );

			  //noinspection OptionalIsPresent
			  if ( latestTransactionLogIndex.HasValue )
			  {
					return new CommitState( metaDataStoreTxId, latestTransactionLogIndex.Value );
			  }
			  else
			  {
					return new CommitState( metaDataStoreTxId );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Optional<long> getLatestTransactionLogIndex(long startTxId, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private long? GetLatestTransactionLogIndex( long startTxId, DatabaseLayout databaseLayout )
		 {
			  if ( !HasTxLogs( databaseLayout ) )
			  {
					return null;
			  }

			  // this is not really a read-only store, because it will create an empty transaction log if there is none
			  ReadOnlyTransactionStore txStore = new ReadOnlyTransactionStore( _pageCache, _fs, databaseLayout, _config, new Monitors() );

			  long lastTxId = BASE_TX_ID;
			  try
			  {
					  using ( Lifespan ignored = new Lifespan( txStore ), TransactionCursor cursor = txStore.GetTransactions( startTxId ) )
					  {
						while ( cursor.next() )
						{
							 CommittedTransactionRepresentation tx = cursor.get();
							 lastTxId = tx.CommitEntry.TxId;
						}
      
						return lastTxId;
					  }
			  }
			  catch ( NoSuchTransactionException )
			  {
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean hasTxLogs(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 public virtual bool HasTxLogs( DatabaseLayout databaseLayout )
		 {
			  return LogFilesBuilder.activeFilesBuilder( databaseLayout, _fs, _pageCache ).withConfig( _config ).build().logFiles().Length > 0;
		 }
	}

}