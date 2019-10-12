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
namespace Org.Neo4j.causalclustering.protocol
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;

	using PipelineWrapper = Org.Neo4j.causalclustering.handlers.PipelineWrapper;
	using Log = Org.Neo4j.Logging.Log;

	public class NettyPipelineBuilderFactory
	{
		 private readonly PipelineWrapper _wrapper;

		 public NettyPipelineBuilderFactory( PipelineWrapper wrapper )
		 {
			  this._wrapper = wrapper;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClientNettyPipelineBuilder client(io.netty.channel.Channel channel, org.neo4j.logging.Log log) throws Exception
		 public virtual ClientNettyPipelineBuilder Client( Channel channel, Log log )
		 {
			  return Create( channel, NettyPipelineBuilder.Client( channel.pipeline(), log ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ServerNettyPipelineBuilder server(io.netty.channel.Channel channel, org.neo4j.logging.Log log) throws Exception
		 public virtual ServerNettyPipelineBuilder Server( Channel channel, Log log )
		 {
			  return Create( channel, NettyPipelineBuilder.Server( channel.pipeline(), log ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <O extends org.neo4j.causalclustering.protocol.ProtocolInstaller_Orientation, BUILDER extends NettyPipelineBuilder<O,BUILDER>> BUILDER create(io.netty.channel.Channel channel, BUILDER nettyPipelineBuilder) throws Exception
		 private BUILDER Create<O, BUILDER>( Channel channel, BUILDER nettyPipelineBuilder ) where O : Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation where BUILDER : NettyPipelineBuilder<O,BUILDER>
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