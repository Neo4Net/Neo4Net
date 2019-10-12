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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using Test = org.junit.Test;


	using Monitor = Org.Neo4j.Kernel.Impl.Api.DefaultTransactionTracer.Monitor;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using LogAppendEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogRotateEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogRotateEvent;
	using TransactionEvent = Org.Neo4j.Kernel.impl.transaction.tracing.TransactionEvent;
	using OnDemandJobScheduler = Org.Neo4j.Test.OnDemandJobScheduler;
	using Clocks = Org.Neo4j.Time.Clocks;
	using FakeClock = Org.Neo4j.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

	public class DefaultTransactionTracerTest
	{
		 private readonly FakeClock _clock = Clocks.fakeClock();
		 private readonly OnDemandJobScheduler _jobScheduler = new OnDemandJobScheduler();
		 private readonly Monitor _monitor = mock( typeof( Monitor ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeStartEndAndTotalTimeForLogRotation()
		 public virtual void ShouldComputeStartEndAndTotalTimeForLogRotation()
		 {
			  DefaultTransactionTracer tracer = new DefaultTransactionTracer( _clock, _monitor, _jobScheduler );

			  TriggerEvent( tracer, 20 );

			  assertEquals( 1, tracer.NumberOfLogRotationEvents() );
			  assertEquals( 20, tracer.LogRotationAccumulatedTotalTimeMillis() );
			  verify( _monitor, times( 1 ) ).lastLogRotationEventDuration( 20L );

			  TriggerEvent( tracer, 30 );

			  // should reset the total time value whenever read
			  assertEquals( 2, tracer.NumberOfLogRotationEvents() );
			  assertEquals( 50, tracer.LogRotationAccumulatedTotalTimeMillis() );
			  verify( _monitor, times( 1 ) ).lastLogRotationEventDuration( 30L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnMinusOneIfNoDataIsAvailableForLogRotation()
		 public virtual void ShouldReturnMinusOneIfNoDataIsAvailableForLogRotation()
		 {
			  DefaultTransactionTracer tracer = new DefaultTransactionTracer( _clock, _monitor, _jobScheduler );

			  _jobScheduler.runJob();

			  assertEquals( 0, tracer.NumberOfLogRotationEvents() );
			  assertEquals( 0, tracer.LogRotationAccumulatedTotalTimeMillis() );
			  verifyZeroInteractions( _monitor );
		 }

		 private void TriggerEvent( DefaultTransactionTracer tracer, int eventDuration )
		 {
			  using ( TransactionEvent txEvent = tracer.BeginTransaction() )
			  {
					using ( CommitEvent commitEvent = txEvent.BeginCommitEvent() )
					{
						 using ( LogAppendEvent logAppendEvent = commitEvent.BeginLogAppend() )
						 {
							  _clock.forward( ThreadLocalRandom.current().nextLong(200), TimeUnit.MILLISECONDS );
							  using ( LogRotateEvent @event = logAppendEvent.BeginLogRotate() )
							  {
									_clock.forward( eventDuration, TimeUnit.MILLISECONDS );
							  }
						 }
					}
			  }

			  _jobScheduler.runJob();
		 }
	}

}