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
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.locking.ResourceTypes.NODE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class CloseCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class CloseCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		 public CloseCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToHandOutClientsIfClosed()
		 public virtual void ShouldNotBeAbleToHandOutClientsIfClosed()
		 {
			  // GIVEN a lock manager and working clients
			  using ( Locks_Client client = Locks.newClient() )
			  {
					client.AcquireExclusive( LockTracer.NONE, ResourceTypes.Node, 0 );
			  }

			  // WHEN
			  Locks.close();

			  // THEN
			  try
			  {
					Locks.newClient();
					fail( "Should fail" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeShouldWaitAllOperationToFinish()
		 public virtual void CloseShouldWaitAllOperationToFinish()
		 {
			  // given
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
			  ClientA.acquireShared( LockTracer.NONE, NODE, 3L );
			  ClientB.acquireShared( LockTracer.NONE, NODE, 1L );
			  AcquireShared( ClientC, LockTracer.NONE, NODE, 2L );
			  AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();
			  AcquireExclusive( ClientC, LockTracer.NONE, NODE, 1L ).callAndAssertWaiting();

			  // when
			  ClientB.close();
			  ClientC.close();
			  ClientA.close();

			  // all locks should be closed at this point regardless of
			  // reader/writer waiter in any threads
			  // those should be gracefully finish and client should be closed

			  LockCountVisitor lockCountVisitor = new LockCountVisitor();
			  Locks.accept( lockCountVisitor );
			  assertEquals( 0, lockCountVisitor.LockCount );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void shouldNotBeAbleToAcquireSharedLockFromClosedClient()
		 public virtual void ShouldNotBeAbleToAcquireSharedLockFromClosedClient()
		 {
			  ClientA.close();
			  ClientA.acquireShared( LockTracer.NONE, NODE, 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void shouldNotBeAbleToAcquireExclusiveLockFromClosedClient()
		 public virtual void ShouldNotBeAbleToAcquireExclusiveLockFromClosedClient()
		 {
			  ClientA.close();
			  ClientA.acquireExclusive( LockTracer.NONE, NODE, 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void shouldNotBeAbleToTryAcquireSharedLockFromClosedClient()
		 public virtual void ShouldNotBeAbleToTryAcquireSharedLockFromClosedClient()
		 {
			  ClientA.close();
			  ClientA.trySharedLock( NODE, 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = LockClientStoppedException.class) public void shouldNotBeAbleToTryAcquireExclusiveLockFromClosedClient()
		 public virtual void ShouldNotBeAbleToTryAcquireExclusiveLockFromClosedClient()
		 {
			  ClientA.close();
			  ClientA.tryExclusiveLock( NODE, 1L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseTryLocksOnClose()
		 public virtual void ReleaseTryLocksOnClose()
		 {
			  assertTrue( ClientA.trySharedLock( ResourceTypes.Node, 1L ) );
			  assertTrue( ClientB.tryExclusiveLock( ResourceTypes.Node, 2L ) );

			  ClientA.close();
			  ClientB.close();

			  LockCountVisitor lockCountVisitor = new LockCountVisitor();
			  Locks.accept( lockCountVisitor );
			  assertEquals( 0, lockCountVisitor.LockCount );
		 }
	}

}