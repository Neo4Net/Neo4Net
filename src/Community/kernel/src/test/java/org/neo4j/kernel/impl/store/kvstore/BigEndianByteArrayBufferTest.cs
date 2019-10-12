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
	using Matcher = org.hamcrest.Matcher;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class BigEndianByteArrayBufferTest
	{
		 internal BigEndianByteArrayBuffer Buffer = new BigEndianByteArrayBuffer( new sbyte[8] );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteLong()
		 public virtual void ShouldWriteLong()
		 {
			  // when
			  Buffer.putLong( 0, unchecked( ( long )0xABCDEF0123456789L ) );

			  // then
			  assertEquals( 0xAB, 0xFF & Buffer.getByte( 0 ) );
			  assertEquals( 0xCD, 0xFF & Buffer.getByte( 1 ) );
			  assertEquals( 0xEF, 0xFF & Buffer.getByte( 2 ) );
			  assertEquals( 0x01, 0xFF & Buffer.getByte( 3 ) );
			  assertEquals( 0x23, 0xFF & Buffer.getByte( 4 ) );
			  assertEquals( 0x45, 0xFF & Buffer.getByte( 5 ) );
			  assertEquals( 0x67, 0xFF & Buffer.getByte( 6 ) );
			  assertEquals( 0x89, 0xFF & Buffer.getByte( 7 ) );
			  assertEquals( 0xABCDEF0123456789L, Buffer.getLong( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteInt()
		 public virtual void ShouldWriteInt()
		 {
			  // when
			  Buffer.putInt( 0, 0x12345678 );
			  Buffer.putInt( 4, unchecked( ( int )0x87654321 ) );

			  // then
			  assertEquals( 0x12345678, Buffer.getInt( 0 ) );
			  assertEquals( 0x87654321, Buffer.getInt( 4 ) );
			  assertEquals( 0x1234567887654321L, Buffer.getLong( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteShort()
		 public virtual void ShouldWriteShort()
		 {
			  // when
			  Buffer.putShort( 0, ( short ) 0x1234 );
			  Buffer.putShort( 2, ( short ) 0x4321 );
			  Buffer.putShort( 4, unchecked( ( short ) 0xABCD ) );
			  Buffer.putShort( 6, unchecked( ( short ) 0xFEDC ) );

			  // then
			  assertEquals( ( short ) 0x1234, Buffer.getShort( 0 ) );
			  assertEquals( ( short ) 0x4321, Buffer.getShort( 2 ) );
			  assertEquals( unchecked( ( short ) 0xABCD ), Buffer.getShort( 4 ) );
			  assertEquals( unchecked( ( short ) 0xFEDC ), Buffer.getShort( 6 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteChar()
		 public virtual void ShouldWriteChar()
		 {
			  // when
			  Buffer.putChar( 0, 'H' );
			  Buffer.putChar( 2, 'E' );
			  Buffer.putChar( 4, 'L' );
			  Buffer.putChar( 6, 'O' );

			  // then
			  assertEquals( 'H', Buffer.getChar( 0 ) );
			  assertEquals( 'E', Buffer.getChar( 2 ) );
			  assertEquals( 'L', Buffer.getChar( 4 ) );
			  assertEquals( 'O', Buffer.getChar( 6 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteByte()
		 public virtual void ShouldWriteByte()
		 {
			  // when
			  for ( int i = 0; i < Buffer.size(); i++ )
			  {
					Buffer.putByte( i, ( sbyte )( ( 1 << i ) + i ) );
			  }

			  // then
			  for ( int i = 0; i < Buffer.size(); i++ )
			  {
					assertEquals( ( sbyte )( ( 1 << i ) + i ), Buffer.getByte( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompareByteArrays()
		 public virtual void ShouldCompareByteArrays()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.hamcrest.Matcher<int> LESS_THAN = lessThan(0);
			  Matcher<int> lessThan = lessThan( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.hamcrest.Matcher<int> GREATER_THAN = greaterThan(0);
			  Matcher<int> greaterThan = greaterThan( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.hamcrest.Matcher<int> EQUAL_TO = equalTo(0);
			  Matcher<int> equalTo = equalTo( 0 );

			  // then
			  AssertCompare( new sbyte[0], equalTo, new sbyte[0] );
			  AssertCompare( new sbyte[]{ 1, 2, 3 }, equalTo, new sbyte[]{ 1, 2, 3 } );
			  AssertCompare( new sbyte[]{ 1, 2, 3 }, lessThan, new sbyte[]{ 1, 2, 4 } );
			  AssertCompare( new sbyte[]{ 1, 2, 3 }, lessThan, new sbyte[]{ 2, 2, 3 } );
			  AssertCompare( new sbyte[]{ 1, 2, 3 }, greaterThan, new sbyte[]{ 1, 2, 0 } );
			  AssertCompare( new sbyte[]{ 1, 2, 3 }, greaterThan, new sbyte[]{ 0, 2, 3 } );
		 }

		 private static void AssertCompare( sbyte[] lhs, Matcher<int> isAsExpected, sbyte[] rhs )
		 {
			  assertThat( BigEndianByteArrayBuffer.Compare( lhs, rhs, 0 ), isAsExpected );
		 }
	}

}