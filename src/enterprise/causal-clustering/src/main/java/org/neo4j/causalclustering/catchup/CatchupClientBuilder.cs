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
namespace Neo4Net.causalclustering.catchup
{
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using SocketChannel = io.netty.channel.socket.SocketChannel;


	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Protocol_ApplicationProtocols = Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols;
	using Protocol_ModifierProtocols = Neo4Net.causalclustering.protocol.Protocol_ModifierProtocols;
	using Neo4Net.causalclustering.protocol;
	using ProtocolInstaller_Orientation_Client = Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client;
	using Neo4Net.causalclustering.protocol;
	using ApplicationProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ApplicationProtocolRepository;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using HandshakeClientInitializer = Neo4Net.causalclustering.protocol.handshake.HandshakeClientInitializer;
	using ModifierProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ModifierProtocolRepository;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.handlers.VoidPipelineWrapperFactory.VOID_WRAPPER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.CATCHUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.time.Clocks.systemClock;

	public class CatchupClientBuilder
	{
		 private Duration _handshakeTimeout = Duration.ofSeconds( 5 );
		 private LogProvider _debugLogProvider = NullLogProvider.Instance;
		 private LogProvider _userLogProvider = NullLogProvider.Instance;
		 private NettyPipelineBuilderFactory _pipelineBuilder = new NettyPipelineBuilderFactory( VOID_WRAPPER );
		 private ApplicationSupportedProtocols _catchupProtocols = new ApplicationSupportedProtocols( CATCHUP, emptyList() );
		 private ICollection<ModifierSupportedProtocols> _modifierProtocols = emptyList();
		 private Clock _clock = systemClock();
		 private long _inactivityTimeoutMillis = TimeUnit.SECONDS.toMillis( 10 );

		 public CatchupClientBuilder()
		 {
		 }

		 public CatchupClientBuilder( ApplicationSupportedProtocols catchupProtocols, ICollection<ModifierSupportedProtocols> modifierProtocols, NettyPipelineBuilderFactory pipelineBuilder, Duration handshakeTimeout, LogProvider debugLogProvider, LogProvider userLogProvider, Clock clock )
		 {
			  this._catchupProtocols = catchupProtocols;
			  this._modifierProtocols = modifierProtocols;
			  this._pipelineBuilder = pipelineBuilder;
			  this._handshakeTimeout = handshakeTimeout;
			  this._debugLogProvider = debugLogProvider;
			  this._userLogProvider = userLogProvider;
			  this._clock = clock;
		 }

		 public virtual CatchupClientBuilder CatchupProtocols( ApplicationSupportedProtocols catchupProtocols )
		 {
			  this._catchupProtocols = catchupProtocols;
			  return this;
		 }

		 public virtual CatchupClientBuilder ModifierProtocols( ICollection<ModifierSupportedProtocols> modifierProtocols )
		 {
			  this._modifierProtocols = modifierProtocols;
			  return this;
		 }

		 public virtual CatchupClientBuilder PipelineBuilder( NettyPipelineBuilderFactory pipelineBuilder )
		 {
			  this._pipelineBuilder = pipelineBuilder;
			  return this;
		 }

		 public virtual CatchupClientBuilder HandshakeTimeout( Duration handshakeTimeout )
		 {
			  this._handshakeTimeout = handshakeTimeout;
			  return this;
		 }

		 public virtual CatchupClientBuilder InactivityTimeoutMillis( long inactivityTimeoutMillis )
		 {
			  this._inactivityTimeoutMillis = inactivityTimeoutMillis;
			  return this;
		 }

		 public virtual CatchupClientBuilder DebugLogProvider( LogProvider debugLogProvider )
		 {
			  this._debugLogProvider = debugLogProvider;
			  return this;
		 }

		 public virtual CatchupClientBuilder UserLogProvider( LogProvider userLogProvider )
		 {
			  this._userLogProvider = userLogProvider;
			  return this;
		 }

		 public virtual CatchupClientBuilder Clock( Clock clock )
		 {
			  this._clock = clock;
			  return this;
		 }

		 public virtual CatchUpClient Build()
		 {
			  ApplicationProtocolRepository applicationProtocolRepository = new ApplicationProtocolRepository( Protocol_ApplicationProtocols.values(), _catchupProtocols );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( Protocol_ModifierProtocols.values(), _modifierProtocols );

			  System.Func<CatchUpResponseHandler, ChannelInitializer<SocketChannel>> channelInitializer = handler =>
			  {
				IList<ProtocolInstaller.Factory<Client, ?>> installers = singletonList( new CatchupProtocolClientInstaller.Factory( _pipelineBuilder, _debugLogProvider, handler ) );

				ProtocolInstallerRepository<Client> protocolInstallerRepository = new ProtocolInstallerRepository<Client>( installers, ModifierProtocolInstaller.allClientInstallers );

				return new HandshakeClientInitializer( applicationProtocolRepository, modifierProtocolRepository, protocolInstallerRepository, _pipelineBuilder, _handshakeTimeout, _debugLogProvider, _userLogProvider );
			  };

			  return new CatchUpClient( _debugLogProvider, _clock, _inactivityTimeoutMillis, channelInitializer );
		 }
	}

}