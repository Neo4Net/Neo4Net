using System;
using System.Collections.Generic;
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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using MemoryAllocator = Neo4Net.Io.mem.MemoryAllocator;
	using DummyPageSwapper = Neo4Net.Io.pagecache.tracing.DummyPageSwapper;
	using EvictionEvent = Neo4Net.Io.pagecache.tracing.EvictionEvent;
	using EvictionRunEvent = Neo4Net.Io.pagecache.tracing.EvictionRunEvent;
	using FlushEvent = Neo4Net.Io.pagecache.tracing.FlushEvent;
	using FlushEventOpportunity = Neo4Net.Io.pagecache.tracing.FlushEventOpportunity;
	using PageFaultEvent = Neo4Net.Io.pagecache.tracing.PageFaultEvent;
	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;
	using DaemonThreadFactory = Neo4Net.Scheduler.DaemonThreadFactory;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class PageListTest
	public class PageListTest
	{
		 private const long TIMEOUT = 5000;
		 private const int ALIGNMENT = 8;

		 private static readonly int[] _pageIds = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		 private static readonly DummyPageSwapper _dummySwapper = new DummyPageSwapper( "", UnsafeUtil.pageSize() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "pageRef = {0}") public static Iterable<Object[]> parameters()
		 public static IEnumerable<object[]> Parameters()
		 {
			  System.Func<int, object[]> toArray = x => new object[]{ x };
			  return () => Arrays.stream(_pageIds).mapToObj(toArray).GetEnumerator();
		 }

		 private static ExecutorService _executor;
		 private static MemoryAllocator _mman;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpStatics()
		 public static void SetUpStatics()
		 {
			  _executor = Executors.newCachedThreadPool( new DaemonThreadFactory() );
			  _mman = MemoryAllocator.createAllocator( "1 MiB", GlobalMemoryTracker.INSTANCE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownStatics()
		 public static void TearDownStatics()
		 {
			  _mman.close();
			  _mman = null;
			  _executor.shutdown();
			  _executor = null;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private readonly int _pageId;
		 private readonly int _prevPageId;
		 private readonly int _nextPageId;
		 private long _pageRef;
		 private long _prevPageRef;
		 private long _nextPageRef;
		 private readonly int _pageSize;
		 private SwapperSet _swappers;
		 private PageList _pageList;

		 public PageListTest( int pageId )
		 {
			  this._pageId = pageId;
			  this._prevPageId = pageId == 0 ? _pageIds.Length - 1 : ( pageId - 1 ) % _pageIds.Length;
			  this._nextPageId = ( pageId + 1 ) % _pageIds.Length;
			  _pageSize = UnsafeUtil.pageSize();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _swappers = new SwapperSet();
			  long victimPage = VictimPageReference.GetVictimPage( _pageSize, GlobalMemoryTracker.INSTANCE );
			  _pageList = new PageList( _pageIds.Length, _pageSize, _mman, _swappers, victimPage, ALIGNMENT );
			  _pageRef = _pageList.deref( _pageId );
			  _prevPageRef = _pageList.deref( _prevPageId );
			  _nextPageRef = _pageList.deref( _nextPageId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustExposePageCount()
		 public virtual void MustExposePageCount()
		 {
			  int pageCount;
			  long victimPage = VictimPageReference.GetVictimPage( _pageSize, GlobalMemoryTracker.INSTANCE );

			  pageCount = 3;
			  assertThat( ( new PageList( pageCount, _pageSize, _mman, _swappers, victimPage, ALIGNMENT ) ).PageCount, @is( pageCount ) );

			  pageCount = 42;
			  assertThat( ( new PageList( pageCount, _pageSize, _mman, _swappers, victimPage, ALIGNMENT ) ).PageCount, @is( pageCount ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToReversePageRedToPageId()
		 public virtual void MustBeAbleToReversePageRedToPageId()
		 {
			  assertThat( _pageList.toId( _pageRef ), @is( _pageId ) );
		 }

		 // xxx ---[ Sequence lock tests ]---

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pagesAreInitiallyExclusivelyLocked()
		 public virtual void PagesAreInitiallyExclusivelyLocked()
		 {
			  assertTrue( _pageList.isExclusivelyLocked( _pageRef ) );
			  _pageList.unlockExclusive( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedOptimisticLockMustValidate()
		 public virtual void UncontendedOptimisticLockMustValidate()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long stamp = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, stamp ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotValidateRandomStamp()
		 public virtual void MustNotValidateRandomStamp()
		 {
			  assertFalse( _pageList.validateReadLock( _pageRef, 4242 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeLockMustInvalidateOptimisticReadLock()
		 public virtual void WriteLockMustInvalidateOptimisticReadLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void takingWriteLockMustInvalidateOptimisticReadLock()
		 public virtual void TakingWriteLockMustInvalidateOptimisticReadLock()
		 {
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void optimisticReadLockMustNotValidateUnderWriteLock()
		 public virtual void OptimisticReadLockMustNotValidateUnderWriteLock()
		 {
			  _pageList.tryWriteLock( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeLockReleaseMustInvalidateOptimisticReadLock()
		 public virtual void WriteLockReleaseMustInvalidateOptimisticReadLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  _pageList.unlockWrite( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedWriteLockMustBeAvailable()
		 public virtual void UncontendedWriteLockMustBeAvailable()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedOptimisticReadLockMustValidateAfterWriteLockRelease()
		 public virtual void UncontendedOptimisticReadLockMustValidateAfterWriteLockRelease()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TIMEOUT) public void writeLocksMustNotBlockOtherWriteLocks()
		 public virtual void WriteLocksMustNotBlockOtherWriteLocks()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TIMEOUT) public void writeLocksMustNotBlockOtherWriteLocksInOtherThreads() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WriteLocksMustNotBlockOtherWriteLocksInOtherThreads()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  int threads = 10;
			  System.Threading.CountdownEvent end = new System.Threading.CountdownEvent( threads );
			  ThreadStart runnable = () =>
			  {
				assertTrue( _pageList.tryWriteLock( _pageRef ) );
				end.Signal();
			  };
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> futures = new java.util.ArrayList<>();
			  IList<Future<object>> futures = new List<Future<object>>();
			  for ( int i = 0; i < threads; i++ )
			  {
					futures.Add( _executor.submit( runnable ) );
			  }
			  end.await();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
			  foreach ( Future<object> future in futures )
			  {
					future.get();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalMonitorStateException.class) public void unmatchedUnlockWriteLockMustThrow()
		 public virtual void UnmatchedUnlockWriteLockMustThrow()
		 {
			  _pageList.unlockWrite( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalMonitorStateException.class, timeout = TIMEOUT) public void writeLockCountOverflowMustThrow()
		 public virtual void WriteLockCountOverflowMustThrow()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  //noinspection InfiniteLoopStatement
			  for ( ; ; )
			  {
					assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustInvalidateOptimisticLock()
		 public virtual void ExclusiveLockMustInvalidateOptimisticLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  _pageList.tryExclusiveLock( _pageRef );
			  _pageList.unlockExclusive( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void takingExclusiveLockMustInvalidateOptimisticLock()
		 public virtual void TakingExclusiveLockMustInvalidateOptimisticLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  _pageList.tryExclusiveLock( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void optimisticReadLockMustNotValidateUnderExclusiveLock()
		 public virtual void OptimisticReadLockMustNotValidateUnderExclusiveLock()
		 {
			  // exclusive lock implied by constructor
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockReleaseMustInvalidateOptimisticReadLock()
		 public virtual void ExclusiveLockReleaseMustInvalidateOptimisticReadLock()
		 {
			  // exclusive lock implied by constructor
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  _pageList.unlockExclusive( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedOptimisticReadLockMustValidateAfterExclusiveLockRelease()
		 public virtual void UncontendedOptimisticReadLockMustValidateAfterExclusiveLockRelease()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryExclusiveLock( _pageRef );
			  _pageList.unlockExclusive( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canTakeUncontendedExclusiveLocks()
		 public virtual void CanTakeUncontendedExclusiveLocks()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeLocksMustFailExclusiveLocks()
		 public virtual void WriteLocksMustFailExclusiveLocks()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentWriteLocksMustFailExclusiveLocks()
		 public virtual void ConcurrentWriteLocksMustFailExclusiveLocks()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustBeAvailableAfterWriteLock()
		 public virtual void ExclusiveLockMustBeAvailableAfterWriteLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cannotTakeExclusiveLockIfAlreadyTaken()
		 public virtual void CannotTakeExclusiveLockIfAlreadyTaken()
		 {
			  // existing exclusive lock implied by constructor
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustBeAvailableAfterExclusiveLock()
		 public virtual void ExclusiveLockMustBeAvailableAfterExclusiveLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TIMEOUT) public void exclusiveLockMustFailWriteLocks()
		 public virtual void ExclusiveLockMustFailWriteLocks()
		 {
			  // exclusive lock implied by constructor
			  assertFalse( _pageList.tryWriteLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalMonitorStateException.class) public void unmatchedUnlockExclusiveLockMustThrow()
		 public virtual void UnmatchedUnlockExclusiveLockMustThrow()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalMonitorStateException.class) public void unmatchedUnlockWriteAfterTakingExclusiveLockMustThrow()
		 public virtual void UnmatchedUnlockWriteAfterTakingExclusiveLockMustThrow()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryExclusiveLock( _pageRef );
			  _pageList.unlockWrite( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TIMEOUT) public void writeLockMustBeAvailableAfterExclusiveLock()
		 public virtual void WriteLockMustBeAvailableAfterExclusiveLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryExclusiveLock( _pageRef );
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockExclusiveMustReturnStampForOptimisticReadLock()
		 public virtual void UnlockExclusiveMustReturnStampForOptimisticReadLock()
		 {
			  // exclusive lock implied by constructor
			  long r = _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockExclusiveAndTakeWriteLockMustInvalidateOptimisticReadLocks()
		 public virtual void UnlockExclusiveAndTakeWriteLockMustInvalidateOptimisticReadLocks()
		 {
			  // exclusive lock implied by constructor
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockExclusiveAndTakeWriteLockMustPreventExclusiveLocks()
		 public virtual void UnlockExclusiveAndTakeWriteLockMustPreventExclusiveLocks()
		 {
			  // exclusive lock implied by constructor
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TIMEOUT) public void unlockExclusiveAndTakeWriteLockMustAllowConcurrentWriteLocks()
		 public virtual void UnlockExclusiveAndTakeWriteLockMustAllowConcurrentWriteLocks()
		 {
			  // exclusive lock implied by constructor
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TIMEOUT) public void unlockExclusiveAndTakeWriteLockMustBeAtomic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnlockExclusiveAndTakeWriteLockMustBeAtomic()
		 {
			  // exclusive lock implied by constructor
			  int threads = Runtime.Runtime.availableProcessors() - 1;
			  System.Threading.CountdownEvent start = new System.Threading.CountdownEvent( threads );
			  AtomicBoolean stop = new AtomicBoolean();
			  _pageList.tryExclusiveLock( _pageRef );
			  ThreadStart runnable = () =>
			  {
				while ( !stop.get() )
				{
					 if ( _pageList.tryExclusiveLock( _pageRef ) )
					 {
						  _pageList.unlockExclusive( _pageRef );
						  throw new Exception( "I should not have gotten that lock" );
					 }
					 start.Signal();
				}
			  };

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> futures = new java.util.ArrayList<>();
			  IList<Future<object>> futures = new List<Future<object>>();
			  for ( int i = 0; i < threads; i++ )
			  {
					futures.Add( _executor.submit( runnable ) );
			  }

			  start.await();
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  stop.set( true );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
			  foreach ( Future<object> future in futures )
			  {
					future.get(); // Assert that this does not throw
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stampFromUnlockExclusiveMustNotBeValidIfThereAreWriteLocks()
		 public virtual void StampFromUnlockExclusiveMustNotBeValidIfThereAreWriteLocks()
		 {
			  // exclusive lock implied by constructor
			  long r = _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  assertFalse( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedFlushLockMustBeAvailable()
		 public virtual void UncontendedFlushLockMustBeAvailable()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryFlushLock( _pageRef ) != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushLockMustNotInvalidateOptimisticReadLock()
		 public virtual void FlushLockMustNotInvalidateOptimisticReadLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushLockMustNotFailWriteLock()
		 public virtual void FlushLockMustNotFailWriteLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryFlushLock( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushLockMustFailExclusiveLock()
		 public virtual void FlushLockMustFailExclusiveLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryFlushLock( _pageRef );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cannotTakeFlushLockIfAlreadyTaken()
		 public virtual void CannotTakeFlushLockIfAlreadyTaken()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryFlushLock( _pageRef ) != 0 );
			  assertFalse( _pageList.tryFlushLock( _pageRef ) != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeLockMustNotFailFlushLock()
		 public virtual void WriteLockMustNotFailFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  assertTrue( _pageList.tryFlushLock( _pageRef ) != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustFailFlushLock()
		 public virtual void ExclusiveLockMustFailFlushLock()
		 {
			  // exclusively locked from constructor
			  assertFalse( _pageList.tryFlushLock( _pageRef ) != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockExclusiveAndTakeWriteLockMustNotFailFlushLock()
		 public virtual void UnlockExclusiveAndTakeWriteLockMustNotFailFlushLock()
		 {
			  // exclusively locked from constructor
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertTrue( _pageList.tryFlushLock( _pageRef ) != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushUnlockMustNotInvalidateOptimisticReadLock()
		 public virtual void FlushUnlockMustNotInvalidateOptimisticReadLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.tryFlushLock( _pageRef ) != 0 );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void optimisticReadLockMustValidateUnderFlushLock()
		 public virtual void OptimisticReadLockMustValidateUnderFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryFlushLock( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushLockReleaseMustNotInvalidateOptimisticReadLock()
		 public virtual void FlushLockReleaseMustNotInvalidateOptimisticReadLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long s = _pageList.tryFlushLock( _pageRef );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalMonitorStateException.class) public void unmatchedUnlockFlushMustThrow()
		 public virtual void UnmatchedUnlockFlushMustThrow()
		 {
			  _pageList.unlockFlush( _pageRef, _pageList.tryOptimisticReadLock( _pageRef ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedOptimisticReadLockMustBeAvailableAfterFlushLock()
		 public virtual void UncontendedOptimisticReadLockMustBeAvailableAfterFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedWriteLockMustBeAvailableAfterFlushLock()
		 public virtual void UncontendedWriteLockMustBeAvailableAfterFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedExclusiveLockMustBeAvailableAfterFlushLock()
		 public virtual void UncontendedExclusiveLockMustBeAvailableAfterFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedFlushLockMustBeAvailableAfterWriteLock()
		 public virtual void UncontendedFlushLockMustBeAvailableAfterWriteLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef );
			  assertTrue( _pageList.tryFlushLock( _pageRef ) != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedFlushLockMustBeAvailableAfterExclusiveLock()
		 public virtual void UncontendedFlushLockMustBeAvailableAfterExclusiveLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryExclusiveLock( _pageRef );
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryFlushLock( _pageRef ) != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uncontendedFlushLockMustBeAvailableAfterFlushLock()
		 public virtual void UncontendedFlushLockMustBeAvailableAfterFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.tryFlushLock( _pageRef ) != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stampFromUnlockExclusiveMustBeValidUnderFlushLock()
		 public virtual void StampFromUnlockExclusiveMustBeValidUnderFlushLock()
		 {
			  // exclusively locked from constructor
			  long r = _pageList.unlockExclusive( _pageRef );
			  _pageList.tryFlushLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void optimisticReadLockMustNotGetInterferenceFromAdjacentWriteLocks()
		 public virtual void OptimisticReadLockMustNotGetInterferenceFromAdjacentWriteLocks()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryWriteLock( _prevPageRef ) );
			  assertTrue( _pageList.tryWriteLock( _nextPageRef ) );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
			  _pageList.unlockWrite( _prevPageRef );
			  _pageList.unlockWrite( _nextPageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void optimisticReadLockMustNotGetInterferenceFromAdjacentExclusiveLocks()
		 public virtual void OptimisticReadLockMustNotGetInterferenceFromAdjacentExclusiveLocks()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryExclusiveLock( _prevPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _nextPageRef ) );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void optimisticReadLockMustNotGetInterferenceFromAdjacentExclusiveAndWriteLocks()
		 public virtual void OptimisticReadLockMustNotGetInterferenceFromAdjacentExclusiveAndWriteLocks()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryExclusiveLock( _prevPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _nextPageRef ) );
			  long r = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
			  _pageList.unlockExclusiveAndTakeWriteLock( _prevPageRef );
			  _pageList.unlockExclusiveAndTakeWriteLock( _nextPageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
			  _pageList.unlockWrite( _prevPageRef );
			  _pageList.unlockWrite( _nextPageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, r ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeLockMustNotGetInterferenceFromAdjacentExclusiveLocks()
		 public virtual void WriteLockMustNotGetInterferenceFromAdjacentExclusiveLocks()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryExclusiveLock( _prevPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _nextPageRef ) );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _nextPageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushLockMustNotGetInterferenceFromAdjacentExclusiveLocks()
		 public virtual void FlushLockMustNotGetInterferenceFromAdjacentExclusiveLocks()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  long s = 0;
			  assertTrue( _pageList.tryExclusiveLock( _prevPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _nextPageRef ) );
			  assertTrue( ( s = _pageList.tryFlushLock( _pageRef ) ) != 0 );
			  _pageList.unlockFlush( _pageRef, s, true );
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _nextPageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void flushLockMustNotGetInterferenceFromAdjacentFlushLocks()
		 public virtual void FlushLockMustNotGetInterferenceFromAdjacentFlushLocks()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  long ps = 0;
			  long ns = 0;
			  long s = 0;
			  assertTrue( ( ps = _pageList.tryFlushLock( _prevPageRef ) ) != 0 );
			  assertTrue( ( ns = _pageList.tryFlushLock( _nextPageRef ) ) != 0 );
			  assertTrue( ( s = _pageList.tryFlushLock( _pageRef ) ) != 0 );
			  _pageList.unlockFlush( _pageRef, s, true );
			  _pageList.unlockFlush( _prevPageRef, ps, true );
			  _pageList.unlockFlush( _nextPageRef, ns, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustNotGetInterferenceFromAdjacentExclusiveLocks()
		 public virtual void ExclusiveLockMustNotGetInterferenceFromAdjacentExclusiveLocks()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryExclusiveLock( _prevPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _nextPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryExclusiveLock( _prevPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _nextPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustNotGetInterferenceFromAdjacentWriteLocks()
		 public virtual void ExclusiveLockMustNotGetInterferenceFromAdjacentWriteLocks()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryWriteLock( _prevPageRef ) );
			  assertTrue( _pageList.tryWriteLock( _nextPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockWrite( _prevPageRef );
			  _pageList.unlockWrite( _nextPageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustNotGetInterferenceFromAdjacentExclusiveAndWriteLocks()
		 public virtual void ExclusiveLockMustNotGetInterferenceFromAdjacentExclusiveAndWriteLocks()
		 {
			  // exclusive locks on prevPageRef, nextPageRef and pageRef are implied from constructor
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusiveAndTakeWriteLock( _prevPageRef );
			  _pageList.unlockExclusiveAndTakeWriteLock( _nextPageRef );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockWrite( _prevPageRef );
			  _pageList.unlockWrite( _nextPageRef );

			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _prevPageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _nextPageRef ) );
			  _pageList.unlockExclusiveAndTakeWriteLock( _prevPageRef );
			  _pageList.unlockExclusiveAndTakeWriteLock( _nextPageRef );
			  _pageList.unlockWrite( _prevPageRef );
			  _pageList.unlockWrite( _nextPageRef );
			  _pageList.unlockExclusive( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustNotGetInterferenceFromAdjacentFlushLocks()
		 public virtual void ExclusiveLockMustNotGetInterferenceFromAdjacentFlushLocks()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  long ps = 0;
			  long ns = 0;
			  assertTrue( ( ps = _pageList.tryFlushLock( _prevPageRef ) ) != 0 );
			  assertTrue( ( ns = _pageList.tryFlushLock( _nextPageRef ) ) != 0 );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockFlush( _prevPageRef, ps, true );
			  _pageList.unlockFlush( _nextPageRef, ns, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void takingWriteLockMustRaiseModifiedFlag()
		 public virtual void TakingWriteLockMustRaiseModifiedFlag()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertFalse( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void turningExclusiveLockIntoWriteLockMustRaiseModifiedFlag()
		 public virtual void TurningExclusiveLockIntoWriteLockMustRaiseModifiedFlag()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertFalse( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  assertFalse( _pageList.isModified( _pageRef ) );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releasingFlushLockMustLowerModifiedFlagIfSuccessful()
		 public virtual void ReleasingFlushLockMustLowerModifiedFlagIfSuccessful()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertFalse( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void loweredModifiedFlagMustRemainLoweredAfterReleasingFlushLock()
		 public virtual void LoweredModifiedFlagMustRemainLoweredAfterReleasingFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertFalse( _pageList.isModified( _pageRef ) );

			  s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertFalse( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releasingFlushLockMustNotLowerModifiedFlagIfUnsuccessful()
		 public virtual void ReleasingFlushLockMustNotLowerModifiedFlagIfUnsuccessful()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, false );
			  assertTrue( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releasingFlushLockMustNotLowerModifiedFlagIfWriteLockWasWithinFlushFlushLock()
		 public virtual void ReleasingFlushLockMustNotLowerModifiedFlagIfWriteLockWasWithinFlushFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long s = _pageList.tryFlushLock( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releasingFlushLockMustNotLowerModifiedFlagIfWriteLockOverlappedTakingFlushLock()
		 public virtual void ReleasingFlushLockMustNotLowerModifiedFlagIfWriteLockOverlappedTakingFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockWrite( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releasingFlushLockMustNotLowerModifiedFlagIfWriteLockOverlappedReleasingFlushLock()
		 public virtual void ReleasingFlushLockMustNotLowerModifiedFlagIfWriteLockOverlappedReleasingFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long s = _pageList.tryFlushLock( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releasingFlushLockMustNotLowerModifiedFlagIfWriteLockOverlappedFlushLock()
		 public virtual void ReleasingFlushLockMustNotLowerModifiedFlagIfWriteLockOverlappedFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releasingFlushLockMustNotInterfereWithAdjacentModifiedFlags()
		 public virtual void ReleasingFlushLockMustNotInterfereWithAdjacentModifiedFlags()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryWriteLock( _prevPageRef ) );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  assertTrue( _pageList.tryWriteLock( _nextPageRef ) );
			  _pageList.unlockWrite( _prevPageRef );
			  _pageList.unlockWrite( _pageRef );
			  _pageList.unlockWrite( _nextPageRef );
			  assertTrue( _pageList.isModified( _prevPageRef ) );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.isModified( _nextPageRef ) );
			  long s = _pageList.tryFlushLock( _pageRef );
			  _pageList.unlockFlush( _pageRef, s, true );
			  assertTrue( _pageList.isModified( _prevPageRef ) );
			  assertFalse( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.isModified( _nextPageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeLockMustNotInterfereWithAdjacentModifiedFlags()
		 public virtual void WriteLockMustNotInterfereWithAdjacentModifiedFlags()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  assertFalse( _pageList.isModified( _prevPageRef ) );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertFalse( _pageList.isModified( _nextPageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void disallowUnlockedPageToExplicitlyLowerModifiedFlag()
		 public virtual void DisallowUnlockedPageToExplicitlyLowerModifiedFlag()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.explicitlyMarkPageUnmodifiedUnderExclusiveLock( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void disallowReadLockedPageToExplicitlyLowerModifiedFlag()
		 public virtual void DisallowReadLockedPageToExplicitlyLowerModifiedFlag()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryOptimisticReadLock( _pageRef );
			  _pageList.explicitlyMarkPageUnmodifiedUnderExclusiveLock( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void disallowFlushLockedPageToExplicitlyLowerModifiedFlag()
		 public virtual void DisallowFlushLockedPageToExplicitlyLowerModifiedFlag()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertThat( _pageList.tryFlushLock( _pageRef ), @is( not( 0L ) ) );
			  _pageList.explicitlyMarkPageUnmodifiedUnderExclusiveLock( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void disallowWriteLockedPageToExplicitlyLowerModifiedFlag()
		 public virtual void DisallowWriteLockedPageToExplicitlyLowerModifiedFlag()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.explicitlyMarkPageUnmodifiedUnderExclusiveLock( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allowExclusiveLockedPageToExplicitlyLowerModifiedFlag()
		 public virtual void AllowExclusiveLockedPageToExplicitlyLowerModifiedFlag()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertFalse( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  _pageList.unlockWrite( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  _pageList.explicitlyMarkPageUnmodifiedUnderExclusiveLock( _pageRef );
			  assertFalse( _pageList.isModified( _pageRef ) );
			  _pageList.unlockExclusive( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockMustTakeFlushLock()
		 public virtual void UnlockWriteAndTryTakeFlushLockMustTakeFlushLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) );
			  long flushStamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertThat( flushStamp, @is( not( 0L ) ) );
			  assertThat( _pageList.tryFlushLock( _pageRef ), @is( 0L ) );
			  _pageList.unlockFlush( _pageRef, flushStamp, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalMonitorStateException.class) public void unlockWriteAndTryTakeFlushLockMustThrowIfNotWriteLocked()
		 public virtual void UnlockWriteAndTryTakeFlushLockMustThrowIfNotWriteLocked()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalMonitorStateException.class) public void unlockWriteAndTryTakeFlushLockMustThrowIfNotWriteLockedButExclusiveLocked()
		 public virtual void UnlockWriteAndTryTakeFlushLockMustThrowIfNotWriteLockedButExclusiveLocked()
		 {
			  // exclusive lock implied by constructor
			  _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockMustFailIfFlushLockIsAlreadyTaken()
		 public virtual void UnlockWriteAndTryTakeFlushLockMustFailIfFlushLockIsAlreadyTaken()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long stamp = _pageList.tryFlushLock( _pageRef );
			  assertThat( stamp, @is( not( 0L ) ) );
			  long secondStamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertThat( secondStamp, @is( 0L ) );
			  _pageList.unlockFlush( _pageRef, stamp, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockMustReleaseWriteLockEvenIfFlushLockFails()
		 public virtual void UnlockWriteAndTryTakeFlushLockMustReleaseWriteLockEvenIfFlushLockFails()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long flushStamp = _pageList.tryFlushLock( _pageRef );
			  assertThat( flushStamp, @is( not( 0L ) ) );
			  assertThat( _pageList.unlockWriteAndTryTakeFlushLock( _pageRef ), @is( 0L ) );
			  long readStamp = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, readStamp ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockMustReleaseWriteLockWhenFlushLockSucceeds()
		 public virtual void UnlockWriteAndTryTakeFlushLockMustReleaseWriteLockWhenFlushLockSucceeds()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertThat( _pageList.unlockWriteAndTryTakeFlushLock( _pageRef ), @is( not( 0L ) ) );
			  long readStamp = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, readStamp ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTrueTakeFlushLockMustRaiseModifiedFlag()
		 public virtual void UnlockWriteAndTrueTakeFlushLockMustRaiseModifiedFlag()
		 {
			  assertFalse( _pageList.isModified( _pageRef ) );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertThat( _pageList.unlockWriteAndTryTakeFlushLock( _pageRef ), @is( not( 0L ) ) );
			  assertTrue( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockAndThenUnlockFlushMustLowerModifiedFlagIfSuccessful()
		 public virtual void UnlockWriteAndTryTakeFlushLockAndThenUnlockFlushMustLowerModifiedFlagIfSuccessful()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  _pageList.unlockFlush( _pageRef, stamp, true );
			  assertFalse( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockAndThenUnlockFlushMustNotLowerModifiedFlagIfFailed()
		 public virtual void UnlockWriteAndTryTakeFlushLockAndThenUnlockFlushMustNotLowerModifiedFlagIfFailed()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  _pageList.unlockFlush( _pageRef, stamp, false );
			  assertTrue( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockWithOverlappingWriterAndThenUnlockFlushMustNotLowerModifiedFlag()
		 public virtual void UnlockWriteAndTryTakeFlushLockWithOverlappingWriterAndThenUnlockFlushMustNotLowerModifiedFlag()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) ); // two write locks, now
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef ); // one flush, one write lock
			  assertThat( stamp, @is( not( 0L ) ) );
			  _pageList.unlockWrite( _pageRef ); // one flush, zero write locks
			  assertTrue( _pageList.isModified( _pageRef ) );
			  _pageList.unlockFlush( _pageRef, stamp, true ); // flush is successful, but had one overlapping writer
			  assertTrue( _pageList.isModified( _pageRef ) ); // so it's still modified
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockAndThenUnlockFlushWithOverlappingWriterMustNotLowerModifiedFlag()
		 public virtual void UnlockWriteAndTryTakeFlushLockAndThenUnlockFlushWithOverlappingWriterMustNotLowerModifiedFlag()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef ); // one flush lock
			  assertThat( stamp, @is( not( 0L ) ) );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) ); // one flush and one write lock
			  _pageList.unlockFlush( _pageRef, stamp, true ); // flush is successful, but have one overlapping writer
			  _pageList.unlockWrite( _pageRef ); // no more locks, but a writer started within flush section ...
			  assertTrue( _pageList.isModified( _pageRef ) ); // ... and overlapped unlockFlush, so it's still modified
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockAndThenUnlockFlushWithContainedWriterMustNotLowerModifiedFlag()
		 public virtual void UnlockWriteAndTryTakeFlushLockAndThenUnlockFlushWithContainedWriterMustNotLowerModifiedFlag()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef ); // one flush lock
			  assertThat( stamp, @is( not( 0L ) ) );
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryWriteLock( _pageRef ) ); // one flush and one write lock
			  _pageList.unlockWrite( _pageRef ); // back to one flush lock
			  _pageList.unlockFlush( _pageRef, stamp, true ); // flush is successful, but had one overlapping writer
			  assertTrue( _pageList.isModified( _pageRef ) ); // so it's still modified
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockThatSucceedsMustPreventOverlappingExclusiveLock()
		 public virtual void UnlockWriteAndTryTakeFlushLockThatSucceedsMustPreventOverlappingExclusiveLock()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.unlockFlush( _pageRef, stamp, true );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockThatFailsMustPreventOverlappingExclusiveLock()
		 public virtual void UnlockWriteAndTryTakeFlushLockThatFailsMustPreventOverlappingExclusiveLock()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertFalse( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.unlockFlush( _pageRef, stamp, false );
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockThatSucceedsMustPreventOverlappingFlushLock()
		 public virtual void UnlockWriteAndTryTakeFlushLockThatSucceedsMustPreventOverlappingFlushLock()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertThat( _pageList.tryFlushLock( _pageRef ), @is( 0L ) );
			  _pageList.unlockFlush( _pageRef, stamp, true );
			  assertThat( _pageList.tryFlushLock( _pageRef ), @is( not( 0L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockThatFailsMustPreventOverlappingFlushLock()
		 public virtual void UnlockWriteAndTryTakeFlushLockThatFailsMustPreventOverlappingFlushLock()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long stamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertThat( _pageList.tryFlushLock( _pageRef ), @is( 0L ) );
			  _pageList.unlockFlush( _pageRef, stamp, false );
			  assertThat( _pageList.tryFlushLock( _pageRef ), @is( not( 0L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockMustNotInvalidateReadersOverlappingWithFlushLock()
		 public virtual void UnlockWriteAndTryTakeFlushLockMustNotInvalidateReadersOverlappingWithFlushLock()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long flushStamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  long readStamp = _pageList.tryOptimisticReadLock( _pageRef );
			  assertTrue( _pageList.validateReadLock( _pageRef, readStamp ) );
			  _pageList.unlockFlush( _pageRef, flushStamp, true );
			  assertTrue( _pageList.validateReadLock( _pageRef, readStamp ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlockWriteAndTryTakeFlushLockMustInvalidateReadersOverlappingWithWriteLock()
		 public virtual void UnlockWriteAndTryTakeFlushLockMustInvalidateReadersOverlappingWithWriteLock()
		 {
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  long readStamp = _pageList.tryOptimisticReadLock( _pageRef );
			  long flushStamp = _pageList.unlockWriteAndTryTakeFlushLock( _pageRef );
			  assertFalse( _pageList.validateReadLock( _pageRef, readStamp ) );
			  _pageList.unlockFlush( _pageRef, flushStamp, true );
			  assertFalse( _pageList.validateReadLock( _pageRef, readStamp ) );
		 }

		 // xxx ---[ Page state tests ]---

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustExposeCachePageSize()
		 public virtual void MustExposeCachePageSize()
		 {
			  PageList list = new PageList( 0, 42, _mman, _swappers, VictimPageReference.GetVictimPage( 42, GlobalMemoryTracker.INSTANCE ), ALIGNMENT );
			  assertThat( list.CachePageSize, @is( 42 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addressesMustBeZeroBeforeInitialisation()
		 public virtual void AddressesMustBeZeroBeforeInitialisation()
		 {
			  assertThat( _pageList.getAddress( _pageRef ), @is( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void initialisingBufferMustConsumeMemoryFromMemoryManager()
		 public virtual void InitialisingBufferMustConsumeMemoryFromMemoryManager()
		 {
			  long initialUsedMemory = _mman.usedMemory();
			  _pageList.initBuffer( _pageRef );
			  long resultingUsedMemory = _mman.usedMemory();
			  int allocatedMemory = ( int )( resultingUsedMemory - initialUsedMemory );
			  assertThat( allocatedMemory, greaterThanOrEqualTo( _pageSize ) );
			  assertThat( allocatedMemory, lessThanOrEqualTo( _pageSize + ALIGNMENT ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addressMustNotBeZeroAfterInitialisation()
		 public virtual void AddressMustNotBeZeroAfterInitialisation()
		 {
			  _pageList.initBuffer( _pageRef );
			  assertThat( _pageList.getAddress( _pageRef ), @is( not( equalTo( 0L ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageListMustBeCopyableViaConstructor()
		 public virtual void PageListMustBeCopyableViaConstructor()
		 {
			  assertThat( _pageList.getAddress( _pageRef ), @is( equalTo( 0L ) ) );
			  PageList pl = new PageList( _pageList );
			  assertThat( pl.GetAddress( _pageRef ), @is( equalTo( 0L ) ) );

			  _pageList.initBuffer( _pageRef );
			  assertThat( _pageList.getAddress( _pageRef ), @is( not( equalTo( 0L ) ) ) );
			  assertThat( pl.GetAddress( _pageRef ), @is( not( equalTo( 0L ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void usageCounterMustBeZeroByDefault()
		 public virtual void UsageCounterMustBeZeroByDefault()
		 {
			  assertTrue( _pageList.decrementUsage( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void usageCounterMustGoUpToFour()
		 public virtual void UsageCounterMustGoUpToFour()
		 {
			  _pageList.incrementUsage( _pageRef );
			  _pageList.incrementUsage( _pageRef );
			  _pageList.incrementUsage( _pageRef );
			  _pageList.incrementUsage( _pageRef );
			  assertFalse( _pageList.decrementUsage( _pageRef ) );
			  assertFalse( _pageList.decrementUsage( _pageRef ) );
			  assertFalse( _pageList.decrementUsage( _pageRef ) );
			  assertTrue( _pageList.decrementUsage( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void usageCounterMustTruncateAtFour()
		 public virtual void UsageCounterMustTruncateAtFour()
		 {
			  _pageList.incrementUsage( _pageRef );
			  _pageList.incrementUsage( _pageRef );
			  _pageList.incrementUsage( _pageRef );
			  _pageList.incrementUsage( _pageRef );
			  _pageList.incrementUsage( _pageRef );
			  assertFalse( _pageList.decrementUsage( _pageRef ) );
			  assertFalse( _pageList.decrementUsage( _pageRef ) );
			  assertFalse( _pageList.decrementUsage( _pageRef ) );
			  assertTrue( _pageList.decrementUsage( _pageRef ) );
			  assertTrue( _pageList.decrementUsage( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementingUsageCounterMustNotInterfereWithAdjacentUsageCounters()
		 public virtual void IncrementingUsageCounterMustNotInterfereWithAdjacentUsageCounters()
		 {
			  _pageList.incrementUsage( _pageRef );
			  _pageList.incrementUsage( _pageRef );
			  assertTrue( _pageList.decrementUsage( _prevPageRef ) );
			  assertTrue( _pageList.decrementUsage( _nextPageRef ) );
			  assertFalse( _pageList.decrementUsage( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void decrementingUsageCounterMustNotInterfereWithAdjacentUsageCounters()
		 public virtual void DecrementingUsageCounterMustNotInterfereWithAdjacentUsageCounters()
		 {
			  foreach ( int id in _pageIds )
			  {
					long @ref = _pageList.deref( id );
					_pageList.incrementUsage( @ref );
					_pageList.incrementUsage( @ref );
			  }

			  assertFalse( _pageList.decrementUsage( _pageRef ) );
			  assertTrue( _pageList.decrementUsage( _pageRef ) );
			  assertFalse( _pageList.decrementUsage( _prevPageRef ) );
			  assertFalse( _pageList.decrementUsage( _nextPageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void filePageIdIsUnboundByDefault()
		 public virtual void FilePageIdIsUnboundByDefault()
		 {
			  assertThat( _pageList.getFilePageId( _pageRef ), @is( PageCursor.UNBOUND_PAGE_ID ) );
		 }

		 // xxx ---[ Page fault tests ]---

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void faultMustThrowWithoutExclusiveLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FaultMustThrowWithoutExclusiveLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.initBuffer( _pageRef );
			  Exception.expect( typeof( System.InvalidOperationException ) );
			  _pageList.fault( _pageRef, _dummySwapper, ( short ) 0, 0, PageFaultEvent.NULL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void faultMustThrowIfSwapperIsNull() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FaultMustThrowIfSwapperIsNull()
		 {
			  // exclusive lock implied by the constructor
			  _pageList.initBuffer( _pageRef );
			  Exception.expect( typeof( System.ArgumentException ) );
			  _pageList.fault( _pageRef, null, ( short ) 0, 0, PageFaultEvent.NULL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void faultMustThrowIfFilePageIdIsUnbound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FaultMustThrowIfFilePageIdIsUnbound()
		 {
			  // exclusively locked from constructor
			  _pageList.initBuffer( _pageRef );
			  Exception.expect( typeof( System.InvalidOperationException ) );
			  _pageList.fault( _pageRef, _dummySwapper, ( short ) 0, PageCursor.UNBOUND_PAGE_ID, PageFaultEvent.NULL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void faultMustReadIntoPage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FaultMustReadIntoPage()
		 {
			  sbyte pageByteContents = unchecked( ( sbyte ) 0xF7 );
			  short swapperId = 1;
			  long filePageId = 2;
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass( this, _pageSize, pageByteContents, filePageId );
			  _pageList.initBuffer( _pageRef );
			  _pageList.fault( _pageRef, swapper, swapperId, filePageId, PageFaultEvent.NULL );

			  long address = _pageList.getAddress( _pageRef );
			  assertThat( address, @is( not( 0L ) ) );
			  for ( int i = 0; i < _pageSize; i++ )
			  {
					sbyte actualByteContents = UnsafeUtil.getByte( address + i );
					if ( actualByteContents != pageByteContents )
					{
						 fail( string.Format( "Page contents where different at address {0:x} + {1}, expected {2:x} but was {3:x}", address, i, pageByteContents, actualByteContents ) );
					}
			  }
		 }

		 private class DummyPageSwapperAnonymousInnerClass : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private sbyte _pageByteContents;
			 private long _filePageId;

			 public DummyPageSwapperAnonymousInnerClass( PageListTest outerInstance, int pageSize, sbyte pageByteContents, long filePageId ) : base( "some file", pageSize )
			 {
				 this.outerInstance = outerInstance;
				 this._pageByteContents = pageByteContents;
				 this._filePageId = filePageId;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(long fpId, long bufferAddress, int bufferSize) throws java.io.IOException
			 public override long read( long fpId, long bufferAddress, int bufferSize )
			 {
				  if ( fpId == _filePageId )
				  {
						UnsafeUtil.setMemory( bufferAddress, bufferSize, _pageByteContents );
						return bufferSize;
				  }
				  throw new IOException( "Did not expect this file page id = " + fpId );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageMustBeLoadedAndBoundAfterFault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageMustBeLoadedAndBoundAfterFault()
		 {
			  // exclusive lock implied by constructor
			  int swapperId = 1;
			  long filePageId = 42;
			  _pageList.initBuffer( _pageRef );
			  _pageList.fault( _pageRef, _dummySwapper, swapperId, filePageId, PageFaultEvent.NULL );
			  assertThat( _pageList.getFilePageId( _pageRef ), @is( filePageId ) );
			  assertThat( _pageList.getSwapperId( _pageRef ), @is( swapperId ) );
			  assertTrue( _pageList.isLoaded( _pageRef ) );
			  assertTrue( _pageList.isBoundTo( _pageRef, swapperId, filePageId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageWith5BytesFilePageIdMustBeLoadedAndBoundAfterFault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageWith5BytesFilePageIdMustBeLoadedAndBoundAfterFault()
		 {
			  // exclusive lock implied by constructor
			  int swapperId = 12;
			  long filePageId = int.MaxValue + 1L;
			  _pageList.initBuffer( _pageRef );
			  _pageList.fault( _pageRef, _dummySwapper, swapperId, filePageId, PageFaultEvent.NULL );
			  assertThat( _pageList.getFilePageId( _pageRef ), @is( filePageId ) );
			  assertThat( _pageList.getSwapperId( _pageRef ), @is( swapperId ) );
			  assertTrue( _pageList.isLoaded( _pageRef ) );
			  assertTrue( _pageList.isBoundTo( _pageRef, swapperId, filePageId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageMustBeLoadedAndNotBoundIfFaultThrows()
		 public virtual void PageMustBeLoadedAndNotBoundIfFaultThrows()
		 {
			  // exclusive lock implied by constructor
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass2( this, _pageSize );
			  int swapperId = 1;
			  long filePageId = 42;
			  _pageList.initBuffer( _pageRef );
			  try
			  {
					_pageList.fault( _pageRef, swapper, swapperId, filePageId, PageFaultEvent.NULL );
					fail();
			  }
			  catch ( IOException e )
			  {
					assertThat( e.Message, @is( "boo" ) );
			  }
			  assertThat( _pageList.getFilePageId( _pageRef ), @is( filePageId ) );
			  assertThat( _pageList.getSwapperId( _pageRef ), @is( 0 ) ); // 0 means not bound
			  assertTrue( _pageList.isLoaded( _pageRef ) );
			  assertFalse( _pageList.isBoundTo( _pageRef, swapperId, filePageId ) );
		 }

		 private class DummyPageSwapperAnonymousInnerClass2 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 public DummyPageSwapperAnonymousInnerClass2( PageListTest outerInstance, int pageSize ) : base( "file", pageSize )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(long filePageId, long bufferAddress, int bufferSize) throws java.io.IOException
			 public override long read( long filePageId, long bufferAddress, int bufferSize )
			 {
				  throw new IOException( "boo" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void faultMustThrowIfPageIsAlreadyBound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FaultMustThrowIfPageIsAlreadyBound()
		 {
			  // exclusive lock implied by constructor
			  short swapperId = 1;
			  long filePageId = 42;
			  _pageList.initBuffer( _pageRef );
			  _pageList.fault( _pageRef, _dummySwapper, swapperId, filePageId, PageFaultEvent.NULL );

			  Exception.expect( typeof( System.InvalidOperationException ) );
			  _pageList.fault( _pageRef, _dummySwapper, swapperId, filePageId, PageFaultEvent.NULL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void faultMustThrowIfPageIsLoadedButNotBound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FaultMustThrowIfPageIsLoadedButNotBound()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  short swapperId = 1;
			  long filePageId = 42;
			  DoFailedFault( swapperId, filePageId );

			  // After the failed page fault, the page is loaded but not bound.
			  // We still can't fault into a loaded page, though.
			  Exception.expect( typeof( System.InvalidOperationException ) );
			  _pageList.fault( _pageRef, _dummySwapper, swapperId, filePageId, PageFaultEvent.NULL );
		 }

		 private void DoFailedFault( short swapperId, long filePageId )
		 {
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.initBuffer( _pageRef );
			  DummyPageSwapper swapper = new DummyPageSwapperAnonymousInnerClass3( this, _pageSize, filePageId );
			  try
			  {
					_pageList.fault( _pageRef, swapper, swapperId, filePageId, PageFaultEvent.NULL );
					fail( "fault should have thrown" );
			  }
			  catch ( IOException e )
			  {
					assertThat( e.Message, @is( "boom" ) );
			  }
		 }

		 private class DummyPageSwapperAnonymousInnerClass3 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private long _filePageId;

			 public DummyPageSwapperAnonymousInnerClass3( PageListTest outerInstance, int pageSize, long filePageId ) : base( "", pageSize )
			 {
				 this.outerInstance = outerInstance;
				 this._filePageId = filePageId;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(long filePageId, long bufferAddress, int bufferSize) throws java.io.IOException
			 public override long read( long filePageId, long bufferAddress, int bufferSize )
			 {
				  throw new IOException( "boom" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void faultMustPopulatePageFaultEvent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FaultMustPopulatePageFaultEvent()
		 {
			  // exclusive lock implied by constructor
			  short swapperId = 1;
			  long filePageId = 42;
			  _pageList.initBuffer( _pageRef );
			  DummyPageSwapper swapper = new DummyPageSwapperAnonymousInnerClass4( this, _pageSize, filePageId );
			  StubPageFaultEvent @event = new StubPageFaultEvent();
			  _pageList.fault( _pageRef, swapper, swapperId, filePageId, @event );
			  assertThat( @event.BytesRead, @is( 333L ) );
			  assertThat( @event.CachePageIdConflict, @is( not( 0 ) ) );
		 }

		 private class DummyPageSwapperAnonymousInnerClass4 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private long _filePageId;

			 public DummyPageSwapperAnonymousInnerClass4( PageListTest outerInstance, int pageSize, long filePageId ) : base( "", pageSize )
			 {
				 this.outerInstance = outerInstance;
				 this._filePageId = filePageId;
			 }

			 public override long read( long filePageId, long bufferAddress, int bufferSize )
			 {
				  return 333;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unboundPageMustNotBeLoaded()
		 public virtual void UnboundPageMustNotBeLoaded()
		 {
			  assertFalse( _pageList.isLoaded( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unboundPageMustNotBeBoundToAnything()
		 public virtual void UnboundPageMustNotBeBoundToAnything()
		 {
			  assertFalse( _pageList.isBoundTo( _pageRef, ( short ) 0, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void boundPagesAreNotBoundToOtherPagesWithSameSwapper() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BoundPagesAreNotBoundToOtherPagesWithSameSwapper()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  long filePageId = 42;
			  short swapperId = 2;
			  DoFault( swapperId, filePageId );

			  assertTrue( _pageList.isBoundTo( _pageRef, swapperId, filePageId ) );
			  assertFalse( _pageList.isBoundTo( _pageRef, swapperId, filePageId + 1 ) );
			  assertFalse( _pageList.isBoundTo( _pageRef, swapperId, filePageId - 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void boundPagesAreNotBoundToOtherPagesWithSameFilePageId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BoundPagesAreNotBoundToOtherPagesWithSameFilePageId()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  short swapperId = 2;
			  DoFault( swapperId, 42 );

			  assertTrue( _pageList.isBoundTo( _pageRef, swapperId, 42 ) );
			  assertFalse( _pageList.isBoundTo( _pageRef, ( short )( swapperId + 1 ), 42 ) );
			  assertFalse( _pageList.isBoundTo( _pageRef, ( short )( swapperId - 1 ), 42 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void faultMustNotInterfereWithAdjacentPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FaultMustNotInterfereWithAdjacentPages()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  DoFault( ( short ) 1, 42 );

			  assertFalse( _pageList.isLoaded( _prevPageRef ) );
			  assertFalse( _pageList.isLoaded( _nextPageRef ) );
			  assertFalse( _pageList.isBoundTo( _prevPageRef, ( short ) 1, 42 ) );
			  assertFalse( _pageList.isBoundTo( _prevPageRef, ( short ) 0, 0 ) );
			  assertFalse( _pageList.isBoundTo( _nextPageRef, ( short ) 1, 42 ) );
			  assertFalse( _pageList.isBoundTo( _nextPageRef, ( short ) 0, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedFaultMustNotInterfereWithAdjacentPages()
		 public virtual void FailedFaultMustNotInterfereWithAdjacentPages()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  DoFailedFault( ( short ) 1, 42 );

			  assertFalse( _pageList.isLoaded( _prevPageRef ) );
			  assertFalse( _pageList.isLoaded( _nextPageRef ) );
			  assertFalse( _pageList.isBoundTo( _prevPageRef, ( short ) 1, 42 ) );
			  assertFalse( _pageList.isBoundTo( _prevPageRef, ( short ) 0, 0 ) );
			  assertFalse( _pageList.isBoundTo( _nextPageRef, ( short ) 1, 42 ) );
			  assertFalse( _pageList.isBoundTo( _nextPageRef, ( short ) 0, 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockMustStillBeHeldAfterFault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExclusiveLockMustStillBeHeldAfterFault()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  DoFault( ( short ) 1, 42 );
			  _pageList.unlockExclusive( _pageRef ); // will throw if lock is not held
		 }

		 // xxx ---[ Page eviction tests ]---

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustFailIfPageIsAlreadyExclusivelyLocked() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustFailIfPageIsAlreadyExclusivelyLocked()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  int swapperId = _swappers.allocate( _dummySwapper );
			  DoFault( swapperId, 42 ); // page is now loaded
			  // pages are delivered from the fault routine with the exclusive lock already held!
			  assertFalse( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictThatFailsOnExclusiveLockMustNotUndoSaidLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictThatFailsOnExclusiveLockMustNotUndoSaidLock()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  int swapperId = _swappers.allocate( _dummySwapper );
			  DoFault( swapperId, 42 ); // page is now loaded
			  // pages are delivered from the fault routine with the exclusive lock already held!
			  _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ); // This attempt will fail
			  assertTrue( _pageList.isExclusivelyLocked( _pageRef ) ); // page should still have its lock
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustFailIfPageIsNotLoaded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustFailIfPageIsNotLoaded()
		 {
			  assertFalse( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustWhenPageIsNotLoadedMustNotLeavePageLocked() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustWhenPageIsNotLoadedMustNotLeavePageLocked()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ); // This attempt fails
			  assertFalse( _pageList.isExclusivelyLocked( _pageRef ) ); // Page should not be left in locked state
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustLeavePageExclusivelyLockedOnSuccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustLeavePageExclusivelyLockedOnSuccess()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  int swapperId = _swappers.allocate( _dummySwapper );
			  DoFault( swapperId, 42 ); // page now bound & exclusively locked
			  _pageList.unlockExclusive( _pageRef ); // no longer exclusively locked; can now be evicted
			  assertTrue( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
			  _pageList.unlockExclusive( _pageRef ); // will throw if lock is not held
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageMustNotBeLoadedAfterSuccessfulEviction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageMustNotBeLoadedAfterSuccessfulEviction()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  int swapperId = _swappers.allocate( _dummySwapper );
			  DoFault( swapperId, 42 ); // page now bound & exclusively locked
			  _pageList.unlockExclusive( _pageRef ); // no longer exclusively locked; can now be evicted
			  assertTrue( _pageList.isLoaded( _pageRef ) );
			  _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL );
			  assertFalse( _pageList.isLoaded( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageMustNotBeBoundAfterSuccessfulEviction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageMustNotBeBoundAfterSuccessfulEviction()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  int swapperId = _swappers.allocate( _dummySwapper );
			  DoFault( swapperId, 42 ); // page now bound & exclusively locked
			  _pageList.unlockExclusive( _pageRef ); // no longer exclusively locked; can now be evicted
			  assertTrue( _pageList.isBoundTo( _pageRef, ( short ) 1, 42 ) );
			  assertTrue( _pageList.isLoaded( _pageRef ) );
			  assertThat( _pageList.getSwapperId( _pageRef ), @is( 1 ) );
			  _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL );
			  assertFalse( _pageList.isBoundTo( _pageRef, ( short ) 1, 42 ) );
			  assertFalse( _pageList.isLoaded( _pageRef ) );
			  assertThat( _pageList.getSwapperId( _pageRef ), @is( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageMustNotBeModifiedAfterSuccessfulEviction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageMustNotBeModifiedAfterSuccessfulEviction()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  int swapperId = _swappers.allocate( _dummySwapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
			  assertFalse( _pageList.isModified( _pageRef ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustFlushPageIfModified() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustFlushPageIfModified()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  AtomicLong writtenFilePageId = new AtomicLong( -1 );
			  AtomicLong writtenBufferAddress = new AtomicLong( -1 );
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass5( this, _pageSize, writtenFilePageId, writtenBufferAddress );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
			  assertThat( writtenFilePageId.get(), @is(42L) );
			  assertThat( writtenBufferAddress.get(), @is(_pageList.getAddress(_pageRef)) );
		 }

		 private class DummyPageSwapperAnonymousInnerClass5 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private AtomicLong _writtenFilePageId;
			 private AtomicLong _writtenBufferAddress;

			 public DummyPageSwapperAnonymousInnerClass5( PageListTest outerInstance, int pageSize, AtomicLong writtenFilePageId, AtomicLong writtenBufferAddress ) : base( "file", pageSize )
			 {
				 this.outerInstance = outerInstance;
				 this._writtenFilePageId = writtenFilePageId;
				 this._writtenBufferAddress = writtenBufferAddress;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
			 public override long write( long filePageId, long bufferAddress )
			 {
				  assertTrue( _writtenFilePageId.compareAndSet( -1, filePageId ) );
				  assertTrue( _writtenBufferAddress.compareAndSet( -1, bufferAddress ) );
				  return base.write( filePageId, bufferAddress );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustNotFlushPageIfNotModified() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustNotFlushPageIfNotModified()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  AtomicInteger writes = new AtomicInteger();
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass6( this, writes );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusive( _pageRef ); // we take no write lock, so page is not modified
			  assertFalse( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
			  assertThat( writes.get(), @is(0) );
		 }

		 private class DummyPageSwapperAnonymousInnerClass6 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private AtomicInteger _writes;

			 public DummyPageSwapperAnonymousInnerClass6( PageListTest outerInstance, AtomicInteger writes ) : base( "a", 313 )
			 {
				 this.outerInstance = outerInstance;
				 this._writes = writes;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
			 public override long write( long filePageId, long bufferAddress )
			 {
				  _writes.AndIncrement;
				  return base.write( filePageId, bufferAddress );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustNotifySwapperOnSuccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustNotifySwapperOnSuccess()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  AtomicBoolean evictionNotified = new AtomicBoolean();
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass7( this, evictionNotified );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
			  assertTrue( evictionNotified.get() );
		 }

		 private class DummyPageSwapperAnonymousInnerClass7 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private AtomicBoolean _evictionNotified;

			 public DummyPageSwapperAnonymousInnerClass7( PageListTest outerInstance, AtomicBoolean evictionNotified ) : base( "a", 313 )
			 {
				 this.outerInstance = outerInstance;
				 this._evictionNotified = evictionNotified;
			 }

			 public override void evicted( long filePageId )
			 {
				  _evictionNotified.set( true );
				  assertThat( filePageId, @is( 42L ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustNotifySwapperOnSuccessEvenWhenFlushing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustNotifySwapperOnSuccessEvenWhenFlushing()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  AtomicBoolean evictionNotified = new AtomicBoolean();
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass8( this, evictionNotified );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
			  assertTrue( evictionNotified.get() );
			  assertFalse( _pageList.isModified( _pageRef ) );
		 }

		 private class DummyPageSwapperAnonymousInnerClass8 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private AtomicBoolean _evictionNotified;

			 public DummyPageSwapperAnonymousInnerClass8( PageListTest outerInstance, AtomicBoolean evictionNotified ) : base( "a", 313 )
			 {
				 this.outerInstance = outerInstance;
				 this._evictionNotified = evictionNotified;
			 }

			 public override void evicted( long filePageId )
			 {
				  _evictionNotified.set( true );
				  assertThat( filePageId, @is( 42L ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustLeavePageUnlockedAndLoadedAndBoundAndModifiedIfFlushThrows() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustLeavePageUnlockedAndLoadedAndBoundAndModifiedIfFlushThrows()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass9( this );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  try
			  {
					_pageList.tryEvict( _pageRef, EvictionRunEvent.NULL );
					fail( "tryEvict should have thrown" );
			  }
			  catch ( IOException )
			  {
					// good
			  }
			  // there should be no lock preventing us from taking an exclusive lock
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  // page should still be loaded...
			  assertTrue( _pageList.isLoaded( _pageRef ) );
			  // ... and bound
			  assertTrue( _pageList.isBoundTo( _pageRef, swapperId, 42 ) );
			  // ... and modified
			  assertTrue( _pageList.isModified( _pageRef ) );
		 }

		 private class DummyPageSwapperAnonymousInnerClass9 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 public DummyPageSwapperAnonymousInnerClass9( PageListTest outerInstance ) : base( "a", 313 )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
			 public override long write( long filePageId, long bufferAddress )
			 {
				  throw new IOException();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustNotNotifySwapperOfEvictionIfFlushThrows() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustNotNotifySwapperOfEvictionIfFlushThrows()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  AtomicBoolean evictionNotified = new AtomicBoolean();
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass10( this, evictionNotified );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  try
			  {
					_pageList.tryEvict( _pageRef, EvictionRunEvent.NULL );
					fail( "tryEvict should have thrown" );
			  }
			  catch ( IOException )
			  {
					// good
			  }
			  // we should not have gotten any notification about eviction
			  assertFalse( evictionNotified.get() );
		 }

		 private class DummyPageSwapperAnonymousInnerClass10 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private AtomicBoolean _evictionNotified;

			 public DummyPageSwapperAnonymousInnerClass10( PageListTest outerInstance, AtomicBoolean evictionNotified ) : base( "a", 313 )
			 {
				 this.outerInstance = outerInstance;
				 this._evictionNotified = evictionNotified;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
			 public override long write( long filePageId, long bufferAddress )
			 {
				  throw new IOException();
			 }

			 public override void evicted( long filePageId )
			 {
				  _evictionNotified.set( true );
			 }
		 }

		 private class EvictionAndFlushRecorder : EvictionEvent, FlushEventOpportunity, FlushEvent
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long FilePageIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal PageSwapper SwapperConflict;
			  internal IOException EvictionException;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long CachePageIdConflict;
			  internal bool EvictionClosed;
			  internal long BytesWritten;
			  internal bool FlushDone;
			  internal IOException FlushException;
			  internal int PagesFlushed;

			  // --- EvictionEvent:

			  public override void Close()
			  {
					this.EvictionClosed = true;
			  }

			  public virtual long FilePageId
			  {
				  set
				  {
						this.FilePageIdConflict = value;
				  }
			  }

			  public virtual PageSwapper Swapper
			  {
				  set
				  {
						this.SwapperConflict = value;
				  }
			  }

			  public override FlushEventOpportunity FlushEventOpportunity()
			  {
					return this;
			  }

			  public override void ThrewException( IOException exception )
			  {
					this.EvictionException = exception;
			  }

			  public virtual long CachePageId
			  {
				  set
				  {
						this.CachePageIdConflict = value;
				  }
			  }

			  // --- FlushEventOpportunity:

			  public override FlushEvent BeginFlush( long filePageId, long cachePageId, PageSwapper swapper )
			  {
					return this;
			  }

			  // --- FlushEvent:

			  public override void AddBytesWritten( long bytes )
			  {
					this.BytesWritten += bytes;
			  }

			  public override void Done()
			  {
					this.FlushDone = true;
			  }

			  public override void Done( IOException exception )
			  {
					this.FlushDone = true;
					this.FlushException = exception;

			  }

			  public override void AddPagesFlushed( int pageCount )
			  {
					this.PagesFlushed += pageCount;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictMustReportToEvictionEvent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictMustReportToEvictionEvent()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  PageSwapper swapper = new DummyPageSwapper( "a", 313 );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusive( _pageRef );
			  EvictionAndFlushRecorder recorder = new EvictionAndFlushRecorder();
			  assertTrue( _pageList.tryEvict( _pageRef, () => recorder ) );
			  assertThat( recorder.EvictionClosed, @is( true ) );
			  assertThat( recorder.FilePageIdConflict, @is( 42L ) );
			  assertThat( recorder.SwapperConflict, sameInstance( swapper ) );
			  assertThat( recorder.EvictionException, @is( nullValue() ) );
			  assertThat( recorder.CachePageIdConflict, @is( _pageRef ) );
			  assertThat( recorder.BytesWritten, @is( 0L ) );
			  assertThat( recorder.FlushDone, @is( false ) );
			  assertThat( recorder.FlushException, @is( nullValue() ) );
			  assertThat( recorder.PagesFlushed, @is( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictThatFlushesMustReportToEvictionAndFlushEvents() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictThatFlushesMustReportToEvictionAndFlushEvents()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  int filePageSize = 313;
			  PageSwapper swapper = new DummyPageSwapper( "a", filePageSize );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  EvictionAndFlushRecorder recorder = new EvictionAndFlushRecorder();
			  assertTrue( _pageList.tryEvict( _pageRef, () => recorder ) );
			  assertThat( recorder.EvictionClosed, @is( true ) );
			  assertThat( recorder.FilePageIdConflict, @is( 42L ) );
			  assertThat( recorder.SwapperConflict, sameInstance( swapper ) );
			  assertThat( recorder.EvictionException, @is( nullValue() ) );
			  assertThat( recorder.CachePageIdConflict, @is( _pageRef ) );
			  assertThat( recorder.BytesWritten, @is( ( long ) filePageSize ) );
			  assertThat( recorder.FlushDone, @is( true ) );
			  assertThat( recorder.FlushException, @is( nullValue() ) );
			  assertThat( recorder.PagesFlushed, @is( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictThatFailsMustReportExceptionsToEvictionAndFlushEvents() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictThatFailsMustReportExceptionsToEvictionAndFlushEvents()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  IOException ioException = new IOException();
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass( this, ioException );
			  int swapperId = _swappers.allocate( swapper );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  EvictionAndFlushRecorder recorder = new EvictionAndFlushRecorder();
			  try
			  {
					_pageList.tryEvict( _pageRef, () => recorder );
					fail( "tryEvict should have thrown" );
			  }
			  catch ( IOException )
			  {
					// Ok
			  }
			  assertThat( recorder.EvictionClosed, @is( true ) );
			  assertThat( recorder.FilePageIdConflict, @is( 42L ) );
			  assertThat( recorder.SwapperConflict, sameInstance( swapper ) );
			  assertThat( recorder.EvictionException, sameInstance( ioException ) );
			  assertThat( recorder.CachePageIdConflict, @is( _pageRef ) );
			  assertThat( recorder.BytesWritten, @is( 0L ) );
			  assertThat( recorder.FlushDone, @is( true ) );
			  assertThat( recorder.FlushException, sameInstance( ioException ) );
			  assertThat( recorder.PagesFlushed, @is( 0 ) );
		 }

		 private class DummyPageSwapperAnonymousInnerClass : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 private IOException _ioException;

			 public DummyPageSwapperAnonymousInnerClass( PageListTest outerInstance, IOException ioException ) : base( "a", 313 )
			 {
				 this.outerInstance = outerInstance;
				 this._ioException = ioException;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
			 public override long write( long filePageId, long bufferAddress )
			 {
				  throw _ioException;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictThatSucceedsMustNotInterfereWithAdjacentPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictThatSucceedsMustNotInterfereWithAdjacentPages()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  PageSwapper swapper = new DummyPageSwapper( "a", 313 );
			  int swapperId = _swappers.allocate( swapper );
			  long prevStamp = _pageList.tryOptimisticReadLock( _prevPageRef );
			  long nextStamp = _pageList.tryOptimisticReadLock( _nextPageRef );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusive( _pageRef );
			  assertTrue( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
			  assertTrue( _pageList.validateReadLock( _prevPageRef, prevStamp ) );
			  assertTrue( _pageList.validateReadLock( _nextPageRef, nextStamp ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictThatFlushesAndSucceedsMustNotInterfereWithAdjacentPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictThatFlushesAndSucceedsMustNotInterfereWithAdjacentPages()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  PageSwapper swapper = new DummyPageSwapper( "a", 313 );
			  int swapperId = _swappers.allocate( swapper );
			  long prevStamp = _pageList.tryOptimisticReadLock( _prevPageRef );
			  long nextStamp = _pageList.tryOptimisticReadLock( _nextPageRef );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  assertTrue( _pageList.tryEvict( _pageRef, EvictionRunEvent.NULL ) );
			  assertTrue( _pageList.validateReadLock( _prevPageRef, prevStamp ) );
			  assertTrue( _pageList.validateReadLock( _nextPageRef, nextStamp ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryEvictThatFailsMustNotInterfereWithAdjacentPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryEvictThatFailsMustNotInterfereWithAdjacentPages()
		 {
			  _pageList.unlockExclusive( _prevPageRef );
			  _pageList.unlockExclusive( _pageRef );
			  _pageList.unlockExclusive( _nextPageRef );
			  PageSwapper swapper = new DummyPageSwapperAnonymousInnerClass2( this );
			  int swapperId = _swappers.allocate( swapper );
			  long prevStamp = _pageList.tryOptimisticReadLock( _prevPageRef );
			  long nextStamp = _pageList.tryOptimisticReadLock( _nextPageRef );
			  DoFault( swapperId, 42 );
			  _pageList.unlockExclusiveAndTakeWriteLock( _pageRef );
			  _pageList.unlockWrite( _pageRef ); // page is now modified
			  assertTrue( _pageList.isModified( _pageRef ) );
			  try
			  {
					_pageList.tryEvict( _pageRef, EvictionRunEvent.NULL );
					fail( "tryEvict should have thrown" );
			  }
			  catch ( IOException )
			  {
					// ok
			  }
			  assertTrue( _pageList.validateReadLock( _prevPageRef, prevStamp ) );
			  assertTrue( _pageList.validateReadLock( _nextPageRef, nextStamp ) );
		 }

		 private class DummyPageSwapperAnonymousInnerClass2 : DummyPageSwapper
		 {
			 private readonly PageListTest _outerInstance;

			 public DummyPageSwapperAnonymousInnerClass2( PageListTest outerInstance ) : base( "a", 313 )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
			 public override long write( long filePageId, long bufferAddress )
			 {
				  throw new IOException();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void failToSetHigherThenSupportedFilePageIdOnFault() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailToSetHigherThenSupportedFilePageIdOnFault()
		 {
			  _pageList.unlockExclusive( _pageRef );
			  short swapperId = 2;
			  DoFault( swapperId, long.MaxValue );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doFault(int swapperId, long filePageId) throws java.io.IOException
		 private void DoFault( int swapperId, long filePageId )
		 {
			  assertTrue( _pageList.tryExclusiveLock( _pageRef ) );
			  _pageList.initBuffer( _pageRef );
			  _pageList.fault( _pageRef, _dummySwapper, swapperId, filePageId, PageFaultEvent.NULL );
		 }

		 // todo freelist? (entries chained via file page ids in a linked list? should work as free pages are always
		 // todo exclusively locked, and thus don't really need an isLoaded check)
	}

}