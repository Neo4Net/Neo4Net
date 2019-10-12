using System.Threading;

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
namespace Neo4Net.Kernel.impl.store.id
{
	using Test = org.junit.Test;


	using Predicates = Neo4Net.Function.Predicates;
	using Race = Neo4Net.Test.Race;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Suppliers.singleton;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToInt;

	public class DelayedBufferTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTheWholeWorkloadShebang() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTheWholeWorkloadShebang()
		 {
			  // GIVEN
			  const int size = 1_000;
			  const long bufferTime = 3;
			  VerifyingConsumer consumer = new VerifyingConsumer( size );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.Clock clock = org.neo4j.time.Clocks.systemClock();
			  Clock clock = Clocks.systemClock();
			  System.Func<long> chunkThreshold = clock.millis;
			  System.Predicate<long> safeThreshold = time => clock.millis() - bufferTime >= time;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DelayedBuffer<long> buffer = new DelayedBuffer<>(chunkThreshold, safeThreshold, 10, consumer);
			  DelayedBuffer<long> buffer = new DelayedBuffer<long>( chunkThreshold, safeThreshold, 10, consumer );
			  MaintenanceThread maintenance = new MaintenanceThread( buffer, 5 );
			  Race adders = new Race();
			  const int numberOfAdders = 20;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] offeredIds = new byte[size];
			  sbyte[] offeredIds = new sbyte[size];
			  for ( int i = 0; i < numberOfAdders; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int finalI = i;
					int finalI = i;
					adders.AddContestant(() =>
					{
					 for ( int j = 0; j < size; j++ )
					 {
						  if ( j % numberOfAdders == finalI )
						  {
								buffer.Offer( j );
								offeredIds[j] = 1;
								parkNanos( MILLISECONDS.toNanos( current().Next(2) ) );
						  }
					 }
					});
			  }

			  // WHEN (multi-threaded) offering of ids
			  adders.Go();
			  // ... ensuring the test is sane itself (did we really offer all these IDs?)
			  for ( int i = 0; i < size; i++ )
			  {
					assertEquals( "ID " + i, ( sbyte ) 1, offeredIds[i] );
			  }
			  maintenance.Halt();
			  buffer.Close();

			  // THEN
			  consumer.AssertHaveOnlySeenRange( 0, size - 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReleaseValuesUntilCrossedThreshold()
		 public virtual void ShouldNotReleaseValuesUntilCrossedThreshold()
		 {
			  // GIVEN
			  VerifyingConsumer consumer = new VerifyingConsumer( 30 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong txOpened = new java.util.concurrent.atomic.AtomicLong();
			  AtomicLong txOpened = new AtomicLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong txClosed = new java.util.concurrent.atomic.AtomicLong();
			  AtomicLong txClosed = new AtomicLong();
			  System.Func<long> chunkThreshold = txOpened.get;
			  System.Predicate<long> safeThreshold = value => txClosed.get() >= value;
			  DelayedBuffer<long> buffer = new DelayedBuffer<long>( chunkThreshold, safeThreshold, 100, consumer );

			  // Transaction spans like these:
			  //    1 |-1--------2-------3---|
			  //    2   |4--5---------|
			  //    3       |---------6----|
			  //    4        |7--8-|
			  //    5          |--------9-------10-|
			  //    6                  |--11----|
			  //    7                    |-12---13---14--|
			  // TIME|1-2-3-4-5-6-7-8-9-a-b-c-d-e-f-g-h-i-j|
			  //  POI     ^   ^     ^         ^     ^     ^
			  //          A   B     C         D     E     F

			  // A
			  txOpened.incrementAndGet(); // <-- TX 1
			  buffer.Offer( 1 );
			  txOpened.incrementAndGet(); // <-- TX 2
			  buffer.Offer( 4 );
			  buffer.Maintenance();
			  assertEquals( 0, consumer.ChunksAccepted() );

			  // B
			  buffer.Offer( 5 );
			  txOpened.incrementAndGet(); // <-- TX 3
			  txOpened.incrementAndGet(); // <-- TX 4
			  buffer.Offer( 7 );
			  buffer.Maintenance();
			  assertEquals( 0, consumer.ChunksAccepted() );

			  // C
			  txOpened.incrementAndGet(); // <-- TX 5
			  buffer.Offer( 2 );
			  buffer.Offer( 8 );
			  // TX 4 closes, but TXs with lower ids are still open
			  buffer.Maintenance();
			  assertEquals( 0, consumer.ChunksAccepted() );

			  // D
			  // TX 2 closes, but TXs with lower ids are still open
			  buffer.Offer( 6 );
			  txOpened.incrementAndGet(); // <-- TX 6
			  buffer.Offer( 9 );
			  buffer.Offer( 3 );
			  txOpened.incrementAndGet(); // <-- TX 7
			  buffer.Offer( 11 );
			  // TX 3 closes, but TXs with lower ids are still open
			  buffer.Offer( 12 );
			  txClosed.set( 4 ); // since 1-4 have now all closed
			  buffer.Maintenance();
			  consumer.AssertHaveOnlySeen( 1, 4, 5, 7 );

			  // E
			  buffer.Offer( 10 );
			  // TX 6 closes, but TXs with lower ids are still open
			  buffer.Offer( 13 );
			  txClosed.set( 6 ); // since 1-6 have now all closed
			  buffer.Maintenance();
			  consumer.AssertHaveOnlySeen( 1, 2, 4, 5, 7, 8 );

			  // F
			  buffer.Offer( 14 );
			  txClosed.set( 7 ); // since 1-7 have now all closed
			  buffer.Maintenance();
			  consumer.AssertHaveOnlySeen( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearCurrentChunk()
		 public virtual void ShouldClearCurrentChunk()
		 {
			  // GIVEN
			  System.Action<long[]> consumer = mock( typeof( System.Action ) );
			  DelayedBuffer<long> buffer = new DelayedBuffer<long>( singleton( 0L ), Predicates.alwaysTrue(), 10, consumer );
			  buffer.Offer( 0 );
			  buffer.Offer( 1 );
			  buffer.Offer( 2 );

			  // WHEN
			  buffer.Clear();
			  buffer.Maintenance();

			  // THEN
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearPreviousChunks()
		 public virtual void ShouldClearPreviousChunks()
		 {
			  // GIVEN
			  System.Action<long[]> consumer = mock( typeof( System.Action ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean safeThreshold = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean safeThreshold = new AtomicBoolean( false );
			  DelayedBuffer<long> buffer = new DelayedBuffer<long>( singleton( 0L ), t => safeThreshold.get(), 10, consumer );
			  // three chunks
			  buffer.Offer( 0 );
			  buffer.Maintenance();
			  buffer.Offer( 1 );
			  buffer.Maintenance();
			  buffer.Offer( 2 );
			  buffer.Maintenance();

			  // WHEN
			  safeThreshold.set( true );
			  buffer.Clear();
			  buffer.Maintenance();

			  // THEN
			  verifyNoMoreInteractions( consumer );
		 }

		 private class MaintenanceThread : Thread
		 {
			  internal readonly DelayedBuffer Buffer;
			  internal readonly long NanoInterval;
			  internal volatile bool End;

			  internal MaintenanceThread( DelayedBuffer buffer, long nanoInterval )
			  {
					this.Buffer = buffer;
					this.NanoInterval = nanoInterval;
					start();
			  }

			  public override void Run()
			  {
					while ( !End )
					{
						 Buffer.maintenance();
						 LockSupport.parkNanos( NanoInterval );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void halt() throws InterruptedException
			  internal virtual void Halt()
			  {
					End = true;
					while ( Alive )
					{
						 Thread.Sleep( 1 );
					}
			  }
		 }

		 private class VerifyingConsumer : System.Action<long[]>
		 {
			  internal readonly bool[] SeenIds;
			  internal int ChunkCount;

			  internal VerifyingConsumer( int size )
			  {
					SeenIds = new bool[size];
			  }

			  internal virtual void AssertHaveOnlySeenRange( long low, long high )
			  {
					long[] values = new long[( int )( high - low + 1 )];
					for ( long id = low, i = 0; id <= high; id++, i++ )
					{
						 values[( int ) i] = id;
					}
					AssertHaveOnlySeen( values );
			  }

			  public override void Accept( long[] chunk )
			  {
					ChunkCount++;
					foreach ( long id in chunk )
					{
						 assertFalse( SeenIds[safeCastLongToInt( id )] );
						 SeenIds[safeCastLongToInt( id )] = true;
					}
			  }

			  internal virtual void AssertHaveOnlySeen( params long[] values )
			  {
					for ( int i = 0, vi = 0; i < SeenIds.Length && vi < values.Length; i++ )
					{
						 bool expectedToBeSeen = values[vi] == i;
						 if ( expectedToBeSeen && !SeenIds[i] )
						 {
							  fail( "Expected to have seen " + i + ", but hasn't" );
						 }
						 else if ( !expectedToBeSeen && SeenIds[i] )
						 {
							  fail( "Expected to NOT have seen " + i + ", but have" );
						 }

						 if ( expectedToBeSeen )
						 {
							  vi++;
						 }
					}
			  }

			  internal virtual int ChunksAccepted()
			  {
					return ChunkCount;
			  }
		 }
	}

}