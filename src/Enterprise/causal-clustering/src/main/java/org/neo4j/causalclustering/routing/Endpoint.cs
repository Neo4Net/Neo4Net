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
namespace Neo4Net.causalclustering.routing
{

	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

	/// <summary>
	/// This class binds a certain role with an address and
	/// thus defines a reachable endpoint with defined capabilities.
	/// </summary>
	public class Endpoint
	{
		 private readonly AdvertisedSocketAddress _address;
		 private readonly Role _role;

		 public Endpoint( AdvertisedSocketAddress address, Role role )
		 {
			  this._address = address;
			  this._role = role;
		 }

		 public Endpoint( AdvertisedSocketAddress address, Role role, string dbName )
		 {
			  this._address = address;
			  this._role = role;
		 }

		 public virtual AdvertisedSocketAddress Address()
		 {
			  return _address;
		 }

		 public static Endpoint Write( AdvertisedSocketAddress address )
		 {
			  return new Endpoint( address, Role.Write );
		 }

		 public static Endpoint Read( AdvertisedSocketAddress address )
		 {
			  return new Endpoint( address, Role.Read );
		 }

		 public static Endpoint Route( AdvertisedSocketAddress address )
		 {
			  return new Endpoint( address, Role.Route );
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
			  Endpoint endPoint = ( Endpoint ) o;
			  return Objects.Equals( _address, endPoint._address ) && _role == endPoint._role;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _address, _role );
		 }

		 public override string ToString()
		 {
			  return "EndPoint{" + "address=" + _address + ", role=" + _role + '}';
		 }
	}

}