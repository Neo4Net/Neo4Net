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
namespace Org.Neo4j.Index.@internal.gbptree
{
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.getUnsignedShort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.putUnsignedShort;

	/// <summary>
	/// Gather utility methods for reading and writing individual dynamic sized
	/// keys. It thus define the layout for:
	/// - Key pointer in offset array (K*), 2B
	/// - keyValueSize, 1B-4B
	/// - Key tombstone, first bit in keyValueSize
	/// 
	/// Format of key/value size is dynamic in itself, first byte being:
	/// <pre>
	/// [T,K,V,k,k,k,k,k]
	/// </pre>
	/// If {@code T} is set key is dead.
	/// If {@code K} is set the next byte contains the higher order bits of the key size.
	/// If {@code V} is set there is a value size to be read directly after key size.
	/// This first byte can fit key size < 32 and we only need the second byte if key size is larger.
	/// Together with the second byte we can fit key size < 8192.
	/// 
	/// Byte following key size bytes (second or third byte depending on how many bytes needed for key size):
	/// <pre>
	/// [V,v,v,v,v,v,v,v]
	/// </pre>
	/// If {@code V} is set the next byte contains the higher order bits of the value size.
	/// This first value size byte can fit value size < 128 and with the second byte we can fit value size < 32768.
	/// 
	/// So in total key/value size has six different looks (not including tombstone being set or not set):
	/// <pre>
	/// One byte key, no value
	/// [0,0,0,k,k,k,k,k]
	/// 
	/// One byte key, one byte value
	/// [0,0,1,k,k,k,k,k][0,v,v,v,v,v,v,v]
	/// 
	/// One byte key, two byte value
	/// [0,0,1,k,k,k,k,k][1,v,v,v,v,v,v,v][v,v,v,v,v,v,v,v]
	/// 
	/// Two byte key, no value
	/// [0,1,0,k,k,k,k,k][0,k,k,k,k,k,k,k]
	/// 
	/// Two byte key, one byte value
	/// [0,1,1,k,k,k,k,k][0,k,k,k,k,k,k,k][0,v,v,v,v,v,v,v]
	/// 
	/// Two byte key, two byte value
	/// [0,1,1,k,k,k,k,k][0,k,k,k,k,k,k,k][1,v,v,v,v,v,v,v][v,v,v,v,v,v,v,v]
	/// </pre>
	/// This key/value size format is used, both for leaves and internal nodes even though internal nodes can never have values.
	/// 
	/// The most significant key bit in the second byte (shown as 0) is not needed for the discrete key sizes for our 8k page size
	/// and is to be considered reserved for future use.
	/// 
	/// Relative layout of key and key_value
	/// KeyOffset points to the exact offset where key entry or key_value entry
	/// can be read.
	/// key entry - [keyValueSize 1B-2B|actualKey]
	/// key_value entry - [keyValueSize 1B-4B|actualKey|actualValue]
	/// 
	/// Tombstone
	/// First bit in keyValueSize is used as a tombstone, set to 1 if key is dead.
	/// </summary>
	public class DynamicSizeUtil
	{
		 internal const int SIZE_OFFSET = 2;
		 internal const int SIZE_KEY_SIZE = 2;
		 internal const int SIZE_VALUE_SIZE = 2;
		 internal static readonly int SizeTotalOverhead = SIZE_OFFSET + SIZE_KEY_SIZE + SIZE_VALUE_SIZE;

		 private const int FLAG_FIRST_BYTE_TOMBSTONE = 0x80;
		 private const long FLAG_READ_TOMBSTONE = unchecked( ( long )0x80000000_00000000L );
		 // mask for one-byte key size to map to the k's in [_,_,_,k,k,k,k,k]
		 internal const int MASK_ONE_BYTE_KEY_SIZE = 0x1F;
		 // max two-byte key size to map to the k's in [_,_,_,k,k,k,k,k][_,k,k,k,k,k,k,k]
		 internal const int MAX_TWO_BYTE_KEY_SIZE = 0xFFF;
		 // mask for one-byte value size to map to the v's in [_,v,v,v,v,v,v,v]
		 internal const int MASK_ONE_BYTE_VALUE_SIZE = 0x7F;
		 // max two-byte value size to map to the v's in [_,v,v,v,v,v,v,v][v,v,v,v,v,v,v,v]
		 private const int MAX_TWO_BYTE_VALUE_SIZE = 0x7FFF;
		 private const int FLAG_HAS_VALUE_SIZE = 0x20;
		 private const int FLAG_ADDITIONAL_KEY_SIZE = 0x40;
		 private const int FLAG_ADDITIONAL_VALUE_SIZE = 0x80;
		 private const int SHIFT_LSB_KEY_SIZE = 5;
		 private const int SHIFT_LSB_VALUE_SIZE = 7;

		 internal static void PutKeyOffset( PageCursor cursor, int keyOffset )
		 {
			  putUnsignedShort( cursor, keyOffset );
		 }

		 internal static int ReadKeyOffset( PageCursor cursor )
		 {
			  return getUnsignedShort( cursor );
		 }

		 internal static void PutKeySize( PageCursor cursor, int keySize )
		 {
			  PutKeyValueSize( cursor, keySize, 0 );
		 }

		 public static void PutKeyValueSize( PageCursor cursor, int keySize, int valueSize )
		 {
			  bool hasAdditionalKeySize = keySize > MASK_ONE_BYTE_KEY_SIZE;
			  bool hasValueSize = valueSize > 0;

			  {
			  // Key size
					sbyte firstByte = ( sbyte )( keySize & MASK_ONE_BYTE_KEY_SIZE ); // Least significant 5 bits
					if ( hasAdditionalKeySize )
					{
						 firstByte |= ( sbyte )FLAG_ADDITIONAL_KEY_SIZE;
						 if ( keySize > MAX_TWO_BYTE_KEY_SIZE )
						 {
							  throw new System.ArgumentException( format( "Max supported key size is %d, but tried to store key of size %d. Please see index documentation for limitations.", MAX_TWO_BYTE_KEY_SIZE, keySize ) );
						 }
					}
					if ( hasValueSize )
					{
						 firstByte |= ( sbyte )FLAG_HAS_VALUE_SIZE;
					}
					cursor.PutByte( firstByte );

					if ( hasAdditionalKeySize )
					{
						 // Assuming no key size larger than 4k
						 cursor.PutByte( ( sbyte )( keySize >> SHIFT_LSB_KEY_SIZE ) );
					}
			  }

			  {
			  // Value size
					if ( hasValueSize )
					{
						 bool needsAdditionalValueSize = valueSize > MASK_ONE_BYTE_VALUE_SIZE;
						 sbyte firstByte = ( sbyte )( valueSize & MASK_ONE_BYTE_VALUE_SIZE ); // Least significant 7 bits
						 if ( needsAdditionalValueSize )
						 {
							  if ( valueSize > MAX_TWO_BYTE_VALUE_SIZE )
							  {
									throw new System.ArgumentException( format( "Max supported value size is %d, but tried to store value of size %d. Please see index documentation for limitations.", MAX_TWO_BYTE_VALUE_SIZE, valueSize ) );
							  }
							  firstByte |= ( sbyte )FLAG_ADDITIONAL_VALUE_SIZE;
						 }
						 cursor.PutByte( firstByte );

						 if ( needsAdditionalValueSize )
						 {
							  // Assuming no value size larger than 16k
							  cursor.PutByte( ( sbyte )( valueSize >> SHIFT_LSB_VALUE_SIZE ) );
						 }
					}
			  }
		 }

		 public static long ReadKeyValueSize( PageCursor cursor )
		 {
			  sbyte firstByte = cursor.Byte;
			  bool hasTombstone = hasTombstone( firstByte );
			  bool hasAdditionalKeySize = ( firstByte & FLAG_ADDITIONAL_KEY_SIZE ) != 0;
			  bool hasValueSize = ( firstByte & FLAG_HAS_VALUE_SIZE ) != 0;
			  int keySizeLsb = firstByte & MASK_ONE_BYTE_KEY_SIZE;
			  long keySize;
			  if ( hasAdditionalKeySize )
			  {
					int keySizeMsb = cursor.Byte & 0xFF;
					keySize = ( keySizeMsb << SHIFT_LSB_KEY_SIZE ) | keySizeLsb;
			  }
			  else
			  {
					keySize = keySizeLsb;
			  }

			  long valueSize;
			  if ( hasValueSize )
			  {
					sbyte firstValueByte = cursor.Byte;
					int valueSizeLsb = firstValueByte & MASK_ONE_BYTE_VALUE_SIZE;
					bool hasAdditionalValueSize = ( firstValueByte & FLAG_ADDITIONAL_VALUE_SIZE ) != 0;
					if ( hasAdditionalValueSize )
					{
						 int valueSizeMsb = cursor.Byte & 0xFF;
						 valueSize = ( valueSizeMsb << SHIFT_LSB_VALUE_SIZE ) | valueSizeLsb;
					}
					else
					{
						 valueSize = valueSizeLsb;
					}
			  }
			  else
			  {
					valueSize = 0;
			  }

			  return ( hasTombstone ? FLAG_READ_TOMBSTONE : 0 ) | ( keySize << ( sizeof( int ) * 8 ) ) | valueSize;
		 }

		 public static int ExtractValueSize( long keyValueSize )
		 {
			  return ( int ) keyValueSize;
		 }

		 public static int ExtractKeySize( long keyValueSize )
		 {
			  return ( int )( ( long )( ( ulong )( keyValueSize & ~FLAG_READ_TOMBSTONE ) >> ( sizeof( int ) * 8 ) ) );
		 }

		 public static int GetOverhead( int keySize, int valueSize )
		 {
			  return 1 + ( keySize > MASK_ONE_BYTE_KEY_SIZE ? 1 : 0 ) + ( valueSize > 0 ? 1 : 0 ) + ( valueSize > MASK_ONE_BYTE_VALUE_SIZE ? 1 : 0 );
		 }

		 internal static bool ExtractTombstone( long keyValueSize )
		 {
			  return ( keyValueSize & FLAG_READ_TOMBSTONE ) != 0;
		 }

		 /// <summary>
		 /// Put a tombstone into key size. </summary>
		 /// <param name="cursor"> on offset to key size where tombstone should be put. </param>
		 internal static void PutTombstone( PageCursor cursor )
		 {
			  int offset = cursor.Offset;
			  sbyte firstByte = cursor.Byte;
			  firstByte = WithTombstoneFlag( firstByte );
			  cursor.Offset = offset;
			  cursor.PutByte( firstByte );
		 }

		 /// <summary>
		 /// Check read key size for tombstone. </summary>
		 /// <returns> True if read key size has tombstone. </returns>
		 private static bool HasTombstone( sbyte firstKeySizeByte )
		 {
			  return ( firstKeySizeByte & FLAG_FIRST_BYTE_TOMBSTONE ) != 0;
		 }

		 private static sbyte WithTombstoneFlag( sbyte firstByte )
		 {
			  assert( firstByte & FLAG_FIRST_BYTE_TOMBSTONE ) == 0 : "First key size byte " + firstByte + " is too large to fit tombstone.";
			  return ( sbyte )( firstByte | FLAG_FIRST_BYTE_TOMBSTONE );
		 }
	}

}