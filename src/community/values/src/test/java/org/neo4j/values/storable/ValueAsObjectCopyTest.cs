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
namespace Neo4Net.Values.Storable
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class ValueAsObjectCopyTest
	{
		private bool InstanceFieldsInitialized = false;

		public ValueAsObjectCopyTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_scalars = Arrays.asList( ShouldGivePublic( Values.ByteValue( ( sbyte )1 ), ( sbyte )1 ), ShouldGivePublic( Values.ShortValue( ( short )2 ), ( short )2 ), ShouldGivePublic( Values.IntValue( 3 ), 3 ), ShouldGivePublic( Values.LongValue( 4L ), 4L ), ShouldGivePublic( Values.FloatValue( 5.0f ), 5.0f ), ShouldGivePublic( Values.DoubleValue( 6.0 ), 6.0 ), ShouldGivePublic( Values.BooleanValue( false ), false ), ShouldGivePublic( Values.CharValue( 'a' ), 'a' ), ShouldGivePublic( Values.StringValue( "b" ), "b" ) );
		}

		 private IEnumerable<AsObjectCopyTest> _scalars;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideScalarValueAsPublic()
		 internal virtual void ShouldProvideScalarValueAsPublic()
		 {
			  foreach ( AsObjectCopyTest test in _scalars )
			  {
					test.AssertGeneratesPublic();
			  }
		 }

		 // DIRECT ARRAYS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectByteArrayAsPublic()
		 internal virtual void ShouldProvideDirectByteArrayAsPublic()
		 {
			  sbyte[] inStore = new sbyte[] { 1 };
			  Value value = Values.ByteArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return byte[]" );

			  sbyte[] arr = ( sbyte[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = -1;
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( sbyte[] ) value.AsObjectCopy() ), "should still generate inStore array" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectShortArrayAsPublic()
		 internal virtual void ShouldProvideDirectShortArrayAsPublic()
		 {
			  short[] inStore = new short[] { 1 };
			  Value value = Values.ShortArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return short[]" );

			  short[] arr = ( short[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = -1;
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( short[] )value.AsObjectCopy() ), "should still generate inStore array" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectIntArrayAsPublic()
		 internal virtual void ShouldProvideDirectIntArrayAsPublic()
		 {
			  int[] inStore = new int[] { 1 };
			  Value value = Values.IntArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return int[]" );

			  int[] arr = ( int[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = -1;
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( int[] )value.AsObjectCopy() ), "should still generate inStore array" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectLongArrayAsPublic()
		 internal virtual void ShouldProvideDirectLongArrayAsPublic()
		 {
			  long[] inStore = new long[] { 1 };
			  Value value = Values.LongArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return long[]" );

			  long[] arr = ( long[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = -1;
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( long[] )value.AsObjectCopy() ), "should still generate inStore array" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectFloatArrayAsPublic()
		 internal virtual void ShouldProvideDirectFloatArrayAsPublic()
		 {
			  float[] inStore = new float[] { 1 };
			  Value value = Values.FloatArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return float[]" );

			  float[] arr = ( float[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = -1;
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( float[] )value.AsObjectCopy() ), "should still generate inStore array" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectDoubleArrayAsPublic()
		 internal virtual void ShouldProvideDirectDoubleArrayAsPublic()
		 {
			  double[] inStore = new double[] { 1 };
			  Value value = Values.DoubleArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return double[]" );

			  double[] arr = ( double[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = -1;
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( double[] )value.AsObjectCopy() ), "should still generate inStore array" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectCharArrayAsPublic()
		 internal virtual void ShouldProvideDirectCharArrayAsPublic()
		 {
			  char[] inStore = new char[] { 'a' };
			  Value value = Values.CharArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return char[]" );

			  char[] arr = ( char[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = 'b';
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( char[] )value.AsObjectCopy() ), "should still generate inStore array" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectStringArrayAsPublic()
		 internal virtual void ShouldProvideDirectStringArrayAsPublic()
		 {
			  string[] inStore = new string[] { "a" };
			  Value value = Values.StringArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return String[]" );

			  string[] arr = ( string[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = "b";
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( string[] )value.AsObjectCopy() ), "should still generate inStore array" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideDirectBooleanArrayAsPublic()
		 internal virtual void ShouldProvideDirectBooleanArrayAsPublic()
		 {
			  bool[] inStore = new bool[] { true };
			  Value value = Values.BooleanArray( inStore );
			  object asObject = value.AsObjectCopy();
			  assertNotNull( asObject, "should return boolean[]" );

			  bool[] arr = ( bool[] ) asObject;
			  assertTrue( Arrays.Equals( inStore, arr ), "should have same values" );

			  arr[0] = false;
			  assertFalse( Arrays.Equals( inStore, arr ), "should not modify inStore array" );
			  assertTrue( Arrays.Equals( inStore, ( bool[] )value.AsObjectCopy() ), "should still generate inStore array" );
		 }

		 private AsObjectCopyTest ShouldGivePublic( Value value, object asObject )
		 {
			  return new AsObjectCopyTest( value, asObject );
		 }

		 private class AsObjectCopyTest
		 {
			  internal readonly Value Value;
			  internal readonly object Expected;

			  internal AsObjectCopyTest( Value value, object expected )
			  {
					this.Value = value;
					this.Expected = expected;
			  }

			  internal virtual void AssertGeneratesPublic()
			  {
					assertThat( Value.asObjectCopy(), equalTo(Expected) );
			  }
		 }
	}

}