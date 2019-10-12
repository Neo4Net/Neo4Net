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
namespace Org.Neo4j.causalclustering.core.consensus.protocol.v1
{
	using Channel = io.netty.channel.Channel;


	using CoreReplicatedContentMarshal = Org.Neo4j.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using RaftMessageEncoder = Org.Neo4j.causalclustering.messaging.marshalling.v1.RaftMessageEncoder;
	using Org.Neo4j.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Org.Neo4j.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using ProtocolInstaller_Orientation = Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class RaftProtocolClientInstallerV1 : ProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>
	{
		 private const Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols APPLICATION_PROTOCOL = Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols.Raft_1;

		 public class Factory : Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Factory<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client, RaftProtocolClientInstallerV1>
		 {
			  public Factory( NettyPipelineBuilderFactory clientPipelineBuilderFactory, LogProvider logProvider ) : base( APPLICATION_PROTOCOL, outerInstance.modifiers -> new RaftProtocolClientInstallerV1( clientPipelineBuilderFactory, outerInstance.modifiers, logProvider ) )
			  {
			  }
		 }

		 private readonly IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> _modifiers;
		 private readonly Log _log;
		 private readonly NettyPipelineBuilderFactory _clientPipelineBuilderFactory;

		 public RaftProtocolClientInstallerV1( NettyPipelineBuilderFactory clientPipelineBuilderFactory, IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> modifiers, LogProvider logProvider )
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