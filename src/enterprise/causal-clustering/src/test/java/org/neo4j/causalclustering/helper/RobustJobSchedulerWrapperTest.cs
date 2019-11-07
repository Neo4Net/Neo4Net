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
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Log = Neo4Net.Logging.Log;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertEventually;

	public class RobustJobSchedulerWrapperTest
	{
		 private readonly int _defaultTimeoutMs = 5000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.kernel.lifecycle.LifeRule schedulerLife = new Neo4Net.kernel.lifecycle.LifeRule(true);
		 public LifeRule SchedulerLife = new LifeRule( true );
		 private readonly IJobScheduler _actualScheduler = createInitializedScheduler();

		 private readonly Log _log = mock( typeof( Log ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  SchedulerLife.add( _actualScheduler );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneOffJobWithExceptionShouldLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OneOffJobWithExceptionShouldLog()
		 {
			  // given
			  Log log = mock( typeof( Log ) );
			  RobustJobSchedulerWrapper robustWrapper = new RobustJobSchedulerWrapper( _actualScheduler, log );

			  AtomicInteger count = new AtomicInteger();
			  System.InvalidOperationException e = new System.InvalidOperationException();

			  // when
			  JobHandle jobHandle = robustWrapper.Schedule(Group.HZ_TOPOLOGY_HEALTH, 100, () =>
			  {
				count.incrementAndGet();
				throw e;
			  });

			  // then
			  assertEventually( "run count", count.get, Matchers.equalTo( 1 ), _defaultTimeoutMs, MILLISECONDS );
			  jobHandle.WaitTermination();
			  verify( log, timeout( _defaultTimeoutMs ).times( 1 ) ).warn( "Uncaught exception", e );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recurringJobWithExceptionShouldKeepRunning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecurringJobWithExceptionShouldKeepRunning()
		 {
			  // given
			  RobustJobSchedulerWrapper robustWrapper = new RobustJobSchedulerWrapper( _actualScheduler, _log );

			  AtomicInteger count = new AtomicInteger();
			  System.InvalidOperationException e = new System.InvalidOperationException();

			  // when
			  int nRuns = 100;
			  JobHandle jobHandle = robustWrapper.ScheduleRecurring(Group.HZ_TOPOLOGY_REFRESH, 1, () =>
			  {
				if ( count.get() < nRuns )
				{
					 count.incrementAndGet();
					 throw e;
				}
			  });

			  // then
			  assertEventually( "run count", count.get, Matchers.equalTo( nRuns ), _defaultTimeoutMs, MILLISECONDS );
			  jobHandle.Cancel( true );
			  verify( _log, timeout( _defaultTimeoutMs ).times( nRuns ) ).warn( "Uncaught exception", e );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recurringJobWithErrorShouldStop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecurringJobWithErrorShouldStop()
		 {
			  // given
			  RobustJobSchedulerWrapper robustWrapper = new RobustJobSchedulerWrapper( _actualScheduler, _log );

			  AtomicInteger count = new AtomicInteger();
			  Exception e = new Exception();

			  // when
			  JobHandle jobHandle = robustWrapper.ScheduleRecurring(Group.HZ_TOPOLOGY_REFRESH, 1, () =>
			  {
				count.incrementAndGet();
				throw e;
			  });

			  // when
			  Thread.Sleep( 50 ); // should not keep increasing during this time

			  // then
			  assertEventually( "run count", count.get, Matchers.equalTo( 1 ), _defaultTimeoutMs, MILLISECONDS );
			  jobHandle.Cancel( true );
			  verify( _log, timeout( _defaultTimeoutMs ).times( 1 ) ).error( "Uncaught error rethrown", e );
		 }
	}

}