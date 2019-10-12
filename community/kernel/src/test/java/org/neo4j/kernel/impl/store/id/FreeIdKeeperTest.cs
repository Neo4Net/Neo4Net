using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.store.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.id.IdContainer.NO_RESULT;

	public class FreeIdKeeperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newlyConstructedInstanceShouldReportProperDefaultValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NewlyConstructedInstanceShouldReportProperDefaultValues()
		 {
			  // Given
			  FreeIdKeeper keeper = FreeIdKeeperAggressive;

			  // then
			  assertEquals( NO_RESULT, keeper.Id );
			  assertEquals( 0, keeper.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void freeingAnIdShouldReturnThatIdAndUpdateTheCountWhenAggressiveModeIsSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FreeingAnIdShouldReturnThatIdAndUpdateTheCountWhenAggressiveModeIsSet()
		 {
			  // Given
			  FreeIdKeeper keeper = FreeIdKeeperAggressive;

			  // when
			  keeper.FreeId( 13 );

			  // then
			  assertEquals( 1, keeper.Count );

			  // when
			  long result = keeper.Id;

			  // then
			  assertEquals( 13, result );
			  assertEquals( 0, keeper.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnMinusOneWhenRunningOutOfIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnMinusOneWhenRunningOutOfIds()
		 {
			  // Given
			  FreeIdKeeper keeper = FreeIdKeeperAggressive;

			  // when
			  keeper.FreeId( 13 );

			  // then
			  assertEquals( 13, keeper.Id );
			  assertEquals( NO_RESULT, keeper.Id );
			  assertEquals( NO_RESULT, keeper.Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyOverflowWhenThresholdIsReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyOverflowWhenThresholdIsReached()
		 {
			  // Given
			  StoreChannel channel = spy( Fs.get().open(new File("id.file"), OpenMode.READ_WRITE) );

			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeperAggressive( channel, batchSize );
			  reset( channel ); // because we get the position in the constructor, we need to reset all calls on the spy

			  // when
			  // we free 9 ids
			  for ( int i = 0; i < batchSize - 1; i++ )
			  {
					keeper.FreeId( i );
			  }

			  // then
			  verifyZeroInteractions( channel );

			  // when we free one more
			  keeper.FreeId( 10 );

			  // then
			  verify( channel ).writeAll( any( typeof( ByteBuffer ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadBackPersistedIdsWhenAggressiveModeIsSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadBackPersistedIdsWhenAggressiveModeIsSet()
		 {
			  // given
			  StoreChannel channel = StoreChannel;

			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeperAggressive( channel, batchSize );

			  // when
			  // we store enough ids to cause overflow to file
			  for ( int i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
			  }

			  // then
			  // they should be returned in order
			  for ( int i = 0; i < batchSize; i++ )
			  {
					assertEquals( i, keeper.Id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadBackManyPersistedIdBatchesWhenAggressiveModeIsSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadBackManyPersistedIdBatchesWhenAggressiveModeIsSet()
		 {
			  // given
			  StoreChannel channel = StoreChannel;

			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeperAggressive( channel, batchSize );
			  ISet<long> freeIds = new HashSet<long>();

			  // when
			  // we store enough ids to cause overflow to file, in two batches
			  for ( long i = 0; i < batchSize * 2; i++ )
			  {
					keeper.FreeId( i );
					freeIds.Add( i );
			  }

			  // then
			  // they should be returned
			  assertEquals( freeIds.Count, keeper.Count );
			  for ( int i = batchSize * 2 - 1; i >= 0; i-- )
			  {
					assertTrue( freeIds.remove( keeper.Id ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFirstReturnNonPersistedIdsAndThenPersistedOnesWhenAggressiveMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFirstReturnNonPersistedIdsAndThenPersistedOnesWhenAggressiveMode()
		 {
			  // this is testing the stack property, but from the viewpoint of avoiding unnecessary disk reads
			  // given
			  StoreChannel channel = StoreChannel;

			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeperAggressive( channel, batchSize );

			  // when
			  // we store enough ids to cause overflow to file
			  for ( int i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
			  }
			  // and then some more
			  int extraIds = 3;
			  for ( int i = batchSize; i < batchSize + extraIds; i++ )
			  {
					keeper.FreeId( i );
			  }

			  // then
			  // the first returned should be the newly freed ones
			  for ( int i = batchSize; i < batchSize + extraIds; i++ )
			  {
					assertEquals( i, keeper.Id );
			  }
			  // and then there should be the persisted ones
			  for ( int i = 0; i < batchSize; i++ )
			  {
					assertEquals( i, keeper.Id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void persistedIdsShouldStillBeCounted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PersistedIdsShouldStillBeCounted()
		 {
			  // given
			  StoreChannel channel = StoreChannel;

			  int batchSize = 10;
			  FreeIdKeeper keeper = new FreeIdKeeper( channel, batchSize, true );

			  // when
			  // we store enough ids to cause overflow to file
			  for ( int i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
			  }
			  // and then some more
			  int extraIds = 3;
			  for ( int i = batchSize; i < batchSize + extraIds; i++ )
			  {
					keeper.FreeId( i );
			  }

			  // then
			  // the count should be returned correctly
			  assertEquals( batchSize + extraIds, keeper.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreAndRestoreIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStoreAndRestoreIds()
		 {
			  // given
			  StoreChannel channel = StoreChannel;

			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeperAggressive( channel, batchSize );
			  ISet<long> freeIds = new HashSet<long>(); // stack guarantees are not maintained between restarts

			  // when
			  // we store enough ids to cause overflow to file
			  for ( long i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
					freeIds.Add( i );
			  }
			  // and then some more
			  int extraIds = 3;
			  for ( long i = batchSize; i < batchSize + extraIds; i++ )
			  {
					keeper.FreeId( i );
					freeIds.Add( i );
			  }
			  // and then we close the keeper
			  keeper.Dispose();
			  channel.close();
			  // and then we open a new one over the same file
			  channel = Fs.get().open(new File("id.file"), OpenMode.READ_WRITE);
			  keeper = GetFreeIdKeeperAggressive( channel, batchSize );

			  // then
			  // the count should be returned correctly
			  assertEquals( batchSize + extraIds, keeper.Count );
			  assertEquals( freeIds.Count, keeper.Count );
			  // and the ids, including the ones that did not cause a write, are still there (as a stack)
			  for ( int i = batchSize + extraIds - 1; i >= 0; i-- )
			  {
					long id = keeper.Id;
					assertTrue( freeIds.Contains( id ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnNewlyReleasedIdsIfAggressiveIsFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnNewlyReleasedIdsIfAggressiveIsFalse()
		 {
			  // given
			  FreeIdKeeper keeper = FreeIdKeeper;

			  // when
			  keeper.FreeId( 1 );
			  long nextFree = keeper.Id;

			  // then
			  assertEquals( NO_RESULT, nextFree );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnIdsPersistedDuringThisRunIfAggressiveIsFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnIdsPersistedDuringThisRunIfAggressiveIsFalse()
		 {
			  // given
			  StoreChannel channel = spy( Fs.get().open(new File("id.file"), OpenMode.READ_WRITE) );

			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeper( channel, batchSize );

			  // when
			  // enough ids are persisted to overflow
			  for ( int i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
			  }

			  // then
			  // stuff must have been written to disk
			  verify( channel, times( 1 ) ).write( any( typeof( ByteBuffer ) ) );
			  // and no ids can be returned
			  assertEquals( NO_RESULT, keeper.Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnIdsRestoredAndIgnoreNewlyReleasedIfAggressiveModeIsFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnIdsRestoredAndIgnoreNewlyReleasedIfAggressiveModeIsFalse()
		 {
			  // given
			  StoreChannel channel = StoreChannel;

			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeper( channel, batchSize );
			  ISet<long> freeIds = new HashSet<long>();
			  for ( long i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
					freeIds.Add( i );
			  }
			  keeper.Dispose();
			  channel.close();
			  // and then we open a new one over the same file
			  channel = Fs.get().open(new File("id.file"), OpenMode.READ_WRITE);
			  keeper = GetFreeIdKeeper( channel, batchSize );

			  // when
			  // we release some ids that spill to disk
			  for ( int i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
			  }

			  // then
			  // we should retrieve all ids restored
			  for ( int i = 0; i < batchSize; i++ )
			  {
					assertTrue( freeIds.remove( keeper.Id ) );
			  }

			  // then
			  // we should have no ids to return
			  assertEquals( NO_RESULT, keeper.Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoResultIfIdsAreRestoredAndExhaustedAndThereAreFreeIdsFromThisRunWithAggressiveFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNoResultIfIdsAreRestoredAndExhaustedAndThereAreFreeIdsFromThisRunWithAggressiveFalse()
		 {
			  // given
			  StoreChannel channel = StoreChannel;

			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeper( channel, batchSize );
			  ISet<long> freeIds = new HashSet<long>();
			  for ( long i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
					freeIds.Add( i );
			  }
			  keeper.Dispose();
			  channel.close();
			  // and then we open a new one over the same file
			  channel = Fs.get().open(new File("id.file"), OpenMode.READ_WRITE);
			  keeper = GetFreeIdKeeper( channel, batchSize );

			  // when - then
			  // we exhaust all ids restored
			  for ( int i = 0; i < batchSize; i++ )
			  {
					assertTrue( freeIds.remove( keeper.Id ) );
			  }

			  // when
			  // we release some ids that spill to disk
			  for ( int i = 0; i < batchSize; i++ )
			  {
					keeper.FreeId( i );
			  }

			  // then
			  // we should have no ids to return
			  assertEquals( NO_RESULT, keeper.Id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnReusedIdsAfterRestart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnReusedIdsAfterRestart()
		 {
			  // given
			  StoreChannel channel = StoreChannel;
			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeperAggressive( channel, batchSize );
			  long idGen = 0;

			  // free 4 batches
			  for ( long i = 0; i < batchSize * 4; i++ )
			  {
					keeper.FreeId( idGen++ );
			  }

			  // reuse 2 batches
			  IList<long> reusedIds = new List<long>();
			  for ( int i = 0; i < batchSize * 2; i++ )
			  {
					long id = keeper.Id;
					reusedIds.Add( id );
			  }

			  // when
			  keeper.Dispose();
			  channel.close();

			  channel = StoreChannel;
			  keeper = GetFreeIdKeeper( channel, batchSize );

			  IList<long> remainingIds = new List<long>();
			  long id;
			  while ( ( id = keeper.Id ) != IdContainer.NO_RESULT )
			  {
					remainingIds.Add( id );
			  }

			  assertEquals( 2 * batchSize, remainingIds.Count );

			  // then
			  foreach ( long? remainingId in remainingIds )
			  {
					assertFalse( reusedIds.Contains( remainingId ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateFileInAggressiveMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateFileInAggressiveMode()
		 {
			  // given
			  StoreChannel channel = StoreChannel;
			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeperAggressive( channel, batchSize );

			  // free 4 batches
			  for ( long i = 0; i < batchSize * 4; i++ )
			  {
					keeper.FreeId( i );
			  }
			  assertEquals( channel.size(), 4 * batchSize * Long.BYTES );

			  // when
			  for ( int i = 0; i < batchSize * 2; i++ )
			  {
					keeper.Id;
			  }

			  // then
			  assertEquals( channel.size(), 2 * batchSize * Long.BYTES );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompactFileOnCloseInRegularMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompactFileOnCloseInRegularMode()
		 {
			  // given
			  StoreChannel channel = StoreChannel;
			  int batchSize = 10;
			  FreeIdKeeper keeper = GetFreeIdKeeper( channel, batchSize );

			  // free 4 batches
			  for ( long i = 0; i < batchSize * 4; i++ )
			  {
					keeper.FreeId( i );
			  }

			  keeper.Dispose();
			  assertEquals( channel.size(), 4 * batchSize * Long.BYTES );
			  channel.close();

			  // after opening again the IDs should be free to reuse
			  channel = StoreChannel;
			  keeper = GetFreeIdKeeper( channel, batchSize );

			  // free 4 more batches on top of the already existing 4
			  for ( long i = 0; i < batchSize * 4; i++ )
			  {
					keeper.FreeId( i );
			  }

			  // fetch 2 batches
			  for ( int i = 0; i < batchSize * 2; i++ )
			  {
					keeper.Id;
			  }

			  keeper.Dispose();

			  // when
			  assertEquals( channel.size(), 6 * batchSize * Long.BYTES );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allocateEmptyBatchWhenNoIdsAreAvailable() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllocateEmptyBatchWhenNoIdsAreAvailable()
		 {
			  FreeIdKeeper freeIdKeeper = FreeIdKeeperAggressive;
			  long[] ids = freeIdKeeper.GetIds( 1024 );
			  assertSame( PrimitiveLongCollections.EMPTY_LONG_ARRAY, ids );
			  assertEquals( 0, freeIdKeeper.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allocateBatchWhenHaveMoreIdsInMemory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllocateBatchWhenHaveMoreIdsInMemory()
		 {
			  FreeIdKeeper freeIdKeeper = FreeIdKeeperAggressive;
			  for ( long id = 1L; id < 7L; id++ )
			  {
					freeIdKeeper.FreeId( id );
			  }
			  long[] ids = freeIdKeeper.GetIds( 5 );
			  assertArrayEquals( new long[]{ 1L, 2L, 3L, 4L, 5L }, ids );
			  assertEquals( 1, freeIdKeeper.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allocateBatchWhenHaveLessIdsInMemory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllocateBatchWhenHaveLessIdsInMemory()
		 {
			  FreeIdKeeper freeIdKeeper = FreeIdKeeperAggressive;
			  for ( long id = 1L; id < 4L; id++ )
			  {
					freeIdKeeper.FreeId( id );
			  }
			  long[] ids = freeIdKeeper.GetIds( 5 );
			  assertArrayEquals( new long[]{ 1L, 2L, 3L }, ids );
			  assertEquals( 0, freeIdKeeper.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allocateBatchWhenHaveLessIdsInMemoryButHaveOnDiskMore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllocateBatchWhenHaveLessIdsInMemoryButHaveOnDiskMore()
		 {
			  FreeIdKeeper freeIdKeeper = GetFreeIdKeeperAggressive( 4 );
			  for ( long id = 1L; id < 11L; id++ )
			  {
					freeIdKeeper.FreeId( id );
			  }
			  long[] ids = freeIdKeeper.GetIds( 7 );
			  assertArrayEquals( new long[]{ 9L, 10L, 5L, 6L, 7L, 8L, 1L }, ids );
			  assertEquals( 3, freeIdKeeper.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allocateBatchWhenHaveLessIdsInMemoryAndOnDisk() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllocateBatchWhenHaveLessIdsInMemoryAndOnDisk()
		 {
			  FreeIdKeeper freeIdKeeper = GetFreeIdKeeperAggressive( 4 );
			  for ( long id = 1L; id < 10L; id++ )
			  {
					freeIdKeeper.FreeId( id );
			  }
			  long[] ids = freeIdKeeper.GetIds( 15 );
			  assertArrayEquals( new long[]{ 9L, 5L, 6L, 7L, 8L, 1L, 2L, 3L, 4L }, ids );
			  assertEquals( 0, freeIdKeeper.Count );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private FreeIdKeeper getFreeIdKeeperAggressive() throws java.io.IOException
		 private FreeIdKeeper FreeIdKeeperAggressive
		 {
			 get
			 {
				  return GetFreeIdKeeperAggressive( StoreChannel, 10 );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private FreeIdKeeper getFreeIdKeeperAggressive(int batchSize) throws java.io.IOException
		 private FreeIdKeeper getFreeIdKeeperAggressive( int batchSize )
		 {
			  return GetFreeIdKeeperAggressive( StoreChannel, batchSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private FreeIdKeeper getFreeIdKeeperAggressive(org.neo4j.io.fs.StoreChannel channel, int batchSize) throws java.io.IOException
		 private FreeIdKeeper getFreeIdKeeperAggressive( StoreChannel channel, int batchSize )
		 {
			  return GetFreeIdKeeper( channel, batchSize, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private FreeIdKeeper getFreeIdKeeper(org.neo4j.io.fs.StoreChannel channel, int batchSize) throws java.io.IOException
		 private FreeIdKeeper GetFreeIdKeeper( StoreChannel channel, int batchSize )
		 {
			  return GetFreeIdKeeper( channel, batchSize, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private FreeIdKeeper getFreeIdKeeper() throws java.io.IOException
		 private FreeIdKeeper GetFreeIdKeeper()
		 {
			  return GetFreeIdKeeper( StoreChannel, 10 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private FreeIdKeeper getFreeIdKeeper(org.neo4j.io.fs.StoreChannel channel, int batchSize, boolean aggressiveMode) throws java.io.IOException
		 private FreeIdKeeper GetFreeIdKeeper( StoreChannel channel, int batchSize, bool aggressiveMode )
		 {
			  return new FreeIdKeeper( channel, batchSize, aggressiveMode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.io.fs.StoreChannel getStoreChannel() throws java.io.IOException
		 private StoreChannel StoreChannel
		 {
			 get
			 {
				  return Fs.get().open(new File("id.file"), OpenMode.READ_WRITE);
			 }
		 }
	}

}