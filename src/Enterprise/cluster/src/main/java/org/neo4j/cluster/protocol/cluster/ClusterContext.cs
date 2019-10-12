using System;
using System.Collections.Generic;

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
namespace Neo4Net.cluster.protocol.cluster
{

	using ObjectInputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using ConfigurationResponseState = Neo4Net.cluster.protocol.cluster.ClusterMessage.ConfigurationResponseState;

	/// <summary>
	/// Represents the context necessary for cluster operations. Includes instance membership calls, election
	/// facilities and liveness detection. This is expected to be used from a variety of cluster components
	/// as part of their state.
	/// </summary>
	/// <seealso cref= ClusterState </seealso>
	public interface ClusterContext : LoggingContext, TimeoutsContext, ConfigurationContext
	{

		 // Cluster API
		 void AddClusterListener( ClusterListener listener );

		 void RemoveClusterListener( ClusterListener listener );

		 // Implementation
		 void Created( string name );

		 void Joining( string name, IEnumerable<URI> instanceList );

		 void AcquiredConfiguration( IDictionary<InstanceId, URI> memberList, IDictionary<string, InstanceId> roles, ISet<InstanceId> failedInstances );

		 void Joined();

		 void Left();

		 void Joined( InstanceId instanceId, URI atURI );

		 void Left( InstanceId node );

		 [Obsolete]
		 void Elected( string roleName, InstanceId instanceId );

		 void Elected( string roleWon, InstanceId winner, InstanceId elector, long version );

		 [Obsolete]
		 void Unelected( string roleName, InstanceId instanceId );

		 void Unelected( string roleLost, InstanceId loser, InstanceId elector, long version );

		 ClusterConfiguration Configuration { get; }

		 bool IsElectedAs( string roleName );

		 bool InCluster { get; }

		 IEnumerable<URI> JoiningInstances { get; }

		 ObjectOutputStreamFactory ObjectOutputStreamFactory { get; }

		 ObjectInputStreamFactory ObjectInputStreamFactory { get; }

		 IList<ClusterMessage.ConfigurationRequestState> DiscoveredInstances { get; }

		 bool HaveWeContactedInstance( ClusterMessage.ConfigurationRequestState configurationRequested );

		 void AddContactingInstance( ClusterMessage.ConfigurationRequestState instance, string discoveryHeader );

		 string GenerateDiscoveryHeader();

		 URI BoundAt { set; }

		 void JoinDenied( ConfigurationResponseState configurationResponseState );

		 bool HasJoinBeenDenied();

		 ConfigurationResponseState JoinDeniedConfigurationResponseState { get; }

		 IEnumerable<InstanceId> OtherInstances { get; }

		 bool IsInstanceJoiningFromDifferentUri( InstanceId joiningId, URI joiningUri );

		 void InstanceIsJoining( InstanceId joiningId, URI uri );

		 string MyName();

		 void DiscoveredLastReceivedInstanceId( long id );

		 bool IsCurrentlyAlive( InstanceId joiningId );

		 long LastDeliveredInstanceId { get; }

		 InstanceId LastElector { get;set; }


		 long LastElectorVersion { get;set; }


		 bool ShouldFilterContactingInstances();

		 /// <returns> The set of instances present in the failed set. This is not the same as the instances which are
		 /// determined to be failed based on suspicions, as failed instance information can also come from the cluster
		 /// configuration response at join time. </returns>
		 ISet<InstanceId> FailedInstances { get; }
	}

	public static class ClusterContext_Fields
	{
		 public const int NO_ELECTOR_VERSION = -1;
	}

}