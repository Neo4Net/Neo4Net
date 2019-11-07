using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.locking
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using AcquireLockTimeoutException = Neo4Net.Kernel.Api.StorageEngine.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class DeferringLockClientTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.RandomRule random = new Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseOfNotHeldSharedLockThrows()
		 public virtual void ReleaseOfNotHeldSharedLockThrows()
		 {
			  // GIVEN
			  TestLocks actualLocks = new TestLocks();
			  TestLocksClient actualClient = actualLocks.NewClient();
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  try
			  {
					// WHEN
					client.ReleaseShared( ResourceTypes.Node, 42 );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					// THEN
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseOfNotHeldExclusiveLockThrows()
		 public virtual void ReleaseOfNotHeldExclusiveLockThrows()
		 {
			  // GIVEN
			  TestLocks actualLocks = new TestLocks();
			  TestLocksClient actualClient = actualLocks.NewClient();
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  try
			  {
					// WHEN
					client.ReleaseExclusive( ResourceTypes.Node, 42 );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					// THEN
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeferAllLocks()
		 public virtual void ShouldDeferAllLocks()
		 {
			  // GIVEN
			  TestLocks actualLocks = new TestLocks();
			  TestLocksClient actualClient = actualLocks.NewClient();
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  // WHEN
			  ISet<LockUnit> expected = new HashSet<LockUnit>();
			  ResourceType[] types = ResourceTypes.values();
			  for ( int i = 0; i < 10_000; i++ )
			  {
					bool exclusive = Random.nextBoolean();
					LockUnit lockUnit = new LockUnit( Random.among( types ), abs( Random.nextLong() ), exclusive );

					if ( exclusive )
					{
						 client.AcquireExclusive( LockTracer.NONE, lockUnit.ResourceType(), lockUnit.ResourceId() );
					}
					else
					{
						 client.AcquireShared( LockTracer.NONE, lockUnit.ResourceType(), lockUnit.ResourceId() );
					}
					expected.Add( lockUnit );
			  }
			  actualClient.AssertRegisteredLocks( Collections.emptySet() );
			  client.AcquireDeferredLocks( LockTracer.NONE );

			  // THEN
			  actualClient.AssertRegisteredLocks( expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopUnderlyingClient()
		 public virtual void ShouldStopUnderlyingClient()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  // WHEN
			  client.Stop();

			  // THEN
			  verify( actualClient ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrepareUnderlyingClient()
		 public virtual void ShouldPrepareUnderlyingClient()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  // WHEN
			  client.Prepare();

			  // THEN
			  verify( actualClient ).prepare();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseUnderlyingClient()
		 public virtual void ShouldCloseUnderlyingClient()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  // WHEN
			  client.Close();

			  // THEN
			  verify( actualClient ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnAcquireWhenStopped()
		 public virtual void ShouldThrowOnAcquireWhenStopped()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.Stop();

			  try
			  {
					// WHEN
					client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
					fail( "Expected exception" );
			  }
			  catch ( LockClientStoppedException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnAcquireWhenClosed()
		 public virtual void ShouldThrowOnAcquireWhenClosed()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.Close();

			  try
			  {
					// WHEN
					client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
					fail( "Expected exception" );
			  }
			  catch ( LockClientStoppedException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenReleaseNotYetAcquiredExclusive()
		 public virtual void ShouldThrowWhenReleaseNotYetAcquiredExclusive()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  try
			  {
					// WHEN
					client.ReleaseExclusive( ResourceTypes.Node, 1 );
					fail( "Expected exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenReleaseNotYetAcquiredShared()
		 public virtual void ShouldThrowWhenReleaseNotYetAcquiredShared()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  try
			  {
					// WHEN
					client.ReleaseShared( ResourceTypes.Node, 1 );
					fail( "Expected exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenReleaseNotMatchingAcquired()
		 public virtual void ShouldThrowWhenReleaseNotMatchingAcquired()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );

			  try
			  {
					// WHEN
					client.ReleaseShared( ResourceTypes.Node, 1 );
					fail( "Expected exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenReleasingLockMultipleTimes()
		 public virtual void ShouldThrowWhenReleasingLockMultipleTimes()
		 {
			  // GIVEN
			  Locks_Client actualClient = mock( typeof( Locks_Client ) );
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.ReleaseExclusive( ResourceTypes.Node, 1 );

			  try
			  {
					// WHEN
					client.ReleaseShared( ResourceTypes.Node, 1 );
					fail( "Expected exception" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLockAcquiredMultipleTimesCanNotBeReleasedAtOnce()
		 public virtual void ExclusiveLockAcquiredMultipleTimesCanNotBeReleasedAtOnce()
		 {
			  // GIVEN
			  TestLocks actualLocks = new TestLocks();
			  TestLocksClient actualClient = actualLocks.NewClient();
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.ReleaseExclusive( ResourceTypes.Node, 1 );

			  // WHEN
			  client.AcquireDeferredLocks( LockTracer.NONE );

			  // THEN
			  actualClient.AssertRegisteredLocks( Collections.singleton( new LockUnit( ResourceTypes.Node, 1, true ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sharedLockAcquiredMultipleTimesCanNotBeReleasedAtOnce()
		 public virtual void SharedLockAcquiredMultipleTimesCanNotBeReleasedAtOnce()
		 {
			  // GIVEN
			  TestLocks actualLocks = new TestLocks();
			  TestLocksClient actualClient = actualLocks.NewClient();
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.AcquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.AcquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.ReleaseShared( ResourceTypes.Node, 1 );

			  // WHEN
			  client.AcquireDeferredLocks( LockTracer.NONE );

			  // THEN
			  actualClient.AssertRegisteredLocks( Collections.singleton( new LockUnit( ResourceTypes.Node, 1, false ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireBothSharedAndExclusiveLockThenReleaseShared()
		 public virtual void AcquireBothSharedAndExclusiveLockThenReleaseShared()
		 {
			  // GIVEN
			  TestLocks actualLocks = new TestLocks();
			  TestLocksClient actualClient = actualLocks.NewClient();
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.AcquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.ReleaseShared( ResourceTypes.Node, 1 );

			  // WHEN
			  client.AcquireDeferredLocks( LockTracer.NONE );

			  // THEN
			  actualClient.AssertRegisteredLocks( Collections.singleton( new LockUnit( ResourceTypes.Node, 1, true ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLocksAcquiredFirst()
		 public virtual void ExclusiveLocksAcquiredFirst()
		 {
			  // GIVEN
			  TestLocks actualLocks = new TestLocks();
			  TestLocksClient actualClient = actualLocks.NewClient();
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.AcquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 2 );
			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 3 );
			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Relationship, 1 );
			  client.AcquireShared( LockTracer.NONE, ResourceTypes.Relationship, 2 );
			  client.AcquireShared( LockTracer.NONE, ResourceTypes.Label, 1 );
			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 42 );

			  // WHEN
			  client.AcquireDeferredLocks( LockTracer.NONE );

			  // THEN
			  ISet<LockUnit> expectedLocks = new LinkedHashSet<LockUnit>( Arrays.asList( new LockUnit( ResourceTypes.Node, 2, true ), new LockUnit( ResourceTypes.Node, 3, true ), new LockUnit( ResourceTypes.Node, 42, true ), new LockUnit( ResourceTypes.Relationship, 1, true ), new LockUnit( ResourceTypes.Node, 1, false ), new LockUnit( ResourceTypes.Relationship, 2, false ), new LockUnit( ResourceTypes.Label, 1, false ) ) );

			  actualClient.AssertRegisteredLocks( expectedLocks );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireBothSharedAndExclusiveLockThenReleaseExclusive()
		 public virtual void AcquireBothSharedAndExclusiveLockThenReleaseExclusive()
		 {
			  // GIVEN
			  TestLocks actualLocks = new TestLocks();
			  TestLocksClient actualClient = actualLocks.NewClient();
			  DeferringLockClient client = new DeferringLockClient( actualClient );

			  client.AcquireShared( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 1 );
			  client.ReleaseExclusive( ResourceTypes.Node, 1 );

			  // WHEN
			  client.AcquireDeferredLocks( LockTracer.NONE );

			  // THEN
			  actualClient.AssertRegisteredLocks( Collections.singleton( new LockUnit( ResourceTypes.Node, 1, false ) ) );
		 }

		 private class TestLocks : LifecycleAdapter, Locks
		 {
			  public override TestLocksClient NewClient()
			  {
					return new TestLocksClient();
			  }

			  public override void Accept( Locks_Visitor visitor )
			  {
			  }

			  public override void Close()
			  {
			  }
		 }

		 private class TestLocksClient : Locks_Client
		 {
			  internal readonly ISet<LockUnit> ActualLockUnits = new LinkedHashSet<LockUnit>();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireShared(Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer tracer, Neo4Net.Kernel.Api.StorageEngine.lock.ResourceType resourceType, long... resourceIds) throws Neo4Net.Kernel.Api.StorageEngine.lock.AcquireLockTimeoutException
			  public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
			  {
					Register( resourceType, false, resourceIds );
			  }

			  internal virtual void AssertRegisteredLocks( ISet<LockUnit> expectedLocks )
			  {
					assertEquals( expectedLocks, ActualLockUnits );
			  }

			  internal virtual bool Register( ResourceType resourceType, bool exclusive, params long[] resourceIds )
			  {
					foreach ( long resourceId in resourceIds )
					{
						 ActualLockUnits.Add( new LockUnit( resourceType, resourceId, exclusive ) );
					}
					return true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer tracer, Neo4Net.Kernel.Api.StorageEngine.lock.ResourceType resourceType, long... resourceIds) throws Neo4Net.Kernel.Api.StorageEngine.lock.AcquireLockTimeoutException
			  public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
			  {
					Register( resourceType, true, resourceIds );
			  }

			  public override bool TryExclusiveLock( ResourceType resourceType, long resourceId )
			  {
					return Register( resourceType, true, resourceId );
			  }

			  public override bool TrySharedLock( ResourceType resourceType, long resourceId )
			  {
					return Register( resourceType, false, resourceId );
			  }

			  public override bool ReEnterShared( ResourceType resourceType, long resourceId )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool ReEnterExclusive( ResourceType resourceType, long resourceId )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void ReleaseShared( ResourceType resourceType, params long[] resourceIds )
			  {
			  }

			  public override void ReleaseExclusive( ResourceType resourceType, params long[] resourceIds )
			  {
			  }

			  public override void Prepare()
			  {
			  }

			  public override void Stop()
			  {
			  }

			  public override void Close()
			  {
			  }

			  public virtual int LockSessionId
			  {
				  get
				  {
						return 0;
				  }
			  }

			  public override Stream<ActiveLock> ActiveLocks()
			  {
					return ActualLockUnits.Select( typeof( ActiveLock ).cast );
			  }

			  public override long ActiveLockCount()
			  {
					return ActualLockUnits.Count;
			  }
		 }
	}

}