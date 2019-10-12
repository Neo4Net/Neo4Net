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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	/// <summary>
	/// Encodes String into a long with very small chance of collision, i.e. two different Strings encoded into
	/// the same long value.
	/// 
	/// Assumes a single thread making all calls to <seealso cref="encode(object)"/>.
	/// </summary>
	public class StringEncoder : Encoder
	{
		 private const long UPPER_INT_MASK = 0x00000000_FFFFFFFFL;
		 private const int FOURTH_BYTE = 0x000000FF;

		 // fixed values
		 private readonly int _numCodes;
		 private const int ENCODING_THRESHOLD = 7;

		 // data changing over time, potentially with each encoding
		 private readonly sbyte[] _reMap = new sbyte[256];
		 private int _numChars;

		 public StringEncoder() : this(2)
		 {
		 }

		 public StringEncoder( int codingStrength )
		 {
			  _numCodes = codingStrength > 2 ? codingStrength : 2;
			  Arrays.fill( _reMap, ( sbyte ) - 1 );
		 }

		 public override long Encode( object s )
		 {
			  int[] val = EncodeInt( ( string ) s );
			  return ( long ) val[0] << 32 | val[1] & UPPER_INT_MASK;
		 }

		 private int[] EncodeInt( string s )
		 {
			  // construct bytes from string
			  int inputLength = s.Length;
			  sbyte[] bytes = new sbyte[inputLength];
			  for ( int i = 0; i < inputLength; i++ )
			  {
					bytes[i] = ( sbyte )( ( s[i] ) % 127 );
			  }
			  ReMap( bytes, inputLength );
			  // encode
			  if ( inputLength <= ENCODING_THRESHOLD )
			  {
					return SimplestCode( bytes, inputLength );
			  }
			  int[] codes = new int[_numCodes];
			  for ( int i = 0; i < _numCodes; )
			  {
					codes[i] = GetCode( bytes, inputLength, 1 );
					codes[i + 1] = GetCode( bytes, inputLength, inputLength - 1 );
					i += 2;
			  }
			  int carryOver = LengthEncoder( inputLength ) << 1;
			  int temp = 0;
			  for ( int i = 0; i < _numCodes; i++ )
			  {
					temp = codes[i] & FOURTH_BYTE;
					codes[i] = ( int )( ( uint )codes[i] >> 8 ) | carryOver << 24;
					carryOver = temp;
			  }
			  return codes;
		 }

		 private int LengthEncoder( int length )
		 {
			  if ( length < 32 )
			  {
					return length;
			  }
			  else if ( length <= 96 )
			  {
					return length >> 1;
			  }
			  else if ( length <= 324 )
			  {
					return length >> 2;
			  }
			  else if ( length <= 580 )
			  {
					return length >> 3;
			  }
			  else if ( length <= 836 )
			  {
					return length >> 4;
			  }
			  else
			  {
					return 127;
			  }
		 }

		 private void ReMap( sbyte[] bytes, int inputLength )
		 {
			  for ( int i = 0; i < inputLength; i++ )
			  {
					if ( _reMap[bytes[i]] == -1 )
					{
						 lock ( this )
						 {
							  if ( _reMap[bytes[i]] == -1 )
							  {
									_reMap[bytes[i]] = ( sbyte )( _numChars++ % 256 );
							  }
						 }
					}
					bytes[i] = _reMap[bytes[i]];
			  }
		 }

		 private int[] SimplestCode( sbyte[] bytes, int inputLength )
		 {
			  int[] codes = new int[]{ 0, 0 };
			  codes[0] = max( inputLength, 1 ) << 25;
			  codes[1] = 0;
			  for ( int i = 0; i < 3 && i < inputLength; i++ )
			  {
					codes[0] = codes[0] | bytes[i] << ( ( 2 - i ) * 8 );
			  }
			  for ( int i = 3; i < 7 && i < inputLength; i++ )
			  {
					codes[1] = codes[1] | ( bytes[i] ) << ( ( 6 - i ) * 8 );
			  }
			  return codes;
		 }

		 private int GetCode( sbyte[] bytes, int inputLength, int order )
		 {
			  long code = 0;
			  int size = inputLength;
			  for ( int i = 0; i < size; i++ )
			  {
					//code += (((long)bytes[(i*order) % size]) << (i % 7)*8);
					long val = bytes[( i * order ) % size];
					for ( int k = 1; k <= i; k++ )
					{
						 long prev = val;
						 val = ( val << 4 ) + prev; //% Integer.MAX_VALUE;
					}
					code += val;
			  }
			  return ( int ) code;
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + _numCodes + "]";
		 }
	}

}