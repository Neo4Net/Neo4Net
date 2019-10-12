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
namespace Org.Neo4j.causalclustering.core
{
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;


	using CatchupServerBuilder = Org.Neo4j.causalclustering.catchup.CatchupServerBuilder;
	using CatchupServerHandler = Org.Neo4j.causalclustering.catchup.CatchupServerHandler;
	using Server = Org.Neo4j.causalclustering.net.Server;
	using NettyPipelineBuilderFactory = Org.Neo4j.causalclustering.protocol.NettyPipelineBuilderFactory;
	using ApplicationSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using ModifierSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class TransactionBackupServiceProvider
	{
		 private readonly LogProvider _logProvider;
		 private readonly LogProvider _userLogProvider;
		 private readonly ChannelInboundHandler _parentHandler;
		 private readonly ApplicationSupportedProtocols _catchupProtocols;
		 private readonly ICollection<ModifierSupportedProtocols> _supportedModifierProtocols;
		 private readonly NettyPipelineBuilderFactory _serverPipelineBuilderFactory;
		 private readonly CatchupServerHandler _catchupServerHandler;

		 public TransactionBackupServiceProvider( LogProvider logProvider, LogProvider userLogProvider, ApplicationSupportedProtocols catchupProtocols, ICollection<ModifierSupportedProtocols> supportedModifierProtocols, NettyPipelineBuilderFactory serverPipelineBuilderFactory, CatchupServerHandler catchupServerHandler, ChannelInboundHandler parentHandler )
		 {
			  this._logProvider = logProvider;
			  this._userLogProvider = userLogProvider;
			  this._parentHandler = parentHandler;
			  this._catchupProtocols = catchupProtocols;
			  this._supportedModifierProtocols = supportedModifierProtocols;
			  this._serverPipelineBuilderFactory = serverPipelineBuilderFactory;
			  this._catchupServerHandler = catchupServerHandler;
		 }

		 public virtual Optional<Server> ResolveIfBackupEnabled( Config config )
		 {
			  if ( config.Get( OnlineBackupSettings.online_backup_enabled ) )
			  {
					ListenSocketAddress backupAddress = HostnamePortAsListenAddress.Resolve( config, OnlineBackupSettings.online_backup_server );
					_logProvider.getLog( typeof( TransactionBackupServiceProvider ) ).info( "Binding backup service on address %s", backupAddress );
					return Optional.of(new CatchupServerBuilder(_catchupServerHandler)
							  .serverHandler( _parentHandler ).catchupProtocols( _catchupProtocols ).modifierProtocols( _supportedModifierProtocols ).pipelineBuilder( _serverPipelineBuilderFactory ).userLogProvider( _userLogProvider ).debugLogProvider( _logProvider ).listenAddress( backupAddress ).serverName( "backup-server" ).build());
			  }
			  else
			  {
					return null;
			  }
		 }
	}

}