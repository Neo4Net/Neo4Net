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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using OperatorCreationException = org.bouncycastle.@operator.OperatorCreationException;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using SecureSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using PkiUtils = Neo4Net.Ssl.PkiUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.transport.integration.Neo4NetWithSocket.DEFAULT_CONNECTOR_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.tls_certificate_file;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.tls_key_file;

	public class CertificatesIT
	{
		 private static File _keyFile;
		 private static File _certFile;
		 private static PkiUtils _certFactory;
		 private static TransportTestUtil _util;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4NetWithSocket server = new Neo4NetWithSocket(getClass(), settings ->
		 public Neo4NetWithSocket Server = new Neo4NetWithSocket(this.GetType(), settings =>
		 {
		  settings.put( tls_certificate_file.name(), _certFile.AbsolutePath );
		  settings.put( tls_key_file.name(), _keyFile.AbsolutePath );
		  settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).type.name(), "BOLT" );
		  settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).enabled.name(), "true" );
		  settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).listen_address.name(), "localhost:0" );
		 });

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseConfiguredCertificate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseConfiguredCertificate()
		 {
			  // GIVEN
			  SecureSocketConnection connection = new SecureSocketConnection();
			  try
			  {
					// WHEN
					connection.Connect( Server.lookupConnector( DEFAULT_CONNECTOR_KEY ) ).send( _util.acceptedVersions( 1, 0, 0, 0 ) );

					// THEN
					ISet<X509Certificate> certificatesSeen = connection.ServerCertificatesSeen;
					assertThat( certificatesSeen, contains( LoadCertificateFromDisk() ) );
			  }
			  finally
			  {
					connection.Disconnect();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.security.cert.X509Certificate loadCertificateFromDisk() throws java.security.cert.CertificateException, java.io.IOException
		 private X509Certificate LoadCertificateFromDisk()
		 {
			  Certificate[] certificates = _certFactory.loadCertificates( _certFile );
			  assertThat( certificates.Length, equalTo( 1 ) );

			  return ( X509Certificate ) certificates[0];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUp() throws java.io.IOException, java.security.GeneralSecurityException, org.bouncycastle.operator.OperatorCreationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void SetUp()
		 {
			  _certFactory = new PkiUtils();
			  _keyFile = File.createTempFile( "key", "pem" );
			  _certFile = File.createTempFile( "key", "pem" );
			  _keyFile.deleteOnExit();
			  _certFile.deleteOnExit();

			  // make sure files are not there
			  _keyFile.delete();
			  _certFile.delete();

			  _certFactory.createSelfSignedCertificate( _certFile, _keyFile, "my.domain" );

			  _util = new TransportTestUtil( new Neo4NetPackV1() );
		 }

	}

}