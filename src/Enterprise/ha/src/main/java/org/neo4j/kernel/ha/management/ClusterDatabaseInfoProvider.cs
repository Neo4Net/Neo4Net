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
namespace Neo4Net.Kernel.ha.management
{

	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using ClusterMember = Neo4Net.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using LastTxIdGetter = Neo4Net.Kernel.impl.core.LastTxIdGetter;
	using ClusterDatabaseInfo = Neo4Net.management.ClusterDatabaseInfo;
	using ClusterMemberInfo = Neo4Net.management.ClusterMemberInfo;

	public class ClusterDatabaseInfoProvider
	{
		 private readonly ClusterMembers _members;
		 private readonly LastTxIdGetter _txIdGetter;
		 private readonly LastUpdateTime _lastUpdateTime;

		 public ClusterDatabaseInfoProvider( ClusterMembers members, LastTxIdGetter txIdGetter, LastUpdateTime lastUpdateTime )
		 {
			  this._members = members;
			  this._txIdGetter = txIdGetter;
			  this._lastUpdateTime = lastUpdateTime;
		 }

		 public virtual ClusterDatabaseInfo Info
		 {
			 get
			 {
				  ClusterMember currentMember = _members.CurrentMember;
				  if ( currentMember == null )
				  {
						return null;
				  }
   
				  System.Func<object, string> nullSafeToString = from => from == null ? "" : from.ToString();
   
				  return new ClusterDatabaseInfo( new ClusterMemberInfo( currentMember.InstanceId.ToString(), currentMember.HAUri != null, true, currentMember.HARole, Iterables.asArray(typeof(string), Iterables.map(nullSafeToString, currentMember.RoleURIs)), Iterables.asArray(typeof(string), Iterables.map(nullSafeToString, currentMember.Roles)) ), _txIdGetter.LastTxId, _lastUpdateTime.getLastUpdateTime() );
			 }
		 }
	}

}