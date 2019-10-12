using System;
using System.Diagnostics;
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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	public sealed class BigEndianByteArrayBuffer : ReadableBuffer, WritableBuffer
	{
		 internal static BigEndianByteArrayBuffer NewBuffer( int size )
		 {
			  return new BigEndianByteArrayBuffer( size );
		 }

		 internal readonly sbyte[] Buffer;

		 internal BigEndianByteArrayBuffer( int size ) : this( new sbyte[size] )
		 {
		 }

		 public BigEndianByteArrayBuffer( sbyte[] buffer )
		 {
			  this.Buffer = buffer;
		 }

		 public override string ToString()
		 {
			  return ToString( Buffer );
		 }

		 internal static string ToString( sbyte[] buffer )
		 {
			  StringBuilder result = ( new StringBuilder( buffer.Length * 6 + 1 ) ).Append( '[' );
			  foreach ( sbyte b in buffer )
			  {
					if ( b >= 32 && b < 127 )
					{
						 if ( b == ( sbyte )'\'' )
						 {
							  result.Append( "'\\''" );
						 }
						 else
						 {
							  result.Append( '\'' ).Append( ( char ) b ).Append( '\'' );
						 }
					}
					else
					{
						 result.Append( "0x" );
						 if ( b < 16 )
						 {
							  result.Append( 0 );
						 }
						 result.Append( ( b & 0xFF ).ToString( "x" ) );
					}
					result.Append( ", " );
			  }
			  if ( result.Length > 1 )
			  {
					result.Length = result.Length - 2;
			  }
			  result.Append( ']' );
			  return result.ToString();
		 }

		 internal static int Compare( sbyte[] key, sbyte[] searchSpace, int offset )
		 {
			  for ( int i = 0; i < key.Length; i++ )
			  {
					int result = ( key[i] & 0xFF ) - ( searchSpace[offset + i] & 0xFF );
					if ( result != 0 )
					{
						 return result;
					}
			  }
			  return 0;
		 }

		 public void Clear()
		 {
			  Fill( ( sbyte ) 0 );
		 }

		 public void Fill( sbyte zero )
		 {
			  Arrays.fill( Buffer, zero );
		 }

		 public override bool AllZeroes()
		 {
			  foreach ( sbyte b in Buffer )
			  {
					if ( b != 0 )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public bool MinusOneAtTheEnd()
		 {
			  for ( int i = 0; i < Buffer.Length / 2; i++ )
			  {
					if ( Buffer[i] != 0 )
					{
						 return false;
					}
			  }

			  for ( int i = Buffer.Length / 2; i < Buffer.Length; i++ )
			  {
					if ( Buffer[i] != -1 )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public void DataFrom( ByteBuffer buffer )
		 {
			  buffer.get( this.Buffer );
		 }

		 public void DataTo( sbyte[] target, int targetPos )
		 {
			  Debug.Assert( target.Length >= targetPos + Buffer.Length, "insufficient space" );
			  Array.Copy( Buffer, 0, target, targetPos, Buffer.Length );
		 }

		 public override int Size()
		 {
			  return Buffer.Length;
		 }

		 public override sbyte GetByte( int offset )
		 {
			  offset = CheckBounds( offset, 1 );
			  return Buffer[offset];
		 }

		 public override BigEndianByteArrayBuffer PutByte( int offset, sbyte value )
		 {
			  return PutValue( offset, value, 1 );
		 }

		 public override short GetShort( int offset )
		 {
			  offset = CheckBounds( offset, 2 );
			  return ( short )( ( ( 0xFF & Buffer[offset] ) << 8 ) | ( 0xFF & Buffer[offset + 1] ) );
		 }

		 public override BigEndianByteArrayBuffer PutShort( int offset, short value )
		 {
			  return PutValue( offset, value, 2 );
		 }

		 public override char GetChar( int offset )
		 {
			  offset = CheckBounds( offset, 2 );
			  return ( char )( ( ( 0xFF & Buffer[offset] ) << 8 ) | ( 0xFF & Buffer[offset + 1] ) );
		 }

		 public override BigEndianByteArrayBuffer PutChar( int offset, char value )
		 {
			  return PutValue( offset, value, 2 );
		 }

		 public override int GetInt( int offset )
		 {
			  offset = CheckBounds( offset, 4 );
			  return ( ( 0xFF & Buffer[offset] ) << 24 ) | ( ( 0xFF & Buffer[offset + 1] ) << 16 ) | ( ( 0xFF & Buffer[offset + 2] ) << 8 ) | 0xFF & Buffer[offset + 3];
		 }

		 public override BigEndianByteArrayBuffer PutInt( int offset, int value )
		 {
			  return PutValue( offset, value, 4 );
		 }

		 public override long GetLong( int offset )
		 {
			  offset = CheckBounds( offset, 8 );
			  return ( ( 0xFFL & Buffer[offset] ) << 56 ) | ( ( 0xFFL & Buffer[offset + 1] ) << 48 ) | ( ( 0xFFL & Buffer[offset + 2] ) << 40 ) | ( ( 0xFFL & Buffer[offset + 3] ) << 32 ) | ( ( 0xFFL & Buffer[offset + 4] ) << 24 ) | ( ( 0xFFL & Buffer[offset + 5] ) << 16 ) | ( ( 0xFFL & Buffer[offset + 6] ) << 8 ) | ( 0xFFL & Buffer[offset + 7] );
		 }

		 public override sbyte[] Get( int offset, sbyte[] target )
		 {
			  Array.Copy( Buffer, offset, target, 0, target.Length );
			  return target;
		 }

		 public override int CompareTo( sbyte[] value )
		 {
			  return Compare( Buffer, value, 0 );
		 }

		 public override BigEndianByteArrayBuffer PutLong( int offset, long value )
		 {
			  return PutValue( offset, value, 8 );
		 }

		 public override BigEndianByteArrayBuffer Put( int offset, sbyte[] value )
		 {
			  Array.Copy( value, 0, Buffer, offset, value.Length );
			  return this;
		 }

		 public override void GetFrom( PageCursor cursor )
		 {
			  cursor.GetBytes( Buffer );
		 }

		 private BigEndianByteArrayBuffer PutValue( int offset, long value, int size )
		 {
			  offset = CheckBounds( offset, size );
			  while ( size-- > 0 )
			  {
					Buffer[offset + size] = unchecked( ( sbyte )( 0xFF & value ) );
					value = ( long )( ( ulong )value >> 8 );
			  }
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void putIntegerAtEnd(long value) throws java.io.IOException
		 internal void PutIntegerAtEnd( long value )
		 {
			  if ( value < -1 )
			  {
					throw new System.ArgumentException( "Negative values different form -1 are not supported." );
			  }
			  if ( this.Size() < 8 )
			  {
					if ( Long.numberOfLeadingZeros( value ) > ( 8 * this.Size() ) )
					{
						 throw new IOException( string.Format( "Cannot write integer value ({0:D}), value capacity = {1:D}", value, this.Size() ) );
					}
			  }
			  for ( int i = Buffer.Length; i-- > 0 && value != 0; )
			  {
					Buffer[i] = unchecked( ( sbyte )( 0xFF & value ) );
					value = ( long )( ( ulong )value >> 8 );
			  }
		 }

		 internal long IntegerFromEnd
		 {
			 get
			 {
				  long value = 0;
				  for ( int i = Math.Max( 0, Buffer.Length - 8 ); i < Buffer.Length; i++ )
				  {
						value = ( value << 8 ) | ( 0xFFL & Buffer[i] );
				  }
				  return value;
			 }
		 }

		 public void Read( WritableBuffer target )
		 {
			  target.Put( 0, Buffer );
		 }

		 private int CheckBounds( int offset, int size )
		 {
			  if ( offset < 0 || offset > size() - size )
			  {
					throw new System.IndexOutOfRangeException( string.Format( "offset={0:D}, buffer size={1:D}, data item size={2:D}", offset, size(), size ) );
			  }
			  return offset;
		 }
	}

}