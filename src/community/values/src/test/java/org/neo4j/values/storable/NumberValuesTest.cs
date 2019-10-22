using System;

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
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.NumberValues.hash;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.AnyValueTestUtil.assertIncomparable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValueTestUtil.toAnyValue;

	internal class NumberValuesTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHashNaN()
		 internal virtual void ShouldHashNaN()
		 {
			  assertThat( hash( Double.NaN ), equalTo( hash( Float.NaN ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHashInfinite()
		 internal virtual void ShouldHashInfinite()
		 {
			  assertThat( hash( double.NegativeInfinity ), equalTo( hash( float.NegativeInfinity ) ) );
			  assertThat( hash( double.PositiveInfinity ), equalTo( hash( float.PositiveInfinity ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNaNCorrectly()
		 internal virtual void ShouldHandleNaNCorrectly()
		 {
			  assertIncomparable( toAnyValue( Double.NaN ), toAnyValue( Double.NaN ) );
			  assertIncomparable( toAnyValue( 1 ), toAnyValue( Double.NaN ) );
			  assertIncomparable( toAnyValue( Double.NaN ), toAnyValue( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHashIntegralDoubleAsLong()
		 internal virtual void ShouldHashIntegralDoubleAsLong()
		 {
			  assertThat( hash( 1337d ), equalTo( hash( 1337L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveSameResultEvenWhenArraysContainDifferentTypes()
		 internal virtual void ShouldGiveSameResultEvenWhenArraysContainDifferentTypes()
		 {
			  int[] ints = new int[32];
			  long[] longs = new long[32];

			  Random r = ThreadLocalRandom.current();
			  for ( int i = 0; i < 32; i++ )
			  {
					int nextInt = r.Next();
					ints[i] = nextInt;
					longs[i] = nextInt;
			  }

			  assertThat( hash( ints ), equalTo( hash( longs ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveSameHashForLongsAndInts()
		 internal virtual void ShouldGiveSameHashForLongsAndInts()
		 {
			  Random r = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1_000_000; i++ )
			  {
					int anInt = r.Next();
					assertThat( anInt, equalTo( hash( ( long ) anInt ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveSameResultEvenWhenArraysContainDifferentTypes2()
		 internal virtual void ShouldGiveSameResultEvenWhenArraysContainDifferentTypes2()
		 {
			  sbyte[] bytes = new sbyte[32];
			  short[] shorts = new short[32];

			  Random r = ThreadLocalRandom.current();
			  for ( int i = 0; i < 32; i++ )
			  {
					sbyte nextByte = ( ( Number )( r.Next() ) ).byteValue();
					bytes[i] = nextByte;
					shorts[i] = nextByte;
			  }

			  assertThat( hash( bytes ), equalTo( hash( shorts ) ) );
		 }
	}

}