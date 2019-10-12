using System;

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
namespace Org.Neo4j.Consistency
{

	using ConsistencyCheckIncompleteException = Org.Neo4j.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using ConsistencyFlags = Org.Neo4j.Consistency.checking.full.ConsistencyFlags;
	using FullCheck = Org.Neo4j.Consistency.checking.full.FullCheck;
	using ConsistencySummaryStatistics = Org.Neo4j.Consistency.report.ConsistencySummaryStatistics;
	using AccessStatistics = Org.Neo4j.Consistency.statistics.AccessStatistics;
	using AccessStatsKeepingStoreAccess = Org.Neo4j.Consistency.statistics.AccessStatsKeepingStoreAccess;
	using DefaultCounts = Org.Neo4j.Consistency.statistics.DefaultCounts;
	using Statistics = Org.Neo4j.Consistency.statistics.Statistics;
	using VerboseStatistics = Org.Neo4j.Consistency.statistics.VerboseStatistics;
	using Suppliers = Org.Neo4j.Function.Suppliers;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using DirectStoreAccess = Org.Neo4j.Kernel.api.direct.DirectStoreAccess;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseKernelExtensions = Org.Neo4j.Kernel.extension.DatabaseKernelExtensions;
	using FullStoreChangeStream = Org.Neo4j.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using DelegatingTokenHolder = Org.Neo4j.Kernel.impl.core.DelegatingTokenHolder;
	using ReadOnlyTokenCreator = Org.Neo4j.Kernel.impl.core.ReadOnlyTokenCreator;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using NativeLabelScanStore = Org.Neo4j.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using ConfiguringPageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using RecoveryRequiredException = Org.Neo4j.Kernel.impl.recovery.RecoveryRequiredException;
	using JobSchedulerFactory = Org.Neo4j.Kernel.impl.scheduler.JobSchedulerFactory;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DefaultIndexProviderMap = Org.Neo4j.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using DuplicatingLog = Org.Neo4j.Logging.DuplicatingLog;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.@internal.SchemaIndexExtensionLoader.instantiateKernelExtensions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.file.Files.createOrOpenAsOutputStream;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.factory.DatabaseInfo.TOOL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.recovery.RecoveryRequiredChecker.assertRecoveryIsNotRequired;

	public class ConsistencyCheckService
	{
		 private readonly DateTime _timestamp;

		 public ConsistencyCheckService() : this(DateTime.Now)
		 {
		 }

		 public ConsistencyCheckService( DateTime timestamp )
		 {
			  this._timestamp = timestamp;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config tuningConfiguration, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, org.neo4j.logging.LogProvider logProvider, boolean verbose) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 [Obsolete]
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, bool verbose )
		 {
			  return RunFullConsistencyCheck( databaseLayout, tuningConfiguration, progressFactory, logProvider, verbose, new ConsistencyFlags( tuningConfiguration ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config config, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, org.neo4j.logging.LogProvider logProvider, boolean verbose, org.neo4j.consistency.checking.full.ConsistencyFlags consistencyFlags) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config config, ProgressMonitorFactory progressFactory, LogProvider logProvider, bool verbose, ConsistencyFlags consistencyFlags )
		 {
			  FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction();
			  try
			  {
					return RunFullConsistencyCheck( databaseLayout, config, progressFactory, logProvider, fileSystem, verbose, consistencyFlags );
			  }
			  finally
			  {
					try
					{
						 fileSystem.Dispose();
					}
					catch ( IOException e )
					{
						 Log log = logProvider.getLog( this.GetType() );
						 log.Error( "Failure during shutdown of file system", e );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config tuningConfiguration, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, org.neo4j.logging.LogProvider logProvider, org.neo4j.io.fs.FileSystemAbstraction fileSystem, boolean verbose) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 [Obsolete]
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, bool verbose )
		 {
			  return RunFullConsistencyCheck( databaseLayout, tuningConfiguration, progressFactory, logProvider, fileSystem, verbose, new ConsistencyFlags( tuningConfiguration ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config config, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, org.neo4j.logging.LogProvider logProvider, org.neo4j.io.fs.FileSystemAbstraction fileSystem, boolean verbose, org.neo4j.consistency.checking.full.ConsistencyFlags consistencyFlags) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config config, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, bool verbose, ConsistencyFlags consistencyFlags )
		 {
			  return RunFullConsistencyCheck( databaseLayout, config, progressFactory, logProvider, fileSystem, verbose, DefaultReportDir( config, databaseLayout.DatabaseDirectory() ), consistencyFlags );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config tuningConfiguration, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, org.neo4j.logging.LogProvider logProvider, org.neo4j.io.fs.FileSystemAbstraction fileSystem, boolean verbose, java.io.File reportDir) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 [Obsolete]
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, bool verbose, File reportDir )
		 {
			  return RunFullConsistencyCheck( databaseLayout, tuningConfiguration, progressFactory, logProvider, fileSystem, verbose, reportDir, new ConsistencyFlags( tuningConfiguration ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config config, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, org.neo4j.logging.LogProvider logProvider, org.neo4j.io.fs.FileSystemAbstraction fileSystem, boolean verbose, java.io.File reportDir, org.neo4j.consistency.checking.full.ConsistencyFlags consistencyFlags) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config config, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, bool verbose, File reportDir, ConsistencyFlags consistencyFlags )
		 {
			  Log log = logProvider.getLog( this.GetType() );
			  JobScheduler jobScheduler = JobSchedulerFactory.createInitialisedScheduler();
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fileSystem, config, PageCacheTracer.NULL, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, logProvider.GetLog( typeof( PageCache ) ), EmptyVersionContextSupplier.EMPTY, jobScheduler );
			  PageCache pageCache = pageCacheFactory.OrCreatePageCache;

			  try
			  {
					return RunFullConsistencyCheck( databaseLayout, config, progressFactory, logProvider, fileSystem, pageCache, verbose, reportDir, consistencyFlags );
			  }
			  finally
			  {
					try
					{
						 pageCache.Close();
					}
					catch ( Exception e )
					{
						 log.Error( "Failure during shutdown of the page cache", e );
					}
					try
					{
						 jobScheduler.close();
					}
					catch ( Exception e )
					{
						 log.Error( "Failure during shutdown of the job scheduler", e );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config tuningConfiguration, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, final org.neo4j.logging.LogProvider logProvider, final org.neo4j.io.fs.FileSystemAbstraction fileSystem, final org.neo4j.io.pagecache.PageCache pageCache, final boolean verbose) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 [Obsolete]
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, bool verbose )
		 {
			  return RunFullConsistencyCheck( databaseLayout, tuningConfiguration, progressFactory, logProvider, fileSystem, pageCache, verbose, new ConsistencyFlags( tuningConfiguration ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config config, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, final org.neo4j.logging.LogProvider logProvider, final org.neo4j.io.fs.FileSystemAbstraction fileSystem, final org.neo4j.io.pagecache.PageCache pageCache, final boolean verbose, org.neo4j.consistency.checking.full.ConsistencyFlags consistencyFlags) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config config, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, bool verbose, ConsistencyFlags consistencyFlags )
		 {
			  return RunFullConsistencyCheck( databaseLayout, config, progressFactory, logProvider, fileSystem, pageCache, verbose, DefaultReportDir( config, databaseLayout.DatabaseDirectory() ), consistencyFlags );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config tuningConfiguration, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, final org.neo4j.logging.LogProvider logProvider, final org.neo4j.io.fs.FileSystemAbstraction fileSystem, final org.neo4j.io.pagecache.PageCache pageCache, final boolean verbose, java.io.File reportDir) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 [Obsolete]
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config tuningConfiguration, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, bool verbose, File reportDir )
		 {
			  return RunFullConsistencyCheck( databaseLayout, tuningConfiguration, progressFactory, logProvider, fileSystem, pageCache, verbose, reportDir, new ConsistencyFlags( tuningConfiguration ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Result runFullConsistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config config, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, final org.neo4j.logging.LogProvider logProvider, final org.neo4j.io.fs.FileSystemAbstraction fileSystem, final org.neo4j.io.pagecache.PageCache pageCache, final boolean verbose, java.io.File reportDir, org.neo4j.consistency.checking.full.ConsistencyFlags consistencyFlags) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Result RunFullConsistencyCheck( DatabaseLayout databaseLayout, Config config, ProgressMonitorFactory progressFactory, LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, bool verbose, File reportDir, ConsistencyFlags consistencyFlags )
		 {
			  AssertRecovered( databaseLayout, config, fileSystem, pageCache );
			  Log log = logProvider.getLog( this.GetType() );
			  config.augment( GraphDatabaseSettings.read_only, TRUE );
			  config.augment( GraphDatabaseSettings.pagecache_warmup_enabled, FALSE );

			  StoreFactory factory = new StoreFactory( databaseLayout, config, new DefaultIdGeneratorFactory( fileSystem ), pageCache, fileSystem, logProvider, EmptyVersionContextSupplier.EMPTY );

			  ConsistencySummaryStatistics summary;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File reportFile = chooseReportPath(reportDir);
			  File reportFile = ChooseReportPath( reportDir );
			  Suppliers.Lazy<PrintWriter> reportWriterSupplier = GetReportWriterSupplier( fileSystem, reportFile );
			  Log reportLog = new ConsistencyReportLog( reportWriterSupplier );

			  // Bootstrap kernel extensions
			  Monitors monitors = new Monitors();
			  LifeSupport life = new LifeSupport();
			  JobScheduler jobScheduler = life.Add( JobSchedulerFactory.createInitialisedScheduler() );
			  TokenHolders tokenHolders = new TokenHolders( new DelegatingTokenHolder( new ReadOnlyTokenCreator(), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY ), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE) );
			  DatabaseKernelExtensions extensions = life.Add( instantiateKernelExtensions( databaseLayout.DatabaseDirectory(), fileSystem, config, new SimpleLogService(logProvider, logProvider), pageCache, jobScheduler, RecoveryCleanupWorkCollector.ignore(), TOOL, monitors, tokenHolders ) );
			  DefaultIndexProviderMap indexes = life.Add( new DefaultIndexProviderMap( extensions, config ) );

			  try
			  {
					  using ( NeoStores neoStores = factory.OpenAllNeoStores() )
					  {
						// Load tokens before starting extensions, etc.
						tokenHolders.PropertyKeyTokens().InitialTokens = neoStores.PropertyKeyTokenStore.Tokens;
						tokenHolders.LabelTokens().InitialTokens = neoStores.LabelTokenStore.Tokens;
						tokenHolders.RelationshipTypeTokens().InitialTokens = neoStores.RelationshipTypeTokenStore.Tokens;
      
						life.Start();
      
						LabelScanStore labelScanStore = new NativeLabelScanStore( pageCache, databaseLayout, fileSystem, Org.Neo4j.Kernel.Impl.Api.scan.FullStoreChangeStream_Fields.Empty, true, monitors, RecoveryCleanupWorkCollector.ignore() );
						life.Add( labelScanStore );
      
						int numberOfThreads = DefaultConsistencyCheckThreadsNumber();
						Statistics statistics;
						StoreAccess storeAccess;
						AccessStatistics stats = new AccessStatistics();
						if ( verbose )
						{
							 statistics = new VerboseStatistics( stats, new DefaultCounts( numberOfThreads ), log );
							 storeAccess = new AccessStatsKeepingStoreAccess( neoStores, stats );
						}
						else
						{
							 statistics = Statistics.NONE;
							 storeAccess = new StoreAccess( neoStores );
						}
						storeAccess.Initialize();
						DirectStoreAccess stores = new DirectStoreAccess( storeAccess, labelScanStore, indexes, tokenHolders );
						FullCheck check = new FullCheck( progressFactory, statistics, numberOfThreads, consistencyFlags, config, true );
						summary = check.Execute( stores, new DuplicatingLog( log, reportLog ) );
					  }
			  }
			  finally
			  {
					life.Shutdown();
					if ( reportWriterSupplier.Initialised )
					{
						 reportWriterSupplier.get().close();
					}
			  }

			  if ( !summary.Consistent )
			  {
					log.Warn( "See '%s' for a detailed consistency report.", reportFile.Path );
					return Result.failure( reportFile );
			  }
			  return Result.success( reportFile );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertRecovered(org.neo4j.io.layout.DatabaseLayout databaseLayout, org.neo4j.kernel.configuration.Config config, org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.pagecache.PageCache pageCache) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 private void AssertRecovered( DatabaseLayout databaseLayout, Config config, FileSystemAbstraction fileSystem, PageCache pageCache )
		 {
			  try
			  {
					assertRecoveryIsNotRequired( fileSystem, pageCache, config, databaseLayout, new Monitors() );
			  }
			  catch ( Exception e ) when ( e is RecoveryRequiredException || e is IOException )
			  {
					throw new ConsistencyCheckIncompleteException( e );
			  }
		 }

		 private static Suppliers.Lazy<PrintWriter> GetReportWriterSupplier( FileSystemAbstraction fileSystem, File reportFile )
		 {
			  return Suppliers.lazySingleton(() =>
			  {
				try
				{
					 return new PrintWriter( createOrOpenAsOutputStream( fileSystem, reportFile, true ) );
				}
				catch ( IOException e )
				{
					 throw new Exception( e );
				}
			  });
		 }

		 private File ChooseReportPath( File reportDir )
		 {
			  return new File( reportDir, DefaultLogFileName( _timestamp ) );
		 }

		 private static File DefaultReportDir( Config tuningConfiguration, File storeDir )
		 {
			  if ( tuningConfiguration.Get( GraphDatabaseSettings.neo4j_home ) == null )
			  {
					tuningConfiguration.augment( GraphDatabaseSettings.neo4j_home, storeDir.AbsolutePath );
					tuningConfiguration.augment( GraphDatabaseSettings.database_path, storeDir.AbsolutePath );
			  }

			  return tuningConfiguration.Get( GraphDatabaseSettings.logs_directory );
		 }

		 private static string DefaultLogFileName( DateTime date )
		 {
			  return format( "inconsistencies-%s.report", ( new SimpleDateFormat( "yyyy-MM-dd.HH.mm.ss" ) ).format( date ) );
		 }

		 public interface Result
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//			  static Result failure(java.io.File reportFile)
	//		  {
	//				return new Result()
	//				{
	//					 @@Override public boolean isSuccessful()
	//					 {
	//						  return false;
	//					 }
	//
	//					 @@Override public File reportFile()
	//					 {
	//						  return reportFile;
	//					 }
	//				};
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//			  static Result success(java.io.File reportFile)
	//		  {
	//				return new Result()
	//				{
	//					 @@Override public boolean isSuccessful()
	//					 {
	//						  return true;
	//					 }
	//
	//					 @@Override public File reportFile()
	//					 {
	//						  return reportFile;
	//					 }
	//				};
	//		  }

			  bool Successful { get; }

			  File ReportFile();
		 }

		 public static int DefaultConsistencyCheckThreadsNumber()
		 {
			  return Math.Max( 1, Runtime.Runtime.availableProcessors() - 1 );
		 }
	}

}