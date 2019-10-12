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
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using ByteArrayPageCursor = Org.Neo4j.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.extractKeySize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.extractValueSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.putKeyValueSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.DynamicSizeUtil.readKeyValueSize;

	internal class DynamicSizeUtilTest
	{
		 private const int KEY_ONE_BYTE_MAX = 0x1F;
		 private static readonly int _keyTwoByteMin = KEY_ONE_BYTE_MAX + 1;
		 private const int KEY_TWO_BYTE_MAX = 0xFFF;
		 private const int VAL_ONE_BYTE_MIN = 1;
		 private const int VAL_ONE_BYTE_MAX = 0x7F;
		 private static readonly int _valTwoByteMin = VAL_ONE_BYTE_MAX + 1;
		 private const int VAL_TWO_BYTE_MAX = 0x7FFF;

		 private PageCursor _cursor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _cursor = ByteArrayPageCursor.wrap( 8192 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPutAndGetKeyValueSize()
		 internal virtual void ShouldPutAndGetKeyValueSize()
		 {
			  //                           KEY SIZE             | VALUE SIZE      | EXPECTED BYTES
			  ShouldPutAndGetKeyValueSize( 0, 0, 1 );
			  ShouldPutAndGetKeyValueSize( 0, VAL_ONE_BYTE_MIN, 2 );
			  ShouldPutAndGetKeyValueSize( 0, VAL_ONE_BYTE_MAX, 2 );
			  ShouldPutAndGetKeyValueSize( 0, _valTwoByteMin, 3 );
			  ShouldPutAndGetKeyValueSize( 0, VAL_TWO_BYTE_MAX, 3 );
			  ShouldPutAndGetKeyValueSize( KEY_ONE_BYTE_MAX, 0, 1 );
			  ShouldPutAndGetKeyValueSize( KEY_ONE_BYTE_MAX, VAL_ONE_BYTE_MIN, 2 );
			  ShouldPutAndGetKeyValueSize( KEY_ONE_BYTE_MAX, VAL_ONE_BYTE_MAX, 2 );
			  ShouldPutAndGetKeyValueSize( KEY_ONE_BYTE_MAX, _valTwoByteMin, 3 );
			  ShouldPutAndGetKeyValueSize( KEY_ONE_BYTE_MAX, VAL_TWO_BYTE_MAX, 3 );
			  ShouldPutAndGetKeyValueSize( _keyTwoByteMin, 0, 2 );
			  ShouldPutAndGetKeyValueSize( _keyTwoByteMin, VAL_ONE_BYTE_MIN, 3 );
			  ShouldPutAndGetKeyValueSize( _keyTwoByteMin, VAL_ONE_BYTE_MAX, 3 );
			  ShouldPutAndGetKeyValueSize( _keyTwoByteMin, _valTwoByteMin, 4 );
			  ShouldPutAndGetKeyValueSize( _keyTwoByteMin, VAL_TWO_BYTE_MAX, 4 );
			  ShouldPutAndGetKeyValueSize( KEY_TWO_BYTE_MAX, 0, 2 );
			  ShouldPutAndGetKeyValueSize( KEY_TWO_BYTE_MAX, VAL_ONE_BYTE_MIN, 3 );
			  ShouldPutAndGetKeyValueSize( KEY_TWO_BYTE_MAX, VAL_ONE_BYTE_MAX, 3 );
			  ShouldPutAndGetKeyValueSize( KEY_TWO_BYTE_MAX, _valTwoByteMin, 4 );
			  ShouldPutAndGetKeyValueSize( KEY_TWO_BYTE_MAX, VAL_TWO_BYTE_MAX, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPutAndGetKeySize()
		 internal virtual void ShouldPutAndGetKeySize()
		 {
			  //                      KEY SIZE        | EXPECTED BYTES
			  ShouldPutAndGetKeySize( 0, 1 );
			  ShouldPutAndGetKeySize( KEY_ONE_BYTE_MAX, 1 );
			  ShouldPutAndGetKeySize( _keyTwoByteMin, 2 );
			  ShouldPutAndGetKeySize( KEY_TWO_BYTE_MAX, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPreventWritingKeyLargerThanMaxPossible()
		 internal virtual void ShouldPreventWritingKeyLargerThanMaxPossible()
		 {
			  // given
			  int keySize = 0xFFF;

			  // when
			  assertThrows( typeof( System.ArgumentException ), () => putKeyValueSize(_cursor, keySize + 1, 0) );

			  // whereas when size is one less than that
			  ShouldPutAndGetKeyValueSize( keySize, 0, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPreventWritingValueLargerThanMaxPossible()
		 internal virtual void ShouldPreventWritingValueLargerThanMaxPossible()
		 {
			  // given
			  int valueSize = 0x7FFF;

			  // when
			  assertThrows( typeof( System.ArgumentException ), () => putKeyValueSize(_cursor, 1, valueSize + 1) );

			  // whereas when size is one less than that
			  ShouldPutAndGetKeyValueSize( 1, valueSize, 3 );
		 }

		 private void ShouldPutAndGetKeySize( int keySize, int expectedBytes )
		 {
			  int size = PutAndGetKey( keySize );
			  assertEquals( expectedBytes, size );
		 }

		 private int PutAndGetKey( int keySize )
		 {
			  int offsetBefore = _cursor.Offset;
			  DynamicSizeUtil.PutKeySize( _cursor, keySize );
			  int offsetAfter = _cursor.Offset;
			  _cursor.Offset = offsetBefore;
			  long readKeySize = readKeyValueSize( _cursor );
			  assertEquals( keySize, extractKeySize( readKeySize ) );
			  return offsetAfter - offsetBefore;
		 }

		 private void ShouldPutAndGetKeyValueSize( int keySize, int valueSize, int expectedBytes )
		 {
			  int size = PutAndGetKeyValue( keySize, valueSize );
			  assertEquals( expectedBytes, size );
		 }

		 private int PutAndGetKeyValue( int keySize, int valueSize )
		 {
			  int offsetBefore = _cursor.Offset;
			  putKeyValueSize( _cursor, keySize, valueSize );
			  int offsetAfter = _cursor.Offset;
			  _cursor.Offset = offsetBefore;
			  long readKeyValueSize = readKeyValueSize( _cursor );
			  int readKeySize = extractKeySize( readKeyValueSize );
			  int readValueSize = extractValueSize( readKeyValueSize );
			  assertEquals( keySize, readKeySize );
			  assertEquals( valueSize, readValueSize );
			  return offsetAfter - offsetBefore;
		 }
	}

}