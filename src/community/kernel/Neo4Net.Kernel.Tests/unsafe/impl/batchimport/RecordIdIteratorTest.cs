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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration.withBatchSize;

	public class RecordIdIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGoPageWiseBackwards()
		 public virtual void ShouldGoPageWiseBackwards()
		 {
			  // GIVEN
			  RecordIdIterator ids = RecordIdIterator.backwards( 0, 33, withBatchSize( DEFAULT, 10 ) );

			  // THEN
			  AssertIds( ids, Array( 30, 31, 32 ), Array( 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 ), Array( 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 ), Array( 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGoPageWiseBackwardsOnCleanBreak()
		 public virtual void ShouldGoPageWiseBackwardsOnCleanBreak()
		 {
			  // GIVEN
			  RecordIdIterator ids = RecordIdIterator.backwards( 0, 20, withBatchSize( DEFAULT, 10 ) );

			  // THEN
			  AssertIds( ids, Array( 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 ), Array( 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGoPageWiseBackwardsOnSingleBatch()
		 public virtual void ShouldGoPageWiseBackwardsOnSingleBatch()
		 {
			  // GIVEN
			  RecordIdIterator ids = RecordIdIterator.backwards( 0, 8, withBatchSize( DEFAULT, 10 ) );

			  // THEN
			  AssertIds( ids, Array( 0, 1, 2, 3, 4, 5, 6, 7 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGoBackwardsToNonZero()
		 public virtual void ShouldGoBackwardsToNonZero()
		 {
			  // GIVEN
			  RecordIdIterator ids = RecordIdIterator.backwards( 12, 34, withBatchSize( DEFAULT, 10 ) );

			  // THEN
			  AssertIds( ids, Array( 30, 31, 32, 33 ), Array( 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 ), Array( 12, 13, 14, 15, 16, 17, 18, 19 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGoForwardsWhenStartingFromNonZero()
		 public virtual void ShouldGoForwardsWhenStartingFromNonZero()
		 {
			  // GIVEN
			  RecordIdIterator ids = RecordIdIterator.forwards( 1, 12, withBatchSize( DEFAULT, 10 ) );

			  // THEN
			  AssertIds( ids, Array( 1, 2, 3, 4, 5, 6, 7, 8, 9 ), Array( 10, 11 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGoForwardsWhenStartingFromNonZero2()
		 public virtual void ShouldGoForwardsWhenStartingFromNonZero2()
		 {
			  // GIVEN
			  RecordIdIterator ids = RecordIdIterator.forwards( 34, 66, withBatchSize( DEFAULT, 10 ) );

			  // THEN
			  AssertIds( ids, Array( 34, 35, 36, 37, 38, 39 ), Array( 40, 41, 42, 43, 44, 45, 46, 47, 48, 49 ), Array( 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 ), Array( 60, 61, 62, 63, 64, 65 ) );
		 }

		 private void AssertIds( RecordIdIterator ids, params long[][] expectedIds )
		 {
			  foreach ( long[] expectedArray in expectedIds )
			  {
					LongIterator iterator = ids.NextBatch();
					assertNotNull( iterator );
					foreach ( long expectedId in expectedArray )
					{
						 assertEquals( expectedId, iterator.next() );
					}
					assertFalse( iterator.hasNext() );
			  }
			  assertNull( ids.NextBatch() );
		 }

		 private static long[] Array( params long[] ids )
		 {
			  return ids;
		 }
	}

}