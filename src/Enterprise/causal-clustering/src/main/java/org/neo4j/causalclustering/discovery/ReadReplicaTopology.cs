using System.Collections.Generic;

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

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class ReadReplicaTopology : Topology<ReadReplicaInfo>
	{
		 public static readonly ReadReplicaTopology Empty = new ReadReplicaTopology( emptyMap() );

		 private readonly IDictionary<MemberId, ReadReplicaInfo> _readReplicaMembers;

		 public ReadReplicaTopology( IDictionary<MemberId, ReadReplicaInfo> readReplicaMembers )
		 {
			  this._readReplicaMembers = readReplicaMembers;
		 }

		 public virtual ICollection<ReadReplicaInfo> AllMemberInfo()
		 {
			  return _readReplicaMembers.Values;
		 }

		 public override IDictionary<MemberId, ReadReplicaInfo> Members()
		 {
			  return _readReplicaMembers;
		 }

		 public override string ToString()
		 {
			  return string.Format( "{{readReplicas={0}}}", _readReplicaMembers );
		 }

		 public override ReadReplicaTopology FilterTopologyByDb( string dbName )
		 {
			  IDictionary<MemberId, ReadReplicaInfo> filteredMembers = FilterHostsByDb( Members(), dbName );

			  return new ReadReplicaTopology( filteredMembers );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  ReadReplicaTopology that = ( ReadReplicaTopology ) o;
			  return Objects.Equals( _readReplicaMembers, that._readReplicaMembers );
		 }

		 public override int GetHashCode()
		 {

			  return Objects.hash( _readReplicaMembers );
		 }
	}

}