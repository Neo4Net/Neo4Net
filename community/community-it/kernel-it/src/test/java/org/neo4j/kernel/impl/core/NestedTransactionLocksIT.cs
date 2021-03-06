﻿/*
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
namespace Org.Neo4j.Kernel.impl.core
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Lock = Org.Neo4j.Graphdb.Lock;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Test;
	using Org.Neo4j.Test.OtherThreadExecutor;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	/// <summary>
	/// Confirms that a nested <seealso cref="Transaction"/> can grab locks with its
	/// explicit methods: <seealso cref="Transaction.acquireReadLock(org.neo4j.graphdb.PropertyContainer) acquireReadLock"/>
	/// and <seealso cref="Transaction.acquireWriteLock(org.neo4j.graphdb.PropertyContainer) acquireWriteLock"/>.
	/// </summary>
	public class NestedTransactionLocksIT
	{
		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.test.OtherThreadExecutor.WorkerCommand<Void, org.neo4j.graphdb.Lock> acquireWriteLock(final org.neo4j.graphdb.Node resource)
		 private OtherThreadExecutor.WorkerCommand<Void, Lock> AcquireWriteLock( Node resource )
		 {
			  return state =>
			  {
				using ( Transaction tx = _db.beginTx() )
				{
					 return tx.acquireWriteLock( resource );
				}
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nestedTransactionCanAcquireLocksFromTransactionObject() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NestedTransactionCanAcquireLocksFromTransactionObject()
		 {
			  // given
			  Node resource = CreateNode();

			  using ( Transaction outerTx = _db.beginTx(), Transaction nestedTx = _db.beginTx() )
			  {
					assertNotSame( outerTx, nestedTx );

					using ( OtherThreadExecutor<Void> otherThread = new OtherThreadExecutor<Void>( "other thread", null ) )
					{
						 // when
						 Lock @lock = nestedTx.AcquireWriteLock( resource );
						 Future<Lock> future = TryToAcquireSameLockOnAnotherThread( resource, otherThread );

						 // then
						 AcquireOnOtherThreadTimesOut( future );

						 // and when
						 @lock.Release();

						 //then
						 assertNotNull( future.get() );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void acquireOnOtherThreadTimesOut(java.util.concurrent.Future<org.neo4j.graphdb.Lock> future) throws InterruptedException, java.util.concurrent.ExecutionException
		 private void AcquireOnOtherThreadTimesOut( Future<Lock> future )
		 {
			  try
			  {
					future.get( 1, SECONDS );
					fail( "The nested transaction seems to not have acquired the lock" );
			  }
			  catch ( TimeoutException )
			  { // Good
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.concurrent.Future<org.neo4j.graphdb.Lock> tryToAcquireSameLockOnAnotherThread(org.neo4j.graphdb.Node resource, org.neo4j.test.OtherThreadExecutor<Void> otherThread) throws Exception
		 private Future<Lock> TryToAcquireSameLockOnAnotherThread( Node resource, OtherThreadExecutor<Void> otherThread )
		 {
			  Future<Lock> future = otherThread.ExecuteDontWait( AcquireWriteLock( resource ) );
			  otherThread.WaitUntilWaiting();
			  return future;
		 }

		 private Node CreateNode()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node n = _db.createNode();
					tx.Success();
					return n;
			  }
		 }
	}

}