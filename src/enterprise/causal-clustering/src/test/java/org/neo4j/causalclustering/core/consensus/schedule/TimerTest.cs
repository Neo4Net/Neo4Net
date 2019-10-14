/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.consensus.schedule
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLog.getInstance;

	/// <summary>
	/// Most aspects of the Timer are tested through the <seealso cref="TimerServiceTest"/>.
	/// </summary>
	public class TimerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.kernel.lifecycle.LifeRule lifeRule = new org.neo4j.kernel.lifecycle.LifeRule(true);
		 public LifeRule LifeRule = new LifeRule( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleConcurrentResetAndInvocationOfHandler()
		 public virtual void ShouldHandleConcurrentResetAndInvocationOfHandler()
		 {
			  // given
			  JobScheduler scheduler = LifeRule.add( createScheduler() );

			  BinaryLatch invoked = new BinaryLatch();
			  BinaryLatch done = new BinaryLatch();

			  TimeoutHandler handler = timer =>
			  {
				invoked.Release();
				done.Await();
			  };

			  Timer timer = new Timer( () => "test", scheduler, Instance, Group.RAFT_TIMER, handler );
			  timer.Set( new FixedTimeout( 0, SECONDS ) );
			  invoked.Await();

			  // when
			  timer.Reset();

			  // then: should not deadlock

			  // cleanup
			  done.Release();
		 }
	}

}