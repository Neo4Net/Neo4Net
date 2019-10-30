using System.Collections.Generic;
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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using UpdatePuller = Neo4Net.Kernel.ha.UpdatePuller;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.HaSettings.tx_push_factor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.SlaveUpdatePuller.UPDATE_PULLER_THREAD_PREFIX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.masterAvailable;

	public class UpdatePullerSwitchIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withCluster(clusterOfSize(2)).withSharedSetting(tx_push_factor, "0").withSharedSetting(org.Neo4Net.kernel.ha.HaSettings.pull_interval, "100s").withFirstInstanceId(6);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withCluster(clusterOfSize(2)).withSharedSetting(tx_push_factor, "0").withSharedSetting(HaSettings.pull_interval, "100s").withFirstInstanceId(6);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updatePullerSwitchOnNodeModeSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpdatePullerSwitchOnNodeModeSwitch()
		 {
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();

			  Label firstLabel = Label.label( "firstLabel" );
			  CreateLabeledNodeOnMaster( cluster, firstLabel );
			  // force update puller to work
			  PullUpdatesOnSlave( cluster );
			  // node should exist on slave now
			  CheckLabeledNodeExistenceOnSlave( cluster, firstLabel );
			  // verify that puller working on slave and not working on master
			  VerifyUpdatePullerThreads( cluster );

			  for ( int i = 1; i <= 2; i++ )
			  {
					// switch roles in cluster - now update puller should be stopped on old slave and start on old master.
					ClusterManager.RepairKit repairKit = cluster.Shutdown( cluster.Master );
					cluster.Await( masterAvailable() );

					Label currentLabel = Label.label( "label_" + i );

					CreateLabeledNodeOnMaster( cluster, currentLabel );

					repairKit.Repair();
					cluster.Await( allSeesAllAsAvailable(), 120 );

					// forcing updates pulling
					PullUpdatesOnSlave( cluster );
					CheckLabeledNodeExistenceOnSlave( cluster, currentLabel );
					// checking pulling threads
					VerifyUpdatePullerThreads( cluster );
			  }
		 }

		 private void VerifyUpdatePullerThreads( ClusterManager.ManagedCluster cluster )
		 {
			  IDictionary<Thread, StackTraceElement[]> threads = Thread.AllStackTraces;
			  Optional<KeyValuePair<Thread, StackTraceElement[]>> masterEntry = FindThreadWithPrefix( threads, UPDATE_PULLER_THREAD_PREFIX + ServerId( cluster.Master ) );
			  assertFalse( format( "Found an update puller on master.%s", masterEntry.map( this.prettyPrint ).orElse( "" ) ), masterEntry.Present );

			  Optional<KeyValuePair<Thread, StackTraceElement[]>> slaveEntry = FindThreadWithPrefix( threads, UPDATE_PULLER_THREAD_PREFIX + ServerId( cluster.AnySlave ) );
			  assertTrue( "Found no update puller on slave", slaveEntry.Present );
		 }

		 private string PrettyPrint( KeyValuePair<Thread, StackTraceElement[]> entry )
		 {
			  return format( "\n\tThread: %s\n\tStackTrace: %s", entry.Key, Arrays.ToString( entry.Value ) );
		 }

		 private InstanceId ServerId( HighlyAvailableGraphDatabase db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( Config ) ).get( ClusterSettings.server_id );
		 }

		 private Optional<KeyValuePair<Thread, StackTraceElement[]>> FindThreadWithPrefix( IDictionary<Thread, StackTraceElement[]> threads, string prefix )
		 {
			  return threads.SetOfKeyValuePairs().Where(entry => entry.Key.Name.StartsWith(prefix)).First();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void pullUpdatesOnSlave(org.Neo4Net.kernel.impl.ha.ClusterManager.ManagedCluster cluster) throws InterruptedException
		 private void PullUpdatesOnSlave( ClusterManager.ManagedCluster cluster )
		 {
			  UpdatePuller updatePuller = cluster.AnySlave.DependencyResolver.resolveDependency( typeof( UpdatePuller ) );
			  assertTrue( "We should always have some updates to pull", updatePuller.TryPullUpdates() );
		 }

		 private void CheckLabeledNodeExistenceOnSlave( ClusterManager.ManagedCluster cluster, Label label )
		 {
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;
			  using ( Transaction transaction = slave.BeginTx() )
			  {
					ResourceIterator<Node> slaveNodes = slave.FindNodes( label );
					assertEquals( 1, Iterators.asList( slaveNodes ).Count );
					transaction.Success();
			  }
		 }

		 private void CreateLabeledNodeOnMaster( ClusterManager.ManagedCluster cluster, Label label )
		 {
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  using ( Transaction transaction = master.BeginTx() )
			  {
					Node masterNode = master.CreateNode();
					masterNode.AddLabel( label );
					transaction.Success();
			  }
		 }
	}

}