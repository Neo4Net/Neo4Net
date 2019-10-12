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
namespace Org.Neo4j.Kernel.impl.locking
{
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.NODE;

	/// <summary>
	/// This is the test suite that tested the original (from 2007) lock manager.
	/// It has been ported to test <seealso cref="org.neo4j.kernel.impl.locking.Locks"/>
	/// to ensure implementors of that API don't fall in any of the traps this test suite sets for them.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class RWLockCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class RWLockCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		 public RWLockCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSingleThread()
		 public virtual void TestSingleThread()
		 {
			  try
			  {
					ClientA.releaseExclusive( NODE, 1L );
					fail( "Invalid release should throw exception" );
			  }
			  catch ( Exception )
			  {
					// good
			  }
			  try
			  {
					ClientA.releaseShared( NODE, 1L );
					fail( "Invalid release should throw exception" );
			  }
			  catch ( Exception )
			  {
					// good
			  }

			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  try
			  {
					ClientA.releaseExclusive( NODE, 1L );
					fail( "Invalid release should throw exception" );
			  }
			  catch ( Exception )
			  {
					// good
			  }

			  ClientA.releaseShared( NODE, 1L );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  try
			  {
					ClientA.releaseShared( NODE, 1L );
					fail( "Invalid release should throw exception" );
			  }
			  catch ( Exception )
			  {
					// good
			  }
			  ClientA.releaseExclusive( NODE, 1L );

			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  ClientA.releaseExclusive( NODE, 1L );
			  ClientA.releaseShared( NODE, 1L );

			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientA.releaseShared( NODE, 1L );
			  ClientA.releaseExclusive( NODE, 1L );

			  for ( int i = 0; i < 10; i++ )
			  {
					if ( ( i % 2 ) == 0 )
					{
						 ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
					}
					else
					{
						 ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
					}
			  }
			  for ( int i = 9; i >= 0; i-- )
			  {
					if ( ( i % 2 ) == 0 )
					{
						 ClientA.releaseExclusive( NODE, 1L );
					}
					else
					{
						 ClientA.releaseShared( NODE, 1L );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleThreads() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMultipleThreads()
		 {
			  LockWorker t1 = new LockWorker( "T1", Locks );
			  LockWorker t2 = new LockWorker( "T2", Locks );
			  LockWorker t3 = new LockWorker( "T3", Locks );
			  LockWorker t4 = new LockWorker( "T4", Locks );
			  long r1 = 1L;
			  try
			  {
					t1.GetReadLock( r1, true );
					t2.GetReadLock( r1, true );
					t3.GetReadLock( r1, true );
					Future<Void> t4Wait = t4.GetWriteLock( r1, false );
					t3.ReleaseReadLock( r1 );
					t2.ReleaseReadLock( r1 );
					assertTrue( !t4Wait.Done );
					t1.ReleaseReadLock( r1 );
					// now we can wait for write lock since it can be acquired
					// get write lock
					t4.AwaitFuture( t4Wait );
					t4.GetReadLock( r1, true );
					t4.GetReadLock( r1, true );
					// put readlock in queue
					Future<Void> t1Wait = t1.GetReadLock( r1, false );
					t4.GetReadLock( r1, true );
					t4.ReleaseReadLock( r1 );
					t4.GetWriteLock( r1, true );
					t4.ReleaseWriteLock( r1 );
					assertTrue( !t1Wait.Done );
					t4.ReleaseWriteLock( r1 );
					// get read lock
					t1.AwaitFuture( t1Wait );
					t4.ReleaseReadLock( r1 );
					// t4 now has 1 readlock and t1 one readlock
					// let t1 drop readlock and t4 get write lock
					t4Wait = t4.GetWriteLock( r1, false );
					t1.ReleaseReadLock( r1 );
					t4.AwaitFuture( t4Wait );

					t4.ReleaseReadLock( r1 );
					t4.ReleaseWriteLock( r1 );

					t4.GetWriteLock( r1, true );
					t1Wait = t1.GetReadLock( r1, false );
					Future<Void> t2Wait = t2.GetReadLock( r1, false );
					Future<Void> t3Wait = t3.GetReadLock( r1, false );
					t4.GetReadLock( r1, true );
					t4.ReleaseWriteLock( r1 );
					t1.AwaitFuture( t1Wait );
					t2.AwaitFuture( t2Wait );
					t3.AwaitFuture( t3Wait );

					t1Wait = t1.GetWriteLock( r1, false );
					t2.ReleaseReadLock( r1 );
					t4.ReleaseReadLock( r1 );
					t3.ReleaseReadLock( r1 );

					t1.AwaitFuture( t1Wait );
					t1.ReleaseWriteLock( r1 );
					t2.GetReadLock( r1, true );
					t1.ReleaseReadLock( r1 );
					t2.GetWriteLock( r1, true );
					t2.ReleaseWriteLock( r1 );
					t2.ReleaseReadLock( r1 );
			  }
			  catch ( Exception e )
			  {
					LockWorkFailureDump dumper = new LockWorkFailureDump( TestDir.file( this.GetType().Name ) );
					File file = dumper.DumpState( Locks, t1, t2, t3, t4 );
					throw new Exception( "Failed, forensics information dumped to " + file.AbsolutePath, e );
			  }
			  finally
			  {
					t1.Dispose();
					t2.Dispose();
					t3.Dispose();
					t4.Dispose();
			  }
		 }

		 public class StressThread : Thread
		 {
			 private readonly RWLockCompatibility _outerInstance;

			  internal readonly Random Rand = new Random( currentTimeMillis() );
			  internal readonly object Read = new object();
			  internal readonly object Write = new object();

			  internal readonly string Name;
			  internal readonly int NumberOfIterations;
			  internal readonly int DepthCount;
			  internal readonly float ReadWriteRatio;
			  internal readonly System.Threading.CountdownEvent StartSignal;
			  internal readonly Locks_Client Client;
			  internal readonly long NodeId;
			  internal Exception Error;

			  internal StressThread( RWLockCompatibility outerInstance, string name, int numberOfIterations, int depthCount, float readWriteRatio, long nodeId, System.Threading.CountdownEvent startSignal ) : base()
			  {
				  this._outerInstance = outerInstance;
					this.NodeId = nodeId;
					this.Client = outerInstance.Locks.newClient();
					this.Name = name;
					this.NumberOfIterations = numberOfIterations;
					this.DepthCount = depthCount;
					this.ReadWriteRatio = readWriteRatio;
					this.StartSignal = startSignal;
			  }

			  public override void Run()
			  {
					try
					{
						 StartSignal.await();
						 Stack<object> lockStack = new Stack<object>();
						 for ( int i = 0; i < NumberOfIterations; i++ )
						 {
							  try
							  {
									int depth = DepthCount;
									do
									{
										 float f = Rand.nextFloat();
										 if ( f < ReadWriteRatio )
										 {
											  Client.acquireShared( LockTracer.NONE, NODE, NodeId );
											  lockStack.Push( Read );
										 }
										 else
										 {
											  Client.acquireExclusive( LockTracer.NONE, NODE, NodeId );
											  lockStack.Push( Write );
										 }
									} while ( --depth > 0 );

									while ( lockStack.Count > 0 )
									{
										 if ( lockStack.Pop() == Read )
										 {
											  Client.releaseShared( NODE, NodeId );
										 }
										 else
										 {
											  Client.releaseExclusive( NODE, NodeId );
										 }
									}
							  }
							  catch ( DeadlockDetectedException )
							  {
							  }
							  finally
							  {
									while ( lockStack.Count > 0 )
									{
										 if ( lockStack.Pop() == Read )
										 {
											  Client.releaseShared( NODE, NodeId );
										 }
										 else
										 {
											  Client.releaseExclusive( NODE, NodeId );
										 }
									}
							  }
						 }
					}
					catch ( Exception e )
					{
						 Error = e;
					}
			  }

			  public override string ToString()
			  {
					return this.Name;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStressMultipleThreads() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestStressMultipleThreads()
		 {
			  long r1 = 1L;
			  StressThread[] stressThreads = new StressThread[100];
			  System.Threading.CountdownEvent startSignal = new System.Threading.CountdownEvent( 1 );
			  for ( int i = 0; i < 100; i++ )
			  {
					stressThreads[i] = new StressThread( this, "Thread" + i, 100, 9, 0.50f, r1, startSignal );
			  }
			  for ( int i = 0; i < 100; i++ )
			  {
					stressThreads[i].Start();
			  }
			  startSignal.Signal();

			  long end = currentTimeMillis() + SECONDS.toMillis(2000);
			  bool anyAlive;
			  while ( ( anyAlive = AnyAliveAndAllWell( stressThreads ) ) && currentTimeMillis() < end )
			  {
					SleepALittle();
			  }

			  foreach ( StressThread stressThread in stressThreads )
			  {
					if ( stressThread.Error != null )
					{
						 throw stressThread.Error;
					}
					else if ( stressThread.IsAlive )
					{
						 foreach ( StackTraceElement stackTraceElement in stressThread.StackTrace )
						 {
							  Console.WriteLine( stackTraceElement );
						 }
					}
			  }
			  if ( anyAlive )
			  {
					throw new Exception( "Expected all threads to complete." );
			  }

		 }

		 private void SleepALittle()
		 {
			  try
			  {
					Thread.Sleep( 100 );
			  }
			  catch ( InterruptedException )
			  {
					Thread.interrupted();
			  }
		 }

		 private bool AnyAliveAndAllWell( StressThread[] stressThreads )
		 {
			  foreach ( StressThread stressThread in stressThreads )
			  {
					if ( stressThread.Error != null )
					{
						 return false;
					}
					if ( stressThread.IsAlive )
					{
						 return true;
					}
			  }
			  return false;
		 }
	}

}