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

	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.NODE;

	/// <summary>
	/// Tests simple acquiring and releasing of single locks.
	/// For testing "stacking" locks on the same client, see <seealso cref="LockReentrancyCompatibility"/>. 
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class AcquireAndReleaseLocksCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class AcquireAndReleaseLocksCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		 public AcquireAndReleaseLocksCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveShouldWaitForExclusive()
		 public virtual void ExclusiveShouldWaitForExclusive()
		 {
			  // When
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );

			  // Then
			  Future<object> clientBLock = AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then this should not block
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveShouldWaitForShared()
		 public virtual void ExclusiveShouldWaitForShared()
		 {
			  // When
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );

			  // Then other shared locks are allowed
			  ClientC.acquireShared( LockTracer.NONE, NODE, 1L );

			  // But exclusive locks should wait
			  Future<object> clientBLock = AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseShared( NODE, 1L );
			  ClientC.releaseShared( NODE, 1L );

			  // Then this should not block
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sharedShouldWaitForExclusive()
		 public virtual void SharedShouldWaitForExclusive()
		 {
			  // When
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );

			  // Then shared locks should wait
			  Future<object> clientBLock = AcquireShared( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // And when
			  ClientA.releaseExclusive( NODE, 1L );

			  // Then this should not block
			  AssertNotWaiting( ClientB, clientBLock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrySharedLock()
		 public virtual void ShouldTrySharedLock()
		 {
			  // Given I've grabbed a share lock
			  assertTrue( ClientA.trySharedLock( NODE, 1L ) );

			  // Then other clients can't have exclusive locks
			  assertFalse( ClientB.tryExclusiveLock( NODE, 1L ) );

			  // But they are allowed share locks
			  assertTrue( ClientB.trySharedLock( NODE, 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTryExclusiveLock()
		 public virtual void ShouldTryExclusiveLock()
		 {
			  // Given I've grabbed an exclusive lock
			  assertTrue( ClientA.tryExclusiveLock( NODE, 1L ) );

			  // Then other clients can't have exclusive locks
			  assertFalse( ClientB.tryExclusiveLock( NODE, 1L ) );

			  // Nor can they have share locks
			  assertFalse( ClientB.trySharedLock( NODE, 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTryUpgradeSharedToExclusive()
		 public virtual void ShouldTryUpgradeSharedToExclusive()
		 {
			  // Given I've grabbed an exclusive lock
			  assertTrue( ClientA.trySharedLock( NODE, 1L ) );

			  // Then I can upgrade it to exclusive
			  assertTrue( ClientA.tryExclusiveLock( NODE, 1L ) );

			  // And other clients are denied it
			  assertFalse( ClientB.trySharedLock( NODE, 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpgradeExclusiveOnTry()
		 public virtual void ShouldUpgradeExclusiveOnTry()
		 {
			  // Given I've grabbed a shared lock
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );

			  // When
			  assertTrue( ClientA.tryExclusiveLock( NODE, 1L ) );

			  // Then I should be able to release it
			  ClientA.releaseExclusive( NODE, 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireMultipleSharedLocks()
		 public virtual void ShouldAcquireMultipleSharedLocks()
		 {
			  ClientA.acquireShared( LockTracer.NONE, NODE, 10, 100, 1000 );

			  assertFalse( ClientB.tryExclusiveLock( NODE, 10 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 100 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 1000 ) );

			  assertEquals( 3, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireMultipleExclusiveLocks()
		 public virtual void ShouldAcquireMultipleExclusiveLocks()
		 {
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 10, 100, 1000 );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );

			  assertEquals( 3, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireMultipleAlreadyAcquiredSharedLocks()
		 public virtual void ShouldAcquireMultipleAlreadyAcquiredSharedLocks()
		 {
			  ClientA.acquireShared( LockTracer.NONE, NODE, 10, 100, 1000 );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 100, 1000, 10000 );

			  assertFalse( ClientB.tryExclusiveLock( NODE, 10 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 100 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 1000 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 10000 ) );

			  assertEquals( 4, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireMultipleAlreadyAcquiredExclusiveLocks()
		 public virtual void ShouldAcquireMultipleAlreadyAcquiredExclusiveLocks()
		 {
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 10, 100, 1000 );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 100, 1000, 10000 );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 10000 ) );

			  assertEquals( 4, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireMultipleSharedLocksWhileHavingSomeExclusiveLocks()
		 public virtual void ShouldAcquireMultipleSharedLocksWhileHavingSomeExclusiveLocks()
		 {
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 10, 100, 1000 );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 100, 1000, 10000 );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 10000 ) );

			  assertEquals( 4, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseSharedLocksAcquiredInABatch()
		 public virtual void ShouldReleaseSharedLocksAcquiredInABatch()
		 {
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1, 10, 100 );
			  assertEquals( 3, LockCount() );

			  ClientA.releaseShared( NODE, 1 );
			  assertEquals( 2, LockCount() );

			  ClientA.releaseShared( NODE, 10 );
			  assertEquals( 1, LockCount() );

			  ClientA.releaseShared( NODE, 100 );
			  assertEquals( 0, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseExclusiveLocksAcquiredInABatch()
		 public virtual void ShouldReleaseExclusiveLocksAcquiredInABatch()
		 {
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1, 10, 100 );
			  assertEquals( 3, LockCount() );

			  ClientA.releaseExclusive( NODE, 1 );
			  assertEquals( 2, LockCount() );

			  ClientA.releaseExclusive( NODE, 10 );
			  assertEquals( 1, LockCount() );

			  ClientA.releaseExclusive( NODE, 100 );
			  assertEquals( 0, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseMultipleSharedLocks()
		 public virtual void ReleaseMultipleSharedLocks()
		 {
			  ClientA.acquireShared( LockTracer.NONE, NODE, 10, 100, 1000 );
			  assertEquals( 3, LockCount() );

			  ClientA.releaseShared( NODE, 100, 1000 );
			  assertEquals( 1, LockCount() );

			  assertFalse( ClientB.tryExclusiveLock( NODE, 10 ) );
			  assertTrue( ClientB.tryExclusiveLock( NODE, 100 ) );
			  assertTrue( ClientB.tryExclusiveLock( NODE, 1000 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseMultipleExclusiveLocks()
		 public virtual void ReleaseMultipleExclusiveLocks()
		 {
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 10, 100, 1000 );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );
			  assertEquals( 3, LockCount() );

			  ClientA.releaseExclusive( NODE, 10, 100 );
			  assertEquals( 1, LockCount() );

			  assertTrue( ClientB.trySharedLock( NODE, 10 ) );
			  assertTrue( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseMultipleAlreadyAcquiredSharedLocks()
		 public virtual void ReleaseMultipleAlreadyAcquiredSharedLocks()
		 {
			  ClientA.acquireShared( LockTracer.NONE, NODE, 10, 100, 1000 );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 100, 1000, 10000 );

			  ClientA.releaseShared( NODE, 100, 1000 );
			  assertEquals( 4, LockCount() );

			  assertFalse( ClientB.tryExclusiveLock( NODE, 100 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 1000 ) );

			  ClientA.releaseShared( NODE, 100, 1000 );
			  assertEquals( 2, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseMultipleAlreadyAcquiredExclusiveLocks()
		 public virtual void ReleaseMultipleAlreadyAcquiredExclusiveLocks()
		 {
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 10, 100, 1000 );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 100, 1000, 10000 );

			  ClientA.releaseExclusive( NODE, 100, 1000 );
			  assertEquals( 4, LockCount() );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 10000 ) );

			  ClientA.releaseExclusive( NODE, 100, 1000 );

			  assertEquals( 2, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseSharedLocksAcquiredSeparately()
		 public virtual void ReleaseSharedLocksAcquiredSeparately()
		 {
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1 );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 2 );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 3 );
			  assertEquals( 3, LockCount() );

			  assertFalse( ClientB.tryExclusiveLock( NODE, 1 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 2 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 3 ) );

			  ClientA.releaseShared( NODE, 1, 2, 3 );

			  assertEquals( 0, LockCount() );
			  assertTrue( ClientB.tryExclusiveLock( NODE, 1 ) );
			  assertTrue( ClientB.tryExclusiveLock( NODE, 2 ) );
			  assertTrue( ClientB.tryExclusiveLock( NODE, 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseExclusiveLocksAcquiredSeparately()
		 public virtual void ReleaseExclusiveLocksAcquiredSeparately()
		 {
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1 );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 2 );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 3 );
			  assertEquals( 3, LockCount() );

			  assertFalse( ClientB.trySharedLock( NODE, 1 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 2 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 3 ) );

			  ClientA.releaseExclusive( NODE, 1, 2, 3 );

			  assertEquals( 0, LockCount() );
			  assertTrue( ClientB.trySharedLock( NODE, 1 ) );
			  assertTrue( ClientB.trySharedLock( NODE, 2 ) );
			  assertTrue( ClientB.trySharedLock( NODE, 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseMultipleSharedLocksWhileHavingSomeExclusiveLocks()
		 public virtual void ReleaseMultipleSharedLocksWhileHavingSomeExclusiveLocks()
		 {
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 10, 100, 1000 );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 100, 1000, 10000 );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 10000 ) );
			  assertEquals( 4, LockCount() );

			  ClientA.releaseShared( NODE, 100, 1000 );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 10000 ) );

			  assertEquals( 4, LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseMultipleExclusiveLocksWhileHavingSomeSharedLocks()
		 public virtual void ReleaseMultipleExclusiveLocksWhileHavingSomeSharedLocks()
		 {
			  ClientA.acquireShared( LockTracer.NONE, NODE, 100, 1000, 10000 );
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 10, 100, 1000 );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 100 ) );
			  assertFalse( ClientB.trySharedLock( NODE, 1000 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 10000 ) );
			  assertEquals( 4, LockCount() );

			  ClientA.releaseExclusive( NODE, 100, 1000 );

			  assertFalse( ClientB.trySharedLock( NODE, 10 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 100 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 1000 ) );
			  assertFalse( ClientB.tryExclusiveLock( NODE, 10000 ) );

			  assertEquals( 4, LockCount() );
		 }

		 private int LockCount()
		 {
			  LockCountVisitor lockVisitor = new LockCountVisitor();
			  Locks.accept( lockVisitor );
			  return lockVisitor.LockCount;
		 }
	}

}