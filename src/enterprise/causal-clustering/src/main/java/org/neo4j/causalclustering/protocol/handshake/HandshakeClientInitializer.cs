using System;

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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using Channel = io.netty.channel.Channel;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using SocketChannel = io.netty.channel.socket.SocketChannel;


	using ExponentialBackoffStrategy = Neo4Net.causalclustering.helper.ExponentialBackoffStrategy;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using ReconnectingChannel = Neo4Net.causalclustering.messaging.ReconnectingChannel;
	using SimpleNettyChannel = Neo4Net.causalclustering.messaging.SimpleNettyChannel;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;


	public class HandshakeClientInitializer : ChannelInitializer<SocketChannel>
	{
		 private readonly ApplicationProtocolRepository _applicationProtocolRepository;
		 private readonly ModifierProtocolRepository _modifierProtocolRepository;
		 private readonly Duration _timeout;
		 private readonly ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client> _protocolInstaller;
		 private readonly NettyPipelineBuilderFactory _pipelineBuilderFactory;
		 private readonly TimeoutStrategy _handshakeDelay;
		 private readonly Log _debugLog;
		 private readonly Log _userLog;

		 public HandshakeClientInitializer( ApplicationProtocolRepository applicationProtocolRepository, ModifierProtocolRepository modifierProtocolRepository, ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client> protocolInstallerRepository, NettyPipelineBuilderFactory pipelineBuilderFactory, Duration handshakeTimeout, LogProvider debugLogProvider, LogProvider userLogProvider )
		 {
			  this._debugLog = debugLogProvider.getLog( this.GetType() );
			  this._userLog = userLogProvider.getLog( this.GetType() );
			  this._applicationProtocolRepository = applicationProtocolRepository;
			  this._modifierProtocolRepository = modifierProtocolRepository;
			  this._timeout = handshakeTimeout;
			  this._protocolInstaller = protocolInstallerRepository;
			  this._pipelineBuilderFactory = pipelineBuilderFactory;
			  this._handshakeDelay = new ExponentialBackoffStrategy( 1, 2000, MILLISECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void installHandlers(io.netty.channel.Channel channel, HandshakeClient handshakeClient) throws Exception
		 private void InstallHandlers( Channel channel, HandshakeClient handshakeClient )
		 {
			  _pipelineBuilderFactory.client( channel, _debugLog ).addFraming().add("handshake_client_encoder", new ClientMessageEncoder()).add("handshake_client_decoder", new ClientMessageDecoder()).add("handshake_client", new NettyHandshakeClient(handshakeClient)).addGate(msg => !(msg is ServerMessage)).install();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void initChannel(io.netty.channel.socket.SocketChannel channel) throws Exception
		 protected internal override void InitChannel( SocketChannel channel )
		 {
			  HandshakeClient handshakeClient = new HandshakeClient();
			  InstallHandlers( channel, handshakeClient );

			  _debugLog.info( "Scheduling handshake (and timeout) local %s remote %s", channel.localAddress(), channel.remoteAddress() );

			  ScheduleHandshake( channel, handshakeClient, _handshakeDelay.newTimeout() );
			  ScheduleTimeout( channel, handshakeClient );
		 }

		 /// <summary>
		 /// schedules the handshake initiation after the connection attempt
		 /// </summary>
		 private void ScheduleHandshake( SocketChannel ch, HandshakeClient handshakeClient, Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout handshakeDelay )
		 {
			  ch.eventLoop().schedule(() =>
			  {
				if ( ch.Active )
				{
					 InitiateHandshake( ch, handshakeClient );
				}
				else if ( ch.Open )
				{
					 handshakeDelay.Increment();
					 ScheduleHandshake( ch, handshakeClient, handshakeDelay );
				}
				else
				{
					 handshakeClient.FailIfNotDone( "Channel closed" );
				}
			  }, handshakeDelay.Millis, MILLISECONDS);
		 }

		 private void ScheduleTimeout( SocketChannel ch, HandshakeClient handshakeClient )
		 {
			  ch.eventLoop().schedule(() =>
			  {
			  if ( handshakeClient.FailIfNotDone( "Timed out after " + _timeout ) )
			  {
				  _debugLog.warn( "Failed handshake after timeout" );
			  }
			  }, _timeout.toMillis(), TimeUnit.MILLISECONDS);
		 }

		 private void InitiateHandshake( Channel channel, HandshakeClient handshakeClient )
		 {
			  _debugLog.info( "Initiating handshake local %s remote %s", channel.localAddress(), channel.remoteAddress() );

			  SimpleNettyChannel channelWrapper = new SimpleNettyChannel( channel, _debugLog );
			  CompletableFuture<ProtocolStack> handshake = handshakeClient.Initiate( channelWrapper, _applicationProtocolRepository, _modifierProtocolRepository );

			  handshake.whenComplete( ( protocolStack, failure ) => onHandshakeComplete( protocolStack, channel, failure ) );
		 }

		 private void OnHandshakeComplete( ProtocolStack protocolStack, Channel channel, Exception failure )
		 {
			  if ( failure != null )
			  {
					_debugLog.error( "Error when negotiating protocol stack", failure );
					channel.pipeline().fireUserEventTriggered(GateEvent.Failure);
					channel.close();
			  }
			  else
			  {
					try
					{
						 UserLog( protocolStack, channel );

						 _debugLog.info( "Installing " + protocolStack );
						 _protocolInstaller.installerFor( protocolStack ).install( channel );
						 channel.attr( ReconnectingChannel.PROTOCOL_STACK_KEY ).set( protocolStack );

						 channel.pipeline().fireUserEventTriggered(GateEvent.Success);
						 channel.flush();
					}
					catch ( Exception e )
					{
						 _debugLog.error( "Error installing pipeline", e );
						 channel.close();
					}
			  }
		 }

		 private void UserLog( ProtocolStack protocolStack, Channel channel )
		 {
			  _userLog.info( format( "Connected to %s [%s]", channel.remoteAddress(), protocolStack ) );
			  channel.closeFuture().addListener(f => _userLog.info(format("Lost connection to %s [%s]", channel.remoteAddress(), protocolStack)));
		 }
	}

}