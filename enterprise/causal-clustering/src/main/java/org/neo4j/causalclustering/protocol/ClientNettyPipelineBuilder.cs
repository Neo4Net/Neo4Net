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
namespace Org.Neo4j.causalclustering.protocol
{
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using LengthFieldBasedFrameDecoder = io.netty.handler.codec.LengthFieldBasedFrameDecoder;
	using LengthFieldPrepender = io.netty.handler.codec.LengthFieldPrepender;

	using Log = Org.Neo4j.Logging.Log;

	public class ClientNettyPipelineBuilder : NettyPipelineBuilder<ProtocolInstaller_Orientation_Client, ClientNettyPipelineBuilder>
	{
		 private const int LENGTH_FIELD_BYTES = 4;

		 internal ClientNettyPipelineBuilder( ChannelPipeline pipeline, Log log ) : base( pipeline, log )
		 {
		 }

		 public override ClientNettyPipelineBuilder AddFraming()
		 {
			  Add( "frame_encoder", new LengthFieldPrepender( LENGTH_FIELD_BYTES ) );
			  Add( "frame_decoder", new LengthFieldBasedFrameDecoder( int.MaxValue, 0, LENGTH_FIELD_BYTES, 0, LENGTH_FIELD_BYTES ) );
			  return this;
		 }
	}

}