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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using UTF8 = Org.Neo4j.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PageCache_Fields.PAGE_SIZE;

	/// <summary>
	/// Stores <seealso cref="string strings"/> in a <seealso cref="ByteArray"/> provided by <seealso cref="NumberArrayFactory"/>. Each string can have different
	/// length, where maximum string length is 2^16 - 1.
	/// </summary>
	public class StringCollisionValues : CollisionValues
	{
		 private readonly long _chunkSize;
		 private readonly ByteArray _cache;
		 private long _offset;
		 private ByteArray _current;

		 public StringCollisionValues( NumberArrayFactory factory, long length )
		 {
			  // Let's have length (also chunk size) be divisible by PAGE_SIZE, such that our calculations below
			  // works for all NumberArray implementations.
			  int remainder = ( int )( length % PAGE_SIZE );
			  if ( remainder != 0 )
			  {
					length += PAGE_SIZE - remainder;
			  }

			  _chunkSize = max( length, PAGE_SIZE );
			  _cache = factory.NewDynamicByteArray( _chunkSize, new sbyte[1] );
			  _current = _cache.at( 0 );
		 }

		 public override long Add( object id )
		 {
			  string @string = ( string ) id;
			  sbyte[] bytes = UTF8.encode( @string );
			  int length = bytes.Length;
			  if ( length > 0xFFFF )
			  {
					throw new System.ArgumentException( @string );
			  }

			  long startOffset = _offset;
			  _cache.setByte( _offset++, 0, ( sbyte ) length );
			  _cache.setByte( _offset++, 0, ( sbyte )( ( int )( ( uint )length >> ( sizeof( sbyte ) * 8 ) ) ) );
			  _current = _cache.at( _offset );
			  for ( int i = 0; i < length; )
			  {
					int bytesLeftToWrite = length - i;
					int bytesLeftInChunk = ( int )( _chunkSize - _offset % _chunkSize );
					int bytesToWriteInThisChunk = min( bytesLeftToWrite, bytesLeftInChunk );
					for ( int j = 0; j < bytesToWriteInThisChunk; j++ )
					{
						 _current.setByte( _offset++, 0, bytes[i++] );
					}

					if ( length > i )
					{
						 _current = _cache.at( _offset );
					}
			  }

			  return startOffset;
		 }

		 public override object Get( long offset )
		 {
			  int length = _cache.getByte( offset++, 0 ) & 0xFF;
			  length |= ( _cache.getByte( offset++, 0 ) & 0xFF ) << ( sizeof( sbyte ) * 8 );
			  ByteArray array = _cache.at( offset );
			  sbyte[] bytes = new sbyte[length];
			  for ( int i = 0; i < length; )
			  {
					int bytesLeftToRead = length - i;
					int bytesLeftInChunk = ( int )( _chunkSize - offset % _chunkSize );
					int bytesToReadInThisChunk = min( bytesLeftToRead, bytesLeftInChunk );
					for ( int j = 0; j < bytesToReadInThisChunk; j++ )
					{
						 bytes[i++] = array.GetByte( offset++, 0 );
					}

					if ( length > i )
					{
						 array = _cache.at( offset );
					}
			  }
			  return UTF8.decode( bytes );
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  _cache.acceptMemoryStatsVisitor( visitor );
		 }

		 public override void Close()
		 {
			  _cache.close();
		 }
	}

}