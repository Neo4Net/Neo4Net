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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	/// <summary>
	/// Abstraction of a {@code byte[]} so that different implementations can be plugged in, for example
	/// off-heap, dynamically growing, or other implementations. This interface is slightly different than
	/// <seealso cref="IntArray"/> and <seealso cref="LongArray"/> in that one index in the array isn't necessarily just a byte,
	/// instead item size can be set to any number and the bytes in an index can be read and written as other
	/// number representations like <seealso cref="setInt(long, int, int) int"/> or <seealso cref="setLong(long, int, long) long"/>,
	/// even a special <seealso cref="set6ByteLong(long, int, long) 6B long"/>. More can easily be added on demand.
	/// 
	/// Each index in the array can hold multiple values, each at its own offset (starting from 0 at each index), e.g.
	/// an array could have items holding values <pre>byte, int, short, long</pre>, where:
	/// - the byte would be accessed using offset=0
	/// - the int would be accessed using offset=1
	/// - the short would be accessed using offset=5
	/// - the long would be accessed using offset=7
	/// </summary>
	/// <seealso cref= NumberArrayFactory </seealso>
	public interface ByteArray : NumberArray<ByteArray>
	{
		 /// <summary>
		 /// Gets the data at the given {@code index}. The data is read into the given byte array.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="into"> byte[] to read into. </param>
		 void Get( long index, sbyte[] into );

		 /// <summary>
		 /// Gets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will get a byte at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to get the value from. </param>
		 /// <returns> the byte at the given offset at the given array index. </returns>
		 sbyte GetByte( long index, int offset );

		 /// <summary>
		 /// Gets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will get a short at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to get the value from. </param>
		 /// <returns> the short at the given offset at the given array index. </returns>
		 short GetShort( long index, int offset );

		 /// <summary>
		 /// Gets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will get a int at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to get the value from. </param>
		 /// <returns> the int at the given offset at the given array index. </returns>
		 int GetInt( long index, int offset );

		 /// <summary>
		 /// Gets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will get a 5-byte long at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to get the value from. </param>
		 /// <returns> the 5-byte long at the given offset at the given array index. </returns>
		 long Get5ByteLong( long index, int offset );

		 /// <summary>
		 /// Gets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will get a 6-byte long at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to get the value from. </param>
		 /// <returns> the 6-byte long at the given offset at the given array index. </returns>
		 long Get6ByteLong( long index, int offset );

		 /// <summary>
		 /// Gets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will get a long at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to get the value from. </param>
		 /// <returns> the long at the given offset at the given array index. </returns>
		 long GetLong( long index, int offset );

		 /// <summary>
		 /// Sets the given {@code data} at the given {@code index}, overwriting all the values in it.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="value"> the byte[] to copy into the given offset at the given array index. </param>
		 void Set( long index, sbyte[] value );

		 /// <summary>
		 /// Sets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will set a byte at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to set the value for. </param>
		 /// <param name="value"> the byte value to set at the given offset at the given array index. </param>
		 void SetByte( long index, int offset, sbyte value );

		 /// <summary>
		 /// Sets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will set a short at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to set the value for. </param>
		 /// <param name="value"> the short value to set at the given offset at the given array index. </param>
		 void SetShort( long index, int offset, short value );

		 /// <summary>
		 /// Sets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will set a int at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to set the value for. </param>
		 /// <param name="value"> the int value to set at the given offset at the given array index. </param>
		 void SetInt( long index, int offset, int value );

		 /// <summary>
		 /// Sets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will set a 5-byte long at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to set the value for. </param>
		 /// <param name="value"> the 5-byte long value to set at the given offset at the given array index. </param>
		 void Set5ByteLong( long index, int offset, long value );

		 /// <summary>
		 /// Sets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will set a 6-byte long at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to set the value for. </param>
		 /// <param name="value"> the 6-byte long value to set at the given offset at the given array index. </param>
		 void Set6ByteLong( long index, int offset, long value );

		 /// <summary>
		 /// Sets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will set a long at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to set the value for. </param>
		 /// <param name="value"> the long value to set at the given offset at the given array index. </param>
		 void SetLong( long index, int offset, long value );

		 /// <summary>
		 /// Gets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will get a 3-byte int at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to get the value from. </param>
		 /// <returns> the 3-byte int at the given offset at the given array index. </returns>
		 int Get3ByteInt( long index, int offset );

		 /// <summary>
		 /// Sets a part of an item, at the given {@code index}. An item in this array can consist of
		 /// multiple values. This call will set a 3-byte int at the given {@code offset}.
		 /// </summary>
		 /// <param name="index"> array index to get. </param>
		 /// <param name="offset"> offset into this index to set the value for. </param>
		 /// <param name="value"> the 3-byte int value to set at the given offset at the given array index. </param>
		 void Set3ByteInt( long index, int offset, int value );
	}

}