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
namespace Neo4Net.Index.@internal.gbptree
{

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	public class SimpleByteArrayLayout : TestLayout<RawBytes, RawBytes>
	{
		 private readonly bool _useFirstLongAsSeed;

		 internal SimpleByteArrayLayout() : this(true)
		 {
		 }

		 internal SimpleByteArrayLayout( bool useFirstLongAsSeed )
		 {
			  this._useFirstLongAsSeed = useFirstLongAsSeed;
		 }

		 public override RawBytes NewKey()
		 {
			  return new RawBytes();
		 }

		 public override RawBytes CopyKey( RawBytes rawBytes, RawBytes into )
		 {
			  return CopyKey( rawBytes, into, rawBytes.Bytes.Length );
		 }

		 private RawBytes CopyKey( RawBytes rawBytes, RawBytes into, int length )
		 {
			  into.Bytes = Arrays.copyOf( rawBytes.Bytes, length );
			  return into;
		 }

		 public override RawBytes NewValue()
		 {
			  return new RawBytes();
		 }

		 public override int KeySize( RawBytes rawBytes )
		 {
			  if ( rawBytes == null )
			  {
					return -1;
			  }
			  return rawBytes.Bytes.Length;
		 }

		 public override int ValueSize( RawBytes rawBytes )
		 {
			  if ( rawBytes == null )
			  {
					return -1;
			  }
			  return rawBytes.Bytes.Length;
		 }

		 public override void WriteKey( PageCursor cursor, RawBytes rawBytes )
		 {
			  cursor.PutBytes( rawBytes.Bytes );
		 }

		 public override void WriteValue( PageCursor cursor, RawBytes rawBytes )
		 {
			  cursor.PutBytes( rawBytes.Bytes );
		 }

		 public override void ReadKey( PageCursor cursor, RawBytes into, int keySize )
		 {
			  into.Bytes = new sbyte[keySize];
			  cursor.GetBytes( into.Bytes );
		 }

		 public override void ReadValue( PageCursor cursor, RawBytes into, int valueSize )
		 {
			  into.Bytes = new sbyte[valueSize];
			  cursor.GetBytes( into.Bytes );
		 }

		 public override bool FixedSize()
		 {
			  return false;
		 }

		 public override void MinimalSplitter( RawBytes left, RawBytes right, RawBytes into )
		 {
			  long leftSeed = KeySeed( left );
			  long rightSeed = KeySeed( right );
			  if ( _useFirstLongAsSeed && leftSeed != rightSeed )
			  {
					// Minimal splitter is first 8B (seed)
					CopyKey( right, into, Long.BYTES );
			  }
			  else
			  {
					// They had the same seed. Need to look at entire array
					int maxLength = Math.Min( left.Bytes.Length, right.Bytes.Length );
					int firstIndexToDiffer = 0;
					for ( ; firstIndexToDiffer < maxLength; firstIndexToDiffer++ )
					{
						 if ( left.Bytes[firstIndexToDiffer] != right.Bytes[firstIndexToDiffer] )
						 {
							  break;
						 }
					}
					// Convert from index to length
					int targetLength = firstIndexToDiffer + 1;
					CopyKey( right, into, targetLength );
			  }
		 }

		 public override long Identifier()
		 {
			  return 666;
		 }

		 public override int MajorVersion()
		 {
			  return 0;
		 }

		 public override int MinorVersion()
		 {
			  return 0;
		 }

		 public override int Compare( RawBytes o1, RawBytes o2 )
		 {
			  if ( o1.Bytes == null )
			  {
					return -1;
			  }
			  if ( o2.Bytes == null )
			  {
					return 1;
			  }
			  if ( _useFirstLongAsSeed )
			  {
					int compare = Long.compare( KeySeed( o1 ), KeySeed( o2 ) );
					return compare != 0 ? compare : ByteArrayCompare( o1.Bytes, o2.Bytes, Long.BYTES );
			  }
			  else
			  {
					return ByteArrayCompare( o1.Bytes, o2.Bytes, 0 );
			  }
		 }

		 internal override int CompareValue( RawBytes v1, RawBytes v2 )
		 {
			  return Compare( v1, v2 );
		 }

		 private int ByteArrayCompare( sbyte[] a, sbyte[] b, int fromPos )
		 {
			  Debug.Assert( a != null && b != null, "Null arrays not supported." );

			  if ( a == b )
			  {
					return 0;
			  }

			  int length = Math.Min( a.Length, b.Length );
			  for ( int i = fromPos; i < length; i++ )
			  {
					int compare = Byte.compare( a[i], b[i] );
					if ( compare != 0 )
					{
						 return compare;
					}
			  }

			  return Integer.compare( a.Length, b.Length );
		 }

		 public override RawBytes Key( long seed )
		 {
			  RawBytes key = NewKey();
			  key.Bytes = FromSeed( seed );
			  return key;
		 }

		 public override RawBytes Value( long seed )
		 {
			  RawBytes value = NewValue();
			  value.Bytes = FromSeed( seed );
			  return value;
		 }

		 public override long KeySeed( RawBytes rawBytes )
		 {
			  return ToSeed( rawBytes );
		 }

		 public override long ValueSeed( RawBytes rawBytes )
		 {
			  return ToSeed( rawBytes );
		 }

		 private long ToSeed( RawBytes rawBytes )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( Long.BYTES );
			  // Because keySearch is done inside the same shouldRetry block as keyCount()
			  // We risk reading crap data. This is not a problem because we will retry
			  // but buffer will throw here if we don't take that into consideration.
			  sbyte[] bytes = rawBytes.Bytes;
			  if ( bytes.Length >= Long.BYTES )
			  {
					buffer.put( bytes, 0, Long.BYTES );
					buffer.flip();
					return buffer.Long;
			  }
			  return 0;
		 }

		 private sbyte[] FromSeed( long seed )
		 {
			  int tail = ( int ) Math.Abs( seed % Long.BYTES );
			  ByteBuffer buffer = ByteBuffer.allocate( Long.BYTES + tail );
			  buffer.putLong( seed );
			  buffer.put( new sbyte[tail] );
			  return buffer.array();
		 }
	}

}