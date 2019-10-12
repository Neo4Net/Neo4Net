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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{

	using VoidPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.VoidPipelineWrapperFactory;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using ChildInitializer = Org.Neo4j.causalclustering.net.ChildInitializer;
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
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using Group = Org.Neo4j.Scheduler.Group;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.CATCHUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocolCategory.COMPRESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	internal class TestCatchupServer : Server
	{
		 private static readonly LogProvider _logProvider = NullLogProvider.Instance;

		 internal TestCatchupServer( FileSystemAbstraction fileSystem, GraphDatabaseAPI graphDb ) : base( ChildInitializer( fileSystem, graphDb ), _logProvider, _logProvider, new ListenSocketAddress( "localhost", PortAuthority.allocatePort() ), "fake-catchup-server" )
		 {
		 }

		 private static ChildInitializer ChildInitializer( FileSystemAbstraction fileSystem, GraphDatabaseAPI graphDb )
		 {
			  ApplicationSupportedProtocols catchupProtocols = new ApplicationSupportedProtocols( CATCHUP, emptyList() );
			  ModifierSupportedProtocols modifierProtocols = new ModifierSupportedProtocols( COMPRESSION, emptyList() );

			  ApplicationProtocolRepository catchupRepository = new ApplicationProtocolRepository( Protocol_ApplicationProtocols.values(), catchupProtocols );
			  ModifierProtocolRepository modifierRepository = new ModifierProtocolRepository( Protocol_ModifierProtocols.values(), singletonList(modifierProtocols) );

			  System.Func<CheckPointer> checkPointer = () => graphDb.DependencyResolver.resolveDependency(typeof(CheckPointer));
			  System.Func<bool> availability = () => graphDb.DependencyResolver.resolveDependency(typeof(DatabaseAvailabilityGuard)).Available;
			  System.Func<NeoStoreDataSource> dataSource = () => graphDb.DependencyResolver.resolveDependency(typeof(NeoStoreDataSource));
			  LogProvider logProvider = NullLogProvider.Instance;

			  Org.Neo4j.Storageengine.Api.StoreId kernelStoreId = dataSource().StoreId;
			  StoreId storeId = new StoreId( kernelStoreId.CreationTime, kernelStoreId.RandomId, kernelStoreId.UpgradeTime, kernelStoreId.UpgradeId );

			  CheckPointerService checkPointerService = new CheckPointerService( checkPointer, createInitialisedScheduler(), Group.CHECKPOINT );
			  RegularCatchupServerHandler catchupServerHandler = new RegularCatchupServerHandler( new Monitors(), logProvider, () => storeId, dataSource, availability, fileSystem, null, checkPointerService );

			  NettyPipelineBuilderFactory pipelineBuilder = new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER );
			  CatchupProtocolServerInstaller.Factory catchupProtocolServerInstaller = new CatchupProtocolServerInstaller.Factory( pipelineBuilder, logProvider, catchupServerHandler );

			  ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server> protocolInstallerRepository = new ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>( singletonList( catchupProtocolServerInstaller ), Org.Neo4j.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllServerInstallers );

			  return new HandshakeServerInitializer( catchupRepository, modifierRepository, protocolInstallerRepository, pipelineBuilder, logProvider );
		 }
	}

}