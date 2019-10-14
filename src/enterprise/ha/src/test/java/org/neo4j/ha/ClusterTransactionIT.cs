using System;

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
namespace Neo4Net.ha
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using AvailabilityListener = Neo4Net.Kernel.availability.AvailabilityListener;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.clusterOfSize;

	public class ClusterTransactionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

		 private ClusterManager.ManagedCluster _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _cluster = ClusterRule.withCluster( clusterOfSize( 3 ) ).withSharedSetting( HaSettings.tx_push_factor, "2" ).withSharedSetting( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ).startCluster();

			  _cluster.await( ClusterManager.allSeesAllAsAvailable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWhenShutdownMasterThenCannotStartTransactionOnSlave() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GivenClusterWhenShutdownMasterThenCannotStartTransactionOnSlave()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase master = cluster.getMaster();
			  HighlyAvailableGraphDatabase master = _cluster.Master;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase slave = cluster.getAnySlave();
			  HighlyAvailableGraphDatabase slave = _cluster.AnySlave;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId;
			  long nodeId;
			  using ( Transaction tx = master.BeginTx() )
			  {
					nodeId = master.CreateNode().Id;
					tx.Success();
			  }

			  _cluster.sync();

			  // When
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.FutureTask<bool> result = new java.util.concurrent.FutureTask<>(() ->
			  FutureTask<bool> result = new FutureTask<bool>(() =>
			  {
				try
				{
					using ( Transaction tx = slave.BeginTx() )
					{
						 tx.AcquireWriteLock( slave.GetNodeById( nodeId ) );
					}
				}
				catch ( Exception e )
				{
					 return contains( e, typeof( TransactionFailureException ) );
				}
				// Fail otherwise
				return false;
			  });

			  DatabaseAvailabilityGuard masterGuard = master.DependencyResolver.resolveDependency( typeof( DatabaseAvailabilityGuard ) );
			  masterGuard.AddListener( new UnavailabilityListener( result ) );

			  master.Shutdown();

			  // Then
			  assertThat( result.get(), equalTo(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveMustConnectLockManagerToNewMasterAfterTwoOtherClusterMembersRoleSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SlaveMustConnectLockManagerToNewMasterAfterTwoOtherClusterMembersRoleSwitch()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase initialMaster = cluster.getMaster();
			  HighlyAvailableGraphDatabase initialMaster = _cluster.Master;
			  HighlyAvailableGraphDatabase firstSlave = _cluster.AnySlave;
			  HighlyAvailableGraphDatabase secondSlave = _cluster.getAnySlave( firstSlave );

			  // Run a transaction on the slaves, to make sure that a master connection has been initialised in all
			  // internal pools.
			  using ( Transaction tx = firstSlave.BeginTx() )
			  {
					firstSlave.CreateNode();
					tx.Success();
			  }
			  using ( Transaction tx = secondSlave.BeginTx() )
			  {
					secondSlave.CreateNode();
					tx.Success();
			  }
			  _cluster.sync();

			  ClusterManager.RepairKit failedMaster = _cluster.fail( initialMaster );
			  _cluster.await( ClusterManager.masterAvailable( initialMaster ) );
			  failedMaster.Repair();
			  _cluster.await( ClusterManager.masterAvailable( initialMaster ) );
			  _cluster.await( ClusterManager.allSeesAllAsAvailable() );

			  // The cluster has now switched the master role to one of the slaves.
			  // The slave that didn't switch, should still have done the work to reestablish the connection to the new
			  // master.
			  HighlyAvailableGraphDatabase slave = _cluster.getAnySlave( initialMaster );
			  using ( Transaction tx = slave.BeginTx() )
			  {
					slave.CreateNode();
					tx.Success();
			  }

			  // We assert that the transaction above does not throw any exceptions, and that we have now created 3 nodes.
			  HighlyAvailableGraphDatabase master = _cluster.Master;
			  using ( Transaction tx = master.BeginTx() )
			  {
					assertThat( Iterables.count( master.AllNodes ), @is( 3L ) );
			  }
		 }

		 private class UnavailabilityListener : AvailabilityListener
		 {
			  internal readonly FutureTask<bool> Result;

			  internal UnavailabilityListener( FutureTask<bool> result )
			  {
					this.Result = result;
			  }

			  public override void Available()
			  {
					//nothing
			  }

			  public override void Unavailable()
			  {
					Result.run();
			  }
		 }
	}

}