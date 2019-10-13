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
namespace Neo4Net.causalclustering.catchup.storecopy
{

	using VoidPipelineWrapperFactory = Neo4Net.causalclustering.handlers.VoidPipelineWrapperFactory;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using ChildInitializer = Neo4Net.causalclustering.net.ChildInitializer;
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
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using Group = Neo4Net.Scheduler.Group;

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

			  Neo4Net.Storageengine.Api.StoreId kernelStoreId = dataSource().StoreId;
			  StoreId storeId = new StoreId( kernelStoreId.CreationTime, kernelStoreId.RandomId, kernelStoreId.UpgradeTime, kernelStoreId.UpgradeId );

			  CheckPointerService checkPointerService = new CheckPointerService( checkPointer, createInitialisedScheduler(), Group.CHECKPOINT );
			  RegularCatchupServerHandler catchupServerHandler = new RegularCatchupServerHandler( new Monitors(), logProvider, () => storeId, dataSource, availability, fileSystem, null, checkPointerService );

			  NettyPipelineBuilderFactory pipelineBuilder = new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER );
			  CatchupProtocolServerInstaller.Factory catchupProtocolServerInstaller = new CatchupProtocolServerInstaller.Factory( pipelineBuilder, logProvider, catchupServerHandler );

			  ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server> protocolInstallerRepository = new ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>( singletonList( catchupProtocolServerInstaller ), Neo4Net.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllServerInstallers );

			  return new HandshakeServerInitializer( catchupRepository, modifierRepository, protocolInstallerRepository, pipelineBuilder, logProvider );
		 }
	}

}