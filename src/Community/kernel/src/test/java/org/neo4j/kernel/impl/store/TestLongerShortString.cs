using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.store
{
	using Test = org.junit.Test;


	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.PropertyRecordFormat.DEFAULT_PAYLOAD_SIZE;

	public class TestLongerShortString
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMasks()
		 public virtual void TestMasks()
		 {
			  assertEquals( 0, 1 & LongerShortString.invertedBitMask( LongerShortString.Numerical ) );
			  assertEquals( 0, 2 & LongerShortString.invertedBitMask( LongerShortString.Date ) );
			  assertEquals( LongerShortString.Numerical.bitMask(), 3 & LongerShortString.invertedBitMask(LongerShortString.Date) );
			  assertEquals( 0, LongerShortString.Numerical.bitMask() & LongerShortString.invertedBitMask(LongerShortString.Numerical, LongerShortString.Date) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeEmptyString()
		 public virtual void CanEncodeEmptyString()
		 {
			  AssertCanEncodeAndDecodeToSame( "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeNumerical()
		 public virtual void CanEncodeNumerical()
		 {
			  AssertCanEncodeAndDecodeToSame( "12345678901234567890" );
			  AssertCanEncodeAndDecodeToSame( "12345678901234567890 +-.,' 321,3" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeDate()
		 public virtual void CanEncodeDate()
		 {
			  AssertCanEncodeAndDecodeToSame( "2011-10-10 12:45:22+0200" );
			  AssertCanEncodeAndDecodeToSame( "2011/10/10 12:45:22+0200" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRandomStrings()
		 public virtual void TestRandomStrings()
		 {
			  for ( int i = 0; i < 1000; i++ )
			  {
					foreach ( TestStringCharset charset in TestStringCharset.values() )
					{
						 IList<string> list = RandomStrings( 100, charset, 30 );
						 foreach ( string @string in list )
						 {
							  PropertyBlock record = new PropertyBlock();
							  if ( LongerShortString.encode( 10, @string, record, DEFAULT_PAYLOAD_SIZE ) )
							  {
									assertEquals( Values.stringValue( @string ), LongerShortString.decode( record ) );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeEmailAndUri()
		 public virtual void CanEncodeEmailAndUri()
		 {
			  AssertCanEncodeAndDecodeToSame( "mattias@neotechnology.com" );
			  AssertCanEncodeAndDecodeToSame( "http://domain:7474/" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeLower()
		 public virtual void CanEncodeLower()
		 {
			  AssertCanEncodeAndDecodeToSame( "folder/generators/templates/controller.ext" );
			  AssertCanEncodeAndDecodeToSame( "folder/generators/templates/controller.extr" );
			  AssertCannotEncode( "folder/generators/templates/controller.extra" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeLowerHex()
		 public virtual void CanEncodeLowerHex()
		 {
			  AssertCanEncodeAndDecodeToSame( "da39a3ee5e6b4b0d3255bfef95601890afd80709" ); // sha1hex('') len=40
			  AssertCanEncodeAndDecodeToSame( "0123456789" + "abcdefabcd" + "0a0b0c0d0e" + "1a1b1c1d1e" + "f9e8d7c6b5" + "a4f3" ); // len=54
			  AssertCannotEncode( "da39a3ee5e6b4b0d3255bfef95601890afd80709" + "0123456789" + "abcde" ); // len=55
			  // test not failing on long illegal hex
			  AssertCannotEncode( "aaaaaaaaaa" + "bbbbbbbbbb" + "cccccccccc" + "dddddddddd" + "eeeeeeeeee" + "x" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeUpperHex()
		 public virtual void CanEncodeUpperHex()
		 {
			  AssertCanEncodeAndDecodeToSame( "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709" ); // sha1HEX('') len=40
			  AssertCanEncodeAndDecodeToSame( "0123456789" + "ABCDEFABCD" + "0A0B0C0D0E" + "1A1B1C1D1E" + "F9E8D7C6B5" + "A4F3" ); // len=54
			  AssertCannotEncode( "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709" + "0123456789" + "ABCDE" ); // len=55
			  // test not failing on long illegal HEX
			  AssertCannotEncode( "AAAAAAAAAA" + "BBBBBBBBBB" + "CCCCCCCCCC" + "DDDDDDDDDD" + "EEEEEEEEEE" + "X" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkMarginalFit()
		 public virtual void CheckMarginalFit()
		 {
			  AssertCanEncodeAndDecodeToSame( "^aaaaaaaaaaaaaaaaaaaaaaaaaa" );
			  AssertCannotEncode( "^aaaaaaaaaaaaaaaaaaaaaaaaaaa" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeUUIDString()
		 public virtual void CanEncodeUUIDString()
		 {
			  AssertCanEncodeAndDecodeToSame( "81fe144f-484b-4a34-8e36-17a021540318" );
		 }

		 private static IList<string> RandomStrings( int count, TestStringCharset charset, int maxLen )
		 {
			  IList<string> result = new List<string>( count );
			  for ( int i = 0; i < count; i++ )
			  {
					result.Add( charset.randomString( maxLen ) );
			  }
			  return result;
		 }

		 private void AssertCanEncodeAndDecodeToSame( string @string )
		 {
			  AssertCanEncodeAndDecodeToSame( @string, DEFAULT_PAYLOAD_SIZE );
		 }

		 private void AssertCanEncodeAndDecodeToSame( string @string, int payloadSize )
		 {
			  PropertyBlock target = new PropertyBlock();
			  assertTrue( LongerShortString.encode( 0, @string, target, payloadSize ) );
			  assertEquals( Values.stringValue( @string ), LongerShortString.decode( target ) );
		 }

		 private void AssertCannotEncode( string @string )
		 {
			  AssertCannotEncode( @string, DEFAULT_PAYLOAD_SIZE );
		 }

		 private void AssertCannotEncode( string @string, int payloadSize )
		 {
			  assertFalse( LongerShortString.encode( 0, @string, new PropertyBlock(), payloadSize ) );
		 }
	}

}