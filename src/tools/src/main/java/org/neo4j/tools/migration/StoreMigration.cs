using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.tools.migration
{

	using GraphDatabaseDependencies = Neo4Net.GraphDb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using DatabaseKernelExtensions = Neo4Net.Kernel.extension.DatabaseKernelExtensions;
	using DefaultExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.DefaultExplicitIndexProvider;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using IJobSchedulerFactory = Neo4Net.Kernel.impl.scheduler.JobSchedulerFactory;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using SimpleKernelContext = Neo4Net.Kernel.impl.spi.SimpleKernelContext;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using DatabaseMigrator = Neo4Net.Kernel.impl.storemigration.DatabaseMigrator;
	using StoreUpgrader = Neo4Net.Kernel.impl.storemigration.StoreUpgrader;
	using VisibleMigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.VisibleMigrationProgressMonitor;
	using StoreMigrator = Neo4Net.Kernel.impl.storemigration.participant.StoreMigrator;
	using FlushablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.FlushablePositionAwareChannel;
	using TransactionLogWriter = Neo4Net.Kernel.impl.transaction.log.TransactionLogWriter;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using DefaultIndexProviderMap = Neo4Net.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreLogService = Neo4Net.Logging.Internal.StoreLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using KernelExtensionFailureStrategies = Neo4Net.Kernel.extension.KernelExtensionFailureStrategies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.extension.KernelExtensionFailureStrategies.ignore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory.createPageCache;

	/// <summary>
	/// Stand alone tool for migrating/upgrading a Neo4Net database from one version to the next.
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
//ORIGINAL LINE: public static void run(final Neo4Net.io.fs.FileSystemAbstraction fs, final java.io.File storeDirectory, Neo4Net.kernel.configuration.Config config, Neo4Net.logging.LogProvider userLogProvider) throws Exception
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
			  IJobScheduler jobScheduler = IJobSchedulerFactory.createInitializedScheduler();
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
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles = Neo4Net.kernel.impl.transaction.log.files.LogFilesBuilder.activeFilesBuilder(databaseLayout, fs, pageCache).withConfig(config).build();
						LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( databaseLayout, fs, pageCache ).withConfig( config ).build();
						LogTailScanner tailScanner = new LogTailScanner( logFiles, new VersionAwareLogEntryReader<Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>(), monitors );
      
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
//ORIGINAL LINE: private static void appendCheckpoint(Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles, Neo4Net.kernel.recovery.LogTailScanner tailScanner) throws java.io.IOException
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