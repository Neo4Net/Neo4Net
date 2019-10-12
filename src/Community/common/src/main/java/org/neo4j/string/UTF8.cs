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
namespace Neo4Net.@string
{

	/// <summary>
	/// Utilities for working with UTF8 encoding and decoding.
	/// </summary>
	public class UTF8
	{
		 public static readonly int MinimumSerialisedLengthBytes = Integer.BYTES;

		 public static sbyte[] Encode( string @string )
		 {
			  return @string.GetBytes( Encoding.UTF8 );
		 }

		 public static string Decode( sbyte[] bytes )
		 {
			  return StringHelper.NewString( bytes, StandardCharsets.UTF_8 );
		 }

		 public static string Decode( sbyte[] bytes, int offset, int length )
		 {
			  return StringHelper.NewString( bytes, offset, length, StandardCharsets.UTF_8 );
		 }

		 public static string GetDecodedStringFrom( ByteBuffer source )
		 {
			  // Currently only one key is supported although the data format supports multiple
			  int count = source.Int;
			  int remaining = source.remaining();
			  if ( count > remaining )
			  {
					throw BadStringFormatException( count, remaining );
			  }
			  sbyte[] data = new sbyte[count];
			  source.get( data );
			  return UTF8.Decode( data );
		 }

		 private static System.ArgumentException BadStringFormatException( int count, int remaining )
		 {
			  return new System.ArgumentException( "Bad string format; claims string is " + count + " bytes long, " + "but only " + remaining + " bytes remain in buffer" );
		 }

		 public static void PutEncodedStringInto( string text, ByteBuffer target )
		 {
			  sbyte[] data = Encode( text );
			  target.putInt( data.Length );
			  target.put( data );
		 }

		 public static int ComputeRequiredByteBufferSize( string text )
		 {
			  return Encode( text ).Length + 4;
		 }

		 private UTF8()
		 {
			  throw new AssertionError( "no instance" );
		 }
	}

}