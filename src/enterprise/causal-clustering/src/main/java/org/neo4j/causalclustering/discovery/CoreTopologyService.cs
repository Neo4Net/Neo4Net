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
	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using ClusterOverviewProcedure = Neo4Net.causalclustering.discovery.procedures.ClusterOverviewProcedure;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;

	/// <summary>
	/// Extends upon the topology service with a few extra services, connected to
	/// the underlying discovery service.
	/// </summary>
	public interface CoreTopologyService : TopologyService
	{
		 void AddLocalCoreTopologyListener( CoreTopologyService_Listener listener );

		 void RemoveLocalCoreTopologyListener( CoreTopologyService_Listener listener );

		 /// <summary>
		 /// Publishes the cluster ID so that other members might discover it.
		 /// Should only succeed to publish if one missing or already the same (CAS logic).
		 /// </summary>
		 /// <param name="clusterId"> The cluster ID to publish.
		 /// </param>
		 /// <returns> True if the cluster ID was successfully CAS:ed, otherwise false. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean setClusterId(org.neo4j.causalclustering.identity.ClusterId clusterId, String dbName) throws InterruptedException;
		 bool SetClusterId( ClusterId clusterId, string dbName );

		 /// <summary>
		 /// Sets or updates the leader memberId for the given database (i.e. Raft consensus group).
		 /// This is intended for informational purposes **only**, e.g. in <seealso cref="ClusterOverviewProcedure"/>.
		 /// The leadership information should otherwise be communicated via raft as before. </summary>
		 /// <param name="leaderInfo"> Information about the new leader </param>
		 /// <param name="dbName"> The database name for which memberId is the new leader </param>
		 void SetLeader( LeaderInfo leaderInfo, string dbName );

		 /// <summary>
		 /// Fetches info for the current leader </summary>
		 LeaderInfo Leader { get; }

		 /// <summary>
		 /// Set the leader memberId to null for a given database (i.e. Raft consensus group).
		 /// This is intended to trigger state cleanup for informational procedures like <seealso cref="ClusterOverviewProcedure"/>
		 /// </summary>
		 /// <param name="term"> The term for which this topology member should handle a stepdown </param>
		 /// <param name="dbName"> The database for which this topology member should handle a stepdown </param>
		 void HandleStepDown( long term, string dbName );
	}

	 public interface CoreTopologyService_Listener
	 {
		  void OnCoreTopologyChange( CoreTopology coreTopology );
		  string DbName();
	 }

}