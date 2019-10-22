using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise.@lock.forseti
{
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using DynamicTest = org.junit.jupiter.api.DynamicTest;
	using TestFactory = org.junit.jupiter.api.TestFactory;
	using ThrowingConsumer = org.junit.jupiter.api.function.ThrowingConsumer;


	using Neo4Net.Functions;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using CommunityLockManger = Neo4Net.Kernel.impl.locking.community.CommunityLockManger;
	using LockWaitStrategies = Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using Neo4Net.Storageengine.Api.@lock;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

	internal class ForsetiFalseDeadlockTest
	{
		 private const int TEST_RUNS = 10;
		 private static ExecutorService _executor = Executors.newCachedThreadPool(r =>
		 {
		  Thread thread = new Thread( r );
		  thread.Daemon = true;
		  return thread;
		 });

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void tearDown()
		 internal static void TearDown()
		 {
			  _executor.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TestFactory Stream<org.junit.jupiter.api.DynamicTest> testMildlyForFalseDeadlocks()
		 internal virtual Stream<DynamicTest> TestMildlyForFalseDeadlocks()
		 {
			  ThrowingConsumer<Fixture> fixtureConsumer = fixture => loopRunTest( fixture, TEST_RUNS );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return DynamicTest.stream( Fixtures(), Fixture::toString, fixtureConsumer );
		 }

		 private static IEnumerator<Fixture> Fixtures()
		 {
			  IList<Fixture> fixtures = new List<Fixture>();

			  // During development I also had iteration counts 1 and 2 here, but they never found anything, so for actually
			  // running this test, I leave only iteration count 100 enabled.
			  int iteration = 100;
			  LockManager[] lockManagers = LockManager.values();
			  LockWaitStrategies[] lockWaitStrategies = Enum.GetValues( typeof( LockWaitStrategies ) );
			  LockType[] lockTypes = LockType.values();
			  foreach ( LockManager lockManager in lockManagers )
			  {
					foreach ( LockWaitStrategies waitStrategy in lockWaitStrategies )
					{
						 if ( waitStrategy == LockWaitStrategies.NO_WAIT )
						 {
							  continue; // Skip NO_WAIT.
						 }
						 foreach ( LockType lockTypeAX in lockTypes )
						 {
							  foreach ( LockType lockTypeAY in lockTypes )
							  {
									foreach ( LockType lockTypeBX in lockTypes )
									{
										 foreach ( LockType lockTypeBY in lockTypes )
										 {
											  fixtures.Add( new Fixture( iteration, lockManager, waitStrategy, lockTypeAX, lockTypeAY, lockTypeBX, lockTypeBY ) );
										 }
									}
							  }
						 }
					}
			  }
			  return fixtures.GetEnumerator();
		 }

		 private static void LoopRunTest( Fixture fixture, int testRuns )
		 {
			  IList<Exception> exceptionList = new List<Exception>();
			  LoopRun( fixture, testRuns, exceptionList );

			  if ( exceptionList.Count > 0 )
			  {
					// We saw exceptions. Run it 99 more times, and then verify that our false deadlock rate is less than 2%.
					int additionalRuns = testRuns * 99;
					LoopRun( fixture, additionalRuns, exceptionList );
					double totalRuns = additionalRuns + testRuns;
					double failures = exceptionList.Count;
					double failureRate = failures / totalRuns;
					if ( failureRate > 0.02 )
					{
						 // We have more than 2% failures. Report it!
						 AssertionError error = new AssertionError( "False deadlock failure rate of " + failureRate + " is greater than 2%" );
						 foreach ( Exception th in exceptionList )
						 {
							  error.addSuppressed( th );
						 }
						 throw error;
					}
			  }
		 }

		 private static void LoopRun( Fixture fixture, int testRuns, IList<Exception> exceptionList )
		 {
			  for ( int i = 0; i < testRuns; i++ )
			  {
					try
					{
						 RunTest( fixture );
					}
					catch ( Exception th )
					{
						 th.addSuppressed( new Exception( "Failed at iteration " + i ) );
						 exceptionList.Add( th );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void runTest(Fixture fixture) throws InterruptedException, java.util.concurrent.ExecutionException
		 private static void RunTest( Fixture fixture )
		 {
			  int iterations = fixture.Iterations();
			  ResourceType resourceType = fixture.CreateResourceType();
			  Locks manager = fixture.CreateLockManager( resourceType );
			  try
			  {
					  using ( Neo4Net.Kernel.impl.locking.Locks_Client a = manager.NewClient(), Neo4Net.Kernel.impl.locking.Locks_Client b = manager.NewClient() )
					  {
						BinaryLatch startLatch = new BinaryLatch();
						BlockedCallable callA = new BlockedCallable( startLatch, () => workloadA(fixture, a, resourceType, iterations) );
						BlockedCallable callB = new BlockedCallable( startLatch, () => workloadB(fixture, b, resourceType, iterations) );
      
						Future<Void> futureA = _executor.submit( callA );
						Future<Void> futureB = _executor.submit( callB );
      
						callA.AwaitBlocked();
						callB.AwaitBlocked();
      
						startLatch.Release();
      
						futureA.get();
						futureB.get();
					  }
			  }
			  finally
			  {
					manager.Close();
			  }
		 }

		 private static void WorkloadA( Fixture fixture, Neo4Net.Kernel.impl.locking.Locks_Client a, ResourceType resourceType, int iterations )
		 {
			  for ( int i = 0; i < iterations; i++ )
			  {
					fixture.AcquireAX( a, resourceType );
					fixture.AcquireAY( a, resourceType );
					fixture.ReleaseAY( a, resourceType );
					fixture.ReleaseAX( a, resourceType );
			  }
		 }

		 private static void WorkloadB( Fixture fixture, Neo4Net.Kernel.impl.locking.Locks_Client b, ResourceType resourceType, int iterations )
		 {
			  for ( int i = 0; i < iterations; i++ )
			  {
					fixture.AcquireBX( b, resourceType );
					fixture.ReleaseBX( b, resourceType );
					fixture.AcquireBY( b, resourceType );
					fixture.ReleaseBY( b, resourceType );
			  }
		 }

		 private class BlockedCallable : Callable<Void>
		 {
			  internal readonly BinaryLatch StartLatch;
			  internal readonly ThrowingAction<Exception> Delegate;
			  internal volatile Thread Runner;

			  internal BlockedCallable( BinaryLatch startLatch, ThrowingAction<Exception> @delegate )
			  {
					this.StartLatch = startLatch;
					this.Delegate = @delegate;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void call() throws Exception
			  public override Void Call()
			  {
					Runner = Thread.CurrentThread;
					StartLatch.await();
					Delegate.apply();
					return null;
			  }

			  internal virtual void AwaitBlocked()
			  {
					Thread t;
					do
					{
						 t = Runner;
					} while ( t == null || t.State != Thread.State.WAITING );
			  }
		 }

		 private class Fixture
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int IterationsConflict;
			  internal readonly LockManager LockManager;
			  internal readonly WaitStrategy WaitStrategy;
			  internal readonly LockType LockTypeAX;
			  internal readonly LockType LockTypeAY;
			  internal readonly LockType LockTypeBX;
			  internal readonly LockType LockTypeBY;

			  internal Fixture( int iterations, LockManager lockManager, WaitStrategy waitStrategy, LockType lockTypeAX, LockType lockTypeAY, LockType lockTypeBX, LockType lockTypeBY )
			  {
					this.IterationsConflict = iterations;
					this.LockManager = lockManager;
					this.WaitStrategy = waitStrategy;
					this.LockTypeAX = lockTypeAX;
					this.LockTypeAY = lockTypeAY;
					this.LockTypeBX = lockTypeBX;
					this.LockTypeBY = lockTypeBY;
			  }

			  internal virtual int Iterations()
			  {
					return IterationsConflict;
			  }

			  internal virtual Locks CreateLockManager( ResourceType resourceType )
			  {
					return LockManager.create( resourceType );
			  }

			  internal virtual ResourceType CreateResourceType()
			  {
					return new ResourceTypeAnonymousInnerClass( this );
			  }

			  private class ResourceTypeAnonymousInnerClass : ResourceType
			  {
				  private readonly Fixture _outerInstance;

				  public ResourceTypeAnonymousInnerClass( Fixture outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public int typeId()
				  {
						return 0;
				  }

				  public WaitStrategy waitStrategy()
				  {
						return _outerInstance.waitStrategy;
				  }

				  public string name()
				  {
						return "MyTestResource";
				  }
			  }

			  internal virtual void AcquireAX( Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType )
			  {
					LockTypeAX.acquire( client, resourceType, 1 );
			  }

			  internal virtual void ReleaseAX( Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType )
			  {
					LockTypeAX.release( client, resourceType, 1 );
			  }

			  internal virtual void AcquireAY( Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType )
			  {
					LockTypeAY.acquire( client, resourceType, 2 );
			  }

			  internal virtual void ReleaseAY( Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType )
			  {
					LockTypeAY.release( client, resourceType, 2 );
			  }

			  internal virtual void AcquireBX( Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType )
			  {
					LockTypeBX.acquire( client, resourceType, 1 );
			  }

			  internal virtual void ReleaseBX( Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType )
			  {
					LockTypeBX.release( client, resourceType, 1 );
			  }

			  internal virtual void AcquireBY( Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType )
			  {
					LockTypeBY.acquire( client, resourceType, 2 );
			  }

			  internal virtual void ReleaseBY( Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType )
			  {
					LockTypeBY.release( client, resourceType, 2 );
			  }

			  public override string ToString()
			  {
					return "iterations=" + IterationsConflict +
							  ", lockManager=" + LockManager +
							  ", waitStrategy=" + WaitStrategy +
							  ", lockTypeAX=" + LockTypeAX +
							  ", lockTypeAY=" + LockTypeAY +
							  ", lockTypeBX=" + LockTypeBX +
							  ", lockTypeBY=" + LockTypeBY;
			  }
		 }

		 public abstract class LockType
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           EXCLUSIVE { public void acquire(org.Neo4Net.kernel.impl.locking.Locks_Client client, org.Neo4Net.storageengine.api.lock.ResourceType resourceType, int resource) { client.acquireExclusive(org.Neo4Net.storageengine.api.lock.LockTracer.NONE, resourceType, resource); } public void release(org.Neo4Net.kernel.impl.locking.Locks_Client client, org.Neo4Net.storageengine.api.lock.ResourceType resourceType, int resource) { client.releaseExclusive(resourceType, resource); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           SHARED { public void acquire(org.Neo4Net.kernel.impl.locking.Locks_Client client, org.Neo4Net.storageengine.api.lock.ResourceType resourceType, int resource) { client.acquireShared(org.Neo4Net.storageengine.api.lock.LockTracer.NONE, resourceType, resource); } public void release(org.Neo4Net.kernel.impl.locking.Locks_Client client, org.Neo4Net.storageengine.api.lock.ResourceType resourceType, int resource) { client.releaseShared(resourceType, resource); } };

			  private static readonly IList<LockType> valueList = new List<LockType>();

			  static LockType()
			  {
				  valueList.Add( EXCLUSIVE );
				  valueList.Add( SHARED );
			  }

			  public enum InnerEnum
			  {
				  EXCLUSIVE,
				  SHARED
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private LockType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public abstract void acquire( Neo4Net.Kernel.impl.locking.Locks_Client client, Neo4Net.Storageengine.Api.@lock.ResourceType resourceType, int resource );

			  public abstract void release( Neo4Net.Kernel.impl.locking.Locks_Client client, Neo4Net.Storageengine.Api.@lock.ResourceType resourceType, int resource );

			 public static IList<LockType> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static LockType valueOf( string name )
			 {
				 foreach ( LockType enumInstance in LockType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 public abstract class LockManager
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           COMMUNITY { public org.Neo4Net.kernel.impl.locking.Locks create(org.Neo4Net.storageengine.api.lock.ResourceType resourceType) { return new org.Neo4Net.kernel.impl.locking.community.CommunityLockManger(org.Neo4Net.kernel.configuration.Config.defaults(), java.time.Clock.systemDefaultZone()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FORSETI { public org.Neo4Net.kernel.impl.locking.Locks create(org.Neo4Net.storageengine.api.lock.ResourceType resourceType) { return new ForsetiLockManager(org.Neo4Net.kernel.configuration.Config.defaults(), java.time.Clock.systemDefaultZone(), resourceType); } };

			  private static readonly IList<LockManager> valueList = new List<LockManager>();

			  static LockManager()
			  {
				  valueList.Add( COMMUNITY );
				  valueList.Add( FORSETI );
			  }

			  public enum InnerEnum
			  {
				  COMMUNITY,
				  FORSETI
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private LockManager( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public abstract Neo4Net.Kernel.impl.locking.Locks create( Neo4Net.Storageengine.Api.@lock.ResourceType resourceType );

			 public static IList<LockManager> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static LockManager valueOf( string name )
			 {
				 foreach ( LockManager enumInstance in LockManager.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}