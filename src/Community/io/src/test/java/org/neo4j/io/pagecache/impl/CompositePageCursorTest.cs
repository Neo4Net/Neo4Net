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
namespace Neo4Net.Io.pagecache.impl
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.matchers.ByteArrayMatcher.byteArray;

	internal class CompositePageCursorTest
	{
		 private const int PAGE_SIZE = 16;
		 private StubPageCursor _first;
		 private StubPageCursor _second;
		 private sbyte[] _bytes = new sbyte[4];

		 private StubPageCursor GeneratePage( int initialPageId, int pageSize, int initialValue )
		 {
			  StubPageCursor cursor = new StubPageCursor( initialPageId, pageSize );
			  for ( int i = 0; i < pageSize; i++ )
			  {
					cursor.PutByte( i, ( sbyte )( initialValue + i ) );
			  }
			  return cursor;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _first = GeneratePage( 0, PAGE_SIZE, 0xA0 );
			  _second = GeneratePage( 2, PAGE_SIZE + 8, 0xB0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteMustHitFirstCursorBeforeFlip()
		 internal virtual void getByteMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 1 );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xA0 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteMustHitSecondCursorAfterFlip()
		 internal virtual void getByteMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 1 );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xA0 ) ) );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xB0 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteMustRespectOffsetIntoFirstCursor()
		 internal virtual void getByteMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 1 );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xA1 ) ) );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xB0 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteMustRespectOffsetIntoSecondCursor()
		 internal virtual void getByteMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 1 );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xA0 ) ) );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xB1 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteMustHitFirstCursorBeforeFlip()
		 internal virtual void PutByteMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 1 );
			  c.PutByte( ( sbyte ) 1 );
			  c.Offset = 0;
			  assertThat( c.Byte, @is( ( sbyte ) 1 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteMustHitSecondCursorAfterFlip()
		 internal virtual void PutByteMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 1 );
			  c.PutByte( ( sbyte ) 1 );
			  c.PutByte( ( sbyte ) 2 );
			  c.Offset = 1;
			  assertThat( c.Byte, @is( ( sbyte ) 2 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteMustRespectOffsetIntoFirstCursor()
		 internal virtual void PutByteMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 1 );
			  c.PutByte( ( sbyte ) 1 );
			  assertThat( _first.getByte( 1 ), @is( ( sbyte ) 1 ) );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xB0 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteMustRespectOffsetIntoSecondCursor()
		 internal virtual void PutByteMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 2 );
			  c.PutByte( ( sbyte ) 1 );
			  c.PutByte( ( sbyte ) 2 );
			  assertThat( _second.getByte( 1 ), @is( ( sbyte ) 2 ) );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xB2 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteWithOffsetMustHitCorrectCursors()
		 internal virtual void getByteWithOffsetMustHitCorrectCursors()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor c = CompositePageCursor.Compose( _first, 1 + 1, _second, 1 );
			  assertThat( c.GetByte( 1 ), @is( unchecked( ( sbyte ) 0xA2 ) ) );
			  assertThat( c.GetByte( 1 + 1 ), @is( unchecked( ( sbyte ) 0xB2 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteWithOffsetMustHitCorrectCursors()
		 internal virtual void PutByteWithOffsetMustHitCorrectCursors()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor c = CompositePageCursor.Compose( _first, 2 * 1, _second, 2 * 1 );
			  c.PutByte( 1, ( sbyte ) 1 );
			  c.PutByte( 1 + 1, ( sbyte ) 2 );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xA1 ) ) );
			  assertThat( c.Byte, @is( ( sbyte ) 1 ) );
			  assertThat( c.Byte, @is( ( sbyte ) 2 ) );
			  assertThat( c.Byte, @is( unchecked( ( sbyte ) 0xB3 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortMustHitFirstCursorBeforeFlip()
		 internal virtual void getShortMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 2, _second, 2 );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xA0A1 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortMustHitSecondCursorAfterFlip()
		 internal virtual void getShortMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 2, _second, 2 );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xA0A1 ) ) );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xB0B1 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortMustRespectOffsetIntoFirstCursor()
		 internal virtual void getShortMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 2, _second, 2 );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xA1A2 ) ) );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xB0B1 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortMustRespectOffsetIntoSecondCursor()
		 internal virtual void getShortMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 2, _second, 2 );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xA0A1 ) ) );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xB1B2 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortMustHitFirstCursorBeforeFlip()
		 internal virtual void PutShortMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 2, _second, 2 );
			  c.PutShort( ( short ) 1 );
			  c.Offset = 0;
			  assertThat( c.Short, @is( ( short ) 1 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortMustHitSecondCursorAfterFlip()
		 internal virtual void PutShortMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 2, _second, 2 );
			  c.PutShort( ( short ) 1 );
			  c.PutShort( ( short ) 2 );
			  c.Offset = 2;
			  assertThat( c.Short, @is( ( short ) 2 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortMustRespectOffsetIntoFirstCursor()
		 internal virtual void PutShortMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 2, _second, 2 );
			  c.PutShort( ( short ) 1 );
			  assertThat( _first.getShort( 1 ), @is( ( short ) 1 ) );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xB0B1 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortMustRespectOffsetIntoSecondCursor()
		 internal virtual void PutShortMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 2, _second, 4 );
			  c.PutShort( ( short ) 1 );
			  c.PutShort( ( short ) 2 );
			  assertThat( _second.getShort( 1 ), @is( ( short ) 2 ) );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xB3B4 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortWithOffsetMustHitCorrectCursors()
		 internal virtual void getShortWithOffsetMustHitCorrectCursors()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor c = CompositePageCursor.Compose( _first, 1 + 2, _second, 2 );
			  assertThat( c.GetShort( 1 ), @is( unchecked( ( short ) 0xA2A3 ) ) );
			  assertThat( c.GetShort( 1 + 2 ), @is( unchecked( ( short ) 0xB2B3 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortWithOffsetMustHitCorrectCursors()
		 internal virtual void PutShortWithOffsetMustHitCorrectCursors()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor c = CompositePageCursor.Compose( _first, 2 * 2, _second, 2 * 2 );
			  c.PutShort( 2, ( short ) 1 );
			  c.PutShort( 2 + 2, ( short ) 2 );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xA1A2 ) ) );
			  assertThat( c.Short, @is( ( short ) 1 ) );
			  assertThat( c.Short, @is( ( short ) 2 ) );
			  assertThat( c.Short, @is( unchecked( ( short ) 0xB4B5 ) ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntMustHitFirstCursorBeforeFlip()
		 internal virtual void getIntMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  assertThat( c.Int, @is( 0xA0A1A2A3 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntMustHitSecondCursorAfterFlip()
		 internal virtual void getIntMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  assertThat( c.Int, @is( 0xA0A1A2A3 ) );
			  assertThat( c.Int, @is( 0xB0B1B2B3 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntMustRespectOffsetIntoFirstCursor()
		 internal virtual void getIntMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  assertThat( c.Int, @is( 0xA1A2A3A4 ) );
			  assertThat( c.Int, @is( 0xB0B1B2B3 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntMustRespectOffsetIntoSecondCursor()
		 internal virtual void getIntMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  assertThat( c.Int, @is( 0xA0A1A2A3 ) );
			  assertThat( c.Int, @is( 0xB1B2B3B4 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntMustHitFirstCursorBeforeFlip()
		 internal virtual void PutIntMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  c.PutInt( 1 );
			  c.Offset = 0;
			  assertThat( c.Int, @is( 1 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntMustHitSecondCursorAfterFlip()
		 internal virtual void PutIntMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  c.PutInt( 1 );
			  c.PutInt( 2 );
			  c.Offset = 4;
			  assertThat( c.Int, @is( 2 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntMustRespectOffsetIntoFirstCursor()
		 internal virtual void PutIntMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  c.PutInt( 1 );
			  assertThat( _first.getInt( 1 ), @is( 1 ) );
			  assertThat( c.Int, @is( 0xB0B1B2B3 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntMustRespectOffsetIntoSecondCursor()
		 internal virtual void PutIntMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 8 );
			  c.PutInt( 1 );
			  c.PutInt( 2 );
			  assertThat( _second.getInt( 1 ), @is( 2 ) );
			  assertThat( c.Int, @is( 0xB5B6B7B8 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntWithOffsetMustHitCorrectCursors()
		 internal virtual void getIntWithOffsetMustHitCorrectCursors()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor c = CompositePageCursor.Compose( _first, 1 + 4, _second, 4 );
			  assertThat( c.GetInt( 1 ), @is( 0xA2A3A4A5 ) );
			  assertThat( c.GetInt( 1 + 4 ), @is( 0xB2B3B4B5 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntWithOffsetMustHitCorrectCursors()
		 internal virtual void PutIntWithOffsetMustHitCorrectCursors()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor c = CompositePageCursor.Compose( _first, 2 * 4, _second, 2 * 4 );
			  c.PutInt( 4, 1 );
			  c.PutInt( 4 + 4, 2 );
			  assertThat( c.Int, @is( 0xA1A2A3A4 ) );
			  assertThat( c.Int, @is( 1 ) );
			  assertThat( c.Int, @is( 2 ) );
			  assertThat( c.Int, @is( 0xB6B7B8B9 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongMustHitFirstCursorBeforeFlip()
		 internal virtual void getLongMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 8, _second, 8 );
			  assertThat( c.Long, @is( 0xA0A1A2A3A4A5A6A7L ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongMustHitSecondCursorAfterFlip()
		 internal virtual void getLongMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 8, _second, 8 );
			  assertThat( c.Long, @is( 0xA0A1A2A3A4A5A6A7L ) );
			  assertThat( c.Long, @is( 0xB0B1B2B3B4B5B6B7L ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongMustRespectOffsetIntoFirstCursor()
		 internal virtual void getLongMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 8, _second, 8 );
			  assertThat( c.Long, @is( 0xA1A2A3A4A5A6A7A8L ) );
			  assertThat( c.Long, @is( 0xB0B1B2B3B4B5B6B7L ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongMustRespectOffsetIntoSecondCursor()
		 internal virtual void getLongMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 8, _second, 8 );
			  assertThat( c.Long, @is( 0xA0A1A2A3A4A5A6A7L ) );
			  assertThat( c.Long, @is( 0xB1B2B3B4B5B6B7B8L ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongMustHitFirstCursorBeforeFlip()
		 internal virtual void PutLongMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 8, _second, 8 );
			  c.PutLong( ( long ) 1 );
			  c.Offset = 0;
			  assertThat( c.Long, @is( ( long ) 1 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongMustHitSecondCursorAfterFlip()
		 internal virtual void PutLongMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 8, _second, 8 );
			  c.PutLong( ( long ) 1 );
			  c.PutLong( ( long ) 2 );
			  c.Offset = 8;
			  assertThat( c.Long, @is( ( long ) 2 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongMustRespectOffsetIntoFirstCursor()
		 internal virtual void PutLongMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 8, _second, 8 );
			  c.PutLong( ( long ) 1 );
			  assertThat( _first.getLong( 1 ), @is( ( long ) 1 ) );
			  assertThat( c.Long, @is( 0xB0B1B2B3B4B5B6B7L ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongMustRespectOffsetIntoSecondCursor()
		 internal virtual void PutLongMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 8, _second, PAGE_SIZE );
			  c.PutLong( ( long ) 1 );
			  c.PutLong( ( long ) 2 );
			  assertThat( _second.getLong( 1 ), @is( ( long ) 2 ) );
			  assertThat( c.Long, @is( 0xB9BABBBCBDBEBFC0L ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongWithOffsetMustHitCorrectCursors()
		 internal virtual void getLongWithOffsetMustHitCorrectCursors()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor c = CompositePageCursor.Compose( _first, 1 + 8, _second, 8 );
			  assertThat( c.GetLong( 1 ), @is( 0xA2A3A4A5A6A7A8A9L ) );
			  assertThat( c.GetLong( 1 + 8 ), @is( 0xB2B3B4B5B6B7B8B9L ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongWithOffsetMustHitCorrectCursors()
		 internal virtual void PutLongWithOffsetMustHitCorrectCursors()
		 {
			  _first = GeneratePage( 0, PAGE_SIZE + 8, 0xA0 );
			  _second = GeneratePage( 0, PAGE_SIZE + 8, 0xC0 );
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor c = CompositePageCursor.Compose( _first, 2 * 8, _second, 2 * 8 );
			  c.PutLong( 8, ( long ) 1 );
			  c.PutLong( 8 + 8, ( long ) 2 );
			  assertThat( c.Long, @is( 0xA1A2A3A4A5A6A7A8L ) );
			  assertThat( c.Long, @is( ( long ) 1 ) );
			  assertThat( c.Long, @is( ( long ) 2 ) );
			  assertThat( c.Long, @is( 0xCACBCCCDCECFD0D1L ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getBytesMustHitFirstCursorBeforeFlip()
		 internal virtual void getBytesMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xA0, 0xA1, 0xA2, 0xA3 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getBytesMustHitSecondCursorAfterFlip()
		 internal virtual void getBytesMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xA0, 0xA1, 0xA2, 0xA3 ) );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xB0, 0xB1, 0xB2, 0xB3 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getBytesMustRespectOffsetIntoFirstCursor()
		 internal virtual void getBytesMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xA1, 0xA2, 0xA3, 0xA4 ) );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xB0, 0xB1, 0xB2, 0xB3 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getBytesMustRespectOffsetIntoSecondCursor()
		 internal virtual void getBytesMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xA0, 0xA1, 0xA2, 0xA3 ) );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xB1, 0xB2, 0xB3, 0xB4 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putBytesMustHitFirstCursorBeforeFlip()
		 internal virtual void PutBytesMustHitFirstCursorBeforeFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 4, _second, 4 );
			  c.PutBytes( new sbyte[]{ 1, 2, 3, 4 } );
			  c.Offset = 0;
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 1, 2, 3, 4 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putBytesMustHitSecondCursorAfterFlip()
		 internal virtual void PutBytesMustHitSecondCursorAfterFlip()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 4 );
			  c.PutBytes( new sbyte[]{ 1 } );
			  c.PutBytes( new sbyte[]{ 2 } );
			  c.Offset = 1;
			  c.GetBytes( _bytes );
			  assertThat( Arrays.copyOfRange( _bytes, 0, 1 ), byteArray( 2 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putBytesMustRespectOffsetIntoFirstCursor()
		 internal virtual void PutBytesMustRespectOffsetIntoFirstCursor()
		 {
			  _first.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 4 );
			  c.PutBytes( new sbyte[]{ 1 } );
			  _first.Offset = 1;
			  _first.getBytes( _bytes );
			  assertThat( Arrays.copyOfRange( _bytes, 0, 1 ), byteArray( 1 ) );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xB0, 0xB1, 0xB2, 0xB3 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putBytesMustRespectOffsetIntoSecondCursor()
		 internal virtual void PutBytesMustRespectOffsetIntoSecondCursor()
		 {
			  _second.Offset = 1;
			  PageCursor c = CompositePageCursor.Compose( _first, 1, _second, 8 );
			  c.PutBytes( new sbyte[]{ 1 } );
			  c.PutBytes( new sbyte[]{ 2 } );
			  _second.Offset = 1;
			  _second.getBytes( _bytes );
			  assertThat( Arrays.copyOfRange( _bytes, 0, 1 ), byteArray( 2 ) );
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xB5, 0xB6, 0xB7, 0xB8 ) );
			  assertFalse( c.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void overlappingGetAccess()
		 internal virtual void OverlappingGetAccess()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  c.Offset = PAGE_SIZE - 2;
			  assertThat( c.Int, @is( 0xAEAFB0B1 ) );
			  c.Offset = PAGE_SIZE - 1;
			  assertThat( c.Short, @is( unchecked( ( short ) 0xAFB0 ) ) );
			  c.Offset = PAGE_SIZE - 4;
			  assertThat( c.Long, @is( 0xACADAEAFB0B1B2B3L ) );
			  c.Offset = PAGE_SIZE - 2;
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0xAE, 0xAF, 0xB0, 0xB1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void overlappingOffsetGetAccess()
		 internal virtual void OverlappingOffsetGetAccess()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  assertThat( c.GetInt( PAGE_SIZE - 2 ), @is( 0xAEAFB0B1 ) );
			  assertThat( c.GetShort( PAGE_SIZE - 1 ), @is( unchecked( ( short ) 0xAFB0 ) ) );
			  assertThat( c.GetLong( PAGE_SIZE - 4 ), @is( 0xACADAEAFB0B1B2B3L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void overlappingPutAccess()
		 internal virtual void OverlappingPutAccess()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  c.Offset = PAGE_SIZE - 2;
			  c.PutInt( 0x01020304 );
			  c.Offset = PAGE_SIZE - 2;
			  assertThat( c.Int, @is( 0x01020304 ) );

			  c.Offset = PAGE_SIZE - 1;
			  c.PutShort( ( short ) 0x0102 );
			  c.Offset = PAGE_SIZE - 1;
			  assertThat( c.Short, @is( ( short ) 0x0102 ) );

			  c.Offset = PAGE_SIZE - 4;
			  c.PutLong( 0x0102030405060708L );
			  c.Offset = PAGE_SIZE - 4;
			  assertThat( c.Long, @is( 0x0102030405060708L ) );

			  c.Offset = PAGE_SIZE - 2;
			  for ( int i = 0; i < _bytes.Length; i++ )
			  {
					_bytes[i] = ( sbyte )( i + 1 );
			  }
			  c.PutBytes( _bytes );
			  c.Offset = PAGE_SIZE - 2;
			  c.GetBytes( _bytes );
			  assertThat( _bytes, byteArray( 0x01, 0x02, 0x03, 0x04 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void overlappingOffsetPutAccess()
		 internal virtual void OverlappingOffsetPutAccess()
		 {
			  PageCursor c = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  c.PutInt( PAGE_SIZE - 2, 0x01020304 );
			  assertThat( c.GetInt( PAGE_SIZE - 2 ), @is( 0x01020304 ) );

			  c.PutShort( PAGE_SIZE - 1, ( short ) 0x0102 );
			  assertThat( c.GetShort( PAGE_SIZE - 1 ), @is( ( short ) 0x0102 ) );

			  c.PutLong( PAGE_SIZE - 4, 0x0102030405060708L );
			  assertThat( c.GetLong( PAGE_SIZE - 4 ), @is( 0x0102030405060708L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeBothCursorsOnClose()
		 internal virtual void CloseBothCursorsOnClose()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.Close();

			  assertTrue( _first.Closed );
			  assertTrue( _second.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextIsNotSupportedOperation()
		 internal virtual void NextIsNotSupportedOperation()
		 {
			  assertThrows(typeof(System.NotSupportedException), () =>
			  {
				PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
				pageCursor.Next();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextWithPageIdIsNotSupportedOperation()
		 internal virtual void NextWithPageIdIsNotSupportedOperation()
		 {
			  assertThrows(typeof(System.NotSupportedException), () =>
			  {
				PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
				pageCursor.Next( 12 );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rewindCompositeCursor()
		 internal virtual void RewindCompositeCursor()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );

			  pageCursor.Long;
			  pageCursor.Long;
			  pageCursor.Long;

			  pageCursor.Rewind();

			  assertEquals( 0, pageCursor.Offset );
			  assertEquals( 1, _first.Offset );
			  assertEquals( 2, _second.Offset );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void markCompositeCursor()
		 public virtual void MarkCompositeCursor()
		 {
			  // GIVEN
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );

			  _first.Byte;
			  _second.Long;

			  int firstMark = _first.Offset;
			  int secondMark = _second.Offset;
			  pageCursor.Mark();

			  _first.Byte;
			  _second.Long;

			  assertNotEquals( firstMark, _first.Offset );
			  assertNotEquals( secondMark, _second.Offset );

			  // WHEN
			  pageCursor.SetOffsetToMark();

			  // THEN
			  assertEquals( firstMark, _first.Offset );
			  assertEquals( secondMark, _second.Offset );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getOffsetMustReturnOffsetIntoView()
		 internal virtual void getOffsetMustReturnOffsetIntoView()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.Long;
			  assertThat( pageCursor.Offset, @is( 8 ) );
			  pageCursor.Long;
			  pageCursor.Long;
			  assertThat( pageCursor.Offset, @is( 24 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void setOffsetMustSetOffsetIntoView()
		 internal virtual void SetOffsetMustSetOffsetIntoView()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.Offset = 13;
			  assertThat( _first.Offset, @is( 13 ) );
			  assertThat( _second.Offset, @is( 0 ) );
			  pageCursor.Offset = 18; // beyond first page cursor
			  assertThat( _first.Offset, @is( PAGE_SIZE ) );
			  assertThat( _second.Offset, @is( 18 - PAGE_SIZE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void raisingOutOfBoundsFlagMustRaiseOutOfBoundsFlag()
		 internal virtual void RaisingOutOfBoundsFlagMustRaiseOutOfBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.RaiseOutOfBounds();
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void currentPageSizeIsUnsupported()
		 internal virtual void CurrentPageSizeIsUnsupported()
		 {
			  assertThrows(typeof(System.NotSupportedException), () =>
			  {
				PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
				pageCursor.CurrentPageSize;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageIdEqualFirstCursorPageIdBeforeFlip()
		 internal virtual void PageIdEqualFirstCursorPageIdBeforeFlip()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  assertEquals( _first.CurrentPageId, pageCursor.CurrentPageId );

			  pageCursor.Long;
			  assertEquals( _first.CurrentPageId, pageCursor.CurrentPageId );

			  pageCursor.Long;
			  assertNotEquals( _first.CurrentPageId, pageCursor.CurrentPageId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageIdEqualSecondCursorPageIdAfterFlip()
		 internal virtual void PageIdEqualSecondCursorPageIdAfterFlip()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  assertNotEquals( _second.CurrentPageId, pageCursor.CurrentPageId );

			  pageCursor.Long;
			  assertNotEquals( _second.CurrentPageId, pageCursor.CurrentPageId );

			  pageCursor.Long;
			  assertEquals( _second.CurrentPageId, pageCursor.CurrentPageId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void retryShouldCheckAndResetBothCursors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RetryShouldCheckAndResetBothCursors()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );

			  assertFalse( pageCursor.ShouldRetry() );

			  _first.NeedsRetry = true;
			  assertTrue( pageCursor.ShouldRetry() );

			  _first.NeedsRetry = false;
			  assertFalse( pageCursor.ShouldRetry() );

			  _second.NeedsRetry = true;
			  assertTrue( pageCursor.ShouldRetry() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void retryMustResetOffsetsInBothCursors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RetryMustResetOffsetsInBothCursors()
		 {
			  _first.Offset = 1;
			  _second.Offset = 2;
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, 8, _second, 8 );

			  pageCursor.Offset = 5;
			  _first.Offset = 3;
			  _second.Offset = 4;
			  _first.NeedsRetry = true;
			  pageCursor.ShouldRetry();
			  assertThat( _first.Offset, @is( 1 ) );
			  assertThat( _second.Offset, @is( 2 ) );
			  assertThat( pageCursor.Offset, @is( 0 ) );

			  pageCursor.Offset = 5;
			  _first.Offset = 3;
			  _second.Offset = 4;
			  _first.NeedsRetry = false;
			  _second.NeedsRetry = true;
			  pageCursor.ShouldRetry();
			  assertThat( _first.Offset, @is( 1 ) );
			  assertThat( _second.Offset, @is( 2 ) );
			  assertThat( pageCursor.Offset, @is( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void retryMustClearTheOutOfBoundsFlags() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RetryMustClearTheOutOfBoundsFlags()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.raiseOutOfBounds();
			  _second.raiseOutOfBounds();
			  pageCursor.RaiseOutOfBounds();
			  _first.NeedsRetry = true;
			  pageCursor.ShouldRetry();
			  assertFalse( _first.checkAndClearBoundsFlag() );
			  assertFalse( _second.checkAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCompositeBoundsFlagMustClearFirstBoundsFlag()
		 internal virtual void CheckAndClearCompositeBoundsFlagMustClearFirstBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.raiseOutOfBounds();
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( _first.checkAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCompositeBoundsFlagMustClearSecondBoundsFlag()
		 internal virtual void CheckAndClearCompositeBoundsFlagMustClearSecondBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _second.raiseOutOfBounds();
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( _second.checkAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void composeMustNotThrowIfFirstLengthExpandsBeyondFirstPage()
		 internal virtual void ComposeMustNotThrowIfFirstLengthExpandsBeyondFirstPage()
		 {
			  CompositePageCursor.Compose( _first, int.MaxValue, _second, PAGE_SIZE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void composeMustNotThrowIfSecondLengthExpandsBeyondSecondPage()
		 internal virtual void ComposeMustNotThrowIfSecondLengthExpandsBeyondSecondPage()
		 {
			  CompositePageCursor.Compose( _first, PAGE_SIZE, _second, int.MaxValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void compositeCursorDoesNotSupportCopyTo()
		 internal virtual void CompositeCursorDoesNotSupportCopyTo()
		 {
			  assertThrows(typeof(System.NotSupportedException), () =>
			  {
				PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
				pageCursor.CopyTo( 0, new StubPageCursor( 0, 7 ), 89, 6 );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getByteBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.Byte;
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getByteOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.GetByte( i );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutByteBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutByte( ( sbyte ) 1 );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutByteOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutByte( i, ( sbyte ) 1 );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 internal virtual void getByteOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.GetByte( -1 );
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 internal virtual void PutByteOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.PutByte( -1, ( sbyte ) 1 );
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getShortBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.Short;
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getShortOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.GetShort( i );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutShortBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutShort( ( short ) 1 );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutShortOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutShort( i, ( short ) 1 );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 internal virtual void getShortOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.GetShort( -1 );
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 internal virtual void PutShortOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.PutShort( -1, ( short ) 1 );
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getIntBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.Int;
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getIntOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.GetInt( i );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutIntBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutInt( 1 );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutIntOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutInt( i, 1 );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 internal virtual void getIntOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.GetInt( -1 );
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 internal virtual void PutIntOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.PutInt( -1, 1 );
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getLongBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.Long;
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getLongOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.GetLong( i );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutLongBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutLong( ( long ) 1 );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutLongOffsetBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutLong( i, ( long ) 1 );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 internal virtual void getLongOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.GetLong( -1 );
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 internal virtual void PutLongOffsetBeforeFirstPageMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  pageCursor.PutLong( -1, ( long ) 1 );
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteArrayBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void getByteArrayBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.GetBytes( _bytes );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteArrayBeyondEndOfViewMustRaiseBoundsFlag()
		 internal virtual void PutByteArrayBeyondEndOfViewMustRaiseBoundsFlag()
		 {
			  PageCursor pageCursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  for ( int i = 0; i < 3 * PAGE_SIZE; i++ )
			  {
					pageCursor.PutBytes( _bytes );
			  }
			  assertTrue( pageCursor.CheckAndClearBoundsFlag() );
			  assertFalse( pageCursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void setCursorErrorMustApplyToCursorAtCurrentOffset()
		 internal virtual void SetCursorErrorMustApplyToCursorAtCurrentOffset()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  string firstMsg = "first boo";
			  string secondMsg = "second boo";

			  cursor.CursorException = firstMsg;
			  assertFalse( cursor.CheckAndClearBoundsFlag() );
			  try
			  {
					_first.checkAndClearCursorException();
					fail( "first checkAndClearCursorError should have thrown" );
			  }
			  catch ( CursorException e )
			  {
					assertThat( e.Message, @is( firstMsg ) );
			  }

			  cursor.Offset = PAGE_SIZE;
			  cursor.CursorException = secondMsg;
			  assertFalse( cursor.CheckAndClearBoundsFlag() );
			  try
			  {
					_second.checkAndClearCursorException();
					fail( "second checkAndClearCursorError should have thrown" );
			  }
			  catch ( CursorException e )
			  {
					assertThat( e.Message, @is( secondMsg ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorErrorMustNotThrowIfNoErrorsAreSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CheckAndClearCursorErrorMustNotThrowIfNoErrorsAreSet()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  cursor.CheckAndClearCursorException();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorErrorMustThrowIfFirstCursorHasError()
		 internal virtual void CheckAndClearCursorErrorMustThrowIfFirstCursorHasError()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.CursorException = "boo";
			  try
			  {
					cursor.CheckAndClearCursorException();
					fail( "composite cursor checkAndClearCursorError should have thrown" );
			  }
			  catch ( CursorException e )
			  {
					assertThat( e.Message, @is( "boo" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorErrorMustThrowIfSecondCursorHasError()
		 internal virtual void CheckAndClearCursorErrorMustThrowIfSecondCursorHasError()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _second.CursorException = "boo";
			  try
			  {
					cursor.CheckAndClearCursorException();
					fail( "composite cursor checkAndClearCursorError should have thrown" );
			  }
			  catch ( CursorException e )
			  {
					assertThat( e.Message, @is( "boo" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorErrorWillOnlyCheckFirstCursorIfBothHaveErrorsSet()
		 internal virtual void CheckAndClearCursorErrorWillOnlyCheckFirstCursorIfBothHaveErrorsSet()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.CursorException = "first boo";
			  _second.CursorException = "second boo";
			  try
			  {
					cursor.CheckAndClearCursorException();
					fail( "composite cursor checkAndClearCursorError should have thrown" );
			  }
			  catch ( CursorException e )
			  {
					assertThat( e.Message, @is( "first boo" ) );
			  }
			  try
			  {
					_second.checkAndClearCursorException();
					fail( "second cursor checkAndClearCursorError should have thrown" );
			  }
			  catch ( CursorException e )
			  {
					assertThat( e.Message, @is( "second boo" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void clearCursorErrorMustClearBothCursors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ClearCursorErrorMustClearBothCursors()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.CursorException = "first boo";
			  _second.CursorException = "second boo";
			  cursor.ClearCursorException();

			  // Now these must not throw
			  _first.checkAndClearCursorException();
			  _second.checkAndClearCursorException();
			  cursor.CheckAndClearCursorException();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void isWriteLockedMustBeTrueIfBothCursorsAreWriteLocked()
		 internal virtual void isWriteLockedMustBeTrueIfBothCursorsAreWriteLocked()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.WriteLocked = true;
			  _second.WriteLocked = true;
			  assertTrue( cursor.WriteLocked );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void isWriteLockedMustBeFalseIfBothCursorsAreNotWriteLocked()
		 internal virtual void isWriteLockedMustBeFalseIfBothCursorsAreNotWriteLocked()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.WriteLocked = false;
			  _second.WriteLocked = false;
			  assertFalse( cursor.WriteLocked );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void isWriteLockedMustBeFalseIfFirstCursorIsNotWriteLocked()
		 internal virtual void isWriteLockedMustBeFalseIfFirstCursorIsNotWriteLocked()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.WriteLocked = false;
			  _second.WriteLocked = true;
			  assertFalse( cursor.WriteLocked );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void isWriteLockedMustBeFalseIfSecondCursorIsNotWriteLocked()
		 internal virtual void isWriteLockedMustBeFalseIfSecondCursorIsNotWriteLocked()
		 {
			  PageCursor cursor = CompositePageCursor.Compose( _first, PAGE_SIZE, _second, PAGE_SIZE );
			  _first.WriteLocked = true;
			  _second.WriteLocked = false;
			  assertFalse( cursor.WriteLocked );
		 }
	}

}