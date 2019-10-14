using System;
using System.Text;

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
	using StringUtils = org.apache.commons.lang3.StringUtils;


	/// <summary>
	/// Represents a hostname and port, optionally with a port range.
	/// Examples: myhost, myhost:1234, myhost:1234-1240, :1234, :1234-1240
	/// </summary>
	public class HostnamePort
	{
		 private readonly string _host;
		 private readonly int[] _ports;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public HostnamePort(String hostnamePort) throws IllegalArgumentException
		 public HostnamePort( string hostnamePort )
		 {
			  Objects.requireNonNull( hostnamePort );

			  string[] parts = SplitHostAndPort( hostnamePort );
			  if ( parts.Length == 1 )
			  {
					_host = StringUtils.defaultIfBlank( parts[0], null );
					_ports = new int[]{ 0, 0 };
			  }
			  else if ( parts.Length == 2 )
			  {
					_host = StringUtils.defaultIfBlank( parts[0], null );

					string[] portStrings = parts[1].Split( "-", true );
					_ports = new int[2];

					if ( portStrings.Length == 1 )
					{
						 _ports[0] = _ports[1] = int.Parse( portStrings[0] );
					}
					else if ( portStrings.Length == 2 )
					{
						 _ports[0] = int.Parse( portStrings[0] );
						 _ports[1] = int.Parse( portStrings[1] );
					}
					else
					{
						 throw new System.ArgumentException( format( "Cannot have more than two port ranges: %s", hostnamePort ) );
					}
			  }
			  else
			  {
					throw new System.ArgumentException( hostnamePort );
			  }
		 }

		 public HostnamePort( string host, int port ) : this( host, port, port )
		 {
		 }

		 public HostnamePort( string host, int portFrom, int portTo )
		 {
			  this._host = host;
			  _ports = new int[]{ portFrom, portTo };
		 }

		 /// <summary>
		 /// The host part, or {@code null} if not given.
		 /// </summary>
		 /// <returns> the host part, or {@code null} if not given </returns>
		 public virtual string Host
		 {
			 get
			 {
				  return _host;
			 }
		 }

		 public static string GetHostAddress( string host, string defaultHost )
		 {
			  if ( string.ReferenceEquals( host, null ) )
			  {
					return defaultHost;
			  }
			  else
			  {
					return host;
			  }
		 }

		 public virtual string getHost( string defaultHost )
		 {
			  return GetHostAddress( _host, defaultHost );
		 }

		 /// <summary>
		 /// The port range as two ints. If only one port given, then both ints have the same value.
		 /// If no port range is given, then the array has {0,0} as value.
		 /// </summary>
		 /// <returns> the port range as two ints, which may have the same value; if no port range has been given both ints are {@code 0} </returns>
		 public virtual int[] Ports
		 {
			 get
			 {
				  return _ports;
			 }
		 }

		 /// <summary>
		 /// The first port, or 0 if no port was given.
		 /// </summary>
		 /// <returns> the first port or {@code 0} if no port was given </returns>
		 public virtual int Port
		 {
			 get
			 {
				  return _ports[0];
			 }
		 }

		 public virtual bool Range
		 {
			 get
			 {
				  return _ports[0] != _ports[1];
			 }
		 }

		 public override string ToString()
		 {
			  return ToString( null );
		 }

		 public virtual string ToString( string defaultHost )
		 {
			  StringBuilder builder = new StringBuilder();
			  string host = GetHost( defaultHost );
			  if ( !string.ReferenceEquals( host, null ) )
			  {
					builder.Append( host );
			  }

			  if ( Port != 0 )
			  {
					builder.Append( ':' );
					builder.Append( Port );
					if ( Range )
					{
						 builder.Append( '-' ).Append( Ports[1] );
					}
			  }

			  return builder.ToString();
		 }

		 public virtual bool Matches( URI toMatch )
		 {
			  bool result = false;
			  for ( int port = _ports[0]; port <= _ports[1]; port++ )
			  {
					if ( port == toMatch.Port )
					{
						 result = true;
						 break;
					}
			  }

			  if ( string.ReferenceEquals( _host, null ) && toMatch.Host == null )
			  {
					return result;
			  }
			  else if ( string.ReferenceEquals( _host, null ) )
			  {
					return false;
			  }

			  // URI may contain IP, so make sure we check it too by converting ours, if necessary
			  string toMatchHost = toMatch.Host;

			  // this tries to match hostnames as they are at first, then tries to extract and match ip addresses of both
			  return result && ( _host.Equals( toMatchHost, StringComparison.OrdinalIgnoreCase ) || GetHost( null ).Equals( GetHostAddress( toMatchHost, toMatchHost ), StringComparison.OrdinalIgnoreCase ) );
		 }

		 private static string[] SplitHostAndPort( string hostnamePort )
		 {
			  hostnamePort = hostnamePort.Trim();

			  int indexOfSchemaSeparator = hostnamePort.IndexOf( "://", StringComparison.Ordinal );
			  if ( indexOfSchemaSeparator != -1 )
			  {
					hostnamePort = hostnamePort.Substring( indexOfSchemaSeparator + 3 );
			  }

			  bool isIPv6HostPort = hostnamePort.StartsWith( "[", StringComparison.Ordinal ) && hostnamePort.Contains( "]" );
			  if ( isIPv6HostPort )
			  {
					int splitIndex = hostnamePort.IndexOf( ']' ) + 1;

					string host = hostnamePort.Substring( 0, splitIndex );
					string port = hostnamePort.Substring( splitIndex );
					if ( StringUtils.isNotBlank( port ) )
					{
						 port = port.Substring( 1 ); // remove ':'
						 return new string[]{ host, port };
					}
					return new string[]{ host };
			  }
			  return hostnamePort.Split( ":", true );
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
			  HostnamePort that = ( HostnamePort ) o;
			  return Objects.Equals( _host, that._host ) && Arrays.Equals( _ports, that._ports );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _host, Arrays.GetHashCode( _ports ) );
		 }
	}

}