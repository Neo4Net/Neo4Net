using System.Diagnostics;

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
namespace Org.Neo4j.causalclustering.catchup.tx
{

	using CommandIndexTracker = Org.Neo4j.causalclustering.core.state.machines.id.CommandIndexTracker;
	using LogIndexTxHeaderEncoding = Org.Neo4j.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionQueue = Org.Neo4j.Kernel.Impl.Api.TransactionQueue;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

	/// <summary>
	/// Accepts transactions and queues them up for being applied in batches.
	/// </summary>
	public class BatchingTxApplier : LifecycleAdapter
	{
		 private readonly int _maxBatchSize;
		 private readonly System.Func<TransactionIdStore> _txIdStoreSupplier;
		 private readonly System.Func<TransactionCommitProcess> _commitProcessSupplier;

		 private readonly PullRequestMonitor _monitor;
		 private readonly PageCursorTracerSupplier _pageCursorTracerSupplier;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 private readonly CommandIndexTracker _commandIndexTracker;
		 private readonly Log _log;

		 private TransactionQueue _txQueue;
		 private TransactionCommitProcess _commitProcess;

		 private volatile long _lastQueuedTxId;
		 private volatile bool _stopped;

		 public BatchingTxApplier( int maxBatchSize, System.Func<TransactionIdStore> txIdStoreSupplier, System.Func<TransactionCommitProcess> commitProcessSupplier, Monitors monitors, PageCursorTracerSupplier pageCursorTracerSupplier, VersionContextSupplier versionContextSupplier, CommandIndexTracker commandIndexTracker, LogProvider logProvider )
		 {
			  this._maxBatchSize = maxBatchSize;
			  this._txIdStoreSupplier = txIdStoreSupplier;
			  this._commitProcessSupplier = commitProcessSupplier;
			  this._pageCursorTracerSupplier = pageCursorTracerSupplier;
			  this._log = logProvider.getLog( this.GetType() );
			  this._monitor = monitors.NewMonitor( typeof( PullRequestMonitor ) );
			  this._versionContextSupplier = versionContextSupplier;
			  this._commandIndexTracker = commandIndexTracker;
		 }

		 public override void Start()
		 {
			  _stopped = false;
			  RefreshFromNewStore();
			  _txQueue = new TransactionQueue(_maxBatchSize, (first, last) =>
			  {
				_commitProcess.commit( first, NULL, EXTERNAL );
				_pageCursorTracerSupplier.get().reportEvents(); // Report paging metrics for the commit
				long lastAppliedRaftLogIndex = LogIndexTxHeaderEncoding.decodeLogIndexFromTxHeader( last.transactionRepresentation().additionalHeader() );
				_commandIndexTracker.AppliedCommandIndex = lastAppliedRaftLogIndex;
			  });
		 }

		 public override void Stop()
		 {
			  _stopped = true;
		 }

		 internal virtual void RefreshFromNewStore()
		 {
			  Debug.Assert( _txQueue == null || _txQueue.Empty );
			  _lastQueuedTxId = _txIdStoreSupplier.get().LastCommittedTransactionId;
			  _commitProcess = _commitProcessSupplier.get();
		 }

		 /// <summary>
		 /// Queues a transaction for application.
		 /// </summary>
		 /// <param name="tx"> The transaction to be queued for application. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void queue(org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation tx) throws Exception
		 public virtual void Queue( CommittedTransactionRepresentation tx )
		 {
			  long receivedTxId = tx.CommitEntry.TxId;
			  long expectedTxId = _lastQueuedTxId + 1;

			  if ( receivedTxId != expectedTxId )
			  {
					_log.warn( "Out of order transaction. Received: %d Expected: %d", receivedTxId, expectedTxId );
					return;
			  }

			  _txQueue.queue( new TransactionToApply( tx.TransactionRepresentation, receivedTxId, _versionContextSupplier.VersionContext ) );

			  if ( !_stopped )
			  {
					_lastQueuedTxId = receivedTxId;
					_monitor.txPullResponse( receivedTxId );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void applyBatch() throws Exception
		 internal virtual void ApplyBatch()
		 {
			  _txQueue.empty();
		 }

		 /// <returns> The id of the last transaction applied. </returns>
		 internal virtual long LastQueuedTxId()
		 {
			  return _lastQueuedTxId;
		 }
	}

}