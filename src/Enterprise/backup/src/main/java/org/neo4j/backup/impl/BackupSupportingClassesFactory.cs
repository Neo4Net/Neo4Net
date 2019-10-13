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
namespace Neo4Net.backup.impl
{

	using CatchUpClient = Neo4Net.causalclustering.catchup.CatchUpClient;
	using CatchupClientBuilder = Neo4Net.causalclustering.catchup.CatchupClientBuilder;
	using RemoteStore = Neo4Net.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyClient = Neo4Net.causalclustering.catchup.storecopy.StoreCopyClient;
	using TransactionLogCatchUpFactory = Neo4Net.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using TxPullClient = Neo4Net.causalclustering.catchup.tx.TxPullClient;
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using SupportedProtocolCreator = Neo4Net.causalclustering.core.SupportedProtocolCreator;
	using PipelineWrapper = Neo4Net.causalclustering.handlers.PipelineWrapper;
	using VoidPipelineWrapperFactory = Neo4Net.causalclustering.handlers.VoidPipelineWrapperFactory;
	using ExponentialBackoffStrategy = Neo4Net.causalclustering.helper.ExponentialBackoffStrategy;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ConfigurableStandalonePageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupProtocolServiceFactory.backupProtocolService;

	/// <summary>
	/// The dependencies for the backup strategies require a valid configuration for initialisation.
	/// By having this factory we can wait until the configuration has been loaded and the provide all the classes required
	/// for backups that are dependant on the config.
	/// </summary>
	public class BackupSupportingClassesFactory
	{
		 protected internal readonly LogProvider LogProvider;
		 protected internal readonly Clock Clock;
		 protected internal readonly Monitors Monitors;
		 protected internal readonly FileSystemAbstraction FileSystemAbstraction;
		 protected internal readonly TransactionLogCatchUpFactory TransactionLogCatchUpFactory;
		 protected internal readonly Stream LogDestination;
		 protected internal readonly OutsideWorld OutsideWorld;
		 private readonly JobScheduler _jobScheduler;

		 protected internal BackupSupportingClassesFactory( BackupModule backupModule )
		 {
			  this.LogProvider = backupModule.LogProvider;
			  this.Clock = backupModule.Clock;
			  this.Monitors = backupModule.Monitors;
			  this.FileSystemAbstraction = backupModule.FileSystemAbstraction;
			  this.TransactionLogCatchUpFactory = backupModule.TransactionLogCatchUpFactory;
			  this._jobScheduler = backupModule.JobScheduler();
			  this.LogDestination = backupModule.OutsideWorld.outStream();
			  this.OutsideWorld = backupModule.OutsideWorld;
		 }

		 /// <summary>
		 /// Resolves all the backing solutions used for performing various backups while sharing key classes and
		 /// configuration.
		 /// </summary>
		 /// <param name="config"> user selected during runtime for performing backups </param>
		 /// <returns> grouping of instances used for performing backups </returns>
		 internal virtual BackupSupportingClasses CreateSupportingClasses( Config config )
		 {
			  Monitors.addMonitorListener( new BackupOutputMonitor( OutsideWorld ) );
			  PageCache pageCache = CreatePageCache( FileSystemAbstraction, config, _jobScheduler );
			  return new BackupSupportingClasses( BackupDelegatorFromConfig( pageCache, config ), HaFromConfig( pageCache ), pageCache, Arrays.asList( pageCache, _jobScheduler ) );
		 }

		 private BackupProtocolService HaFromConfig( PageCache pageCache )
		 {
			  System.Func<FileSystemAbstraction> fileSystemSupplier = () => FileSystemAbstraction;
			  return backupProtocolService( fileSystemSupplier, LogProvider, LogDestination, Monitors, pageCache );
		 }

		 private BackupDelegator BackupDelegatorFromConfig( PageCache pageCache, Config config )
		 {
			  CatchUpClient catchUpClient = catchUpClient( config );
			  TxPullClient txPullClient = new TxPullClient( catchUpClient, Monitors );
			  ExponentialBackoffStrategy backOffStrategy = new ExponentialBackoffStrategy( 1, config.Get( CausalClusteringSettings.store_copy_backoff_max_wait ).toMillis(), TimeUnit.MILLISECONDS );
			  StoreCopyClient storeCopyClient = new StoreCopyClient( catchUpClient, Monitors, LogProvider, backOffStrategy );

			  RemoteStore remoteStore = new RemoteStore( LogProvider, FileSystemAbstraction, pageCache, storeCopyClient, txPullClient, TransactionLogCatchUpFactory, config, Monitors );

			  return BackupDelegator( remoteStore, catchUpClient, storeCopyClient );
		 }

		 protected internal virtual PipelineWrapper CreatePipelineWrapper( Config config )
		 {
			  return ( new VoidPipelineWrapperFactory() ).forClient(config, null, LogProvider, OnlineBackupSettings.ssl_policy);
		 }

		 private CatchUpClient CatchUpClient( Config config )
		 {
			  SupportedProtocolCreator supportedProtocolCreator = new SupportedProtocolCreator( config, LogProvider );
			  ApplicationSupportedProtocols supportedCatchupProtocols = supportedProtocolCreator.CreateSupportedCatchupProtocol();
			  ICollection<ModifierSupportedProtocols> supportedModifierProtocols = supportedProtocolCreator.CreateSupportedModifierProtocols();
			  NettyPipelineBuilderFactory clientPipelineBuilderFactory = new NettyPipelineBuilderFactory( CreatePipelineWrapper( config ) );
			  Duration handshakeTimeout = config.Get( CausalClusteringSettings.handshake_timeout );
			  long inactivityTimeoutMillis = config.Get( CausalClusteringSettings.catch_up_client_inactivity_timeout ).toMillis();
			  return ( new CatchupClientBuilder( supportedCatchupProtocols, supportedModifierProtocols, clientPipelineBuilderFactory, handshakeTimeout, LogProvider, LogProvider, Clock ) ).inactivityTimeoutMillis( inactivityTimeoutMillis ).build();
		 }

		 private static BackupDelegator BackupDelegator( RemoteStore remoteStore, CatchUpClient catchUpClient, StoreCopyClient storeCopyClient )
		 {
			  return new BackupDelegator( remoteStore, catchUpClient, storeCopyClient );
		 }

		 private static PageCache CreatePageCache( FileSystemAbstraction fileSystemAbstraction, Config config, JobScheduler jobScheduler )
		 {
			  return ConfigurableStandalonePageCacheFactory.createPageCache( fileSystemAbstraction, config, jobScheduler );
		 }
	}

}