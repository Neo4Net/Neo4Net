using System;

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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using OngoingStubbing = org.mockito.stubbing.OngoingStubbing;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using ComException = Neo4Net.com.ComException;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using InvalidEpochException = Neo4Net.Kernel.ha.com.master.InvalidEpochException;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using InvalidEpochExceptionHandler = Neo4Net.Kernel.ha.com.slave.InvalidEpochExceptionHandler;
	using CountingJobScheduler = Neo4Net.Kernel.impl.util.CountingJobScheduler;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class SlaveUpdatePullerTest
	{
		private bool InstanceFieldsInitialized = false;

		public SlaveUpdatePullerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_jobScheduler = new CountingJobScheduler( _scheduledJobs, createInitializedScheduler() );
			_updatePuller = new SlaveUpdatePuller( _requestContextFactory, _master, _lastUpdateTime, _logProvider, _instanceId, _databaseAvailabilityGuard, _invalidEpochHandler, _jobScheduler, _monitor );
		}

		 private readonly AtomicInteger _scheduledJobs = new AtomicInteger();
		 private readonly InstanceId _instanceId = new InstanceId( 1 );
		 private readonly Config _config = mock( typeof( Config ) );
		 private readonly DatabaseAvailabilityGuard _databaseAvailabilityGuard = mock( typeof( DatabaseAvailabilityGuard ) );
		 private readonly LastUpdateTime _lastUpdateTime = mock( typeof( LastUpdateTime ) );
		 private readonly Master _master = mock( typeof( Master ), RETURNS_MOCKS );
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private readonly RequestContextFactory _requestContextFactory = mock( typeof( RequestContextFactory ) );
		 private readonly InvalidEpochExceptionHandler _invalidEpochHandler = mock( typeof( InvalidEpochExceptionHandler ) );
		 private readonly SlaveUpdatePuller.Monitor _monitor = mock( typeof( SlaveUpdatePuller.Monitor ) );
		 private JobScheduler _jobScheduler;
		 private SlaveUpdatePuller _updatePuller;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  when( _requestContextFactory.newRequestContext() ).thenReturn(new RequestContext(42, 42, 42, 42, 42));
			  when( _config.get( HaSettings.PullInterval ) ).thenReturn( Duration.ofSeconds( 1 ) );
			  when( _config.get( ClusterSettings.server_id ) ).thenReturn( _instanceId );
			  when( _databaseAvailabilityGuard.isAvailable( anyLong() ) ).thenReturn(true);
			  _jobScheduler.init();
			  _jobScheduler.start();
			  _updatePuller.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _updatePuller.stop();
			  _jobScheduler.stop();
			  _jobScheduler.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void initializationMustBeIdempotent()
		 public virtual void InitialisationMustBeIdempotent()
		 {
			  _updatePuller.start();
			  _updatePuller.start();
			  _updatePuller.start();
			  assertThat( _scheduledJobs.get(), @is(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopPullingAfterStop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopPullingAfterStop()
		 {
			  // WHEN
			  _updatePuller.pullUpdates();

			  // THEN
			  verify( _lastUpdateTime, times( 1 ) ).LastUpdateTime = anyLong();
			  verify( _databaseAvailabilityGuard, times( 1 ) ).isAvailable( anyLong() );
			  verify( _master, times( 1 ) ).pullUpdates( ArgumentMatchers.any() );
			  verify( _monitor, times( 1 ) ).pulledUpdates( anyLong() );

			  // WHEN
			  _updatePuller.stop();
			  _updatePuller.pullUpdates();

			  // THEN
			  verifyNoMoreInteractions( _lastUpdateTime, _databaseAvailabilityGuard );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void keepPullingUpdatesOnConsecutiveCalls() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void KeepPullingUpdatesOnConsecutiveCalls()
		 {
			  // WHEN
			  _updatePuller.pullUpdates();

			  // THEN
			  verify( _lastUpdateTime, times( 1 ) ).LastUpdateTime = anyLong();
			  verify( _databaseAvailabilityGuard, times( 1 ) ).isAvailable( anyLong() );
			  verify( _master, times( 1 ) ).pullUpdates( ArgumentMatchers.any() );
			  verify( _monitor, times( 1 ) ).pulledUpdates( anyLong() );

			  // WHEN
			  _updatePuller.pullUpdates();

			  // THEN
			  verify( _lastUpdateTime, times( 2 ) ).LastUpdateTime = anyLong();
			  verify( _databaseAvailabilityGuard, times( 2 ) ).isAvailable( anyLong() );
			  verify( _master, times( 2 ) ).pullUpdates( ArgumentMatchers.any() );
			  verify( _monitor, times( 2 ) ).pulledUpdates( anyLong() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void falseOnTryPullUpdatesOnInactivePuller() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FalseOnTryPullUpdatesOnInactivePuller()
		 {
			  // GIVEN
			  _updatePuller.stop();

			  // WHEN
			  bool result = _updatePuller.tryPullUpdates();

			  // THEN
			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfPullerInitiallyInactiveStrict() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfPullerInitiallyInactiveStrict()
		 {
			  // GIVEN
			  UpdatePuller_Condition condition = mock( typeof( UpdatePuller_Condition ) );
			  _updatePuller.stop();

			  // WHEN
			  try
			  {
					_updatePuller.pullUpdates( condition, true );
					fail( "Should have thrown" );
			  }
			  catch ( System.InvalidOperationException )
			  { // THEN Good
					verifyNoMoreInteractions( condition );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfPullerBecomesInactiveWhileWaitingStrict() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfPullerBecomesInactiveWhileWaitingStrict()
		 {
			  // GIVEN
			  UpdatePuller_Condition condition = mock( typeof( UpdatePuller_Condition ) );

			  when( condition.Evaluate( anyInt(), anyInt() ) ).thenAnswer(invocation =>
			  {
				_updatePuller.stop();
				return false;
			  });

			  // WHEN
			  try
			  {
					_updatePuller.pullUpdates( condition, true );
					fail( "Should have thrown" );
			  }
			  catch ( System.InvalidOperationException )
			  { // THEN Good
					verify( condition ).evaluate( anyInt(), anyInt() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleInvalidEpochByNotifyingItsHandler() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleInvalidEpochByNotifyingItsHandler()
		 {
			  // GIVEN
			  doThrow( typeof( InvalidEpochException ) ).when( _master ).pullUpdates( any( typeof( RequestContext ) ) );

			  // WHEN
			  _updatePuller.pullUpdates();

			  // THEN
			  verify( _invalidEpochHandler ).handle();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldCopeWithHardExceptionsLikeOutOfMemory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCopeWithHardExceptionsLikeOutOfMemory()
		 {
			  // GIVEN
			  System.OutOfMemoryException oom = new System.OutOfMemoryException();
			  when( _master.pullUpdates( any( typeof( RequestContext ) ) ) ).thenThrow( oom ).thenReturn( Response.empty() );

			  // WHEN making the first pull
			  _updatePuller.pullUpdates();

			  // THEN the OOM should be caught and logged
			  _logProvider.assertAtLeastOnce( inLog( typeof( SlaveUpdatePuller ) ).error( org.hamcrest.Matchers.any( typeof( string ) ), sameInstance( oom ) ) );

			  // WHEN that has passed THEN we should still be making pull attempts.
			  _updatePuller.pullUpdates();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCapExcessiveComExceptionLogging() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCapExcessiveComExceptionLogging()
		 {
			  OngoingStubbing<Response<Void>> updatePullStubbing = when( _master.pullUpdates( any( typeof( RequestContext ) ) ) );
			  updatePullStubbing.thenThrow( new ComException() );

			  for ( int i = 0; i < SlaveUpdatePuller.LogCap + 20; i++ )
			  {
					_updatePuller.pullUpdates();
			  }

			  _logProvider.assertContainsThrowablesMatching( 0, Repeat( new ComException(), SlaveUpdatePuller.LogCap ) );

			  // And we should be able to recover afterwards
			  updatePullStubbing.thenReturn( Response.empty() ).thenThrow(new ComException());

			  _updatePuller.pullUpdates(); // This one will succeed and unlock the circuit breaker
			  _updatePuller.pullUpdates(); // And then we log another exception

			  _logProvider.assertContainsThrowablesMatching( 0, Repeat( new ComException(), SlaveUpdatePuller.LogCap + 1 ) );
		 }

		 private Exception[] Repeat( Exception throwable, int count )
		 {
			  Exception[] throwables = new Exception[count];
			  for ( int i = 0; i < count; i++ )
			  {
					throwables[i] = throwable;
			  }
			  return throwables;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCapExcessiveInvalidEpochExceptionLogging() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCapExcessiveInvalidEpochExceptionLogging()
		 {
			  OngoingStubbing<Response<Void>> updatePullStubbing = when( _master.pullUpdates( any( typeof( RequestContext ) ) ) );
			  updatePullStubbing.thenThrow( new InvalidEpochException( 2, 1 ) );

			  for ( int i = 0; i < SlaveUpdatePuller.LogCap + 20; i++ )
			  {
					_updatePuller.pullUpdates();
			  }

			  _logProvider.assertContainsThrowablesMatching( 0, Repeat( new InvalidEpochException( 2, 1 ), SlaveUpdatePuller.LogCap ) );

			  // And we should be able to recover afterwards
			  updatePullStubbing.thenReturn( Response.empty() ).thenThrow(new InvalidEpochException(2, 1));

			  _updatePuller.pullUpdates(); // This one will succeed and unlock the circuit breaker
			  _updatePuller.pullUpdates(); // And then we log another exception

			  _logProvider.assertContainsThrowablesMatching( 0, Repeat( new InvalidEpochException( 2, 1 ), SlaveUpdatePuller.LogCap + 1 ) );
		 }

	}

}