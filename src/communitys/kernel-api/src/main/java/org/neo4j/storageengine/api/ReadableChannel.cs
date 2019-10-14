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
namespace Neo4Net.Storageengine.Api
{

	/// <summary>
	/// Represents a channel from where primitive values can be read. Mirrors <seealso cref="WritableChannel"/> in
	/// data types that can be read.
	/// </summary>
	public interface ReadableChannel : System.IDisposable
	{
		 /// <returns> the next {@code byte} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: byte get() throws java.io.IOException;
		 sbyte Get();

		 /// <returns> the next {@code short} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: short getShort() throws java.io.IOException;
		 short Short { get; }

		 /// <returns> the next {@code int} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int getInt() throws java.io.IOException;
		 int Int { get; }

		 /// <returns> the next {@code long} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getLong() throws java.io.IOException;
		 long Long { get; }

		 /// <returns> the next {@code float} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: float getFloat() throws java.io.IOException;
		 float Float { get; }

		 /// <returns> the next {@code double} in this channel. </returns>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: double getDouble() throws java.io.IOException;
		 double Double { get; }

		 /// <summary>
		 /// Reads the next {@code length} bytes from this channel and puts them into {@code bytes}.
		 /// Will throw <seealso cref="System.IndexOutOfRangeException"/> if {@code length} exceeds the length of {@code bytes}.
		 /// </summary>
		 /// <param name="bytes"> {@code byte[]} to put read bytes into. </param>
		 /// <param name="length"> number of bytes to read from the channel. </param>
		 /// <exception cref="IOException"> I/O error from channel. </exception>
		 /// <exception cref="ReadPastEndException"> if not enough data was available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void get(byte[] bytes, int length) throws java.io.IOException;
		 void Get( sbyte[] bytes, int length );
	}

}