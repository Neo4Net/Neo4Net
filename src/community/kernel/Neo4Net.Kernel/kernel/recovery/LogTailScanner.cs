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

	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using LogEntryCursor = Neo4Net.Kernel.impl.transaction.log.LogEntryCursor;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using PhysicalLogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using ReadAheadLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using LogEntryVersion = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.LogVersionRepository_Fields.INITIAL_LOG_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.recovery.Recovery.throwUnableToCleanRecover;

	/// <summary>
	/// This class collects information about the latest entries in the transaction log. Since the only way we have to collect
	/// said information is to scan the transaction log from beginning to end, which is costly, we do this once and save the
	/// result for others to consume.
	/// <para>
	/// Due to the nature of transaction logs and log rotation, a single transaction log file has to be scanned forward, and
	/// if the required data is not found we search backwards through log file versions.
	/// </para>
	/// </summary>
	public class LogTailScanner
	{
		 internal static long NoTransactionId = -1;
		 private readonly LogFiles _logFiles;
		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _logEntryReader;
		 private LogTailInformation _logTailInformation;
		 private readonly LogTailScannerMonitor _monitor;
		 private readonly bool _failOnCorruptedLogFiles;

		 public LogTailScanner( LogFiles logFiles, LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader, Monitors monitors ) : this( logFiles, logEntryReader, monitors, false )
		 {
		 }

		 public LogTailScanner( LogFiles logFiles, LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader, Monitors monitors, bool failOnCorruptedLogFiles )
		 {
			  this._logFiles = logFiles;
			  this._logEntryReader = logEntryReader;
			  this._monitor = monitors.NewMonitor( typeof( LogTailScannerMonitor ) );
			  this._failOnCorruptedLogFiles = failOnCorruptedLogFiles;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private LogTailInformation findLogTail() throws java.io.IOException
		 private LogTailInformation FindLogTail()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long highestLogVersion = logFiles.getHighestLogVersion();
			  long highestLogVersion = _logFiles.HighestLogVersion;
			  long version = highestLogVersion;
			  long versionToSearchForCommits = highestLogVersion;
			  LogEntryStart latestStartEntry = null;
			  long oldestStartEntryTransaction = -1;
			  long oldestVersionFound = -1;
			  LogEntryVersion latestLogEntryVersion = null;
			  bool startRecordAfterCheckpoint = false;
			  bool corruptedTransactionLogs = false;

			  while ( version >= _logFiles.LowestLogVersion && version >= INITIAL_LOG_VERSION )
			  {
					oldestVersionFound = version;
					CheckPoint latestCheckPoint = null;
					try
					{
							using ( LogVersionedStoreChannel channel = _logFiles.openForVersion( version ), ReadAheadLogChannel readAheadLogChannel = new ReadAheadLogChannel( channel ), LogEntryCursor cursor = new LogEntryCursor( _logEntryReader, readAheadLogChannel ) )
							{
							 LogEntry entry;
							 long maxEntryReadPosition = 0;
							 while ( cursor.Next() )
							 {
								  entry = cursor.Get();
      
								  // Collect data about latest checkpoint
								  if ( entry is CheckPoint )
								  {
										latestCheckPoint = entry.As();
								  }
								  else if ( entry is LogEntryCommit )
								  {
										if ( oldestStartEntryTransaction == NoTransactionId )
										{
											 oldestStartEntryTransaction = ( ( LogEntryCommit ) entry ).TxId;
										}
								  }
								  else if ( entry is LogEntryStart )
								  {
										LogEntryStart startEntry = entry.As();
										if ( version == versionToSearchForCommits )
										{
											 latestStartEntry = startEntry;
										}
										startRecordAfterCheckpoint = true;
								  }
      
								  // Collect data about latest entry version, only in first log file
								  if ( version == versionToSearchForCommits || latestLogEntryVersion == null )
								  {
										latestLogEntryVersion = entry.Version;
								  }
								  maxEntryReadPosition = readAheadLogChannel.Position();
							 }
							 if ( HasUnreadableBytes( channel, maxEntryReadPosition ) )
							 {
								  corruptedTransactionLogs = true;
							 }
							}
					}
					 catch ( Exception e ) when ( e is Exception || e is ClosedByInterruptException )
					 {
						 // These should not be parsing errors
						 throw e;
					 }
					catch ( Exception t )
					{
						 _monitor.corruptedLogFile( version, t );
						 if ( _failOnCorruptedLogFiles )
						 {
							  throwUnableToCleanRecover( t );
						 }
						 corruptedTransactionLogs = true;
					}

					if ( latestCheckPoint != null )
					{
						 return CheckpointTailInformation( highestLogVersion, latestStartEntry, oldestVersionFound, latestLogEntryVersion, latestCheckPoint, corruptedTransactionLogs );
					}

					version--;

					// if we have found no commits in the latest log, keep searching in the next one
					if ( latestStartEntry == null )
					{
						 versionToSearchForCommits--;
					}
			  }

			  return new LogTailInformation( corruptedTransactionLogs || startRecordAfterCheckpoint, oldestStartEntryTransaction, oldestVersionFound, highestLogVersion, latestLogEntryVersion );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean hasUnreadableBytes(org.Neo4Net.kernel.impl.transaction.log.LogVersionedStoreChannel channel, long maxEntryReadEndPosition) throws java.io.IOException
		 private bool HasUnreadableBytes( LogVersionedStoreChannel channel, long maxEntryReadEndPosition )
		 {
			  return channel.position() > maxEntryReadEndPosition;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected LogTailInformation checkpointTailInformation(long highestLogVersion, org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryStart latestStartEntry, long oldestVersionFound, org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryVersion latestLogEntryVersion, org.Neo4Net.kernel.impl.transaction.log.entry.CheckPoint latestCheckPoint, boolean corruptedTransactionLogs) throws java.io.IOException
		 protected internal virtual LogTailInformation CheckpointTailInformation( long highestLogVersion, LogEntryStart latestStartEntry, long oldestVersionFound, LogEntryVersion latestLogEntryVersion, CheckPoint latestCheckPoint, bool corruptedTransactionLogs )
		 {
			  LogPosition checkPointLogPosition = latestCheckPoint.LogPosition;
			  ExtractedTransactionRecord transactionRecord = ExtractFirstTxIdAfterPosition( checkPointLogPosition, highestLogVersion );
			  long firstTxIdAfterPosition = transactionRecord.Id;
			  bool startRecordAfterCheckpoint = ( firstTxIdAfterPosition != NoTransactionId ) || ( ( latestStartEntry != null ) && ( latestStartEntry.StartPosition.CompareTo( latestCheckPoint.LogPosition ) >= 0 ) );
			  bool corruptedLogs = transactionRecord.Failure || corruptedTransactionLogs;
			  return new LogTailInformation( latestCheckPoint, corruptedLogs || startRecordAfterCheckpoint, firstTxIdAfterPosition, oldestVersionFound, highestLogVersion, latestLogEntryVersion );
		 }

		 /// <summary>
		 /// Extracts txId from first commit entry, when starting reading at the given {@code position}.
		 /// If no commit entry found in the version, the reader will continue into next version(s) up till
		 /// {@code maxLogVersion} until finding one.
		 /// </summary>
		 /// <param name="initialPosition"> <seealso cref="LogPosition"/> to start scan from. </param>
		 /// <param name="maxLogVersion"> max log version to scan. </param>
		 /// <returns> value object that contains first transaction id of closes commit entry to {@code initialPosition},
		 /// or <seealso cref="LogTailInformation.NO_TRANSACTION_ID"/> if not found. And failure flag that will be set to true if
		 /// there was some exception during transaction log processing. </returns>
		 /// <exception cref="IOException"> on channel close I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected ExtractedTransactionRecord extractFirstTxIdAfterPosition(org.Neo4Net.kernel.impl.transaction.log.LogPosition initialPosition, long maxLogVersion) throws java.io.IOException
		 protected internal virtual ExtractedTransactionRecord ExtractFirstTxIdAfterPosition( LogPosition initialPosition, long maxLogVersion )
		 {
			  LogPosition currentPosition = initialPosition;
			  while ( currentPosition.LogVersion <= maxLogVersion )
			  {
					LogVersionedStoreChannel storeChannel = TryOpenStoreChannel( currentPosition );
					if ( storeChannel != null )
					{
						 try
						 {
							  storeChannel.Position( currentPosition.ByteOffset );
							  using ( ReadAheadLogChannel logChannel = new ReadAheadLogChannel( storeChannel ), LogEntryCursor cursor = new LogEntryCursor( _logEntryReader, logChannel ) )
							  {
									while ( cursor.Next() )
									{
										 LogEntry entry = cursor.Get();
										 if ( entry is LogEntryCommit )
										 {
											  return new ExtractedTransactionRecord( ( ( LogEntryCommit ) entry ).TxId );
										 }
									}
							  }
						 }
						 catch ( Exception t )
						 {
							  _monitor.corruptedLogFile( currentPosition.LogVersion, t );
							  return new ExtractedTransactionRecord( true );
						 }
						 finally
						 {
							  storeChannel.close();
						 }
					}

					currentPosition = LogPosition.start( currentPosition.LogVersion + 1 );
			  }
			  return new ExtractedTransactionRecord();
		 }

		 /// <summary>
		 /// Collects information about the tail of the transaction log, i.e. last checkpoint, last entry etc.
		 /// Since this is an expensive task we do it once and reuse the result. This method is thus lazy and the first one
		 /// calling it will take the hit.
		 /// <para>
		 /// This is only intended to be used during startup. If you need to track the state of the tail, that can be done more
		 /// efficiently at runtime, and this method should then only be used to restore said state.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> snapshot of the state of the transaction logs tail at startup. </returns>
		 /// <exception cref="UnderlyingStorageException"> if any errors occurs while parsing the transaction logs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LogTailInformation getTailInformation() throws org.Neo4Net.kernel.impl.store.UnderlyingStorageException
		 public virtual LogTailInformation TailInformation
		 {
			 get
			 {
				  if ( _logTailInformation == null )
				  {
						try
						{
							 _logTailInformation = FindLogTail();
						}
						catch ( IOException e )
						{
							 throw new UnderlyingStorageException( "Error encountered while parsing transaction logs", e );
						}
				  }
   
				  return _logTailInformation;
			 }
		 }

		 private PhysicalLogVersionedStoreChannel TryOpenStoreChannel( LogPosition currentPosition )
		 {
			  try
			  {
					return _logFiles.openForVersion( currentPosition.LogVersion );
			  }
			  catch ( IOException )
			  {
					return null;
			  }
		 }

		 internal class ExtractedTransactionRecord
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long IdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool FailureConflict;

			  internal ExtractedTransactionRecord() : this(NoTransactionId, false)
			  {
			  }

			  internal ExtractedTransactionRecord( long txId ) : this( txId, false )
			  {
			  }

			  internal ExtractedTransactionRecord( bool failure ) : this( NoTransactionId, failure )
			  {
			  }

			  internal ExtractedTransactionRecord( long txId, bool failure )
			  {
					this.IdConflict = txId;
					this.FailureConflict = failure;
			  }

			  public virtual long Id
			  {
				  get
				  {
						return IdConflict;
				  }
			  }

			  public virtual bool Failure
			  {
				  get
				  {
						return FailureConflict;
				  }
			  }
		 }

		 public class LogTailInformation
		 {

			  public readonly CheckPoint LastCheckPoint;
			  public readonly long FirstTxIdAfterLastCheckPoint;
			  public readonly long OldestLogVersionFound;
			  public readonly long CurrentLogVersion;
			  public readonly LogEntryVersion LatestLogEntryVersion;
			  internal readonly bool RecordAfterCheckpoint;

			  public LogTailInformation( bool recordAfterCheckpoint, long firstTxIdAfterLastCheckPoint, long oldestLogVersionFound, long currentLogVersion, LogEntryVersion latestLogEntryVersion ) : this( null, recordAfterCheckpoint, firstTxIdAfterLastCheckPoint, oldestLogVersionFound, currentLogVersion, latestLogEntryVersion )
			  {
			  }

			  internal LogTailInformation( CheckPoint lastCheckPoint, bool recordAfterCheckpoint, long firstTxIdAfterLastCheckPoint, long oldestLogVersionFound, long currentLogVersion, LogEntryVersion latestLogEntryVersion )
			  {
					this.LastCheckPoint = lastCheckPoint;
					this.FirstTxIdAfterLastCheckPoint = firstTxIdAfterLastCheckPoint;
					this.OldestLogVersionFound = oldestLogVersionFound;
					this.CurrentLogVersion = currentLogVersion;
					this.LatestLogEntryVersion = latestLogEntryVersion;
					this.RecordAfterCheckpoint = recordAfterCheckpoint;
			  }

			  public virtual bool CommitsAfterLastCheckpoint()
			  {
					return RecordAfterCheckpoint;
			  }
		 }

	}

}