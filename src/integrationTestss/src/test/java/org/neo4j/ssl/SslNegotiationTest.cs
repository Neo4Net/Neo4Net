/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Ssl
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using SslParameters = Neo4Net.Ssl.SslContextFactory.SslParameters;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ssl.SslContextFactory.SslParameters.protocols;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ssl.SslResourceBuilder.selfSignedKeyId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class SslNegotiationTest
	public class SslNegotiationTest
	{
		 private const string OLD_CIPHER_A = "SSL_RSA_WITH_NULL_SHA";
		 private const string OLD_CIPHER_B = "SSL_RSA_WITH_RC4_128_MD5";
		 private const string OLD_CIPHER_C = "SSL_RSA_WITH_3DES_EDE_CBC_SHA";

		 private const string NEW_CIPHER_A = "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA";
		 private const string NEW_CIPHER_B = "TLS_RSA_WITH_AES_128_CBC_SHA256";
		 private const string NEW_CIPHER_C = "TLS_DHE_RSA_WITH_AES_128_CBC_SHA256";

		 private const string TLSV10 = "TLSv1";
		 private const string TLSV11 = "TLSv1.1";
		 private const string TLSV12 = "TLSv1.2";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.DefaultFileSystemRule fsRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FsRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public TestSetup setup;
		 public TestSetup Setup;

		 private SecureServer _server;
		 private SecureClient _client;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Object[] params()
		 public static object[] Params()
		 {
			  return new TestSetup[]
			  {
				  new TestSetup( protocols( TLSV12 ).ciphers( NEW_CIPHER_A ), protocols( TLSV12 ).ciphers( NEW_CIPHER_A ), true, TLSV12, NEW_CIPHER_A ),
				  new TestSetup( protocols( TLSV10 ).ciphers( OLD_CIPHER_A ), protocols( TLSV11 ).ciphers( OLD_CIPHER_A ), false ),
				  new TestSetup( protocols( TLSV11 ).ciphers( OLD_CIPHER_A ), protocols( TLSV10 ).ciphers( OLD_CIPHER_A ), false ),
				  new TestSetup( protocols( TLSV11 ).ciphers( NEW_CIPHER_A ), protocols( TLSV12 ).ciphers( NEW_CIPHER_A ), false ),
				  new TestSetup( protocols( TLSV12 ).ciphers( NEW_CIPHER_A ), protocols( TLSV11 ).ciphers( NEW_CIPHER_A ), false ),
				  new TestSetup( protocols( TLSV10 ).ciphers( OLD_CIPHER_A ), protocols( TLSV10 ).ciphers( OLD_CIPHER_B ), false ),
				  new TestSetup( protocols( TLSV11 ).ciphers( NEW_CIPHER_A ), protocols( TLSV11 ).ciphers( NEW_CIPHER_B ), false ),
				  new TestSetup( protocols( TLSV12 ).ciphers( NEW_CIPHER_A ), protocols( TLSV12 ).ciphers( NEW_CIPHER_B ), false ),
				  new TestSetup( protocols( TLSV12 ).ciphers( NEW_CIPHER_B, NEW_CIPHER_A ), protocols( TLSV12 ).ciphers( NEW_CIPHER_C, NEW_CIPHER_A ), true, TLSV12, NEW_CIPHER_A )
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _client != null )
			  {
					_client.disconnect();
			  }
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNegotiateCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNegotiateCorrectly()
		 {
			  SslResource sslServerResource = selfSignedKeyId( 0 ).trustKeyId( 1 ).install( TestDir.directory( "server" ) );
			  SslResource sslClientResource = selfSignedKeyId( 1 ).trustKeyId( 0 ).install( TestDir.directory( "client" ) );

			  _server = new SecureServer( SslContextFactory.MakeSslPolicy( sslServerResource, Setup.serverParams ) );

			  _server.start();
			  _client = new SecureClient( SslContextFactory.MakeSslPolicy( sslClientResource, Setup.clientParams ) );
			  _client.connect( _server.port() );

			  try
			  {
					assertTrue( _client.sslHandshakeFuture().get(1, MINUTES).Active );
					assertEquals( Setup.expectedProtocol, _client.protocol() );
					assertEquals( Setup.expectedCipher.Substring( 4 ), _client.ciphers().Substring(4) ); // cut away SSL_ or TLS_
			  }
			  catch ( ExecutionException )
			  {
					assertFalse( Setup.expectedSuccess );
			  }
		 }

		 private class TestSetup
		 {
			  internal readonly SslParameters ServerParams;
			  internal readonly SslParameters ClientParams;

			  internal readonly bool ExpectedSuccess;
			  internal readonly string ExpectedProtocol;
			  internal readonly string ExpectedCipher;

			  internal TestSetup( SslParameters serverParams, SslParameters clientParams, bool expectedSuccess ) : this( serverParams, clientParams, expectedSuccess, null, null )
			  {
			  }

			  internal TestSetup( SslParameters serverParams, SslParameters clientParams, bool expectedSuccess, string expectedProtocol, string expectedCipher )
			  {
					this.ServerParams = serverParams;
					this.ClientParams = clientParams;
					this.ExpectedSuccess = expectedSuccess;
					this.ExpectedProtocol = expectedProtocol;
					this.ExpectedCipher = expectedCipher;
			  }

			  public override string ToString()
			  {
					return "TestSetup{" + "serverParams=" + ServerParams + ", clientParams=" + ClientParams + ", expectedSuccess=" + ExpectedSuccess +
							 ", expectedProtocol='" + ExpectedProtocol + '\'' + ", expectedCipher='" + ExpectedCipher + '\'' + '}';
			  }
		 }
	}

}