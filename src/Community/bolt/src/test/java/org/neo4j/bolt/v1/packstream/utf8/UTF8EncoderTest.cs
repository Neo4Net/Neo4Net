using System;

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
namespace Neo4Net.Bolt.v1.packstream.utf8
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class UTF8EncoderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeDecode()
		 public virtual void ShouldEncodeDecode()
		 {
			  AssertEncodes( "" );
			  AssertEncodes( "a" );
			  AssertEncodes( "ä" );
			  AssertEncodes( "äa" );
			  AssertEncodes( "基本上，電腦只是處理數位。它們指定一個數位，來儲存字母或其他字元。在創造Unicode之前，" + "有數百種指定這些數位的編碼系統。沒有一個編碼可以包含足夠的字元，例如：單單歐洲共同體就需要好幾種不同的編碼來包括所有的語言。" + "即使是單一種語言，例如英語，也沒有哪一個編碼可以適用於所有的字母、標點符號，和常用的技術符號" );
			  AssertEncodes( StringHelper.NewString( new sbyte[( int ) Math.Pow( 2, 18 )] ) ); // bigger than default buffer size
		 }

		 private void AssertEncodes( string val )
		 {
			  assertEquals( val, EncodeDecode( val ) );
		 }

		 private string EncodeDecode( string original )
		 {
			  ByteBuffer encoded = UTF8Encoder.fastestAvailableEncoder().encode(original);
			  sbyte[] b = new sbyte[encoded.remaining()];
			  encoded.get( b );
			  return StringHelper.NewString( b, StandardCharsets.UTF_8 );
		 }
	}

}