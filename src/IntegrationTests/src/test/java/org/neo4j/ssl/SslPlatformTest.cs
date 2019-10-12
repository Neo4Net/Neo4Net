/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using SslProvider = io.netty.handler.ssl.SslProvider;
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isOneOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ssl.SslResourceBuilder.selfSignedKeyId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("FieldCanBeLocal") public class SslPlatformTest
	public class SslPlatformTest
	{
		 private static readonly sbyte[] _request = new sbyte[] { 1, 2, 3, 4 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.DefaultFileSystemRule fsRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FsRule = new DefaultFileSystemRule();

		 private SecureServer _server;
		 private SecureClient _client;
		 private ByteBuf _expected;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportOpenSSLOnSupportedPlatforms() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportOpenSSLOnSupportedPlatforms()
		 {
			  // depends on the statically linked uber-jar with boring ssl: http://netty.io/wiki/forked-tomcat-native.html
			  assumeTrue( SystemUtils.IS_OS_WINDOWS || SystemUtils.IS_OS_LINUX || SystemUtils.IS_OS_MAC_OSX );
			  assumeThat( System.getProperty( "os.arch" ), equalTo( "x86_64" ) );
			  assumeThat( SystemUtils.JAVA_VENDOR, isOneOf( "Oracle Corporation", "Sun Microsystems Inc." ) );

			  // given
			  SslResource sslServerResource = selfSignedKeyId( 0 ).trustKeyId( 1 ).install( TestDir.directory( "server" ) );
			  SslResource sslClientResource = selfSignedKeyId( 1 ).trustKeyId( 0 ).install( TestDir.directory( "client" ) );

			  _server = new SecureServer( SslContextFactory.MakeSslPolicy( sslServerResource, SslProvider.OPENSSL ) );

			  _server.start();
			  _client = new SecureClient( SslContextFactory.MakeSslPolicy( sslClientResource, SslProvider.OPENSSL ) );
			  _client.connect( _server.port() );

			  // when
			  ByteBuf request = ByteBufAllocator.DEFAULT.buffer().writeBytes(_request);
			  _client.channel().writeAndFlush(request);

			  // then
			  _expected = ByteBufAllocator.DEFAULT.buffer().writeBytes(SecureServer.Response);
			  _client.sslHandshakeFuture().get(1, MINUTES);
			  _client.assertResponse( _expected );
		 }
	}

}