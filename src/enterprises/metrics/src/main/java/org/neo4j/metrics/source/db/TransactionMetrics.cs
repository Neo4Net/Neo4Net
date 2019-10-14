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
namespace Neo4Net.metrics.source.db
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using TransactionCounters = Neo4Net.Kernel.impl.transaction.stats.TransactionCounters;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Database transaction metrics")]
	public class TransactionMetrics : LifecycleAdapter
	{
		 private const string TRANSACTION_PREFIX = "neo4j.transaction";

		 [Documented("The total number of started transactions")]
		 public static readonly string TxStarted = name( TRANSACTION_PREFIX, "started" );
		 [Documented("The highest peak of concurrent transactions ever seen on this machine")]
		 public static readonly string TxPeakConcurrent = name( TRANSACTION_PREFIX, "peak_concurrent" );

		 [Documented("The number of currently active transactions")]
		 public static readonly string TxActive = name( TRANSACTION_PREFIX, "active" );
		 [Documented("The number of currently active read transactions")]
		 public static readonly string ReadTxActive = name( TRANSACTION_PREFIX, "active_read" );
		 [Documented("The number of currently active write transactions")]
		 public static readonly string WriteTxActive = name( TRANSACTION_PREFIX, "active_write" );

		 [Documented("The total number of committed transactions")]
		 public static readonly string TxCommitted = name( TRANSACTION_PREFIX, "committed" );
		 [Documented("The total number of committed read transactions")]
		 public static readonly string ReadTxCommitted = name( TRANSACTION_PREFIX, "committed_read" );
		 [Documented("The total number of committed write transactions")]
		 public static readonly string WriteTxCommitted = name( TRANSACTION_PREFIX, "committed_write" );

		 [Documented("The total number of rolled back transactions")]
		 public static readonly string TxRollbacks = name( TRANSACTION_PREFIX, "rollbacks" );
		 [Documented("The total number of rolled back read transactions")]
		 public static readonly string ReadTxRollbacks = name( TRANSACTION_PREFIX, "rollbacks_read" );
		 [Documented("The total number of rolled back write transactions")]
		 public static readonly string WriteTxRollbacks = name( TRANSACTION_PREFIX, "rollbacks_write" );

		 [Documented("The total number of terminated transactions")]
		 public static readonly string TxTerminated = name( TRANSACTION_PREFIX, "terminated" );
		 [Documented("The total number of terminated read transactions")]
		 public static readonly string ReadTxTerminated = name( TRANSACTION_PREFIX, "terminated_read" );
		 [Documented("The total number of terminated write transactions")]
		 public static readonly string WriteTxTerminated = name( TRANSACTION_PREFIX, "terminated_write" );

		 [Documented("The ID of the last committed transaction")]
		 public static readonly string LastCommittedTxId = name( TRANSACTION_PREFIX, "last_committed_tx_id" );
		 [Documented("The ID of the last closed transaction")]
		 public static readonly string LastClosedTxId = name( TRANSACTION_PREFIX, "last_closed_tx_id" );

		 private readonly MetricRegistry _registry;
		 private readonly TransactionCounters _transactionCounters;
		 private readonly System.Func<TransactionIdStore> _transactionIdStore;

		 public TransactionMetrics( MetricRegistry registry, System.Func<TransactionIdStore> transactionIdStore, TransactionCounters transactionCounters )
		 {
			  this._registry = registry;
			  this._transactionIdStore = transactionIdStore;
			  this._transactionCounters = transactionCounters;
		 }

		 public override void Start()
		 {
			  _registry.register( TxStarted, ( Gauge<long> ) _transactionCounters.getNumberOfStartedTransactions );
			  _registry.register( TxPeakConcurrent, ( Gauge<long> ) _transactionCounters.getPeakConcurrentNumberOfTransactions );

			  _registry.register( TxActive, ( Gauge<long> ) _transactionCounters.getNumberOfActiveTransactions );
			  _registry.register( ReadTxActive, ( Gauge<long> ) _transactionCounters.getNumberOfActiveReadTransactions );
			  _registry.register( WriteTxActive, ( Gauge<long> ) _transactionCounters.getNumberOfActiveWriteTransactions );

			  _registry.register( TxCommitted, ( Gauge<long> ) _transactionCounters.getNumberOfCommittedTransactions );
			  _registry.register( ReadTxCommitted, ( Gauge<long> ) _transactionCounters.getNumberOfCommittedReadTransactions );
			  _registry.register( WriteTxCommitted, ( Gauge<long> ) _transactionCounters.getNumberOfCommittedWriteTransactions );

			  _registry.register( TxRollbacks, ( Gauge<long> ) _transactionCounters.getNumberOfRolledBackTransactions );
			  _registry.register( ReadTxRollbacks, ( Gauge<long> ) _transactionCounters.getNumberOfRolledBackReadTransactions );
			  _registry.register( WriteTxRollbacks, ( Gauge<long> ) _transactionCounters.getNumberOfRolledBackWriteTransactions );

			  _registry.register( TxTerminated, ( Gauge<long> ) _transactionCounters.getNumberOfTerminatedTransactions );
			  _registry.register( ReadTxTerminated, ( Gauge<long> ) _transactionCounters.getNumberOfTerminatedReadTransactions );
			  _registry.register( WriteTxTerminated, ( Gauge<long> ) _transactionCounters.getNumberOfTerminatedWriteTransactions );

			  _registry.register( LastCommittedTxId, ( Gauge<long> )() => _transactionIdStore.get().LastCommittedTransactionId );
			  _registry.register( LastClosedTxId, ( Gauge<long> )() => _transactionIdStore.get().LastClosedTransactionId );
		 }

		 public override void Stop()
		 {
			  _registry.remove( TxStarted );
			  _registry.remove( TxPeakConcurrent );

			  _registry.remove( TxActive );
			  _registry.remove( ReadTxActive );
			  _registry.remove( WriteTxActive );

			  _registry.remove( TxCommitted );
			  _registry.remove( ReadTxCommitted );
			  _registry.remove( WriteTxCommitted );

			  _registry.remove( TxRollbacks );
			  _registry.remove( ReadTxRollbacks );
			  _registry.remove( WriteTxRollbacks );

			  _registry.remove( TxTerminated );
			  _registry.remove( ReadTxTerminated );
			  _registry.remove( WriteTxTerminated );

			  _registry.remove( LastCommittedTxId );
			  _registry.remove( LastClosedTxId );
		 }
	}

}