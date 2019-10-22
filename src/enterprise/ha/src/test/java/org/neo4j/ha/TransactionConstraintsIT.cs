using System;
using System.Threading;

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
namespace Neo4Net.ha
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using InstanceId = Neo4Net.cluster.InstanceId;
	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Lock = Neo4Net.GraphDb.Lock;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using NotInTransactionException = Neo4Net.GraphDb.NotInTransactionException;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using TransientTransactionFailureException = Neo4Net.GraphDb.TransientTransactionFailureException;
	using DeadlockDetectedException = Neo4Net.Kernel.DeadlockDetectedException;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using Neo4Net.Test;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;
	using DumpProcessInformationRule = Neo4Net.Test.rule.dump.DumpProcessInformationRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.dump.DumpProcessInformationRule.localVm;

	public class TransactionConstraintsIT
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionConstraintsIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _dumpInfo ).around( _exception );
		}

		 private static readonly int _slaveOnlyId = ClusterManager.FIRST_SERVER_ID + 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withSharedSetting(org.Neo4Net.kernel.ha.HaSettings.pull_interval, "0").withInstanceSetting(org.Neo4Net.kernel.ha.HaSettings.slave_only, serverId -> serverId == SLAVE_ONLY_ID ? "true" : "false");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withSharedSetting(HaSettings.pull_interval, "0").withInstanceSetting(HaSettings.slave_only, serverId => serverId == _slaveOnlyId ? "true" : "false");

		 private DumpProcessInformationRule _dumpInfo = new DumpProcessInformationRule( 1, MINUTES, localVm( System.out ) );
		 private ExpectedException _exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(dumpInfo).around(exception);
		 public RuleChain RuleChain;

		 protected internal ClusterManager.ManagedCluster Cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Cluster = ClusterRule.startCluster();
		 }

		 private const string PROPERTY_KEY = "name";
		 private const string PROPERTY_VALUE = "yo";
		 private static readonly Label _label = Label.label( "Person" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startTxAsSlaveAndFinishItAfterHavingSwitchedToMasterShouldNotSucceed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartTxAsSlaveAndFinishItAfterHavingSwitchedToMasterShouldNotSucceed()
		 {
			  // GIVEN
			  IGraphDatabaseService db = Cluster.getAnySlave( SlaveOnlySlave );

			  // WHEN
			  Transaction tx = Db.beginTx();
			  try
			  {
					Db.createNode().setProperty("name", "slave");
					tx.Success();
			  }
			  finally
			  {
					HighlyAvailableGraphDatabase oldMaster = Cluster.Master;
					Cluster.shutdown( oldMaster );
					// Wait for new master
					Cluster.await( masterAvailable( oldMaster ) );
					AssertFinishGetsTransactionFailure( tx );
			  }

			  // THEN
			  assertEquals( db, Cluster.Master );
			  // to prevent a deadlock scenario which occurs if this test exists (and @After starts)
			  // before the db has recovered from its KERNEL_PANIC
			  AwaitFullyOperational( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startTxAsSlaveAndFinishItAfterAnotherMasterBeingAvailableShouldNotSucceed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartTxAsSlaveAndFinishItAfterAnotherMasterBeingAvailableShouldNotSucceed()
		 {
			  // GIVEN
			  HighlyAvailableGraphDatabase db = SlaveOnlySlave;
			  HighlyAvailableGraphDatabase oldMaster;

			  // WHEN
			  Transaction tx = Db.beginTx();
			  try
			  {
					Db.createNode().setProperty("name", "slave");
					tx.Success();
			  }
			  finally
			  {
					oldMaster = Cluster.Master;
					Cluster.shutdown( oldMaster );
					// Wait for new master
					Cluster.await( masterAvailable( oldMaster ) );
					// THEN
					AssertFinishGetsTransactionFailure( tx );
			  }

			  assertFalse( Db.Master );
			  assertFalse( oldMaster.Master );
			  // to prevent a deadlock scenario which occurs if this test exists (and @After starts)
			  // before the db has recovered from its KERNEL_PANIC
			  AwaitFullyOperational( db );
		 }

		 private HighlyAvailableGraphDatabase SlaveOnlySlave
		 {
			 get
			 {
				  HighlyAvailableGraphDatabase db = Cluster.getMemberByServerId( new InstanceId( _slaveOnlyId ) );
				  assertEquals( _slaveOnlyId, Cluster.getServerId( db ).toIntegerIndex() );
				  assertFalse( Db.Master );
				  return db;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveShouldNotBeAbleToProduceAnInvalidTransaction()
		 public virtual void SlaveShouldNotBeAbleToProduceAnInvalidTransaction()
		 {
			  // GIVEN
			  HighlyAvailableGraphDatabase aSlave = Cluster.AnySlave;
			  Node node = CreateMiniTree( aSlave );

			  Transaction tx = aSlave.BeginTx();
			  // Deleting this node isn't allowed since it still has relationships
			  node.Delete();
			  tx.Success();

			  // EXPECT
			  _exception.expect( typeof( ConstraintViolationException ) );

			  // WHEN
			  tx.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterShouldNotBeAbleToProduceAnInvalidTransaction()
		 public virtual void MasterShouldNotBeAbleToProduceAnInvalidTransaction()
		 {
			  // GIVEN
			  HighlyAvailableGraphDatabase master = Cluster.Master;
			  Node node = CreateMiniTree( master );

			  Transaction tx = master.BeginTx();
			  // Deleting this node isn't allowed since it still has relationships
			  node.Delete();
			  tx.Success();

			  // EXPECT
			  _exception.expect( typeof( ConstraintViolationException ) );

			  // WHEN
			  tx.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeOperationOnSlaveHasToBePerformedWithinTransaction()
		 public virtual void WriteOperationOnSlaveHasToBePerformedWithinTransaction()
		 {
			  // GIVEN
			  HighlyAvailableGraphDatabase aSlave = Cluster.AnySlave;

			  // WHEN
			  try
			  {
					aSlave.CreateNode();
					fail( "Shouldn't be able to do a write operation outside a transaction" );
			  }
			  catch ( NotInTransactionException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeOperationOnMasterHasToBePerformedWithinTransaction()
		 public virtual void WriteOperationOnMasterHasToBePerformedWithinTransaction()
		 {
			  // GIVEN
			  HighlyAvailableGraphDatabase master = Cluster.Master;

			  // WHEN
			  try
			  {
					master.CreateNode();
					fail( "Shouldn't be able to do a write operation outside a transaction" );
			  }
			  catch ( NotInTransactionException )
			  {
					// THEN
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveShouldNotBeAbleToModifyNodeDeletedOnMaster()
		 public virtual void SlaveShouldNotBeAbleToModifyNodeDeletedOnMaster()
		 {
			  // GIVEN
			  // -- node created on slave
			  HighlyAvailableGraphDatabase aSlave = Cluster.AnySlave;
			  Node node = CreateNode( aSlave, PROPERTY_VALUE );
			  // -- that node delete on master, but the slave doesn't see it yet
			  DeleteNode( Cluster.Master, node.Id );

			  // WHEN
			  try
			  {
					  using ( Transaction slaveTransaction = aSlave.BeginTx() )
					  {
						node.SetProperty( "name", "test" );
						fail( "Shouldn't be able to modify a node deleted on master" );
					  }
			  }
			  catch ( NotFoundException )
			  {
					// THEN
					// -- the transactions gotten back in the response should delete that node
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deadlockDetectionInvolvingTwoSlaves() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeadlockDetectionInvolvingTwoSlaves()
		 {
			  HighlyAvailableGraphDatabase slave1 = Cluster.AnySlave;
			  DeadlockDetectionBetween( slave1, Cluster.getAnySlave( slave1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deadlockDetectionInvolvingSlaveAndMaster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeadlockDetectionInvolvingSlaveAndMaster()
		 {
			  DeadlockDetectionBetween( Cluster.AnySlave, Cluster.Master );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deadlockDetectionBetween(org.Neo4Net.kernel.ha.HighlyAvailableGraphDatabase slave1, final org.Neo4Net.kernel.ha.HighlyAvailableGraphDatabase slave2) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void DeadlockDetectionBetween( HighlyAvailableGraphDatabase slave1, HighlyAvailableGraphDatabase slave2 )
		 {
			  // GIVEN
			  // -- two members acquiring a read lock on the same IEntity
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node commonNode;
			  Node commonNode;
			  using ( Transaction tx = slave1.BeginTx() )
			  {
					commonNode = slave1.CreateNode();
					tx.Success();
			  }

			  OtherThreadExecutor<HighlyAvailableGraphDatabase> thread2 = new OtherThreadExecutor<HighlyAvailableGraphDatabase>( "T2", slave2 );
			  Transaction tx1 = slave1.BeginTx();
			  Transaction tx2 = thread2.Execute( new BeginTx() );
			  tx1.AcquireReadLock( commonNode );
			  thread2.Execute( state => tx2.AcquireReadLock( commonNode ) );
			  // -- and one of them wanting (and awaiting) to upgrade its read lock to a write lock
			  Future<Lock> writeLockFuture = thread2.ExecuteDontWait(state =>
			  {
				using ( Transaction ignored = tx2 ) // Close transaction no matter what happens
				{
					 return tx2.AcquireWriteLock( commonNode );
				}
			  });

			  for ( int i = 0; i < 10; i++ )
			  {
					thread2.WaitUntilThreadState( Thread.State.TIMED_WAITING, Thread.State.WAITING );
					Thread.Sleep( 2 );
			  }

			  try // Close transaction no matter what happens
			  {
					  using ( Transaction ignored = tx1 )
					  {
						// WHEN
						tx1.AcquireWriteLock( commonNode );
      
						// -- Deadlock detection is non-deterministic, so either the slave or the master will detect it
						writeLockFuture.get();
						fail( "Deadlock exception should have been thrown" );
					  }
			  }
			  catch ( DeadlockDetectedException )
			  {
					// THEN -- deadlock should be avoided with this exception
			  }
			  catch ( ExecutionException e )
			  {
					// OR -- the tx2 thread fails with executionexception, caused by deadlock on its end
					assertThat( e.InnerException, instanceOf( typeof( DeadlockDetectedException ) ) );
			  }

			  thread2.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdSchemaConstraintsMustBeRetainedAcrossModeSwitches() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatedSchemaConstraintsMustBeRetainedAcrossModeSwitches()
		 {
			  // GIVEN
			  // -- a node with a label and a property, and a constraint on those
			  HighlyAvailableGraphDatabase master = Cluster.Master;
			  CreateConstraint( master, _label, PROPERTY_KEY );
			  CreateNode( master, PROPERTY_VALUE, _label ).Id;

			  // WHEN
			  Cluster.sync();
			  ClusterManager.RepairKit originalMasterRepairKit = Cluster.fail( master );
			  Cluster.await( masterAvailable( master ) );
			  TakeTheLeadInAnEventualMasterSwitch( Cluster.Master );
			  Cluster.sync();

			  originalMasterRepairKit.Repair();
			  Cluster.await( allSeesAllAsAvailable() );
			  Cluster.sync();

			  // THEN the constraints should still be in place and enforced
			  int i = 0;
			  foreach ( HighlyAvailableGraphDatabase instance in Cluster.AllMembers )
			  {
					try
					{
						 CreateNode( instance, PROPERTY_VALUE, _label );
						 fail( "Node with " + PROPERTY_VALUE + " should already exist" );
					}
					catch ( ConstraintViolationException )
					{
						 // Good, this node should already exist
					}
					for ( int p = 0; p < i - 1; p++ )
					{
						 try
						 {
							  CreateNode( instance, PROPERTY_VALUE + p.ToString(), _label );
							  fail( "Node with " + PROPERTY_VALUE + p.ToString() + " should already exist" );
						 }
						 catch ( ConstraintViolationException )
						 {
							  // Good
						 }
					}

					CreateNode( instance, PROPERTY_VALUE + i.ToString(), _label );
					i++;
			  }
		 }

		 private void TakeTheLeadInAnEventualMasterSwitch( IGraphDatabaseService db )
		 {
			  CreateNode( db, PROPERTY_VALUE );
		 }

		 private Node CreateNode( IGraphDatabaseService db, object propertyValue, params Label[] labels )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					foreach ( Label label in labels )
					{
						 node.AddLabel( label );
					}
					node.SetProperty( PROPERTY_KEY, propertyValue );
					tx.Success();
					return node;
			  }
		 }

		 private void CreateConstraint( HighlyAvailableGraphDatabase db, Label label, string propertyName )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(label).assertPropertyIsUnique(propertyName).create();
					tx.Success();
			  }
		 }

		 private Node CreateMiniTree( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node root = Db.createNode();
					root.CreateRelationshipTo( Db.createNode(), MyRelTypes.TEST );
					root.CreateRelationshipTo( Db.createNode(), MyRelTypes.TEST );
					tx.Success();
					return root;
			  }
		 }

		 private void DeleteNode( HighlyAvailableGraphDatabase db, long id )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getNodeById( id ).delete();
					tx.Success();
			  }
		 }

		 private void AssertFinishGetsTransactionFailure( Transaction tx )
		 {
			  try
			  {
					tx.Close();
					fail( "Transaction shouldn't be able to finish" );
			  }
			  catch ( Exception e ) when ( e is TransientTransactionFailureException || e is TransactionFailureException )
			  { // Good
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitFullyOperational(org.Neo4Net.graphdb.GraphDatabaseService db) throws InterruptedException
		 private void AwaitFullyOperational( IGraphDatabaseService db )
		 {
			  long endTime = currentTimeMillis() + MINUTES.toMillis(1);
			  for ( int i = 0; currentTimeMillis() < endTime; i++ )
			  {
					try
					{
						 DoABogusTransaction( db );
						 break;
					}
					catch ( Exception e )
					{
						 if ( i > 0 && i % 10 == 0 )
						 {
							  Console.WriteLine( e.ToString() );
							  Console.Write( e.StackTrace );
						 }
						 Thread.Sleep( 1000 );
					}
			  }
		 }

		 private void DoABogusTransaction( IGraphDatabaseService db )
		 {
			  using ( Transaction ignore = Db.beginTx() )
			  {
					Db.createNode();
			  }
		 }
	}

}