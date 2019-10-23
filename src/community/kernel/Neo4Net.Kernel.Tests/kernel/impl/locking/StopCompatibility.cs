using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.locking
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;
	using Neo4Net.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.NODE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class StopCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class StopCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		 private const long FIRST_NODE_ID = 42;
		 private const long SECOND_NODE_ID = 4242;
		 private static readonly LockTracer _tracer = LockTracer.NONE;

		 private Locks_Client _client;

		 public StopCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _client = Locks.newClient();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _client.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReleaseWriteLockWaitersOnStop()
		 public virtual void MustReleaseWriteLockWaitersOnStop()
		 {
			  // Given
			  ClientA.acquireShared( _tracer, NODE, 1L );
			  ClientB.acquireShared( _tracer, NODE, 2L );
			  ClientC.acquireShared( _tracer, NODE, 3L );
			  AcquireExclusive( ClientB, _tracer, NODE, 1L ).callAndAssertWaiting();
			  AcquireExclusive( ClientC, _tracer, NODE, 1L ).callAndAssertWaiting();

			  // When
			  ClientC.stop();
			  ClientB.stop();
			  ClientA.stop();

			  // All locks clients should be stopped at this point, and all all locks should be released because none of the
			  // clients entered the prepare phase
			  LockCountVisitor lockCountVisitor = new LockCountVisitor();
			  Locks.accept( lockCountVisitor );
			  assertEquals( 0, lockCountVisitor.LockCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotReleaseLocksAfterPrepareOnStop()
		 public virtual void MustNotReleaseLocksAfterPrepareOnStop()
		 {
			  // Given
			  ClientA.acquireShared( _tracer, NODE, 1L );
			  ClientA.acquireExclusive( _tracer, NODE, 2L );
			  ClientA.prepare();

			  // When
			  ClientA.stop();

			  // The client entered the prepare phase, so it gets to keep its locks
			  LockCountVisitor lockCountVisitor = new LockCountVisitor();
			  Locks.accept( lockCountVisitor );
			  assertEquals( 2, lockCountVisitor.LockCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReleaseUnpreparedLocksOnStop()
		 public virtual void MustReleaseUnpreparedLocksOnStop()
		 {
			  // Given
			  ClientA.acquireShared( _tracer, NODE, 1L );
			  ClientA.acquireExclusive( _tracer, NODE, 2L );

			  // When
			  ClientA.stop();

			  // The client was stopped before it could enter the prepare phase, so all of its locks are released
			  LockCountVisitor lockCountVisitor = new LockCountVisitor();
			  Locks.accept( lockCountVisitor );
			  assertEquals( 0, lockCountVisitor.LockCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReleaseReadLockWaitersOnStop()
		 public virtual void MustReleaseReadLockWaitersOnStop()
		 {
			  // Given
			  ClientA.acquireExclusive( _tracer, NODE, 1L );
			  ClientB.acquireExclusive( _tracer, NODE, 2L );
			  AcquireShared( ClientB, _tracer, NODE, 1L ).callAndAssertWaiting();

			  // When
			  ClientB.stop();
			  ClientA.stop();

			  // All locks clients should be stopped at this point, and all all locks should be released because none of the
			  // clients entered the prepare phase
			  LockCountVisitor lockCountVisitor = new LockCountVisitor();
			  Locks.accept( lockCountVisitor );
			  assertEquals( 0, lockCountVisitor.LockCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void prepareMustAllowAcquiringNewLocksAfterStop()
		 public virtual void PrepareMustAllowAcquiringNewLocksAfterStop()
		 {
			  // Given
			  ClientA.prepare();
			  ClientA.stop();

			  // When
			  ClientA.acquireShared( _tracer, NODE, 1 );
			  ClientA.acquireExclusive( _tracer, NODE, 2 );

			  // Stopped essentially has no effect when it comes after the client has entered the prepare phase
			  LockCountVisitor lockCountVisitor = new LockCountVisitor();
			  Locks.accept( lockCountVisitor );
			  assertEquals( 2, lockCountVisitor.LockCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void prepareMustThrowWhenClientStopped()
		 public virtual void PrepareMustThrowWhenClientStopped()
		 {
			  StoppedClient().prepare();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void acquireSharedThrowsWhenClientStopped()
		 public virtual void AcquireSharedThrowsWhenClientStopped()
		 {
			  StoppedClient().acquireShared(_tracer, NODE, 1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void acquireExclusiveThrowsWhenClientStopped()
		 public virtual void AcquireExclusiveThrowsWhenClientStopped()
		 {
			  StoppedClient().acquireExclusive(_tracer, NODE, 1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void trySharedLockThrowsWhenClientStopped()
		 public virtual void TrySharedLockThrowsWhenClientStopped()
		 {
			  StoppedClient().trySharedLock(NODE, 1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void tryExclusiveLockThrowsWhenClientStopped()
		 public virtual void TryExclusiveLockThrowsWhenClientStopped()
		 {
			  StoppedClient().tryExclusiveLock(NODE, 1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void releaseSharedThrowsWhenClientStopped()
		 public virtual void ReleaseSharedThrowsWhenClientStopped()
		 {
			  StoppedClient().releaseShared(NODE, 1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void releaseExclusiveThrowsWhenClientStopped()
		 public virtual void ReleaseExclusiveThrowsWhenClientStopped()
		 {
			  StoppedClient().releaseExclusive(NODE, 1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sharedLockCanBeStopped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SharedLockCanBeStopped()
		 {
			  AcquireExclusiveLockInThisThread();

			  LockAcquisition sharedLockAcquisition = AcquireSharedLockInAnotherThread();
			  AssertThreadIsWaitingForLock( sharedLockAcquisition );

			  sharedLockAcquisition.Stop();
			  AssertLockAcquisitionFailed( sharedLockAcquisition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockCanBeStopped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExclusiveLockCanBeStopped()
		 {
			  AcquireExclusiveLockInThisThread();

			  LockAcquisition exclusiveLockAcquisition = AcquireExclusiveLockInAnotherThread();
			  AssertThreadIsWaitingForLock( exclusiveLockAcquisition );

			  exclusiveLockAcquisition.Stop();
			  AssertLockAcquisitionFailed( exclusiveLockAcquisition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireSharedLockAfterSharedLockStoppedOtherThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireSharedLockAfterSharedLockStoppedOtherThread()
		 {
			  AcquiredLock thisThreadsExclusiveLock = AcquireExclusiveLockInThisThread();

			  LockAcquisition sharedLockAcquisition1 = AcquireSharedLockInAnotherThread();
			  AssertThreadIsWaitingForLock( sharedLockAcquisition1 );

			  sharedLockAcquisition1.Stop();
			  AssertLockAcquisitionFailed( sharedLockAcquisition1 );

			  thisThreadsExclusiveLock.Release();

			  LockAcquisition sharedLockAcquisition2 = AcquireSharedLockInAnotherThread();
			  AssertLockAcquisitionSucceeded( sharedLockAcquisition2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireExclusiveLockAfterExclusiveLockStoppedOtherThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireExclusiveLockAfterExclusiveLockStoppedOtherThread()
		 {
			  AcquiredLock thisThreadsExclusiveLock = AcquireExclusiveLockInThisThread();

			  LockAcquisition exclusiveLockAcquisition1 = AcquireExclusiveLockInAnotherThread();
			  AssertThreadIsWaitingForLock( exclusiveLockAcquisition1 );

			  exclusiveLockAcquisition1.Stop();
			  AssertLockAcquisitionFailed( exclusiveLockAcquisition1 );

			  thisThreadsExclusiveLock.Release();

			  LockAcquisition exclusiveLockAcquisition2 = AcquireExclusiveLockInAnotherThread();
			  AssertLockAcquisitionSucceeded( exclusiveLockAcquisition2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireSharedLockAfterExclusiveLockStoppedOtherThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireSharedLockAfterExclusiveLockStoppedOtherThread()
		 {
			  AcquiredLock thisThreadsExclusiveLock = AcquireExclusiveLockInThisThread();

			  LockAcquisition exclusiveLockAcquisition = AcquireExclusiveLockInAnotherThread();
			  AssertThreadIsWaitingForLock( exclusiveLockAcquisition );

			  exclusiveLockAcquisition.Stop();
			  AssertLockAcquisitionFailed( exclusiveLockAcquisition );

			  thisThreadsExclusiveLock.Release();

			  LockAcquisition sharedLockAcquisition = AcquireSharedLockInAnotherThread();
			  AssertLockAcquisitionSucceeded( sharedLockAcquisition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireExclusiveLockAfterSharedLockStoppedOtherThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireExclusiveLockAfterSharedLockStoppedOtherThread()
		 {
			  AcquiredLock thisThreadsExclusiveLock = AcquireExclusiveLockInThisThread();

			  LockAcquisition sharedLockAcquisition = AcquireSharedLockInAnotherThread();
			  AssertThreadIsWaitingForLock( sharedLockAcquisition );

			  sharedLockAcquisition.Stop();
			  AssertLockAcquisitionFailed( sharedLockAcquisition );

			  thisThreadsExclusiveLock.Release();

			  LockAcquisition exclusiveLockAcquisition = AcquireExclusiveLockInAnotherThread();
			  AssertLockAcquisitionSucceeded( exclusiveLockAcquisition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireSharedLockAfterSharedLockStoppedSameThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireSharedLockAfterSharedLockStoppedSameThread()
		 {
			  AcquireLockAfterOtherLockStoppedSameThread( true, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireExclusiveLockAfterExclusiveLockStoppedSameThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireExclusiveLockAfterExclusiveLockStoppedSameThread()
		 {
			  AcquireLockAfterOtherLockStoppedSameThread( false, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireSharedLockAfterExclusiveLockStoppedSameThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireSharedLockAfterExclusiveLockStoppedSameThread()
		 {
			  AcquireLockAfterOtherLockStoppedSameThread( true, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireExclusiveLockAfterSharedLockStoppedSameThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireExclusiveLockAfterSharedLockStoppedSameThread()
		 {
			  AcquireLockAfterOtherLockStoppedSameThread( false, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeClientAfterSharedLockStopped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseClientAfterSharedLockStopped()
		 {
			  CloseClientAfterLockStopped( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeClientAfterExclusiveLockStopped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseClientAfterExclusiveLockStopped()
		 {
			  CloseClientAfterLockStopped( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireExclusiveLockWhileHoldingSharedLockCanBeStopped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AcquireExclusiveLockWhileHoldingSharedLockCanBeStopped()
		 {
			  AcquiredLock thisThreadsSharedLock = AcquireSharedLockInThisThread();

			  System.Threading.CountdownEvent sharedLockAcquired = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent startExclusiveLock = new System.Threading.CountdownEvent( 1 );
			  LockAcquisition acquisition = AcquireSharedAndExclusiveLocksInAnotherThread( sharedLockAcquired, startExclusiveLock );

			  Await( sharedLockAcquired );
			  startExclusiveLock.Signal();
			  AssertThreadIsWaitingForLock( acquisition );

			  acquisition.Stop();
			  AssertLockAcquisitionFailed( acquisition );

			  thisThreadsSharedLock.Release();
			  AssertNoLocksHeld();
		 }

		 private Locks_Client StoppedClient()
		 {
			  try
			  {
					_client.stop();
					return _client;
			  }
			  catch ( Exception t )
			  {
					throw new AssertionError( "Unable to stop client", t );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeClientAfterLockStopped(boolean shared) throws Exception
		 private void CloseClientAfterLockStopped( bool shared )
		 {
			  AcquiredLock thisThreadsExclusiveLock = AcquireExclusiveLockInThisThread();

			  System.Threading.CountdownEvent firstLockAcquired = new System.Threading.CountdownEvent( 1 );
			  LockAcquisition acquisition = TryAcquireTwoLocksLockInAnotherThread( shared, firstLockAcquired );

			  Await( firstLockAcquired );
			  AssertThreadIsWaitingForLock( acquisition );
			  AssertLocksHeld( FIRST_NODE_ID, SECOND_NODE_ID );

			  acquisition.Stop();
			  AssertLockAcquisitionFailed( acquisition );
			  AssertLocksHeld( FIRST_NODE_ID );

			  thisThreadsExclusiveLock.Release();
			  AssertNoLocksHeld();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void acquireLockAfterOtherLockStoppedSameThread(boolean firstLockShared, boolean secondLockShared) throws Exception
		 private void AcquireLockAfterOtherLockStoppedSameThread( bool firstLockShared, bool secondLockShared )
		 {
			  AcquiredLock thisThreadsExclusiveLock = AcquireExclusiveLockInThisThread();

			  System.Threading.CountdownEvent firstLockFailed = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent startSecondLock = new System.Threading.CountdownEvent( 1 );

			  LockAcquisition lockAcquisition = AcquireTwoLocksInAnotherThread( firstLockShared, secondLockShared, firstLockFailed, startSecondLock );
			  AssertThreadIsWaitingForLock( lockAcquisition );

			  lockAcquisition.Stop();
			  Await( firstLockFailed );
			  thisThreadsExclusiveLock.Release();
			  startSecondLock.Signal();

			  AssertLockAcquisitionSucceeded( lockAcquisition );
		 }

		 private AcquiredLock AcquireSharedLockInThisThread()
		 {
			  _client.acquireShared( _tracer, NODE, FIRST_NODE_ID );
			  AssertLocksHeld( FIRST_NODE_ID );
			  return AcquiredLock.Shared( _client, NODE, FIRST_NODE_ID );
		 }

		 private AcquiredLock AcquireExclusiveLockInThisThread()
		 {
			  _client.acquireExclusive( _tracer, NODE, FIRST_NODE_ID );
			  AssertLocksHeld( FIRST_NODE_ID );
			  return AcquiredLock.Exclusive( _client, NODE, FIRST_NODE_ID );
		 }

		 private LockAcquisition AcquireSharedLockInAnotherThread()
		 {
			  return AcquireLockInAnotherThread( true );
		 }

		 private LockAcquisition AcquireExclusiveLockInAnotherThread()
		 {
			  return AcquireLockInAnotherThread( false );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private LockAcquisition acquireLockInAnotherThread(final boolean shared)
		 private LockAcquisition AcquireLockInAnotherThread( bool shared )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockAcquisition lockAcquisition = new LockAcquisition();
			  LockAcquisition lockAcquisition = new LockAcquisition();

			  Future<Void> future = ThreadA.execute(state =>
			  {
				Locks_Client client = NewLockClient( lockAcquisition );
				if ( shared )
				{
					 client.AcquireShared( _tracer, NODE, FIRST_NODE_ID );
				}
				else
				{
					 client.AcquireExclusive( _tracer, NODE, FIRST_NODE_ID );
				}
				return null;
			  });
			  lockAcquisition.SetFuture( future, ThreadA.get() );

			  return lockAcquisition;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private LockAcquisition acquireTwoLocksInAnotherThread(final boolean firstShared, final boolean secondShared, final java.util.concurrent.CountDownLatch firstLockFailed, final java.util.concurrent.CountDownLatch startSecondLock)
		 private LockAcquisition AcquireTwoLocksInAnotherThread( bool firstShared, bool secondShared, System.Threading.CountdownEvent firstLockFailed, System.Threading.CountdownEvent startSecondLock )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockAcquisition lockAcquisition = new LockAcquisition();
			  LockAcquisition lockAcquisition = new LockAcquisition();

			  Future<Void> future = ThreadA.execute(state =>
			  {
				using ( Locks_Client client = NewLockClient( lockAcquisition ) )
				{
					 try
					 {
						  if ( firstShared )
						  {
								client.AcquireShared( _tracer, NODE, FIRST_NODE_ID );
						  }
						  else
						  {
								client.AcquireExclusive( _tracer, NODE, FIRST_NODE_ID );
						  }
						  fail( "Transaction termination expected" );
					 }
					 catch ( Exception e )
					 {
						  assertThat( e, instanceOf( typeof( LockClientStoppedException ) ) );
					 }
				}

				lockAcquisition.Client = null;
				firstLockFailed.Signal();
				Await( startSecondLock );

				using ( Locks_Client client = NewLockClient( lockAcquisition ) )
				{
					 if ( secondShared )
					 {
						  client.AcquireShared( _tracer, NODE, FIRST_NODE_ID );
					 }
					 else
					 {
						  client.AcquireExclusive( _tracer, NODE, FIRST_NODE_ID );
					 }
				}
				return null;
			  });
			  lockAcquisition.SetFuture( future, ThreadA.get() );

			  return lockAcquisition;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private LockAcquisition acquireSharedAndExclusiveLocksInAnotherThread(final java.util.concurrent.CountDownLatch sharedLockAcquired, final java.util.concurrent.CountDownLatch startExclusiveLock)
		 private LockAcquisition AcquireSharedAndExclusiveLocksInAnotherThread( System.Threading.CountdownEvent sharedLockAcquired, System.Threading.CountdownEvent startExclusiveLock )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockAcquisition lockAcquisition = new LockAcquisition();
			  LockAcquisition lockAcquisition = new LockAcquisition();

			  Future<Void> future = ThreadA.execute(state =>
			  {
				using ( Locks_Client client = NewLockClient( lockAcquisition ) )
				{
					 client.AcquireShared( _tracer, NODE, FIRST_NODE_ID );

					 sharedLockAcquired.Signal();
					 Await( startExclusiveLock );

					 client.AcquireExclusive( _tracer, NODE, FIRST_NODE_ID );
				}
				return null;
			  });
			  lockAcquisition.SetFuture( future, ThreadA.get() );

			  return lockAcquisition;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private LockAcquisition tryAcquireTwoLocksLockInAnotherThread(final boolean shared, final java.util.concurrent.CountDownLatch firstLockAcquired)
		 private LockAcquisition TryAcquireTwoLocksLockInAnotherThread( bool shared, System.Threading.CountdownEvent firstLockAcquired )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockAcquisition lockAcquisition = new LockAcquisition();
			  LockAcquisition lockAcquisition = new LockAcquisition();

			  Future<Void> future = ThreadA.execute(state =>
			  {
				using ( Locks_Client client = NewLockClient( lockAcquisition ) )
				{
					 if ( shared )
					 {
						  client.AcquireShared( _tracer, NODE, SECOND_NODE_ID );
					 }
					 else
					 {
						  client.AcquireExclusive( _tracer, NODE, SECOND_NODE_ID );
					 }

					 firstLockAcquired.Signal();

					 if ( shared )
					 {
						  client.AcquireShared( _tracer, NODE, FIRST_NODE_ID );
					 }
					 else
					 {
						  client.AcquireExclusive( _tracer, NODE, FIRST_NODE_ID );
					 }
				}
				return null;
			  });
			  lockAcquisition.SetFuture( future, ThreadA.get() );

			  return lockAcquisition;
		 }

		 private Locks_Client NewLockClient( LockAcquisition lockAcquisition )
		 {
			  Locks_Client client = Locks.newClient();
			  lockAcquisition.Client = client;
			  return client;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void assertLocksHeld(final Long... expectedResourceIds)
		 private void AssertLocksHeld( params Long[] expectedResourceIds )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<long> expectedLockedIds = java.util.Arrays.asList(expectedResourceIds);
			  IList<long> expectedLockedIds = Arrays.asList( expectedResourceIds );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<long> seenLockedIds = new java.util.ArrayList<>();
			  IList<long> seenLockedIds = new List<long>();

			  Locks.accept( ( resourceType, resourceId, description, estimatedWaitTime, lockIdentityHashCode ) => seenLockedIds.Add( resourceId ) );

			  expectedLockedIds.Sort();
			  seenLockedIds.Sort();
			  assertEquals( "unexpected locked resource ids", expectedLockedIds, seenLockedIds );
		 }

		 private void AssertNoLocksHeld()
		 {
			  Locks.accept( ( resourceType, resourceId, description, estimatedWaitTime, lockIdentityHashCode ) => fail( "Unexpected lock on " + resourceType + " " + resourceId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertThreadIsWaitingForLock(LockAcquisition lockAcquisition) throws Exception
		 private void AssertThreadIsWaitingForLock( LockAcquisition lockAcquisition )
		 {
			  for ( int i = 0; i < 30 && !Suite.isAwaitingLockAcquisition( lockAcquisition.Executor.waitUntilWaiting() ); i++ )
			  {
					LockSupport.parkNanos( MILLISECONDS.toNanos( 100 ) );
			  }
			  assertFalse( "locking thread completed", lockAcquisition.Completed() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertLockAcquisitionSucceeded(LockAcquisition lockAcquisition) throws Exception
		 private void AssertLockAcquisitionSucceeded( LockAcquisition lockAcquisition )
		 {
			  bool completed = false;
			  for ( int i = 0; i < 30; i++ )
			  {
					try
					{
						 assertNull( lockAcquisition.Result() );
						 completed = true;
					}
					catch ( TimeoutException )
					{
					}
			  }
			  assertTrue( "lock was not acquired in time", completed );
			  assertTrue( "locking thread seem to be still in progress", lockAcquisition.Completed() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertLockAcquisitionFailed(LockAcquisition lockAcquisition) throws Exception
		 private void AssertLockAcquisitionFailed( LockAcquisition lockAcquisition )
		 {
			  ExecutionException executionException = null;
			  for ( int i = 0; i < 30; i++ )
			  {
					try
					{
						 lockAcquisition.Result();
						 fail( "Transaction termination expected" );
					}
					catch ( ExecutionException e )
					{
						 executionException = e;
					}
					catch ( TimeoutException )
					{
					}
			  }
			  assertNotNull( "execution should fail", executionException );
			  assertThat( executionException.InnerException, instanceOf( typeof( LockClientStoppedException ) ) );
			  assertTrue( "locking thread seem to be still in progress", lockAcquisition.Completed() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void await(java.util.concurrent.CountDownLatch latch) throws InterruptedException
		 private static void Await( System.Threading.CountdownEvent latch )
		 {
			  if ( !latch.await( 1, TimeUnit.MINUTES ) )
			  {
					fail( "Count down did not happen" );
			  }
		 }

		 private class LockAcquisition
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: volatile java.util.concurrent.Future<?> future;
			  internal volatile Future<object> FutureConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile Locks_Client ClientConflict;
			  internal volatile OtherThreadExecutor<Void> Executor;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> getFuture()
			  internal virtual Future<object> Future
			  {
				  get
				  {
						Objects.requireNonNull( FutureConflict, "lock acquisition was not initialized with future" );
						return FutureConflict;
				  }
			  }

			  internal virtual void SetFuture<T1>( Future<T1> future, OtherThreadExecutor<Void> executor )
			  {
					this.FutureConflict = future;
					this.Executor = executor;
			  }

			  internal virtual Locks_Client Client
			  {
				  get
				  {
						Objects.requireNonNull( ClientConflict, "lock acquisition was not initialized with client" );
						return ClientConflict;
				  }
				  set
				  {
						this.ClientConflict = value;
				  }
			  }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object result() throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
			  internal virtual object Result()
			  {
					return Future.get( 100, TimeUnit.MILLISECONDS );
			  }

			  internal virtual bool Completed()
			  {
					return Future.Done;
			  }

			  internal virtual void Stop()
			  {
					Client.stop();
			  }
		 }

		 private class AcquiredLock
		 {
			  internal readonly Locks_Client Client;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool SharedConflict;
			  internal readonly ResourceType ResourceType;
			  internal readonly long ResourceId;

			  internal AcquiredLock( Locks_Client client, bool shared, ResourceType resourceType, long resourceId )
			  {
					this.Client = client;
					this.SharedConflict = shared;
					this.ResourceType = resourceType;
					this.ResourceId = resourceId;
			  }

			  internal static AcquiredLock Shared( Locks_Client client, ResourceType resourceType, long resourceId )
			  {
					return new AcquiredLock( client, true, resourceType, resourceId );
			  }

			  internal static AcquiredLock Exclusive( Locks_Client client, ResourceType resourceType, long resourceId )
			  {
					return new AcquiredLock( client, false, resourceType, resourceId );
			  }

			  internal virtual void Release()
			  {
					if ( SharedConflict )
					{
						 Client.releaseShared( ResourceType, ResourceId );
					}
					else
					{
						 Client.releaseExclusive( ResourceType, ResourceId );
					}
			  }
		 }
	}

}