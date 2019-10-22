using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	using Test = org.junit.Test;
	using Neo4Net.causalclustering.core.consensus.log.segmented.OpenEndRangeMap;

	public class OpenEndRangeMapTest
	{
		 private OpenEndRangeMap<int, string> _ranges = new OpenEndRangeMap<int, string>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindNothingInEmptyMap()
		 public virtual void ShouldFindNothingInEmptyMap()
		 {
			  AssertRange( -100, 100, new ValueRange<>( null, null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindSingleRange()
		 public virtual void ShouldFindSingleRange()
		 {
			  // when
			  _ranges.replaceFrom( 0, "A" );

			  // then
			  AssertRange( -100, -1, new ValueRange<>( 0, null ) );
			  AssertRange( 0, 100, new ValueRange<>( null, "A" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleRanges()
		 public virtual void ShouldHandleMultipleRanges()
		 {
			  // when
			  _ranges.replaceFrom( 0, "A" );
			  _ranges.replaceFrom( 5, "B" );
			  _ranges.replaceFrom( 10, "C" );

			  // then
			  AssertRange( -100, -1, new ValueRange<>( 0, null ) );
			  AssertRange( 0, 4, new ValueRange<>( 5, "A" ) );
			  AssertRange( 5, 9, new ValueRange<>( 10, "B" ) );
			  AssertRange( 10, 100, new ValueRange<>( null, "C" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateAtPreviousEntry()
		 public virtual void ShouldTruncateAtPreviousEntry()
		 {
			  // given
			  _ranges.replaceFrom( 0, "A" );
			  _ranges.replaceFrom( 10, "B" );

			  // when
			  ICollection<string> removed = _ranges.replaceFrom( 10, "C" );

			  // then
			  AssertRange( -100, -1, new ValueRange<>( 0, null ) );
			  AssertRange( 0, 9, new ValueRange<>( 10, "A" ) );
			  AssertRange( 10, 100, new ValueRange<>( null, "C" ) );

			  assertThat( removed, hasItems( "B" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateBeforePreviousEntry()
		 public virtual void ShouldTruncateBeforePreviousEntry()
		 {
			  // given
			  _ranges.replaceFrom( 0, "A" );
			  _ranges.replaceFrom( 10, "B" );

			  // when
			  ICollection<string> removed = _ranges.replaceFrom( 7, "C" );

			  // then
			  AssertRange( -100, -1, new ValueRange<>( 0, null ) );
			  AssertRange( 0, 6, new ValueRange<>( 7, "A" ) );
			  AssertRange( 7, 100, new ValueRange<>( null, "C" ) );

			  assertThat( removed, hasItems( "B" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateSeveralEntries()
		 public virtual void ShouldTruncateSeveralEntries()
		 {
			  // given
			  _ranges.replaceFrom( 0, "A" );
			  _ranges.replaceFrom( 10, "B" );
			  _ranges.replaceFrom( 20, "C" );
			  _ranges.replaceFrom( 30, "D" );

			  // when
			  ICollection<string> removed = _ranges.replaceFrom( 15, "E" );

			  // then
			  AssertRange( -100, -1, new ValueRange<>( 0, null ) );
			  AssertRange( 0, 9, new ValueRange<>( 10, "A" ) );
			  AssertRange( 10, 14, new ValueRange<>( 15, "B" ) );
			  AssertRange( 15, 100, new ValueRange<>( null, "E" ) );

			  assertThat( removed, hasItems( "C", "D" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyPruneWholeEntries()
		 public virtual void ShouldOnlyPruneWholeEntries()
		 {
			  // given
			  _ranges.replaceFrom( 0, "A" );
			  _ranges.replaceFrom( 5, "B" );

			  ICollection<string> removed;

			  // when / then
			  removed = _ranges.remove( 4 );
			  assertTrue( removed.Count == 0 );

			  // when
			  removed = _ranges.remove( 5 );
			  assertFalse( removed.Count == 0 );
			  assertThat( removed, hasItems( "A" ) );
		 }

		 private void AssertRange<T>( int from, int to, ValueRange<int, T> expected )
		 {
			  for ( int i = from; i <= to; i++ )
			  {
					ValueRange<int, string> valueRange = _ranges.lookup( i );
					assertEquals( expected, valueRange );
			  }
		 }
	}

}