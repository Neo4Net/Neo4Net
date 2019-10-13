using System;

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
namespace Neo4Net.management
{
	/// @deprecated high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release. 
	[Obsolete("high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release."), Serializable]
	public class ClusterDatabaseInfo : ClusterMemberInfo
	{
		 private long _lastCommittedTxId;
		 private long _lastUpdateTime;

		 public ClusterDatabaseInfo( ClusterMemberInfo memberInfo, long lastCommittedTxId, long lastUpdateTime ) : base( memberInfo.InstanceId, memberInfo.Available, memberInfo.Alive, memberInfo.HaRole, memberInfo.Uris, memberInfo.Roles )
		 {
			  this._lastCommittedTxId = lastCommittedTxId;
			  this._lastUpdateTime = lastUpdateTime;
		 }

		 public virtual long LastCommittedTxId
		 {
			 get
			 {
				  return _lastCommittedTxId;
			 }
		 }

		 public virtual long LastUpdateTime
		 {
			 get
			 {
				  return _lastUpdateTime;
			 }
		 }
	}

}