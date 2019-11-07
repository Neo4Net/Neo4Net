using System;

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
namespace Neo4Net.GraphDb
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using LockCountVisitor = Neo4Net.Kernel.impl.locking.LockCountVisitor;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using CommunityLockClient = Neo4Net.Kernel.impl.locking.community.CommunityLockClient;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.rootCause;

	public class GraphDatabaseShutdownTest
	{
		 private GraphDatabaseAPI _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.concurrent.OtherThreadRule<Void> t2 = new Neo4Net.test.rule.concurrent.OtherThreadRule<>("T2");
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>( "T2" );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.concurrent.OtherThreadRule<Void> t3 = new Neo4Net.test.rule.concurrent.OtherThreadRule<>("T3");
		 public readonly OtherThreadRule<Void> T3 = new OtherThreadRule<Void>( "T3" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _db = NewDb();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionShouldReleaseLocksWhenGraphDbIsBeingShutdown()
		 public virtual void TransactionShouldReleaseLocksWhenGraphDbIsBeingShutdown()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.locking.Locks locks = db.getDependencyResolver().resolveDependency(Neo4Net.kernel.impl.locking.Locks.class);
			  Locks locks = _db.DependencyResolver.resolveDependency( typeof( Locks ) );
			  assertEquals( 0, LockCount( locks ) );
			  Exception exceptionThrownByTxClose = null;

			  // WHEN
			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						Node node = _db.createNode();
						tx.AcquireWriteLock( node );
						assertThat( LockCount( locks ), greaterThanOrEqualTo( 1 ) );
      
						_db.shutdown();
      
						_db.createNode();
						tx.Success();
					  }
			  }
			  catch ( Exception e )
			  {
					exceptionThrownByTxClose = e;
			  }

			  // THEN
			  assertThat( exceptionThrownByTxClose, instanceOf( typeof( DatabaseShutdownException ) ) );
			  assertFalse( _db.isAvailable( 1 ) );
			  assertEquals( 0, LockCount( locks ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToShutdownWhenThereAreTransactionsWaitingForLocks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToShutdownWhenThereAreTransactionsWaitingForLocks()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Node node;
			  Node node;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					tx.Success();
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch nodeLockedLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent nodeLockedLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch shutdownCalled = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent shutdownCalled = new System.Threading.CountdownEvent( 1 );

			  // WHEN
			  // one thread locks previously created node and initiates graph db shutdown
			  Future<Void> shutdownFuture = T2.execute(state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 node.AddLabel( label( "ABC" ) );
					 nodeLockedLatch.Signal();

					 // Wait for T3 to start waiting for this node write lock
					 T3.get().waitUntilWaiting(details => details.isAt(typeof(CommunityLockClient), "acquireExclusive"));

					 _db.shutdown();

					 shutdownCalled.Signal();
					 tx.Success();
				}
				return null;
			  });

			  // other thread tries to lock the same node while it has been locked and graph db is being shutdown
			  Future<Void> secondTxResult = T3.execute(state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 nodeLockedLatch.await();

					 // T2 awaits this thread to get into a waiting state for this node write lock
					 node.AddLabel( label( "DEF" ) );

					 shutdownCalled.await();
					 tx.Success();
				}
				return null;
			  });

			  // start waiting when the trap has been triggered
			  try
			  {
					secondTxResult.get( 60, SECONDS );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( rootCause( e ), instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
			  try
			  {
					shutdownFuture.get();
					fail( "Should thrown exception since transaction should be canceled." );
			  }
			  catch ( Exception e )
			  {
					assertThat( rootCause( e ), instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
		 }

		 private static int LockCount( Locks locks )
		 {
			  LockCountVisitor lockCountVisitor = new LockCountVisitor();
			  locks.Accept( lockCountVisitor );
			  return lockCountVisitor.LockCount;
		 }

		 private GraphDatabaseAPI NewDb()
		 {
			  return ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().newGraphDatabase();
		 }
	}

}