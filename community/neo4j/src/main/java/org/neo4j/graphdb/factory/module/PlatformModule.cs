using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Graphdb.factory.module
{

	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using URLAccessRule = Org.Neo4j.Graphdb.security.URLAccessRule;
	using Org.Neo4j.Helpers.Collection;
	using GroupingRecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.GroupingRecoveryCleanupWorkCollector;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using DiagnosticsManager = Org.Neo4j.@internal.Diagnostics.DiagnosticsManager;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileSystemLifecycleAdapter = Org.Neo4j.Io.fs.FileSystemLifecycleAdapter;
	using StoreLayout = Org.Neo4j.Io.layout.StoreLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using GlobalKernelExtensions = Org.Neo4j.Kernel.extension.GlobalKernelExtensions;
	using Org.Neo4j.Kernel.extension;
	using KernelExtensionFailureStrategies = Org.Neo4j.Kernel.extension.KernelExtensionFailureStrategies;
	using LogRotationMonitor = Org.Neo4j.Kernel.Impl.Api.LogRotationMonitor;
	using TransactionVersionContextSupplier = Org.Neo4j.Kernel.impl.context.TransactionVersionContextSupplier;
	using DatabasePanicEventGenerator = Org.Neo4j.Kernel.impl.core.DatabasePanicEventGenerator;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using ConfiguringPageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using PageCacheLifecycle = Org.Neo4j.Kernel.impl.pagecache.PageCacheLifecycle;
	using QueryEngineProvider = Org.Neo4j.Kernel.impl.query.QueryEngineProvider;
	using JobSchedulerFactory = Org.Neo4j.Kernel.impl.scheduler.JobSchedulerFactory;
	using URLAccessRules = Org.Neo4j.Kernel.impl.security.URLAccessRules;
	using SimpleKernelContext = Org.Neo4j.Kernel.impl.spi.SimpleKernelContext;
	using CheckPointerMonitor = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointerMonitor;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using CachingOffHeapBlockAllocator = Org.Neo4j.Kernel.impl.util.collection.CachingOffHeapBlockAllocator;
	using CapacityLimitingBlockAllocatorDecorator = Org.Neo4j.Kernel.impl.util.collection.CapacityLimitingBlockAllocatorDecorator;
	using CollectionsFactorySupplier = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using OffHeapBlockAllocator = Org.Neo4j.Kernel.impl.util.collection.OffHeapBlockAllocator;
	using OffHeapCollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.OffHeapCollectionsFactory;
	using JvmChecker = Org.Neo4j.Kernel.info.JvmChecker;
	using JvmMetadataRepository = Org.Neo4j.Kernel.info.JvmMetadataRepository;
	using SystemDiagnostics = Org.Neo4j.Kernel.info.SystemDiagnostics;
	using KernelEventHandlers = Org.Neo4j.Kernel.@internal.KernelEventHandlers;
	using Version = Org.Neo4j.Kernel.@internal.Version;
	using GlobalStoreLocker = Org.Neo4j.Kernel.@internal.locker.GlobalStoreLocker;
	using StoreLocker = Org.Neo4j.Kernel.@internal.locker.StoreLocker;
	using StoreLockerLifecycleAdapter = Org.Neo4j.Kernel.@internal.locker.StoreLockerLifecycleAdapter;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Tracers = Org.Neo4j.Kernel.monitoring.tracing.Tracers;
	using Level = Org.Neo4j.Logging.Level;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using StoreLogService = Org.Neo4j.Logging.@internal.StoreLogService;
	using DeferredExecutor = Org.Neo4j.Scheduler.DeferredExecutor;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using Clocks = Org.Neo4j.Time.Clocks;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;
	using UsageData = Org.Neo4j.Udc.UsageData;
	using UsageDataKeys = Org.Neo4j.Udc.UsageDataKeys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.tx_state_off_heap_block_cache_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.tx_state_off_heap_max_cacheable_block_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.LifecycleAdapter.onShutdown;

	/// <summary>
	/// Platform module for <seealso cref="GraphDatabaseFacadeFactory"/>. This creates
	/// all the services needed by <seealso cref="AbstractEditionModule"/> implementations.
	/// </summary>
	public class PlatformModule
	{
		 public readonly PageCache PageCache;

		 public readonly Monitors Monitors;

		 public readonly Dependencies Dependencies;

		 public readonly LogService Logging;

		 public readonly LifeSupport Life;

		 public readonly StoreLayout StoreLayout;

		 public readonly DatabaseInfo DatabaseInfo;

		 public readonly DiagnosticsManager DiagnosticsManager;

		 public readonly KernelEventHandlers EventHandlers;
		 public readonly DatabasePanicEventGenerator PanicEventGenerator;

		 public readonly Tracers Tracers;

		 public readonly Config Config;

		 public readonly FileSystemAbstraction FileSystem;

		 public readonly DataSourceManager DataSourceManager;

		 public readonly GlobalKernelExtensions GlobalKernelExtensions;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public final Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensionFactories;
		 public readonly IEnumerable<KernelExtensionFactory<object>> KernelExtensionFactories;
		 public readonly IEnumerable<QueryEngineProvider> EngineProviders;

		 public readonly URLAccessRule UrlAccessRule;

		 public readonly JobScheduler JobScheduler;

		 public readonly SystemNanoClock Clock;

		 public readonly VersionContextSupplier VersionContextSupplier;

		 public readonly RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector;

		 public readonly CollectionsFactorySupplier CollectionsFactorySupplier;

		 public readonly UsageData UsageData;

		 public readonly ConnectorPortRegister ConnectorPortRegister;

		 public PlatformModule( File providedStoreDir, Config config, DatabaseInfo databaseInfo, GraphDatabaseFacadeFactory.Dependencies externalDependencies )
		 {
			  this.DatabaseInfo = databaseInfo;
			  this.DataSourceManager = new DataSourceManager( config );
			  Dependencies = new Dependencies();
			  Dependencies.satisfyDependency( databaseInfo );

			  Clock = Dependencies.satisfyDependency( CreateClock() );
			  Life = Dependencies.satisfyDependency( CreateLife() );

			  this.StoreLayout = StoreLayout.of( providedStoreDir );

			  config.AugmentDefaults( GraphDatabaseSettings.neo4j_home, StoreLayout.storeDirectory().Path );
			  this.Config = Dependencies.satisfyDependency( config );

			  FileSystem = Dependencies.satisfyDependency( CreateFileSystemAbstraction() );
			  Life.add( new FileSystemLifecycleAdapter( FileSystem ) );

			  // Component monitoring
			  Monitors = externalDependencies.Monitors() == null ? new Monitors() : externalDependencies.Monitors();
			  Dependencies.satisfyDependency( Monitors );

			  JobScheduler = Life.add( Dependencies.satisfyDependency( CreateJobScheduler() ) );
			  StartDeferredExecutors( JobScheduler, externalDependencies.DeferredExecutors() );

			  // Cleanup after recovery, used by GBPTree, added to life in NeoStoreDataSource
			  RecoveryCleanupWorkCollector = new GroupingRecoveryCleanupWorkCollector( JobScheduler );
			  Dependencies.satisfyDependency( RecoveryCleanupWorkCollector );

			  // Database system information, used by UDC
			  UsageData = new UsageData( JobScheduler );
			  Dependencies.satisfyDependency( Life.add( UsageData ) );

			  // If no logging was passed in from the outside then create logging and register
			  // with this life
			  Logging = Dependencies.satisfyDependency( CreateLogService( externalDependencies.UserLogProvider() ) );

			  config.Logger = Logging.getInternalLog( typeof( Config ) );

			  Life.add( Dependencies.satisfyDependency( new StoreLockerLifecycleAdapter( CreateStoreLocker() ) ) );

			  ( new JvmChecker( Logging.getInternalLog( typeof( JvmChecker ) ), new JvmMetadataRepository() ) ).checkJvmCompatibilityAndIssueWarning();

			  string desiredImplementationName = config.Get( GraphDatabaseSettings.tracer );
			  Tracers = Dependencies.satisfyDependency( new Tracers( desiredImplementationName, Logging.getInternalLog( typeof( Tracers ) ), Monitors, JobScheduler, Clock ) );
			  Dependencies.satisfyDependency( Tracers.pageCacheTracer );
			  Dependencies.satisfyDependency( FirstImplementor( typeof( LogRotationMonitor ), Tracers.transactionTracer, LogRotationMonitor.NULL ) );
			  Dependencies.satisfyDependency( FirstImplementor( typeof( CheckPointerMonitor ), Tracers.checkPointTracer, CheckPointerMonitor.NULL ) );

			  VersionContextSupplier = CreateCursorContextSupplier( config );

			  CollectionsFactorySupplier = CreateCollectionsFactorySupplier( config, Life );

			  Dependencies.satisfyDependency( VersionContextSupplier );
			  PageCache = Dependencies.satisfyDependency( CreatePageCache( FileSystem, config, Logging, Tracers, VersionContextSupplier, JobScheduler ) );

			  Life.add( new PageCacheLifecycle( PageCache ) );

			  DiagnosticsManager = Life.add( Dependencies.satisfyDependency( new DiagnosticsManager( Logging.getInternalLog( typeof( DiagnosticsManager ) ) ) ) );
			  SystemDiagnostics.registerWith( DiagnosticsManager );

			  Dependencies.satisfyDependency( DataSourceManager );

			  KernelExtensionFactories = externalDependencies.KernelExtensions();
			  EngineProviders = externalDependencies.ExecutionEngines();
			  GlobalKernelExtensions = Dependencies.satisfyDependency( new GlobalKernelExtensions( new SimpleKernelContext( StoreLayout.storeDirectory(), databaseInfo, Dependencies ), KernelExtensionFactories, Dependencies, KernelExtensionFailureStrategies.fail() ) );

			  UrlAccessRule = Dependencies.satisfyDependency( URLAccessRules.combined( externalDependencies.UrlAccessRules() ) );

			  ConnectorPortRegister = new ConnectorPortRegister();
			  Dependencies.satisfyDependency( ConnectorPortRegister );

			  EventHandlers = new KernelEventHandlers( Logging.getInternalLog( typeof( KernelEventHandlers ) ) );
			  PanicEventGenerator = new DatabasePanicEventGenerator( EventHandlers );

			  PublishPlatformInfo( Dependencies.resolveDependency( typeof( UsageData ) ) );
		 }

		 private void StartDeferredExecutors( JobScheduler jobScheduler, IEnumerable<Pair<DeferredExecutor, Group>> deferredExecutors )
		 {
			  foreach ( Pair<DeferredExecutor, Group> executorGroupPair in deferredExecutors )
			  {
					DeferredExecutor executor = executorGroupPair.First();
					Group group = executorGroupPair.Other();
					executor.SatisfyWith( jobScheduler.Executor( group ) );
			  }
		 }

		 protected internal virtual VersionContextSupplier CreateCursorContextSupplier( Config config )
		 {
			  return config.Get( GraphDatabaseSettings.snapshot_query ) ? new TransactionVersionContextSupplier() : EmptyVersionContextSupplier.EMPTY;
		 }

		 protected internal virtual StoreLocker CreateStoreLocker()
		 {
			  return new GlobalStoreLocker( FileSystem, StoreLayout );
		 }

		 protected internal virtual SystemNanoClock CreateClock()
		 {
			  return Clocks.nanoClock();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <T> T firstImplementor(Class<T> type, Object... candidates)
		 private static T FirstImplementor<T>( Type type, params object[] candidates )
		 {
				 type = typeof( T );
			  foreach ( object candidate in candidates )
			  {
					if ( type.IsInstanceOfType( candidate ) )
					{
						 return ( T ) candidate;
					}
			  }
			  return null;
		 }

		 private static void PublishPlatformInfo( UsageData sysInfo )
		 {
			  sysInfo.Set( UsageDataKeys.version, Version.Neo4jVersion );
			  sysInfo.Set( UsageDataKeys.revision, Version.KernelVersion );
		 }

		 public virtual LifeSupport CreateLife()
		 {
			  return new LifeSupport();
		 }

		 protected internal virtual FileSystemAbstraction CreateFileSystemAbstraction()
		 {
			  return new DefaultFileSystemAbstraction();
		 }

		 protected internal virtual LogService CreateLogService( LogProvider userLogProvider )
		 {
			  long internalLogRotationThreshold = Config.get( GraphDatabaseSettings.store_internal_log_rotation_threshold );
			  long internalLogRotationDelay = Config.get( GraphDatabaseSettings.store_internal_log_rotation_delay ).toMillis();
			  int internalLogMaxArchives = Config.get( GraphDatabaseSettings.store_internal_log_max_archives );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.internal.StoreLogService.Builder builder = org.neo4j.logging.internal.StoreLogService.withRotation(internalLogRotationThreshold, internalLogRotationDelay, internalLogMaxArchives, jobScheduler);
			  StoreLogService.Builder builder = StoreLogService.withRotation( internalLogRotationThreshold, internalLogRotationDelay, internalLogMaxArchives, JobScheduler );

			  if ( userLogProvider != null )
			  {
					builder.WithUserLogProvider( userLogProvider );
			  }

			  builder.WithRotationListener( logProvider => DiagnosticsManager.dumpAll( logProvider.getLog( typeof( DiagnosticsManager ) ) ) );

			  foreach ( string debugContext in Config.get( GraphDatabaseSettings.store_internal_debug_contexts ) )
			  {
					builder.WithLevel( debugContext, Level.DEBUG );
			  }
			  builder.WithDefaultLevel( Config.get( GraphDatabaseSettings.store_internal_log_level ) ).withTimeZone( Config.get( GraphDatabaseSettings.db_timezone ).ZoneId );

			  File logFile = Config.get( store_internal_log_path );
			  if ( !logFile.ParentFile.exists() )
			  {
					logFile.ParentFile.mkdirs();
			  }
			  StoreLogService logService;
			  try
			  {
					logService = builder.WithInternalLog( logFile ).build( FileSystem );
			  }
			  catch ( IOException ex )
			  {
					throw new Exception( ex );
			  }
			  return Life.add( logService );
		 }

		 protected internal virtual JobScheduler CreateJobScheduler()
		 {
			  return JobSchedulerFactory.createInitialisedScheduler();
		 }

		 protected internal virtual PageCache CreatePageCache( FileSystemAbstraction fileSystem, Config config, LogService logging, Tracers tracers, VersionContextSupplier versionContextSupplier, JobScheduler jobScheduler )
		 {
			  Log pageCacheLog = logging.GetInternalLog( typeof( PageCache ) );
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fileSystem, config, tracers.PageCacheTracer, tracers.PageCursorTracerSupplier, pageCacheLog, versionContextSupplier, jobScheduler );
			  PageCache pageCache = pageCacheFactory.OrCreatePageCache;

			  if ( config.Get( GraphDatabaseSettings.dump_configuration ) )
			  {
					pageCacheFactory.DumpConfiguration();
			  }
			  return pageCache;
		 }

		 private static CollectionsFactorySupplier CreateCollectionsFactorySupplier( Config config, LifeSupport life )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.factory.GraphDatabaseSettings.TransactionStateMemoryAllocation allocation = config.get(org.neo4j.graphdb.factory.GraphDatabaseSettings.tx_state_memory_allocation);
			  GraphDatabaseSettings.TransactionStateMemoryAllocation allocation = config.Get( GraphDatabaseSettings.tx_state_memory_allocation );
			  switch ( allocation )
			  {
			  case GraphDatabaseSettings.TransactionStateMemoryAllocation.ON_HEAP:
					return Org.Neo4j.Kernel.impl.util.collection.CollectionsFactorySupplier_Fields.OnHeap;
			  case GraphDatabaseSettings.TransactionStateMemoryAllocation.OFF_HEAP:
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.CachingOffHeapBlockAllocator allocator = new org.neo4j.kernel.impl.util.collection.CachingOffHeapBlockAllocator(config.get(tx_state_off_heap_max_cacheable_block_size), config.get(tx_state_off_heap_block_cache_size));
					CachingOffHeapBlockAllocator allocator = new CachingOffHeapBlockAllocator( config.Get( tx_state_off_heap_max_cacheable_block_size ), config.Get( tx_state_off_heap_block_cache_size ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator sharedBlockAllocator;
					OffHeapBlockAllocator sharedBlockAllocator;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long maxMemory = config.get(org.neo4j.graphdb.factory.GraphDatabaseSettings.tx_state_max_off_heap_memory);
					long maxMemory = config.Get( GraphDatabaseSettings.tx_state_max_off_heap_memory );
					if ( maxMemory > 0 )
					{
						 sharedBlockAllocator = new CapacityLimitingBlockAllocatorDecorator( allocator, maxMemory );
					}
					else
					{
						 sharedBlockAllocator = allocator;
					}
					life.Add( onShutdown( sharedBlockAllocator.release ) );
					return () => new OffHeapCollectionsFactory(sharedBlockAllocator);
			  default:
					throw new System.ArgumentException( "Unknown transaction state memory allocation value: " + allocation );
			  }
		 }
	}

}