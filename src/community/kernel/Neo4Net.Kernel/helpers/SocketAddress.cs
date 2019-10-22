using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Helpers
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;

	/// <summary>
	/// Socket address derived from configuration.
	/// There is no network awareness at all, just stores the raw configuration exactly as it comes.
	/// </summary>
	public class SocketAddress
	{
		 private static readonly ICollection<string> _wildcards = asSet( "0.0.0.0", "::" );

		 private readonly string _hostname;
		 private readonly int _port;

		 public SocketAddress( string hostname, int port )
		 {
			  if ( string.ReferenceEquals( hostname, null ) )
			  {
					throw new System.ArgumentException( "hostname cannot be null" );
			  }
			  if ( hostname.Contains( "[" ) || hostname.Contains( "]" ) )
			  {
					throw new System.ArgumentException( "hostname cannot contain '[' or ']'" );
			  }

			  this._hostname = hostname;
			  this._port = port;
		 }

		 /// <returns> hostname or IP address; we don't care. </returns>
		 public virtual string Hostname
		 {
			 get
			 {
				  return _hostname;
			 }
		 }

		 public virtual int Port
		 {
			 get
			 {
				  return _port;
			 }
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public virtual InetSocketAddress SocketAddressConflict()
		 {
			  return new InetSocketAddress( _hostname, _port );
		 }

		 public virtual bool Wildcard
		 {
			 get
			 {
				  return _wildcards.Contains( _hostname );
			 }
		 }

		 public virtual bool IPv6
		 {
			 get
			 {
				  return IsIPv6( _hostname );
			 }
		 }

		 public override string ToString()
		 {
			  return Format( _hostname, _port );
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
			  SocketAddress that = ( SocketAddress ) o;
			  return _port == that._port && Objects.Equals( _hostname, that._hostname );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _hostname, _port );
		 }

		 public static string Format( java.net.SocketAddress address )
		 {
			  if ( address is InetSocketAddress )
			  {
					InetSocketAddress inetSocketAddress = ( InetSocketAddress ) address;
					return Format( inetSocketAddress.HostString, inetSocketAddress.Port );
			  }
			  return address.ToString();
		 }

		 public static string Format( string hostname, int port )
		 {
			  return string.format( IsIPv6( hostname ) ? "[%s]:%s" : "%s:%s", hostname, port );
		 }

		 private static bool isIPv6( string hostname )
		 {
			  return hostname.Contains( ":" );
		 }
	}

}