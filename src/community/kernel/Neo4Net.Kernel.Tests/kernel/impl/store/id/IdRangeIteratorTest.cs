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
namespace Neo4Net.Kernel.impl.store.id
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdRangeIterator.VALUE_REPRESENTING_NULL;

	public class IdRangeIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnValueRepresentingNullIfWeExhaustIdRange()
		 public virtual void ShouldReturnValueRepresentingNullIfWeExhaustIdRange()
		 {
			  // given
			  int rangeLength = 1024;
			  IdRangeIterator iterator = ( new IdRange( new long[]{}, 0, rangeLength ) ).GetEnumerator();

			  // when
			  for ( int i = 0; i < rangeLength; i++ )
			  {
					iterator.NextId();
			  }

			  // then
			  assertEquals( IdRangeIterator.VALUE_REPRESENTING_NULL, iterator.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotHaveAnyGaps()
		 public virtual void ShouldNotHaveAnyGaps()
		 {
			  // given
			  int rangeLength = 1024;
			  IdRangeIterator iterator = ( new IdRange( new long[]{}, 0, rangeLength ) ).GetEnumerator();

			  // when
			  ISet<long> seenIds = new HashSet<long>();
			  for ( int i = 0; i < rangeLength; i++ )
			  {
					seenIds.Add( iterator.NextId() );
					if ( i > 0 )
					{
						 // then
						 assertTrue( "Missing id " + ( i - 1 ), seenIds.Contains( ( long ) i - 1 ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseDefragIdsFirst()
		 public virtual void ShouldUseDefragIdsFirst()
		 {
			  // given
			  int rangeLength = 1024;
			  IdRangeIterator iterator = ( new IdRange( new long[] { 7, 8, 9 }, 1024, rangeLength ) ).GetEnumerator();

			  // then
			  assertEquals( 7, iterator.NextId() );
			  assertEquals( 8, iterator.NextId() );
			  assertEquals( 9, iterator.NextId() );
			  assertEquals( 1024, iterator.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNextIdBatchFromOnlyDefragIds()
		 public virtual void ShouldGetNextIdBatchFromOnlyDefragIds()
		 {
			  // given
			  IdRangeIterator iterator = ( new IdRange( new long[] { 1, 2, 3, 4, 5, 6 }, 7, 0 ) ).GetEnumerator();

			  // when
			  IdRangeIterator subRange = iterator.NextIdBatch( 5 ).GetEnumerator();

			  // then
			  assertEquals( 6, iterator.NextId() );
			  for ( long i = 0; i < 5; i++ )
			  {
					assertEquals( 1 + i, subRange.NextId() );
			  }
			  assertEquals( VALUE_REPRESENTING_NULL, subRange.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNextIdBatchFromOnlyDefragIdsWhenSomeDefragIdsHaveAlreadyBeenReturned()
		 public virtual void ShouldGetNextIdBatchFromOnlyDefragIdsWhenSomeDefragIdsHaveAlreadyBeenReturned()
		 {
			  // given
			  IdRangeIterator iterator = ( new IdRange( new long[] { 1, 2, 3, 4, 5, 6 }, 7, 0 ) ).GetEnumerator();
			  iterator.NextId();
			  iterator.NextId();

			  // when
			  IdRangeIterator subRange = iterator.NextIdBatch( 3 ).GetEnumerator();

			  // then
			  assertEquals( 6, iterator.NextId() );
			  for ( long i = 0; i < 3; i++ )
			  {
					assertEquals( 3 + i, subRange.NextId() );
			  }
			  assertEquals( VALUE_REPRESENTING_NULL, subRange.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNextIdBatchFromSomeDefragAndSomeRangeIds()
		 public virtual void ShouldGetNextIdBatchFromSomeDefragAndSomeRangeIds()
		 {
			  // given
			  IdRangeIterator iterator = ( new IdRange( new long[] { 1, 2, 3 }, 10, 5 ) ).GetEnumerator();
			  iterator.NextId();

			  // when
			  IdRangeIterator subRange = iterator.NextIdBatch( 5 ).GetEnumerator();

			  // then
			  assertEquals( 13, iterator.NextId() );
			  assertEquals( 2, subRange.NextId() );
			  assertEquals( 3, subRange.NextId() );
			  assertEquals( 10, subRange.NextId() );
			  assertEquals( 11, subRange.NextId() );
			  assertEquals( 12, subRange.NextId() );
			  assertEquals( VALUE_REPRESENTING_NULL, subRange.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNextIdBatchFromSomeRangeIds()
		 public virtual void ShouldGetNextIdBatchFromSomeRangeIds()
		 {
			  // given
			  IdRangeIterator iterator = ( new IdRange( EMPTY_LONG_ARRAY, 0, 20 ) ).GetEnumerator();
			  iterator.NextId();

			  // when
			  IdRangeIterator subRange = iterator.NextIdBatch( 5 ).GetEnumerator();

			  // then
			  assertEquals( 6, iterator.NextId() );
			  assertEquals( 1, subRange.NextId() );
			  assertEquals( 2, subRange.NextId() );
			  assertEquals( 3, subRange.NextId() );
			  assertEquals( 4, subRange.NextId() );
			  assertEquals( 5, subRange.NextId() );
			  assertEquals( VALUE_REPRESENTING_NULL, subRange.NextId() );

			  // when
			  subRange = iterator.NextIdBatch( 2 ).GetEnumerator();

			  // then
			  assertEquals( 9, iterator.NextId() );
			  assertEquals( 7, subRange.NextId() );
			  assertEquals( 8, subRange.NextId() );
			  assertEquals( VALUE_REPRESENTING_NULL, subRange.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNextIdBatchFromSomeRangeIdsWhenThereAreUsedDefragIds()
		 public virtual void ShouldGetNextIdBatchFromSomeRangeIdsWhenThereAreUsedDefragIds()
		 {
			  // given
			  IdRangeIterator iterator = ( new IdRange( new long[] { 0, 1, 2 }, 3, 10 ) ).GetEnumerator();
			  iterator.NextId();
			  iterator.NextId();
			  iterator.NextId();

			  // when
			  IdRangeIterator subRange = iterator.NextIdBatch( 3 ).GetEnumerator();

			  // then
			  assertEquals( 6, iterator.NextId() );
			  assertEquals( 3, subRange.NextId() );
			  assertEquals( 4, subRange.NextId() );
			  assertEquals( 5, subRange.NextId() );
			  assertEquals( VALUE_REPRESENTING_NULL, subRange.NextId() );

			  // when
			  subRange = iterator.NextIdBatch( 3 ).GetEnumerator();

			  // then
			  assertEquals( 10, iterator.NextId() );
			  assertEquals( 7, subRange.NextId() );
			  assertEquals( 8, subRange.NextId() );
			  assertEquals( 9, subRange.NextId() );
			  assertEquals( VALUE_REPRESENTING_NULL, subRange.NextId() );
		 }
	}

}