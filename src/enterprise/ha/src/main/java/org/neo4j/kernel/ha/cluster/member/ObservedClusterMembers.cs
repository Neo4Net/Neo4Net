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
namespace Neo4Net.Kernel.ha.cluster.member
{

	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterMemberEvents = Neo4Net.cluster.member.ClusterMemberEvents;
	using ClusterMemberListener = Neo4Net.cluster.member.ClusterMemberListener;
	using Cluster = Neo4Net.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using Heartbeat = Neo4Net.cluster.protocol.heartbeat.Heartbeat;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using Neo4Net.Kernel.impl.util;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

	/// <summary>
	/// Keeps list of members, their roles and availability.
	/// List is based on different notifications about cluster and HA events.
	/// List is basically a 'best guess' of the cluster state because message ordering is not guaranteed.
	/// This class should be used only when imprecise state is acceptable.
	/// <para>
	/// For up-to-date cluster state use <seealso cref="ClusterMembers"/>.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= ClusterMembers </seealso>
	public class ObservedClusterMembers
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly System.Predicate<ClusterMember> _alive = ClusterMember::isAlive;

		 private readonly Log _log;
		 private readonly InstanceId _me;
		 private readonly IDictionary<InstanceId, ClusterMember> _members = new CopyOnWriteHashMap<InstanceId, ClusterMember>();

		 public ObservedClusterMembers( LogProvider logProvider, Cluster cluster, Heartbeat heartbeat, ClusterMemberEvents events, InstanceId me )
		 {
			  this._me = me;
			  this._log = logProvider.getLog( this.GetType() );
			  cluster.AddClusterListener( new HAMClusterListener( this ) );
			  heartbeat.AddHeartbeatListener( new HAMHeartbeatListener( this ) );
			  events.AddClusterMemberListener( new HAMClusterMemberListener( this ) );
		 }

		 public virtual IEnumerable<ClusterMember> Members
		 {
			 get
			 {
				  return _members.Values;
			 }
		 }

		 public virtual IEnumerable<ClusterMember> AliveMembers
		 {
			 get
			 {
				  return Iterables.filter( _alive, _members.Values );
			 }
		 }

		 public virtual ClusterMember CurrentMember
		 {
			 get
			 {
				  foreach ( ClusterMember clusterMember in Members )
				  {
						if ( clusterMember.InstanceId.Equals( _me ) )
						{
							 return clusterMember;
						}
				  }
				  return null;
			 }
		 }

		 private ClusterMember GetMember( InstanceId server )
		 {
			  ClusterMember clusterMember = _members[server];
			  if ( clusterMember == null )
			  {
					throw new System.InvalidOperationException( "Member " + server + " not found in " + new Dictionary<>( _members ) );
			  }
			  return clusterMember;
		 }

		 private class HAMClusterListener : Neo4Net.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly ObservedClusterMembers _outerInstance;

			 public HAMClusterListener( ObservedClusterMembers outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void EnteredCluster( ClusterConfiguration configuration )
			  {
					IDictionary<InstanceId, ClusterMember> newMembers = new Dictionary<InstanceId, ClusterMember>();
					foreach ( InstanceId memberClusterId in configuration.MemberIds )
					{
						 newMembers[memberClusterId] = new ClusterMember( memberClusterId );
					}
					outerInstance.members.Clear();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					outerInstance.members.putAll( newMembers );
			  }

			  public override void LeftCluster()
			  {
					outerInstance.members.Clear();
			  }

			  public override void JoinedCluster( InstanceId member, URI memberUri )
			  {
					outerInstance.members[member] = new ClusterMember( member );
			  }

			  public override void LeftCluster( InstanceId instanceId, URI member )
			  {
					outerInstance.members.Remove( instanceId );
			  }
		 }

		 private class HAMClusterMemberListener : Neo4Net.cluster.member.ClusterMemberListener_Adapter
		 {
			 private readonly ObservedClusterMembers _outerInstance;

			 public HAMClusterMemberListener( ObservedClusterMembers outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal InstanceId MasterId;

			  public override void CoordinatorIsElected( InstanceId coordinatorId )
			  {
					if ( coordinatorId.Equals( this.MasterId ) )
					{
						 return;
					}
					this.MasterId = coordinatorId;
					IDictionary<InstanceId, ClusterMember> newMembers = new Dictionary<InstanceId, ClusterMember>();
					foreach ( KeyValuePair<InstanceId, ClusterMember> memberEntry in outerInstance.members.SetOfKeyValuePairs() )
					{
						 newMembers[memberEntry.Key] = memberEntry.Value.unavailableAs( HighAvailabilityModeSwitcher.MASTER ).unavailableAs( HighAvailabilityModeSwitcher.SLAVE );
					}
					outerInstance.members.Clear();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					outerInstance.members.putAll( newMembers );
			  }

			  public override void MemberIsAvailable( string role, InstanceId instanceId, URI roleUri, StoreId storeId )
			  {
					outerInstance.members[instanceId] = outerInstance.getMember( instanceId ).AvailableAs( role, roleUri, storeId );
			  }

			  public override void MemberIsUnavailable( string role, InstanceId unavailableId )
			  {
					ClusterMember member;
					try
					{
						 member = outerInstance.getMember( unavailableId );
						 outerInstance.members[unavailableId] = member.UnavailableAs( role );
					}
					catch ( System.InvalidOperationException )
					{
						 outerInstance.log.Warn( "Unknown member with id '" + unavailableId + "' reported unavailable as '" + role + "'" );
					}
			  }

			  public override void MemberIsFailed( InstanceId instanceId )
			  {
					// Make it unavailable for all its current roles
					ClusterMember member = outerInstance.getMember( instanceId );
					foreach ( string role in member.Roles )
					{
						 member = member.UnavailableAs( role );
					}
					outerInstance.members[instanceId] = member;
			  }
		 }

		 private class HAMHeartbeatListener : Neo4Net.cluster.protocol.heartbeat.HeartbeatListener_Adapter
		 {
			 private readonly ObservedClusterMembers _outerInstance;

			 public HAMHeartbeatListener( ObservedClusterMembers outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Failed( InstanceId server )
			  {
					if ( outerInstance.members.ContainsKey( server ) )
					{
						 outerInstance.members[server] = outerInstance.getMember( server ).Failed();
					}
			  }

			  public override void Alive( InstanceId server )
			  {
					if ( outerInstance.members.ContainsKey( server ) )
					{
						 outerInstance.members[server] = outerInstance.getMember( server ).Alive();
					}
			  }
		 }
	}

}