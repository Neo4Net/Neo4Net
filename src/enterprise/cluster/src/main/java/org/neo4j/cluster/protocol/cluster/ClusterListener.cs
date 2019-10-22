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
namespace Neo4Net.cluster.protocol.cluster
{

	/// <summary>
	/// Listener interface for cluster configuration changes. Register instances
	/// of this interface with <seealso cref="Cluster"/>
	/// </summary>
	public interface ClusterListener
	{
		 /// <summary>
		 /// When I enter the cluster as a member, this callback notifies me
		 /// that this has been agreed upon by the entire cluster.
		 /// </summary>
		 /// <param name="clusterConfiguration"> </param>
		 void EnteredCluster( ClusterConfiguration clusterConfiguration );

		 /// <summary>
		 /// When I leave the cluster, this callback notifies me
		 /// that this has been agreed upon by the entire cluster.
		 /// </summary>
		 void LeftCluster();

		 /// <summary>
		 /// When another instance joins as a member, this callback is invoked
		 /// </summary>
		 /// <param name="member"> </param>
		 void JoinedCluster( InstanceId instanceId, URI member );

		 /// <summary>
		 /// When another instance leaves the cluster, this callback is invoked.
		 /// Implicitly, any roles that this member had, are revoked.
		 /// </summary>
		 /// <param name="member"> </param>
		 void LeftCluster( InstanceId instanceId, URI member );

		 /// <summary>
		 /// When a member (including potentially myself) has been elected to a particular role, this callback is invoked.
		 /// Combine this callback with the leftCluster to keep track of current set of role->member mappings.
		 /// </summary>
		 /// <param name="role"> </param>
		 /// <param name="electedMember"> </param>
		 void Elected( string role, InstanceId instanceId, URI electedMember );

		 void Unelected( string role, InstanceId instanceId, URI electedMember );
	}

	 public abstract class ClusterListener_Adapter : ClusterListener
	 {
		  public override void EnteredCluster( ClusterConfiguration clusterConfiguration )
		  {
		  }

		  public override void JoinedCluster( InstanceId instanceId, URI member )
		  {
		  }

		  public override void LeftCluster( InstanceId instanceId, URI member )
		  {
		  }

		  public override void LeftCluster()
		  {
		  }

		  public override void Elected( string role, InstanceId instanceId, URI electedMember )
		  {
		  }

		  public override void Unelected( string role, InstanceId instanceId, URI electedMember )
		  {
		  }
	 }

}