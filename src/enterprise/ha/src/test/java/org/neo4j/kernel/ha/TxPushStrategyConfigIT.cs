using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using InstanceId = Neo4Net.cluster.InstanceId;
	using TransactionTemplate = Neo4Net.Helpers.TransactionTemplate;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterSeesSlavesAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class TxPushStrategyConfigIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

		 /// <summary>
		 /// These are _indexes_ of cluster members in machineIds
		 /// </summary>
		 private const int MASTER = 1;
		 private const int FIRST_SLAVE = 2;
		 private const int SECOND_SLAVE = 3;
		 private const int THIRD_SLAVE = 4;
		 private InstanceId[] _machineIds;

		 private readonly MissedReplicasMonitor _monitorListener = new MissedReplicasMonitor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPushToSlavesInDescendingOrder()
		 public virtual void ShouldPushToSlavesInDescendingOrder()
		 {
			  ManagedCluster cluster = StartCluster( 4, 2, HaSettings.TxPushStrategy.FixedDescending );

			  for ( int i = 0; i < 5; i++ )
			  {
					int missed = CreateTransactionOnMaster( cluster );
					AssertLastTransactions( cluster, LastTx( THIRD_SLAVE, BASE_TX_ID + 1 + i, missed ) );
					AssertLastTransactions( cluster, LastTx( SECOND_SLAVE, BASE_TX_ID + 1 + i, missed ) );
					AssertLastTransactions( cluster, LastTx( FIRST_SLAVE, BASE_TX_ID, missed ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPushToSlavesInAscendingOrder()
		 public virtual void ShouldPushToSlavesInAscendingOrder()
		 {
			  ManagedCluster cluster = StartCluster( 4, 2, HaSettings.TxPushStrategy.FixedAscending );

			  for ( int i = 0; i < 5; i++ )
			  {
					int missed = CreateTransactionOnMaster( cluster );
					AssertLastTransactions( cluster, LastTx( FIRST_SLAVE, BASE_TX_ID + 1 + i, missed ) );
					AssertLastTransactions( cluster, LastTx( SECOND_SLAVE, BASE_TX_ID + 1 + i, missed ) );
					AssertLastTransactions( cluster, LastTx( THIRD_SLAVE, BASE_TX_ID, missed ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoRoundRobin()
		 public virtual void TwoRoundRobin()
		 {
			  ManagedCluster cluster = StartCluster( 4, 2, HaSettings.TxPushStrategy.RoundRobin );

			  HighlyAvailableGraphDatabase master = cluster.Master;
			  Monitors monitors = master.DependencyResolver.resolveDependency( typeof( Monitors ) );
			  AtomicInteger totalMissedReplicas = new AtomicInteger();
			  monitors.AddMonitorListener( ( MasterTransactionCommitProcess.Monitor ) totalMissedReplicas.addAndGet );
			  long txId = GetLastTx( master );
			  int count = 15;
			  for ( int i = 0; i < count; i++ )
			  {
					CreateTransactionOnMaster( cluster );
			  }

			  long min = -1;
			  long max = -1;
			  foreach ( GraphDatabaseAPI db in cluster.AllMembers )
			  {
					long tx = GetLastTx( db );
					min = min == -1 ? tx : min( min, tx );
					max = max == -1 ? tx : max( max, tx );
			  }

			  assertEquals( txId + count, max );
			  assertTrue( "There should be members with transactions in the cluster", min != -1 && max != -1 );

			  int minLaggingBehindThreshold = 1 + totalMissedReplicas.get();
			  assertThat( "There should at most be a txId gap of 1 among the cluster members since the transaction pushing " + "goes in a round robin fashion. min:" + min + ", max:" + max, ( int )( max - min ), lessThanOrEqualTo( minLaggingBehindThreshold ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPushToOneLessSlaveOnSlaveCommit()
		 public virtual void ShouldPushToOneLessSlaveOnSlaveCommit()
		 {
			  ManagedCluster cluster = StartCluster( 4, 2, HaSettings.TxPushStrategy.FixedDescending );

			  int missed = 0;
			  missed += CreateTransactionOn( cluster, new InstanceId( FIRST_SLAVE ) );
			  AssertLastTransactions( cluster, LastTx( MASTER, BASE_TX_ID + 1, missed ), LastTx( FIRST_SLAVE, BASE_TX_ID + 1, missed ), LastTx( SECOND_SLAVE, BASE_TX_ID, missed ), LastTx( THIRD_SLAVE, BASE_TX_ID + 1, missed ) );

			  missed += CreateTransactionOn( cluster, new InstanceId( SECOND_SLAVE ) );
			  AssertLastTransactions( cluster, LastTx( MASTER, BASE_TX_ID + 2, missed ), LastTx( FIRST_SLAVE, BASE_TX_ID + 1, missed ), LastTx( SECOND_SLAVE, BASE_TX_ID + 2, missed ), LastTx( THIRD_SLAVE, BASE_TX_ID + 2, missed ) );

			  missed += CreateTransactionOn( cluster, new InstanceId( THIRD_SLAVE ) );
			  AssertLastTransactions( cluster, LastTx( MASTER, BASE_TX_ID + 3, missed ), LastTx( FIRST_SLAVE, BASE_TX_ID + 1, missed ), LastTx( SECOND_SLAVE, BASE_TX_ID + 3, missed ), LastTx( THIRD_SLAVE, BASE_TX_ID + 3, missed ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slavesListGetsUpdatedWhenSlaveLeavesNicely()
		 public virtual void SlavesListGetsUpdatedWhenSlaveLeavesNicely()
		 {
			  ManagedCluster cluster = StartCluster( 3, 1, HaSettings.TxPushStrategy.FixedAscending );

			  cluster.Shutdown( cluster.AnySlave );
			  cluster.Await( masterSeesSlavesAsAvailable( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveListIsCorrectAfterMasterSwitch()
		 public virtual void SlaveListIsCorrectAfterMasterSwitch()
		 {
			  ManagedCluster cluster = StartCluster( 3, 1, HaSettings.TxPushStrategy.FixedAscending );
			  cluster.Shutdown( cluster.Master );
			  cluster.Await( masterAvailable() );
			  HighlyAvailableGraphDatabase newMaster = cluster.Master;
			  cluster.Await( masterSeesSlavesAsAvailable( 1 ) );
			  int missed = CreateTransaction( cluster, newMaster );
			  AssertLastTransactions( cluster, LastTx( FIRST_SLAVE, BASE_TX_ID + 1, missed ), LastTx( SECOND_SLAVE, BASE_TX_ID + 1, missed ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slavesListGetsUpdatedWhenSlaveRageQuits()
		 public virtual void SlavesListGetsUpdatedWhenSlaveRageQuits()
		 {
			  ManagedCluster cluster = StartCluster( 3, 1, HaSettings.TxPushStrategy.FixedAscending );
			  cluster.Fail( cluster.AnySlave );

			  cluster.Await( masterSeesSlavesAsAvailable( 1 ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.kernel.impl.ha.ClusterManager.ManagedCluster startCluster(int memberCount, final int pushFactor, final HaSettings.TxPushStrategy pushStrategy)
		 private ManagedCluster StartCluster( int memberCount, int pushFactor, HaSettings.TxPushStrategy pushStrategy )
		 {
			  ManagedCluster cluster = ClusterRule.withCluster( clusterOfSize( memberCount ) ).withSharedSetting( HaSettings.TxPushFactor, "" + pushFactor ).withSharedSetting( HaSettings.TxPushStrategy, pushStrategy.name() ).startCluster();

			  MapMachineIds( cluster );

			  return cluster;
		 }

		 private void MapMachineIds( ManagedCluster cluster )
		 {
			  _machineIds = new InstanceId[cluster.Size()];
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  master.DependencyResolver.resolveDependency( typeof( Monitors ) ).addMonitorListener( _monitorListener );
			  _machineIds[0] = cluster.GetServerId( master );
			  IList<HighlyAvailableGraphDatabase> slaves = new List<HighlyAvailableGraphDatabase>();
			  foreach ( HighlyAvailableGraphDatabase hadb in cluster.AllMembers )
			  {
					if ( !hadb.Master )
					{
						 slaves.Add( hadb );
						 hadb.DependencyResolver.resolveDependency( typeof( Monitors ) ).removeMonitorListener( _monitorListener );
					}
			  }
			  slaves.sort( System.Collections.IComparer.comparing( cluster.getServerId ) );
			  IEnumerator<HighlyAvailableGraphDatabase> iter = slaves.GetEnumerator();
			  for ( int i = 1; iter.MoveNext(); i++ )
			  {
					_machineIds[i] = cluster.GetServerId( iter.Current );
			  }
		 }

		 private void AssertLastTransactions( ManagedCluster cluster, params LastTxMapping[] transactionMappings )
		 {
			  StringBuilder failures = new StringBuilder();
			  foreach ( LastTxMapping mapping in transactionMappings )
			  {
					GraphDatabaseAPI db = cluster.GetMemberByServerId( mapping.ServerId );
					mapping.Format( failures, GetLastTx( db ) );
			  }
			  assertEquals( failures.ToString(), 0, failures.Length );
		 }

		 private long GetLastTx( GraphDatabaseAPI db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) ).LastCommittedTransactionId;
		 }

		 private LastTxMapping LastTx( int serverIndex, long txId, int missed )
		 {
			  InstanceId serverId = _machineIds[serverIndex - 1];
			  return new LastTxMapping( serverId, txId, missed );
		 }

		 private int CreateTransactionOnMaster( ManagedCluster cluster )
		 {
			  return CreateTransaction( cluster, cluster.Master );
		 }

		 private int CreateTransactionOn( ManagedCluster cluster, InstanceId serverId )
		 {
			  return CreateTransaction( cluster, cluster.GetMemberByServerId( serverId ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private int createTransaction(final org.neo4j.kernel.impl.ha.ClusterManager.ManagedCluster cluster, final org.neo4j.kernel.internal.GraphDatabaseAPI db)
		 private int CreateTransaction( ManagedCluster cluster, GraphDatabaseAPI db )
		 {
			  TransactionTemplate template = ( new TransactionTemplate() ).with(db).retries(10).backoff(1, TimeUnit.SECONDS).monitor(new Monitor_AdapterAnonymousInnerClass(this, cluster));

			  template.Execute(transaction =>
			  {
				_monitorListener.clear();
				Db.createNode();
			  });

			  return _monitorListener.missed();
		 }

		 private class Monitor_AdapterAnonymousInnerClass : TransactionTemplate.Monitor_Adapter
		 {
			 private readonly TxPushStrategyConfigIT _outerInstance;

			 private ManagedCluster _cluster;

			 public Monitor_AdapterAnonymousInnerClass( TxPushStrategyConfigIT outerInstance, ManagedCluster cluster )
			 {
				 this.outerInstance = outerInstance;
				 this._cluster = cluster;
			 }

			 public override void retrying()
			 {
				  Console.Error.WriteLine( "Retrying..." );
			 }

			 public override void failure( Exception ex )
			 {
				  Console.Error.WriteLine( "Attempt failed with " + ex );

				  // Assume this is because of master switch
				  // Redo the machine id mapping
				  _cluster.await( allSeesAllAsAvailable() );
				  outerInstance.mapMachineIds( _cluster );
			 }
		 }

		 private class LastTxMapping
		 {
			  internal readonly InstanceId ServerId;
			  internal readonly long TxId;
			  internal readonly int Missed;

			  internal LastTxMapping( InstanceId serverId, long txId, int missed )
			  {
					this.ServerId = serverId;
					this.TxId = txId;
					this.Missed = missed;
			  }

			  public virtual void Format( StringBuilder failures, long txId )
			  {
					if ( txId < this.TxId - this.Missed || txId > this.TxId )
					{
						 if ( failures.Length > 0 )
						 {
							  failures.Append( ", " );
						 }
						 failures.Append( string.Format( "tx id on server:{0:D}, expected [{1:D}] but was [{2:D}]", ServerId.toIntegerIndex(), this.TxId, txId ) );
					}
			  }
		 }

		 private class MissedReplicasMonitor : MasterTransactionCommitProcess.Monitor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int MissedConflict;

			  public override void MissedReplicas( int number )
			  {
					MissedConflict = number;
			  }

			  internal virtual int Missed()
			  {
					return MissedConflict;
			  }

			  internal virtual void Clear()
			  {
					MissedConflict = 0;
			  }
		 }
	}

}