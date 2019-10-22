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
namespace Neo4Net.Kernel.impl.transaction.log.files
{

	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class TransactionLogFileInformation : LogFileInformation
	{
		 private readonly LogFiles _logFiles;
		 private readonly LogHeaderCache _logHeaderCache;
		 private readonly TransactionLogFileTimestampMapper _logFileTimestampMapper;
		 private readonly TransactionLogFilesContext _logFileContext;

		 internal TransactionLogFileInformation( LogFiles logFiles, LogHeaderCache logHeaderCache, TransactionLogFilesContext context )
		 {
			  this._logFiles = logFiles;
			  this._logHeaderCache = logHeaderCache;
			  this._logFileContext = context;
			  this._logFileTimestampMapper = new TransactionLogFileTimestampMapper( logFiles, context.LogEntryReader );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getFirstExistingEntryId() throws java.io.IOException
		 public virtual long FirstExistingEntryId
		 {
			 get
			 {
				  long version = _logFiles.HighestLogVersion;
				  long candidateFirstTx = -1;
				  while ( _logFiles.versionExists( version ) )
				  {
						candidateFirstTx = GetFirstEntryId( version );
						version--;
				  }
				  version++; // the loop above goes back one version too far.
   
				  // OK, so we now have the oldest existing log version here. Open it and see if there's any transaction
				  // in there. If there is then that transaction is the first one that we have.
				  return _logFiles.hasAnyEntries( version ) ? candidateFirstTx : -1;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getFirstEntryId(long version) throws java.io.IOException
		 public override long GetFirstEntryId( long version )
		 {
			  long? logHeader = _logHeaderCache.getLogHeader( version );
			  if ( logHeader != null )
			  { // It existed in cache
					return logHeader + 1;
			  }

			  // Wasn't cached, go look for it
			  if ( _logFiles.versionExists( version ) )
			  {
					long previousVersionLastCommittedTx = _logFiles.extractHeader( version ).lastCommittedTxId;
					_logHeaderCache.putHeader( version, previousVersionLastCommittedTx );
					return previousVersionLastCommittedTx + 1;
			  }
			  return -1;
		 }

		 public virtual long LastEntryId
		 {
			 get
			 {
				  return _logFileContext.LastCommittedTransactionId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getFirstStartRecordTimestamp(long version) throws java.io.IOException
		 public override long GetFirstStartRecordTimestamp( long version )
		 {
			  return _logFileTimestampMapper.getTimestampForVersion( version );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean transactionExistsOnDisk(long transactionId) throws java.io.IOException
		 public override bool TransactionExistsOnDisk( long transactionId )
		 {
			  long lowestOnDisk = FirstExistingEntryId;
			  long highestOnDisk = LastEntryId;
			  return ( transactionId >= BASE_TX_ID ) && ( transactionId >= lowestOnDisk && transactionId <= highestOnDisk ); // and it's in the on-disk range
		 }

		 private class TransactionLogFileTimestampMapper
		 {

			  internal readonly LogFiles LogFiles;
			  internal readonly LogEntryReader<ReadableLogChannel> LogEntryReader;

			  internal TransactionLogFileTimestampMapper( LogFiles logFiles, LogEntryReader<ReadableLogChannel> logEntryReader )
			  {
					this.LogFiles = logFiles;
					this.LogEntryReader = logEntryReader;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getTimestampForVersion(long version) throws java.io.IOException
			  internal virtual long GetTimestampForVersion( long version )
			  {
					LogPosition position = LogPosition.start( version );
					using ( ReadableLogChannel channel = LogFiles.LogFile.getReader( position ) )
					{
						 LogEntry entry;
						 while ( ( entry = LogEntryReader.readLogEntry( channel ) ) != null )
						 {
							  if ( entry is LogEntryStart )
							  {
									return entry.As<LogEntryStart>().TimeWritten;
							  }
						 }
					}
					return -1;
			  }
		 }
	}

}