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

	using Neo4Net.Cursors;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using Neo4Net.Kernel.impl.transaction.log.entry;

	/// <summary>
	/// <seealso cref="IOCursor"/> abstraction on top of a <seealso cref="LogEntryReader"/>
	/// </summary>
	public class LogEntryCursor : IOCursor<LogEntry>
	{
		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _logEntryReader;
		 private readonly ReadableClosablePositionAwareChannel _channel;
		 private readonly LogPositionMarker _position = new LogPositionMarker();
		 private LogEntry _entry;

		 public LogEntryCursor( LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader, ReadableClosablePositionAwareChannel channel )
		 {
			  this._logEntryReader = logEntryReader;
			  this._channel = channel;
		 }

		 public override LogEntry Get()
		 {
			  return _entry;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  _entry = _logEntryReader.readLogEntry( _channel );

			  return _entry != null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _channel.Dispose();
		 }

		 /// <summary>
		 /// Reading <seealso cref="LogEntry log entries"/> may have the source move over physically multiple log files.
		 /// This accessor returns the log version of the most recent call to <seealso cref="next()"/>.
		 /// </summary>
		 /// <returns> the log version of the most recent <seealso cref="LogEntry"/> returned from {@link #next(). </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getCurrentLogVersion() throws java.io.IOException
		 public virtual long CurrentLogVersion
		 {
			 get
			 {
				  _channel.getCurrentPosition( _position );
				  return _position.LogVersion;
			 }
		 }
	}

}