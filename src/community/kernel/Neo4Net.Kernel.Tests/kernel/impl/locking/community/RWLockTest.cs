using System.Threading;

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
namespace Neo4Net.Kernel.impl.locking.community
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using Transaction = Neo4Net.GraphDb.Transaction;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class RWLockTest
	{
		 private const long TEST_TIMEOUT_MILLIS = 10_000;

		 private static ExecutorService _executor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void initExecutor()
		 public static void InitExecutor()
		 {
			  _executor = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopExecutor() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void StopExecutor()
		 {
			  _executor.shutdown();
			  _executor.awaitTermination( 2, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void assertWriteLockDoesNotLeakMemory()
		 public virtual void AssertWriteLockDoesNotLeakMemory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RagManager ragManager = new RagManager();
			  RagManager ragManager = new RagManager();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockResource resource = new LockResource(Neo4Net.kernel.impl.locking.ResourceTypes.NODE, 0);
			  LockResource resource = new LockResource( ResourceTypes.NODE, 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lock = createRWLock(ragManager, resource);
			  RWLock @lock = CreateRWLock( ragManager, resource );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Transaction tx1 = mock(Neo4Net.graphdb.Transaction.class);
			  Transaction tx1 = mock( typeof( Transaction ) );

			  @lock.Mark();
			  @lock.AcquireWriteLock( LockTracer.NONE, tx1 );
			  @lock.Mark();

			  assertEquals( 1, @lock.TxLockElementCount );
			  @lock.ReleaseWriteLock( tx1 );
			  assertEquals( 0, @lock.TxLockElementCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void assertReadLockDoesNotLeakMemory()
		 public virtual void AssertReadLockDoesNotLeakMemory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RagManager ragManager = new RagManager();
			  RagManager ragManager = new RagManager();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockResource resource = new LockResource(Neo4Net.kernel.impl.locking.ResourceTypes.NODE, 0);
			  LockResource resource = new LockResource( ResourceTypes.NODE, 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lock = createRWLock(ragManager, resource);
			  RWLock @lock = CreateRWLock( ragManager, resource );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Transaction tx1 = mock(Neo4Net.graphdb.Transaction.class);
			  Transaction tx1 = mock( typeof( Transaction ) );

			  @lock.Mark();
			  @lock.AcquireReadLock( LockTracer.NONE, tx1 );
			  @lock.Mark();

			  assertEquals( 1, @lock.TxLockElementCount );
			  @lock.ReleaseReadLock( tx1 );
			  assertEquals( 0, @lock.TxLockElementCount );
		 }

		 /*
		  * In case if writer thread can't grab write lock now, it should be added to
		  * into a waiting list, wait till resource will be free and grab it.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT_MILLIS) public void testWaitingWriterLock() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWaitingWriterLock()
		 {
			  RagManager ragManager = new RagManager();
			  LockResource resource = new LockResource( ResourceTypes.NODE, 1L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lock = createRWLock(ragManager, resource);
			  RWLock @lock = CreateRWLock( ragManager, resource );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction lockTransaction = new LockTransaction();
			  LockTransaction lockTransaction = new LockTransaction();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction anotherTransaction = new LockTransaction();
			  LockTransaction anotherTransaction = new LockTransaction();

			  @lock.Mark();
			  @lock.AcquireReadLock( LockTracer.NONE, lockTransaction );
			  @lock.Mark();
			  @lock.AcquireReadLock( LockTracer.NONE, anotherTransaction );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch writerCompletedLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent writerCompletedLatch = new System.Threading.CountdownEvent( 1 );

			  ThreadStart writer = CreateWriter( @lock, lockTransaction, writerCompletedLatch );

			  // start writer that will be placed in a wait list
			  _executor.execute( writer );

			  // wait till writer will be added into a list of waiters
			  WaitWaitingThreads( @lock, 1 );

			  assertEquals( "No writers for now.", 0, @lock.WriteCount );
			  assertEquals( 2, @lock.ReadCount );

			  // releasing read locks that will allow writer to grab the lock
			  @lock.ReleaseReadLock( lockTransaction );
			  @lock.ReleaseReadLock( anotherTransaction );

			  // wait till writer will have write lock
			  writerCompletedLatch.await();

			  assertEquals( 1, @lock.WriteCount );
			  assertEquals( 0, @lock.ReadCount );

			  // now releasing write lock
			  @lock.ReleaseWriteLock( lockTransaction );

			  assertEquals( "Lock should not have any writers left.", 0, @lock.WriteCount );
			  assertEquals( "No waiting threads left.", 0, @lock.WaitingThreadsCount );
			  assertEquals( "No lock elements left.", 0, @lock.TxLockElementCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT_MILLIS) public void testWaitingReaderLock() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWaitingReaderLock()
		 {
			  RagManager ragManager = new RagManager();
			  LockResource resource = new LockResource( ResourceTypes.NODE, 1L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lock = createRWLock(ragManager, resource);
			  RWLock @lock = CreateRWLock( ragManager, resource );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction transaction = new LockTransaction();
			  LockTransaction transaction = new LockTransaction();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction readerTransaction = new LockTransaction();
			  LockTransaction readerTransaction = new LockTransaction();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch readerCompletedLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent readerCompletedLatch = new System.Threading.CountdownEvent( 1 );

			  @lock.Mark();
			  @lock.AcquireWriteLock( LockTracer.NONE, transaction );

			  ThreadStart reader = CreateReader( @lock, readerTransaction, readerCompletedLatch );

			  // start reader that should wait for writer to release write lock
			  _executor.execute( reader );

			  WaitWaitingThreads( @lock, 1 );

			  assertEquals( 1, @lock.WriteCount );
			  assertEquals( "No readers for now", 0, @lock.ReadCount );

			  @lock.ReleaseWriteLock( transaction );

			  // wait till reader finish lock grab
			  readerCompletedLatch.await();

			  assertEquals( 0, @lock.WriteCount );
			  assertEquals( 1, @lock.ReadCount );

			  @lock.ReleaseReadLock( readerTransaction );

			  assertEquals( "Lock should not have any readers left.", 0, @lock.ReadCount );
			  assertEquals( "No waiting threads left.", 0, @lock.WaitingThreadsCount );
			  assertEquals( "No lock elements left.", 0, @lock.TxLockElementCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT_MILLIS) public void testThreadRemovedFromWaitingListOnDeadlock() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestThreadRemovedFromWaitingListOnDeadlock()
		 {
			  RagManager ragManager = Mockito.mock( typeof( RagManager ) );
			  LockResource resource = new LockResource( ResourceTypes.NODE, 1L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lock = createRWLock(ragManager, resource);
			  RWLock @lock = CreateRWLock( ragManager, resource );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction lockTransaction = new LockTransaction();
			  LockTransaction lockTransaction = new LockTransaction();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction anotherTransaction = new LockTransaction();
			  LockTransaction anotherTransaction = new LockTransaction();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch exceptionLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent exceptionLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch completionLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent completionLatch = new System.Threading.CountdownEvent( 1 );

			  Mockito.doNothing().doAnswer(invocation =>
			  {
				exceptionLatch.Signal();
				throw new DeadlockDetectedException( "Deadlock" );
			  }).when( ragManager ).checkWaitOn( @lock, lockTransaction );

			  @lock.Mark();
			  @lock.Mark();
			  @lock.AcquireReadLock( LockTracer.NONE, lockTransaction );
			  @lock.AcquireReadLock( LockTracer.NONE, anotherTransaction );

			  // writer will be added to a waiting list
			  // then spurious wake up will be simulated
			  // and deadlock will be detected
			  ThreadStart writer = () =>
			  {
				try
				{
					 @lock.Mark();
					 @lock.AcquireWriteLock( LockTracer.NONE, lockTransaction );
				}
				catch ( DeadlockDetectedException )
				{
					 // ignored
				}
				completionLatch.Signal();
			  };
			  _executor.execute( writer );

			  WaitWaitingThreads( @lock, 1 );

			  // sending notify for all threads till our writer will not cause deadlock exception
			  do
			  {
					//noinspection SynchronizationOnLocalVariableOrMethodParameter
					lock ( @lock )
					{
						 Monitor.PulseAll( @lock );
					}
			  } while ( exceptionLatch.CurrentCount == 1 );

			  // waiting for writer to finish
			  completionLatch.await();

			  assertEquals( "In case of deadlock caused by spurious wake up " + "thread should be removed from waiting list", 0, @lock.WaitingThreadsCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLockCounters() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestLockCounters()
		 {
			  RagManager ragManager = new RagManager();
			  LockResource resource = new LockResource( ResourceTypes.NODE, 1L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lock = createRWLock(ragManager, resource);
			  RWLock @lock = CreateRWLock( ragManager, resource );
			  LockTransaction lockTransaction = new LockTransaction();
			  LockTransaction anotherTransaction = new LockTransaction();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction writeTransaction = new LockTransaction();
			  LockTransaction writeTransaction = new LockTransaction();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch writerCompletedLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent writerCompletedLatch = new System.Threading.CountdownEvent( 1 );

			  @lock.Mark();
			  @lock.AcquireReadLock( LockTracer.NONE, lockTransaction );
			  @lock.Mark();
			  @lock.AcquireReadLock( LockTracer.NONE, anotherTransaction );

			  assertEquals( 2, @lock.ReadCount );
			  assertEquals( 0, @lock.WriteCount );
			  assertEquals( 2, @lock.TxLockElementCount );

			  ThreadStart writer = CreateWriter( @lock, writeTransaction, writerCompletedLatch );

			  _executor.submit( writer );

			  WaitWaitingThreads( @lock, 1 );

			  // check that all reader, writes, threads counters are correct
			  assertEquals( 2, @lock.ReadCount );
			  assertEquals( 0, @lock.WriteCount );
			  assertEquals( 3, @lock.TxLockElementCount );
			  assertEquals( 1, @lock.WaitingThreadsCount );

			  @lock.ReleaseReadLock( lockTransaction );
			  @lock.ReleaseReadLock( anotherTransaction );
			  writerCompletedLatch.await();

			  // test readers and waiting thread gone
			  assertEquals( 0, @lock.ReadCount );
			  assertEquals( 1, @lock.WriteCount );
			  assertEquals( 1, @lock.TxLockElementCount );
			  assertEquals( 0, @lock.WaitingThreadsCount );

			  @lock.ReleaseWriteLock( writeTransaction );

			  // check lock is clean in the end
			  assertEquals( 0, @lock.TxLockElementCount );
			  assertEquals( 0, @lock.WaitingThreadsCount );
			  assertEquals( 0, @lock.ReadCount );
			  assertEquals( 0, @lock.WriteCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT_MILLIS) public void testDeadlockDetection() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestDeadlockDetection()
		 {
			  RagManager ragManager = new RagManager();
			  LockResource node1 = new LockResource( ResourceTypes.NODE, 1L );
			  LockResource node2 = new LockResource( ResourceTypes.NODE, 2L );
			  LockResource node3 = new LockResource( ResourceTypes.NODE, 3L );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lockNode1 = createRWLock(ragManager, node1);
			  RWLock lockNode1 = CreateRWLock( ragManager, node1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lockNode2 = createRWLock(ragManager, node2);
			  RWLock lockNode2 = CreateRWLock( ragManager, node2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lockNode3 = createRWLock(ragManager, node3);
			  RWLock lockNode3 = CreateRWLock( ragManager, node3 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction client1Transaction = new LockTransaction();
			  LockTransaction client1Transaction = new LockTransaction();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction client2Transaction = new LockTransaction();
			  LockTransaction client2Transaction = new LockTransaction();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction client3Transaction = new LockTransaction();
			  LockTransaction client3Transaction = new LockTransaction();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch deadLockDetector = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent deadLockDetector = new System.Threading.CountdownEvent( 1 );

			  lockNode1.Mark();
			  lockNode1.AcquireWriteLock( LockTracer.NONE, client1Transaction );
			  lockNode2.Mark();
			  lockNode2.AcquireWriteLock( LockTracer.NONE, client2Transaction );
			  lockNode3.Mark();
			  lockNode3.AcquireWriteLock( LockTracer.NONE, client3Transaction );

			  ThreadStart readerLockNode2 = CreateReaderForDeadlock( lockNode3, client1Transaction, deadLockDetector );
			  ThreadStart readerLockNode3 = CreateReaderForDeadlock( lockNode1, client2Transaction, deadLockDetector );
			  ThreadStart readerLockNode1 = CreateReaderForDeadlock( lockNode2, client3Transaction, deadLockDetector );
			  _executor.execute( readerLockNode2 );
			  _executor.execute( readerLockNode3 );
			  _executor.execute( readerLockNode1 );

			  // Deadlock should occur
			  assertTrue( "Deadlock was detected as expected.", deadLockDetector.await( TEST_TIMEOUT_MILLIS, TimeUnit.MILLISECONDS ) );

			  lockNode3.ReleaseWriteLock( client3Transaction );
			  lockNode2.ReleaseWriteLock( client2Transaction );
			  lockNode1.ReleaseWriteLock( client1Transaction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT_MILLIS) public void testLockRequestsTermination() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestLockRequestsTermination()
		 {
			  // given
			  RagManager ragManager = new RagManager();
			  LockResource node1 = new LockResource( ResourceTypes.NODE, 1L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RWLock lock = createRWLock(ragManager, node1);
			  RWLock @lock = CreateRWLock( ragManager, node1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction mainTransaction = new LockTransaction();
			  LockTransaction mainTransaction = new LockTransaction();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction writerTransaction = new LockTransaction();
			  LockTransaction writerTransaction = new LockTransaction();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch writerCompletedLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent writerCompletedLatch = new System.Threading.CountdownEvent( 1 );
			  ThreadStart conflictingWriter = CreateFailedWriter( @lock, writerTransaction, writerCompletedLatch );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LockTransaction readerTransaction = new LockTransaction();
			  LockTransaction readerTransaction = new LockTransaction();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch readerCompletedLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent readerCompletedLatch = new System.Threading.CountdownEvent( 1 );
			  ThreadStart reader = CreateFailedReader( @lock, readerTransaction, readerCompletedLatch );

			  // when
			  @lock.Mark();
			  assertTrue( @lock.AcquireWriteLock( LockTracer.NONE, mainTransaction ) );
			  _executor.submit( reader );
			  _executor.submit( conflictingWriter );

			  // wait waiters to come
			  WaitWaitingThreads( @lock, 2 );
			  assertEquals( 3, @lock.TxLockElementCount );

			  // when
			  @lock.TerminateLockRequestsForLockTransaction( readerTransaction );
			  @lock.TerminateLockRequestsForLockTransaction( writerTransaction );

			  readerCompletedLatch.await();
			  writerCompletedLatch.await();

			  // expect only main write lock counters and elements present
			  // all the rest should be cleaned up
			  assertEquals( 0, @lock.WaitingThreadsCount );
			  assertEquals( 0, @lock.ReadCount );
			  assertEquals( 1, @lock.WriteCount );
			  assertEquals( 1, @lock.TxLockElementCount );

		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable createReader(final RWLock lock, final LockTransaction transaction, final java.util.concurrent.CountDownLatch latch)
		 private ThreadStart CreateReader( RWLock @lock, LockTransaction transaction, System.Threading.CountdownEvent latch )
		 {
			  return () =>
			  {
				@lock.Mark();
				@lock.AcquireReadLock( LockTracer.NONE, transaction );
				latch.Signal();
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable createFailedReader(final RWLock lock, final LockTransaction transaction, final java.util.concurrent.CountDownLatch latch)
		 private ThreadStart CreateFailedReader( RWLock @lock, LockTransaction transaction, System.Threading.CountdownEvent latch )
		 {
			  return () =>
			  {
				@lock.Mark();
				assertFalse( @lock.AcquireReadLock( LockTracer.NONE, transaction ) );
				latch.Signal();
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable createWriter(final RWLock lock, final LockTransaction transaction, final java.util.concurrent.CountDownLatch latch)
		 private ThreadStart CreateWriter( RWLock @lock, LockTransaction transaction, System.Threading.CountdownEvent latch )
		 {
			  return () =>
			  {
				@lock.Mark();
				@lock.AcquireWriteLock( LockTracer.NONE, transaction );
				latch.Signal();
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable createFailedWriter(final RWLock lock, final LockTransaction transaction, final java.util.concurrent.CountDownLatch latch)
		 private ThreadStart CreateFailedWriter( RWLock @lock, LockTransaction transaction, System.Threading.CountdownEvent latch )
		 {
			  return () =>
			  {
				@lock.Mark();
				assertFalse( @lock.AcquireWriteLock( LockTracer.NONE, transaction ) );
				latch.Signal();
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable createReaderForDeadlock(final RWLock node, final LockTransaction transaction, final java.util.concurrent.CountDownLatch latch)
		 private ThreadStart CreateReaderForDeadlock( RWLock node, LockTransaction transaction, System.Threading.CountdownEvent latch )
		 {
			  return () =>
			  {
				try
				{
					 node.Mark();
					 node.AcquireReadLock( LockTracer.NONE, transaction );
				}
				catch ( DeadlockDetectedException )
				{
					 latch.Signal();
				}
			  };
		 }

		 private RWLock CreateRWLock( RagManager ragManager, LockResource resource )
		 {
			  return new RWLock( resource, ragManager, Clocks.systemClock(), 0 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitWaitingThreads(RWLock lock, int expectedThreads) throws InterruptedException
		 private void WaitWaitingThreads( RWLock @lock, int expectedThreads )
		 {
			  while ( @lock.WaitingThreadsCount != expectedThreads )
			  {
					Thread.Sleep( 20 );
			  }
		 }
	}

}