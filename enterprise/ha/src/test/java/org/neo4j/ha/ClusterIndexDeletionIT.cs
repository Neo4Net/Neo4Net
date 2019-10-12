/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using HaSettings = Org.Neo4j.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ManagedCluster = Org.Neo4j.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class ClusterIndexDeletionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withSharedSetting(org.neo4j.kernel.ha.HaSettings.tx_push_factor, "2").withSharedSetting(org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.neo4j.kernel.configuration.Settings.FALSE);
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(HaSettings.tx_push_factor, "2").withSharedSetting(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWithCreatedIndexWhenDeleteIndexOnMasterThenIndexIsDeletedOnSlave()
		 public virtual void GivenClusterWithCreatedIndexWhenDeleteIndexOnMasterThenIndexIsDeletedOnSlave()
		 {
			  ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  using ( Transaction tx = master.BeginTx() )
			  {
					master.Index().forNodes("Test");
					tx.Success();
			  }

			  cluster.Sync();

			  HighlyAvailableGraphDatabase aSlave = cluster.AnySlave;
			  using ( Transaction tx = aSlave.BeginTx() )
			  {
					assertThat( aSlave.Index().existsForNodes("Test"), equalTo(true) );
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = master.BeginTx() )
			  {
					master.Index().forNodes("Test").delete();
					tx.Success();
			  }

			  cluster.Sync();

			  // Then
			  HighlyAvailableGraphDatabase anotherSlave = cluster.AnySlave;
			  using ( Transaction tx = anotherSlave.BeginTx() )
			  {
					assertThat( anotherSlave.Index().existsForNodes("Test"), equalTo(false) );
					tx.Success();
			  }
		 }
	}

}