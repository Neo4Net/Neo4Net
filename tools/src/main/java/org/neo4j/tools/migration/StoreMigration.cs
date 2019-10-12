using System;

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
namespace Org.Neo4j.tools.migration
{

	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Args = Org.Neo4j.Helpers.Args;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using DatabaseKernelExtensions = Org.Neo4j.Kernel.extension.DatabaseKernelExtensions;
	using DefaultExplicitIndexProvider = Org.Neo4j.Kernel.Impl.Api.DefaultExplicitIndexProvider;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using JobSchedulerFactory = Org.Neo4j.Kernel.impl.scheduler.JobSchedulerFactory;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using SimpleKernelContext = Org.Neo4j.Kernel.impl.spi.SimpleKernelContext;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using DatabaseMigrator = Org.Neo4j.Kernel.impl.storemigration.DatabaseMigrator;
	using StoreUpgrader = Org.Neo4j.Kernel.impl.storemigration.StoreUpgrader;
	using VisibleMigrationProgressMonitor = Org.Neo4j.Kernel.impl.storemigration.monitoring.VisibleMigrationProgressMonitor;
	using StoreMigrator = Org.Neo4j.Kernel.impl.storemigration.participant.StoreMigrator;
	using FlushablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.FlushablePositionAwareChannel;
	using TransactionLogWriter = Org.Neo4j.Kernel.impl.transaction.log.TransactionLogWriter;
	using LogEntryWriter = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using DefaultIndexProviderMap = Org.Neo4j.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogTailScanner = Org.Neo4j.Kernel.recovery.LogTailScanner;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreLogService = Org.Neo4j.Logging.@internal.StoreLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using KernelExtensionFailureStrategies = Org.Neo4j.Kernel.extension.KernelExtensionFailureStrategies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.extension.KernelExtensionFailureStrategies.ignore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory.createPageCache;

	/// <summary>
	/// Stand alone tool for migrating/upgrading a neo4j database from one version to the next.
	/// </summary>
	/// <seealso cref= StoreMigrator </seealso>
	//: TODO introduce abstract tool class as soon as we will have several tools in tools module
	public class StoreMigration
	{
		 private const string HELP_FLAG = "help";

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
		 public static void Main( string[] args )
		 {
			  Args arguments = Args.withFlags( HELP_FLAG ).parse( args );
			  if ( arguments.GetBoolean( HELP_FLAG, false ) || args.Length == 0 )
			  {
					PrintUsageAndExit();
			  }
			  File storeDir = ParseDir( arguments );

			  FormattedLogProvider userLogProvider = FormattedLogProvider.toOutputStream( System.out );
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					( new StoreMigration() ).run(fileSystem, storeDir, MigrationConfig, userLogProvider);
			  }
		 }

		 private static Config MigrationConfig
		 {
			 get
			 {
				  return Config.defaults( GraphDatabaseSettings.allow_upgrade, Settings.TRUE );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void run(final org.neo4j.io.fs.FileSystemAbstraction fs, final java.io.File storeDirectory, org.neo4j.kernel.configuration.Config config, org.neo4j.logging.LogProvider userLogProvider) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static void Run( FileSystemAbstraction fs, File storeDirectory, Config config, LogProvider userLogProvider )
		 {
			  StoreLogService logService = StoreLogService.withUserLogProvider( userLogProvider ).withInternalLog( config.Get( store_internal_log_path ) ).build( fs );

			  VisibleMigrationProgressMonitor progressMonitor = new VisibleMigrationProgressMonitor( logService.GetUserLog( typeof( StoreMigration ) ) );

			  LifeSupport life = new LifeSupport();

			  life.Add( logService );

			  // Add participants from kernel extensions...
			  DefaultExplicitIndexProvider migrationIndexProvider = new DefaultExplicitIndexProvider();

			  Log log = userLogProvider.GetLog( typeof( StoreMigration ) );
			  JobScheduler jobScheduler = JobSchedulerFactory.createInitialisedScheduler();
			  try
			  {
					  using ( PageCache pageCache = createPageCache( fs, config, jobScheduler ) )
					  {
						Dependencies deps = new Dependencies();
						Monitors monitors = new Monitors();
						deps.SatisfyDependencies( fs, config, migrationIndexProvider, pageCache, logService, monitors, RecoveryCleanupWorkCollector.immediate() );
      
						KernelContext kernelContext = new SimpleKernelContext( storeDirectory, DatabaseInfo.UNKNOWN, deps );
						DatabaseKernelExtensions kernelExtensions = life.Add( new DatabaseKernelExtensions( kernelContext, GraphDatabaseDependencies.newDependencies().kernelExtensions(), deps, ignore() ) );
      
						DatabaseLayout databaseLayout = DatabaseLayout.of( storeDirectory );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles = org.neo4j.kernel.impl.transaction.log.files.LogFilesBuilder.activeFilesBuilder(databaseLayout, fs, pageCache).withConfig(config).build();
						LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( databaseLayout, fs, pageCache ).withConfig( config ).build();
						LogTailScanner tailScanner = new LogTailScanner( logFiles, new VersionAwareLogEntryReader<Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>(), monitors );
      
						DefaultIndexProviderMap indexProviderMap = life.Add( new DefaultIndexProviderMap( kernelExtensions, config ) );
      
						// Add the kernel store migrator
						life.Start();
      
						long startTime = DateTimeHelper.CurrentUnixTimeMillis();
						DatabaseMigrator migrator = new DatabaseMigrator( progressMonitor, fs, config, logService, indexProviderMap, migrationIndexProvider, pageCache, RecordFormatSelector.selectForConfig( config, userLogProvider ), tailScanner, jobScheduler );
						migrator.Migrate( databaseLayout );
      
						// Append checkpoint so the last log entry will have the latest version
						AppendCheckpoint( logFiles, tailScanner );
      
						long duration = DateTimeHelper.CurrentUnixTimeMillis() - startTime;
						log.Info( format( "Migration completed in %d s%n", duration / 1000 ) );
					  }
			  }
			  catch ( Exception e )
			  {
					throw new StoreUpgrader.UnableToUpgradeException( "Failure during upgrade", e );
			  }
			  finally
			  {
					life.Shutdown();
					jobScheduler.close();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void appendCheckpoint(org.neo4j.kernel.impl.transaction.log.files.LogFiles logFiles, org.neo4j.kernel.recovery.LogTailScanner tailScanner) throws java.io.IOException
		 private static void AppendCheckpoint( LogFiles logFiles, LogTailScanner tailScanner )
		 {
			  using ( Lifespan lifespan = new Lifespan( logFiles ) )
			  {
					FlushablePositionAwareChannel writer = logFiles.LogFile.Writer;
					TransactionLogWriter transactionLogWriter = new TransactionLogWriter( new LogEntryWriter( writer ) );
					transactionLogWriter.CheckPoint( tailScanner.TailInformation.lastCheckPoint.LogPosition );
					writer.PrepareForFlush().flush();
			  }
		 }

		 private static File ParseDir( Args args )
		 {
			  if ( args.Orphans().Count != 1 )
			  {
					Console.WriteLine( "Error: too much arguments provided." );
					PrintUsageAndExit();
			  }
			  File dir = new File( args.Orphans()[0] );
			  if ( !dir.Directory )
			  {
					Console.WriteLine( "Invalid directory: '" + dir + "'" );
					PrintUsageAndExit();
			  }
			  return dir;
		 }

		 private static void PrintUsageAndExit()
		 {
			  Console.WriteLine( "Store migration tool performs migration of a store in specified location to latest " + "supported store version." );
			  Console.WriteLine();
			  Console.WriteLine( "Options:" );
			  Console.WriteLine( "-help    print this help message" );
			  Console.WriteLine();
			  Console.WriteLine( "Usage:" );
			  Console.WriteLine( "./storeMigration [option] <store directory>" );
			  Environment.Exit( 1 );
		 }
	}

}