﻿using System;

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
namespace Org.Neo4j.causalclustering.handlers
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandlerAdapter = io.netty.channel.ChannelHandlerAdapter;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;

	public class ExceptionMonitoringHandler : ChannelHandlerAdapter
	{
		 public interface Monitor
		 {
			  void ExceptionCaught( Channel channel, Exception cause );
		 }

		 private readonly Monitor _monitor;

		 public ExceptionMonitoringHandler( Monitor monitor )
		 {
			  this._monitor = monitor;
		 }

		 public override void ExceptionCaught( ChannelHandlerContext ctx, Exception cause )
		 {
			  if ( ctx != null )
			  {
					_monitor.exceptionCaught( ctx.channel(), cause );
					ctx.fireExceptionCaught( cause );
			  }
			  else
			  {
					_monitor.exceptionCaught( null, cause );
			  }
		 }
	}

}