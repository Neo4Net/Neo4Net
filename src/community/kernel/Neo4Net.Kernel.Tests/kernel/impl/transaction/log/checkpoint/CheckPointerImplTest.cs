using System;
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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{
	using Test = org.junit.Test;


	using Neo4Net.Functions;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using LogPruning = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruning;
	using CheckPointTracer = Neo4Net.Kernel.impl.transaction.tracing.CheckPointTracer;
	using LogCheckPointEvent = Neo4Net.Kernel.impl.transaction.tracing.LogCheckPointEvent;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.ThreadTestUtils.forkFuture;

	public class CheckPointerImplTest
	{
		 private static readonly SimpleTriggerInfo _info = new SimpleTriggerInfo( "Test" );

		 private readonly TransactionIdStore _txIdStore = mock( typeof( TransactionIdStore ) );
		 private readonly CheckPointThreshold _threshold = mock( typeof( CheckPointThreshold ) );
		 private readonly StorageEngine _storageEngine = mock( typeof( StorageEngine ) );
		 private readonly LogPruning _logPruning = mock( typeof( LogPruning ) );
		 private readonly TransactionAppender _appender = mock( typeof( TransactionAppender ) );
		 private readonly DatabaseHealth _health = mock( typeof( DatabaseHealth ) );
		 private readonly CheckPointTracer _tracer = mock( typeof( CheckPointTracer ), RETURNS_MOCKS );
		 private IOLimiter _limiter = mock( typeof( IOLimiter ) );

		 private readonly long _initialTransactionId = 2L;
		 private readonly long _transactionId = 42L;
		 private readonly LogPosition _logPosition = new LogPosition( 16L, 233L );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFlushIfItIsNotNeeded() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFlushIfItIsNotNeeded()
		 {
			  // Given
			  CheckPointerImpl checkPointing = CheckPointer();
			  when( _threshold.isCheckPointingNeeded( anyLong(), any(typeof(TriggerInfo)) ) ).thenReturn(false);

			  checkPointing.Start();

			  // When
			  long txId = checkPointing.CheckPointIfNeeded( _info );

			  // Then
			  assertEquals( -1, txId );
			  verifyZeroInteractions( _storageEngine );
			  verifyZeroInteractions( _tracer );
			  verifyZeroInteractions( _appender );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlushIfItIsNeeded() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFlushIfItIsNeeded()
		 {
			  // Given
			  CheckPointerImpl checkPointing = CheckPointer();
			  when( _threshold.isCheckPointingNeeded( anyLong(), eq(_info) ) ).thenReturn(true, false);
			  MockTxIdStore();

			  checkPointing.Start();

			  // When
			  long txId = checkPointing.CheckPointIfNeeded( _info );

			  // Then
			  assertEquals( _transactionId, txId );
			  verify( _storageEngine, times( 1 ) ).flushAndForce( _limiter );
			  verify( _health, times( 2 ) ).assertHealthy( typeof( IOException ) );
			  verify( _appender, times( 1 ) ).checkPoint( eq( _logPosition ), any( typeof( LogCheckPointEvent ) ) );
			  verify( _threshold, times( 1 ) ).initialize( _initialTransactionId );
			  verify( _threshold, times( 1 ) ).checkPointHappened( _transactionId );
			  verify( _threshold, times( 1 ) ).isCheckPointingNeeded( _transactionId, _info );
			  verify( _logPruning, times( 1 ) ).pruneLogs( _logPosition.LogVersion );
			  verify( _tracer, times( 1 ) ).beginCheckPoint();
			  verifyNoMoreInteractions( _storageEngine, _health, _appender, _threshold, _tracer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForceCheckPointAlways() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForceCheckPointAlways()
		 {
			  // Given
			  CheckPointerImpl checkPointing = CheckPointer();
			  when( _threshold.isCheckPointingNeeded( anyLong(), eq(_info) ) ).thenReturn(false);
			  MockTxIdStore();

			  checkPointing.Start();

			  // When
			  long txId = checkPointing.ForceCheckPoint( _info );

			  // Then
			  assertEquals( _transactionId, txId );
			  verify( _storageEngine, times( 1 ) ).flushAndForce( _limiter );
			  verify( _health, times( 2 ) ).assertHealthy( typeof( IOException ) );
			  verify( _appender, times( 1 ) ).checkPoint( eq( _logPosition ), any( typeof( LogCheckPointEvent ) ) );
			  verify( _threshold, times( 1 ) ).initialize( _initialTransactionId );
			  verify( _threshold, times( 1 ) ).checkPointHappened( _transactionId );
			  verify( _threshold, never() ).isCheckPointingNeeded(_transactionId, _info);
			  verify( _logPruning, times( 1 ) ).pruneLogs( _logPosition.LogVersion );
			  verify( _tracer, times( 1 ) ).beginCheckPoint();
			  verifyNoMoreInteractions( _storageEngine, _health, _appender, _threshold );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckPointAlwaysWhenThereIsNoRunningCheckPoint() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckPointAlwaysWhenThereIsNoRunningCheckPoint()
		 {
			  // Given
			  CheckPointerImpl checkPointing = CheckPointer();
			  when( _threshold.isCheckPointingNeeded( anyLong(), eq(_info) ) ).thenReturn(false);
			  MockTxIdStore();

			  checkPointing.Start();

			  // When
			  long txId = checkPointing.TryCheckPoint( _info );

			  // Then
			  assertEquals( _transactionId, txId );
			  verify( _storageEngine, times( 1 ) ).flushAndForce( _limiter );
			  verify( _health, times( 2 ) ).assertHealthy( typeof( IOException ) );
			  verify( _appender, times( 1 ) ).checkPoint( eq( _logPosition ), any( typeof( LogCheckPointEvent ) ) );
			  verify( _threshold, times( 1 ) ).initialize( _initialTransactionId );
			  verify( _threshold, times( 1 ) ).checkPointHappened( _transactionId );
			  verify( _threshold, never() ).isCheckPointingNeeded(_transactionId, _info);
			  verify( _logPruning, times( 1 ) ).pruneLogs( _logPosition.LogVersion );
			  verify( _tracer, times( 1 ) ).beginCheckPoint();
			  verifyNoMoreInteractions( _storageEngine, _health, _appender, _threshold );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void forceCheckPointShouldWaitTheCurrentCheckPointingToCompleteBeforeRunning() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ForceCheckPointShouldWaitTheCurrentCheckPointingToCompleteBeforeRunning()
		 {
			  // Given
			  Lock @lock = new ReentrantLock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.locks.Lock spyLock = spy(lock);
			  Lock spyLock = spy( @lock );

			  doAnswer(invocation =>
			  {
				verify( _appender ).checkPoint( any( typeof( LogPosition ) ), any( typeof( LogCheckPointEvent ) ) );
				reset( _appender );
				invocation.callRealMethod();
				return null;
			  }).when( spyLock ).unlock();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CheckPointerImpl checkPointing = checkPointer(mutex(spyLock));
			  CheckPointerImpl checkPointing = CheckPointer( Mutex( spyLock ) );
			  MockTxIdStore();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch startSignal = new java.util.concurrent.CountDownLatch(2);
			  System.Threading.CountdownEvent startSignal = new System.Threading.CountdownEvent( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch completed = new java.util.concurrent.CountDownLatch(2);
			  System.Threading.CountdownEvent completed = new System.Threading.CountdownEvent( 2 );

			  checkPointing.Start();

			  Thread checkPointerThread = new CheckPointerThread( checkPointing, startSignal, completed );

			  Thread forceCheckPointThread = new Thread(() =>
			  {
				try
				{
					 startSignal.Signal();
					 startSignal.await();
					 checkPointing.ForceCheckPoint( _info );

					 completed.Signal();
				}
				catch ( Exception e )
				{
					 throw new Exception( e );
				}
			  });

			  // when
			  checkPointerThread.Start();
			  forceCheckPointThread.Start();

			  completed.await();

			  verify( spyLock, times( 2 ) ).@lock();
			  verify( spyLock, times( 2 ) ).unlock();
		 }

		 private StoreCopyCheckPointMutex Mutex( Lock @lock )
		 {
			  return new StoreCopyCheckPointMutex( new ReadWriteLockAnonymousInnerClass( this, @lock ) );
		 }

		 private class ReadWriteLockAnonymousInnerClass : ReadWriteLock
		 {
			 private readonly CheckPointerImplTest _outerInstance;

			 private Lock @lock;

			 public ReadWriteLockAnonymousInnerClass( CheckPointerImplTest outerInstance, Lock @lock )
			 {
				 this.outerInstance = outerInstance;
				 this.@lock = @lock;
			 }

			 public override Lock writeLock()
			 {
				  return @lock;
			 }

			 public override Lock readLock()
			 {
				  throw new System.NotSupportedException();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryCheckPointShouldWaitTheCurrentCheckPointingToCompleteNoRunCheckPointButUseTheTxIdOfTheEarlierRun() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryCheckPointShouldWaitTheCurrentCheckPointingToCompleteNoRunCheckPointButUseTheTxIdOfTheEarlierRun()
		 {
			  // Given
			  Lock @lock = mock( typeof( Lock ) );
			  when( @lock.tryLock( anyLong(), any(typeof(TimeUnit)) ) ).thenReturn(true);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CheckPointerImpl checkPointing = checkPointer(mutex(lock));
			  CheckPointerImpl checkPointing = CheckPointer( Mutex( @lock ) );
			  MockTxIdStore();

			  checkPointing.ForceCheckPoint( _info );

			  verify( _appender ).checkPoint( eq( _logPosition ), any( typeof( LogCheckPointEvent ) ) );
			  reset( _appender );

			  checkPointing.TryCheckPoint( _info );

			  verifyNoMoreInteractions( _appender );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustUseIoLimiterFromFlushing() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustUseIoLimiterFromFlushing()
		 {
			  _limiter = new IOLimiterAnonymousInnerClass( this );
			  when( _threshold.isCheckPointingNeeded( anyLong(), eq(_info) ) ).thenReturn(true, false);
			  MockTxIdStore();
			  CheckPointerImpl checkPointing = CheckPointer();

			  checkPointing.Start();
			  checkPointing.CheckPointIfNeeded( _info );

			  verify( _storageEngine ).flushAndForce( _limiter );
		 }

		 private class IOLimiterAnonymousInnerClass : IOLimiter
		 {
			 private readonly CheckPointerImplTest _outerInstance;

			 public IOLimiterAnonymousInnerClass( CheckPointerImplTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public long maybeLimitIO( long previousStamp, int recentlyCompletedIOs, Flushable flushable )
			 {
				  return 42;
			 }

			 public bool Limited
			 {
				 get
				 {
					  return true;
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFlushAsFastAsPossibleDuringForceCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustFlushAsFastAsPossibleDuringForceCheckPoint()
		 {
			  AtomicBoolean doneDisablingLimits = new AtomicBoolean();
			  _limiter = new IOLimiterAnonymousInnerClass2( this, doneDisablingLimits );
			  MockTxIdStore();
			  CheckPointerImpl checkPointer = checkPointer();
			  checkPointer.ForceCheckPoint( new SimpleTriggerInfo( "test" ) );
			  assertTrue( doneDisablingLimits.get() );
		 }

		 private class IOLimiterAnonymousInnerClass2 : IOLimiter
		 {
			 private readonly CheckPointerImplTest _outerInstance;

			 private AtomicBoolean _doneDisablingLimits;

			 public IOLimiterAnonymousInnerClass2( CheckPointerImplTest outerInstance, AtomicBoolean doneDisablingLimits )
			 {
				 this.outerInstance = outerInstance;
				 this._doneDisablingLimits = doneDisablingLimits;
			 }

			 public long maybeLimitIO( long previousStamp, int recentlyCompletedIOs, Flushable flushable )
			 {
				  return 0;
			 }

			 public void enableLimit()
			 {
				  _doneDisablingLimits.set( true );
			 }

			 public bool Limited
			 {
				 get
				 {
					  return _doneDisablingLimits.get();
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFlushAsFastAsPossibleDuringTryCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustFlushAsFastAsPossibleDuringTryCheckPoint()
		 {

			  AtomicBoolean doneDisablingLimits = new AtomicBoolean();
			  _limiter = new IOLimiterAnonymousInnerClass3( this, doneDisablingLimits );
			  MockTxIdStore();
			  CheckPointerImpl checkPointer = checkPointer();
			  checkPointer.TryCheckPoint( _info );
			  assertTrue( doneDisablingLimits.get() );
		 }

		 private class IOLimiterAnonymousInnerClass3 : IOLimiter
		 {
			 private readonly CheckPointerImplTest _outerInstance;

			 private AtomicBoolean _doneDisablingLimits;

			 public IOLimiterAnonymousInnerClass3( CheckPointerImplTest outerInstance, AtomicBoolean doneDisablingLimits )
			 {
				 this.outerInstance = outerInstance;
				 this._doneDisablingLimits = doneDisablingLimits;
			 }

			 public long maybeLimitIO( long previousStamp, int recentlyCompletedIOs, Flushable flushable )
			 {
				  return 0;
			 }

			 public void enableLimit()
			 {
				  _doneDisablingLimits.set( true );
			 }

			 public bool Limited
			 {
				 get
				 {
					  return _doneDisablingLimits.get();
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryCheckPointMustWaitForOnGoingCheckPointsToCompleteAsLongAsTimeoutPredicateIsFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryCheckPointMustWaitForOnGoingCheckPointsToCompleteAsLongAsTimeoutPredicateIsFalse()
		 {
			  MockTxIdStore();
			  CheckPointerImpl checkPointer = checkPointer();
			  BinaryLatch arriveFlushAndForce = new BinaryLatch();
			  BinaryLatch finishFlushAndForce = new BinaryLatch();

			  doAnswer(invocation =>
			  {
				arriveFlushAndForce.Release();
				finishFlushAndForce.Await();
				return null;
			  }).when( _storageEngine ).flushAndForce( _limiter );

			  Thread forceCheckPointThread = new Thread(() =>
			  {
				try
				{
					 checkPointer.ForceCheckPoint( _info );
				}
				catch ( Exception e )
				{
					 Console.WriteLine( e.ToString() );
					 Console.Write( e.StackTrace );
					 throw new Exception( e );
				}
			  });
			  forceCheckPointThread.Start();

			  arriveFlushAndForce.Await(); // Wait for force-thread to arrive in flushAndForce().

			  System.Func<bool> predicate = mock( typeof( System.Func<bool> ) );
			  when( predicate() ).thenReturn(false, false, true);
			  assertThat( checkPointer.TryCheckPoint( _info, predicate ), @is( -1L ) ); // We decided to not wait for the on-going check point to finish.

			  finishFlushAndForce.Release(); // Let the flushAndForce complete.
			  forceCheckPointThread.Join();

			  assertThat( checkPointer.TryCheckPoint( _info, predicate ), @is( this._transactionId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyAsyncActionCausesConcurrentFlushingRush(Neo4Net.function.ThrowingConsumer<CheckPointerImpl,java.io.IOException> asyncAction) throws Exception
		 private void VerifyAsyncActionCausesConcurrentFlushingRush( ThrowingConsumer<CheckPointerImpl, IOException> asyncAction )
		 {
			  AtomicLong limitDisableCounter = new AtomicLong();
			  AtomicLong observedRushCount = new AtomicLong();
			  BinaryLatch backgroundCheckPointStartedLatch = new BinaryLatch();
			  BinaryLatch forceCheckPointStartLatch = new BinaryLatch();

			  _limiter = new IOLimiterAnonymousInnerClass4( this, limitDisableCounter, forceCheckPointStartLatch );

			  MockTxIdStore();
			  CheckPointerImpl checkPointer = checkPointer();

			  doAnswer(invocation =>
			  {
				backgroundCheckPointStartedLatch.Release();
				forceCheckPointStartLatch.Await();
				long newValue = limitDisableCounter.get();
				observedRushCount.set( newValue );
				return null;
			  }).when( _storageEngine ).flushAndForce( _limiter );

			  Future<object> forceCheckPointer = forkFuture(() =>
			  {
				backgroundCheckPointStartedLatch.Await();
				asyncAction.Accept( checkPointer );
				return null;
			  });

			  when( _threshold.isCheckPointingNeeded( anyLong(), eq(_info) ) ).thenReturn(true);
			  checkPointer.CheckPointIfNeeded( _info );
			  forceCheckPointer.get();
			  assertThat( observedRushCount.get(), @is(1L) );
		 }

		 private class IOLimiterAnonymousInnerClass4 : IOLimiter
		 {
			 private readonly CheckPointerImplTest _outerInstance;

			 private AtomicLong _limitDisableCounter;
			 private BinaryLatch _forceCheckPointStartLatch;

			 public IOLimiterAnonymousInnerClass4( CheckPointerImplTest outerInstance, AtomicLong limitDisableCounter, BinaryLatch forceCheckPointStartLatch )
			 {
				 this.outerInstance = outerInstance;
				 this._limitDisableCounter = limitDisableCounter;
				 this._forceCheckPointStartLatch = forceCheckPointStartLatch;
			 }

			 public long maybeLimitIO( long previousStamp, int recentlyCompletedIOs, Flushable flushable )
			 {
				  return 0;
			 }

			 public void disableLimit()
			 {
				  _limitDisableCounter.AndIncrement;
				  _forceCheckPointStartLatch.release();
			 }

			 public void enableLimit()
			 {
				  _limitDisableCounter.AndDecrement;
			 }

			 public bool Limited
			 {
				 get
				 {
					  return _limitDisableCounter.get() != 0;
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5000) public void mustRequestFastestPossibleFlushWhenForceCheckPointIsCalledDuringBackgroundCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustRequestFastestPossibleFlushWhenForceCheckPointIsCalledDuringBackgroundCheckPoint()
		 {
			  VerifyAsyncActionCausesConcurrentFlushingRush( checkPointer => checkPointer.forceCheckPoint( new SimpleTriggerInfo( "async" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5000) public void mustRequestFastestPossibleFlushWhenTryCheckPointIsCalledDuringBackgroundCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustRequestFastestPossibleFlushWhenTryCheckPointIsCalledDuringBackgroundCheckPoint()
		 {
			  VerifyAsyncActionCausesConcurrentFlushingRush( checkPointer => checkPointer.tryCheckPoint( new SimpleTriggerInfo( "async" ) ) );
		 }

		 private CheckPointerImpl CheckPointer( StoreCopyCheckPointMutex mutex )
		 {
			  return new CheckPointerImpl( _txIdStore, _threshold, _storageEngine, _logPruning, _appender, _health, NullLogProvider.Instance, _tracer, _limiter, mutex );
		 }

		 private CheckPointerImpl CheckPointer()
		 {
			  return CheckPointer( new StoreCopyCheckPointMutex() );
		 }

		 private void MockTxIdStore()
		 {
			  long[] triggerCommittedTransaction = new long[] { _transactionId, _logPosition.LogVersion, _logPosition.ByteOffset };
			  when( _txIdStore.LastClosedTransaction ).thenReturn( triggerCommittedTransaction );
			  when( _txIdStore.LastClosedTransactionId ).thenReturn( _initialTransactionId, _transactionId, _transactionId );
		 }

		 private class CheckPointerThread : Thread
		 {
			  internal readonly CheckPointerImpl CheckPointing;
			  internal readonly System.Threading.CountdownEvent StartSignal;
			  internal readonly System.Threading.CountdownEvent Completed;

			  internal CheckPointerThread( CheckPointerImpl checkPointing, System.Threading.CountdownEvent startSignal, System.Threading.CountdownEvent completed )
			  {
					this.CheckPointing = checkPointing;
					this.StartSignal = startSignal;
					this.Completed = completed;
			  }

			  public override void Run()
			  {
					try
					{
						 StartSignal.Signal();
						 StartSignal.await();
						 CheckPointing.forceCheckPoint( _info );
						 Completed.Signal();
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
			  }
		 }
	}

}