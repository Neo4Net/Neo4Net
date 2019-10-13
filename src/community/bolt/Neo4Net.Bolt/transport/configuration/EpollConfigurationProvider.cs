﻿using System;

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
namespace Neo4Net.Bolt.transport.configuration
{
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using ServerChannel = io.netty.channel.ServerChannel;
	using EpollEventLoopGroup = io.netty.channel.epoll.EpollEventLoopGroup;
	using EpollServerSocketChannel = io.netty.channel.epoll.EpollServerSocketChannel;

	public class EpollConfigurationProvider : ServerConfigurationProvider
	{
		 public static readonly EpollConfigurationProvider Instance = new EpollConfigurationProvider();

		 private EpollConfigurationProvider()
		 {
		 }

		 public override EventLoopGroup CreateEventLoopGroup( int numberOfThreads, ThreadFactory threadFactory )
		 {
			  return new EpollEventLoopGroup( numberOfThreads, threadFactory );
		 }

		 public virtual Type ChannelClass
		 {
			 get
			 {
				  return typeof( EpollServerSocketChannel );
			 }
		 }
	}

}