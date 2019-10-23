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
using System;

namespace Neo4Net.Kernel.Api.StorageEngine
{

	/// <summary>
	/// Represents a channel from where primitive values can be read. Mirrors <seealso cref="WritableChannel"/> in
	/// data types that can be read.
	/// </summary>
	public interface IReadableChannel : IDisposable
	{
		 /// <returns> the next {@code byte} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
		 sbyte Get();

		 /// <returns> the next {@code short} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
		 short Short { get; }

		 /// <returns> the next {@code int} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
		 int Int { get; }

		 /// <returns> the next {@code long} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
		 long Long { get; }

		 /// <returns> the next {@code float} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
		 float Float { get; }

		 /// <returns> the next {@code double} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>

		 double Double { get; }

		 /// <summary>
		 /// Reads the next {@code length} bytes from this channel and puts them into {@code bytes}.
		 /// Will throw <seealso cref="System.IndexOutOfRangeException"/> if {@code length} exceeds the length of {@code bytes}.
		 /// </summary>
		 /// <param name="bytes"> {@code byte[]} to put read bytes into. </param>
		 /// <param name="length"> number of bytes to read from the channel. </param>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
		 void Get( sbyte[] bytes, int length );
	}

}