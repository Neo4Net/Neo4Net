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
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class NumberAwareStringComparatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleNumber()
		 public virtual void ShouldHandleSingleNumber()
		 {
			  // LESSER
			  AssertLesser( "123", "456" );
			  AssertLesser( "123", "1234" );
			  AssertLesser( "1", "12" );

			  // SAME
			  AssertSame( "123", "123" );
			  AssertSame( "001", "1" );

			  // GREATER
			  AssertGreater( "555", "66" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMixedAlthoughSimilarNumbersAndStrings()
		 public virtual void ShouldHandleMixedAlthoughSimilarNumbersAndStrings()
		 {
			  AssertLesser( "same-1-thing-45", "same-12-thing-45" );
			  AssertGreater( "same-2-thing-46", "same-2-thing-45" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMixedAndDifferentNumbersAndStrings()
		 public virtual void ShouldHandleMixedAndDifferentNumbersAndStrings()
		 {
			  AssertLesser( "same123thing456", "same123thing456andmore" );
			  AssertGreater( "same12", "same1thing456andmore" );
		 }

		 private void AssertLesser( string first, string other )
		 {
			  assertTrue( Compare( first, other ) < 0 );
		 }

		 private void AssertSame( string first, string other )
		 {
			  assertEquals( 0, Compare( first, other ) );
		 }

		 private void AssertGreater( string first, string other )
		 {
			  assertTrue( Compare( first, other ) > 0 );
		 }

		 private int Compare( string first, string other )
		 {
			  return ( new NumberAwareStringComparator() ).Compare(first, other);
		 }
	}

}