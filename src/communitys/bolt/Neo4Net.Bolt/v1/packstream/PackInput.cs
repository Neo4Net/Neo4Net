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
namespace Neo4Net.Bolt.v1.packstream
{

	/// <summary>
	/// This is what <seealso cref="PackStream"/> uses to ingest data, implement this on top of any data source of your choice to
	/// deserialize the stream with <seealso cref="PackStream"/>.
	/// </summary>
	public interface PackInput
	{
		 /// <summary>
		 /// Consume one byte </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: byte readByte() throws java.io.IOException;
		 sbyte ReadByte();

		 /// <summary>
		 /// Consume a 2-byte signed integer </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: short readShort() throws java.io.IOException;
		 short ReadShort();

		 /// <summary>
		 /// Consume a 4-byte signed integer </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int readInt() throws java.io.IOException;
		 int ReadInt();

		 /// <summary>
		 /// Consume an 8-byte signed integer </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long readLong() throws java.io.IOException;
		 long ReadLong();

		 /// <summary>
		 /// Consume an 8-byte IEEE 754 "double format" floating-point number </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: double readDouble() throws java.io.IOException;
		 double ReadDouble();

		 /// <summary>
		 /// Consume a specified number of bytes </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackInput readBytes(byte[] into, int offset, int toRead) throws java.io.IOException;
		 PackInput ReadBytes( sbyte[] into, int offset, int toRead );

		 /// <summary>
		 /// Get the next byte without forwarding the internal pointer </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: byte peekByte() throws java.io.IOException;
		 sbyte PeekByte();
	}

}