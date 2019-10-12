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
namespace Org.Neo4j.causalclustering.routing.load_balancing.plugins.server_policies
{

	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;

	/// <summary>
	/// Hold the server information that is interesting for load balancing purposes.
	/// </summary>
	public class ServerInfo
	{
		 private readonly AdvertisedSocketAddress _boltAddress;
		 private MemberId _memberId;
		 private ISet<string> _groups;

		 public ServerInfo( AdvertisedSocketAddress boltAddress, MemberId memberId, ISet<string> groups )
		 {
			  this._boltAddress = boltAddress;
			  this._memberId = memberId;
			  this._groups = groups;
		 }

		 public virtual MemberId MemberId()
		 {
			  return _memberId;
		 }

		 internal virtual AdvertisedSocketAddress BoltAddress()
		 {
			  return _boltAddress;
		 }

		 internal virtual ISet<string> Groups()
		 {
			  return _groups;
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
			  ServerInfo that = ( ServerInfo ) o;
			  return Objects.Equals( _boltAddress, that._boltAddress ) && Objects.Equals( _memberId, that._memberId ) && Objects.Equals( _groups, that._groups );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _boltAddress, _memberId, _groups );
		 }

		 public override string ToString()
		 {
			  return "ServerInfo{" + "boltAddress=" + _boltAddress + ", memberId=" + _memberId + ", groups=" + _groups + '}';
		 }
	}

}