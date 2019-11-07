﻿/*
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

	using Transaction = Neo4Net.GraphDb.Transaction;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class ClusterIndexDeletionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.ha.ClusterRule clusterRule = new Neo4Net.test.ha.ClusterRule().withSharedSetting(Neo4Net.kernel.ha.HaSettings.tx_push_factor, "2").withSharedSetting(Neo4Net.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, Neo4Net.kernel.configuration.Settings.FALSE);
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