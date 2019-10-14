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
namespace Neo4Net.Kernel.impl.locking.community
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using Mockito = org.mockito.Mockito;

	using Config = Neo4Net.Kernel.configuration.Config;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class LockManagerImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowGetReadWriteLocks()
		 public virtual void ShouldAllowGetReadWriteLocks()
		 {
			  // given
			  LockResource node1 = new LockResource( ResourceTypes.NODE, 1L );
			  LockResource node2 = new LockResource( ResourceTypes.NODE, 2L );
			  LockTransaction lockTransaction = new LockTransaction();
			  LockManagerImpl lockManager = CreateLockManager();

			  // expect
			  assertTrue( lockManager.GetReadLock( LockTracer.NONE, node1, lockTransaction ) );
			  assertTrue( lockManager.GetReadLock( LockTracer.NONE, node2, lockTransaction ) );
			  assertTrue( lockManager.GetWriteLock( LockTracer.NONE, node2, lockTransaction ) );

			  lockManager.ReleaseReadLock( node1, lockTransaction );
			  lockManager.ReleaseReadLock( node2, lockTransaction );
			  lockManager.ReleaseWriteLock( node2, lockTransaction );

			  int lockCount = CountLocks( lockManager );
			  assertEquals( 0, lockCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleReleaseNotExistingLock()
		 public virtual void ShouldNotBePossibleReleaseNotExistingLock()
		 {
			  // given
			  LockResource node1 = new LockResource( ResourceTypes.NODE, 1L );
			  LockTransaction lockTransaction = new LockTransaction();
			  LockManagerImpl lockManager = CreateLockManager();

			  // expect
			  ExpectedException.expect( typeof( LockNotFoundException ) );
			  ExpectedException.expectMessage( "Lock not found for: " );

			  // when
			  lockManager.ReleaseReadLock( node1, lockTransaction );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanupNotUsedLocks()
		 public virtual void ShouldCleanupNotUsedLocks()
		 {
			  // given
			  LockResource node = new LockResource( ResourceTypes.NODE, 1L );
			  LockTransaction lockTransaction = new LockTransaction();
			  LockManagerImpl lockManager = CreateLockManager();
			  lockManager.GetWriteLock( LockTracer.NONE, node, lockTransaction );

			  // expect
			  assertTrue( lockManager.TryReadLock( node, lockTransaction ) );
			  assertEquals( 1, CountLocks( lockManager ) );

			  // and when
			  lockManager.ReleaseWriteLock( node, lockTransaction );

			  // expect to see one old reader
			  assertEquals( 1, CountLocks( lockManager ) );

			  // and when
			  lockManager.ReleaseReadLock( node, lockTransaction );

			  // no more locks left
			  assertEquals( 0, CountLocks( lockManager ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseNotAcquiredLocks()
		 public virtual void ShouldReleaseNotAcquiredLocks()
		 {

			  // given
			  LockResource node = new LockResource( ResourceTypes.NODE, 1L );
			  LockTransaction lockTransaction = new LockTransaction();
			  RWLock rwLock = Mockito.mock( typeof( RWLock ) );
			  LockManagerImpl lockManager = new MockedLockLockManager( this, new RagManager(), rwLock );

			  // expect
			  lockManager.TryReadLock( node, lockTransaction );

			  // during client close any of the attempts to get read/write lock can be scheduled as last one
			  // in that case lock will hot have marks, readers, writers anymore and optimistically created lock
			  // need to be removed from global map resource map
			  assertEquals( 0, CountLocks( lockManager ) );
		 }

		 private LockManagerImpl CreateLockManager()
		 {
			  return new LockManagerImpl( new RagManager(), Config.defaults(), Clocks.systemClock() );
		 }

		 private int CountLocks( LockManagerImpl lockManager )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] counter = new int[1];
			  int[] counter = new int[1];
			  lockManager.Accept(element =>
			  {
				counter[0]++;
				return false;
			  });
			  return counter[0];
		 }

		 private class MockedLockLockManager : LockManagerImpl
		 {
			 private readonly LockManagerImplTest _outerInstance;


			  internal RWLock Lock;

			  internal MockedLockLockManager( LockManagerImplTest outerInstance, RagManager ragManager, RWLock @lock ) : base( ragManager, Config.defaults(), Clocks.systemClock() )
			  {
				  this._outerInstance = outerInstance;
					this.Lock = @lock;
			  }

			  protected internal override RWLock CreateLock( LockResource resource )
			  {
					return Lock;
			  }
		 }

	}

}