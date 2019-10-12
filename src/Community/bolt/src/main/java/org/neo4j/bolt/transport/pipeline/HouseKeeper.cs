using System;

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
namespace Neo4Net.Bolt.transport.pipeline
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using EventExecutorGroup = io.netty.util.concurrent.EventExecutorGroup;

	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Log = Neo4Net.Logging.Log;

	public class HouseKeeper : ChannelInboundHandlerAdapter
	{
		 private readonly BoltConnection _connection;
		 private readonly Log _log;
		 private bool _failed;

		 public HouseKeeper( BoltConnection connection, Log log )
		 {
			  this._connection = connection;
			  this._log = log;
		 }

		 public override void ChannelInactive( ChannelHandlerContext ctx )
		 {
			  _connection.stop();
		 }

		 public override void ExceptionCaught( ChannelHandlerContext ctx, Exception cause )
		 {
			  if ( _failed || IsShuttingDown( ctx ) )
			  {
					return;
			  }
			  _failed = true; // log only the first exception to not pollute the log

			  try
			  {
					// Netty throws a NativeIoException on connection reset - directly importing that class
					// caused a host of linking errors, because it depends on JNI to work. Hence, we just
					// test on the message we know we'll get.
					if ( Exceptions.contains( cause, e => e.Message.contains( "Connection reset by peer" ) ) )
					{
						 _log.warn( "Fatal error occurred when handling a client connection, " + "remote peer unexpectedly closed connection: %s", ctx.channel() );
					}
					else
					{
						 _log.error( "Fatal error occurred when handling a client connection: " + ctx.channel(), cause );
					}
			  }
			  finally
			  {
					ctx.close();
			  }
		 }

		 private static bool IsShuttingDown( ChannelHandlerContext ctx )
		 {
			  EventExecutorGroup eventLoopGroup = ctx.executor().parent();
			  return eventLoopGroup != null && eventLoopGroup.ShuttingDown;
		 }
	}

}