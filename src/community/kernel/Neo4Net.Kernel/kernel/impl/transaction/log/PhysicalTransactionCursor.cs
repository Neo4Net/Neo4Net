using System.Collections.Generic;
using System.Diagnostics;

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

	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

	public class PhysicalTransactionCursor<T> : TransactionCursor where T : ReadableClosablePositionAwareChannel
	{
		 private readonly T _channel;
		 private readonly LogEntryCursor _logEntryCursor;
		 private readonly LogPositionMarker _lastGoodPositionMarker = new LogPositionMarker();

		 private CommittedTransactionRepresentation _current;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PhysicalTransactionCursor(T channel, Neo4Net.kernel.impl.transaction.log.entry.LogEntryReader<T> entryReader) throws java.io.IOException
		 public PhysicalTransactionCursor( T channel, LogEntryReader<T> entryReader )
		 {
			  this._channel = channel;
			  channel.GetCurrentPosition( _lastGoodPositionMarker );
			  this._logEntryCursor = new LogEntryCursor( ( LogEntryReader<ReadableClosablePositionAwareChannel> ) entryReader, channel );
		 }

		 public override CommittedTransactionRepresentation Get()
		 {
			  return _current;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  // Clear the previous deserialized transaction so that it won't have to be kept in heap while deserializing
			  // the next one. Could be problematic if both are really big.
			  _current = null;

			  while ( true )
			  {
					if ( !_logEntryCursor.next() )
					{
						 return false;
					}

					LogEntry entry = _logEntryCursor.get();
					if ( entry is CheckPoint )
					{
						 // this is a good position anyhow
						 _channel.getCurrentPosition( _lastGoodPositionMarker );
						 continue;
					}

					Debug.Assert( entry is LogEntryStart, "Expected Start entry, read " + entry + " instead" );
					LogEntryStart startEntry = entry.As();
					LogEntryCommit commitEntry;

					IList<StorageCommand> entries = new List<StorageCommand>();
					while ( true )
					{
						 if ( !_logEntryCursor.next() )
						 {
							  return false;
						 }

						 entry = _logEntryCursor.get();
						 if ( entry is LogEntryCommit )
						 {
							  commitEntry = entry.As();
							  break;
						 }

						 LogEntryCommand command = entry.As();
						 entries.Add( command.Command );
					}

					PhysicalTransactionRepresentation transaction = new PhysicalTransactionRepresentation( entries );
					transaction.SetHeader( startEntry.AdditionalHeader, startEntry.MasterId, startEntry.LocalId, startEntry.TimeWritten, startEntry.LastCommittedTxWhenTransactionStarted, commitEntry.TimeWritten, -1 );
					_current = new CommittedTransactionRepresentation( startEntry, transaction, commitEntry );
					_channel.getCurrentPosition( _lastGoodPositionMarker );
					return true;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _logEntryCursor.close();
		 }

		 /// <returns> last known good position, which is a <seealso cref="LogPosition"/> after a <seealso cref="CheckPoint"/> or
		 /// a <seealso cref="LogEntryCommit"/>. </returns>
		 public override LogPosition Position()
		 {
			  return _lastGoodPositionMarker.newPosition();
		 }
	}

}