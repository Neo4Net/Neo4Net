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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using Neo4Net.Cursors;
	using TransactionMetadata = Neo4Net.Kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogHeaderVisitor = Neo4Net.Kernel.impl.transaction.log.files.LogHeaderVisitor;
	using ReversedMultiFileTransactionCursor = Neo4Net.Kernel.impl.transaction.log.reverse.ReversedMultiFileTransactionCursor;
	using ReversedTransactionCursorMonitor = Neo4Net.Kernel.impl.transaction.log.reverse.ReversedTransactionCursorMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_CHECKSUM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_COMMIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_START;

	public class PhysicalLogicalTransactionStore : LogicalTransactionStore
	{
		 private static readonly TransactionMetadataCache.TransactionMetadata _metadataForEmptyStore = new TransactionMetadataCache.TransactionMetadata( -1, -1, LogPosition.Start( 0 ), BASE_TX_CHECKSUM, BASE_TX_COMMIT_TIMESTAMP );

		 private readonly LogFile _logFile;
		 private readonly TransactionMetadataCache _transactionMetadataCache;
		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _logEntryReader;
		 private readonly Monitors _monitors;
		 private readonly bool _failOnCorruptedLogFiles;
		 private LogFiles _logFiles;

		 public PhysicalLogicalTransactionStore( LogFiles logFiles, TransactionMetadataCache transactionMetadataCache, LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader, Monitors monitors, bool failOnCorruptedLogFiles )
		 {
			  this._logFiles = logFiles;
			  this._logFile = logFiles.LogFile;
			  this._transactionMetadataCache = transactionMetadataCache;
			  this._logEntryReader = logEntryReader;
			  this._monitors = monitors;
			  this._failOnCorruptedLogFiles = failOnCorruptedLogFiles;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionCursor getTransactions(LogPosition position) throws java.io.IOException
		 public override TransactionCursor GetTransactions( LogPosition position )
		 {
			  return new PhysicalTransactionCursor<>( _logFile.getReader( position ), new VersionAwareLogEntryReader<>() );
		 }

		 public override TransactionCursor GetTransactionsInReverseOrder( LogPosition backToPosition )
		 {
			  return ReversedMultiFileTransactionCursor.fromLogFile( _logFiles, _logFile, backToPosition, _failOnCorruptedLogFiles, _monitors.newMonitor( typeof( ReversedTransactionCursorMonitor ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionCursor getTransactions(final long transactionIdToStartFrom) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override TransactionCursor GetTransactions( long transactionIdToStartFrom )
		 {
			  // look up in position cache
			  try
			  {
					TransactionMetadataCache.TransactionMetadata transactionMetadata = _transactionMetadataCache.getTransactionMetadata( transactionIdToStartFrom );
					if ( transactionMetadata != null )
					{
						 // we're good
						 ReadableLogChannel channel = _logFile.getReader( transactionMetadata.StartPosition );
						 return new PhysicalTransactionCursor<>( channel, _logEntryReader );
					}

					// ask logFiles about the version it may be in
					LogVersionLocator headerVisitor = new LogVersionLocator( transactionIdToStartFrom );
					_logFiles.accept( headerVisitor );

					// ask LogFile
					TransactionPositionLocator transactionPositionLocator = new TransactionPositionLocator( transactionIdToStartFrom, _logEntryReader );
					_logFile.accept( transactionPositionLocator, headerVisitor.LogPosition );
					LogPosition position = transactionPositionLocator.GetAndCacheFoundLogPosition( _transactionMetadataCache );
					return new PhysicalTransactionCursor<>( _logFile.getReader( position ), _logEntryReader );
			  }
			  catch ( FileNotFoundException e )
			  {
					throw new NoSuchTransactionException( transactionIdToStartFrom, "Log position acquired, but couldn't find the log file itself. Perhaps it just recently was " + "deleted? [" + e.Message + "]", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata getMetadataFor(long transactionId) throws java.io.IOException
		 public override TransactionMetadata GetMetadataFor( long transactionId )
		 {
			  if ( transactionId <= BASE_TX_ID )
			  {
					return _metadataForEmptyStore;
			  }

			  TransactionMetadata transactionMetadata = _transactionMetadataCache.getTransactionMetadata( transactionId );
			  if ( transactionMetadata == null )
			  {
					using ( IOCursor<CommittedTransactionRepresentation> cursor = GetTransactions( transactionId ) )
					{
						 while ( cursor.next() )
						 {
							  CommittedTransactionRepresentation tx = cursor.get();
							  LogEntryCommit commitEntry = tx.CommitEntry;
							  long committedTxId = commitEntry.TxId;
							  long timeWritten = commitEntry.TimeWritten;
							  TransactionMetadata metadata = _transactionMetadataCache.cacheTransactionMetadata( committedTxId, tx.StartEntry.StartPosition, tx.StartEntry.MasterId, tx.StartEntry.LocalId, LogEntryStart.checksum( tx.StartEntry ), timeWritten );
							  if ( committedTxId == transactionId )
							  {
									transactionMetadata = metadata;
							  }
						 }
					}
					if ( transactionMetadata == null )
					{
						 throw new NoSuchTransactionException( transactionId );
					}
			  }

			  return transactionMetadata;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean existsOnDisk(long transactionId) throws java.io.IOException
		 public override bool ExistsOnDisk( long transactionId )
		 {
			  return _logFiles.LogFileInformation.transactionExistsOnDisk( transactionId );
		 }

		 public class TransactionPositionLocator : Neo4Net.Kernel.impl.transaction.log.files.LogFile_LogFileVisitor
		 {
			  internal readonly long StartTransactionId;
			  internal readonly LogEntryReader<ReadableClosablePositionAwareChannel> LogEntryReader;
			  internal LogEntryStart StartEntryForFoundTransaction;
			  internal long CommitTimestamp;

			  internal TransactionPositionLocator( long startTransactionId, LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader )
			  {
					this.StartTransactionId = startTransactionId;
					this.LogEntryReader = logEntryReader;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(ReadableClosablePositionAwareChannel channel) throws java.io.IOException
			  public override bool Visit( ReadableClosablePositionAwareChannel channel )
			  {
					LogEntry logEntry;
					LogEntryStart startEntry = null;
					while ( ( logEntry = LogEntryReader.readLogEntry( channel ) ) != null )
					{
						 switch ( logEntry.Type )
						 {
						 case TX_START:
							  startEntry = logEntry.As();
							  break;
						 case TX_COMMIT:
							  LogEntryCommit commit = logEntry.As();
							  if ( commit.TxId == StartTransactionId )
							  {
									StartEntryForFoundTransaction = startEntry;
									CommitTimestamp = commit.TimeWritten;
									return false;
							  }
						 default: // just skip commands
							  break;
						 }
					}
					return true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LogPosition getAndCacheFoundLogPosition(TransactionMetadataCache transactionMetadataCache) throws NoSuchTransactionException
			  public virtual LogPosition GetAndCacheFoundLogPosition( TransactionMetadataCache transactionMetadataCache )
			  {
					if ( StartEntryForFoundTransaction == null )
					{
						 throw new NoSuchTransactionException( StartTransactionId );
					}
					transactionMetadataCache.CacheTransactionMetadata( StartTransactionId, StartEntryForFoundTransaction.StartPosition, StartEntryForFoundTransaction.MasterId, StartEntryForFoundTransaction.LocalId, LogEntryStart.checksum( StartEntryForFoundTransaction ), CommitTimestamp );
					return StartEntryForFoundTransaction.StartPosition;
			  }
		 }

		 public sealed class LogVersionLocator : LogHeaderVisitor
		 {
			  internal readonly long TransactionId;
			  internal LogPosition FoundPosition;

			  public LogVersionLocator( long transactionId )
			  {
					this.TransactionId = transactionId;
			  }

			  public override bool Visit( LogPosition position, long firstTransactionIdInLog, long lastTransactionIdInLog )
			  {
					bool foundIt = TransactionId >= firstTransactionIdInLog && TransactionId <= lastTransactionIdInLog;
					if ( foundIt )
					{
						 FoundPosition = position;
					}
					return !foundIt; // continue as long we don't find it
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LogPosition getLogPosition() throws NoSuchTransactionException
			  public LogPosition LogPosition
			  {
				  get
				  {
						if ( FoundPosition == null )
						{
							 throw new NoSuchTransactionException( TransactionId, "Couldn't find any log containing " + TransactionId );
						}
						return FoundPosition;
				  }
			  }
		 }
	}

}