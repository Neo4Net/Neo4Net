﻿/*
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
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using ChannelHandlerContext = org.jboss.netty.channel.ChannelHandlerContext;
	using MessageEvent = org.jboss.netty.channel.MessageEvent;
	using SimpleChannelHandler = org.jboss.netty.channel.SimpleChannelHandler;

	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;

	/// <summary>
	/// This Netty handler will report through a monitor how many bytes are read/written.
	/// </summary>
	public class MonitorChannelHandler : SimpleChannelHandler
	{
		 private ByteCounterMonitor _byteCounterMonitor;

		 public MonitorChannelHandler( ByteCounterMonitor byteCounterMonitor )
		 {
			  this._byteCounterMonitor = byteCounterMonitor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void messageReceived(org.jboss.netty.channel.ChannelHandlerContext ctx, org.jboss.netty.channel.MessageEvent e) throws Exception
		 public override void MessageReceived( ChannelHandlerContext ctx, MessageEvent e )
		 {
			  if ( e.Message is ChannelBuffer )
			  {
					_byteCounterMonitor.bytesRead( ( ( ChannelBuffer ) e.Message ).readableBytes() );
			  }

			  base.MessageReceived( ctx, e );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeRequested(org.jboss.netty.channel.ChannelHandlerContext ctx, org.jboss.netty.channel.MessageEvent e) throws Exception
		 public override void WriteRequested( ChannelHandlerContext ctx, MessageEvent e )
		 {
			  if ( e.Message is ChannelBuffer )
			  {
					_byteCounterMonitor.bytesWritten( ( ( ChannelBuffer ) e.Message ).readableBytes() );
			  }

			  base.WriteRequested( ctx, e );
		 }
	}

}