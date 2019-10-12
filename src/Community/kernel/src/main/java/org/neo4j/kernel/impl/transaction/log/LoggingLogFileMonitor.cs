using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogFileCreationMonitor = Neo4Net.Kernel.impl.transaction.log.files.LogFileCreationMonitor;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using RecoveryMonitor = Neo4Net.Kernel.recovery.RecoveryMonitor;
	using RecoveryStartInformationProvider = Neo4Net.Kernel.recovery.RecoveryStartInformationProvider;
	using Log = Neo4Net.Logging.Log;

	public class LoggingLogFileMonitor : LogFileCreationMonitor, Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation_Monitor, RecoveryMonitor, RecoveryStartInformationProvider.Monitor
	{
		 private long _firstTransactionRecovered = -1;
		 private long _lastTransactionRecovered;
		 private readonly Log _log;

		 public LoggingLogFileMonitor( Log log )
		 {
			  this._log = log;
		 }

		 public override void RecoveryRequired( LogPosition startPosition )
		 {
			  _log.info( "Recovery required from position " + startPosition );
		 }

		 public override void RecoveryCompleted( int numberOfRecoveredTransactions )
		 {
			  if ( numberOfRecoveredTransactions != 0 )
			  {
					_log.info( format( "Recovery completed. %d transactions, first:%d, last:%d recovered", numberOfRecoveredTransactions, _firstTransactionRecovered, _lastTransactionRecovered ) );
			  }
			  else
			  {
					_log.info( "No recovery required" );
			  }
		 }

		 public override void FailToRecoverTransactionsAfterCommit( Exception t, LogEntryCommit commitEntry, LogPosition recoveryToPosition )
		 {
			  _log.warn( format( "Fail to recover all transactions. Last recoverable transaction id:%d, committed " + "at:%d. Any later transaction after %s are unreadable and will be truncated.", commitEntry.TxId, commitEntry.TimeWritten, recoveryToPosition ), t );
		 }

		 public override void FailToRecoverTransactionsAfterPosition( Exception t, LogPosition recoveryFromPosition )
		 {
			  _log.warn( format( "Fail to recover all transactions. Any later transactions after position %s are " + "unreadable and will be truncated.", recoveryFromPosition ), t );
		 }

		 public override void StartedRotating( long currentVersion )
		 {
		 }

		 public override void FinishedRotating( long currentVersion )
		 {
		 }

		 public override void TransactionRecovered( long txId )
		 {
			  if ( _firstTransactionRecovered == -1 )
			  {
					_firstTransactionRecovered = txId;
			  }
			  _lastTransactionRecovered = txId;
		 }

		 public override void Created( File logFile, long logVersion, long lastTransactionId )
		 {
			  _log.info( format( "Rotated to transaction log [%s] version=%d, last transaction in previous log=%d", logFile, logVersion, lastTransactionId ) );
		 }

		 public override void NoCommitsAfterLastCheckPoint( LogPosition logPosition )
		 {
			  _log.info( format( "No commits found after last check point (which is at %s)", logPosition != null ? logPosition.ToString() : "<no log position given>" ) );
		 }

		 public override void CommitsAfterLastCheckPoint( LogPosition logPosition, long firstTxIdAfterLastCheckPoint )
		 {
			  _log.info( format( "Commits found after last check point (which is at %s). First txId after last checkpoint: %d ", logPosition, firstTxIdAfterLastCheckPoint ) );
		 }

		 public override void NoCheckPointFound()
		 {
			  _log.info( "No check point found in transaction log" );
		 }
	}

}