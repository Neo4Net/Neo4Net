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
namespace Org.Neo4j.causalclustering.catchup
{
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;

	using Server = Org.Neo4j.causalclustering.net.Server;
	using Org.Neo4j.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Org.Neo4j.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Protocol_ApplicationProtocols = Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols;
	using Protocol_ModifierProtocols = Org.Neo4j.causalclustering.protocol.Protocol_ModifierProtocols;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using ApplicationProtocolRepository = Org.Neo4j.causalclustering.protocol.handshake.ApplicationProtocolRepository;
	using ApplicationSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using HandshakeServerInitializer = Org.Neo4j.causalclustering.protocol.handshake.HandshakeServerInitializer;
	using ModifierProtocolRepository = Org.Neo4j.causalclustering.protocol.handshake.ModifierProtocolRepository;
	using ModifierSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.handlers.VoidPipelineWrapperFactory.VOID_WRAPPER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.CATCHUP;

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

			  ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server> protocolInstallerRepository = new ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>( singletonList( catchupProtocolServerInstaller ), Org.Neo4j.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllServerInstallers );

			  HandshakeServerInitializer handshakeServerInitializer = new HandshakeServerInitializer( applicationProtocolRepository, modifierProtocolRepository, protocolInstallerRepository, _pipelineBuilder, _debugLogProvider );

			  return new Server( handshakeServerInitializer, _parentHandler, _debugLogProvider, _userLogProvider, _listenAddress, _serverName );
		 }
	}

}