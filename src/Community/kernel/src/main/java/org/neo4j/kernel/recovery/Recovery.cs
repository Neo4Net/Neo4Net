using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.recovery
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using TransactionCursor = Neo4Net.Kernel.impl.transaction.log.TransactionCursor;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.RECOVERY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.REVERSE_RECOVERY;

	/// <summary>
	/// This is the process of doing a recovery on the transaction log and store, and is executed
	/// at startup of <seealso cref="org.neo4j.kernel.NeoStoreDataSource"/>.
	/// </summary>
	public class Recovery : LifecycleAdapter
	{

		 private readonly RecoveryService _recoveryService;
		 private readonly RecoveryMonitor _monitor;
		 private readonly CorruptedLogsTruncator _logsTruncator;
		 private readonly Lifecycle _schemaLife;
		 private readonly ProgressReporter _progressReporter;
		 private readonly bool _failOnCorruptedLogFiles;
		 private int _numberOfRecoveredTransactions;

		 public Recovery( RecoveryService recoveryService, CorruptedLogsTruncator logsTruncator, Lifecycle schemaLife, RecoveryMonitor monitor, ProgressReporter progressReporter, bool failOnCorruptedLogFiles )
		 {
			  this._recoveryService = recoveryService;
			  this._monitor = monitor;
			  this._logsTruncator = logsTruncator;
			  this._schemaLife = schemaLife;
			  this._progressReporter = progressReporter;
			  this._failOnCorruptedLogFiles = failOnCorruptedLogFiles;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {
			  RecoveryStartInformation recoveryStartInformation = _recoveryService.RecoveryStartInformation;
			  if ( !recoveryStartInformation.RecoveryRequired )
			  {
					// If there is nothing to recovery, then the schema is initialised immediately.
					_schemaLife.init();
					return;
			  }

			  LogPosition recoveryPosition = recoveryStartInformation.RecoveryPosition;

			  _monitor.recoveryRequired( recoveryPosition );
			  _recoveryService.startRecovery();

			  LogPosition recoveryToPosition = recoveryPosition;
			  CommittedTransactionRepresentation lastTransaction = null;
			  CommittedTransactionRepresentation lastReversedTransaction = null;
			  try
			  {
					long lowestRecoveredTxId = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
					using ( TransactionCursor transactionsToRecover = _recoveryService.getTransactionsInReverseOrder( recoveryPosition ), RecoveryApplier recoveryVisitor = _recoveryService.getRecoveryApplier( REVERSE_RECOVERY ) )
					{
						 while ( transactionsToRecover.next() )
						 {
							  CommittedTransactionRepresentation transaction = transactionsToRecover.get();
							  if ( lastReversedTransaction == null )
							  {
									lastReversedTransaction = transaction;
									InitProgressReporter( recoveryStartInformation, lastReversedTransaction );
							  }
							  recoveryVisitor.visit( transaction );
							  lowestRecoveredTxId = transaction.CommitEntry.TxId;
							  ReportProgress();
						 }
					}

					_monitor.reverseStoreRecoveryCompleted( lowestRecoveredTxId );

					// We cannot initialise the schema (tokens, schema cache, indexing service, etc.) until we have returned the store to a consistent state.
					// We need to be able to read the store before we can even figure out what indexes, tokens, etc. we have. Hence we defer the initialisation
					// of the schema life until after we've done the reverse recovery.
					_schemaLife.init();

					using ( TransactionCursor transactionsToRecover = _recoveryService.getTransactions( recoveryPosition ), RecoveryApplier recoveryVisitor = _recoveryService.getRecoveryApplier( RECOVERY ) )
					{
						 while ( transactionsToRecover.next() )
						 {
							  lastTransaction = transactionsToRecover.get();
							  long txId = lastTransaction.CommitEntry.TxId;
							  recoveryVisitor.visit( lastTransaction );
							  _monitor.transactionRecovered( txId );
							  _numberOfRecoveredTransactions++;
							  recoveryToPosition = transactionsToRecover.Position();
							  ReportProgress();
						 }
						 recoveryToPosition = transactionsToRecover.Position();
					}
			  }
			  catch ( Exception e ) when ( e is Exception || e is ClosedByInterruptException )
			  {
					// We do not want to truncate logs based on these exceptions. Since users can influence them with config changes
					// the users are able to workaround this if truncations is really needed.
					throw e;
			  }
			  catch ( Exception t )
			  {
					if ( _failOnCorruptedLogFiles )
					{
						 ThrowUnableToCleanRecover( t );
					}
					if ( lastTransaction != null )
					{
						 LogEntryCommit commitEntry = lastTransaction.CommitEntry;
						 _monitor.failToRecoverTransactionsAfterCommit( t, commitEntry, recoveryToPosition );
					}
					else
					{
						 _monitor.failToRecoverTransactionsAfterPosition( t, recoveryPosition );
						 recoveryToPosition = recoveryPosition;
					}
			  }
			  _progressReporter.completed();
			  _logsTruncator.truncate( recoveryToPosition );

			  _recoveryService.transactionsRecovered( lastTransaction, recoveryToPosition );
			  _monitor.recoveryCompleted( _numberOfRecoveredTransactions );
		 }

		 internal static void ThrowUnableToCleanRecover( Exception t )
		 {
			  throw new Exception( "Error reading transaction logs, recovery not possible. To force the database to start anyway, you can specify '" + GraphDatabaseSettings.fail_on_corrupted_log_files.name() + "=false'. This will try to recover as much " + "as possible and then truncate the corrupt part of the transaction log. Doing this means your database " + "integrity might be compromised, please consider restoring from a consistent backup instead.", t );
		 }

		 private void InitProgressReporter( RecoveryStartInformation recoveryStartInformation, CommittedTransactionRepresentation lastReversedTransaction )
		 {
			  long numberOfTransactionToRecover = GetNumberOfTransactionToRecover( recoveryStartInformation, lastReversedTransaction );
			  // since we will process each transaction twice (doing reverse and direct detour) we need to
			  // multiply number of transactions that we want to recover by 2 to be able to report correct progress
			  _progressReporter.start( numberOfTransactionToRecover * 2 );
		 }

		 private void ReportProgress()
		 {
			  _progressReporter.progress( 1 );
		 }

		 private long GetNumberOfTransactionToRecover( RecoveryStartInformation recoveryStartInformation, CommittedTransactionRepresentation lastReversedTransaction )
		 {
			  return lastReversedTransaction.CommitEntry.TxId - recoveryStartInformation.FirstTxIdAfterLastCheckPoint + 1;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  _schemaLife.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _schemaLife.stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  _schemaLife.shutdown();
		 }
	}

}