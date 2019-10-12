using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using BeforeAll = org.junit.jupiter.api.BeforeAll;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using RepeatedTest = org.junit.jupiter.api.RepeatedTest;
	using Test = org.junit.jupiter.api.Test;


	using GlobalMemoryTracker = Org.Neo4j.Memory.GlobalMemoryTracker;
	using DaemonThreadFactory = Org.Neo4j.Scheduler.DaemonThreadFactory;
	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	internal class SequenceLockStressIT
	{
		 private static ExecutorService _executor;
		 private static long _lockAddr;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeAll static void initialise()
		 internal static void Initialise()
		 {
			  _lockAddr = UnsafeUtil.allocateMemory( Long.BYTES );
			  _executor = Executors.newCachedThreadPool( new DaemonThreadFactory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void cleanup()
		 internal static void Cleanup()
		 {
			  _executor.shutdown();
			  UnsafeUtil.free( _lockAddr, Long.BYTES, GlobalMemoryTracker.INSTANCE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void allocateLock()
		 internal virtual void AllocateLock()
		 {
			  UnsafeUtil.putLong( _lockAddr, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(2) void stressTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StressTest()
		 {
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] data = new int[10][10];
			  int[][] data = RectangularArrays.RectangularIntArray( 10, 10 );
			  AtomicBoolean stop = new AtomicBoolean();
			  AtomicInteger writerId = new AtomicInteger();

			  abstract class Worker implements ThreadStart
			  {
					public void run()
					{
						 try
						 {
							  doWork();
						 }
						 finally
						 {
							  stop.set( true );
						 }
					}

					protected abstract void doWork();
			  }

			  Worker reader = new WorkerAnonymousInnerClass( this, data, stop );

			  Worker writer = new WorkerAnonymousInnerClass2( this, data, stop, writerId );

			  Worker exclusive = new WorkerAnonymousInnerClass3( this, data, stop );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> readers = new java.util.ArrayList<>();
			  IList<Future<object>> readers = new List<Future<object>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> writers = new java.util.ArrayList<>();
			  IList<Future<object>> writers = new List<Future<object>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> exclusiveFuture = executor.submit(exclusive);
			  Future<object> exclusiveFuture = _executor.submit( exclusive );
			  for ( int i = 0; i < 20; i++ )
			  {
					readers.Add( _executor.submit( reader ) );
			  }
			  for ( int i = 0; i < data.Length; i++ )
			  {
					writers.Add( _executor.submit( writer ) );
			  }

			  long deadline = DateTimeHelper.CurrentUnixTimeMillis() + 1000;
			  while ( !stop.get() && DateTimeHelper.CurrentUnixTimeMillis() < deadline )
			  {
					Thread.Sleep( 20 );
			  }
			  stop.set( true );

			  exclusiveFuture.get();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : writers)
			  foreach ( Future<object> future in writers )
			  {
					future.get();
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : readers)
			  foreach ( Future<object> future in readers )
			  {
					future.get();
			  }
		 }

		 private class WorkerAnonymousInnerClass : Worker
		 {
			 private readonly SequenceLockStressIT _outerInstance;

			 private int[][] _data;
			 private AtomicBoolean _stop;

			 public WorkerAnonymousInnerClass( SequenceLockStressIT outerInstance, int[][] data, AtomicBoolean stop )
			 {
				 this.outerInstance = outerInstance;
				 this._data = data;
				 this._stop = stop;
			 }

			 protected internal override void doWork()
			 {
				  while ( !_stop.get() )
				  {
						ThreadLocalRandom rng = ThreadLocalRandom.current();
						int[] record = _data[rng.Next( _data.Length )];

						long stamp = OffHeapPageLock.TryOptimisticReadLock( _lockAddr );
						int value = record[0];
						bool consistent = true;
						foreach ( int i in record )
						{
							 consistent &= i == value;
						}
						if ( OffHeapPageLock.ValidateReadLock( _lockAddr, stamp ) && !consistent )
						{
							 throw new AssertionError( "inconsistent read" );
						}
				  }
			 }
		 }

		 private class WorkerAnonymousInnerClass2 : Worker
		 {
			 private readonly SequenceLockStressIT _outerInstance;

			 private int[][] _data;
			 private AtomicBoolean _stop;
			 private AtomicInteger _writerId;

			 public WorkerAnonymousInnerClass2( SequenceLockStressIT outerInstance, int[][] data, AtomicBoolean stop, AtomicInteger writerId )
			 {
				 this.outerInstance = outerInstance;
				 this._data = data;
				 this._stop = stop;
				 this._writerId = writerId;
			 }

			 private volatile long unused;

			 protected internal override void doWork()
			 {
				  int id = _writerId.AndIncrement;
				  int counter = 1;
				  ThreadLocalRandom rng = ThreadLocalRandom.current();
				  int smallSpin = rng.Next( 5, 50 );
				  int bigSpin = rng.Next( 100, 1000 );

				  while ( !_stop.get() )
				  {
						if ( OffHeapPageLock.TryWriteLock( _lockAddr ) )
						{
							 int[] record = _data[id];
							 for ( int i = 0; i < record.Length; i++ )
							 {
								  record[i] = counter;
								  for ( int j = 0; j < smallSpin; j++ )
								  {
										unused = rng.nextLong();
								  }
							 }
							 OffHeapPageLock.UnlockWrite( _lockAddr );
						}

						for ( int j = 0; j < bigSpin; j++ )
						{
							 unused = rng.nextLong();
						}
				  }
			 }
		 }

		 private class WorkerAnonymousInnerClass3 : Worker
		 {
			 private readonly SequenceLockStressIT _outerInstance;

			 private int[][] _data;
			 private AtomicBoolean _stop;

			 public WorkerAnonymousInnerClass3( SequenceLockStressIT outerInstance, int[][] data, AtomicBoolean stop )
			 {
				 this.outerInstance = outerInstance;
				 this._data = data;
				 this._stop = stop;
			 }

			 private volatile long unused;

			 protected internal override void doWork()
			 {
				  ThreadLocalRandom rng = ThreadLocalRandom.current();
				  int spin = rng.Next( 20, 2000 );
				  while ( !_stop.get() )
				  {
						while ( !OffHeapPageLock.TryExclusiveLock( _lockAddr ) )
						{
						}
						long sumA = 0;
						long sumB = 0;
						foreach ( int[] ints in _data )
						{
							 foreach ( int i in ints )
							 {
								  sumA += i;
							 }
						}
						for ( int i = 0; i < spin; i++ )
						{
							 unused = rng.nextLong();
						}
						foreach ( int[] record in _data )
						{
							 foreach ( int value in record )
							 {
								  sumB += value;
							 }
							 Arrays.fill( record, 0 );
						}
						OffHeapPageLock.UnlockExclusive( _lockAddr );
						if ( sumA != sumB )
						{
							 throw new AssertionError( "Inconsistent exclusive lock. 'Sum A' = " + sumA + ", 'Sum B' = " + sumB );
						}
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void thoroughlyEnsureAtomicityOfUnlockExclusiveAndTakeWriteLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ThoroughlyEnsureAtomicityOfUnlockExclusiveAndTakeWriteLock()
		 {
			  for ( int i = 0; i < 30000; i++ )
			  {
					UnlockExclusiveAndTakeWriteLockMustBeAtomic();
					OffHeapPageLock.UnlockWrite( _lockAddr );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void unlockExclusiveAndTakeWriteLockMustBeAtomic() throws Exception
		 private void UnlockExclusiveAndTakeWriteLockMustBeAtomic()
		 {
			  int threads = Runtime.Runtime.availableProcessors() - 1;
			  System.Threading.CountdownEvent start = new System.Threading.CountdownEvent( threads );
			  AtomicBoolean stop = new AtomicBoolean();
			  OffHeapPageLock.TryExclusiveLock( _lockAddr );
			  ThreadStart runnable = () =>
			  {
				while ( !stop.get() )
				{
					 if ( OffHeapPageLock.TryExclusiveLock( _lockAddr ) )
					 {
						  OffHeapPageLock.UnlockExclusive( _lockAddr );
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
			  OffHeapPageLock.UnlockExclusiveAndTakeWriteLock( _lockAddr );
			  stop.set( true );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
			  foreach ( Future<object> future in futures )
			  {
					future.get(); // Assert that this does not throw
			  }
		 }
	}

}