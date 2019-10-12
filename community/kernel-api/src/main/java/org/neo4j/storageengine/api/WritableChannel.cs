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
namespace Org.Neo4j.Storageengine.Api
{

	/// <summary>
	/// Represents an infinite channel to write primitive data to.
	/// </summary>
	public interface WritableChannel
	{
		 /// <summary>
		 /// Writes a {@code byte} to this channel.
		 /// </summary>
		 /// <param name="value"> byte value. </param>
		 /// <returns> this channel, for fluent usage. </returns>
		 /// <exception cref="IOException"> if I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: WritableChannel put(byte value) throws java.io.IOException;
		 WritableChannel Put( sbyte value );

		 /// <summary>
		 /// Writes a {@code short} to this channel.
		 /// </summary>
		 /// <param name="value"> short value. </param>
		 /// <returns> this channel, for fluent usage. </returns>
		 /// <exception cref="IOException"> if I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: WritableChannel putShort(short value) throws java.io.IOException;
		 WritableChannel PutShort( short value );

		 /// <summary>
		 /// Writes a {@code int} to this channel.
		 /// </summary>
		 /// <param name="value"> int value. </param>
		 /// <returns> this channel, for fluent usage. </returns>
		 /// <exception cref="IOException"> if I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: WritableChannel putInt(int value) throws java.io.IOException;
		 WritableChannel PutInt( int value );

		 /// <summary>
		 /// Writes a {@code long} to this channel.
		 /// </summary>
		 /// <param name="value"> long value. </param>
		 /// <returns> this channel, for fluent usage. </returns>
		 /// <exception cref="IOException"> if I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: WritableChannel putLong(long value) throws java.io.IOException;
		 WritableChannel PutLong( long value );

		 /// <summary>
		 /// Writes a {@code float} to this channel.
		 /// </summary>
		 /// <param name="value"> float value. </param>
		 /// <returns> this channel, for fluent usage. </returns>
		 /// <exception cref="IOException"> if I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: WritableChannel putFloat(float value) throws java.io.IOException;
		 WritableChannel PutFloat( float value );

		 /// <summary>
		 /// Writes a {@code double} to this channel.
		 /// </summary>
		 /// <param name="value"> double value. </param>
		 /// <returns> this channel, for fluent usage. </returns>
		 /// <exception cref="IOException"> if I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: WritableChannel putDouble(double value) throws java.io.IOException;
		 WritableChannel PutDouble( double value );

		 /// <summary>
		 /// Writes a {@code byte[]} to this channel.
		 /// </summary>
		 /// <param name="value"> byte array. </param>
		 /// <param name="length"> number of items of the array to write. </param>
		 /// <returns> this channel, for fluent usage. </returns>
		 /// <exception cref="IOException"> if I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: WritableChannel put(byte[] value, int length) throws java.io.IOException;
		 WritableChannel Put( sbyte[] value, int length );
	}

}