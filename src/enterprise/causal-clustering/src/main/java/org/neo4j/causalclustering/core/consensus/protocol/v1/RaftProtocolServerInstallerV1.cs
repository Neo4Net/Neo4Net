using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.protocol.v1
{
	using Channel = io.netty.channel.Channel;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;


	using CoreReplicatedContentMarshal = Neo4Net.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using RaftMessageDecoder = Neo4Net.causalclustering.messaging.marshalling.v1.RaftMessageDecoder;
	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ProtocolInstaller_Orientation = Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class RaftProtocolServerInstallerV1 : ProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>
	{
		 private const Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols APPLICATION_PROTOCOL = Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols.Raft_1;

		 public class Factory : Neo4Net.causalclustering.protocol.ProtocolInstaller_Factory<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server, RaftProtocolServerInstallerV1>
		 {
			  public Factory( ChannelInboundHandler raftMessageHandler, NettyPipelineBuilderFactory pipelineBuilderFactory, LogProvider logProvider ) : base( APPLICATION_PROTOCOL, outerInstance.modifiers -> new RaftProtocolServerInstallerV1( raftMessageHandler, pipelineBuilderFactory, outerInstance.modifiers, logProvider ) )
			  {
			  }
		 }

		 private readonly ChannelInboundHandler _raftMessageHandler;
		 private readonly NettyPipelineBuilderFactory _pipelineBuilderFactory;
		 private readonly IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> _modifiers;
		 private readonly Log _log;

		 public RaftProtocolServerInstallerV1( ChannelInboundHandler raftMessageHandler, NettyPipelineBuilderFactory pipelineBuilderFactory, IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> modifiers, LogProvider logProvider )
		 {
			  this._raftMessageHandler = raftMessageHandler;
			  this._pipelineBuilderFactory = pipelineBuilderFactory;
			  this._modifiers = modifiers;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void install(io.netty.channel.Channel channel) throws Exception
		 public override void Install( Channel channel )
		 {
			  _pipelineBuilderFactory.server( channel, _log ).modify( _modifiers ).addFraming().add("raft_decoder", new RaftMessageDecoder(CoreReplicatedContentMarshal.marshaller(), Clock.systemUTC())).add("raft_handler", _raftMessageHandler).install();
		 }

		 public override Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol ApplicationProtocol()
		 {
			  return APPLICATION_PROTOCOL;
		 }

		 public override ICollection<ICollection<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>> Modifiers()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _modifiers.Select( ModifierProtocolInstaller::protocols ).ToList();
		 }
	}

}