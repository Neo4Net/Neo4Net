﻿using System;
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
namespace Neo4Net.Test.causalclustering
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class ClusterRuleIT
	{
		 private const int NUMBER_OF_PORTS_USED_BY_CORE_MEMBER = 6;
		 private const int NUMBER_OF_PORTS_USED_BY_READ_REPLICA = 4;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final ClusterRule clusterRule = new ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAssignPortsToMembersAutomatically() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAssignPortsToMembersAutomatically()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.withNumberOfCoreMembers(3).withNumberOfReadReplicas(5).startCluster();
			  Cluster<object> cluster = ClusterRule.withNumberOfCoreMembers( 3 ).withNumberOfReadReplicas( 5 ).startCluster();

			  int numberOfCoreMembers = cluster.CoreMembers().Count;
			  assertThat( numberOfCoreMembers, @is( 3 ) );
			  int numberOfReadReplicas = cluster.ReadReplicas().Count;
			  assertThat( numberOfReadReplicas, @is( 5 ) );

			  ISet<int> portsUsed = GatherPortsUsed( cluster );

			  // so many for core members, so many for read replicas, all unique
			  assertThat( portsUsed.Count, @is( numberOfCoreMembers * NUMBER_OF_PORTS_USED_BY_CORE_MEMBER + numberOfReadReplicas * NUMBER_OF_PORTS_USED_BY_READ_REPLICA ) );
		 }

		 private ISet<int> GatherPortsUsed<T1>( Cluster<T1> cluster )
		 {
			  ISet<int> portsUsed = new HashSet<int>();

			  foreach ( CoreClusterMember coreClusterMember in cluster.CoreMembers() )
			  {
					portsUsed.Add( GetPortFromSetting( coreClusterMember, CausalClusteringSettings.discovery_listen_address.name() ) );
					portsUsed.Add( GetPortFromSetting( coreClusterMember, CausalClusteringSettings.transaction_listen_address.name() ) );
					portsUsed.Add( GetPortFromSetting( coreClusterMember, CausalClusteringSettings.raft_listen_address.name() ) );
					portsUsed.Add( GetPortFromSetting( coreClusterMember, OnlineBackupSettings.online_backup_server.name() ) );
					portsUsed.Add( GetPortFromSetting( coreClusterMember, ( new BoltConnector( "bolt" ) ).listen_address.name() ) );
					portsUsed.Add( GetPortFromSetting( coreClusterMember, ( new HttpConnector( "http" ) ).listen_address.name() ) );
			  }

			  foreach ( ReadReplica readReplica in cluster.ReadReplicas() )
			  {
					portsUsed.Add( GetPortFromSetting( readReplica, CausalClusteringSettings.transaction_listen_address.name() ) );
					portsUsed.Add( GetPortFromSetting( readReplica, OnlineBackupSettings.online_backup_server.name() ) );
					portsUsed.Add( GetPortFromSetting( readReplica, ( new BoltConnector( "bolt" ) ).listen_address.name() ) );
					portsUsed.Add( GetPortFromSetting( readReplica, ( new HttpConnector( "http" ) ).listen_address.name() ) );
			  }
			  return portsUsed;
		 }

		 private int GetPortFromSetting( ClusterMember coreClusterMember, string settingName )
		 {
			  string setting = coreClusterMember.settingValue( settingName );
			  return Convert.ToInt32( setting.Split( ":", true )[1] );
		 }
	}

}