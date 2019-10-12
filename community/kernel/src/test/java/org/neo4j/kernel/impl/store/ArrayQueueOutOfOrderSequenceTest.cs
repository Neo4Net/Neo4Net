using System;
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
namespace Org.Neo4j.Kernel.impl.store
{
	using Test = org.junit.Test;


	using ArrayQueueOutOfOrderSequence = Org.Neo4j.Kernel.impl.util.ArrayQueueOutOfOrderSequence;
	using OutOfOrderSequence = Org.Neo4j.Kernel.impl.util.OutOfOrderSequence;
	using Race = Org.Neo4j.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ArrayQueueOutOfOrderSequenceTest
	{
		 private readonly long[] _emptyMeta = new long[]{ 42L };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExposeGapFreeSequenceSingleThreaded()
		 public virtual void ShouldExposeGapFreeSequenceSingleThreaded()
		 {
			  // GIVEN
			  OutOfOrderSequence sequence = new ArrayQueueOutOfOrderSequence( 0L, 10, new long[1] );

			  // WHEN/THEN
			  sequence.Offer( 1, new long[]{ 1 } );
			  AssertGet( sequence, 1, new long[]{ 1 } );

			  sequence.Offer( 2, new long[]{ 2 } );
			  AssertGet( sequence, 2, new long[]{ 2 } );

			  sequence.Offer( 4, new long[]{ 3 } );
			  AssertGet( sequence, 2, new long[]{ 2 } );

			  sequence.Offer( 3, new long[]{ 4 } );
			  AssertGet( sequence, 4, new long[]{ 3 } );

			  sequence.Offer( 5, new long[]{ 5 } );
			  AssertGet( sequence, 5, new long[]{ 5 } );

			  // AND WHEN/THEN
			  sequence.Offer( 10, new long[]{ 6 } );
			  sequence.Offer( 11, new long[]{ 7 } );
			  sequence.Offer( 8, new long[]{ 8 } );
			  sequence.Offer( 9, new long[]{ 9 } );
			  sequence.Offer( 7, new long[]{ 10 } );
			  AssertGet( sequence, 5, new long[]{ 5 } );
			  sequence.Offer( 6, new long[]{ 11 } );
			  AssertGet( sequence, 11L, new long[]{ 7 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtendArrayIfNeedBe()
		 public virtual void ShouldExtendArrayIfNeedBe()
		 {
			  // GIVEN
			  OutOfOrderSequence sequence = new ArrayQueueOutOfOrderSequence( 0L, 5, new long[1] );

			  sequence.Offer( 3L, new long[]{ 0 } );
			  sequence.Offer( 2L, new long[]{ 1 } );
			  sequence.Offer( 5L, new long[]{ 2 } );
			  sequence.Offer( 4L, new long[]{ 3 } );

			  // WHEN offering a number that should result in extending the array
			  sequence.Offer( 6L, new long[]{ 4 } );
			  // and WHEN offering the missing number to fill the gap
			  sequence.Offer( 1L, new long[]{ 5 } );

			  // THEN the high number should be visible
			  AssertGet( sequence, 6L, new long[]{ 4 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDealWithThisScenario()
		 public virtual void ShouldDealWithThisScenario()
		 {
			  // GIVEN
			  OutOfOrderSequence sequence = new ArrayQueueOutOfOrderSequence( 0, 5, new long[1] );
			  assertTrue( sequence.Offer( 1, new long[]{ 0 } ) );
			  assertFalse( sequence.Offer( 3, new long[]{ 0 } ) );
			  assertFalse( sequence.Offer( 4, new long[]{ 0 } ) );
			  assertTrue( sequence.Offer( 2, new long[]{ 0 } ) );
			  assertFalse( sequence.Offer( 6, new long[]{ 0 } ) );
			  assertTrue( sequence.Offer( 5, new long[]{ 0 } ) );
			  // leave out 7
			  assertFalse( sequence.Offer( 8, new long[]{ 0 } ) );
			  assertFalse( sequence.Offer( 9, new long[]{ 0 } ) );
			  assertFalse( sequence.Offer( 10, new long[]{ 0 } ) );
			  assertFalse( sequence.Offer( 11, new long[]{ 0 } ) );
			  // putting 12 should need extending the backing queue array
			  assertFalse( sequence.Offer( 12, new long[]{ 0 } ) );
			  assertFalse( sequence.Offer( 13, new long[]{ 0 } ) );
			  assertFalse( sequence.Offer( 14, new long[]{ 0 } ) );

			  // WHEN finally offering nr 7
			  assertTrue( sequence.Offer( 7, new long[]{ 0 } ) );

			  // THEN the number should jump to 14
			  AssertGet( sequence, 14, new long[]{ 0 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepItsCoolWhenMultipleThreadsAreHammeringIt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepItsCoolWhenMultipleThreadsAreHammeringIt()
		 {
			  // An interesting note is that during tests the call to sequence#offer made no difference
			  // in performance, so there seems to be no visible penalty in using ArrayQueueOutOfOrderSequence.

			  // GIVEN a sequence with intentionally low starting queue size
			  System.Func<long, long[]> metaFunction = number => new long[]{ number + 2, number * 2 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong numberSource = new java.util.concurrent.atomic.AtomicLong();
			  AtomicLong numberSource = new AtomicLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.OutOfOrderSequence sequence = new org.neo4j.kernel.impl.util.ArrayQueueOutOfOrderSequence(numberSource.get(), 5, metaFunction.apply(numberSource.get()));
			  OutOfOrderSequence sequence = new ArrayQueueOutOfOrderSequence( numberSource.get(), 5, metaFunction(numberSource.get()) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Race race = new org.neo4j.test.Race().withEndCondition(() -> numberSource.get() > 10_000_000);
			  Race race = ( new Race() ).withEndCondition(() => numberSource.get() > 10_000_000);
			  int offerThreads = max( 2, Runtime.Runtime.availableProcessors() - 1 );
			  race.AddContestants(offerThreads, () =>
			  {
				long number = numberSource.incrementAndGet();
				sequence.Offer( number, metaFunction( number ) );
			  });
			  ThreadStart verifier = () =>
			  {
				long[] highest = sequence.Get();
				long[] expectedMeta = metaFunction( highest[0] );
				assertArrayEquals( expectedMeta, copyOfRange( highest, 1, highest.Length ) );
			  };
			  race.AddContestant( verifier );
			  race.Go();

			  // THEN
			  verifier.run();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void highestEverSeenTest()
		 public virtual void HighestEverSeenTest()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.OutOfOrderSequence sequence = new org.neo4j.kernel.impl.util.ArrayQueueOutOfOrderSequence(0, 5, EMPTY_META);
			  OutOfOrderSequence sequence = new ArrayQueueOutOfOrderSequence( 0, 5, _emptyMeta );
			  assertEquals( 0L, sequence.HighestEverSeen() );

			  sequence.Offer( 1L, _emptyMeta );
			  assertEquals( 1L, sequence.HighestEverSeen() );

			  sequence.Offer( 42L, _emptyMeta );
			  assertEquals( 42L, sequence.HighestEverSeen() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToTimeoutWaitingForNumber() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToTimeoutWaitingForNumber()
		 {
			  // given
			  long timeout = 10;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.OutOfOrderSequence sequence = new org.neo4j.kernel.impl.util.ArrayQueueOutOfOrderSequence(3, 5, EMPTY_META);
			  OutOfOrderSequence sequence = new ArrayQueueOutOfOrderSequence( 3, 5, _emptyMeta );

			  long startTime = DateTimeHelper.CurrentUnixTimeMillis();
			  try
			  {
					// when
					sequence.Await( 4, timeout );
					fail();
			  }
			  catch ( TimeoutException )
			  {
					// expected
			  }

			  long endTime = DateTimeHelper.CurrentUnixTimeMillis();
			  assertThat( endTime - startTime, greaterThanOrEqualTo( timeout ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReturnImmediatelyWhenNumberAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReturnImmediatelyWhenNumberAvailable()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.OutOfOrderSequence sequence = new org.neo4j.kernel.impl.util.ArrayQueueOutOfOrderSequence(4, 5, EMPTY_META);
			  OutOfOrderSequence sequence = new ArrayQueueOutOfOrderSequence( 4, 5, _emptyMeta );

			  // when
			  sequence.Await( 4, 0 );

			  // then: should return without exceptions
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeNotifiedWhenNumberAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeNotifiedWhenNumberAvailable()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Semaphore done = new java.util.concurrent.Semaphore(0);
			  Semaphore done = new Semaphore( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.OutOfOrderSequence sequence = new org.neo4j.kernel.impl.util.ArrayQueueOutOfOrderSequence(3, 5, EMPTY_META);
			  OutOfOrderSequence sequence = new ArrayQueueOutOfOrderSequence( 3, 5, _emptyMeta );

			  Thread numberWaiter = new Thread(() =>
			  {
				try
				{
					 sequence.Await( 5, 60_000 );
				}
				catch ( Exception e ) when ( e is TimeoutException || e is InterruptedException )
				{
					 fail( "Should not have thrown" );
				}

				done.release();
			  });

			  numberWaiter.Start();

			  assertFalse( done.tryAcquire( 10, TimeUnit.MILLISECONDS ) );
			  sequence.Offer( 4, _emptyMeta );
			  assertFalse( done.tryAcquire( 10, TimeUnit.MILLISECONDS ) );
			  sequence.Offer( 5, _emptyMeta );
			  assertTrue( done.tryAcquire( 10_000, TimeUnit.MILLISECONDS ) );

			  numberWaiter.Join();
		 }

		 private void AssertGet( OutOfOrderSequence sequence, long number, long[] meta )
		 {
			  long[] data = sequence.Get();
			  long[] expected = new long[meta.Length + 1];
			  expected[0] = number;
			  Array.Copy( meta, 0, expected, 1, meta.Length );
			  assertArrayEquals( expected, data );
		 }
	}

}