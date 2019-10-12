using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class UtilsTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectCollisions()
		 public virtual void ShouldDetectCollisions()
		 {
			  // GIVEN
			  long[] first = new long[] { 1, 4, 7, 10, 100, 101 };
			  long[] other = new long[] { 2, 3, 34, 75, 101 };

			  // WHEN
			  bool collides = Utils.AnyIdCollides( first, first.Length, other, other.Length );

			  // THEN
			  assertTrue( collides );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportDisjointArraysAsCollision()
		 public virtual void ShouldNotReportDisjointArraysAsCollision()
		 {
			  // GIVEN
			  long[] first = new long[] { 1, 4, 7, 10, 100, 101 };
			  long[] other = new long[] { 2, 3, 34, 75, 102 };

			  // WHEN
			  bool collides = Utils.AnyIdCollides( first, first.Length, other, other.Length );

			  // THEN
			  assertFalse( collides );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeCorrectForSomeRandomBatches()
		 public virtual void ShouldBeCorrectForSomeRandomBatches()
		 {
			  // GIVEN
			  Random random = ThreadLocalRandom.current();
			  long[][] batches = new long[20][];
			  for ( int i = 0; i < batches.Length; i++ )
			  {
					batches[i] = RandomBatch( 1_000, random, 5_000_000 );
			  }

			  // WHEN
			  foreach ( long[] rBatch in batches )
			  {
					foreach ( long[] lBatch in batches )
					{
						 // THEN
						 assertEquals( ActuallyCollides( rBatch, lBatch ), Utils.AnyIdCollides( rBatch, rBatch.Length, lBatch, lBatch.Length ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeIdsInto()
		 public virtual void ShouldMergeIdsInto()
		 {
			  // GIVEN
			  long[] values = new long[]{ 2, 4, 10, 11, 14 };
			  long[] into = new long[]{ 1, 5, 6, 11, 25 };
			  int intoLengthBefore = into.Length;
			  into = Arrays.copyOf( into, into.Length + values.Length );

			  // WHEN
			  Utils.MergeSortedInto( values, into, intoLengthBefore );

			  // THEN
			  assertArrayEquals( new long[] { 1, 2, 4, 5, 6, 10, 11, 11, 14, 25 }, into );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeSomeRandomIdsInto()
		 public virtual void ShouldMergeSomeRandomIdsInto()
		 {
			  // GIVEN
			  Random random = ThreadLocalRandom.current();
			  int batchSize = 10_000;

			  // WHEN
			  for ( int i = 0; i < 100; i++ )
			  {
					long[] values = RandomBatch( batchSize, random, 100_000_000 );
					long[] into = RandomBatch( batchSize, random, 100_000_000 );
					long[] expectedMergedArray = ManuallyMerge( values, into );
					into = Arrays.copyOf( into, batchSize * 2 );
					Utils.MergeSortedInto( values, into, batchSize );
					assertArrayEquals( expectedMergedArray, into );
			  }
		 }

		 private long[] ManuallyMerge( long[] values, long[] into )
		 {
			  long[] all = new long[values.Length + into.Length];
			  Array.Copy( values, 0, all, 0, values.Length );
			  Array.Copy( into, 0, all, values.Length, into.Length );
			  Arrays.sort( all );
			  return all;
		 }

		 private bool ActuallyCollides( long[] b1, long[] b2 )
		 {
			  for ( int i = 0; i < b1.Length; i++ )
			  {
					for ( int j = 0; j < b2.Length; j++ )
					{
						 if ( b1[i] == b2[j] )
						 {
							  return true;
						 }
					}
			  }
			  return false;
		 }

		 private long[] RandomBatch( int length, Random random, int max )
		 {
			  long[] result = new long[length];
			  RandomBatchInto( result, length, random, max );
			  return result;
		 }

		 private void RandomBatchInto( long[] into, int length, Random random, int max )
		 {
			  for ( int i = 0; i < length; i++ )
			  {
					into[i] = random.Next( max );
			  }
			  Arrays.sort( into, 0, length );
		 }
	}

}