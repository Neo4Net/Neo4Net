using System;
using System.Diagnostics;

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
namespace Org.Neo4j.Values.Storable
{

	using HashFunction = Org.Neo4j.Hashing.HashFunction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.utf8Value;

	/*
	 * Just as a normal StringValue but is backed by a byte array and does string
	 * serialization lazily when necessary.
	 *
	 */
	public sealed class UTF8StringValue : StringValue
	{
		 /// <summary>
		 /// Used for removing the high order bit from byte. </summary>
		 private static readonly int _highBitMask = 0b0111_1111;
		 /// <summary>
		 /// Used for detecting non-continuation bytes. For example {@code 0b10xx_xxxx}. </summary>
		 private static readonly int _nonContinuationBitMask = 0b0100_0000;

		 private volatile string _value;
		 private readonly sbyte[] _bytes;
		 private readonly int _offset;
		 private readonly int _byteLength;

		 internal UTF8StringValue( sbyte[] bytes, int offset, int length )
		 {
			  Debug.Assert( bytes != null );
			  this._bytes = bytes;
			  this._offset = offset;
			  this._byteLength = length;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteUTF8( _bytes, _offset, _byteLength );
		 }

		 public override bool Equals( Value value )
		 {
			  if ( value is UTF8StringValue )
			  {
					UTF8StringValue other = ( UTF8StringValue ) value;
					if ( _byteLength != other._byteLength )
					{
						 return false;
					}
					for ( int i = _offset, j = other._offset; i < _byteLength; i++, j++ )
					{
						 if ( _bytes[i] != other._bytes[j] )
						 {
							  return false;
						 }
					}
					return true;
			  }
			  else
			  {
					return base.Equals( value );
			  }
		 }

		 internal override string Value()
		 {
			  string s = _value;
			  if ( string.ReferenceEquals( s, null ) )
			  {
					lock ( this )
					{
						 s = _value;
						 if ( string.ReferenceEquals( s, null ) )
						 {
							  _value = s = StringHelper.NewString( _bytes, _offset, _byteLength, StandardCharsets.UTF_8 );
						 }
					}
			  }
			  return s;
		 }

		 public override int Length()
		 {
			  return NumberOfCodePoints( _bytes, _offset, _byteLength );
		 }

		 private static int NumberOfCodePoints( sbyte[] bytes, int offset, int byteLength )
		 {
			  int count = 0, i = offset, len = offset + byteLength;
			  while ( i < len )
			  {
					sbyte b = bytes[i];
					//If high bit is zero (equivalent to the byte being positive in two's complement)
					//we are dealing with an ascii value and use a single byte for storing the value.
					if ( b >= 0 )
					{
						 i++;
						 count++;
						 continue;
					}

					//The number of high bits tells us how many bytes we use to store the value
					//e.g. 110xxxx -> need two bytes, 1110xxxx -> need three bytes, 11110xxx -> needs
					//four bytes
					while ( b < 0 )
					{
						 i++;
						 b = ( sbyte )( b << 1 );
					}
					count++;
			  }
			  return count;
		 }

		 public override int ComputeHash()
		 {
			  if ( _bytes.Length == 0 || _byteLength == 0 )
			  {
					return 0;
			  }

			  CodePointCursor cpc = new CodePointCursor( _bytes, _offset );
			  int hash = 1;
			  int len = _offset + _byteLength;

			  while ( cpc.I < len )
			  {
					hash = 31 * hash + ( int ) cpc.NextCodePoint();
			  }
			  return hash;
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  CodePointCursor cpc = new CodePointCursor( _bytes, _offset );
			  int len = _offset + _byteLength;

			  while ( cpc.I < len )
			  {
					long codePointA = cpc.NextCodePoint() << 32;
					long codePointB = 0L;
					if ( cpc.I < len )
					{
						 codePointB = cpc.NextCodePoint();
					}
					hash = hashFunction.Update( hash, codePointA + codePointB );
			  }

			  return hashFunction.Update( hash, cpc.CodePointCount );
		 }

		 public class CodePointCursor
		 {
			  internal sbyte[] Values;
			  internal int I;
			  internal int CodePointCount;

			  public CodePointCursor( sbyte[] values, int offset )
			  {
					this.Values = values;
					this.I = offset;
			  }

			  public virtual long NextCodePoint()
			  {
					CodePointCount++;
					sbyte b = Values[I];
					//If high bit is zero (equivalent to the byte being positive in two's complement)
					//we are dealing with an ascii value and use a single byte for storing the value.
					if ( b >= 0 )
					{
						 I++;
						 return b;
					}

					//We can now have one of three situations.
					//Byte1    Byte2    Byte3    Byte4
					//110xxxxx 10xxxxxx
					//1110xxxx 10xxxxxx 10xxxxxx
					//11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
					//Figure out how many bytes we need by reading the number of leading bytes
					int bytesNeeded = 0;
					while ( b < 0 )
					{
						 bytesNeeded++;
						 b = ( sbyte )( b << 1 );
					}
					int codePoint = codePoint( Values, b, I, bytesNeeded );
					I += bytesNeeded;
					return codePoint;
			  }
		 }

		 public override TextValue Substring( int start, int length )
		 {
			  if ( start < 0 || length < 0 )
			  {
					throw new System.IndexOutOfRangeException( "Cannot handle negative start index nor negative length" );
			  }
			  if ( length == 0 )
			  {
					return StringValue.EMPTY;
			  }

			  int end = start + length;
			  sbyte[] values = _bytes;
			  int count = 0, byteStart = -1, byteEnd = -1, i = _offset, len = _offset + _byteLength;
			  while ( i < len )
			  {
					if ( count == start )
					{
						 byteStart = i;
					}
					if ( count == end )
					{
						 byteEnd = i;
						 break;
					}
					sbyte b = values[i];
					//If high bit is zero (equivalent to the byte being positive in two's complement)
					//we are dealing with an ascii value and use a single byte for storing the value.
					if ( b >= 0 )
					{
						 i++;
					}

					while ( b < 0 )
					{
						 i++;
						 b = ( sbyte )( b << 1 );
					}
					count++;
			  }
			  if ( byteEnd < 0 )
			  {
					byteEnd = len;
			  }
			  if ( byteStart < 0 )
			  {
					return StringValue.EMPTY;
			  }
			  return new UTF8StringValue( values, byteStart, byteEnd - byteStart );
		 }

		 public override TextValue Trim()
		 {
			  sbyte[] values = _bytes;

			  if ( values.Length == 0 || _byteLength == 0 )
			  {
					return this;
			  }

			  int startIndex = TrimLeftIndex();
			  int endIndex = TrimRightIndex();
			  if ( startIndex > endIndex )
			  {
					return StringValue.EMPTY;
			  }

			  return new UTF8StringValue( values, startIndex, Math.Max( endIndex + 1 - startIndex, 0 ) );
		 }

		 public override TextValue Ltrim()
		 {
			  sbyte[] values = _bytes;
			  if ( values.Length == 0 || _byteLength == 0 )
			  {
					return this;
			  }

			  int startIndex = TrimLeftIndex();
			  if ( startIndex >= values.Length )
			  {
					return StringValue.EMPTY;
			  }
			  return new UTF8StringValue( values, startIndex, values.Length - startIndex );
		 }

		 public override TextValue Rtrim()
		 {
			  sbyte[] values = _bytes;
			  if ( values.Length == 0 || _byteLength == 0 )
			  {
					return this;
			  }

			  int endIndex = TrimRightIndex();
			  if ( endIndex < 0 )
			  {
					return StringValue.EMPTY;
			  }
			  return new UTF8StringValue( values, _offset, endIndex + 1 - _offset );
		 }

		 public override TextValue Plus( TextValue other )
		 {
			  if ( other is UTF8StringValue )
			  {
					UTF8StringValue rhs = ( UTF8StringValue ) other;
					sbyte[] newBytes = new sbyte[_byteLength + rhs._byteLength];
					Array.Copy( _bytes, _offset, newBytes, 0, _byteLength );
					Array.Copy( rhs._bytes, rhs._offset, newBytes, _byteLength, rhs._byteLength );
					return utf8Value( newBytes );
			  }

			  return Values.StringValue( StringValueConflict() + other.StringValue() );
		 }

		 public override bool StartsWith( TextValue other )
		 {

			  if ( other is UTF8StringValue )
			  {
					UTF8StringValue suffix = ( UTF8StringValue ) other;
					return StartsWith( suffix, 0 );
			  }

			  return Value().StartsWith(other.StringValue(), StringComparison.Ordinal);
		 }

		 public override bool EndsWith( TextValue other )
		 {

			  if ( other is UTF8StringValue )
			  {
					UTF8StringValue suffix = ( UTF8StringValue ) other;
					return StartsWith( suffix, _byteLength - suffix._byteLength );
			  }

			  return Value().EndsWith(other.StringValue(), StringComparison.Ordinal);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("StatementWithEmptyBody") @Override public boolean contains(TextValue other)
		 public override bool Contains( TextValue other )
		 {

			  if ( other is UTF8StringValue )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final UTF8StringValue substring = (UTF8StringValue) other;
					UTF8StringValue substring = ( UTF8StringValue ) other;
					if ( _byteLength == 0 )
					{
						 return substring._byteLength == 0;
					}
					if ( substring._byteLength == 0 )
					{
						 return true;
					}
					if ( substring._byteLength > _byteLength )
					{
						 return false;
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte first = substring.bytes[substring.offset];
					sbyte first = substring._bytes[substring._offset];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int max = offset + byteLength - substring.byteLength;
					int max = _offset + _byteLength - substring._byteLength;
					for ( int pos = _offset; pos <= max; pos++ )
					{
						 //find first byte
						 if ( _bytes[pos] != first )
						 {
							  while ( ++pos <= max && _bytes[pos] != first )
							  {
									//do nothing
							  }
						 }

						 //Now we have the first byte match, look at the rest
						 if ( pos <= max )
						 {
							  int i = pos + 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int end = pos + substring.byteLength;
							  int end = pos + substring._byteLength;
							  for ( int j = substring._offset + 1; i < end && _bytes[i] == substring._bytes[j]; j++, i++ )
							  {
									//do nothing
							  }

							  if ( i == end )
							  {
									return true;
							  }
						 }
					}
					return false;
			  }

			  return Value().Contains(other.StringValue());
		 }

		 private bool StartsWith( UTF8StringValue prefix, int startPos )
		 {
			  int thisOffset = _offset + startPos;
			  int prefixOffset = prefix._offset;
			  int prefixCount = prefix._byteLength;
			  if ( startPos < 0 || prefixCount > _byteLength )
			  {
					return false;
			  }

			  while ( --prefixCount >= 0 )
			  {
					if ( _bytes[thisOffset++] != prefix._bytes[prefixOffset++] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public override TextValue Reverse()
		 {
			  sbyte[] values = _bytes;

			  if ( values.Length == 0 || _byteLength == 0 )
			  {
					return StringValue.EMPTY;
			  }

			  int i = _offset, len = _offset + _byteLength;
			  sbyte[] newValues = new sbyte[_byteLength];
			  while ( i < len )
			  {
					sbyte b = values[i];
					//If high bit is zero (equivalent to the byte being positive in two's complement)
					//we are dealing with an ascii value and use a single byte for storing the value.
					if ( b >= 0 )
					{
						 //a single byte is trivial to reverse
						 //just put it at the opposite end of the new array
						 newValues[len - 1 - i] = b;
						 i++;
						 continue;
					}

					//We can now have one of three situations.
					//Byte1    Byte2    Byte3    Byte4
					//110xxxxx 10xxxxxx
					//1110xxxx 10xxxxxx 10xxxxxx
					//11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
					//Figure out how many bytes we need by reading the number of leading bytes
					int bytesNeeded = 0;
					while ( b < 0 )
					{
						 bytesNeeded++;
						 b = ( sbyte )( b << 1 );
					}
					//reversing when multiple bytes are needed for the code point we cannot just reverse
					//since we need to preserve the code point while moving it,
					//e.g. [A, b1,b2, B] -> [B, b1,b2, A]
					Array.Copy( values, i, newValues, len - i - bytesNeeded, bytesNeeded );
					i += bytesNeeded;
			  }

			  return new UTF8StringValue( newValues, 0, newValues.Length );
		 }

		 public override int CompareTo( TextValue other )
		 {
			  if ( !( other is UTF8StringValue ) )
			  {
					return base.CompareTo( other );
			  }
			  UTF8StringValue otherUTF8 = ( UTF8StringValue ) other;
			  return ByteArrayCompare( _bytes, _offset, _byteLength, otherUTF8._bytes, otherUTF8._offset, otherUTF8._byteLength );
		 }

		 public static int ByteArrayCompare( sbyte[] value1, sbyte[] value2 )
		 {
			  return ByteArrayCompare( value1, 0, value1.Length, value2, 0, value2.Length );
		 }

		 public static int ByteArrayCompare( sbyte[] value1, int value1Offset, int value1Length, sbyte[] value2, int value2Offset, int value2Length )
		 {
			  int lim = Math.Min( value1Length, value2Length );
			  for ( int i = 0; i < lim; i++ )
			  {
					sbyte b1 = value1[i + value1Offset];
					sbyte b2 = value2[i + value2Offset];
					if ( b1 != b2 )
					{
						 return ( ( ( int ) b1 ) & 0xFF ) - ( ( ( int ) b2 ) & 0xFF );
					}
			  }
			  return value1Length - value2Length;
		 }

		 internal override Matcher Matcher( Pattern pattern )
		 {
			  return pattern.matcher( Value() ); // TODO: can we do better here?
		 }

		 /// <summary>
		 /// Returns the left-most index into the underlying byte array that does not belong to a whitespace code point
		 /// </summary>
		 private int TrimLeftIndex()
		 {
			  int i = _offset, len = _offset + _byteLength;
			  while ( i < len )
			  {
					sbyte b = _bytes[i];
					//If high bit is zero (equivalent to the byte being positive in two's complement)
					//we are dealing with an ascii value and use a single byte for storing the value.
					if ( b >= 0 )
					{
						 if ( !char.IsWhiteSpace( b ) )
						 {
							  return i;
						 }
						 i++;
						 continue;
					}

					//We can now have one of three situations.
					//Byte1    Byte2    Byte3    Byte4
					//110xxxxx 10xxxxxx
					//1110xxxx 10xxxxxx 10xxxxxx
					//11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
					//Figure out how many bytes we need by reading the number of leading bytes
					int bytesNeeded = 0;
					while ( b < 0 )
					{
						 bytesNeeded++;
						 b = ( sbyte )( b << 1 );
					}
					int codePoint = codePoint( _bytes, b, i, bytesNeeded );
					if ( !char.IsWhiteSpace( codePoint ) )
					{
						 return i;
					}
					i += bytesNeeded;
			  }
			  return i;
		 }

		 /// <summary>
		 /// Returns the right-most index into the underlying byte array that does not belong to a whitespace code point
		 /// </summary>
		 private int TrimRightIndex()
		 {
			  int index = _offset + _byteLength - 1;
			  while ( index >= 0 )
			  {
					sbyte b = _bytes[index];
					//If high bit is zero (equivalent to the byte being positive in two's complement)
					//we are dealing with an ascii value and use a single byte for storing the value.
					if ( b >= 0 )
					{
						 if ( !char.IsWhiteSpace( b ) )
						 {
							  return index;
						 }
						 index--;
						 continue;
					}

					//We can now have one of three situations.
					//Byte1    Byte2    Byte3    Byte4
					//110xxxxx 10xxxxxx
					//1110xxxx 10xxxxxx 10xxxxxx
					//11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
					int bytesNeeded = 1;
					while ( ( b & _nonContinuationBitMask ) == 0 )
					{
						 bytesNeeded++;
						 b = _bytes[--index];
					}

					int codePoint = codePoint( _bytes, ( sbyte )( b << bytesNeeded ), index, bytesNeeded );
					if ( !char.IsWhiteSpace( codePoint ) )
					{
						 return Math.Min( index + bytesNeeded - 1, _bytes.Length - 1 );
					}
					index--;

			  }
			  return index;
		 }

		 public sbyte[] Bytes()
		 {
			  return _bytes;
		 }

		 private static int CodePoint( sbyte[] bytes, sbyte currentByte, int i, int bytesNeeded )
		 {
			  int codePoint;
			  switch ( bytesNeeded )
			  {
			  case 2:
					codePoint = ( currentByte << 4 ) | ( bytes[i + 1] & _highBitMask );
					break;
			  case 3:
					codePoint = ( currentByte << 9 ) | ( ( bytes[i + 1] & _highBitMask ) << 6 ) | ( bytes[i + 2] & _highBitMask );
					break;
			  case 4:
					codePoint = ( currentByte << 14 ) | ( ( bytes[i + 1] & _highBitMask ) << 12 ) | ( ( bytes[i + 2] & _highBitMask ) << 6 ) | ( bytes[i + 3] & _highBitMask );
					break;
			  default:
					throw new System.ArgumentException( "Malformed UTF8 value" );
			  }
			  return codePoint;
		 }
	}

}