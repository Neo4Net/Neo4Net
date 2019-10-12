using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.consensus.protocol.v2
{
	using Channel = io.netty.channel.Channel;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;


	using ContentTypeProtocol = Org.Neo4j.causalclustering.messaging.marshalling.v2.ContentTypeProtocol;
	using ContentTypeDispatcher = Org.Neo4j.causalclustering.messaging.marshalling.v2.decoding.ContentTypeDispatcher;
	using DecodingDispatcher = Org.Neo4j.causalclustering.messaging.marshalling.v2.decoding.DecodingDispatcher;
	using RaftMessageComposer = Org.Neo4j.causalclustering.messaging.marshalling.v2.decoding.RaftMessageComposer;
	using ReplicatedContentDecoder = Org.Neo4j.causalclustering.messaging.marshalling.v2.decoding.ReplicatedContentDecoder;
	using Org.Neo4j.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Org.Neo4j.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using ProtocolInstaller_Orientation = Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class RaftProtocolServerInstallerV2 : ProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>
	{
		 private const Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols APPLICATION_PROTOCOL = Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols.Raft_2;
		 private readonly LogProvider _logProvider;

		 public class Factory : Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Factory<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server, RaftProtocolServerInstallerV2>
		 {
			  public Factory( ChannelInboundHandler raftMessageHandler, NettyPipelineBuilderFactory pipelineBuilderFactory, LogProvider logProvider ) : base( APPLICATION_PROTOCOL, outerInstance.modifiers -> new RaftProtocolServerInstallerV2( raftMessageHandler, pipelineBuilderFactory, outerInstance.modifiers, logProvider ) )
			  {
			  }
		 }

		 private readonly ChannelInboundHandler _raftMessageHandler;
		 private readonly NettyPipelineBuilderFactory _pipelineBuilderFactory;
		 private readonly IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> _modifiers;
		 private readonly Log _log;

		 public RaftProtocolServerInstallerV2( ChannelInboundHandler raftMessageHandler, NettyPipelineBuilderFactory pipelineBuilderFactory, IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> modifiers, LogProvider logProvider )
		 {
			  this._raftMessageHandler = raftMessageHandler;
			  this._pipelineBuilderFactory = pipelineBuilderFactory;
			  this._modifiers = modifiers;
			  this._logProvider = logProvider;
			  this._log = this._logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void install(io.netty.channel.Channel channel) throws Exception
		 public override void Install( Channel channel )
		 {

			  ContentTypeProtocol contentTypeProtocol = new ContentTypeProtocol();
			  DecodingDispatcher decodingDispatcher = new DecodingDispatcher( contentTypeProtocol, _logProvider );
			  _pipelineBuilderFactory.server( channel, _log ).modify( _modifiers ).addFraming().add("raft_content_type_dispatcher", new ContentTypeDispatcher(contentTypeProtocol)).add("raft_component_decoder", decodingDispatcher).add("raft_content_decoder", new ReplicatedContentDecoder(contentTypeProtocol)).add("raft_message_composer", new RaftMessageComposer(Clock.systemUTC())).add("raft_handler", _raftMessageHandler).install();
		 }

		 public override Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocol ApplicationProtocol()
		 {
			  return APPLICATION_PROTOCOL;
		 }

		 public override ICollection<ICollection<Org.Neo4j.causalclustering.protocol.Protocol_ModifierProtocol>> Modifiers()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _modifiers.Select( ModifierProtocolInstaller::protocols ).ToList();
		 }
	}

}