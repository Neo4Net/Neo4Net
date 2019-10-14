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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{

	using CommandReader = Neo4Net.Storageengine.Api.CommandReader;
	using CommandReaderFactory = Neo4Net.Storageengine.Api.CommandReaderFactory;

	/// <summary>
	/// Reads and parses the next <seealso cref="LogEntry"/> from <seealso cref="ReadableClosableChannel"/>, given the <seealso cref="LogEntryVersion"/>.
	/// </summary>
	/// @param <T> Specific type of <seealso cref="LogEntry"/> returned from
	/// <seealso cref="parse(LogEntryVersion, ReadableClosableChannel, LogPositionMarker, CommandReaderFactory)"/>. </param>
	public interface LogEntryParser<T> where T : LogEntry
	{
		 /// <summary>
		 /// Parses the next <seealso cref="LogEntry"/> read from the {@code channel}.
		 /// </summary>
		 /// <param name="version"> <seealso cref="LogEntryVersion"/> this log entry is determined to be of. </param>
		 /// <param name="channel"> <seealso cref="ReadableClosableChannel"/> to read the data from. </param>
		 /// <param name="marker"> <seealso cref="LogPositionMarker"/> marking the position in the {@code channel} that is the
		 /// start of this entry. </param>
		 /// <param name="commandReaderFactory"> <seealso cref="CommandReaderFactory"/> for retrieving a <seealso cref="CommandReader"/>
		 /// for reading commands from, for log entry types that need that. </param>
		 /// <returns> the next <seealso cref="LogEntry"/> read and parsed from the {@code channel}. </returns>
		 /// <exception cref="IOException"> I/O error from channel or if data was read past the end of the channel. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T parse(LogEntryVersion version, org.neo4j.kernel.impl.transaction.log.ReadableClosableChannel channel, org.neo4j.kernel.impl.transaction.log.LogPositionMarker marker, org.neo4j.storageengine.api.CommandReaderFactory commandReaderFactory) throws java.io.IOException;
		 T Parse( LogEntryVersion version, ReadableClosableChannel channel, LogPositionMarker marker, CommandReaderFactory commandReaderFactory );

		 /// <returns> code representing the type of log entry. </returns>
		 sbyte ByteCode();

		 /// <returns> whether or not entries parsed by this parser should be skipped, like an empty entry. </returns>
		 bool Skip();
	}

}