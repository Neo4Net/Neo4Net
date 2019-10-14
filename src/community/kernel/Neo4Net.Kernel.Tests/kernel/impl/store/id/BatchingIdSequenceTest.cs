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

	using IdValidator = Neo4Net.Kernel.impl.store.id.validation.IdValidator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;

	public class BatchingIdSequenceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ShouldSkipNullId()
		 public virtual void ShouldSkipNullId()
		 {
			  BatchingIdSequence idSequence = new BatchingIdSequence();

			  idSequence.Set( IdGeneratorImpl.INTEGER_MINUS_ONE - 1 );
			  assertEquals( IdGeneratorImpl.INTEGER_MINUS_ONE - 1, idSequence.Peek() );

			  // The 'NULL Id' should be skipped, and never be visible anywhere.
			  // Peek should always return what nextId will return

			  assertEquals( IdGeneratorImpl.INTEGER_MINUS_ONE - 1, idSequence.NextId() );
			  assertEquals( IdGeneratorImpl.INTEGER_MINUS_ONE + 1, idSequence.Peek() );
			  assertEquals( IdGeneratorImpl.INTEGER_MINUS_ONE + 1, idSequence.NextId() );

			  // And what if someone were to set it directly to the NULL id
			  idSequence.Set( IdGeneratorImpl.INTEGER_MINUS_ONE );

			  assertEquals( IdGeneratorImpl.INTEGER_MINUS_ONE + 1, idSequence.Peek() );
			  assertEquals( IdGeneratorImpl.INTEGER_MINUS_ONE + 1, idSequence.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void resetShouldSetDefault()
		 public virtual void ResetShouldSetDefault()
		 {
			  BatchingIdSequence idSequence = new BatchingIdSequence();

			  idSequence.Set( 99L );

			  assertEquals( 99L, idSequence.Peek() );
			  assertEquals( 99L, idSequence.NextId() );
			  assertEquals( 100L, idSequence.Peek() );

			  idSequence.Reset();

			  assertEquals( 0L, idSequence.Peek() );
			  assertEquals( 0L, idSequence.NextId() );
			  assertEquals( 1L, idSequence.Peek() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipReservedIdWhenGettingBatches()
		 public virtual void ShouldSkipReservedIdWhenGettingBatches()
		 {
			  // GIVEN
			  int batchSize = 10;
			  BatchingIdSequence idSequence = new BatchingIdSequence( IdGeneratorImpl.INTEGER_MINUS_ONE - batchSize - batchSize / 2 );

			  // WHEN
			  IdRange range1 = idSequence.NextIdBatch( batchSize );
			  IdRange range2 = idSequence.NextIdBatch( batchSize );

			  // THEN
			  AssertNoReservedId( range1 );
			  AssertNoReservedId( range2 );
		 }

		 private void AssertNoReservedId( IdRange range )
		 {
			  foreach ( long id in range.DefragIds )
			  {
					assertFalse( IdValidator.isReservedId( id ) );
			  }

			  assertFalse( IdValidator.hasReservedIdInRange( range.RangeStart, range.RangeStart + range.RangeLength ) );
		 }
	}

}