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
namespace Org.Neo4j.Ssl
{

	public class EssentialEngineModifications : System.Func<SSLEngine, SSLEngine>
	{
		 private readonly string[] _tlsVersions;
		 private readonly bool _isClient;

		 public EssentialEngineModifications( string[] tlsVersions, bool isClient )
		 {
			  this._tlsVersions = tlsVersions;
			  this._isClient = isClient;
		 }

		 /// <summary>
		 /// Apply engine modifications that will exist in any use-case of TLS
		 /// </summary>
		 /// <param name="sslEngine"> the ssl engine that will be used for the connections. Is mutated. </param>
		 /// <returns> the updated sslEngine (should be the same as the original, but don't rely on that) </returns>
		 public override SSLEngine Apply( SSLEngine sslEngine )
		 {
			  if ( _tlsVersions != null )
			  {
					sslEngine.EnabledProtocols = _tlsVersions;
			  }
			  sslEngine.UseClientMode = _isClient;
			  return sslEngine;
		 }
	}

}