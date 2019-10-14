using System;
using System.Threading;

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
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Answer = org.mockito.stubbing.Answer;


	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterClient = Neo4Net.cluster.client.ClusterClient;
	using ClusterMemberAvailability = Neo4Net.cluster.member.ClusterMemberAvailability;
	using Election = Neo4Net.cluster.protocol.election.Election;
	using ComException = Neo4Net.com.ComException;
	using CancellationRequest = Neo4Net.Helpers.CancellationRequest;
	using Config = Neo4Net.Kernel.configuration.Config;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using SimpleLogService = Neo4Net.Logging.@internal.SimpleLogService;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.PENDING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.TO_SLAVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class HighAvailabilityModeSwitcherTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBroadcastMasterIsAvailableIfMasterAndReceiveMasterIsElected()
		 public virtual void ShouldBroadcastMasterIsAvailableIfMasterAndReceiveMasterIsElected()
		 {
			  // Given
			  ClusterMemberAvailability availability = mock( typeof( ClusterMemberAvailability ) );
			  HighAvailabilityModeSwitcher toTest = CreateModeSwitcher( availability );

			  // When
			  toTest.MasterIsElected( new HighAvailabilityMemberChangeEvent( HighAvailabilityMemberState.MASTER, HighAvailabilityMemberState.MASTER, new InstanceId( 2 ), URI.create( "ha://someone" ) ) );

			  // Then
				 /*
				  * The second argument to memberIsAvailable below is null because it has not been set yet. This would require
				  * a switch to master which we don't do here.
				  */
			  verify( availability ).memberIsAvailable( HighAvailabilityModeSwitcher.MASTER, null, StoreId.DEFAULT );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBroadcastSlaveIsAvailableIfSlaveAndReceivesMasterIsAvailable()
		 public virtual void ShouldBroadcastSlaveIsAvailableIfSlaveAndReceivesMasterIsAvailable()
		 {

			  // Given
			  ClusterMemberAvailability availability = mock( typeof( ClusterMemberAvailability ) );
			  HighAvailabilityModeSwitcher toTest = CreateModeSwitcher( availability );

			  // When
			  toTest.MasterIsAvailable( new HighAvailabilityMemberChangeEvent( HighAvailabilityMemberState.SLAVE, HighAvailabilityMemberState.SLAVE, new InstanceId( 2 ), URI.create( "ha://someone" ) ) );

			  // Then
				 /*
				  * The second argument to memberIsAvailable below is null because it has not been set yet. This would require
				  * a switch to master which we don't do here.
				  */
			  verify( availability ).memberIsAvailable( HighAvailabilityModeSwitcher.SLAVE, null, StoreId.DEFAULT );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBroadcastIfSlaveAndReceivesMasterIsElected()
		 public virtual void ShouldNotBroadcastIfSlaveAndReceivesMasterIsElected()
		 {

			  // Given
			  ClusterMemberAvailability availability = mock( typeof( ClusterMemberAvailability ) );
			  HighAvailabilityModeSwitcher toTest = CreateModeSwitcher( availability );

			  // When
			  toTest.MasterIsElected( new HighAvailabilityMemberChangeEvent( HighAvailabilityMemberState.SLAVE, HighAvailabilityMemberState.SLAVE, new InstanceId( 2 ), URI.create( "ha://someone" ) ) );

			  // Then
				 /*
				  * The second argument to memberIsAvailable below is null because it has not been set yet. This would require
				  * a switch to master which we don't do here.
				  */
			  verifyZeroInteractions( availability );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBroadcastIfMasterAndReceivesSlaveIsAvailable()
		 public virtual void ShouldNotBroadcastIfMasterAndReceivesSlaveIsAvailable()
		 {

			  // Given
			  ClusterMemberAvailability availability = mock( typeof( ClusterMemberAvailability ) );
			  HighAvailabilityModeSwitcher toTest = CreateModeSwitcher( availability );

			  // When
			  toTest.SlaveIsAvailable( new HighAvailabilityMemberChangeEvent( HighAvailabilityMemberState.MASTER, HighAvailabilityMemberState.MASTER, new InstanceId( 2 ), URI.create( "ha://someone" ) ) );

			  // Then
				 /*
				  * The second argument to memberIsAvailable below is null because it has not been set yet. This would require
				  * a switch to master which we don't do here.
				  */
			  verifyZeroInteractions( availability );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReswitchToSlaveIfNewMasterBecameElectedAndAvailableDuringSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReswitchToSlaveIfNewMasterBecameElectedAndAvailableDuringSwitch()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch switching = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent switching = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch slaveAvailable = new java.util.concurrent.CountDownLatch(2);
			  System.Threading.CountdownEvent slaveAvailable = new System.Threading.CountdownEvent( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean firstSwitch = new java.util.concurrent.atomic.AtomicBoolean(true);
			  AtomicBoolean firstSwitch = new AtomicBoolean( true );
			  ClusterMemberAvailability availability = mock( typeof( ClusterMemberAvailability ) );
			  SwitchToSlaveCopyThenBranch switchToSlave = mock( typeof( SwitchToSlaveCopyThenBranch ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("resource") org.neo4j.kernel.ha.cluster.SwitchToMaster switchToMaster = mock(org.neo4j.kernel.ha.cluster.SwitchToMaster.class);
			  SwitchToMaster switchToMaster = mock( typeof( SwitchToMaster ) );

			  when( switchToSlave.SwitchToSlaveConflict( any( typeof( LifeSupport ) ), any( typeof( URI ) ), any( typeof( URI ) ), any( typeof( CancellationRequest ) ) ) ).thenAnswer(invocationOnMock =>
			  {
						  switching.Signal();
						  CancellationRequest cancel = invocationOnMock.getArgument( 3 );
						  if ( firstSwitch.get() )
						  {
								while ( !cancel.cancellationRequested() )
								{
									 Thread.Sleep( 1 );
								}
								firstSwitch.set( false );
						  }
						  slaveAvailable.Signal();
						  return URI.create( "ha://slave" );
			  });

			  HighAvailabilityModeSwitcher toTest = new HighAvailabilityModeSwitcher( switchToSlave, switchToMaster, mock( typeof( Election ) ), availability, mock( typeof( ClusterClient ) ), StoreSupplierMock(), mock(typeof(InstanceId)), new ComponentSwitcherContainer(), NeoStoreDataSourceSupplierMock(), NullLogService.Instance );
			  toTest.Init();
			  toTest.Start();
			  toTest.ListeningAt( URI.create( "ha://server3?serverId=3" ) );

			  // When
			  // This will start a switch to slave
			  toTest.MasterIsAvailable( new HighAvailabilityMemberChangeEvent( PENDING, TO_SLAVE, mock( typeof( InstanceId ) ), URI.create( "ha://server1" ) ) );
			  // Wait until it starts and blocks on the cancellation request
			  switching.await();
			  // change the elected master, moving to pending, cancelling the previous change. This will block until the
			  // previous switch is aborted
			  toTest.MasterIsElected( new HighAvailabilityMemberChangeEvent( TO_SLAVE, PENDING, new InstanceId( 2 ), URI.create( "ha://server2" ) ) );
			  // Now move to the new master by switching to TO_SLAVE
			  toTest.MasterIsAvailable( new HighAvailabilityMemberChangeEvent( PENDING, TO_SLAVE, new InstanceId( 2 ), URI.create( "ha://server2" ) ) );

			  // Then
			  // The second switch must happen and this test won't block
			  slaveAvailable.await();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecognizeNewMasterIfNewMasterBecameAvailableDuringSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecognizeNewMasterIfNewMasterBecameAvailableDuringSwitch()
		 {
			  // When messages coming in the following ordering, the slave should detect that the master id has changed
			  // M1: Get masterIsAvailable for instance 1 at PENDING state, changing PENDING -> TO_SLAVE
			  // M2: Get masterIsAvailable for instance 2 at TO_SLAVE state, changing TO_SLAVE -> TO_SLAVE

			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch firstMasterAvailableHandled = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent firstMasterAvailableHandled = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch secondMasterAvailableComes = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent secondMasterAvailableComes = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch secondMasterAvailableHandled = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent secondMasterAvailableHandled = new System.Threading.CountdownEvent( 1 );

			  SwitchToSlaveCopyThenBranch switchToSlave = mock( typeof( SwitchToSlaveCopyThenBranch ) );

			  HighAvailabilityModeSwitcher modeSwitcher = new HighAvailabilityModeSwitcherAnonymousInnerClass( this, switchToSlave, mock( typeof( SwitchToMaster ) ), mock( typeof( Election ) ), mock( typeof( ClusterMemberAvailability ) ), mock( typeof( ClusterClient ) ), mock( typeof( System.Func ) ), NeoStoreDataSourceSupplierMock(), NullLogService.Instance, firstMasterAvailableHandled, secondMasterAvailableComes, secondMasterAvailableHandled );
			  modeSwitcher.Init();
			  modeSwitcher.Start();
			  modeSwitcher.ListeningAt( URI.create( "ha://server3?serverId=3" ) );

			  // When

			  // masterIsAvailable for instance 1
			  URI uri1 = URI.create( "ha://server1" );
			  // The first masterIsAvailable should fail so that the slave instance stops at TO_SLAVE state
			  doThrow( new ComException( "Fail to switch to slave and reschedule to retry" ) ).when( switchToSlave ).switchToSlave( any( typeof( LifeSupport ) ), any( typeof( URI ) ), eq( uri1 ), any( typeof( CancellationRequest ) ) );

			  modeSwitcher.MasterIsAvailable( new HighAvailabilityMemberChangeEvent( PENDING, TO_SLAVE, new InstanceId( 1 ), uri1 ) );
			  firstMasterAvailableHandled.await(); // wait until the first masterIsAvailable triggers the exception handling
			  verify( switchToSlave ).switchToSlave( any( typeof( LifeSupport ) ), any( typeof( URI ) ), eq( uri1 ), any( typeof( CancellationRequest ) ) );

			  // masterIsAvailable for instance 2
			  URI uri2 = URI.create( "ha://server2" );
			  modeSwitcher.MasterIsAvailable( new HighAvailabilityMemberChangeEvent( TO_SLAVE, TO_SLAVE, new InstanceId( 2 ), uri2 ) );
			  secondMasterAvailableComes.Signal();
			  secondMasterAvailableHandled.await(); // wait until switchToSlave method is invoked again

			  // Then
			  // switchToSlave should be retried with new master id
			  verify( switchToSlave ).switchToSlave( any( typeof( LifeSupport ) ), any( typeof( URI ) ), eq( uri2 ), any( typeof( CancellationRequest ) ) );
		 }

		 private class HighAvailabilityModeSwitcherAnonymousInnerClass : HighAvailabilityModeSwitcher
		 {
			 private readonly HighAvailabilityModeSwitcherTest _outerInstance;

			 private System.Threading.CountdownEvent _firstMasterAvailableHandled;
			 private System.Threading.CountdownEvent _secondMasterAvailableComes;
			 private System.Threading.CountdownEvent _secondMasterAvailableHandled;

			 public HighAvailabilityModeSwitcherAnonymousInnerClass( HighAvailabilityModeSwitcherTest outerInstance, SwitchToSlaveCopyThenBranch switchToSlave, UnknownType mock, UnknownType mock, UnknownType mock, UnknownType mock, UnknownType mock, DataSourceManager neoStoreDataSourceSupplierMock, NullLogService getInstance, System.Threading.CountdownEvent firstMasterAvailableHandled, System.Threading.CountdownEvent secondMasterAvailableComes, System.Threading.CountdownEvent secondMasterAvailableHandled ) : base( switchToSlave, mock, mock, mock, mock, mock, new InstanceId( 4 ), new ComponentSwitcherContainer(), neoStoreDataSourceSupplierMock, getInstance )
			 {
				 this.outerInstance = outerInstance;
				 this._firstMasterAvailableHandled = firstMasterAvailableHandled;
				 this._secondMasterAvailableComes = secondMasterAvailableComes;
				 this._secondMasterAvailableHandled = secondMasterAvailableHandled;
			 }

			 internal override ScheduledExecutorService createExecutor()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.ScheduledExecutorService executor = mock(java.util.concurrent.ScheduledExecutorService.class);
				  ScheduledExecutorService executor = mock( typeof( ScheduledExecutorService ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.ExecutorService realExecutor = java.util.concurrent.Executors.newSingleThreadExecutor();
				  ExecutorService realExecutor = Executors.newSingleThreadExecutor();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: when(executor.submit(any(Runnable.class))).thenAnswer((org.mockito.stubbing.Answer<java.util.concurrent.Future<?>>) invocation -> realExecutor.submit(() -> ((Runnable) invocation.getArgument(0)).run()));
				  when( executor.submit( any( typeof( ThreadStart ) ) ) ).thenAnswer( ( Answer<Future<object>> ) invocation => realExecutor.submit( () => ((ThreadStart) invocation.getArgument(0)).run() ) );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: when(executor.schedule(any(Runnable.class), anyLong(), any(java.util.concurrent.TimeUnit.class))).thenAnswer((org.mockito.stubbing.Answer<java.util.concurrent.Future<?>>) invocation ->
				  when( executor.schedule( any( typeof( ThreadStart ) ), anyLong(), any(typeof(TimeUnit)) ) ).thenAnswer((Answer<Future<object>>) invocation =>
				  {
							  realExecutor.submit((Callable<Void>)() =>
							  {
									_firstMasterAvailableHandled.Signal();

									// wait until the second masterIsAvailable comes and then call switchToSlave
									// method
									_secondMasterAvailableComes.await();
									( ( ThreadStart ) invocation.getArgument( 0 ) ).run();
									_secondMasterAvailableHandled.Signal();
									return null;
							  });
							  return mock( typeof( ScheduledFuture ) );
				  });
				  return executor;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotResetAvailableMasterURIIfElectionResultReceived() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotResetAvailableMasterURIIfElectionResultReceived()
		 {
			  /*
			   * It is possible that a masterIsElected nulls out the current available master URI in the HAMS. That can
			   * be a problem if handing the mIE event is concurrent with an ongoing switch which re-runs because
			   * the store was incompatible or a log was missing. In such a case it will find a null master URI on
			   * rerun and it will fail.
			   */

			  // Given
			  SwitchToSlaveCopyThenBranch switchToSlave = mock( typeof( SwitchToSlaveCopyThenBranch ) );
			  // The fist run through switchToSlave
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch firstCallMade = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent firstCallMade = new System.Threading.CountdownEvent( 1 );
			  // The second run through switchToSlave
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch secondCallMade = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent secondCallMade = new System.Threading.CountdownEvent( 1 );
			  // The latch for waiting for the masterIsElected to come through
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch waitForSecondMessage = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent waitForSecondMessage = new System.Threading.CountdownEvent( 1 );

			  HighAvailabilityModeSwitcher toTest = new HighAvailabilityModeSwitcher( switchToSlave, mock( typeof( SwitchToMaster ) ), mock( typeof( Election ) ), mock( typeof( ClusterMemberAvailability ) ), mock( typeof( ClusterClient ) ), StoreSupplierMock(), new InstanceId(1), new ComponentSwitcherContainer(), NeoStoreDataSourceSupplierMock(), NullLogService.Instance );
			  URI uri1 = URI.create( "ha://server1" );
			  toTest.Init();
			  toTest.Start();
			  toTest.ListeningAt( URI.create( "ha://server3?serverId=3" ) );

			  when( switchToSlave.SwitchToSlaveConflict( any( typeof( LifeSupport ) ), any( typeof( URI ) ), any( typeof( URI ) ), any( typeof( CancellationRequest ) ) ) ).thenAnswer(invocation =>
			  {
						  firstCallMade.Signal();
						  waitForSecondMessage.await();
						  throw new MismatchingStoreIdException( StoreId.DEFAULT, StoreId.DEFAULT );
			  }).thenAnswer(invocation =>
			  {
								secondCallMade.Signal();
								return URI.create( "ha://server3" );
						  });

			  // When

			  // The first message goes through, start the first run
			  toTest.MasterIsAvailable( new HighAvailabilityMemberChangeEvent( PENDING, TO_SLAVE, new InstanceId( 1 ), uri1 ) );
			  // Wait for it to be processed but get just before the exception
			  firstCallMade.await();
			  // It is just about to throw the exception, i.e. rerun. Send in the event
			  toTest.MasterIsElected( new HighAvailabilityMemberChangeEvent( TO_SLAVE, TO_SLAVE, new InstanceId( 1 ), null ) );
			  // Allow to continue and do the second run
			  waitForSecondMessage.Signal();
			  // Wait for the call to finish
			  secondCallMade.await();

			  // Then
			  verify( switchToSlave, times( 2 ) ).switchToSlave( any( typeof( LifeSupport ) ), any( typeof( URI ) ), eq( uri1 ), any( typeof( CancellationRequest ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTakeNoActionIfSwitchingToSlaveForItselfAsMaster()
		 public virtual void ShouldTakeNoActionIfSwitchingToSlaveForItselfAsMaster()
		 {
			  // Given
			  // A HAMS
			  SwitchToSlaveCopyThenBranch switchToSlave = mock( typeof( SwitchToSlaveCopyThenBranch ) );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  SimpleLogService logService = new SimpleLogService( NullLogProvider.Instance, logProvider );

			  HighAvailabilityModeSwitcher toTest = new HighAvailabilityModeSwitcher( switchToSlave, mock( typeof( SwitchToMaster ) ), mock( typeof( Election ) ), mock( typeof( ClusterMemberAvailability ) ), mock( typeof( ClusterClient ) ), StoreSupplierMock(), new InstanceId(2), new ComponentSwitcherContainer(), NeoStoreDataSourceSupplierMock(), logService );
			  // That is properly started
			  toTest.Init();
			  toTest.Start();
			  /*
			   * This is the URI at which we are registered as server - includes our own id, but we don't necessarily listen
			   * there
			   */
			  URI serverHaUri = URI.create( "ha://server2?serverId=2" );
			  toTest.ListeningAt( serverHaUri );

			  // When
			  // The HAMS tries to switch to slave for a master that is itself
			  toTest.MasterIsAvailable( new HighAvailabilityMemberChangeEvent( PENDING, TO_SLAVE, new InstanceId( 2 ), serverHaUri ) );

			  // Then
			  // No switching to slave must happen
			  verifyZeroInteractions( switchToSlave );
			  // And an error must be logged
			  logProvider.AssertAtLeastOnce( inLog( typeof( HighAvailabilityModeSwitcher ) ).error( "I (ha://server2?serverId=2) tried to switch to " + "slave for myself as master (ha://server2?serverId=2)" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformForcedElections()
		 public virtual void ShouldPerformForcedElections()
		 {
			  // Given
			  ClusterMemberAvailability memberAvailability = mock( typeof( ClusterMemberAvailability ) );
			  Election election = mock( typeof( Election ) );

			  HighAvailabilityModeSwitcher modeSwitcher = new HighAvailabilityModeSwitcher( mock( typeof( SwitchToSlaveCopyThenBranch ) ), mock( typeof( SwitchToMaster ) ), election, memberAvailability, mock( typeof( ClusterClient ) ), StoreSupplierMock(), mock(typeof(InstanceId)), new ComponentSwitcherContainer(), NeoStoreDataSourceSupplierMock(), NullLogService.Instance );

			  // When
			  modeSwitcher.ForceElections();

			  // Then
			  InOrder inOrder = inOrder( memberAvailability, election );
			  inOrder.verify( memberAvailability ).memberIsUnavailable( HighAvailabilityModeSwitcher.SLAVE );
			  inOrder.verify( election ).performRoleElections();
			  inOrder.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformForcedElectionsOnlyOnce()
		 public virtual void ShouldPerformForcedElectionsOnlyOnce()
		 {
			  // Given: HAMS
			  ClusterMemberAvailability memberAvailability = mock( typeof( ClusterMemberAvailability ) );
			  Election election = mock( typeof( Election ) );

			  HighAvailabilityModeSwitcher modeSwitcher = new HighAvailabilityModeSwitcher( mock( typeof( SwitchToSlaveCopyThenBranch ) ), mock( typeof( SwitchToMaster ) ), election, memberAvailability, mock( typeof( ClusterClient ) ), StoreSupplierMock(), mock(typeof(InstanceId)), new ComponentSwitcherContainer(), NeoStoreDataSourceSupplierMock(), NullLogService.Instance );

			  // When: reelections are forced multiple times
			  modeSwitcher.ForceElections();
			  modeSwitcher.ForceElections();
			  modeSwitcher.ForceElections();

			  // Then: instance sens out memberIsUnavailable and asks for elections and does this only once
			  InOrder inOrder = inOrder( memberAvailability, election );
			  inOrder.verify( memberAvailability ).memberIsUnavailable( HighAvailabilityModeSwitcher.SLAVE );
			  inOrder.verify( election ).performRoleElections();
			  inOrder.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowForcedElectionsAfterModeSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowForcedElectionsAfterModeSwitch()
		 {
			  // Given
			  SwitchToSlaveCopyThenBranch switchToSlave = mock( typeof( SwitchToSlaveCopyThenBranch ) );
			  when( switchToSlave.SwitchToSlaveConflict( any( typeof( LifeSupport ) ), Null, any( typeof( URI ) ), any( typeof( CancellationRequest ) ) ) ).thenReturn( URI.create( "http://localhost" ) );
			  ClusterMemberAvailability memberAvailability = mock( typeof( ClusterMemberAvailability ) );
			  Election election = mock( typeof( Election ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch modeSwitchHappened = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent modeSwitchHappened = new System.Threading.CountdownEvent( 1 );

			  HighAvailabilityModeSwitcher modeSwitcher = new HighAvailabilityModeSwitcherAnonymousInnerClass2( this, switchToSlave, mock( typeof( SwitchToMaster ) ), election, memberAvailability, mock( typeof( ClusterClient ) ), StoreSupplierMock(), mock(typeof(InstanceId)), NeoStoreDataSourceSupplierMock(), NullLogService.Instance, modeSwitchHappened );

			  modeSwitcher.Init();
			  modeSwitcher.Start();

			  modeSwitcher.ForceElections();
			  reset( memberAvailability, election );

			  // When
			  modeSwitcher.MasterIsAvailable( new HighAvailabilityMemberChangeEvent( PENDING, TO_SLAVE, mock( typeof( InstanceId ) ), URI.create( "http://localhost:9090?serverId=42" ) ) );
			  modeSwitchHappened.await();
			  modeSwitcher.ForceElections();

			  // Then
			  InOrder inOrder = inOrder( memberAvailability, election );
			  inOrder.verify( memberAvailability ).memberIsUnavailable( HighAvailabilityModeSwitcher.SLAVE );
			  inOrder.verify( election ).performRoleElections();
			  inOrder.verifyNoMoreInteractions();
		 }

		 private class HighAvailabilityModeSwitcherAnonymousInnerClass2 : HighAvailabilityModeSwitcher
		 {
			 private readonly HighAvailabilityModeSwitcherTest _outerInstance;

			 private System.Threading.CountdownEvent _modeSwitchHappened;

			 public HighAvailabilityModeSwitcherAnonymousInnerClass2( HighAvailabilityModeSwitcherTest outerInstance, SwitchToSlaveCopyThenBranch switchToSlave, UnknownType mock, Election election, ClusterMemberAvailability memberAvailability, UnknownType mock, System.Func<StoreId> storeSupplierMock, UnknownType mock, DataSourceManager neoStoreDataSourceSupplierMock, NullLogService getInstance, System.Threading.CountdownEvent modeSwitchHappened ) : base( switchToSlave, mock, election, memberAvailability, mock, storeSupplierMock, mock, new ComponentSwitcherContainer(), neoStoreDataSourceSupplierMock, getInstance )
			 {
				 this.outerInstance = outerInstance;
				 this._modeSwitchHappened = modeSwitchHappened;
			 }

			 internal override ScheduledExecutorService createExecutor()
			 {
				  ScheduledExecutorService executor = mock( typeof( ScheduledExecutorService ) );

				  doAnswer(invocation =>
				  {
					( ( ThreadStart ) invocation.getArgument( 0 ) ).run();
					_modeSwitchHappened.Signal();
					return mock( typeof( Future ) );
				  }).when( executor ).submit( any( typeof( ThreadStart ) ) );

				  return executor;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseProperServerIdWhenDemotingFromMasterOnException() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseProperServerIdWhenDemotingFromMasterOnException()
		 {
			  /*
			   * This a test that acts as a driver to prove a bug which had an instance send out a demote message
			   * with instance id -1, since it used HAMS#getServerId(URI) with a URI coming from the NetworkReceiver binding
			   * which did not contain the serverId URI argument. This has been fixed by explicitly adding the instanceid
			   * as a constructor argument of the HAMS.
			   */
			  // Given
			  SwitchToSlaveCopyThenBranch sts = mock( typeof( SwitchToSlaveCopyThenBranch ) );
			  SwitchToMaster stm = mock( typeof( SwitchToMaster ) );
			  // this is necessary to trigger a revert which uses the serverId from the HAMS#me field
			  when( stm.SwitchToMasterConflict( any( typeof( LifeSupport ) ), any( typeof( URI ) ) ) ).thenThrow( new Exception() );
			  Election election = mock( typeof( Election ) );
			  ClusterMemberAvailability cma = mock( typeof( ClusterMemberAvailability ) );
			  InstanceId instanceId = new InstanceId( 14 );

			  HighAvailabilityModeSwitcher theSwitcher = new HighAvailabilityModeSwitcher( sts, stm, election, cma, mock( typeof( ClusterClient ) ), StoreSupplierMock(), instanceId, new ComponentSwitcherContainer(), NeoStoreDataSourceSupplierMock(), NullLogService.Instance );

			  theSwitcher.Init();
			  theSwitcher.Start();

			  /*
			   * This is the trick, kind of. NetworkReceiver creates this and passes it on to NetworkReceiver#getURI() and
			   * that
			   * is what HAMS uses as the HAMS#me field value. But we should not be using this to extract the instanceId.
			   * Note the lack of a serverId argument
			   */
			  URI listeningAt = URI.create( "ha://0.0.0.0:5001?name=someName" );
			  theSwitcher.ListeningAt( listeningAt );

			  // When
			  try
			  {
					// the instance fails to switch to master
					theSwitcher.MasterIsElected( new HighAvailabilityMemberChangeEvent( HighAvailabilityMemberState.PENDING, HighAvailabilityMemberState.TO_MASTER, instanceId, listeningAt ) );

			  }
			  finally
			  {
					theSwitcher.Stop();
					theSwitcher.Shutdown();
			  }

			  // Then
			  // The demotion message must have used the proper instance id
			  verify( election ).demote( instanceId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSwitchToSlaveForNullMasterAndBeSilentWhenMovingToDetached()
		 public virtual void ShouldSwitchToSlaveForNullMasterAndBeSilentWhenMovingToDetached()
		 {
			  // Given
			  SwitchToSlaveCopyThenBranch sts = mock( typeof( SwitchToSlaveCopyThenBranch ) );
			  SwitchToMaster stm = mock( typeof( SwitchToMaster ) );
			  Election election = mock( typeof( Election ) );
			  ClusterMemberAvailability cma = mock( typeof( ClusterMemberAvailability ) );
			  InstanceId instanceId = new InstanceId( 14 );
			  ComponentSwitcher componentSwitcher = mock( typeof( ComponentSwitcher ) );

			  HighAvailabilityModeSwitcher theSwitcher = new HighAvailabilityModeSwitcher( sts, stm, election, cma, mock( typeof( ClusterClient ) ), StoreSupplierMock(), instanceId, componentSwitcher, NeoStoreDataSourceSupplierMock(), NullLogService.Instance );

			  // When
			  theSwitcher.Init();
			  theSwitcher.Start();
			  theSwitcher.InstanceDetached( new HighAvailabilityMemberChangeEvent( HighAvailabilityMemberState.MASTER, HighAvailabilityMemberState.PENDING, null, null ) );

			  // Then
			  verify( componentSwitcher ).switchToSlave();
			  verifyZeroInteractions( cma );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static System.Func<org.neo4j.storageengine.api.StoreId> storeSupplierMock()
		 public static System.Func<StoreId> StoreSupplierMock()
		 {
			  System.Func<StoreId> supplier = mock( typeof( System.Func ) );
			  when( supplier() ).thenReturn(StoreId.DEFAULT);
			  return supplier;
		 }

		 private static HighAvailabilityModeSwitcher CreateModeSwitcher( ClusterMemberAvailability availability )
		 {
			  return new HighAvailabilityModeSwitcher( mock( typeof( SwitchToSlaveCopyThenBranch ) ), mock( typeof( SwitchToMaster ) ), mock( typeof( Election ) ), availability, mock( typeof( ClusterClient ) ), StoreSupplierMock(), mock(typeof(InstanceId)), new ComponentSwitcherContainer(), NeoStoreDataSourceSupplierMock(), NullLogService.Instance );
		 }

		 private static DataSourceManager NeoStoreDataSourceSupplierMock()
		 {
			  DataSourceManager dataSourceManager = new DataSourceManager( Config.defaults() );
			  dataSourceManager.Register( mock( typeof( NeoStoreDataSource ) ) );
			  return dataSourceManager;
		 }
	}

}