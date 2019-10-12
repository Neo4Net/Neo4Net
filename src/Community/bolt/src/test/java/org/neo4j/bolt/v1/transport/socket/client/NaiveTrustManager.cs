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
namespace Neo4Net.Bolt.v1.transport.socket.client
{

	/// <summary>
	/// Trust self-signed certificates </summary>
	public class NaiveTrustManager : X509TrustManager
	{
		 private readonly System.Action<X509Certificate> _certSink;

		 public NaiveTrustManager( System.Action<X509Certificate> certSink )
		 {
			  this._certSink = certSink;
		 }

		 public override void CheckClientTrusted( X509Certificate[] x509Certificates, string s )
		 {
			  foreach ( X509Certificate x509Certificate in x509Certificates )
			  {
					_certSink.accept( x509Certificate );
			  }
		 }

		 public override void CheckServerTrusted( X509Certificate[] x509Certificates, string s )
		 {
			  foreach ( X509Certificate x509Certificate in x509Certificates )
			  {
					_certSink.accept( x509Certificate );
			  }
		 }

		 public override X509Certificate[] AcceptedIssuers
		 {
			 get
			 {
				  return null;
			 }
		 }
	}

}