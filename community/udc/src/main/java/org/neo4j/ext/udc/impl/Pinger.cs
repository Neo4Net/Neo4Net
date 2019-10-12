using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Ext.Udc.impl
{

	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.PING;

	internal class Pinger
	{
		 private readonly HostnamePort _address;
		 private readonly UdcInformationCollector _collector;
		 private int _pingCount;

		 internal Pinger( HostnamePort address, UdcInformationCollector collector )
		 {
			  this._address = address;
			  this._collector = collector;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void ping() throws java.io.IOException
		 internal virtual void Ping()
		 {
			  _pingCount++;

			  IDictionary<string, string> usageDataMap = _collector.UdcParams;

			  StringBuilder uri = new StringBuilder( "http://" + _address + "/" + "?" );

			  foreach ( KeyValuePair<string, string> entry in usageDataMap.SetOfKeyValuePairs() )
			  {
					uri.Append( entry.Key );
					uri.Append( "=" );
					uri.Append( URLEncoder.encode( entry.Value, StandardCharsets.UTF_8.name() ) );
					uri.Append( "+" );
			  }

			  uri.Append( PING + "=" ).Append( _pingCount );

			  URL url = new URL( uri.ToString() );
			  URLConnection con = url.openConnection();

			  con.DoInput = true;
			  con.DoOutput = false;
			  con.UseCaches = false;
			  con.connect();

			  con.InputStream.close();
		 }

		 internal virtual int PingCount
		 {
			 get
			 {
				  return _pingCount;
			 }
		 }
	}

}