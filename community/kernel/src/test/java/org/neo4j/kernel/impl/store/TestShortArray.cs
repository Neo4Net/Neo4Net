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
namespace Org.Neo4j.Kernel.impl.store
{
	using Test = org.junit.Test;

	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TestShortArray
	{
		 private static readonly int _defaultPayloadSize = PropertyType.PayloadSize;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeSomeSampleArraysWithDefaultPayloadSize()
		 public virtual void CanEncodeSomeSampleArraysWithDefaultPayloadSize()
		 {
			  AssertCanEncodeAndDecodeToSameValue( new bool[]{ true, false, true, true, true, true, true, true, true, true, false, true } );
			  AssertCanEncodeAndDecodeToSameValue( new sbyte[]{ ( sbyte ) - 1, ( sbyte ) - 10, 43, 127, 0, 4, 2, 3, 56, 47, 67, 43 } );
			  AssertCanEncodeAndDecodeToSameValue( new short[]{ 1, 2, 3, 45, 5, 6, 7 } );
			  AssertCanEncodeAndDecodeToSameValue( new int[]{ 1, 2, 3, 4, 5, 6, 7 } );
			  AssertCanEncodeAndDecodeToSameValue( new long[]{ 1, 2, 3, 4, 5, 6, 7 } );
			  AssertCanEncodeAndDecodeToSameValue( new float[]{ 0.34f, 0.21f } );
			  AssertCanEncodeAndDecodeToSameValue( new long[]{ 1L << 63, 1L << 63 } );
			  AssertCanEncodeAndDecodeToSameValue( new long[]{ 1L << 63, 1L << 63, 1L << 63 } );
			  AssertCanEncodeAndDecodeToSameValue( new sbyte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } );
			  AssertCanEncodeAndDecodeToSameValue( new long[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCannotEncodeMarginal()
		 public virtual void TestCannotEncodeMarginal()
		 {
			  AssertCanNotEncode( new long[]{ 1L << 15, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEncodeBiggerArraysWithBiggerPayloadSize()
		 public virtual void CanEncodeBiggerArraysWithBiggerPayloadSize()
		 {
			  int[] intArray = intArray( 10, 2600 );
			  AssertCanEncodeAndDecodeToSameValue( intArray, 32 );
		 }

		 private void AssertCanNotEncode( object intArray )
		 {
			  AssertCanNotEncode( intArray, _defaultPayloadSize );
		 }

		 private void AssertCanNotEncode( object intArray, int payloadSize )
		 {
			  assertFalse( ShortArray.encode( 0, intArray, new PropertyBlock(), payloadSize ) );
		 }

		 private int[] IntArray( int count, int stride )
		 {
			  int[] result = new int[count];
			  for ( int i = 0; i < count; i++ )
			  {
					result[i] = i * stride;
			  }
			  return result;
		 }

		 private void AssertCanEncodeAndDecodeToSameValue( object value )
		 {
			  AssertCanEncodeAndDecodeToSameValue( value, PropertyType.PayloadSize );
		 }

		 private void AssertCanEncodeAndDecodeToSameValue( object value, int payloadSize )
		 {
			  PropertyBlock target = new PropertyBlock();
			  bool encoded = ShortArray.encode( 0, value, target, payloadSize );
			  assertTrue( encoded );
			  assertEquals( Values.of( value ), ShortArray.decode( target ) );
		 }
	}

}