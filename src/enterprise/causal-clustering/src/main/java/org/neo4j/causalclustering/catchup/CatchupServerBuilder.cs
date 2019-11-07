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
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;

	using Server = Neo4Net.causalclustering.net.Server;
	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Protocol_ApplicationProtocols = Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols;
	using Protocol_ModifierProtocols = Neo4Net.causalclustering.protocol.Protocol_ModifierProtocols;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ApplicationProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ApplicationProtocolRepository;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using HandshakeServerInitializer = Neo4Net.causalclustering.protocol.handshake.HandshakeServerInitializer;
	using ModifierProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ModifierProtocolRepository;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.handlers.VoidPipelineWrapperFactory.VOID_WRAPPER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.CATCHUP;

	public class CatchupServerBuilder
	{
		 private readonly CatchupServerHandler _catchupServerHandler;
		 private LogProvider _debugLogProvider = NullLogProvider.Instance;
		 private LogProvider _userLogProvider = NullLogProvider.Instance;
		 private NettyPipelineBuilderFactory _pipelineBuilder = new NettyPipelineBuilderFactory( VOID_WRAPPER );
		 private ApplicationSupportedProtocols _catchupProtocols = new ApplicationSupportedProtocols( CATCHUP, emptyList() );
		 private ICollection<ModifierSupportedProtocols> _modifierProtocols = emptyList();
		 private ChannelInboundHandler _parentHandler;
		 private ListenSocketAddress _listenAddress;
		 private string _serverName = "catchup-server";

		 public CatchupServerBuilder( CatchupServerHandler catchupServerHandler )
		 {
			  this._catchupServerHandler = catchupServerHandler;
		 }

		 public virtual CatchupServerBuilder CatchupProtocols( ApplicationSupportedProtocols catchupProtocols )
		 {
			  this._catchupProtocols = catchupProtocols;
			  return this;
		 }

		 public virtual CatchupServerBuilder ModifierProtocols( ICollection<ModifierSupportedProtocols> modifierProtocols )
		 {
			  this._modifierProtocols = modifierProtocols;
			  return this;
		 }

		 public virtual CatchupServerBuilder PipelineBuilder( NettyPipelineBuilderFactory pipelineBuilder )
		 {
			  this._pipelineBuilder = pipelineBuilder;
			  return this;
		 }

		 public virtual CatchupServerBuilder ServerHandler( ChannelInboundHandler parentHandler )
		 {
			  this._parentHandler = parentHandler;
			  return this;
		 }

		 public virtual CatchupServerBuilder ListenAddress( ListenSocketAddress listenAddress )
		 {
			  this._listenAddress = listenAddress;
			  return this;
		 }

		 public virtual CatchupServerBuilder UserLogProvider( LogProvider userLogProvider )
		 {
			  this._userLogProvider = userLogProvider;
			  return this;
		 }

		 public virtual CatchupServerBuilder DebugLogProvider( LogProvider debugLogProvider )
		 {
			  this._debugLogProvider = debugLogProvider;
			  return this;
		 }

		 public virtual CatchupServerBuilder ServerName( string serverName )
		 {
			  this._serverName = serverName;
			  return this;
		 }

		 public virtual Server Build()
		 {
			  ApplicationProtocolRepository applicationProtocolRepository = new ApplicationProtocolRepository( Protocol_ApplicationProtocols.values(), _catchupProtocols );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( Protocol_ModifierProtocols.values(), _modifierProtocols );

			  CatchupProtocolServerInstaller.Factory catchupProtocolServerInstaller = new CatchupProtocolServerInstaller.Factory( _pipelineBuilder, _debugLogProvider, _catchupServerHandler );

			  ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server> protocolInstallerRepository = new ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>( singletonList( catchupProtocolServerInstaller ), Neo4Net.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllServerInstallers );

			  HandshakeServerInitializer handshakeServerInitializer = new HandshakeServerInitializer( applicationProtocolRepository, modifierProtocolRepository, protocolInstallerRepository, _pipelineBuilder, _debugLogProvider );

			  return new Server( handshakeServerInitializer, _parentHandler, _debugLogProvider, _userLogProvider, _listenAddress, _serverName );
		 }
	}

}