using System;

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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using SocketChannel = io.netty.channel.socket.SocketChannel;


	using SimpleNettyChannel = Neo4Net.causalclustering.messaging.SimpleNettyChannel;
	using ChildInitializer = Neo4Net.causalclustering.net.ChildInitializer;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using SocketAddress = Neo4Net.Helpers.SocketAddress;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class HandshakeServerInitializer : ChildInitializer
	{
		 private readonly Log _log;
		 private readonly ApplicationProtocolRepository _applicationProtocolRepository;
		 private readonly ModifierProtocolRepository _modifierProtocolRepository;
		 private readonly ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server> _protocolInstallerRepository;
		 private readonly NettyPipelineBuilderFactory _pipelineBuilderFactory;

		 public HandshakeServerInitializer( ApplicationProtocolRepository applicationProtocolRepository, ModifierProtocolRepository modifierProtocolRepository, ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server> protocolInstallerRepository, NettyPipelineBuilderFactory pipelineBuilderFactory, LogProvider logProvider )
		 {
			  this._log = logProvider.getLog( this.GetType() );
			  this._applicationProtocolRepository = applicationProtocolRepository;
			  this._modifierProtocolRepository = modifierProtocolRepository;
			  this._protocolInstallerRepository = protocolInstallerRepository;
			  this._pipelineBuilderFactory = pipelineBuilderFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void initChannel(io.netty.channel.socket.SocketChannel ch) throws Exception
		 public override void InitChannel( SocketChannel ch )
		 {
			  _log.info( "Installing handshake server local %s remote %s", ch.localAddress(), ch.remoteAddress() );

			  _pipelineBuilderFactory.server( ch, _log ).addFraming().add("handshake_server_encoder", new ServerMessageEncoder()).add("handshake_server_decoder", new ServerMessageDecoder()).add("handshake_server", CreateHandshakeServer(ch)).install();
		 }

		 private NettyHandshakeServer CreateHandshakeServer( SocketChannel channel )
		 {
			  HandshakeServer handshakeServer = new HandshakeServer(_applicationProtocolRepository, _modifierProtocolRepository, new SimpleNettyChannel(channel, _log)
			 );

			  handshakeServer.ProtocolStackFuture().whenComplete((protocolStack, failure) => onHandshakeComplete(protocolStack, channel, failure));
			  channel.closeFuture().addListener(f =>
			  {
			  try
			  {
				  channel.parent().pipeline().fireUserEventTriggered(new ServerHandshakeFinishedEvent_Closed(ToSocketAddress(channel)));
			  }
			  catch ( RejectedExecutionException )
			  {
			  }
			  });
			  return new NettyHandshakeServer( handshakeServer );
		 }

		 private void OnHandshakeComplete( ProtocolStack protocolStack, SocketChannel channel, Exception failure )
		 {
			  if ( failure != null )
			  {
					_log.error( "Error when negotiating protocol stack", failure );
					return;
			  }

			  try
			  {
					_protocolInstallerRepository.installerFor( protocolStack ).install( channel );
					channel.parent().pipeline().fireUserEventTriggered(new ServerHandshakeFinishedEvent_Created(ToSocketAddress(channel), protocolStack));
			  }
			  catch ( Exception t )
			  {
					_log.error( "Error installing protocol stack", t );
			  }
		 }

		 private SocketAddress ToSocketAddress( SocketChannel channel )
		 {
			  InetSocketAddress inetSocketAddress = channel.remoteAddress();
			  return new SocketAddress( inetSocketAddress.HostString, inetSocketAddress.Port );
		 }
	}

}