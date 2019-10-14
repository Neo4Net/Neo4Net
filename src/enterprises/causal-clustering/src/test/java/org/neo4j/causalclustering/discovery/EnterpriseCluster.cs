using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.discovery
{

	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;

	public class EnterpriseCluster : Cluster<DiscoveryServiceFactory>
	{
		 public EnterpriseCluster( File parentDir, int noOfCoreMembers, int noOfReadReplicas, DiscoveryServiceFactory discoveryServiceFactory, IDictionary<string, string> coreParams, IDictionary<string, System.Func<int, string>> instanceCoreParams, IDictionary<string, string> readReplicaParams, IDictionary<string, System.Func<int, string>> instanceReadReplicaParams, string recordFormat, IpFamily ipFamily, bool useWildcard ) : base( parentDir, noOfCoreMembers, noOfReadReplicas, discoveryServiceFactory, coreParams, instanceCoreParams, readReplicaParams, instanceReadReplicaParams, recordFormat, ipFamily, useWildcard )
		 {
		 }

		 public EnterpriseCluster( File parentDir, int noOfCoreMembers, int noOfReadReplicas, DiscoveryServiceFactory discoveryServiceFactory, IDictionary<string, string> coreParams, IDictionary<string, System.Func<int, string>> instanceCoreParams, IDictionary<string, string> readReplicaParams, IDictionary<string, System.Func<int, string>> instanceReadReplicaParams, string recordFormat, IpFamily ipFamily, bool useWildcard, ISet<string> dbNames ) : base( parentDir, noOfCoreMembers, noOfReadReplicas, discoveryServiceFactory, coreParams, instanceCoreParams, readReplicaParams, instanceReadReplicaParams, recordFormat, ipFamily, useWildcard, dbNames )
		 {
		 }

		 protected internal override CoreClusterMember CreateCoreClusterMember( int serverId, int discoveryPort, int clusterSize, IList<AdvertisedSocketAddress> initialHosts, string recordFormat, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams )
		 {
			  int txPort = PortAuthority.allocatePort();
			  int raftPort = PortAuthority.allocatePort();
			  int boltPort = PortAuthority.allocatePort();
			  int httpPort = PortAuthority.allocatePort();
			  int backupPort = PortAuthority.allocatePort();

			  return new CoreClusterMember( serverId, discoveryPort, txPort, raftPort, boltPort, httpPort, backupPort, clusterSize, initialHosts, DiscoveryServiceFactory, recordFormat, ParentDir, extraParams, instanceExtraParams, ListenAddress, AdvertisedAddress );
		 }

		 protected internal override ReadReplica CreateReadReplica( int serverId, IList<AdvertisedSocketAddress> initialHosts, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams, string recordFormat, Monitors monitors )
		 {
			  int boltPort = PortAuthority.allocatePort();
			  int httpPort = PortAuthority.allocatePort();
			  int txPort = PortAuthority.allocatePort();
			  int backupPort = PortAuthority.allocatePort();
			  int discoveryPort = PortAuthority.allocatePort();

			  return new ReadReplica( ParentDir, serverId, boltPort, httpPort, txPort, backupPort, discoveryPort, DiscoveryServiceFactory, initialHosts, extraParams, instanceExtraParams, recordFormat, monitors, AdvertisedAddress, ListenAddress );
		 }
	}

}