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
namespace Neo4Net.causalclustering.catchup.storecopy
{

	using TransactionLogCatchUpFactory = Neo4Net.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using TransactionLogCatchUpWriter = Neo4Net.causalclustering.catchup.tx.TransactionLogCatchUpWriter;
	using TxPullClient = Neo4Net.causalclustering.catchup.tx.TxPullClient;
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using StoreCopyClientMonitor = Neo4Net.com.storecopy.StoreCopyClientMonitor;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.catchup.CatchupResult.E_TRANSACTION_PRUNED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

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
//ORIGINAL LINE: public org.Neo4Net.causalclustering.catchup.CatchupResult tryCatchingUp(org.Neo4Net.helpers.AdvertisedSocketAddress from, org.Neo4Net.causalclustering.identity.StoreId expectedStoreId, org.Neo4Net.io.layout.DatabaseLayout databaseLayout, boolean keepTxLogsInDir, boolean forceTransactionLogRotation) throws StoreCopyFailedException, java.io.IOException
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
//ORIGINAL LINE: public void copy(org.Neo4Net.causalclustering.catchup.CatchupAddressProvider addressProvider, org.Neo4Net.causalclustering.identity.StoreId expectedStoreId, org.Neo4Net.io.layout.DatabaseLayout destinationLayout, boolean rotateTransactionsManually) throws StoreCopyFailedException
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
//ORIGINAL LINE: private org.Neo4Net.causalclustering.catchup.CatchupResult pullTransactions(org.Neo4Net.helpers.AdvertisedSocketAddress from, org.Neo4Net.causalclustering.identity.StoreId expectedStoreId, org.Neo4Net.io.layout.DatabaseLayout databaseLayout, long fromTxId, boolean asPartOfStoreCopy, boolean keepTxLogsInStoreDir, boolean rotateTransactionsManually) throws java.io.IOException, StoreCopyFailedException
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
//ORIGINAL LINE: public org.Neo4Net.causalclustering.identity.StoreId getStoreId(org.Neo4Net.helpers.AdvertisedSocketAddress from) throws StoreIdDownloadFailedException
		 public virtual StoreId GetStoreId( AdvertisedSocketAddress from )
		 {
			  return _storeCopyClient.fetchStoreId( from );
		 }
	}

}