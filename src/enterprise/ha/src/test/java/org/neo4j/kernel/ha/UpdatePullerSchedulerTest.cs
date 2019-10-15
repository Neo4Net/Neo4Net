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
namespace Neo4Net.Kernel.ha
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

	public class UpdatePullerSchedulerTest
	{
		 private UpdatePuller _updatePuller;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _updatePuller = mock( typeof( UpdatePuller ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipUpdatePullingSchedulingWithZeroInterval()
		 public virtual void SkipUpdatePullingSchedulingWithZeroInterval()
		 {
			  IJobScheduler jobScheduler = mock( typeof( IJobScheduler ) );
			  UpdatePullerScheduler pullerScheduler = new UpdatePullerScheduler( jobScheduler, NullLogProvider.Instance, _updatePuller, 0 );

			  // when start puller scheduler - nothing should be scheduled
			  pullerScheduler.Init();

			  verifyZeroInteractions( jobScheduler, _updatePuller );

			  // should be able shutdown scheduler
			  pullerScheduler.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void scheduleUpdatePulling() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScheduleUpdatePulling()
		 {
			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler( false );
			  UpdatePullerScheduler pullerScheduler = new UpdatePullerScheduler( jobScheduler, NullLogProvider.Instance, _updatePuller, 10 );

			  // schedule update pulling and run it
			  pullerScheduler.Init();
			  jobScheduler.RunJob();

			  verify( _updatePuller ).pullUpdates();
			  assertNotNull( "Job should be scheduled", jobScheduler.Job );

			  // stop scheduler - job should be canceled
			  pullerScheduler.Shutdown();

			  assertNull( "Job should be canceled", jobScheduler.Job );
		 }

	}

}