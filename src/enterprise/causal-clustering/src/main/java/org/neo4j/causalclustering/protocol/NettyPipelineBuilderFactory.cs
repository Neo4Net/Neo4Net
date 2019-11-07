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
namespace Neo4Net.causalclustering.protocol
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;

	using PipelineWrapper = Neo4Net.causalclustering.handlers.PipelineWrapper;
	using Log = Neo4Net.Logging.Log;

	public class NettyPipelineBuilderFactory
	{
		 private readonly PipelineWrapper _wrapper;

		 public NettyPipelineBuilderFactory( PipelineWrapper wrapper )
		 {
			  this._wrapper = wrapper;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClientNettyPipelineBuilder client(io.netty.channel.Channel channel, Neo4Net.logging.Log log) throws Exception
		 public virtual ClientNettyPipelineBuilder Client( Channel channel, Log log )
		 {
			  return Create( channel, NettyPipelineBuilder.Client( channel.pipeline(), log ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ServerNettyPipelineBuilder server(io.netty.channel.Channel channel, Neo4Net.logging.Log log) throws Exception
		 public virtual ServerNettyPipelineBuilder Server( Channel channel, Log log )
		 {
			  return Create( channel, NettyPipelineBuilder.Server( channel.pipeline(), log ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <O extends Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation, BUILDER extends NettyPipelineBuilder<O,BUILDER>> BUILDER create(io.netty.channel.Channel channel, BUILDER nettyPipelineBuilder) throws Exception
		 private BUILDER Create<O, BUILDER>( Channel channel, BUILDER nettyPipelineBuilder ) where O : Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation where BUILDER : NettyPipelineBuilder<O,BUILDER>
		 {
			  int i = 0;
			  foreach ( ChannelHandler handler in _wrapper.handlersFor( channel ) )
			  {
					nettyPipelineBuilder.add( string.Format( "{0}_{1:D}", _wrapper.name(), i ), handler );
					i++;
			  }
			  return nettyPipelineBuilder;
		 }
	}

}