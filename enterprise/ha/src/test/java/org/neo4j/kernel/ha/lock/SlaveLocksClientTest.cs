using System;

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
namespace Org.Neo4j.Kernel.ha.@lock
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using InOrder = org.mockito.InOrder;
	using OngoingStubbing = org.mockito.stubbing.OngoingStubbing;

	using ComException = Org.Neo4j.com.ComException;
	using ResourceReleaser = Org.Neo4j.com.ResourceReleaser;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using TransactionStream = Org.Neo4j.com.TransactionStream;
	using Org.Neo4j.com;
	using TransientFailureException = Org.Neo4j.Graphdb.TransientFailureException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using LockClientStoppedException = Org.Neo4j.Kernel.impl.locking.LockClientStoppedException;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using CommunityLockManger = Org.Neo4j.Kernel.impl.locking.community.CommunityLockManger;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.ResourceReleaser_Fields.NO_OP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLog.getInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.StoreId.DEFAULT;

	public class SlaveLocksClientTest
	{
		 private Master _master;
		 private Locks _lockManager;
		 private Org.Neo4j.Kernel.impl.locking.Locks_Client _local;
		 private SlaveLocksClient _client;
		 private DatabaseAvailabilityGuard _databaseAvailabilityGuard;
		 private AssertableLogProvider _logProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _master = mock( typeof( Master ) );
			  _databaseAvailabilityGuard = new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, Clocks.fakeClock(), Instance );

			  _lockManager = new CommunityLockManger( Config.defaults(), Clocks.systemClock() );
			  _local = spy( _lockManager.newClient() );
			  _logProvider = new AssertableLogProvider();

			  LockResult lockResultOk = new LockResult( LockStatus.OkLocked );
			  TransactionStreamResponse<LockResult> responseOk = new TransactionStreamResponse<LockResult>( lockResultOk, null, Org.Neo4j.com.TransactionStream_Fields.Empty, Org.Neo4j.com.ResourceReleaser_Fields.NoOp );

			  WhenMasterAcquireShared().thenReturn(responseOk);

			  WhenMasterAcquireExclusive().thenReturn(responseOk);

			  _client = new SlaveLocksClient( _master, _local, _lockManager, mock( typeof( RequestContextFactory ) ), _databaseAvailabilityGuard, _logProvider );
		 }

		 private OngoingStubbing<Response<LockResult>> WhenMasterAcquireShared()
		 {
			  return when( _master.acquireSharedLock( Null, any( typeof( ResourceType ) ), ArgumentMatchers.any<long[]>() ) );
		 }

		 private OngoingStubbing<Response<LockResult>> WhenMasterAcquireExclusive()
		 {
			  return when( _master.acquireExclusiveLock( Null, any( typeof( ResourceType ) ), ArgumentMatchers.any<long[]>() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _local.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTakeSharedLockOnMasterIfWeAreAlreadyHoldingSaidLock()
		 public virtual void ShouldNotTakeSharedLockOnMasterIfWeAreAlreadyHoldingSaidLock()
		 {
			  // When taking a lock twice
			  _client.acquireShared( LockTracer.NONE, NODE, 1 );
			  _client.acquireShared( LockTracer.NONE, NODE, 1 );

			  // Then only a single network round-trip should be observed
			  verify( _master ).acquireSharedLock( null, NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTakeExclusiveLockOnMasterIfWeAreAlreadyHoldingSaidLock()
		 public virtual void ShouldNotTakeExclusiveLockOnMasterIfWeAreAlreadyHoldingSaidLock()
		 {
			  // When taking a lock twice
			  _client.acquireExclusive( LockTracer.NONE, NODE, 1 );
			  _client.acquireExclusive( LockTracer.NONE, NODE, 1 );

			  // Then only a single network roundtrip should be observed
			  verify( _master ).acquireExclusiveLock( null, NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowAcquiringReleasingAndReacquiringExclusive()
		 public virtual void ShouldAllowAcquiringReleasingAndReacquiringExclusive()
		 {
			  // Given we have grabbed and released a lock
			  _client.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  _client.releaseExclusive( NODE, 1L );

			  // When we grab and release that lock again
			  _client.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  _client.releaseExclusive( NODE, 1L );

			  // Then this should cause the local lock manager to hold the lock
			  verify( _local, times( 2 ) ).tryExclusiveLock( NODE, 1L );
			  verify( _local, times( 2 ) ).releaseExclusive( NODE, 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowAcquiringReleasingAndReacquiringShared()
		 public virtual void ShouldAllowAcquiringReleasingAndReacquiringShared()
		 {
			  // Given we have grabbed and released a lock
			  _client.acquireShared( LockTracer.NONE, NODE, 1L );
			  _client.releaseShared( NODE, 1L );

			  // When we grab and release that lock again
			  _client.acquireShared( LockTracer.NONE, NODE, 1L );
			  _client.releaseShared( NODE, 1L );

			  // Then this should cause the local lock manager to hold the lock
			  verify( _local, times( 2 ) ).trySharedLock( NODE, 1L );
			  verify( _local, times( 2 ) ).releaseShared( NODE, 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseReEntryMethodsOnLocalLocksForReEntryExclusive()
		 public virtual void ShouldUseReEntryMethodsOnLocalLocksForReEntryExclusive()
		 {
			  // Given we have grabbed and released a lock
			  _client.acquireExclusive( LockTracer.NONE, NODE, 1L );

			  // When we grab and release that lock again
			  _client.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  _client.releaseExclusive( NODE, 1L );

			  // Then this should cause the local lock manager to hold the lock
			  InOrder order = inOrder( _local );
			  order.verify( _local, times( 1 ) ).reEnterExclusive( NODE, 1L );
			  order.verify( _local, times( 1 ) ).tryExclusiveLock( NODE, 1L );
			  order.verify( _local, times( 1 ) ).reEnterExclusive( NODE, 1L );
			  order.verify( _local, times( 1 ) ).releaseExclusive( NODE, 1L );
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseReEntryMethodsOnLocalLocksForReEntryShared()
		 public virtual void ShouldUseReEntryMethodsOnLocalLocksForReEntryShared()
		 {
			  // Given we have grabbed and released a lock
			  _client.acquireShared( LockTracer.NONE, NODE, 1L );

			  // When we grab and release that lock again
			  _client.acquireShared( LockTracer.NONE, NODE, 1L );
			  _client.releaseShared( NODE, 1L );

			  // Then this should cause the local lock manager to hold the lock
			  InOrder order = inOrder( _local );
			  order.verify( _local, times( 1 ) ).reEnterShared( NODE, 1L );
			  order.verify( _local, times( 1 ) ).trySharedLock( NODE, 1L );
			  order.verify( _local, times( 1 ) ).reEnterShared( NODE, 1L );
			  order.verify( _local, times( 1 ) ).releaseShared( NODE, 1L );
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoLockSessionIfNotInitialized()
		 public virtual void ShouldReturnNoLockSessionIfNotInitialized()
		 {
			  // When
			  int lockSessionId = _client.LockSessionId;

			  // Then
			  assertThat( lockSessionId, equalTo( -1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnDelegateIdIfInitialized()
		 public virtual void ShouldReturnDelegateIdIfInitialized()
		 {
			  // Given
			  _client.acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, 1L );

			  // When
			  int lockSessionId = _client.LockSessionId;

			  // Then
			  assertThat( lockSessionId, equalTo( _local.LockSessionId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = DistributedLockFailureException.class) public void mustThrowIfStartingNewLockSessionOnMasterThrowsComException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustThrowIfStartingNewLockSessionOnMasterThrowsComException()
		 {
			  when( _master.newLockSession( Null ) ).thenThrow( new ComException() );

			  _client.acquireShared( LockTracer.NONE, NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = DistributedLockFailureException.class) public void mustThrowIfStartingNewLockSessionOnMasterThrowsTransactionFailureException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustThrowIfStartingNewLockSessionOnMasterThrowsTransactionFailureException()
		 {
			  when( _master.newLockSession( Null ) ).thenThrow( new TransactionFailureException( Org.Neo4j.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, "Not now" ) );

			  _client.acquireShared( LockTracer.NONE, NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = DistributedLockFailureException.class) public void acquireSharedMustThrowIfMasterThrows()
		 public virtual void AcquireSharedMustThrowIfMasterThrows()
		 {
			  WhenMasterAcquireShared().thenThrow(new ComException());

			  _client.acquireShared( LockTracer.NONE, NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = DistributedLockFailureException.class) public void acquireExclusiveMustThrowIfMasterThrows()
		 public virtual void AcquireExclusiveMustThrowIfMasterThrows()
		 {
			  WhenMasterAcquireExclusive().thenThrow(new ComException());

			  _client.acquireExclusive( LockTracer.NONE, NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnsupportedOperationException.class) public void tryExclusiveMustBeUnsupported()
		 public virtual void TryExclusiveMustBeUnsupported()
		 {
			  _client.tryExclusiveLock( NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnsupportedOperationException.class) public void trySharedMustBeUnsupported()
		 public virtual void TrySharedMustBeUnsupported()
		 {
			  _client.trySharedLock( NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = DistributedLockFailureException.class) public void closeMustThrowIfMasterThrows()
		 public virtual void CloseMustThrowIfMasterThrows()
		 {
			  when( _master.endLockSession( Null, anyBoolean() ) ).thenThrow(new ComException());

			  _client.acquireExclusive( LockTracer.NONE, NODE, 1 ); // initialise
			  _client.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCloseLocalClientEvenIfMasterThrows()
		 public virtual void MustCloseLocalClientEvenIfMasterThrows()
		 {
			  when( _master.endLockSession( Null, anyBoolean() ) ).thenThrow(new ComException());

			  try
			  {
					_client.acquireExclusive( LockTracer.NONE, NODE, 1 ); // initialise
					_client.close();
					fail( "Expected client.close to throw" );
			  }
			  catch ( Exception )
			  {
			  }
			  verify( _local ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.TransientDatabaseFailureException.class) public void mustThrowTransientTransactionFailureIfDatabaseUnavailable()
		 public virtual void MustThrowTransientTransactionFailureIfDatabaseUnavailable()
		 {
			  _databaseAvailabilityGuard.shutdown();

			  _client.acquireExclusive( LockTracer.NONE, NODE, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWithTransientErrorOnDbUnavailable()
		 public virtual void ShouldFailWithTransientErrorOnDbUnavailable()
		 {
			  // GIVEN
			  _databaseAvailabilityGuard.shutdown();

			  // WHEN
			  try
			  {
					_client.acquireExclusive( LockTracer.NONE, NODE, 0 );
					fail( "Should fail" );
			  }
			  catch ( TransientFailureException )
			  {
					// THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireSharedFailsWhenClientStopped()
		 public virtual void AcquireSharedFailsWhenClientStopped()
		 {
			  SlaveLocksClient client = StoppedClient();
			  try
			  {
					client.AcquireShared( LockTracer.NONE, NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseSharedFailsWhenClientStopped()
		 public virtual void ReleaseSharedFailsWhenClientStopped()
		 {
			  SlaveLocksClient client = StoppedClient();
			  try
			  {
					client.ReleaseShared( NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireExclusiveFailsWhenClientStopped()
		 public virtual void AcquireExclusiveFailsWhenClientStopped()
		 {
			  SlaveLocksClient client = StoppedClient();
			  try
			  {
					client.AcquireExclusive( LockTracer.NONE, NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseExclusiveFailsWhenClientStopped()
		 public virtual void ReleaseExclusiveFailsWhenClientStopped()
		 {
			  SlaveLocksClient client = StoppedClient();
			  try
			  {
					client.ReleaseExclusive( NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLockSessionIdWhenClientStopped()
		 public virtual void getLockSessionIdWhenClientStopped()
		 {
			  SlaveLocksClient client = StoppedClient();
			  try
			  {
					client.LockSessionId;
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireSharedFailsWhenClientClosed()
		 public virtual void AcquireSharedFailsWhenClientClosed()
		 {
			  SlaveLocksClient client = ClosedClient();
			  try
			  {
					client.AcquireShared( LockTracer.NONE, NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseSharedFailsWhenClientClosed()
		 public virtual void ReleaseSharedFailsWhenClientClosed()
		 {
			  SlaveLocksClient client = ClosedClient();
			  try
			  {
					client.ReleaseShared( NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireExclusiveFailsWhenClientClosed()
		 public virtual void AcquireExclusiveFailsWhenClientClosed()
		 {
			  SlaveLocksClient client = ClosedClient();
			  try
			  {
					client.AcquireExclusive( LockTracer.NONE, NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseExclusiveFailsWhenClientClosed()
		 public virtual void ReleaseExclusiveFailsWhenClientClosed()
		 {
			  SlaveLocksClient client = ClosedClient();
			  try
			  {
					client.ReleaseExclusive( NODE, 1 );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getLockSessionIdWhenClientClosed()
		 public virtual void getLockSessionIdWhenClientClosed()
		 {
			  SlaveLocksClient client = ClosedClient();
			  try
			  {
					client.LockSessionId;
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopLocalLocksAndEndLockSessionOnMasterWhenStopped()
		 public virtual void StopLocalLocksAndEndLockSessionOnMasterWhenStopped()
		 {
			  _client.acquireShared( LockTracer.NONE, NODE, 1 );

			  _client.stop();

			  verify( _local ).stop();
			  verify( _master ).endLockSession( Null, eq( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeLocalLocksAndEndLockSessionOnMasterWhenClosed()
		 public virtual void CloseLocalLocksAndEndLockSessionOnMasterWhenClosed()
		 {
			  _client.acquireShared( LockTracer.NONE, NODE, 1 );

			  _client.close();

			  verify( _local ).close();
			  verify( _master ).endLockSession( Null, eq( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeAfterStopped()
		 public virtual void CloseAfterStopped()
		 {
			  _client.acquireShared( LockTracer.NONE, NODE, 1 );

			  _client.stop();
			  _client.close();

			  InOrder inOrder = inOrder( _master, _local );
			  inOrder.verify( _master ).endLockSession( Null, eq( false ) );
			  inOrder.verify( _local ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeWhenNotInitialized()
		 public virtual void CloseWhenNotInitialized()
		 {
			  _client.close();

			  verify( _local ).close();
			  verifyNoMoreInteractions( _master );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopDoesNotThrowWhenMasterCommunicationThrowsComException()
		 public virtual void StopDoesNotThrowWhenMasterCommunicationThrowsComException()
		 {
			  ComException error = new ComException( "Communication failure" );
			  when( _master.endLockSession( Null, anyBoolean() ) ).thenThrow(error);

			  _client.stop();

			  _logProvider.assertExactly( inLog( typeof( SlaveLocksClient ) ).warn( equalTo( "Unable to stop lock session on master" ), CoreMatchers.instanceOf( typeof( DistributedLockFailureException ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopDoesNotThrowWhenMasterCommunicationThrows()
		 public virtual void StopDoesNotThrowWhenMasterCommunicationThrows()
		 {
			  Exception error = new System.ArgumentException( "Wrong params" );
			  when( _master.endLockSession( Null, anyBoolean() ) ).thenThrow(error);

			  _client.stop();

			  _logProvider.assertExactly( inLog( typeof( SlaveLocksClient ) ).warn( equalTo( "Unable to stop lock session on master" ), CoreMatchers.equalTo( error ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeReasonForNotLocked()
		 public virtual void ShouldIncludeReasonForNotLocked()
		 {
			  // GIVEN
			  SlaveLocksClient client = NewSlaveLocksClient( _lockManager );
			  LockResult lockResult = new LockResult( LockStatus.NotLocked, "Simply not locked" );
			  Response<LockResult> response = new TransactionObligationResponse<LockResult>( lockResult, DEFAULT, 2, NO_OP );
			  long nodeId = 0;
			  ResourceTypes resourceType = NODE;
			  when( _master.acquireExclusiveLock( Null, eq( resourceType ), anyLong() ) ).thenReturn(response);

			  // WHEN
			  try
			  {
					client.AcquireExclusive( LockTracer.NONE, resourceType, nodeId );
					fail( "Should have failed" );
			  }
			  catch ( System.NotSupportedException e )
			  {
					// THEN
					assertThat( e.Message, containsString( lockResult.Message ) );
					assertThat( e.Message, containsString( lockResult.Status.name() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireDeferredSharedLocksForLabelsAndRelationshipTypes()
		 public virtual void AcquireDeferredSharedLocksForLabelsAndRelationshipTypes()
		 {
			  foreach ( ResourceTypes type in ResourceTypes.values() )
			  {
					_client.acquireShared( LockTracer.NONE, type, 1, 2 );
			  }
			  foreach ( ResourceTypes type in ResourceTypes.values() )
			  {
					_client.acquireShared( LockTracer.NONE, type, 2, 3 );
			  }
			  _client.acquireShared( LockTracer.NONE, ResourceTypes.LABEL, 7 );
			  _client.acquireShared( LockTracer.NONE, ResourceTypes.RELATIONSHIP_TYPE, 12 );

			  _client.acquireDeferredSharedLocks( LockTracer.NONE );

			  verify( _master ).acquireSharedLock( null, ResourceTypes.LABEL, 1, 2, 3, 7 );
			  verify( _master ).acquireSharedLock( null, ResourceTypes.RELATIONSHIP_TYPE, 1, 2, 3, 12 );
		 }

		 private SlaveLocksClient NewSlaveLocksClient( Locks lockManager )
		 {
			  return new SlaveLocksClient( _master, _local, lockManager, mock( typeof( RequestContextFactory ) ), _databaseAvailabilityGuard, _logProvider );
		 }

		 private SlaveLocksClient StoppedClient()
		 {
			  _client.stop();
			  return _client;
		 }

		 private SlaveLocksClient ClosedClient()
		 {
			  _client.acquireShared( LockTracer.NONE, NODE, 1 ); // trigger new lock session initialization
			  _client.close();
			  return _client;
		 }
	}

}