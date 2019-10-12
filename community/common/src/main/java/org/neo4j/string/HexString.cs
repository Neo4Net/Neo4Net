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
namespace Org.Neo4j.@string
{
	/// <summary>
	/// Utility for dealing with hexadecimal strings.
	/// </summary>
	public class HexString
	{
		 private static readonly char[] _hexArray = "0123456789ABCDEF".ToCharArray();

		 private HexString()
		 {
		 }

		 /// <summary>
		 /// Converts a byte array to a hexadecimal string.
		 /// </summary>
		 /// <param name="bytes"> Bytes to be encoded </param>
		 /// <returns> A string of hex characters [0-9A-F] </returns>
		 public static string EncodeHexString( sbyte[] bytes )
		 {
			  char[] hexChars = new char[bytes.Length * 2];
			  for ( int j = 0; j < bytes.Length; j++ )
			  {
					int v = bytes[j] & 0xFF;
					hexChars[j * 2] = _hexArray[( int )( ( uint )v >> 4 )];
					hexChars[j * 2 + 1] = _hexArray[v & 0x0F];
			  }
			  return new string( hexChars );
		 }

		 /// <summary>
		 /// Converts a hexadecimal string to a byte array
		 /// </summary>
		 /// <param name="hexString"> A string of hexadecimal characters [0-9A-Fa-f] to decode </param> </returns>
		 /// <returns> Decoded bytes, or null if the {<param name="hexString">} is not valid </param>
		 public static sbyte[] DecodeHexString( string hexString )
		 {
			  int len = hexString.Length;
			  sbyte[] data = new sbyte[len / 2];
			  for ( int i = 0, j = 0; i < len; i += 2, j++ )
			  {
					int highByte = Character.digit( hexString[i], 16 ) << 4;
					int lowByte = Character.digit( hexString[i + 1], 16 );
					if ( highByte < 0 || lowByte < 0 )
					{
						 return null;
					}
					data[j] = ( sbyte )( highByte + lowByte );
			  }
			  return data;
		 }
	}

}