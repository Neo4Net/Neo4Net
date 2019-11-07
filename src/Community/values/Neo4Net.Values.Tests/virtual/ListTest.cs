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
namespace Neo4Net.Values.@virtual
{
	using Test = org.junit.jupiter.api.Test;

	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.booleanArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.charArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.doubleArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.floatArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.intArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.longArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.shortArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.AnyValueTestUtil.assertEqualValues;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.AnyValueTestUtil.assertIncomparable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.AnyValueTestUtil.assertNotEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValueTestUtil.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.range;

	internal class ListTest
	{

		 private ListValue[] _equivalentLists = new ListValue[] { VirtualValues.List( Values.longValue( 1L ), Values.longValue( 4L ), Values.longValue( 7L ) ), range( 1L, 8L, 3L ), VirtualValues.FromArray( Values.longArray( new long[]{ 1L, 4L, 7L } ) ), VirtualValues.List( NO_VALUE, longValue( 1L ), NO_VALUE, longValue( 4L ), longValue( 7L ), NO_VALUE ).dropNoValues(), list(-2L, 1L, 4L, 7L, 10L).slice(1, 4), list(-2L, 1L, 4L, 7L).drop(1), list(1L, 4L, 7L, 10L, 13L).take(3), list(7L, 4L, 1L).reverse(), VirtualValues.Concat(list(1L, 4L), list(7L)) };

		 private ListValue[] _nonEquivalentLists = new ListValue[] { VirtualValues.List( Values.longValue( 1L ), Values.longValue( 4L ), Values.longValue( 7L ) ), range( 2L, 9L, 3L ), VirtualValues.FromArray( Values.longArray( new long[]{ 3L, 6L, 9L } ) ), VirtualValues.List( NO_VALUE, longValue( 1L ), NO_VALUE, longValue( 5L ), longValue( 7L ), NO_VALUE ).dropNoValues(), list(-2L, 1L, 5L, 8L, 11L).slice(1, 4), list(-2L, 6L, 9L, 12L).drop(1), list(7L, 10L, 13L, 10L, 13L).take(3), list(15L, 12L, 9L).reverse(), VirtualValues.Concat(list(10L, 13L), list(16L)) };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeEqualToItself()
		 internal virtual void ShouldBeEqualToItself()
		 {
			  assertEqual( list( new string[]{ "hi" }, 3.0 ), list( new string[]{ "hi" }, 3.0 ) );

			  assertEqual( list(), list() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeEqualToArrayIfValuesAreEqual()
		 internal virtual void ShouldBeEqualToArrayIfValuesAreEqual()
		 {
			  // the empty list equals any array that is empty
			  assertEqualValues( list(), booleanArray(new bool[]{}) );
			  assertEqualValues( list(), byteArray(new sbyte[]{}) );
			  assertEqualValues( list(), charArray(new char[]{}) );
			  assertEqualValues( list(), doubleArray(new double[]{}) );
			  assertEqualValues( list(), floatArray(new float[]{}) );
			  assertEqualValues( list(), intArray(new int[]{}) );
			  assertEqualValues( list(), longArray(new long[]{}) );
			  assertEqualValues( list(), shortArray(new short[]{}) );
			  assertEqualValues( list(), stringArray() );

			  //actual values to test the equality
			  assertEqualValues( list( true ), booleanArray( new bool[]{ true } ) );
			  assertEqualValues( list( true, false ), booleanArray( new bool[]{ true, false } ) );
			  assertEqualValues( list( 84 ), byteArray( "T".GetBytes() ) );
			  assertEqualValues( list( 84, 104, 105, 115, 32, 105, 115, 32, 106, 117, 115, 116, 32, 97, 32, 116, 101, 115, 116 ), byteArray( "This is just a test".GetBytes() ) );
			  assertEqualValues( list( 'h' ), charArray( new char[]{ 'h' } ) );
			  assertEqualValues( list( 'h', 'i' ), charArray( new char[]{ 'h', 'i' } ) );
			  assertEqualValues( list( 1.0 ), doubleArray( new double[]{ 1.0 } ) );
			  assertEqualValues( list( 1.0, 2.0 ), doubleArray( new double[]{ 1.0, 2.0 } ) );
			  assertEqualValues( list( 1.5f ), floatArray( new float[]{ 1.5f } ) );
			  assertEqualValues( list( 1.5f, -5f ), floatArray( new float[]{ 1.5f, -5f } ) );
			  assertEqualValues( list( 1 ), intArray( new int[]{ 1 } ) );
			  assertEqualValues( list( 1, -3 ), intArray( new int[]{ 1, -3 } ) );
			  assertEqualValues( list( 2L ), longArray( new long[]{ 2L } ) );
			  assertEqualValues( list( 2L, -3L ), longArray( new long[]{ 2L, -3L } ) );
			  assertEqualValues( list( ( short ) 2 ), shortArray( new short[]{ ( short ) 2 } ) );
			  assertEqualValues( list( ( short ) 2, ( short ) - 3 ), shortArray( new short[]{ ( short ) 2, ( short ) - 3 } ) );
			  assertEqualValues( list( "hi" ), stringArray( "hi" ) );
			  assertEqualValues( list( "hi", "ho" ), stringArray( "hi", "ho" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotEqual()
		 internal virtual void ShouldNotEqual()
		 {
			  assertNotEqual( list(), list(2) );
			  assertNotEqual( list(), list(1, 2) );
			  assertNotEqual( list( 1 ), list( 2 ) );
			  assertNotEqual( list( 1 ), list( 1, 2 ) );
			  assertNotEqual( list( 1, 1 ), list( 1, 2 ) );
			  assertNotEqual( list( 1, "d" ), list( 1, "f" ) );
			  assertNotEqual( list( 1, "d" ), list( "d", 1 ) );
			  assertNotEqual( list( "d" ), list( false ) );
			  assertNotEqual( list( Values.stringArray( "d" ) ), list( "d" ) );

			  assertNotEqual( list( longArray( new long[]{ 3, 4, 5 } ) ), list( intArray( new int[]{ 3, 4, 50 } ) ) );

			  // different value types
			  assertNotEqual( list( true, true ), intArray( new int[]{ 0, 0 } ) );
			  assertNotEqual( list( true, true ), longArray( new long[]{ 0L, 0L } ) );
			  assertNotEqual( list( true, true ), shortArray( new short[]{ ( short ) 0, ( short ) 0 } ) );
			  assertNotEqual( list( true, true ), floatArray( new float[]{ 0.0f, 0.0f } ) );
			  assertNotEqual( list( true, true ), doubleArray( new double[]{ 0.0, 0.0 } ) );
			  assertNotEqual( list( true, true ), charArray( new char[]{ 'T', 'T' } ) );
			  assertNotEqual( list( true, true ), stringArray( "True", "True" ) );
			  assertNotEqual( list( true, true ), byteArray( new sbyte[]{ ( sbyte ) 0, ( sbyte ) 0 } ) );

			  // wrong or missing items
			  assertNotEqual( list( true ), booleanArray( new bool[]{ true, false } ) );
			  assertNotEqual( list( true, true ), booleanArray( new bool[]{ true, false } ) );
			  assertNotEqual( list( 84, 104, 32, 105, 115, 32, 106, 117, 115, 116, 32, 97, 32, 116, 101, 115, 116 ), byteArray( "This is just a test".GetBytes() ) );
			  assertNotEqual( list( 'h' ), charArray( new char[]{ 'h', 'i' } ) );
			  assertNotEqual( list( 'h', 'o' ), charArray( new char[]{ 'h', 'i' } ) );
			  assertNotEqual( list( 9.0, 2.0 ), doubleArray( new double[]{ 1.0, 2.0 } ) );
			  assertNotEqual( list( 1.0 ), doubleArray( new double[]{ 1.0, 2.0 } ) );
			  assertNotEqual( list( 1.5f ), floatArray( new float[]{ 1.5f, -5f } ) );
			  assertNotEqual( list( 1.5f, 5f ), floatArray( new float[]{ 1.5f, -5f } ) );
			  assertNotEqual( list( 1, 3 ), intArray( new int[]{ 1, -3 } ) );
			  assertNotEqual( list( -3 ), intArray( new int[]{ 1, -3 } ) );
			  assertNotEqual( list( 2L, 3L ), longArray( new long[]{ 2L, -3L } ) );
			  assertNotEqual( list( 2L ), longArray( new long[]{ 2L, -3L } ) );
			  assertNotEqual( list( ( short ) 2, ( short ) 3 ), shortArray( new short[]{ ( short ) 2, ( short ) - 3 } ) );
			  assertNotEqual( list( ( short ) 2 ), shortArray( new short[]{ ( short ) 2, ( short ) - 3 } ) );
			  assertNotEqual( list( "hi", "hello" ), stringArray( "hi" ) );
			  assertNotEqual( list( "hello" ), stringArray( "hi" ) );

			  assertNotEqual( list( 1, 'b' ), charArray( new char[]{ 'a', 'b' } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNullInList()
		 internal virtual void ShouldHandleNullInList()
		 {
			  assertIncomparable( list( 1, null ), list( 1, 2 ) );
			  assertNotEqual( list( 1, null ), list( 2, 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCoerce()
		 internal virtual void ShouldCoerce()
		 {
			  assertEqual( list( new string[]{ "h" }, 3.0 ), list( new char[]{ 'h' }, 3 ) );

			  assertEqualValues( list( "a", 'b' ), charArray( new char[]{ 'a', 'b' } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRecurse()
		 internal virtual void ShouldRecurse()
		 {
			  assertEqual( list( 'a', list( 'b', list( 'c' ) ) ), list( 'a', list( 'b', list( 'c' ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNestCorrectly()
		 internal virtual void ShouldNestCorrectly()
		 {
			  assertEqual( list( booleanArray( new bool[]{ true, false } ), intArray( new int[]{ 1, 2 } ), stringArray( "Hello", "World" ) ), list( booleanArray( new bool[]{ true, false } ), intArray( new int[]{ 1, 2 } ), stringArray( "Hello", "World" ) ) );

			  assertNotEqual( list( booleanArray( new bool[]{ true, false } ), intArray( new int[]{ 5, 2 } ), stringArray( "Hello", "World" ) ), list( booleanArray( new bool[]{ true, false } ), intArray( new int[]{ 1, 2 } ), stringArray( "Hello", "World" ) ) );

			  assertNotEqual( list( intArray( new int[]{ 1, 2 } ), booleanArray( new bool[]{ true, false } ), stringArray( "Hello", "World" ) ), list( booleanArray( new bool[]{ true, false } ), intArray( new int[]{ 1, 2 } ), stringArray( "Hello", "World" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRecurseAndCoerce()
		 internal virtual void ShouldRecurseAndCoerce()
		 {
			  assertEqual( list( "a", list( 'b', list( "c" ) ) ), list( 'a', list( "b", list( 'c' ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTreatDifferentListImplementationSimilar()
		 internal virtual void ShouldTreatDifferentListImplementationSimilar()
		 {
			  foreach ( ListValue list1 in _equivalentLists )
			  {
					foreach ( ListValue list2 in _equivalentLists )
					{
						 assertEqual( list1, list2 );
						 assertArrayEquals( list1.AsArray(), list2.AsArray(), format("%s.asArray != %s.toArray", list1.GetType().Name, list2.GetType().Name) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotTreatDifferentListImplementationSimilarOfNonEquivalentListsSimilar()
		 internal virtual void ShouldNotTreatDifferentListImplementationSimilarOfNonEquivalentListsSimilar()
		 {
			  foreach ( ListValue list1 in _nonEquivalentLists )
			  {
					foreach ( ListValue list2 in _nonEquivalentLists )
					{
						 if ( list1 == list2 )
						 {
							  continue;
						 }
						 assertNotEqual( list1, list2 );
						 assertFalse( Arrays.Equals( list1.AsArray(), list2.AsArray() ), format("%s.asArray != %s.toArray", list1.GetType().Name, list2.GetType().Name) );
					}
			  }
		 }
	}

}