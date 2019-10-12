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
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class HexPrinterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintACoupleOfLines()
		 public virtual void ShouldPrintACoupleOfLines()
		 {
			  // GIVEN
			  MemoryStream outStream = new MemoryStream();
			  PrintStream @out = new PrintStream( outStream );
			  HexPrinter printer = new HexPrinter( @out );

			  // WHEN
			  for ( sbyte value = 0; value < 40; value++ )
			  {
					printer.Append( value );
			  }

			  // THEN
			  @out.flush();
			  assertEquals( format( "00 01 02 03 04 05 06 07    08 09 0A 0B 0C 0D 0E 0F    " + "10 11 12 13 14 15 16 17    18 19 1A 1B 1C 1D 1E 1F%n" + "20 21 22 23 24 25 26 27" ), outStream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintUserSpecifiedBytesGroupingFormat()
		 public virtual void ShouldPrintUserSpecifiedBytesGroupingFormat()
		 {
			  // GIVEN
			  MemoryStream outStream = new MemoryStream();
			  PrintStream @out = new PrintStream( outStream );
			  HexPrinter printer = ( new HexPrinter( @out ) ).WithBytesGroupingFormat( 12, 4, ", " );

			  // WHEN
			  for ( sbyte value = 0; value < 30; value++ )
			  {
					printer.Append( value );
			  }

			  // THEN
			  @out.flush();
			  assertEquals( format( "00 01 02 03, 04 05 06 07, 08 09 0A 0B%n" + "0C 0D 0E 0F, 10 11 12 13, 14 15 16 17%n" + "18 19 1A 1B, 1C 1D" ), outStream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGroupingWhenBytesPerGroupIsGreaterThanBytesPerLine()
		 public virtual void ShouldNotGroupingWhenBytesPerGroupIsGreaterThanBytesPerLine()
		 {
			  // GIVEN
			  MemoryStream outStream = new MemoryStream();
			  PrintStream @out = new PrintStream( outStream );
			  HexPrinter printer = ( new HexPrinter( @out ) ).WithBytesPerLine( 12 ).withBytesPerGroup( 100 );

			  // WHEN
			  for ( sbyte value = 0; value < 30; value++ )
			  {
					printer.Append( value );
			  }

			  // THEN
			  @out.flush();
			  assertEquals( format( "00 01 02 03 04 05 06 07 08 09 0A 0B%n" + "0C 0D 0E 0F 10 11 12 13 14 15 16 17%n" + "18 19 1A 1B 1C 1D" ), outStream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintUserSpecifiedLineNumberFormat()
		 public virtual void ShouldPrintUserSpecifiedLineNumberFormat()
		 {
			  // GIVEN
			  MemoryStream outStream = new MemoryStream();
			  PrintStream @out = new PrintStream( outStream );
			  HexPrinter printer = ( new HexPrinter( @out ) ).WithLineNumberFormat( 5, "[", "]" );

			  // WHEN
			  for ( sbyte value = 0; value < 40; value++ )
			  {
					printer.Append( value );
			  }

			  // THEN
			  @out.flush();
			  assertEquals( format( "[0x00000]" + "00 01 02 03 04 05 06 07    08 09 0A 0B 0C 0D 0E 0F    " + "10 11 12 13 14 15 16 17    18 19 1A 1B 1C 1D 1E 1F%n" + "[0x00001]" + "20 21 22 23 24 25 26 27" ), outStream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartFromUserSpecifiedLineNumber()
		 public virtual void ShouldStartFromUserSpecifiedLineNumber()
		 {
			  // GIVEN
			  MemoryStream outStream = new MemoryStream();
			  PrintStream @out = new PrintStream( outStream );
			  HexPrinter printer = ( new HexPrinter( @out ) ).WithLineNumberDigits( 2 ).withLineNumberOffset( 0xA8 );

			  // WHEN
			  for ( sbyte value = 0; value < 40; value++ )
			  {
					printer.Append( value );
			  }

			  // THEN
			  @out.flush();
			  assertEquals( format( "@ 0xA8: " + "00 01 02 03 04 05 06 07    08 09 0A 0B 0C 0D 0E 0F    " + "10 11 12 13 14 15 16 17    18 19 1A 1B 1C 1D 1E 1F%n" + "@ 0xA9: " + "20 21 22 23 24 25 26 27" ), outStream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintPartOfByteBuffer()
		 public virtual void ShouldPrintPartOfByteBuffer()
		 {
			  ByteBuffer bytes = ByteBuffer.allocate( 1024 );
			  for ( sbyte value = 0; value < 33; value++ )
			  {
					bytes.put( value );
			  }
			  string hexString = HexPrinter.Hex( bytes, 3, 8 );
			  assertEquals( format( "03 04 05 06 07 08 09 0A" ), hexString );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyPrintBytesWrittenToBuffer()
		 public virtual void ShouldOnlyPrintBytesWrittenToBuffer()
		 {
			  // Given
			  ByteBuffer bytes = ByteBuffer.allocate( 1024 );
			  for ( sbyte value = 0; value < 10; value++ )
			  {
					bytes.put( value );
			  }
			  bytes.flip();

			  // When
			  string hexString = HexPrinter.Hex( bytes );

			  // Then
			  assertEquals( format( "00 01 02 03 04 05 06 07    08 09" ), hexString );
		 }
	}

}