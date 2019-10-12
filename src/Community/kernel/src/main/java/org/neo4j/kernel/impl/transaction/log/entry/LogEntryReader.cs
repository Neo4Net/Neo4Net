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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{


	/// <summary>
	/// Reads <seealso cref="LogEntry"/> instances from a <seealso cref="ReadableClosableChannel source"/>. Instances are expected to be
	/// immutable and handle concurrent calls from multiple threads.
	/// </summary>
	/// @param <S> source to read bytes from. </param>
	public interface LogEntryReader<S> where S : Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel
	{
		 /// <summary>
		 /// Reads the next <seealso cref="LogEntry"/> from the given source.
		 /// </summary>
		 /// <param name="source"> <seealso cref="ReadableClosableChannel"/> to read from. </param>
		 /// <returns> the read <seealso cref="LogEntry"/> or {@code null} if there were no more complete entries in the given source. </returns>
		 /// <exception cref="IOException"> if source throws exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: LogEntry readLogEntry(S source) throws java.io.IOException;
		 LogEntry ReadLogEntry( S source );
	}

}