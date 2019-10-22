﻿/*
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

	using Neo4Net.causalclustering.catchup;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	public class TxPullClient
	{
		 private readonly CatchUpClient _catchUpClient;
		 private PullRequestMonitor _pullRequestMonitor;

		 public TxPullClient( CatchUpClient catchUpClient, Monitors monitors )
		 {
			  this._catchUpClient = catchUpClient;
			  this._pullRequestMonitor = monitors.NewMonitor( typeof( PullRequestMonitor ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.causalclustering.catchup.TxPullRequestResult pullTransactions(org.Neo4Net.helpers.AdvertisedSocketAddress fromAddress, org.Neo4Net.causalclustering.identity.StoreId storeId, long previousTxId, TxPullResponseListener txPullResponseListener) throws org.Neo4Net.causalclustering.catchup.CatchUpClientException
		 public virtual TxPullRequestResult PullTransactions( AdvertisedSocketAddress fromAddress, StoreId storeId, long previousTxId, TxPullResponseListener txPullResponseListener )
		 {
			  _pullRequestMonitor.txPullRequest( previousTxId );
			  return _catchUpClient.makeBlockingRequest( fromAddress, new TxPullRequest( previousTxId, storeId ), new CatchUpResponseAdaptorAnonymousInnerClass( this, previousTxId, txPullResponseListener ) );
		 }

		 private class CatchUpResponseAdaptorAnonymousInnerClass : CatchUpResponseAdaptor<TxPullRequestResult>
		 {
			 private readonly TxPullClient _outerInstance;

			 private long _previousTxId;
			 private Neo4Net.causalclustering.catchup.tx.TxPullResponseListener _txPullResponseListener;

			 public CatchUpResponseAdaptorAnonymousInnerClass( TxPullClient outerInstance, long previousTxId, Neo4Net.causalclustering.catchup.tx.TxPullResponseListener txPullResponseListener )
			 {
				 this.outerInstance = outerInstance;
				 this._previousTxId = previousTxId;
				 this._txPullResponseListener = txPullResponseListener;
				 lastTxIdReceived = previousTxId;
			 }

			 private long lastTxIdReceived;

			 public override void onTxPullResponse( CompletableFuture<TxPullRequestResult> signal, TxPullResponse response )
			 {
				  this.lastTxIdReceived = response.Tx().CommitEntry.TxId;
				  _txPullResponseListener.onTxReceived( response );
			 }

			 public override void onTxStreamFinishedResponse( CompletableFuture<TxPullRequestResult> signal, TxStreamFinishedResponse response )
			 {
				  signal.complete( new TxPullRequestResult( response.Status(), lastTxIdReceived ) );
			 }
		 }
	}

}