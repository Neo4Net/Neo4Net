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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using ManagedCluster = Org.Neo4j.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsJoined;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.clusterWithAdditionalClients;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterSeesMembers;

	public class ClusterClientPaddingIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withSharedSetting(org.neo4j.cluster.ClusterSettings.heartbeat_interval, "1s").withSharedSetting(org.neo4j.cluster.ClusterSettings.heartbeat_timeout, "10s");
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