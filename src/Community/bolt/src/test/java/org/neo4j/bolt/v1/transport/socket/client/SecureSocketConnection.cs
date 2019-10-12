using System;
using System.Collections.Generic;

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

	public class SecureSocketConnection : SocketConnection
	{
		 private readonly ISet<X509Certificate> _serverCertificatesSeen = new HashSet<X509Certificate>();

		 public SecureSocketConnection()
		 {
			  Socket = CreateSecureSocket();
		 }

		 private Socket CreateSecureSocket()
		 {
			  try
			  {
					SSLContext context = SSLContext.getInstance( "TLS" );
					context.init( new KeyManager[0], new TrustManager[]{ new NaiveTrustManager( _serverCertificatesSeen.add ) }, new SecureRandom() );

					return context.SocketFactory.createSocket();
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 public virtual ISet<X509Certificate> ServerCertificatesSeen
		 {
			 get
			 {
				  return _serverCertificatesSeen;
			 }
		 }

	}

}