/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Ssl
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.ssl.SslResourceBuilder.caSignedKeyId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.ssl.SslResourceBuilder.selfSignedKeyId;

	public class SslTrustTest
	{
		 private const int UNRELATED_ID = 5; // SslContextFactory requires us to trust something

		 private static readonly sbyte[] _request = new sbyte[] { 1, 2, 3, 4 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDir = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.fs.DefaultFileSystemRule fsRule = new Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FsRule = new DefaultFileSystemRule();

		 private SecureServer _server;
		 private SecureClient _client;
		 private ByteBuf _expected;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _expected != null )
			  {
					_expected.release();
			  }
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
//ORIGINAL LINE: @Test public void partiesWithMutualTrustShouldCommunicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PartiesWithMutualTrustShouldCommunicate()
		 {
			  // given
			  SslResource sslServerResource = selfSignedKeyId( 0 ).trustKeyId( 1 ).install( TestDir.directory( "server" ) );
			  SslResource sslClientResource = selfSignedKeyId( 1 ).trustKeyId( 0 ).install( TestDir.directory( "client" ) );

			  _server = new SecureServer( SslContextFactory.MakeSslPolicy( sslServerResource ) );

			  _server.start();
			  _client = new SecureClient( SslContextFactory.MakeSslPolicy( sslClientResource ) );
			  _client.connect( _server.port() );

			  // when
			  ByteBuf request = ByteBufAllocator.DEFAULT.buffer().writeBytes(_request);
			  _client.channel().writeAndFlush(request);

			  // then
			  _expected = ByteBufAllocator.DEFAULT.buffer().writeBytes(SecureServer.Response);
			  _client.sslHandshakeFuture().get(1, MINUTES);
			  _client.assertResponse( _expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void partiesWithMutualTrustThroughCAShouldCommunicate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PartiesWithMutualTrustThroughCAShouldCommunicate()
		 {
			  // given
			  SslResource sslServerResource = caSignedKeyId( 0 ).trustSignedByCA().install(TestDir.directory("server"));
			  SslResource sslClientResource = caSignedKeyId( 1 ).trustSignedByCA().install(TestDir.directory("client"));

			  _server = new SecureServer( SslContextFactory.MakeSslPolicy( sslServerResource ) );

			  _server.start();
			  _client = new SecureClient( SslContextFactory.MakeSslPolicy( sslClientResource ) );
			  _client.connect( _server.port() );

			  // when
			  ByteBuf request = ByteBufAllocator.DEFAULT.buffer().writeBytes(_request);
			  _client.channel().writeAndFlush(request);

			  // then
			  _expected = ByteBufAllocator.DEFAULT.buffer().writeBytes(SecureServer.Response);
			  _client.sslHandshakeFuture().get(1, MINUTES);
			  _client.assertResponse( _expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverShouldNotCommunicateWithUntrustedClient() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ServerShouldNotCommunicateWithUntrustedClient()
		 {
			  // given
			  SslResource sslClientResource = selfSignedKeyId( 1 ).trustKeyId( 0 ).install( TestDir.directory( "client" ) );
			  SslResource sslServerResource = selfSignedKeyId( 0 ).trustKeyId( UNRELATED_ID ).install( TestDir.directory( "server" ) );

			  _server = new SecureServer( SslContextFactory.MakeSslPolicy( sslServerResource ) );

			  _server.start();
			  _client = new SecureClient( SslContextFactory.MakeSslPolicy( sslClientResource ) );
			  _client.connect( _server.port() );

			  try
			  {
					// when
					_client.sslHandshakeFuture().get(1, MINUTES);
					fail();
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( SSLException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientShouldNotCommunicateWithUntrustedServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClientShouldNotCommunicateWithUntrustedServer()
		 {
			  // given
			  SslResource sslClientResource = selfSignedKeyId( 0 ).trustKeyId( UNRELATED_ID ).install( TestDir.directory( "client" ) );
			  SslResource sslServerResource = selfSignedKeyId( 1 ).trustKeyId( 0 ).install( TestDir.directory( "server" ) );

			  _server = new SecureServer( SslContextFactory.MakeSslPolicy( sslServerResource ) );

			  _server.start();
			  _client = new SecureClient( SslContextFactory.MakeSslPolicy( sslClientResource ) );
			  _client.connect( _server.port() );

			  try
			  {
					_client.sslHandshakeFuture().get(1, MINUTES);
					fail();
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( SSLException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void partiesWithMutualTrustThroughCAShouldNotCommunicateWhenServerRevoked() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PartiesWithMutualTrustThroughCAShouldNotCommunicateWhenServerRevoked()
		 {
			  // given
			  SslResource sslServerResource = caSignedKeyId( 0 ).trustSignedByCA().install(TestDir.directory("server"));
			  SslResource sslClientResource = caSignedKeyId( 1 ).trustSignedByCA().revoke(0).install(TestDir.directory("client"));

			  _server = new SecureServer( SslContextFactory.MakeSslPolicy( sslServerResource ) );

			  _server.start();
			  _client = new SecureClient( SslContextFactory.MakeSslPolicy( sslClientResource ) );
			  _client.connect( _server.port() );

			  try
			  {
					_client.sslHandshakeFuture().get(1, MINUTES);
					fail( "Server should have been revoked" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( SSLException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void partiesWithMutualTrustThroughCAShouldNotCommunicateWhenClientRevoked() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PartiesWithMutualTrustThroughCAShouldNotCommunicateWhenClientRevoked()
		 {
			  // given
			  SslResource sslServerResource = caSignedKeyId( 0 ).trustSignedByCA().revoke(1).install(TestDir.directory("server"));
			  SslResource sslClientResource = caSignedKeyId( 1 ).trustSignedByCA().install(TestDir.directory("client"));

			  _server = new SecureServer( SslContextFactory.MakeSslPolicy( sslServerResource ) );

			  _server.start();
			  _client = new SecureClient( SslContextFactory.MakeSslPolicy( sslClientResource ) );
			  _client.connect( _server.port() );

			  try
			  {
					_client.sslHandshakeFuture().get(1, MINUTES);
					fail( "Client should have been revoked" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( SSLException ) ) );
			  }
		 }
	}

}