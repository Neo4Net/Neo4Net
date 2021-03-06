﻿using System;

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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{

	using TransactionLogCatchUpFactory = Org.Neo4j.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using TransactionLogCatchUpWriter = Org.Neo4j.causalclustering.catchup.tx.TransactionLogCatchUpWriter;
	using TxPullClient = Org.Neo4j.causalclustering.catchup.tx.TxPullClient;
	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using StoreCopyClientMonitor = Org.Neo4j.com.storecopy.StoreCopyClientMonitor;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_TRANSACTION_PRUNED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	/// <summary>
	/// Entry point for remote store related RPC.
	/// </summary>
	public class RemoteStore
	{
		 private readonly Log _log;
		 private readonly Config _config;
		 private readonly Monitors _monitors;
		 private readonly FileSystemAbstraction _fs;
		 private readonly PageCache _pageCache;
		 private readonly LogProvider _logProvider;
		 private readonly StoreCopyClient _storeCopyClient;
		 private readonly TxPullClient _txPullClient;
		 private readonly TransactionLogCatchUpFactory _transactionLogFactory;
		 private readonly CommitStateHelper _commitStateHelper;

		 public RemoteStore( LogProvider logProvider, FileSystemAbstraction fs, PageCache pageCache, StoreCopyClient storeCopyClient, TxPullClient txPullClient, TransactionLogCatchUpFactory transactionLogFactory, Config config, Monitors monitors )
		 {
			  this._logProvider = logProvider;
			  this._storeCopyClient = storeCopyClient;
			  this._txPullClient = txPullClient;
			  this._fs = fs;
			  this._pageCache = pageCache;
			  this._transactionLogFactory = transactionLogFactory;
			  this._config = config;
			  this._monitors = monitors;
			  this._log = logProvider.getLog( this.GetType() );
			  this._commitStateHelper = new CommitStateHelper( pageCache, fs, config );
		 }

		 /// <summary>
		 /// Later stages of the startup process require at least one transaction to
		 /// figure out the mapping between the transaction log and the consensus log.
		 /// 
		 /// If there are no transaction logs then we can pull from and including
		 /// the index which the metadata store points to. This would be the case
		 /// for example with a backup taken during an idle period of the system.
		 /// 
		 /// However, if there are transaction logs then we want to find out where
		 /// they end and pull from there, excluding the last one so that we do not
		 /// get duplicate entries.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.catchup.CatchupResult tryCatchingUp(org.neo4j.helpers.AdvertisedSocketAddress from, org.neo4j.causalclustering.identity.StoreId expectedStoreId, org.neo4j.io.layout.DatabaseLayout databaseLayout, boolean keepTxLogsInDir, boolean forceTransactionLogRotation) throws StoreCopyFailedException, java.io.IOException
		 public virtual CatchupResult TryCatchingUp( AdvertisedSocketAddress from, StoreId expectedStoreId, DatabaseLayout databaseLayout, bool keepTxLogsInDir, bool forceTransactionLogRotation )
		 {
			  CommitState commitState = _commitStateHelper.getStoreState( databaseLayout );
			  _log.info( "Store commit state: " + commitState );

			  if ( commitState.TransactionLogIndex().HasValue )
			  {
					return PullTransactions( from, expectedStoreId, databaseLayout, commitState.TransactionLogIndex().Value + 1, false, keepTxLogsInDir, forceTransactionLogRotation );
			  }
			  else
			  {
					CatchupResult catchupResult;
					if ( commitState.MetaDataStoreIndex() == BASE_TX_ID )
					{
						 return PullTransactions( from, expectedStoreId, databaseLayout, commitState.MetaDataStoreIndex() + 1, false, keepTxLogsInDir, forceTransactionLogRotation );
					}
					else
					{
						 catchupResult = PullTransactions( from, expectedStoreId, databaseLayout, commitState.MetaDataStoreIndex(), false, keepTxLogsInDir, forceTransactionLogRotation );
						 if ( catchupResult == E_TRANSACTION_PRUNED )
						 {
							  return PullTransactions( from, expectedStoreId, databaseLayout, commitState.MetaDataStoreIndex() + 1, false, keepTxLogsInDir, forceTransactionLogRotation );
						 }
					}
					return catchupResult;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copy(org.neo4j.causalclustering.catchup.CatchupAddressProvider addressProvider, org.neo4j.causalclustering.identity.StoreId expectedStoreId, org.neo4j.io.layout.DatabaseLayout destinationLayout, boolean rotateTransactionsManually) throws StoreCopyFailedException
		 public virtual void Copy( CatchupAddressProvider addressProvider, StoreId expectedStoreId, DatabaseLayout destinationLayout, bool rotateTransactionsManually )
		 {
			  try
			  {
					long lastFlushedTxId;
					StreamToDiskProvider streamToDiskProvider = new StreamToDiskProvider( destinationLayout.DatabaseDirectory(), _fs, _monitors );
					lastFlushedTxId = _storeCopyClient.copyStoreFiles( addressProvider, expectedStoreId, streamToDiskProvider, () => new MaximumTotalTime(_config.get(CausalClusteringSettings.store_copy_max_retry_time_per_request).Seconds, TimeUnit.SECONDS), destinationLayout.DatabaseDirectory() );

					_log.info( "Store files need to be recovered starting from: %d", lastFlushedTxId );

					CatchupResult catchupResult = PullTransactions( addressProvider.Primary(), expectedStoreId, destinationLayout, lastFlushedTxId, true, true, rotateTransactionsManually );
					if ( catchupResult != SUCCESS_END_OF_STREAM )
					{
						 throw new StoreCopyFailedException( "Failed to pull transactions: " + catchupResult );
					}
			  }
			  catch ( Exception e ) when ( e is CatchupAddressResolutionException || e is IOException )
			  {
					throw new StoreCopyFailedException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.catchup.CatchupResult pullTransactions(org.neo4j.helpers.AdvertisedSocketAddress from, org.neo4j.causalclustering.identity.StoreId expectedStoreId, org.neo4j.io.layout.DatabaseLayout databaseLayout, long fromTxId, boolean asPartOfStoreCopy, boolean keepTxLogsInStoreDir, boolean rotateTransactionsManually) throws java.io.IOException, StoreCopyFailedException
		 private CatchupResult PullTransactions( AdvertisedSocketAddress from, StoreId expectedStoreId, DatabaseLayout databaseLayout, long fromTxId, bool asPartOfStoreCopy, bool keepTxLogsInStoreDir, bool rotateTransactionsManually )
		 {
			  StoreCopyClientMonitor storeCopyClientMonitor = _monitors.newMonitor( typeof( StoreCopyClientMonitor ) );
			  storeCopyClientMonitor.StartReceivingTransactions( fromTxId );
			  long previousTxId = fromTxId - 1;
			  try
			  {
					  using ( TransactionLogCatchUpWriter writer = _transactionLogFactory.create( databaseLayout, _fs, _pageCache, _config, _logProvider, fromTxId, asPartOfStoreCopy, keepTxLogsInStoreDir, rotateTransactionsManually ) )
					  {
						_log.info( "Pulling transactions from %s starting with txId: %d", from, fromTxId );
						CatchupResult lastStatus;
      
						TxPullRequestResult result = _txPullClient.pullTransactions( from, expectedStoreId, previousTxId, writer );
						lastStatus = result.CatchupResult();
						previousTxId = result.LastTxId();
      
						return lastStatus;
					  }
			  }
			  catch ( CatchUpClientException e )
			  {
					throw new StoreCopyFailedException( e );
			  }
			  finally
			  {
					storeCopyClientMonitor.FinishReceivingTransactions( previousTxId );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.identity.StoreId getStoreId(org.neo4j.helpers.AdvertisedSocketAddress from) throws StoreIdDownloadFailedException
		 public virtual StoreId GetStoreId( AdvertisedSocketAddress from )
		 {
			  return _storeCopyClient.fetchStoreId( from );
		 }
	}

}