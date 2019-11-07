using System;
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
namespace Neo4Net.GraphDb
{
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DeadlockDetectedException = Neo4Net.Kernel.DeadlockDetectedException;
	using DatabaseAvailability = Neo4Net.Kernel.availability.DatabaseAvailability;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using Barrier = Neo4Net.Test.Barrier;
	using Neo4Net.Test.OtherThreadExecutor;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.iterableWithSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class IGraphDatabaseServiceTest
	{
		private bool InstanceFieldsInitialized = false;

		public IGraphDatabaseServiceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Chain = RuleChain.outerRule( _testDirectory ).around( _exception ).around( _t2 ).around( _t3 );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final Neo4Net.test.rule.DatabaseRule globalDb = new Neo4Net.test.rule.ImpermanentDatabaseRule().withSetting(Neo4Net.graphdb.factory.GraphDatabaseSettings.shutdown_transaction_end_timeout, "10s");
		 public static readonly DatabaseRule GlobalDb = new ImpermanentDatabaseRule().withSetting(GraphDatabaseSettings.shutdown_transaction_end_timeout, "10s");

		 private readonly ExpectedException _exception = ExpectedException.none();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 private readonly OtherThreadRule<Void> _t2 = new OtherThreadRule<Void>( "T2-" + this.GetType().FullName );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 private readonly OtherThreadRule<Void> _t3 = new OtherThreadRule<Void>( "T3-" + this.GetType().FullName );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(testDirectory).around(exception).around(t2).around(t3);
		 public RuleChain Chain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenShutdownDatabaseWhenBeginTxThenExceptionIsThrown()
		 public virtual void GivenShutdownDatabaseWhenBeginTxThenExceptionIsThrown()
		 {
			  // Given
			  IGraphDatabaseService db = TemporaryDatabase;

			  Db.shutdown();

			  // Expect
			  _exception.expect( typeof( DatabaseShutdownException ) );

			  // When
			  Db.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenDatabaseAndStartedTxWhenShutdownThenWaitForTxToFinish() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GivenDatabaseAndStartedTxWhenShutdownThenWaitForTxToFinish()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IGraphDatabaseService db = getTemporaryDatabase();
			  IGraphDatabaseService db = TemporaryDatabase;

			  // When
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  Future<object> txFuture = _t2.execute(state =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 barrier.Reached();
					 Db.createNode();
					 tx.Success();
				}
				return null;
			  });

			  // i.e. wait for transaction to start
			  barrier.Await();

			  // now there's a transaction open, blocked on continueTxSignal
			  Future<object> shutdownFuture = _t3.execute(state =>
			  {
				Db.shutdown();
				return null;
			  });
			  _t3.get().waitUntilWaiting(location => location.isAt(typeof(DatabaseAvailability), "stop"));
			  barrier.Release();
			  try
			  {
					txFuture.get();
			  }
			  catch ( ExecutionException )
			  {
					// expected
			  }
			  shutdownFuture.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateTransactionThrowsExceptionOnNextOperation()
		 public virtual void TerminateTransactionThrowsExceptionOnNextOperation()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IGraphDatabaseService db = globalDb;
			  IGraphDatabaseService db = GlobalDb;

			  using ( Transaction tx = Db.beginTx() )
			  {
					tx.Terminate();
					try
					{
						 Db.createNode();
						 fail( "Failed to throw TransactionTerminateException" );
					}
					catch ( TransactionTerminatedException )
					{
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateNestedTransactionThrowsExceptionOnNextOperation()
		 public virtual void TerminateNestedTransactionThrowsExceptionOnNextOperation()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IGraphDatabaseService db = globalDb;
			  IGraphDatabaseService db = GlobalDb;

			  using ( Transaction tx = Db.beginTx() )
			  {
					using ( Transaction nested = Db.beginTx() )
					{
						 tx.Terminate();
					}
					try
					{
						 Db.createNode();
						 fail( "Failed to throw TransactionTerminateException" );
					}
					catch ( TransactionTerminatedException )
					{
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateNestedTransactionThrowsExceptionOnNextNestedOperation()
		 public virtual void TerminateNestedTransactionThrowsExceptionOnNextNestedOperation()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IGraphDatabaseService db = globalDb;
			  IGraphDatabaseService db = GlobalDb;

			  using ( Transaction tx = Db.beginTx() )
			  {
					using ( Transaction nested = Db.beginTx() )
					{
						 tx.Terminate();
						 try
						 {
							  Db.createNode();
							  fail( "Failed to throw TransactionTerminateException" );
						 }
						 catch ( TransactionTerminatedException )
						 {
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateNestedTransactionThrowsExceptionOnNextNestedOperationMultiThreadedVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateNestedTransactionThrowsExceptionOnNextNestedOperationMultiThreadedVersion()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IGraphDatabaseService db = getTemporaryDatabase();
			  IGraphDatabaseService db = TemporaryDatabase;
			  try
			  {
					// When
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch txSet = new java.util.concurrent.CountDownLatch(1);
					System.Threading.CountdownEvent txSet = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch terminated = new java.util.concurrent.CountDownLatch(1);
					System.Threading.CountdownEvent terminated = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Transaction[] outer = {null};
					Transaction[] outer = new Transaction[] { null };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Exception[] threadFail = {null};
					Exception[] threadFail = new Exception[] { null };

					Thread worker = new Thread(() =>
					{
					 try
					 {
						 using ( Transaction inner = Db.beginTx() )
						 {
							  outer[0] = inner;
							  txSet.Signal();
							  terminated.await();
							  Db.createNode();
							  fail( "should have failed earlier" );
						 }
					 }
					 catch ( Exception e )
					 {
						  threadFail[0] = e;
					 }
					});
					worker.Start();
					txSet.await();
					outer[0].Terminate();
					terminated.Signal();
					worker.Join();
					assertThat( threadFail[0], instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateNestedTransactionThrowsExceptionOnNextNestedOperationMultiThreadedVersionWithNestedTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateNestedTransactionThrowsExceptionOnNextNestedOperationMultiThreadedVersionWithNestedTx()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IGraphDatabaseService db = getTemporaryDatabase();
			  IGraphDatabaseService db = TemporaryDatabase;
			  try
			  {
					// When
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch txSet = new java.util.concurrent.CountDownLatch(1);
					System.Threading.CountdownEvent txSet = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch terminated = new java.util.concurrent.CountDownLatch(1);
					System.Threading.CountdownEvent terminated = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Transaction[] outer = {null};
					Transaction[] outer = new Transaction[] { null };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Exception[] threadFail = {null};
					Exception[] threadFail = new Exception[] { null };

					Thread worker = new Thread(() =>
					{
					 Transaction transaction = Db.beginTx();
					 try
					 {
						 using ( Transaction inner = Db.beginTx() )
						 {
							  outer[0] = inner;
							  txSet.Signal();
							  terminated.await();
							  Db.createNode();
							  fail( "should have failed earlier" );
						 }
					 }
					 catch ( Exception e )
					 {
						  threadFail[0] = e;
					 }
					 finally
					 {
						  transaction.Close();
					 }
					});
					worker.Start();
					txSet.await();
					outer[0].Terminate();
					terminated.Signal();
					worker.Join();
					assertThat( threadFail[0], instanceOf( typeof( TransactionTerminatedException ) ) );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenDatabaseAndStartedTxWhenShutdownAndStartNewTxThenBeginTxTimesOut() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GivenDatabaseAndStartedTxWhenShutdownAndStartNewTxThenBeginTxTimesOut()
		 {
			  // Given
			  IGraphDatabaseService db = TemporaryDatabase;

			  // When
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  _t2.execute(state =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 barrier.Reached(); // <-- this triggers t3 to start a db.shutdown()
				}
				return null;
			  });

			  barrier.Await();
			  Future<object> shutdownFuture = _t3.execute(state =>
			  {
				Db.shutdown();
				return null;
			  });
			  _t3.get().waitUntilWaiting(location => location.isAt(typeof(DatabaseAvailability), "stop"));
			  barrier.Release(); // <-- this triggers t2 to continue its transaction
			  shutdownFuture.get();

			  try
			  {
					Db.beginTx();
					fail( "Should fail" );
			  }
			  catch ( DatabaseShutdownException )
			  {
					//THEN good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLetDetectedDeadlocksDuringCommitBeThrownInTheirOriginalForm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLetDetectedDeadlocksDuringCommitBeThrownInTheirOriginalForm()
		 {
			  // GIVEN a database with a couple of entities:
			  // (n1) --> (r1) --> (r2) --> (r3)
			  // (n2)
			  IGraphDatabaseService db = GlobalDb;
			  Node n1 = CreateNode( db );
			  Node n2 = CreateNode( db );
			  Relationship r3 = CreateRelationship( n1 );
			  Relationship r2 = CreateRelationship( n1 );
			  Relationship r1 = CreateRelationship( n1 );

			  // WHEN creating a deadlock scenario where the final deadlock would have happened due to locks
			  //      acquired during linkage of relationship records
			  //
			  // (r1) <-- (t1)
			  //   |       ^
			  //   v       |
			  // (t2) --> (n2)
			  Transaction t1Tx = Db.beginTx();
			  Transaction t2Tx = _t2.execute( BeginTx( db ) ).get();
			  // (t1) <-- (n2)
			  n2.SetProperty( "locked", "indeed" );
			  // (t2) <-- (r1)
			  _t2.execute( SetProperty( r1, "locked", "absolutely" ) ).get();
			  // (t2) --> (n2)
			  Future<object> t2n2Wait = _t2.execute( SetProperty( n2, "locked", "In my dreams" ) );
			  _t2.get().waitUntilWaiting();
			  // (t1) --> (r1) although delayed until commit, this is accomplished by deleting an adjacent
			  //               relationship so that its surrounding relationships are locked at commit time.
			  r2.Delete();
			  t1Tx.Success();
			  try
			  {
					t1Tx.Close();
					fail( "Should throw exception about deadlock" );
			  }
			  catch ( Exception e )
			  {
					assertEquals( typeof( DeadlockDetectedException ), e.GetType() );
			  }
			  finally
			  {
					t2n2Wait.get();
					_t2.execute( Close( t2Tx ) ).get();
			  }
		 }

		 /// <summary>
		 /// GitHub issue #5996
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminationOfClosedTransactionDoesNotInfluenceNextTransaction()
		 public virtual void TerminationOfClosedTransactionDoesNotInfluenceNextTransaction()
		 {
			  IGraphDatabaseService db = GlobalDb;

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  Transaction transaction = Db.beginTx();
			  using ( Transaction tx = transaction )
			  {
					Db.createNode();
					tx.Success();
			  }
			  transaction.Terminate();

			  using ( Transaction tx = Db.beginTx() )
			  {
					assertThat( Db.AllNodes, @is( iterableWithSize( 2 ) ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.test.OtherThreadExecutor.WorkerCommand<Void, Transaction> beginTx(final IGraphDatabaseService db)
		 private WorkerCommand<Void, Transaction> BeginTx( IGraphDatabaseService db )
		 {
			  return state => Db.beginTx();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.test.OtherThreadExecutor.WorkerCommand<Void, Object> setProperty(final IPropertyContainer IEntity, final String key, final String value)
		 private WorkerCommand<Void, object> SetProperty( IPropertyContainer IEntity, string key, string value )
		 {
			  return state =>
			  {
				entity.SetProperty( key, value );
				return null;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.test.OtherThreadExecutor.WorkerCommand<Void, Void> close(final Transaction tx)
		 private WorkerCommand<Void, Void> Close( Transaction tx )
		 {
			  return state =>
			  {
				tx.Close();
				return null;
			  };
		 }

		 private Relationship CreateRelationship( Node node )
		 {
			  using ( Transaction tx = node.GraphDatabase.beginTx() )
			  {
					Relationship rel = node.CreateRelationshipTo( node, MyRelTypes.TEST );
					tx.Success();
					return rel;
			  }
		 }

		 private Node CreateNode( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					tx.Success();
					return node;
			  }
		 }

		 private IGraphDatabaseService TemporaryDatabase
		 {
			 get
			 {
				  return ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder(_testDirectory.directory("impermanent")).setConfig(GraphDatabaseSettings.shutdown_transaction_end_timeout, "10s").newGraphDatabase();
			 }
		 }
	}

}