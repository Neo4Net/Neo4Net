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
namespace Neo4Net.Graphdb.factory.module
{

	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using AbstractEditionModule = Neo4Net.Graphdb.factory.module.edition.AbstractEditionModule;
	using URLAccessRule = Neo4Net.Graphdb.security.URLAccessRule;
	using Neo4Net.Helpers.Collection;
	using GroupingRecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.GroupingRecoveryCleanupWorkCollector;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using DiagnosticsManager = Neo4Net.@internal.Diagnostics.DiagnosticsManager;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileSystemLifecycleAdapter = Neo4Net.Io.fs.FileSystemLifecycleAdapter;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using GlobalKernelExtensions = Neo4Net.Kernel.extension.GlobalKernelExtensions;
	using Neo4Net.Kernel.extension;
	using KernelExtensionFailureStrategies = Neo4Net.Kernel.extension.KernelExtensionFailureStrategies;
	using LogRotationMonitor = Neo4Net.Kernel.Impl.Api.LogRotationMonitor;
	using TransactionVersionContextSupplier = Neo4Net.Kernel.impl.context.TransactionVersionContextSupplier;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using PageCacheLifecycle = Neo4Net.Kernel.impl.pagecache.PageCacheLifecycle;
	using QueryEngineProvider = Neo4Net.Kernel.impl.query.QueryEngineProvider;
	using JobSchedulerFactory = Neo4Net.Kernel.impl.scheduler.JobSchedulerFactory;
	using URLAccessRules = Neo4Net.Kernel.impl.security.URLAccessRules;
	using SimpleKernelContext = Neo4Net.Kernel.impl.spi.SimpleKernelContext;
	using CheckPointerMonitor = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointerMonitor;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using CachingOffHeapBlockAllocator = Neo4Net.Kernel.impl.util.collection.CachingOffHeapBlockAllocator;
	using CapacityLimitingBlockAllocatorDecorator = Neo4Net.Kernel.impl.util.collection.CapacityLimitingBlockAllocatorDecorator;
	using CollectionsFactorySupplier = Neo4Net.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using OffHeapBlockAllocator = Neo4Net.Kernel.impl.util.collection.OffHeapBlockAllocator;
	using OffHeapCollectionsFactory = Neo4Net.Kernel.impl.util.collection.OffHeapCollectionsFactory;
	using JvmChecker = Neo4Net.Kernel.info.JvmChecker;
	using JvmMetadataRepository = Neo4Net.Kernel.info.JvmMetadataRepository;
	using SystemDiagnostics = Neo4Net.Kernel.info.SystemDiagnostics;
	using KernelEventHandlers = Neo4Net.Kernel.@internal.KernelEventHandlers;
	using Version = Neo4Net.Kernel.@internal.Version;
	using GlobalStoreLocker = Neo4Net.Kernel.@internal.locker.GlobalStoreLocker;
	using StoreLocker = Neo4Net.Kernel.@internal.locker.StoreLocker;
	using StoreLockerLifecycleAdapter = Neo4Net.Kernel.@internal.locker.StoreLockerLifecycleAdapter;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Tracers = Neo4Net.Kernel.monitoring.tracing.Tracers;
	using Level = Neo4Net.Logging.Level;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using StoreLogService = Neo4Net.Logging.@internal.StoreLogService;
	using DeferredExecutor = Neo4Net.Scheduler.DeferredExecutor;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using Clocks = Neo4Net.Time.Clocks;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;
	using UsageData = Neo4Net.Udc.UsageData;
	using UsageDataKeys = Neo4Net.Udc.UsageDataKeys;

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
					return Neo4Net.Kernel.impl.util.collection.CollectionsFactorySupplier_Fields.OnHeap;
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