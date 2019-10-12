using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.locking
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RunWith = org.junit.runner.RunWith;
	using Suite = org.junit.runners.Suite;


	using Config = Neo4Net.Kernel.configuration.Config;
	using AcquireLockTimeoutException = Neo4Net.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using Neo4Net.Test;
	using WaitDetails = Neo4Net.Test.OtherThreadExecutor.WaitDetails;
	using Neo4Net.Test.OtherThreadExecutor;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Neo4Net.Test.rule.concurrent;
	using ParameterizedSuiteRunner = Neo4Net.Test.runner.ParameterizedSuiteRunner;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.concurrent.OtherThreadRule.isWaiting;

	/// <summary>
	/// Base for locking tests. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(ParameterizedSuiteRunner.class) @Suite.SuiteClasses({AcquireAndReleaseLocksCompatibility.class, DeadlockCompatibility.class, LockReentrancyCompatibility.class, RWLockCompatibility.class, StopCompatibility.class, CloseCompatibility.class, AcquisitionTimeoutCompatibility.class, TracerCompatibility.class, ActiveLocksListingCompatibility.class}) public abstract class LockingCompatibilityTestSuite
	public abstract class LockingCompatibilityTestSuite
	{
		 protected internal abstract Locks CreateLockManager( Config config, Clock clock );

		 /// <summary>
		 /// Implementing this requires intricate knowledge of implementation of the particular locks client.
		 /// This is the most efficient way of telling whether or not a thread awaits a lock acquisition or not
		 /// so the price we pay for the potential fragility introduced here we gain in much snappier testing
		 /// when testing deadlocks and lock acquisitions.
		 /// </summary>
		 /// <param name="details"> <seealso cref="WaitDetails"/> gotten at a confirmed thread wait/block or similar,
		 /// see <seealso cref="OtherThreadExecutor"/>. </param>
		 /// <returns> {@code true} if the wait details marks a wait on a lock acquisition, otherwise {@code false}
		 /// so that a new thread wait/block will be registered and this method called again. </returns>
		 protected internal abstract bool IsAwaitingLockAcquisition( OtherThreadExecutor.WaitDetails details );

		 public abstract class Compatibility
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.concurrent.OtherThreadRule<Void> threadA = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
			  public OtherThreadRule<Void> ThreadA = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.concurrent.OtherThreadRule<Void> threadB = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
			  public OtherThreadRule<Void> ThreadB = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.concurrent.OtherThreadRule<Void> threadC = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
			  public OtherThreadRule<Void> ThreadC = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
			  public TestDirectory TestDir = TestDirectory.testDirectory();

			  protected internal readonly LockingCompatibilityTestSuite Suite;

			  protected internal Locks Locks;
			  protected internal Locks_Client ClientA;
			  protected internal Locks_Client ClientB;
			  protected internal Locks_Client ClientC;

			  internal readonly IDictionary<Locks_Client, OtherThreadRule<Void>> ClientToThreadMap = new Dictionary<Locks_Client, OtherThreadRule<Void>>();

			  public Compatibility( LockingCompatibilityTestSuite suite )
			  {
					this.Suite = suite;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
			  public virtual void Before()
			  {
					this.Locks = Suite.createLockManager( Config.defaults(), Clocks.systemClock() );
					ClientA = this.Locks.newClient();
					ClientB = this.Locks.newClient();
					ClientC = this.Locks.newClient();

					ClientToThreadMap[ClientA] = ThreadA;
					ClientToThreadMap[ClientB] = ThreadB;
					ClientToThreadMap[ClientC] = ThreadC;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
			  public virtual void After()
			  {
					ClientA.close();
					ClientB.close();
					ClientC.close();
					Locks.close();
					ClientToThreadMap.Clear();
			  }

			  // Utilities

			  public abstract class LockCommand : OtherThreadExecutor.WorkerCommand<Void, object>
			  {
				  public abstract R DoWork( T state );
				  private readonly LockingCompatibilityTestSuite.Compatibility _outerInstance;

					internal readonly OtherThreadRule<Void> Thread;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
					internal readonly Locks_Client ClientConflict;

					protected internal LockCommand( LockingCompatibilityTestSuite.Compatibility outerInstance, OtherThreadRule<Void> thread, Locks_Client client )
					{
						this._outerInstance = outerInstance;
						 this.Thread = thread;
						 this.ClientConflict = client;
					}

					public virtual Future<object> Call()
					{
						 return Thread.execute( this );
					}

					public virtual Future<object> CallAndAssertWaiting()
					{
						 Future<object> otherThreadLock = Call();
						 assertThat( Thread, Waiting );
						 assertFalse( "Should not have acquired lock.", otherThreadLock.Done );
						 return otherThreadLock;
					}

					public virtual Future<object> CallAndAssertNotWaiting()
					{
						 Future<object> run = Call();
						 outerInstance.AssertNotWaiting( ClientConflict, run );
						 return run;
					}

					public override object DoWork( Void state )
					{
						 DoWork( ClientConflict );
						 return null;
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void doWork(Locks_Client client) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException;
					internal abstract void DoWork( Locks_Client client );

					public virtual Locks_Client Client()
					{
						 return ClientConflict;
					}
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected LockCommand acquireExclusive(final Locks_Client client, final org.neo4j.storageengine.api.lock.LockTracer tracer, final org.neo4j.storageengine.api.lock.ResourceType resourceType, final long key)
			  protected internal virtual LockCommand AcquireExclusive( Locks_Client client, LockTracer tracer, ResourceType resourceType, long key )
			  {
					return new LockCommandAnonymousInnerClass( this, ClientToThreadMap[client], client, tracer, resourceType, key );
			  }

			  private class LockCommandAnonymousInnerClass : LockCommand
			  {
				  private readonly Compatibility _outerInstance;

				  private Neo4Net.Kernel.impl.locking.Locks_Client _client;
				  private LockTracer _tracer;
				  private ResourceType _resourceType;
				  private long _key;

				  public LockCommandAnonymousInnerClass( Compatibility outerInstance, OtherThreadRule<Void> get, Neo4Net.Kernel.impl.locking.Locks_Client client, LockTracer tracer, ResourceType resourceType, long key ) : base( outerInstance, get, client )
				  {
					  this.outerInstance = outerInstance;
					  this._client = client;
					  this._tracer = tracer;
					  this._resourceType = resourceType;
					  this._key = key;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doWork(Locks_Client client) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
				  public override void doWork( Locks_Client client )
				  {
						client.AcquireExclusive( _tracer, _resourceType, _key );
				  }
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected LockCommand acquireShared(Locks_Client client, final org.neo4j.storageengine.api.lock.LockTracer tracer, final org.neo4j.storageengine.api.lock.ResourceType resourceType, final long key)
			  protected internal virtual LockCommand AcquireShared( Locks_Client client, LockTracer tracer, ResourceType resourceType, long key )
			  {
					return new LockCommandAnonymousInnerClass2( this, ClientToThreadMap[client], client, tracer, resourceType, key );
			  }

			  private class LockCommandAnonymousInnerClass2 : LockCommand
			  {
				  private readonly Compatibility _outerInstance;

				  private Neo4Net.Kernel.impl.locking.Locks_Client _client;
				  private LockTracer _tracer;
				  private ResourceType _resourceType;
				  private long _key;

				  public LockCommandAnonymousInnerClass2( Compatibility outerInstance, OtherThreadRule<Void> get, Neo4Net.Kernel.impl.locking.Locks_Client client, LockTracer tracer, ResourceType resourceType, long key ) : base( outerInstance, get, client )
				  {
					  this.outerInstance = outerInstance;
					  this._client = client;
					  this._tracer = tracer;
					  this._resourceType = resourceType;
					  this._key = key;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doWork(Locks_Client client) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
				  public override void doWork( Locks_Client client )
				  {
						client.AcquireShared( _tracer, _resourceType, _key );
				  }
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected LockCommand release(final Locks_Client client, final org.neo4j.storageengine.api.lock.ResourceType resourceType, final long key)
			  protected internal virtual LockCommand Release( Locks_Client client, ResourceType resourceType, long key )
			  {
					return new LockCommandAnonymousInnerClass3( this, ClientToThreadMap[client], client, resourceType, key );
			  }

			  private class LockCommandAnonymousInnerClass3 : LockCommand
			  {
				  private readonly Compatibility _outerInstance;

				  private Neo4Net.Kernel.impl.locking.Locks_Client _client;
				  private ResourceType _resourceType;
				  private long _key;

				  public LockCommandAnonymousInnerClass3( Compatibility outerInstance, OtherThreadRule<Void> get, Neo4Net.Kernel.impl.locking.Locks_Client client, ResourceType resourceType, long key ) : base( outerInstance, get, client )
				  {
					  this.outerInstance = outerInstance;
					  this._client = client;
					  this._resourceType = resourceType;
					  this._key = key;
				  }

				  public override void doWork( Locks_Client client )
				  {
						client.ReleaseExclusive( _resourceType, _key );
				  }
			  }

			  protected internal virtual void AssertNotWaiting( Locks_Client client, Future<object> @lock )
			  {
					try
					{
						 @lock.get( 5, TimeUnit.SECONDS );
					}
					catch ( Exception e ) when ( e is ExecutionException || e is TimeoutException || e is InterruptedException )
					{
						 throw new Exception( "Waiting for lock timed out!" );
					}
			  }

			  protected internal virtual void AssertWaiting( Locks_Client client, Future<object> @lock )
			  {
					try
					{
						 @lock.get( 10, TimeUnit.MILLISECONDS );
						 fail( "Should be waiting." );
					}
					catch ( TimeoutException )
					{
						 // Ok
					}
					catch ( Exception e ) when ( e is ExecutionException || e is InterruptedException )
					{
						 throw new Exception( e );
					}
					assertThat( ClientToThreadMap[client], Waiting );
			  }
		 }
	}

}