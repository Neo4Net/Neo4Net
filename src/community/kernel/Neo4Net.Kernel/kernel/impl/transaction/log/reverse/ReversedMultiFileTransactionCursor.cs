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
namespace Neo4Net.Kernel.impl.transaction.log.reverse
{

	using Neo4Net.Functions;
	using Neo4Net.Kernel.impl.transaction.log;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.LogPosition.start;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.LogVersionBridge_Fields.NO_MORE_CHANNELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.reverse.EagerlyReversedTransactionCursor.eagerlyReverse;

	/// <summary>
	/// Similar to <seealso cref="PhysicalTransactionCursor"/> and actually uses it internally. This main difference is that transactions
	/// are returned in reverse order, starting from the end and back towards (and including) a specified <seealso cref="LogPosition"/>.
	/// 
	/// Since the transaction log format lacks data which would allow for a memory efficient reverse reading implementation,
	/// this implementation tries to minimize peak memory consumption by efficiently reading a single log version at a time
	/// in reverse order before moving over to the previous version. Peak memory consumption compared to normal
	/// <seealso cref="PhysicalTransactionCursor"/> should be negligible due to the offset mapping that <seealso cref="ReversedSingleFileTransactionCursor"/>
	/// does.
	/// </summary>
	/// <seealso cref= ReversedSingleFileTransactionCursor </seealso>
	public class ReversedMultiFileTransactionCursor : TransactionCursor
	{
		 private readonly LogPosition _backToPosition;
		 private readonly ThrowingFunction<LogPosition, TransactionCursor, IOException> _cursorFactory;

		 private long _currentVersion;
		 private TransactionCursor _currentLogTransactionCursor;

		 /// <summary>
		 /// Utility method for creating a <seealso cref="ReversedMultiFileTransactionCursor"/> with a <seealso cref="LogFile"/> as the source of
		 /// <seealso cref="TransactionCursor"/> for each log version.
		 /// </summary>
		 /// <param name="logFile"> <seealso cref="LogFile"/> to supply log entries forming transactions. </param>
		 /// <param name="backToPosition"> <seealso cref="LogPosition"/> to read backwards to. </param>
		 /// <param name="failOnCorruptedLogFiles"> fail reading from log files as soon as first error is encountered </param>
		 /// <param name="monitor"> reverse transaction cursor monitor </param>
		 /// <returns> a <seealso cref="TransactionCursor"/> which returns transactions from the end of the log stream and backwards to
		 /// and including transaction starting at <seealso cref="LogPosition"/>. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
		 public static TransactionCursor FromLogFile( LogFiles logFiles, LogFile logFile, LogPosition backToPosition, bool failOnCorruptedLogFiles, ReversedTransactionCursorMonitor monitor )
		 {
			  long highestVersion = logFiles.HighestLogVersion;
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  ThrowingFunction<LogPosition, TransactionCursor, IOException> factory = position =>
			  {
				ReadableLogChannel channel = logFile.GetReader( position, NO_MORE_CHANNELS );
				if ( channel is ReadAheadLogChannel )
				{
					 // This is a channel which can be positioned explicitly and is the typical case for such channels
					 // Let's take advantage of this fact and use a bit smarter reverse implementation
					 return new ReversedSingleFileTransactionCursor( ( ReadAheadLogChannel ) channel, logEntryReader, failOnCorruptedLogFiles, monitor );
				}

				// Fall back to simply eagerly reading each single log file and reversing in memory
				return eagerlyReverse( new PhysicalTransactionCursor<>( channel, logEntryReader ) );
			  };
			  return new ReversedMultiFileTransactionCursor( factory, highestVersion, backToPosition );
		 }

		 /// <param name="cursorFactory"> creates <seealso cref="TransactionCursor"/> from a given <seealso cref="LogPosition"/>. The returned cursor must
		 /// return transactions from the end of that <seealso cref="LogPosition.getLogVersion() log version"/> and backwards in reverse order
		 /// to, and including, the transaction at the <seealso cref="LogPosition"/> given to it. </param>
		 /// <param name="highestVersion"> highest log version right now. </param>
		 /// <param name="backToPosition"> the start position of the last transaction to return from this cursor. </param>
		 internal ReversedMultiFileTransactionCursor( ThrowingFunction<LogPosition, TransactionCursor, IOException> cursorFactory, long highestVersion, LogPosition backToPosition )
		 {
			  this._cursorFactory = cursorFactory;
			  this._backToPosition = backToPosition;
			  this._currentVersion = highestVersion + 1;
		 }

		 public override CommittedTransactionRepresentation Get()
		 {
			  return _currentLogTransactionCursor.get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  while ( _currentLogTransactionCursor == null || !_currentLogTransactionCursor.next() )
			  {
					// We've run out of transactions in this log version, back up to a previous one
					_currentVersion--;
					if ( _currentVersion < _backToPosition.LogVersion )
					{
						 return false;
					}

					CloseCurrent();
					LogPosition position = _currentVersion > _backToPosition.LogVersion ? start( _currentVersion ) : _backToPosition;
					_currentLogTransactionCursor = _cursorFactory.apply( position );
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  CloseCurrent();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeCurrent() throws java.io.IOException
		 private void CloseCurrent()
		 {
			  if ( _currentLogTransactionCursor != null )
			  {
					_currentLogTransactionCursor.close();
					_currentLogTransactionCursor = null;
			  }
		 }

		 public override LogPosition Position()
		 {
			  return _currentLogTransactionCursor.position();
		 }
	}

}