using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus
{
	using Test = org.junit.Test;


	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	public class ContinuousJobTest
	{
		 private const long DEFAULT_TIMEOUT_MS = 15_000;
		 private readonly JobScheduler _scheduler = createInitialisedScheduler();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunJobContinuously() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunJobContinuously()
		 {
			  // given
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 10 );
			  ThreadStart task = latch.countDown;

			  ContinuousJob continuousJob = new ContinuousJob( _scheduler.threadFactory( Group.RAFT_BATCH_HANDLER ), task, NullLogProvider.Instance );

			  // when
			  using ( Lifespan ignored = new Lifespan( _scheduler, continuousJob ) )
			  {
					//then
					assertTrue( latch.await( DEFAULT_TIMEOUT_MS, MILLISECONDS ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateOnStop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateOnStop()
		 {
			  // given: this task is gonna take >20 ms total
			  Semaphore semaphore = new Semaphore( -20 );

			  ThreadStart task = () =>
			  {
				LockSupport.parkNanos( 1_000_000 ); // 1 ms
				semaphore.release();
			  };

			  ContinuousJob continuousJob = new ContinuousJob( _scheduler.threadFactory( Group.RAFT_BATCH_HANDLER ), task, NullLogProvider.Instance );

			  // when
			  long startTime = DateTimeHelper.CurrentUnixTimeMillis();
			  using ( Lifespan ignored = new Lifespan( _scheduler, continuousJob ) )
			  {
					semaphore.acquireUninterruptibly();
			  }
			  long runningTime = DateTimeHelper.CurrentUnixTimeMillis() - startTime;

			  // then
			  assertThat( runningTime, lessThan( DEFAULT_TIMEOUT_MS ) );

			  //noinspection StatementWithEmptyBody
			  while ( semaphore.tryAcquire() )
			  {
					// consume all outstanding permits
			  }

			  // no more permits should be granted
			  semaphore.tryAcquire( 10, MILLISECONDS );
		 }
	}

}