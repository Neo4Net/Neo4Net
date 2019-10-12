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
namespace Org.Neo4j.Collection.primitive
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;

	internal class PrimitiveArraysTest
	{
		 private static readonly int[] _noInts = new int[0];
		 private static readonly int[] _oneInt = new int[]{ 1 };
		 private static readonly long[] _noLongs = new long[0];
		 private static readonly long[] _oneLong = new long[]{ 1 };

		 // union() null checks. Actual behaviour is tested in PrimitiveSortedArraySetUnionTest

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void union_shouldHandleNullInput()
		 internal virtual void UnionShouldHandleNullInput()
		 {
			  assertThat( PrimitiveArrays.Union( null, null ), nullValue() );
			  assertThat( PrimitiveArrays.Union( null, _noInts ), equalTo( _noInts ) );
			  assertThat( PrimitiveArrays.Union( _noInts, null ), equalTo( _noInts ) );
			  assertThat( PrimitiveArrays.Union( null, _oneInt ), equalTo( _oneInt ) );
			  assertThat( PrimitiveArrays.Union( _oneInt, null ), equalTo( _oneInt ) );
		 }

		 // intersect()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intersect_shouldHandleNullInput()
		 internal virtual void IntersectShouldHandleNullInput()
		 {
			  assertThat( PrimitiveArrays.Intersect( null, null ), equalTo( _noLongs ) );
			  assertThat( PrimitiveArrays.Intersect( null, _noLongs ), equalTo( _noLongs ) );
			  assertThat( PrimitiveArrays.Intersect( _noLongs, null ), equalTo( _noLongs ) );
			  assertThat( PrimitiveArrays.Intersect( null, _oneLong ), equalTo( _noLongs ) );
			  assertThat( PrimitiveArrays.Intersect( _oneLong, null ), equalTo( _noLongs ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intersect_shouldHandleNonIntersectingArrays()
		 internal virtual void IntersectShouldHandleNonIntersectingArrays()
		 {
			  assertThat( PrimitiveArrays.Intersect( new long[]{ 1, 2, 3 }, new long[]{ 4, 5, 6 } ), equalTo( _noLongs ) );

			  assertThat( PrimitiveArrays.Intersect( new long[]{ 14, 15, 16 }, new long[]{ 1, 2, 3 } ), equalTo( _noLongs ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intersect_shouldHandleIntersectingArrays()
		 internal virtual void IntersectShouldHandleIntersectingArrays()
		 {
			  assertThat( PrimitiveArrays.Intersect( new long[]{ 1, 2, 3 }, new long[]{ 3, 4, 5 } ), IsArray( 3 ) );

			  assertThat( PrimitiveArrays.Intersect( new long[]{ 3, 4, 5 }, new long[]{ 1, 2, 3, 4 } ), IsArray( 3, 4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intersect_shouldHandleComplexIntersectingArraysWithGaps()
		 internal virtual void IntersectShouldHandleComplexIntersectingArraysWithGaps()
		 {
			  assertThat( PrimitiveArrays.Intersect( new long[]{ 4, 6, 9, 11, 12, 15 }, new long[]{ 2, 3, 4, 7, 8, 9, 12, 16, 19 } ), IsArray( 4, 9, 12 ) );
			  assertThat( PrimitiveArrays.Intersect( new long[]{ 2, 3, 4, 7, 8, 9, 12, 16, 19 }, new long[]{ 4, 6, 9, 11, 12, 15 } ), IsArray( 4, 9, 12 ) );
		 }

		 // symmetricDifference()

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void symDiff_shouldHandleNullInput()
		 internal virtual void SymDiffShouldHandleNullInput()
		 {
			  assertThat( PrimitiveArrays.SymmetricDifference( null, null ), equalTo( null ) );
			  assertThat( PrimitiveArrays.SymmetricDifference( null, _noLongs ), equalTo( _noLongs ) );
			  assertThat( PrimitiveArrays.SymmetricDifference( _noLongs, null ), equalTo( _noLongs ) );
			  assertThat( PrimitiveArrays.SymmetricDifference( null, _oneLong ), equalTo( _oneLong ) );
			  assertThat( PrimitiveArrays.SymmetricDifference( _oneLong, null ), equalTo( _oneLong ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void symDiff_shouldHandleNonIntersectingArrays()
		 internal virtual void SymDiffShouldHandleNonIntersectingArrays()
		 {
			  assertThat( PrimitiveArrays.SymmetricDifference( new long[]{ 1, 2, 3 }, new long[]{ 4, 5, 6 } ), IsArray( 1, 2, 3, 4, 5, 6 ) );

			  assertThat( PrimitiveArrays.SymmetricDifference( new long[]{ 14, 15, 16 }, new long[]{ 1, 2, 3 } ), IsArray( 1, 2, 3, 14, 15, 16 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void symDiff_shouldHandleIntersectingArrays()
		 internal virtual void SymDiffShouldHandleIntersectingArrays()
		 {
			  assertThat( PrimitiveArrays.SymmetricDifference( new long[]{ 1, 2, 3 }, new long[]{ 3, 4, 5 } ), IsArray( 1, 2, 4, 5 ) );

			  assertThat( PrimitiveArrays.SymmetricDifference( new long[]{ 3, 4, 5 }, new long[]{ 1, 2, 3, 4 } ), IsArray( 1, 2, 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void symDiff_shouldHandleComplexIntersectingArraysWithGaps()
		 internal virtual void SymDiffShouldHandleComplexIntersectingArraysWithGaps()
		 {
			  assertThat( PrimitiveArrays.SymmetricDifference( new long[]{ 4, 6, 9, 11, 12, 15 }, new long[]{ 2, 3, 4, 7, 8, 9, 12, 16, 19 } ), IsArray( 2, 3, 6, 7, 8, 11, 15, 16, 19 ) );
			  assertThat( PrimitiveArrays.SymmetricDifference( new long[]{ 2, 3, 4, 7, 8, 9, 12, 16, 19 }, new long[]{ 4, 6, 9, 11, 12, 15 } ), IsArray( 2, 3, 6, 7, 8, 11, 15, 16, 19 ) );
		 }

		 // count unique

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCountUnique()
		 internal virtual void ShouldCountUnique()
		 {
			  assertThat( PrimitiveArrays.CountUnique( new long[]{ 1, 2, 3 }, new long[]{ 4, 5, 6 } ), IsIntPair( 3, 3 ) );

			  assertThat( PrimitiveArrays.CountUnique( new long[]{ 1, 2, 3 }, new long[]{ 3, 6 } ), IsIntPair( 2, 1 ) );

			  assertThat( PrimitiveArrays.CountUnique( new long[]{ 1, 2, 3 }, new long[]{ 3 } ), IsIntPair( 2, 0 ) );

			  assertThat( PrimitiveArrays.CountUnique( new long[]{ 3 }, new long[]{ 1, 2, 3 } ), IsIntPair( 0, 2 ) );

			  assertThat( PrimitiveArrays.CountUnique( new long[]{ 3 }, new long[]{ 3 } ), IsIntPair( 0, 0 ) );

			  assertThat( PrimitiveArrays.CountUnique( new long[]{ 3, 6, 8 }, new long[]{} ), IsIntPair( 3, 0 ) );

			  assertThat( PrimitiveArrays.CountUnique( new long[]{}, new long[]{3, 6, 8} ), IsIntPair( 0, 3 ) );

			  assertThat( PrimitiveArrays.CountUnique( new long[]{}, new long[]{} ), IsIntPair( 0, 0 ) );

			  assertThat( PrimitiveArrays.CountUnique( new long[]{ 4, 6, 9, 11, 12, 15 }, new long[]{ 2, 3, 4, 7, 8, 9, 12, 16, 19 } ), IsIntPair( 3, 6 ) );
		 }

		 // helpers

		 private static Matcher<long> IsIntPair( int left, int right )
		 {
			  return new BaseMatcherAnonymousInnerClass( left, right );
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<long>
		 {
			 private int _left;
			 private int _right;

			 public BaseMatcherAnonymousInnerClass( int left, int right )
			 {
				 this._left = left;
				 this._right = right;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( _left );
				  description.appendValue( _right );
			 }

			 public override bool matches( object o )
			 {
				  return o is long? && ( ( long? ) o ) == ( ( ( long ) _left << 32 ) | _right );
			 }
		 }

		 private static Matcher<long[]> IsArray( params long[] values )
		 {
			  return equalTo( values );
		 }
	}

}