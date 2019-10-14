using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	public class HeapByteArray : HeapNumberArray<ByteArray>, ByteArray
	{
		 private readonly int _length;
		 private readonly sbyte[] _array;
		 private readonly ByteBuffer _buffer;
		 private readonly sbyte[] _defaultValue;
		 private readonly bool _defaultValueIsUniform;

		 public HeapByteArray( int length, sbyte[] defaultValue, long @base ) : base( defaultValue.Length, @base )
		 {
			  this._length = length;
			  this._defaultValue = defaultValue;
			  this._array = new sbyte[itemSize * length];
			  this._buffer = ByteBuffer.wrap( _array );
			  this._defaultValueIsUniform = IsUniform( defaultValue );
			  Clear();
		 }

		 public override long Length()
		 {
			  return _length;
		 }

		 public override void Swap( long fromIndex, long toIndex )
		 {
			  // Byte-wise swap
			  for ( int i = 0; i < itemSize; i++ )
			  {
					int fromOffset = Index( fromIndex, i );
					int toOffset = Index( toIndex, i );
					sbyte intermediary = _array[fromOffset];
					_array[fromOffset] = _array[toOffset];
					_array[toOffset] = intermediary;
			  }
		 }

		 public override void Clear()
		 {
			  if ( _defaultValueIsUniform )
			  {
					Arrays.fill( _array, _defaultValue[0] );
			  }
			  else
			  {
					for ( int i = 0; i < _length; i++ )
					{
						 Array.Copy( _defaultValue, 0, _array, i * itemSize, itemSize );
					}
			  }
		 }

		 private static bool IsUniform( sbyte[] value )
		 {
			  sbyte reference = value[0];
			  for ( int i = 1; i < value.Length; i++ )
			  {
					if ( reference != value[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public override void Get( long index, sbyte[] into )
		 {
			  Array.Copy( _array, index( index, 0 ), into, 0, itemSize );
		 }

		 public override sbyte GetByte( long index, int offset )
		 {
			  return _buffer.get( index( index, offset ) );
		 }

		 public override short GetShort( long index, int offset )
		 {
			  return _buffer.getShort( index( index, offset ) );
		 }

		 public override int GetInt( long index, int offset )
		 {
			  return _buffer.getInt( index( index, offset ) );
		 }

		 public override int Get3ByteInt( long index, int offset )
		 {
			  int address = index( index, offset );
			  return Get3ByteIntFromByteBuffer( _buffer, address );
		 }

		 public override long Get5ByteLong( long index, int offset )
		 {
			  return Get5BLongFromByteBuffer( _buffer, index( index, offset ) );
		 }

		 public override long Get6ByteLong( long index, int offset )
		 {
			  return Get6BLongFromByteBuffer( _buffer, index( index, offset ) );
		 }

		 protected internal static int Get3ByteIntFromByteBuffer( ByteBuffer buffer, int address )
		 {
			  int lowWord = buffer.getShort( address ) & 0xFFFF;
			  int highByte = buffer.get( address + Short.BYTES ) & 0xFF;
			  int result = lowWord | ( highByte << ( sizeof( short ) * 8 ) );
			  return result == 0xFFFFFF ? -1 : result;
		 }

		 protected internal static long Get5BLongFromByteBuffer( ByteBuffer buffer, int startOffset )
		 {
			  long low4b = buffer.getInt( startOffset ) & 0xFFFFFFFFL;
			  long high1b = buffer.get( startOffset + Integer.BYTES ) & 0xFF;
			  long result = low4b | ( high1b << 32 );
			  return result == 0xFFFFFFFFFFL ? -1 : result;
		 }

		 protected internal static long Get6BLongFromByteBuffer( ByteBuffer buffer, int startOffset )
		 {
			  long low4b = buffer.getInt( startOffset ) & 0xFFFFFFFFL;
			  long high2b = buffer.getShort( startOffset + Integer.BYTES ) & 0xFFFF;
			  long result = low4b | ( high2b << 32 );
			  return result == 0xFFFFFFFFFFFFL ? -1 : result;
		 }

		 public override long GetLong( long index, int offset )
		 {
			  return _buffer.getLong( index( index, offset ) );
		 }

		 public override void Set( long index, sbyte[] value )
		 {
			  Array.Copy( value, 0, _array, index( index, 0 ), itemSize );
		 }

		 public override void SetByte( long index, int offset, sbyte value )
		 {
			  _buffer.put( index( index, offset ), value );
		 }

		 public override void SetShort( long index, int offset, short value )
		 {
			  _buffer.putShort( index( index, offset ), value );
		 }

		 public override void SetInt( long index, int offset, int value )
		 {
			  _buffer.putInt( index( index, offset ), value );
		 }

		 public override void Set5ByteLong( long index, int offset, long value )
		 {
			  int absIndex = index( index, offset );
			  _buffer.putInt( absIndex, ( int ) value );
			  _buffer.put( absIndex + Integer.BYTES, ( sbyte )( ( long )( ( ulong )value >> 32 ) ) );
		 }

		 public override void Set6ByteLong( long index, int offset, long value )
		 {
			  int absIndex = index( index, offset );
			  _buffer.putInt( absIndex, ( int ) value );
			  _buffer.putShort( absIndex + Integer.BYTES, ( short )( ( long )( ( ulong )value >> 32 ) ) );
		 }

		 public override void SetLong( long index, int offset, long value )
		 {
			  _buffer.putLong( index( index, offset ), value );
		 }

		 public override void Set3ByteInt( long index, int offset, int value )
		 {
			  int address = index( index, offset );
			  _buffer.putShort( address, ( short ) value );
			  _buffer.put( address + Short.BYTES, ( sbyte )( ( int )( ( uint )value >> ( sizeof( short ) * 8 ) ) ) );
		 }

		 private int Index( long index, int offset )
		 {
			  return toIntExact( ( rebase( index ) * itemSize ) + offset );
		 }
	}

}