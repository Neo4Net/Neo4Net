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
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;

	public class RenewableBatchIdSequenceTest
	{
		private bool InstanceFieldsInitialized = false;

		public RenewableBatchIdSequenceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_ids = new RenewableBatchIdSequence( _idSource, BATCH_SIZE, _excessIds.add );
		}

		 public const int BATCH_SIZE = 5;

		 private readonly IdSource _idSource = new IdSource();
		 private readonly IList<long> _excessIds = new List<long>();
		 private RenewableBatchIdSequence _ids;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRequestIdBatchFromSourceOnFirstCall()
		 public virtual void ShouldRequestIdBatchFromSourceOnFirstCall()
		 {
			  // given
			  assertEquals( 0, _idSource.calls );

			  // when/then
			  assertEquals( 0, _ids.nextId() );
			  assertEquals( 1, _idSource.calls );
			  for ( int i = 1; i < BATCH_SIZE; i++ )
			  {
					assertEquals( i, _ids.nextId() );
					assertEquals( 1, _idSource.calls );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRequestIdBatchFromSourceOnDepletingCurrent()
		 public virtual void ShouldRequestIdBatchFromSourceOnDepletingCurrent()
		 {
			  // given
			  assertEquals( 0, _idSource.calls );
			  for ( int i = 0; i < BATCH_SIZE; i++ )
			  {
					assertEquals( i, _ids.nextId() );
			  }
			  assertEquals( 1, _idSource.calls );

			  // when
			  long firstIdOfNextBatch = _ids.nextId();

			  // then
			  assertEquals( BATCH_SIZE, firstIdOfNextBatch );
			  assertEquals( 2, _idSource.calls );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveBackExcessIdsOnClose()
		 public virtual void ShouldGiveBackExcessIdsOnClose()
		 {
			  // given
			  for ( int i = 0; i < BATCH_SIZE / 2; i++ )
			  {
					_ids.nextId();
			  }

			  // when
			  _ids.close();

			  // then
			  assertEquals( BATCH_SIZE - BATCH_SIZE / 2, _excessIds.Count );
			  for ( long i = BATCH_SIZE / 2; i < BATCH_SIZE; i++ )
			  {
					assertTrue( _excessIds.Contains( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleCloseWithNoCurrentBatch()
		 public virtual void ShouldHandleCloseWithNoCurrentBatch()
		 {
			  // when
			  _ids.close();

			  // then
			  assertTrue( _excessIds.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyCloseOnce()
		 public virtual void ShouldOnlyCloseOnce()
		 {
			  // given
			  for ( int i = 0; i < BATCH_SIZE / 2; i++ )
			  {
					_ids.nextId();
			  }

			  // when
			  _ids.close();

			  // then
			  for ( long i = BATCH_SIZE / 2; i < BATCH_SIZE; i++ )
			  {
					assertTrue( _excessIds.RemoveAt( i ) );
			  }

			  // and when closing one more time
			  _ids.close();

			  // then
			  assertTrue( _excessIds.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContinueThroughEmptyIdBatch()
		 public virtual void ShouldContinueThroughEmptyIdBatch()
		 {
			  // given
			  IdSequence idSource = mock( typeof( IdSequence ) );
			  IEnumerator<IdRange> ranges = asList( new IdRange( EMPTY_LONG_ARRAY, 0, BATCH_SIZE ), new IdRange( EMPTY_LONG_ARRAY, BATCH_SIZE, 0 ), new IdRange( EMPTY_LONG_ARRAY, BATCH_SIZE, BATCH_SIZE ) ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( idSource.NextIdBatch( anyInt() ) ).thenAnswer(invocation => ranges.next());
			  RenewableBatchIdSequence ids = new RenewableBatchIdSequence( idSource, BATCH_SIZE, _excessIds.add );

			  // when/then
			  for ( long expectedId = 0; expectedId < BATCH_SIZE * 2; expectedId++ )
			  {
					assertEquals( expectedId, ids.NextId() );
			  }
		 }

		 private class IdSource : IdSequence
		 {
			  internal int Calls;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long NextIdConflict;

			  public override IdRange NextIdBatch( int batchSize )
			  {
					Calls++;
					try
					{
						 return new IdRange( EMPTY_LONG_ARRAY, NextIdConflict, batchSize );
					}
					finally
					{
						 NextIdConflict += batchSize;
					}
			  }

			  public override long NextId()
			  {
					throw new System.NotSupportedException( "Should not be used" );
			  }
		 }
	}

}