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
namespace Neo4Net.cluster.member
{

	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	/// <summary>
	/// A ClusterMemberListener is listening for events from elections and availability state.
	/// <para>
	/// These are invoked by translating atomic broadcast messages to methods on this interface.
	/// </para>
	/// </summary>
	public interface ClusterMemberListener
	{
		 /// <summary>
		 /// Called when new coordinator has been elected.
		 /// </summary>
		 /// <param name="coordinatorId"> the Id of the coordinator </param>
		 void CoordinatorIsElected( InstanceId coordinatorId );

		 /// <summary>
		 /// Called when a member announces that it is available to play a particular role, e.g. master or slave.
		 /// After this it can be assumed that the member is ready to consume messages related to that role.
		 /// </summary>
		 /// <param name="role"> </param>
		 /// <param name="availableId"> the role connection information for the new role holder </param>
		 /// <param name="atUri"> the URI at which the instance is available at </param>
		 /// <param name="storeId"> the identifier of a store that became available </param>
		 void MemberIsAvailable( string role, InstanceId availableId, URI atUri, StoreId storeId );

		 /// <summary>
		 /// Called when a member is no longer available for fulfilling a particular role.
		 /// </summary>
		 /// <param name="role"> The role for which the member is unavailable </param>
		 /// <param name="unavailableId"> The id of the member which became unavailable for that role </param>
		 void MemberIsUnavailable( string role, InstanceId unavailableId );

		 /// <summary>
		 /// Called when a member is considered failed, by quorum.
		 /// </summary>
		 /// <param name="instanceId"> of the failed server </param>
		 void MemberIsFailed( InstanceId instanceId );

		 /// <summary>
		 /// Called when a member is considered alive again, by quorum.
		 /// </summary>
		 /// <param name="instanceId"> of the now alive server </param>
		 void MemberIsAlive( InstanceId instanceId );
	}

	 public abstract class ClusterMemberListener_Adapter : ClusterMemberListener
	 {
		  public override void CoordinatorIsElected( InstanceId coordinatorId )
		  {
		  }

		  public override void MemberIsAvailable( string role, InstanceId availableId, URI atURI, StoreId storeId )
		  {
		  }

		  public override void MemberIsUnavailable( string role, InstanceId unavailableId )
		  {
		  }

		  public override void MemberIsFailed( InstanceId instanceId )
		  {
		  }

		  public override void MemberIsAlive( InstanceId instanceId )
		  {
		  }
	 }

}