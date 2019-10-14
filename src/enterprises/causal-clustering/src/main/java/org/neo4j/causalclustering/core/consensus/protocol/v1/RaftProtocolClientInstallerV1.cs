using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.consensus.protocol.v1
{
	using Channel = io.netty.channel.Channel;


	using CoreReplicatedContentMarshal = Neo4Net.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using RaftMessageEncoder = Neo4Net.causalclustering.messaging.marshalling.v1.RaftMessageEncoder;
	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ProtocolInstaller_Orientation = Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class RaftProtocolClientInstallerV1 : ProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>
	{
		 private const Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols APPLICATION_PROTOCOL = Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols.Raft_1;

		 public class Factory : Neo4Net.causalclustering.protocol.ProtocolInstaller_Factory<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client, RaftProtocolClientInstallerV1>
		 {
			  public Factory( NettyPipelineBuilderFactory clientPipelineBuilderFactory, LogProvider logProvider ) : base( APPLICATION_PROTOCOL, outerInstance.modifiers -> new RaftProtocolClientInstallerV1( clientPipelineBuilderFactory, outerInstance.modifiers, logProvider ) )
			  {
			  }
		 }

		 private readonly IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> _modifiers;
		 private readonly Log _log;
		 private readonly NettyPipelineBuilderFactory _clientPipelineBuilderFactory;

		 public RaftProtocolClientInstallerV1( NettyPipelineBuilderFactory clientPipelineBuilderFactory, IList<ModifierProtocolInstaller<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> modifiers, LogProvider logProvider )
		 {
			  this._modifiers = modifiers;
			  this._log = logProvider.getLog( this.GetType() );
			  this._clientPipelineBuilderFactory = clientPipelineBuilderFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void install(io.netty.channel.Channel channel) throws Exception
		 public override void Install( Channel channel )
		 {
			  _clientPipelineBuilderFactory.client( channel, _log ).modify( _modifiers ).addFraming().add("raft_encoder", new RaftMessageEncoder(CoreReplicatedContentMarshal.marshaller())).install();
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