using System.Collections.Generic;
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
namespace Neo4Net.Kernel.impl.util
{

	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;
	using UTF8 = Neo4Net.@string.UTF8;

	public abstract class IoPrimitiveUtils
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String readString(org.neo4j.storageengine.api.ReadableChannel channel, int length) throws java.io.IOException
		 public static string ReadString( ReadableChannel channel, int length )
		 {
			  Debug.Assert( length >= 0, "invalid array length " + length );
			  sbyte[] chars = new sbyte[length];
			  channel.Get( chars, length );
			  return UTF8.decode( chars );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void write3bLengthAndString(org.neo4j.storageengine.api.WritableChannel channel, String string) throws java.io.IOException
		 public static void Write3bLengthAndString( WritableChannel channel, string @string )
		 {
			  sbyte[] chars = UTF8.encode( @string );
			  // 3 bytes to represent the length (4 is a bit overkill)... maybe
			  // this space optimization is a bit overkill also :)
			  channel.PutShort( ( short )chars.Length );
			  channel.Put( ( sbyte )( chars.Length >> 16 ) );
			  channel.Put( chars, chars.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String read3bLengthAndString(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 public static string Read3bLengthAndString( ReadableChannel channel )
		 {
			  short lengthShort = channel.Short;
			  sbyte lengthByte = channel.Get();
			  int length = ( lengthByte << 16 ) | ( lengthShort & 0xFFFF );
			  sbyte[] chars = new sbyte[length];
			  channel.Get( chars, length );
			  return UTF8.decode( chars );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void write2bLengthAndString(org.neo4j.storageengine.api.WritableChannel channel, String string) throws java.io.IOException
		 public static void Write2bLengthAndString( WritableChannel channel, string @string )
		 {
			  sbyte[] chars = UTF8.encode( @string );
			  channel.PutShort( ( short )chars.Length );
			  channel.Put( chars, chars.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String read2bLengthAndString(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 public static string Read2bLengthAndString( ReadableChannel channel )
		 {
			  short length = channel.Short;
			  return ReadString( channel, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static char[] readCharArray(java.nio.channels.ReadableByteChannel channel, ByteBuffer buffer, char[] charArray) throws java.io.IOException
		 private static char[] ReadCharArray( ReadableByteChannel channel, ByteBuffer buffer, char[] charArray )
		 {
			  buffer.clear();
			  int charsLeft = charArray.Length;
			  int maxSize = buffer.capacity() / 2;
			  int offset = 0; // offset in chars
			  while ( charsLeft > 0 )
			  {
					if ( charsLeft > maxSize )
					{
						 buffer.limit( maxSize * 2 );
						 charsLeft -= maxSize;
					}
					else
					{
						 buffer.limit( charsLeft * 2 );
						 charsLeft = 0;
					}
					if ( channel.read( buffer ) != buffer.limit() )
					{
						 return null;
					}
					buffer.flip();
					int length = buffer.limit() / 2;
					buffer.asCharBuffer().get(charArray, offset, length);
					offset += length;
					buffer.clear();
			  }
			  return charArray;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean readAndFlip(java.nio.channels.ReadableByteChannel channel, ByteBuffer buffer, int bytes) throws java.io.IOException
		 public static bool ReadAndFlip( ReadableByteChannel channel, ByteBuffer buffer, int bytes )
		 {
			  buffer.clear();
			  buffer.limit( bytes );
			  while ( buffer.hasRemaining() )
			  {
					int read = channel.read( buffer );

					if ( read == -1 )
					{
						 return false;
					}
			  }
			  buffer.flip();
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static System.Nullable<int> readInt(java.nio.channels.ReadableByteChannel channel, ByteBuffer buffer) throws java.io.IOException
		 public static int? ReadInt( ReadableByteChannel channel, ByteBuffer buffer )
		 {
			  return ReadAndFlip( channel, buffer, 4 ) ? buffer.Int : null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] readBytes(java.nio.channels.ReadableByteChannel channel, byte[] array) throws java.io.IOException
		 public static sbyte[] ReadBytes( ReadableByteChannel channel, sbyte[] array )
		 {
			  return ReadBytes( channel, array, array.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] readBytes(java.nio.channels.ReadableByteChannel channel, byte[] array, int length) throws java.io.IOException
		 public static sbyte[] ReadBytes( ReadableByteChannel channel, sbyte[] array, int length )
		 {
			  return ReadAndFlip( channel, ByteBuffer.wrap( array ), length ) ? array : null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Map<String, String> read2bMap(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 public static IDictionary<string, string> Read2bMap( ReadableChannel channel )
		 {
			  short size = channel.Short;
			  IDictionary<string, string> map = new Dictionary<string, string>( size );
			  for ( int i = 0; i < size; i++ )
			  {
					string key = Read2bLengthAndString( channel );
					string value = Read2bLengthAndString( channel );
					map[key] = value;
			  }
			  return map;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String readLengthAndString(java.nio.channels.ReadableByteChannel channel, ByteBuffer buffer) throws java.io.IOException
		 public static string ReadLengthAndString( ReadableByteChannel channel, ByteBuffer buffer )
		 {
			  int? length = ReadInt( channel, buffer );
			  if ( length != null )
			  {
					char[] chars = new char[length];
					chars = ReadCharArray( channel, buffer, chars );
					return chars == null ? null : new string( chars );
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeLengthAndString(org.neo4j.io.fs.StoreChannel channel, ByteBuffer buffer, String value) throws java.io.IOException
		 public static void WriteLengthAndString( StoreChannel channel, ByteBuffer buffer, string value )
		 {
			  char[] chars = value.ToCharArray();
			  int length = chars.Length;
			  WriteInt( channel, buffer, length );
			  WriteChars( channel, buffer, chars );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeChars(org.neo4j.io.fs.StoreChannel channel, ByteBuffer buffer, char[] chars) throws java.io.IOException
		 private static void WriteChars( StoreChannel channel, ByteBuffer buffer, char[] chars )
		 {
			  int position = 0;
			  do
			  {
					buffer.clear();
					int leftToWrite = chars.Length - position;
					if ( leftToWrite * 2 < buffer.capacity() )
					{
						 buffer.asCharBuffer().put(chars, position, leftToWrite);
						 buffer.limit( leftToWrite * 2 );
						 channel.write( buffer );
						 position += leftToWrite;
					}
					else
					{
						 int length = buffer.capacity() / 2;
						 buffer.asCharBuffer().put(chars, position, length);
						 buffer.limit( length * 2 );
						 channel.write( buffer );
						 position += length;
					}
			  } while ( position < chars.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeInt(org.neo4j.io.fs.StoreChannel channel, ByteBuffer buffer, int value) throws java.io.IOException
		 public static void WriteInt( StoreChannel channel, ByteBuffer buffer, int value )
		 {
			  buffer.clear();
			  buffer.putInt( value );
			  buffer.flip();
			  channel.write( buffer );
		 }

		 public static object[] AsArray( object propertyValue )
		 {
			  if ( propertyValue.GetType().IsArray )
			  {
					int length = Array.getLength( propertyValue );
					object[] result = new object[length];
					for ( int i = 0; i < length; i++ )
					{
						 result[i] = Array.get( propertyValue, i );
					}
					return result;
			  }
			  return new object[] { propertyValue };
		 }
	}

}