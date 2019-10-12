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
namespace Neo4Net.Bolt.v1.packstream
{

	/// <summary>
	/// This is where <seealso cref="PackStream"/> writes its output to.
	/// </summary>
	public interface PackOutput : System.IDisposable
	{
		 /// <summary>
		 /// Prepare this output to write a message. Later successful message should be signaled by <seealso cref="messageSucceeded()"/>
		 /// and failed message by <seealso cref="messageFailed()"/>;
		 /// </summary>
		 void BeginMessage();

		 /// <summary>
		 /// Finalize previously started message.
		 /// </summary>
		 /// <exception cref="IOException"> when message can't be written to the network channel. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void messageSucceeded() throws java.io.IOException;
		 void MessageSucceeded();

		 /// <summary>
		 /// Discard previously started message.
		 /// </summary>
		 /// <exception cref="IOException"> when message can't be written to the network channel. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void messageFailed() throws java.io.IOException;
		 void MessageFailed();

		 /// <summary>
		 /// If implementation has been buffering data, it should flush those buffers now. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackOutput flush() throws java.io.IOException;
		 PackOutput Flush();

		 /// <summary>
		 /// Produce a single byte </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackOutput writeByte(byte value) throws java.io.IOException;
		 PackOutput WriteByte( sbyte value );

		 /// <summary>
		 /// Produce binary data </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackOutput writeBytes(ByteBuffer data) throws java.io.IOException;
		 PackOutput WriteBytes( ByteBuffer data );

		 /// <summary>
		 /// Produce binary data </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackOutput writeBytes(byte[] data, int offset, int amountToWrite) throws java.io.IOException;
		 PackOutput WriteBytes( sbyte[] data, int offset, int amountToWrite );

		 /// <summary>
		 /// Produce a 4-byte signed integer </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackOutput writeShort(short value) throws java.io.IOException;
		 PackOutput WriteShort( short value );

		 /// <summary>
		 /// Produce a 4-byte signed integer </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackOutput writeInt(int value) throws java.io.IOException;
		 PackOutput WriteInt( int value );

		 /// <summary>
		 /// Produce an 8-byte signed integer </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackOutput writeLong(long value) throws java.io.IOException;
		 PackOutput WriteLong( long value );

		 /// <summary>
		 /// Produce an 8-byte IEEE 754 "double format" floating-point number </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PackOutput writeDouble(double value) throws java.io.IOException;
		 PackOutput WriteDouble( double value );
	}

}