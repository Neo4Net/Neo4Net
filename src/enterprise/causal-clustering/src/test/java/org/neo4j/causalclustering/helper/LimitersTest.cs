using System;
using System.Threading;

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
namespace Neo4Net.causalclustering.helper
{
	using Test = org.junit.Test;


	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.helper.Limiters.rateLimiter;

	public class LimitersTest
	{

		 private readonly Duration _eternity = Duration.ofDays( 1000 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRateLimitCalls()
		 public virtual void ShouldRateLimitCalls()
		 {
			  // given
			  int intervalMillis = 10;
			  FakeClock clock = Clocks.fakeClock();

			  System.Action<ThreadStart> cap = rateLimiter( Duration.ofMillis( intervalMillis ), clock );
			  AtomicInteger cnt = new AtomicInteger();
			  ThreadStart op = cnt.incrementAndGet;

			  // when
			  cap( op );
			  cap( op );
			  cap( op );

			  // then
			  assertThat( cnt.get(), equalTo(1) );

			  // when
			  clock.Forward( intervalMillis, MILLISECONDS );
			  cap( op );
			  cap( op );
			  cap( op );

			  // then
			  assertThat( cnt.get(), equalTo(2) );

			  // when
			  clock.Forward( 1000 * intervalMillis, MILLISECONDS );
			  cap( op );
			  cap( op );
			  cap( op );

			  // then
			  assertThat( cnt.get(), equalTo(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyAllowOneThreadPerInterval() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyAllowOneThreadPerInterval()
		 {
			  // given
			  int intervalMillis = 10;
			  int nThreads = 10;
			  int iterations = 100;

			  FakeClock clock = Clocks.fakeClock();
			  System.Action<ThreadStart> cap = rateLimiter( Duration.ofMillis( intervalMillis ), clock );
			  AtomicInteger cnt = new AtomicInteger();
			  ThreadStart op = cnt.incrementAndGet;

			  for ( int iteration = 1; iteration <= iterations; iteration++ )
			  {
					// given
					clock.Forward( intervalMillis, MILLISECONDS );
					System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );

					ExecutorService es = Executors.newCachedThreadPool();
					for ( int j = 0; j < nThreads; j++ )
					{
						 es.submit(() =>
						 {
						 try
						 {
							 latch.await();
						 }
						 catch ( InterruptedException e )
						 {
							 Console.WriteLine( e.ToString() );
							 Console.Write( e.StackTrace );
						 }
						 cap( op );
						 });
					}

					// when
					latch.Signal();
					es.shutdown();
					es.awaitTermination( 10, SECONDS );

					// then
					assertThat( cnt.get(), equalTo(iteration) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void distinctRateLimitersOperateIndependently() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DistinctRateLimitersOperateIndependently()
		 {
			  // given
			  Limiters limiters = new Limiters( Clocks.fakeClock() );
			  AtomicInteger cnt = new AtomicInteger();

			  System.Action<ThreadStart> rateLimiterA = Neo4Net.causalclustering.helper.Limiters.rateLimiter( "A", _eternity );
			  System.Action<ThreadStart> rateLimiterB = Neo4Net.causalclustering.helper.Limiters.rateLimiter( "B", _eternity );

			  // when
			  rateLimiterA( cnt.incrementAndGet );
			  rateLimiterA( cnt.incrementAndGet );
			  rateLimiterA( cnt.incrementAndGet );

			  rateLimiterB( cnt.incrementAndGet );
			  rateLimiterB( cnt.incrementAndGet );
			  rateLimiterB( cnt.incrementAndGet );

			  // then
			  assertEquals( 2, cnt.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSameRateLimiterForSameHandle() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnSameRateLimiterForSameHandle()
		 {
			  // given
			  Limiters limiters = new Limiters( Clocks.fakeClock() );
			  AtomicInteger cnt = new AtomicInteger();

			  System.Action<ThreadStart> rateLimiterA = Neo4Net.causalclustering.helper.Limiters.rateLimiter( "SAME", _eternity );
			  System.Action<ThreadStart> rateLimiterB = Neo4Net.causalclustering.helper.Limiters.rateLimiter( "SAME", _eternity );

			  // when
			  rateLimiterA( cnt.incrementAndGet );
			  rateLimiterA( cnt.incrementAndGet );

			  rateLimiterB( cnt.incrementAndGet );
			  rateLimiterB( cnt.incrementAndGet );

			  // then
			  assertEquals( 1, cnt.get() );
		 }
	}

}