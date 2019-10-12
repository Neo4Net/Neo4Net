using System;

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
namespace Org.Neo4j.com
{
	using ServerBootstrap = org.jboss.netty.bootstrap.ServerBootstrap;
	using Channel = org.jboss.netty.channel.Channel;
	using ChannelException = org.jboss.netty.channel.ChannelException;
	using Test = org.junit.Test;

	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;


	public class PortRangeSocketBinderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReThrowExceptionIfCannotBindToPort()
		 public virtual void ShouldReThrowExceptionIfCannotBindToPort()
		 {
			  // given
			  HostnamePort localhost = new HostnamePort( "localhost", 9000 );
			  ServerBootstrap bootstrap = mock( typeof( ServerBootstrap ) );

			  when( bootstrap.bind( new InetSocketAddress( "localhost", 9000 ) ) ).thenThrow( new ChannelException() );

			  try
			  {
					// when
					( new PortRangeSocketBinder( bootstrap ) ).BindToFirstAvailablePortInRange( localhost );
					fail( "should have thrown ChannelException" );
			  }
			  catch ( ChannelException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReThrowExceptionIfCannotBindToAnyOfThePortsInTheRange()
		 public virtual void ShouldReThrowExceptionIfCannotBindToAnyOfThePortsInTheRange()
		 {
			  // given
			  HostnamePort localhost = new HostnamePort( "localhost", 9000, 9002 );
			  ServerBootstrap bootstrap = mock( typeof( ServerBootstrap ) );

			  when( bootstrap.bind( new InetSocketAddress( "localhost", 9000 ) ) ).thenThrow( new ChannelException( "Failed to bind to: 9000" ) );
			  when( bootstrap.bind( new InetSocketAddress( "localhost", 9001 ) ) ).thenThrow( new ChannelException( "Failed to bind to: 9001" ) );
			  when( bootstrap.bind( new InetSocketAddress( "localhost", 9002 ) ) ).thenThrow( new ChannelException( "Failed to bind to: 9002" ) );

			  try
			  {
					// when
					( new PortRangeSocketBinder( bootstrap ) ).BindToFirstAvailablePortInRange( localhost );
					fail( "should have thrown ChannelException" );
			  }
			  catch ( ChannelException ex )
			  {
					// expected
					assertEquals( 2, SuppressedExceptions( ex ) );
			  }
		 }

		 private int SuppressedExceptions( Exception throwable )
		 {
			  int suppressed = 0;
			  foreach ( Exception ignored in throwable.Suppressed )
			  {
					suppressed++;
					suppressed = suppressed + SuppressedExceptions( ignored );

			  }
			  return suppressed;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnChannelAndSocketIfPortIsFree()
		 public virtual void ShouldReturnChannelAndSocketIfPortIsFree()
		 {
			  // given
			  HostnamePort localhost = new HostnamePort( "localhost", 9000 );
			  ServerBootstrap bootstrap = mock( typeof( ServerBootstrap ) );
			  Channel channel = mock( typeof( Channel ) );

			  when( bootstrap.bind( new InetSocketAddress( "localhost", 9000 ) ) ).thenReturn( channel );

			  // when
			  Connection connection = ( new PortRangeSocketBinder( bootstrap ) ).BindToFirstAvailablePortInRange( localhost );

			  //then
			  assertEquals( channel, connection.Channel );
			  assertEquals( new InetSocketAddress( "localhost", 9000 ), connection.SocketAddress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnChannelAndSocketIfAnyPortsAreFree()
		 public virtual void ShouldReturnChannelAndSocketIfAnyPortsAreFree()
		 {
			  // given
			  HostnamePort localhost = new HostnamePort( "localhost", 9000, 9001 );
			  ServerBootstrap bootstrap = mock( typeof( ServerBootstrap ) );
			  Channel channel = mock( typeof( Channel ) );

			  when( bootstrap.bind( new InetSocketAddress( "localhost", 9000 ) ) ).thenThrow( new ChannelException() );
			  when( bootstrap.bind( new InetSocketAddress( "localhost", 9001 ) ) ).thenReturn( channel );

			  // when
			  Connection connection = ( new PortRangeSocketBinder( bootstrap ) ).BindToFirstAvailablePortInRange( localhost );

			  //then
			  assertEquals( channel, connection.Channel );
			  assertEquals( new InetSocketAddress( localhost.Host, 9001 ), connection.SocketAddress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnChannelAndSocketIfPortRangeIsInverted()
		 public virtual void ShouldReturnChannelAndSocketIfPortRangeIsInverted()
		 {
			  // given
			  HostnamePort localhost = new HostnamePort( "localhost", 9001, 9000 );
			  ServerBootstrap bootstrap = mock( typeof( ServerBootstrap ) );
			  Channel channel = mock( typeof( Channel ) );

			  when( bootstrap.bind( new InetSocketAddress( "localhost", 9001 ) ) ).thenReturn( channel );

			  // when
			  Connection connection = ( new PortRangeSocketBinder( bootstrap ) ).BindToFirstAvailablePortInRange( localhost );

			  //then
			  assertEquals( channel, connection.Channel );
			  assertEquals( new InetSocketAddress( localhost.Host, 9001 ), connection.SocketAddress );

		 }
	}

}