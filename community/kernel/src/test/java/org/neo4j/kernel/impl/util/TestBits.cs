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
namespace Org.Neo4j.Kernel.impl.util
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.bits;

	public class TestBits
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void asBytes()
		 public virtual void AsBytes()
		 {
			  int numberOfBytes = 14;
			  Bits bits = bits( numberOfBytes );
			  for ( sbyte i = 0; i < numberOfBytes; i++ )
			  {
					bits.Put( i );
			  }

			  sbyte[] bytes = bits.AsBytes();
			  for ( sbyte i = 0; i < numberOfBytes; i++ )
			  {
					assertEquals( i, bytes[i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doubleAsBytes()
		 public virtual void DoubleAsBytes()
		 {
			  double[] array1 = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
			  Bits bits = Bits.BitsConflict( array1.Length * 8 );
			  foreach ( double value in array1 )
			  {
					bits.put( Double.doubleToRawLongBits( value ) );
			  }
			  string first = bits.ToString();
			  sbyte[] asBytes = bits.AsBytes();
			  string other = Bits.BitsFromBytes( asBytes ).ToString();
			  assertEquals( first, other );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doubleAsBytesWithOffset()
		 public virtual void DoubleAsBytesWithOffset()
		 {
			  double[] array1 = new double[]{ 1.0, 2.0, 3.0, 4.0, 5.0 };
			  Bits bits = Bits.BitsConflict( array1.Length * 8 );
			  foreach ( double value in array1 )
			  {
					bits.put( Double.doubleToRawLongBits( value ) );
			  }
			  int offset = 6;
			  sbyte[] asBytesOffset = bits.AsBytes( offset );
			  sbyte[] asBytes = bits.AsBytes();
			  assertEquals( asBytes.Length, array1.Length * 8 );
			  assertEquals( asBytesOffset.Length, array1.Length * 8 + offset );
			  for ( int i = 0; i < asBytes.Length; i++ )
			  {
					assertEquals( asBytesOffset[i + offset], asBytes[i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeAndRead()
		 public virtual void WriteAndRead()
		 {
			  for ( int b = 5; b <= 8; b++ )
			  {
					Bits bits = Bits.BitsConflict( 16 );
					for ( sbyte value = 0; value < 16; value++ )
					{
						 bits.Put( value, b );
					}
					for ( sbyte expected = 0; bits.Available(); expected++ )
					{
						 assertEquals( expected, bits.GetByte( b ) );
					}
			  }

			  for ( sbyte value = sbyte.MinValue; value < sbyte.MaxValue; value++ )
			  {
					Bits bits = Bits.BitsConflict( 8 );
					bits.Put( value );
					assertEquals( value, bits.Byte );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeAndReadByteBuffer()
		 public virtual void WriteAndReadByteBuffer()
		 {
			  sbyte[] bytes = new sbyte[512];
			  ByteBuffer buffer = ByteBuffer.wrap( bytes ).order( ByteOrder.LITTLE_ENDIAN );
			  buffer.putLong( 123456789L );
			  buffer.flip();
			  Bits bits = Bits.BitsFromBytes( bytes, 0, buffer.limit() );

			  assertEquals( 123456789L, bits.Long );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void numberToStringSeparatesAfter8Bits()
		 public virtual void NumberToStringSeparatesAfter8Bits()
		 {
			  StringBuilder builder = new StringBuilder();
			  Bits.NumberToString( builder, 0b11111111, 2 );
			  assertThat( builder.ToString(), @is("[00000000,11111111]") );
		 }

	}

}