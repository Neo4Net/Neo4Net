using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Consistency
{

	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using FullCheck = Neo4Net.Consistency.checking.full.FullCheck;
	using ConsistencySummaryStatistics = Neo4Net.Consistency.report.ConsistencySummaryStatistics;
	using AccessStatistics = Neo4Net.Consistency.statistics.AccessStatistics;
	using AccessStatsKeepingStoreAccess = Neo4Net.Consistency.statistics.AccessStatsKeepingStoreAccess;
	using DefaultCounts = Neo4Net.Consistency.statistics.DefaultCounts;
	using Statistics = Neo4Net.Consistency.statistics.Statistics;
	using VerboseStatistics = Neo4Net.Consistency.statistics.VerboseStatistics;
	using Suppliers = Neo4Net.Functions.Suppliers;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using DirectStoreAccess = Neo4Net.Kernel.api.direct.DirectStoreAccess;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseKernelExtensions = Neo4Net.Kernel.extension.DatabaseKernelExtensions;
	using FullStoreChangeStream = Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using DelegatingTokenHolder = Neo4Net.Kernel.impl.core.DelegatingTokenHolder;
	using ReadOnlyTokenCreator = Neo4Net.Kernel.impl.core.ReadOnlyTokenCreator;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using NativeLabelScanStore = Neo4Net.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using RecoveryRequiredException = Neo4Net.Kernel.impl.recovery.RecoveryRequiredException;
	using IJobSchedulerFactory = Neo4Net.Kernel.impl.scheduler.JobSchedulerFactory;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DefaultIndexProviderMap = Neo4Net.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using DuplicatingLog = Neo4Net.Logging.DuplicatingLog;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.Internal.SchemaIndexExtensionLoader.instantiateKernelExtensions;
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
			  IJobScheduler jobScheduler = IJobSchedulerFactory.createInitializedScheduler();
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fileSystem, config, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, logProvider.GetLog( typeof( PageCache ) ), EmptyVersionContextSupplier.EMPTY, jobScheduler );
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
			  IJobScheduler jobScheduler = life.Add( IJobSchedulerFactory.createInitializedScheduler() );
			  TokenHolders tokenHolders = new TokenHolders( new DelegatingTokenHolder( new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY ), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE) );
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
      
						LabelScanStore labelScanStore = new NativeLabelScanStore( pageCache, databaseLayout, fileSystem, Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream_Fields.Empty, true, monitors, RecoveryCleanupWorkCollector.ignore() );
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
					if ( reportWriterSupplier.Initialized )
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