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
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;

	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.NODE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class LockReentrancyCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class LockReentrancyCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		 public LockReentrancyCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireExclusiveIfClientIsOnlyOneHoldingShared()
		 public virtual void ShouldAcquireExclusiveIfClientIsOnlyOneHoldingShared()
		 {
			  // When
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );

			  // Then shared locks should wait
			  Future<object> clientBLock = AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then other thread should still wait
			  AssertWaiting( ClientB, clientBLock );

			  // But when
			  ClientA.releaseShared( NODE, 1L );

			  // Then
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetainExclusiveLockAfterReleasingSharedLock()
		 public virtual void ShouldRetainExclusiveLockAfterReleasingSharedLock()
		 {
			  // When
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );

			  // Then shared locks should wait
			  Future<object> clientBLock = AcquireShared( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseShared( NODE, 1L );

			  // Then other thread should still wait
			  AssertWaiting( ClientB, clientBLock );

			  // But when
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRetainSharedLockWhenAcquiredAfterExclusiveLock()
		 public virtual void ShouldRetainSharedLockWhenAcquiredAfterExclusiveLock()
		 {
			  // When
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );

			  // Then this should wait
			  Future<object> clientBLock = AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then other thread should still wait
			  AssertWaiting( ClientB, clientBLock );

			  // But when
			  ClientA.releaseShared( NODE, 1L );

			  // Then
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sharedLocksShouldStack()
		 public virtual void SharedLocksShouldStack()
		 {
			  // When
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );

			  // Then exclusive locks should wait
			  Future<object> clientBLock = AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseShared( NODE, 1L );
			  ClientA.releaseShared( NODE, 1L );

			  // Then other thread should still wait
			  AssertWaiting( ClientB, clientBLock );

			  // But when
			  ClientA.releaseShared( NODE, 1L );

			  // Then
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLocksShouldBeReentrantAndBlockOtherExclusiveLocks()
		 public virtual void ExclusiveLocksShouldBeReentrantAndBlockOtherExclusiveLocks()
		 {
			  // When
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );

			  // Then exclusive locks should wait
			  Future<object> clientBLock = AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseExclusive( NODE, 1L );
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then other thread should still wait
			  AssertWaiting( ClientB, clientBLock );

			  // But when
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLocksShouldBeReentrantAndBlockOtherSharedLocks()
		 public virtual void ExclusiveLocksShouldBeReentrantAndBlockOtherSharedLocks()
		 {
			  // When
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientA.tryExclusiveLock( NODE, 1L );

			  // Then exclusive locks should wait
			  Future<object> clientBLock = AcquireShared( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseExclusive( NODE, 1L );
			  ClientA.releaseShared( NODE, 1L );

			  // Then other thread should still wait
			  AssertWaiting( ClientB, clientBLock );

			  // But when
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sharedLocksShouldNotReplaceExclusiveLocks()
		 public virtual void SharedLocksShouldNotReplaceExclusiveLocks()
		 {
			  // When
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );

			  // Then shared locks should wait
			  Future<object> clientBLock = AcquireShared( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseShared( NODE, 1L );

			  // Then other thread should still wait
			  AssertWaiting( ClientB, clientBLock );

			  // But when
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpgradeAndDowngradeSameSharedLock()
		 public virtual void ShouldUpgradeAndDowngradeSameSharedLock()
		 {
			  // when
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientB.acquireShared( LockTracer.NONE, NODE, 1L );

			  LockIdentityExplorer sharedLockExplorer = new LockIdentityExplorer( NODE, 1L );
			  Locks.accept( sharedLockExplorer );

			  // then xclusive should wait for shared from other client to be released
			  Future<object> exclusiveLockFuture = AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // and when
			  ClientA.releaseShared( NODE, 1L );

			  // exclusive lock should be received
			  AssertNotWaiting( ClientB, exclusiveLockFuture );

			  // and when releasing exclusive
			  ClientB.releaseExclusive( NODE, 1L );

			  // we still should have same read lock
			  LockIdentityExplorer releasedLockExplorer = new LockIdentityExplorer( NODE, 1L );
			  Locks.accept( releasedLockExplorer );

			  // we still hold same lock as before
			  assertEquals( sharedLockExplorer.LockIdentityHashCode, releasedLockExplorer.LockIdentityHashCode );
		 }

		 private class LockIdentityExplorer : Locks_Visitor
		 {
			  internal readonly ResourceType ResourceType;
			  internal readonly long ResourceId;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long LockIdentityHashCodeConflict;

			  internal LockIdentityExplorer( ResourceType resourceType, long resourceId )
			  {
					this.ResourceType = resourceType;
					this.ResourceId = resourceId;
			  }

			  public override void Visit( ResourceType resourceType, long resourceId, string description, long estimatedWaitTime, long lockIdentityHashCode )
			  {
					if ( this.ResourceType.Equals( resourceType ) && this.ResourceId == resourceId )
					{
						 this.LockIdentityHashCodeConflict = lockIdentityHashCode;
					}
			  }

			  public virtual long LockIdentityHashCode
			  {
				  get
				  {
						return LockIdentityHashCodeConflict;
				  }
			  }
		 }
	}

}