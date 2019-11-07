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

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsJoined;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.clusterWithAdditionalClients;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.masterSeesMembers;

	public class ClusterClientPaddingIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.ha.ClusterRule clusterRule = new Neo4Net.test.ha.ClusterRule().withSharedSetting(Neo4Net.cluster.ClusterSettings.heartbeat_interval, "1s").withSharedSetting(Neo4Net.cluster.ClusterSettings.heartbeat_timeout, "10s");
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(ClusterSettings.heartbeat_interval, "1s").withSharedSetting(ClusterSettings.heartbeat_timeout, "10s");

		 private ManagedCluster _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _cluster = ClusterRule.withCluster( clusterWithAdditionalClients( 2, 1 ) ).withAvailabilityChecks( masterAvailable(), masterSeesMembers(3), allSeesAllAsJoined() ).startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void additionalClusterClientCanHelpBreakTiesWhenMasterIsShutDown()
		 public virtual void AdditionalClusterClientCanHelpBreakTiesWhenMasterIsShutDown()
		 {
			  HighlyAvailableGraphDatabase sittingMaster = _cluster.Master;
			  _cluster.shutdown( sittingMaster );
			  _cluster.await( masterAvailable( sittingMaster ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void additionalClusterClientCanHelpBreakTiesWhenMasterFails()
		 public virtual void AdditionalClusterClientCanHelpBreakTiesWhenMasterFails()
		 {
			  HighlyAvailableGraphDatabase sittingMaster = _cluster.Master;
			  _cluster.fail( sittingMaster );
			  _cluster.await( masterAvailable( sittingMaster ) );
		 }
	}

}