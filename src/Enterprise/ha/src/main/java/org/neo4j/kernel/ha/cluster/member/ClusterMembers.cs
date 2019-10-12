using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.ha.cluster.member
{

	using InstanceId = Neo4Net.cluster.InstanceId;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;

	/// <summary>
	/// Keeps a list of members, their roles and availability for display for example in JMX or REST.
	/// <para>
	/// Member state info is based on <seealso cref="ObservedClusterMembers"/> and <seealso cref="HighAvailabilityMemberStateMachine"/>.
	/// State of the current member is always valid, all other instances are only 'best effort'.
	/// </para>
	/// </summary>
	public class ClusterMembers
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ClusterMember> inRole(final String role)
		 public static System.Predicate<ClusterMember> InRole( string role )
		 {
			  return item => item.hasRole( role );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ClusterMember> hasInstanceId(final org.neo4j.cluster.InstanceId instanceId)
		 public static System.Predicate<ClusterMember> HasInstanceId( InstanceId instanceId )
		 {
			  return item => item.InstanceId.Equals( instanceId );
		 }

		 private readonly ObservedClusterMembers _observedClusterMembers;
		 private readonly HighAvailabilityMemberStateMachine _stateMachine;

		 public ClusterMembers( ObservedClusterMembers observedClusterMembers, HighAvailabilityMemberStateMachine stateMachine )
		 {
			  this._observedClusterMembers = observedClusterMembers;
			  this._stateMachine = stateMachine;
		 }

		 public virtual ClusterMember CurrentMember
		 {
			 get
			 {
				  ClusterMember currentMember = _observedClusterMembers.CurrentMember;
				  if ( currentMember == null )
				  {
						return null;
				  }
				  HighAvailabilityMemberState currentState = _stateMachine.CurrentState;
				  return UpdateRole( currentMember, currentState );
			 }
		 }

		 public virtual string CurrentMemberRole
		 {
			 get
			 {
				  ClusterMember currentMember = CurrentMember;
				  return ( currentMember == null ) ? HighAvailabilityModeSwitcher.UNKNOWN : currentMember.HARole;
			 }
		 }

		 public virtual IEnumerable<ClusterMember> Members
		 {
			 get
			 {
				  return GetActualMembers( _observedClusterMembers.Members );
			 }
		 }

		 public virtual IEnumerable<ClusterMember> AliveMembers
		 {
			 get
			 {
				  return GetActualMembers( _observedClusterMembers.AliveMembers );
			 }
		 }

		 private IEnumerable<ClusterMember> GetActualMembers( IEnumerable<ClusterMember> members )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ClusterMember currentMember = getCurrentMember();
			  ClusterMember currentMember = CurrentMember;
			  if ( currentMember == null )
			  {
					return members;
			  }
			  return Iterables.map( member => currentMember.InstanceId.Equals( member.InstanceId ) ? currentMember : member, members );
		 }

		 private static ClusterMember UpdateRole( ClusterMember member, HighAvailabilityMemberState state )
		 {
			  switch ( state.innerEnumValue )
			  {
			  case HighAvailabilityMemberState.InnerEnum.MASTER:
					return member.AvailableAs( HighAvailabilityModeSwitcher.MASTER, member.HAUri, member.StoreId );
			  case HighAvailabilityMemberState.InnerEnum.SLAVE:
					return member.AvailableAs( HighAvailabilityModeSwitcher.SLAVE, member.HAUri, member.StoreId );
			  default:
					return member.Unavailable();
			  }
		 }

		 public override string ToString()
		 {
			  StringBuilder buf = new StringBuilder();
			  foreach ( ClusterMember clusterMember in Members )
			  {
					buf.Append( "  " ).Append( clusterMember.InstanceId ).Append( ":" ).Append( clusterMember.HARole ).Append( " (is alive = " ).Append( clusterMember.Alive ).Append( ")" ).Append( format( "%n" ) );
			  }
			  return buf.ToString();
		 }
	}

}