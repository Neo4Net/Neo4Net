﻿using System;
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
namespace Org.Neo4j.causalclustering.helper
{
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using Log = Org.Neo4j.Logging.Log;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class RobustJobSchedulerWrapperTest
	{
		 private readonly int _defaultTimeoutMs = 5000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.kernel.lifecycle.LifeRule schedulerLife = new org.neo4j.kernel.lifecycle.LifeRule(true);
		 public LifeRule SchedulerLife = new LifeRule( true );
		 private readonly JobScheduler _actualScheduler = createInitialisedScheduler();

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