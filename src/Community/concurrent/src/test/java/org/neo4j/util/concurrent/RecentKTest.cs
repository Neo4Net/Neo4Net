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
namespace Neo4Net.Util.concurrent
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;

	internal class RecentKTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEvictOnOverflow()
		 internal virtual void ShouldEvictOnOverflow()
		 {
			  // When & Then
			  assertThat( AppendSequence( 1, 1, 1, 1, 1, 1, 1 ), YieldsSet( 1 ) );
			  assertThat( AppendSequence( 1, 2, 3, 4, 1, 1, 1 ), YieldsSet( 1, 3, 4 ) );
			  assertThat( AppendSequence( 1, 1, 1, 2, 2, 6, 4, 4, 1, 1, 2, 2, 2, 5, 5 ), YieldsSet( 1, 2, 5 ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.hamcrest.Matcher<RecentK<int>> yieldsSet(final Integer... expectedItems)
		 private Matcher<RecentK<int>> YieldsSet( params Integer[] expectedItems )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( this, expectedItems );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<RecentK<int>>
		 {
			 private readonly RecentKTest _outerInstance;

			 private Integer[] _expectedItems;

			 public TypeSafeMatcherAnonymousInnerClass( RecentKTest outerInstance, Integer[] expectedItems )
			 {
				 this.outerInstance = outerInstance;
				 this._expectedItems = expectedItems;
			 }

			 protected internal override bool matchesSafely( RecentK<int> recentK )
			 {
				  assertThat( recentK.RecentItems(), containsInAnyOrder(_expectedItems) );
				  assertThat( recentK.RecentItems().Count, equalTo(_expectedItems.Length) );
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValueList( "[", ",", "]", _expectedItems );
			 }
		 }

		 private RecentK<int> AppendSequence( params int[] items )
		 {
			  RecentK<int> recentK = new RecentK<int>( 3 );
			  foreach ( int item in items )
			  {
					recentK.Add( item );
			  }
			  return recentK;
		 }
	}

}