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
namespace Neo4Net.cluster.com
{
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelHandlerContext = org.jboss.netty.channel.ChannelHandlerContext;
	using MessageEvent = org.jboss.netty.channel.MessageEvent;
	using Test = org.junit.Test;


	using Neo4Net.cluster.com.message;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;


	public class NetworkReceiverTest
	{
		 internal const int PORT = 1234;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetURIWithWildCard()
		 public virtual void TestGetURIWithWildCard()
		 {
			  NetworkReceiver networkReceiver = new NetworkReceiver( mock( typeof( NetworkReceiver.Monitor ) ), mock( typeof( NetworkReceiver.Configuration ) ), mock( typeof( LogProvider ) ) );

			  // Wildcard should not be resolved here
			  const string wildCard = "0.0.0.0";
			  URI uri = networkReceiver.GetURI( new InetSocketAddress( wildCard, PORT ) );

			  assertEquals( wildCard + " does not match Uri host: " + uri.Host, wildCard, uri.Host );
			  assertEquals( PORT, uri.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetURIWithLocalHost()
		 public virtual void TestGetURIWithLocalHost()
		 {
			  NetworkReceiver networkReceiver = new NetworkReceiver( mock( typeof( NetworkReceiver.Monitor ) ), mock( typeof( NetworkReceiver.Configuration ) ), mock( typeof( LogProvider ) ) );

			  // We should NOT do a reverse DNS lookup for hostname. It might not be routed properly.
			  URI uri = networkReceiver.GetURI( new InetSocketAddress( "localhost", PORT ) );

			  assertEquals( "Uri host is not localhost ip: " + uri.Host, "127.0.0.1", uri.Host );
			  assertEquals( PORT, uri.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMessageReceivedOriginFix() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMessageReceivedOriginFix()
		 {
			  LogProvider logProvider = mock( typeof( LogProvider ) );
			  Log log = mock( typeof( Log ) );
			  when( logProvider.GetLog( typeof( NetworkReceiver ) ) ).thenReturn( log );
			  NetworkReceiver networkReceiver = new NetworkReceiver( mock( typeof( NetworkReceiver.Monitor ) ), mock( typeof( NetworkReceiver.Configuration ) ), logProvider );

			  // This defines where message is coming from
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.InetSocketAddress inetSocketAddress = new java.net.InetSocketAddress("localhost", PORT);
			  InetSocketAddress inetSocketAddress = new InetSocketAddress( "localhost", PORT );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.jboss.netty.channel.Channel channel = mock(org.jboss.netty.channel.Channel.class);
			  Channel channel = mock( typeof( Channel ) );
			  when( channel.LocalAddress ).thenReturn( inetSocketAddress );
			  when( channel.RemoteAddress ).thenReturn( inetSocketAddress );

			  ChannelHandlerContext ctx = mock( typeof( ChannelHandlerContext ) );
			  when( ctx.Channel ).thenReturn( channel );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.com.message.Message message = org.neo4j.cluster.com.message.Message.to(() -> "test", new java.net.URI("cluster://anywhere"));
			  Message message = Message.to( () => "test", new URI("cluster://anywhere") );

			  MessageEvent messageEvent = mock( typeof( MessageEvent ) );
			  when( messageEvent.RemoteAddress ).thenReturn( inetSocketAddress );
			  when( messageEvent.Message ).thenReturn( message );
			  when( messageEvent.Channel ).thenReturn( channel );

			  // the original HEADER_FROM header should be ignored
			  message.setHeader( Message.HEADER_FROM, "cluster://someplace:1234" );

			  new Neo4Net.cluster.com.NetworkReceiver.MessageReceiver( networkReceiver ).messageReceived( ctx, messageEvent );

			  assertEquals( "HEADER_FROM header should have been changed to visible ip address: " + message.getHeader( Message.HEADER_FROM ), "cluster://127.0.0.1:1234", message.getHeader( Message.HEADER_FROM ) );
		 }
	}

}