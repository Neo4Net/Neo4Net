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
namespace Neo4Net.Kernel.ha
{

	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterMemberListener = Neo4Net.cluster.member.ClusterMemberListener;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using AvailabilityListener = Neo4Net.Kernel.availability.AvailabilityListener;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	/// <summary>
	/// This class logs whenever important cluster or high availability events
	/// are issued.
	/// </summary>
	public class HighAvailabilityLogger : ClusterMemberListener, ClusterListener, AvailabilityListener
	{
		 private readonly Log _log;
		 private readonly InstanceId _myId;
		 private URI _myUri;

		 public HighAvailabilityLogger( LogProvider logProvider, InstanceId myId )
		 {
			  this._log = logProvider.getLog( this.GetType() );
			  this._myId = myId;
		 }

		 // Cluster events

		 /// <summary>
		 /// Logged when the instance itself joins or rejoins a cluster
		 /// </summary>
		 /// <param name="clusterConfiguration"> </param>
		 public override void EnteredCluster( ClusterConfiguration clusterConfiguration )
		 {
			  _myUri = clusterConfiguration.GetUriForId( _myId );
			  _log.info( "Instance %s entered the cluster", PrintId( _myId, _myUri ) );
		 }

		 /// <summary>
		 /// Logged when the instance itself leaves the cluster
		 /// </summary>
		 public override void LeftCluster()
		 {
			  _log.info( "Instance %s left the cluster", PrintId( _myId, _myUri ) );
		 }

		 /// <summary>
		 /// Logged when another instance joins the cluster
		 /// </summary>
		 /// <param name="instanceId"> </param>
		 /// <param name="member"> </param>
		 public override void JoinedCluster( InstanceId instanceId, URI member )
		 {
			  _log.info( "Instance %s joined the cluster", PrintId( instanceId, member ) );
		 }

		 /// <summary>
		 /// Logged when another instance leaves the cluster
		 /// </summary>
		 /// <param name="instanceId"> </param>
		 public override void LeftCluster( InstanceId instanceId, URI member )
		 {
			  _log.info( "Instance %s has left the cluster", PrintId( instanceId, member ) );
		 }

		 /// <summary>
		 /// Logged when an instance is elected for a role, such as coordinator of a cluster.
		 /// </summary>
		 /// <param name="role"> </param>
		 /// <param name="instanceId"> </param>
		 /// <param name="electedMember"> </param>
		 public override void Elected( string role, InstanceId instanceId, URI electedMember )
		 {
			  _log.info( "Instance %s was elected as %s", PrintId( instanceId, electedMember ), role );
		 }

		 /// <summary>
		 /// Logged when an instance is demoted from a role.
		 /// </summary>
		 /// <param name="role"> </param>
		 /// <param name="instanceId"> </param>
		 /// <param name="electedMember"> </param>
		 public override void Unelected( string role, InstanceId instanceId, URI electedMember )
		 {
			  _log.info( "Instance %s was demoted as %s", PrintId( instanceId, electedMember ), role );
		 }

		 // HA events
		 public override void CoordinatorIsElected( InstanceId coordinatorId )
		 {
		 }

		 /// <summary>
		 /// Logged when a member becomes available as a role, such as MASTER or SLAVE.
		 /// </summary>
		 /// <param name="role"> </param>
		 /// <param name="availableId"> the role connection information for the new role holder </param>
		 /// <param name="atUri">       the URI at which the instance is available at </param>
		 public override void MemberIsAvailable( string role, InstanceId availableId, URI atUri, StoreId storeId )
		 {
			  _log.info( "Instance %s is available as %s at %s with %s", PrintId( availableId, atUri ), role, atUri.toASCIIString(), storeId );
		 }

		 /// <summary>
		 /// Logged when a member becomes unavailable as a role, such as MASTER or SLAVE.
		 /// </summary>
		 /// <param name="role">          The role for which the member is unavailable </param>
		 /// <param name="unavailableId"> The id of the member which became unavailable for that role </param>
		 public override void MemberIsUnavailable( string role, InstanceId unavailableId )
		 {
			  _log.info( "Instance %s is unavailable as %s", PrintId( unavailableId, null ), role );
		 }

		 /// <summary>
		 /// Logged when another instance is detected as being failed.
		 /// </summary>
		 /// <param name="instanceId"> </param>
		 public override void MemberIsFailed( InstanceId instanceId )
		 {
			  _log.info( "Instance %s has failed", PrintId( instanceId, null ) );
		 }

		 /// <summary>
		 /// Logged when another instance is detected as being alive again.
		 /// </summary>
		 /// <param name="instanceId"> </param>
		 public override void MemberIsAlive( InstanceId instanceId )
		 {
			  _log.info( "Instance %s is alive", PrintId( instanceId, null ) );
		 }

		 // InstanceAccessGuard events

		 /// <summary>
		 /// Logged when users are allowed to access the database for transactions.
		 /// </summary>
		 public override void Available()
		 {
			  _log.info( "Database available for write transactions" );
		 }

		 /// <summary>
		 /// Logged when users are not allowed to access the database for transactions.
		 /// </summary>
		 public override void Unavailable()
		 {
			  _log.info( "Write transactions to database disabled" );
		 }

		 private string PrintId( InstanceId id, URI member )
		 {
			  string name = id.InstanceNameFromURI( member );
			  return name + ( id.Equals( _myId ) ? " (this server) " : " " );
		 }
	}

}