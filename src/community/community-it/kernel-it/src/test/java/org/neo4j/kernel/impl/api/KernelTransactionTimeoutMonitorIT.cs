using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class KernelTransactionTimeoutMonitorIT
	{
		private bool InstanceFieldsInitialized = false;

		public KernelTransactionTimeoutMonitorIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Database = CreateDatabaseRule();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule database = createDatabaseRule();
		 public DatabaseRule Database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

		 private const int NODE_ID = 0;
		 private ExecutorService _executor;

		 protected internal virtual DatabaseRule CreateDatabaseRule()
		 {
			  return ( new EmbeddedDatabaseRule() ).withSetting(GraphDatabaseSettings.transaction_monitor_check_interval, "100ms");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _executor = Executors.newSingleThreadExecutor();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _executor.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 30_000) public void terminateExpiredTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateExpiredTransaction()
		 {
			  using ( Transaction transaction = Database.beginTx() )
			  {
					Database.createNode();
					transaction.Success();
			  }

			  ExpectedException.expectMessage( "The transaction has been terminated." );

			  using ( Transaction transaction = Database.beginTx() )
			  {
					Node nodeById = Database.getNodeById( NODE_ID );
					nodeById.SetProperty( "a", "b" );
					_executor.submit( StartAnotherTransaction() ).get();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 30_000) public void terminatingTransactionMustEagerlyReleaseTheirLocks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminatingTransactionMustEagerlyReleaseTheirLocks()
		 {
			  AtomicBoolean nodeLockAcquired = new AtomicBoolean();
			  AtomicBoolean lockerDone = new AtomicBoolean();
			  BinaryLatch lockerPause = new BinaryLatch();
			  long nodeId;
			  using ( Transaction tx = Database.beginTx() )
			  {
					nodeId = Database.createNode().Id;
					tx.Success();
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> locker = executor.submit(() ->
			  Future<object> locker = _executor.submit(() =>
			  {
				using ( Transaction tx = Database.beginTx() )
				{
					 Node node = Database.getNodeById( nodeId );
					 tx.AcquireReadLock( node );
					 nodeLockAcquired.set( true );
					 lockerPause.Await();
				}
				lockerDone.set( true );
			  });

			  bool proceed;
			  do
			  {
					proceed = nodeLockAcquired.get();
			  } while ( !proceed );

			  TerminateOngoingTransaction();

			  assertFalse( lockerDone.get() ); // but the thread should still be blocked on the latch
			  // Yet we should be able to proceed and grab the locks they once held
			  using ( Transaction tx = Database.beginTx() )
			  {
					// Write-locking is only possible if their shared lock was released
					tx.AcquireWriteLock( Database.getNodeById( nodeId ) );
					tx.Success();
			  }
			  // No exception from our lock client being stopped (e.g. we ended up blocked for too long) or from timeout
			  lockerPause.Release();
			  locker.get();
			  assertTrue( lockerDone.get() );
		 }

		 private void TerminateOngoingTransaction()
		 {
			  ISet<KernelTransactionHandle> kernelTransactionHandles = Database.resolveDependency( typeof( KernelTransactions ) ).activeTransactions();
			  assertThat( kernelTransactionHandles, hasSize( 1 ) );
			  foreach ( KernelTransactionHandle kernelTransactionHandle in kernelTransactionHandles )
			  {
					kernelTransactionHandle.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
			  }
		 }

		 private ThreadStart StartAnotherTransaction()
		 {
			  return () =>
			  {
				using ( InternalTransaction ignored = Database.beginTransaction( KernelTransaction.Type.@implicit, LoginContext.AUTH_DISABLED, 1, TimeUnit.SECONDS ) )
				{
					 Node node = Database.getNodeById( NODE_ID );
					 node.setProperty( "c", "d" );
				}
			  };
		 }
	}

}