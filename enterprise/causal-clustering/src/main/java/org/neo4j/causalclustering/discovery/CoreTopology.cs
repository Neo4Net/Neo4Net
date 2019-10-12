using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.discovery
{

	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;


	public class CoreTopology : Topology<CoreServerInfo>
	{
		 public static readonly CoreTopology Empty = new CoreTopology( null, false, emptyMap() );

		 private readonly ClusterId _clusterId;
		 private readonly bool _canBeBootstrapped;
		 private readonly IDictionary<MemberId, CoreServerInfo> _coreMembers;

		 public CoreTopology( ClusterId clusterId, bool canBeBootstrapped, IDictionary<MemberId, CoreServerInfo> coreMembers )
		 {
			  this._clusterId = clusterId;
			  this._canBeBootstrapped = canBeBootstrapped;
			  this._coreMembers = new Dictionary<MemberId, CoreServerInfo>( coreMembers );
		 }

		 public override IDictionary<MemberId, CoreServerInfo> Members()
		 {
			  return _coreMembers;
		 }

		 public virtual ClusterId ClusterId()
		 {
			  return _clusterId;
		 }

		 public virtual bool CanBeBootstrapped()
		 {
			  return _canBeBootstrapped;
		 }

		 public override string ToString()
		 {
			  return format( "{clusterId=%s, bootstrappable=%s, coreMembers=%s}", _clusterId, CanBeBootstrapped(), _coreMembers );
		 }

		 public virtual Optional<MemberId> RandomCoreMemberId()
		 {
			  if ( _coreMembers.Count == 0 )
			  {
					return null;
			  }
			  return _coreMembers.Keys.Skip( ThreadLocalRandom.current().Next(_coreMembers.Count) ).First();
		 }

		 public override CoreTopology FilterTopologyByDb( string dbName )
		 {
			  IDictionary<MemberId, CoreServerInfo> filteredMembers = FilterHostsByDb( Members(), dbName );

			  return new CoreTopology( ClusterId(), CanBeBootstrapped(), filteredMembers );
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
			  CoreTopology that = ( CoreTopology ) o;
			  return Objects.Equals( _coreMembers, that._coreMembers );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _coreMembers );
		 }
	}

}