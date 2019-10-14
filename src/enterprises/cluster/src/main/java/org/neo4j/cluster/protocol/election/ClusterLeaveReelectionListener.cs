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
namespace Neo4Net.cluster.protocol.election
{

	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// When an instance leaves a cluster, demote it from all its current roles.
	/// </summary>
	public class ClusterLeaveReelectionListener : Neo4Net.cluster.protocol.cluster.ClusterListener_Adapter
	{
		 private readonly Election _election;
		 private readonly Log _log;

		 public ClusterLeaveReelectionListener( Election election, LogProvider logProvider )
		 {
			  this._election = election;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void LeftCluster( InstanceId instanceId, URI member )
		 {
			  string name = instanceId.InstanceNameFromURI( member );
			  _log.warn( "Demoting member " + name + " because it left the cluster" );
			  // Suggest reelection for all roles of this node
			  _election.demote( instanceId );
		 }
	}

}