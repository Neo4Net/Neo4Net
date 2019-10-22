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
namespace Neo4Net.Kernel.ha.@lock
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using LockRecord = Neo4Net.Kernel.ha.@lock.trace.LockRecord;
	using RecordingLockTracer = Neo4Net.Kernel.ha.@lock.trace.RecordingLockTracer;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using Tracers = Neo4Net.Kernel.monitoring.tracing.Tracers;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class SlaveStatementLocksFactoryIT
	{
		 private static readonly Label _testLabel = Label.label( "testLabel" );
		 private const string TEST_PROPERTY = "testProperty";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withSharedSetting(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.tracer, "slaveLocksTracer").withSharedSetting(org.Neo4Net.kernel.ha.HaSettings.tx_push_factor, "2");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withSharedSetting(GraphDatabaseSettings.tracer, "slaveLocksTracer").withSharedSetting(HaSettings.tx_push_factor, "2");
		 private ClusterManager.ManagedCluster _managedCluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _managedCluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void acquireSharedLocksDuringSlaveWriteTx()
		 public virtual void AcquireSharedLocksDuringSlaveWriteTx()
		 {
			  HighlyAvailableGraphDatabase anySlave = _managedCluster.AnySlave;
			  HighlyAvailableGraphDatabase master = _managedCluster.Master;

			  CreateSingleTestLabeledNode( master );

			  LockRecord sharedLabelLock = LockRecord.of( false, ResourceTypes.LABEL, 0 );
			  IList<LockRecord> requestedLocks = GetRequestedLocks( anySlave );
			  assertFalse( requestedLocks.Contains( sharedLabelLock ) );

			  CreateSingleTestLabeledNode( anySlave );

			  assertTrue( requestedLocks.Contains( sharedLabelLock ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotAcquireSharedLocksDuringSlaveReadTx()
		 public virtual void DoNotAcquireSharedLocksDuringSlaveReadTx()
		 {
			  HighlyAvailableGraphDatabase anySlave = _managedCluster.AnySlave;
			  HighlyAvailableGraphDatabase master = _managedCluster.Master;

			  using ( Transaction tx = master.BeginTx() )
			  {
					Node node = master.CreateNode( _testLabel );
					node.SetProperty( TEST_PROPERTY, "a" );
					tx.Success();
			  }

			  CreateIndex( master, _testLabel, TEST_PROPERTY );

			  using ( Transaction transaction = anySlave.BeginTx() )
			  {
					assertEquals( 1, Iterables.count( anySlave.Schema().getIndexes(_testLabel) ) );
					transaction.Success();
			  }
			  assertTrue( GetRequestedLocks( anySlave ).Count == 0 );
		 }

		 private void CreateSingleTestLabeledNode( HighlyAvailableGraphDatabase master )
		 {
			  using ( Transaction tx = master.BeginTx() )
			  {
					master.CreateNode( _testLabel );
					tx.Success();
			  }
		 }

		 private void CreateIndex( HighlyAvailableGraphDatabase master, Label label, string property )
		 {
			  using ( Transaction transaction = master.BeginTx() )
			  {
					master.Schema().indexFor(label).on(property).create();
					transaction.Success();
			  }

			  using ( Transaction transaction = master.BeginTx() )
			  {
					master.Schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					transaction.Success();
			  }
		 }

		 private IList<LockRecord> GetRequestedLocks( HighlyAvailableGraphDatabase master )
		 {
			  Tracers tracers = master.DependencyResolver.resolveDependency( typeof( Tracers ) );
			  RecordingLockTracer lockTracer = ( RecordingLockTracer ) tracers.LockTracer;
			  return lockTracer.RequestedLocks;
		 }

	}

}