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
namespace Neo4Net.Values.Storable
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class ValueTupleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEqual()
		 internal virtual void ShouldEqual()
		 {
			  VerifyEquals( Tuple( true ), Tuple( true ) );
			  AssertNotEquals( Tuple( true ), Tuple( false ) );
			  VerifyEquals( Tuple( 1, 2, 3, 4L ), Tuple( 1.0, 2.0, 3, ( sbyte )4 ) );
			  AssertNotEquals( Tuple( 2, 3, 1 ), Tuple( 1, 2, 3 ) );
			  AssertNotEquals( Tuple( 1, 2, 3, 4 ), Tuple( 1, 2, 3 ) );
			  AssertNotEquals( Tuple( 1, 2, 3 ), Tuple( 1, 2, 3, 4 ) );
			  VerifyEquals( Tuple( ( object ) new int[]{ 3 } ), Tuple( ( object ) new int[]{ 3 } ) );
			  VerifyEquals( Tuple( ( object ) new int[]{ 3 } ), Tuple( ( object ) new sbyte[]{ 3 } ) );
			  VerifyEquals( Tuple( 'a', new int[]{ 3 }, "c" ), Tuple( 'a', new int[]{ 3 }, "c" ) );
		 }

		 private ValueTuple Tuple( params object[] objs )
		 {
			  Value[] values = new Value[objs.Length];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					values[i] = Values.Of( objs[i] );
			  }
			  return ValueTuple.Of( values );
		 }

		 private void VerifyEquals( ValueTuple a, ValueTuple b )
		 {
			  assertThat( a, equalTo( b ) );
			  assertThat( b, equalTo( a ) );
			  assertEquals( a.GetHashCode(), b.GetHashCode(), format("Expected hashCode for %s and %s to be equal", a, b) );
		 }

		 private void AssertNotEquals( ValueTuple a, ValueTuple b )
		 {
			  assertThat( a, not( equalTo( b ) ) );
			  assertThat( b, not( equalTo( a ) ) );
		 }
	}

}