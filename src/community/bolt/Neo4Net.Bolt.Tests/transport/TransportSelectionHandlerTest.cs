using System;

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
namespace Neo4Net.Bolt.transport
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using Test = org.junit.Test;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.AssertableLogProvider.inLog;

	public class TransportSelectionHandlerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogOnUnexpectedExceptionsAndClosesContext() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogOnUnexpectedExceptionsAndClosesContext()
		 {
			  // Given
			  ChannelHandlerContext context = ChannelHandlerContextMock();
			  AssertableLogProvider logging = new AssertableLogProvider();
			  TransportSelectionHandler handler = new TransportSelectionHandler( null, null, false, false, logging, null );

			  // When
			  Exception cause = new Exception( "Oh no!" );
			  handler.ExceptionCaught( context, cause );

			  // Then
			  verify( context ).close();
			  logging.AssertExactly( inLog( typeof( TransportSelectionHandler ) ).error( equalTo( "Fatal error occurred when initialising pipeline: " + context.channel() ), sameInstance(cause) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogConnectionResetErrorsAtWarningLevelAndClosesContext() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogConnectionResetErrorsAtWarningLevelAndClosesContext()
		 {
			  // Given
			  ChannelHandlerContext context = ChannelHandlerContextMock();
			  AssertableLogProvider logging = new AssertableLogProvider();
			  TransportSelectionHandler handler = new TransportSelectionHandler( null, null, false, false, logging, null );

			  IOException connResetError = new IOException( "Connection reset by peer" );

			  // When
			  handler.ExceptionCaught( context, connResetError );

			  // Then
			  verify( context ).close();
			  logging.AssertExactly( inLog( typeof( TransportSelectionHandler ) ).warn( "Fatal error occurred when initialising pipeline, " + "remote peer unexpectedly closed connection: %s", context.channel() ) );
		 }

		 private static ChannelHandlerContext ChannelHandlerContextMock()
		 {
			  Channel channel = mock( typeof( Channel ) );
			  ChannelHandlerContext context = mock( typeof( ChannelHandlerContext ) );
			  when( context.channel() ).thenReturn(channel);
			  return context;
		 }
	}

}