using System;
using System.Text;

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
namespace Org.Neo4j.Kernel.impl.util
{

	/// <summary>
	/// Got bits to store, shift and retrieve and they are more than what fits in a long?
	/// Use <seealso cref="Bits"/> then.
	/// </summary>
	public sealed class Bits : ICloneable
	{
		 // 3: ...
		 // 2:   [   23    ][   22    ][   21    ][   20    ][   19    ][   18    ][   17    ][   16    ] <--\
		 //                                                                                                   |
		 //    /---------------------------------------------------------------------------------------------/
		 //   |
		 // 1: \-[   15    ][   14    ][   13    ][   12    ][   11    ][   10    ][    9    ][    8    ] <--\
		 //                                                                                                   |
		 //    /---------------------------------------------------------------------------------------------/
		 //   |
		 // 0: \-[    7    ][    6    ][    5    ][    4    ][    3    ][    2    ][    1    ][    0    ] <---- START
		 private readonly long[] _longs;
		 private readonly int _numberOfBytes;
		 private int _writePosition;
		 private int _readPosition;

		 /*
		  * Calculate all the right overflow masks
		  */
		 private static readonly long[] _rightOverflowMasks;

		 static Bits()
		 {
			  _rightOverflowMasks = new long[( sizeof( long ) * 8 )];
			  long mask = 1L;
			  for ( int i = 0; i < _rightOverflowMasks.Length; i++ )
			  {
					_rightOverflowMasks[i] = mask;
					mask <<= 1;
					mask |= 0x1L;
			  }
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Bits BitsConflict( int numberOfBytes )
		 {
			  int requiredLongs = requiredLongs( numberOfBytes );
			  return new Bits( new long[requiredLongs], numberOfBytes );
		 }

		 public static int RequiredLongs( int numberOfBytes )
		 {
			  return ( ( numberOfBytes - 1 ) >> 3 ) + 1; // /8
		 }

		 public static Bits BitsFromLongs( long[] longs )
		 {
			  return new Bits( longs, longs.Length << 3 ); // *8
		 }

		 public static Bits BitsFromBytes( sbyte[] bytes )
		 {
			  return BitsFromBytes( bytes, 0 );
		 }

		 public static Bits BitsFromBytes( sbyte[] bytes, int startIndex )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int count = bytes.length;
			  int count = bytes.Length;
			  Bits bits = bits( count - startIndex );
			  for ( int i = startIndex; i < count; i++ )
			  {
					bits.Put( bytes[i] );
			  }
			  return bits;
		 }

		 public static Bits BitsFromBytes( sbyte[] bytes, int offset, int length )
		 {
			  Bits bits = bits( length - offset );
			  for ( int i = offset; i < ( offset + length ); i++ )
			  {
					bits.Put( bytes[i] );
			  }
			  return bits;
		 }

		 private Bits( long[] longs, int numberOfBytes )
		 {
			  this._longs = longs;
			  this._numberOfBytes = numberOfBytes;
		 }

		 /// <summary>
		 /// A mask which has the {@code steps} least significant bits set to 1, all others 0.
		 /// It's used to carry bits over between carriers (longs) when shifting right.
		 /// </summary>
		 /// <param name="steps"> the number of least significant bits to have set to 1 in the mask. </param>
		 /// <returns> the created mask. </returns>
		 public static long RightOverflowMask( int steps )
		 {
			  return _rightOverflowMasks[steps - 1];
		 }

		 /// <summary>
		 /// Returns the underlying long values that has got all the bits applied.
		 /// The first item in the array has got the most significant bits.
		 /// </summary>
		 /// <returns> the underlying long values that has got all the bits applied. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EI_EXPOSE_REP") public long[] getLongs()
		 public long[] Longs
		 {
			 get
			 {
				  return _longs;
			 }
		 }

		 public sbyte[] AsBytes()
		 {
			  return AsBytes( 0 );
		 }

		 public sbyte[] AsBytes( int offsetBytes )
		 {
			  int readPositionBefore = _readPosition;
			  _readPosition = 0;
			  try
			  {
					sbyte[] result = new sbyte[_numberOfBytes + offsetBytes];
					for ( int i = 0; i < _numberOfBytes; i++ )
					{
						 result[i + offsetBytes] = Byte;
					}
					return result;
			  }
			  finally
			  {
					_readPosition = readPositionBefore;
			  }
		 }

		 /// <summary>
		 /// A very nice toString, showing each bit, divided into groups of bytes and
		 /// lines of 8 bytes.
		 /// </summary>
		 public override string ToString()
		 {
			  StringBuilder builder = new StringBuilder();
			  for ( int longIndex = _longs.Length - 1; longIndex >= 0; longIndex-- )
			  {
					long value = _longs[longIndex];
					if ( builder.Length > 0 )
					{
						 builder.Append( '\n' );
					}
					builder.Append( longIndex );
					builder.Append( ':' );
					NumberToString( builder, value, 8 );
					if ( longIndex == 0 )
					{
						 builder.Append( " <-- START" );
					}
			  }
			  return builder.ToString();
		 }

		 public static void NumberToString( StringBuilder builder, long value, int numberOfBytes )
		 {
			  builder.Append( '[' );
			  for ( int i = 8 * numberOfBytes - 1; i >= 0; i-- )
			  {
					bool isSet = ( value & ( 1L << i ) ) != 0;
					builder.Append( isSet ? "1" : "0" );
					if ( i > 0 && i % 8 == 0 )
					{
						 builder.Append( ',' );
					}
			  }
			  builder.Append( ']' );
		 }

		 public static string NumbersToBitString( sbyte[] values )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( sbyte value in values )
			  {
					NumberToString( builder, value, 1 );
			  }
			  return builder.ToString();
		 }

		 public static string NumbersToBitString( short[] values )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( short value in values )
			  {
					NumberToString( builder, value, 2 );
			  }
			  return builder.ToString();
		 }

		 public static string NumbersToBitString( int[] values )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( int value in values )
			  {
					NumberToString( builder, value, 4 );
			  }
			  return builder.ToString();
		 }

		 public static string NumbersToBitString( long[] values )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( long value in values )
			  {
					NumberToString( builder, value, 8 );
			  }
			  return builder.ToString();
		 }

		 public override Bits Clone()
		 {
			  return new Bits( Arrays.copyOf( _longs, _longs.Length ), _numberOfBytes );
		 }

		 public Bits Put( sbyte value )
		 {
			  return put( value, ( sizeof( sbyte ) * 8 ) );
		 }

		 public Bits Put( sbyte value, int steps )
		 {
			  return Put( ( long ) value, steps );
		 }

		 public Bits Put( short value )
		 {
			  return put( value, ( sizeof( short ) * 8 ) );
		 }

		 public Bits Put( short value, int steps )
		 {
			  return Put( ( long ) value, steps );
		 }

		 public Bits Put( int value )
		 {
			  return put( value, ( sizeof( int ) * 8 ) );
		 }

		 public Bits Put( int value, int steps )
		 {
			  return Put( ( long ) value, steps );
		 }

		 public Bits Put( long value )
		 {
			  return put( value, ( sizeof( long ) * 8 ) );
		 }

		 public Bits Put( long value, int steps )
		 {
			  int lowLongIndex = _writePosition >> 6; // /64
			  int lowBitInLong = _writePosition % 64;
			  int lowBitsAvailable = 64 - lowBitInLong;
			  long lowValueMask = RightOverflowMask( Math.Min( lowBitsAvailable, steps ) );
			  _longs[lowLongIndex] |= ( value & lowValueMask ) << lowBitInLong;
			  if ( steps > lowBitsAvailable )
			  { // High bits
					long highValueMask = RightOverflowMask( steps - lowBitsAvailable );
					_longs[lowLongIndex + 1] |= ( ( long )( ( ulong )value >> lowBitsAvailable ) ) & highValueMask;
			  }
			  _writePosition += steps;
			  return this;
		 }

		 public Bits Put( sbyte[] bytes, int offset, int length )
		 {
			  for ( int i = offset; i < offset + length; i++ )
			  {
					put( bytes[i], ( sizeof( sbyte ) * 8 ) );
			  }
			  return this;
		 }

		 public bool Available()
		 {
			  return _readPosition < _writePosition;
		 }

		 public sbyte Byte
		 {
			 get
			 {
				  return GetByte( ( sizeof( sbyte ) * 8 ) );
			 }
		 }

		 public sbyte getByte( int steps )
		 {
			  return ( sbyte ) GetLong( steps );
		 }

		 public short Short
		 {
			 get
			 {
				  return GetShort( ( sizeof( short ) * 8 ) );
			 }
		 }

		 public short getShort( int steps )
		 {
			  return ( short ) GetLong( steps );
		 }

		 public int Int
		 {
			 get
			 {
				  return GetInt( ( sizeof( int ) * 8 ) );
			 }
		 }

		 public int getInt( int steps )
		 {
			  return ( int ) GetLong( steps );
		 }

		 public long UnsignedInt
		 {
			 get
			 {
				  return GetInt( ( sizeof( int ) * 8 ) ) & 0xFFFFFFFFL;
			 }
		 }

		 public long Long
		 {
			 get
			 {
				  return GetLong( ( sizeof( long ) * 8 ) );
			 }
		 }

		 public long getLong( int steps )
		 {
			  int lowLongIndex = _readPosition >> 6; // 64
			  int lowBitInLong = _readPosition % 64;
			  int lowBitsAvailable = 64 - lowBitInLong;
			  long lowLongMask = RightOverflowMask( Math.Min( lowBitsAvailable, steps ) ) << lowBitInLong;
			  long lowValue = _longs[lowLongIndex] & lowLongMask;
			  long result = ( long )( ( ulong )lowValue >> lowBitInLong );
			  if ( steps > lowBitsAvailable )
			  { // High bits
					long highLongMask = RightOverflowMask( steps - lowBitsAvailable );
					result |= ( _longs[lowLongIndex + 1] & highLongMask ) << lowBitsAvailable;
			  }
			  _readPosition += steps;
			  return result;
		 }

		 public static bool BitFlag( sbyte flags, sbyte flag )
		 {
			  assert( flag & ( -flag ) ) == flag : "flag should be a power of 2, not: 0x" + flag.ToString( "x" );
			  return ( flags & flag ) == flag;
		 }

		 public static sbyte BitFlag( bool value, sbyte flag )
		 {
			  assert( flag & ( -flag ) ) == flag : "flag should be a power of 2, not: 0x" + flag.ToString( "x" );
			  return value ? flag : 0;
		 }

		 public static sbyte NotFlag( sbyte flags, sbyte flag )
		 {
			  assert( flag & ( -flag ) ) == flag : "flag should be a power of 2, not: 0x" + flag.ToString( "x" );
			  return ( sbyte )( flags & ( ~flag ) );
		 }

		 public static sbyte BitFlags( params sbyte[] flags )
		 {
			  sbyte result = 0;
			  foreach ( sbyte flag in flags )
			  {
					result |= flag;
			  }
			  return result;
		 }

		 /// <summary>
		 /// Clear the position and data.
		 /// </summary>
		 public void Clear( bool zeroBits )
		 {
			  if ( zeroBits )
			  {
					// TODO optimize so that only the touched longs gets cleared
					Arrays.fill( _longs, 0L );
			  }
			  _readPosition = _writePosition = 0;
		 }

		 /// <summary>
		 /// Given the write position, how many longs are in use.
		 /// </summary>
		 public int LongsInUse()
		 {
			  return ( ( _writePosition - 1 ) / ( sizeof( long ) * 8 ) ) + 1;
		 }
	}

}