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
namespace Neo4Net.Kernel.impl.util
{

	using HostnamePort = Neo4Net.Helpers.HostnamePort;

	public class OptionalHostnamePort
	{
		 private Optional<string> _hostname;
		 private int? _port;
		 private int? _upperRangePort;

		 public OptionalHostnamePort( Optional<string> hostname, int? port, int? upperRangePort )
		 {
			  this._hostname = hostname;
			  this._port = port;
			  this._upperRangePort = upperRangePort;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public OptionalHostnamePort(@Nullable String hostname, @Nullable Integer port, @Nullable Integer upperRangePort)
		 public OptionalHostnamePort( string hostname, Integer port, Integer upperRangePort )
		 {
			  this._hostname = Optional.ofNullable( hostname );
			  this._port = Optional.ofNullable( port );
			  this._upperRangePort = Optional.ofNullable( upperRangePort );
		 }

		 public virtual Optional<string> Hostname
		 {
			 get
			 {
				  return _hostname;
			 }
		 }

		 public virtual int? Port
		 {
			 get
			 {
				  return _port;
			 }
		 }

		 public virtual int? UpperRangePort
		 {
			 get
			 {
				  return _upperRangePort;
			 }
		 }
		 public override string ToString()
		 {
			  return string.Format( "OptionalHostnamePort<{0},{1},{2}>", _hostname, _port, _upperRangePort );
		 }
	}

}