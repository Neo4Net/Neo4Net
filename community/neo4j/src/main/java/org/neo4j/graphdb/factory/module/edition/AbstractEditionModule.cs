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
namespace Org.Neo4j.Graphdb.factory.module.edition
{

	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using DefaultDatabaseManager = Org.Neo4j.Dmbs.Database.DefaultDatabaseManager;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using DatabaseEditionContext = Org.Neo4j.Graphdb.factory.module.edition.context.DatabaseEditionContext;
	using Service = Org.Neo4j.Helpers.Service;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using RestartableFileSystemWatcher = Org.Neo4j.Io.fs.watcher.RestartableFileSystemWatcher;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using SecurityModule = Org.Neo4j.Kernel.api.security.SecurityModule;
	using SecurityProvider = Org.Neo4j.Kernel.api.security.provider.SecurityProvider;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using AccessCapability = Org.Neo4j.Kernel.impl.factory.AccessCapability;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using ProcedureConfig = Org.Neo4j.Kernel.impl.proc.ProcedureConfig;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using DatabaseTransactionStats = Org.Neo4j.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using TransactionCounters = Org.Neo4j.Kernel.impl.transaction.stats.TransactionCounters;
	using DefaultFileDeletionEventListener = Org.Neo4j.Kernel.impl.util.watcher.DefaultFileDeletionEventListener;
	using DefaultFileSystemWatcherService = Org.Neo4j.Kernel.impl.util.watcher.DefaultFileSystemWatcherService;
	using FileSystemWatcherService = Org.Neo4j.Kernel.impl.util.watcher.FileSystemWatcherService;
	using Log = Org.Neo4j.Logging.Log;
	using Logger = Org.Neo4j.Logging.Logger;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using UsageData = Org.Neo4j.Udc.UsageData;
	using UsageDataKeys = Org.Neo4j.Udc.UsageDataKeys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.proc.temporal.TemporalFunction.registerTemporalFunctions;

	/// <summary>
	/// Edition module for <seealso cref="GraphDatabaseFacadeFactory"/>. Implementations of this class
	/// need to create all the services that would be specific for a particular edition of the database.
	/// </summary>
	public abstract class AbstractEditionModule
	{
		 private readonly DatabaseTransactionStats _databaseStatistics = new DatabaseTransactionStats();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal NetworkConnectionTracker ConnectionTrackerConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal ThreadToStatementContextBridge ThreadToTransactionBridgeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal long TransactionStartTimeoutConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal TransactionHeaderInformationFactory HeaderInformationFactoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal SchemaWriteGuard SchemaWriteGuardConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal ConstraintSemantics ConstraintSemanticsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal AccessCapability AccessCapabilityConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal IOLimiter IoLimiterConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal System.Func<File, FileSystemWatcherService> WatcherServiceFactoryConflict;
		 protected internal AvailabilityGuard GlobalAvailabilityGuard;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal SecurityProvider SecurityProviderConflict;

		 public abstract DatabaseEditionContext CreateDatabaseContext( string databaseName );

		 protected internal virtual FileSystemWatcherService CreateFileSystemWatcherService( FileSystemAbstraction fileSystem, File databaseDirectory, LogService logging, JobScheduler jobScheduler, Config config, System.Predicate<string> fileNameFilter )
		 {
			  if ( !config.Get( GraphDatabaseSettings.filewatcher_enabled ) )
			  {
					Log log = logging.GetInternalLog( this.GetType() );
					log.Info( "File watcher disabled by configuration." );
					return FileSystemWatcherService.EMPTY_WATCHER;
			  }

			  try
			  {
					RestartableFileSystemWatcher watcher = new RestartableFileSystemWatcher( fileSystem.FileWatcher() );
					watcher.AddFileWatchEventListener( new DefaultFileDeletionEventListener( logging, fileNameFilter ) );
					watcher.Watch( databaseDirectory );
					// register to watch database dir parent folder to see when database dir removed
					watcher.Watch( databaseDirectory.ParentFile );
					return new DefaultFileSystemWatcherService( jobScheduler, watcher );
			  }
			  catch ( Exception e )
			  {
					Log log = logging.GetInternalLog( this.GetType() );
					log.Warn( "Can not create file watcher for current file system. File monitoring capabilities for store " + "files will be disabled.", e );
					return FileSystemWatcherService.EMPTY_WATCHER;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerProcedures(org.neo4j.kernel.impl.proc.Procedures procedures, org.neo4j.kernel.impl.proc.ProcedureConfig procedureConfig) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public virtual void RegisterProcedures( Procedures procedures, ProcedureConfig procedureConfig )
		 {
			  procedures.RegisterProcedure( typeof( Org.Neo4j.Kernel.builtinprocs.BuiltInProcedures ) );
			  procedures.RegisterProcedure( typeof( Org.Neo4j.Kernel.builtinprocs.TokenProcedures ) );
			  procedures.RegisterProcedure( typeof( Org.Neo4j.Kernel.builtinprocs.BuiltInDbmsProcedures ) );
			  procedures.RegisterBuiltInFunctions( typeof( Org.Neo4j.Kernel.builtinprocs.BuiltInFunctions ) );
			  registerTemporalFunctions( procedures, procedureConfig );

			  RegisterEditionSpecificProcedures( procedures );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void registerEditionSpecificProcedures(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 protected internal abstract void RegisterEditionSpecificProcedures( Procedures procedures );

		 protected internal virtual void PublishEditionInfo( UsageData sysInfo, DatabaseInfo databaseInfo, Config config )
		 {
			  sysInfo.Set( UsageDataKeys.edition, databaseInfo.Edition );
			  sysInfo.Set( UsageDataKeys.operationalMode, databaseInfo.OperationalMode );
			  config.Augment( GraphDatabaseSettings.editionName, databaseInfo.Edition.ToString() );
		 }

		 public virtual DatabaseManager CreateDatabaseManager( GraphDatabaseFacade graphDatabaseFacade, PlatformModule platform, AbstractEditionModule edition, Procedures procedures, Logger msgLog )
		 {
			  return new DefaultDatabaseManager( platform, edition, procedures, msgLog, graphDatabaseFacade );
		 }

		 public abstract void CreateSecurityModule( PlatformModule platformModule, Procedures procedures );

		 protected internal static SecurityModule SetupSecurityModule( PlatformModule platformModule, AbstractEditionModule editionModule, Log log, Procedures procedures, string key )
		 {
			  SecurityModule.Dependencies securityModuleDependencies = new SecurityModuleDependenciesDependencies( platformModule, editionModule, procedures );
			  IEnumerable<SecurityModule> candidates = Service.load( typeof( SecurityModule ) );
			  foreach ( SecurityModule candidate in candidates )
			  {
					if ( candidate.Matches( key ) )
					{
						 try
						 {
							  candidate.Setup( securityModuleDependencies );
							  return candidate;
						 }
						 catch ( Exception e )
						 {
							  string errorMessage = "Failed to load security module.";
							  string innerErrorMessage = e.Message;

							  if ( !string.ReferenceEquals( innerErrorMessage, null ) )
							  {
									log.Error( errorMessage + " Caused by: " + innerErrorMessage, e );
							  }
							  else
							  {
									log.Error( errorMessage, e );
							  }
							  throw new Exception( errorMessage, e );
						 }
					}
			  }
			  string errorMessage = "Failed to load security module with key '" + key + "'.";
			  log.Error( errorMessage );
			  throw new System.ArgumentException( errorMessage );
		 }

		 protected internal virtual NetworkConnectionTracker CreateConnectionTracker()
		 {
			  return NetworkConnectionTracker.NO_OP;
		 }

		 public virtual DatabaseTransactionStats CreateTransactionMonitor()
		 {
			  return _databaseStatistics;
		 }

		 public virtual TransactionCounters GlobalTransactionCounter()
		 {
			  return _databaseStatistics;
		 }

		 public virtual AvailabilityGuard GetGlobalAvailabilityGuard( Clock clock, LogService logService, Config config )
		 {
			  if ( GlobalAvailabilityGuard == null )
			  {
					GlobalAvailabilityGuard = new DatabaseAvailabilityGuard( config.Get( GraphDatabaseSettings.active_database ), clock, logService.GetInternalLog( typeof( DatabaseAvailabilityGuard ) ) );
			  }
			  return GlobalAvailabilityGuard;
		 }

		 public virtual DatabaseAvailabilityGuard CreateDatabaseAvailabilityGuard( string databaseName, Clock clock, LogService logService, Config config )
		 {
			  return ( DatabaseAvailabilityGuard ) GetGlobalAvailabilityGuard( clock, logService, config );
		 }

		 public virtual void CreateDatabases( DatabaseManager databaseManager, Config config )
		 {
			  databaseManager.CreateDatabase( config.Get( GraphDatabaseSettings.active_database ) );
		 }

		 public virtual long TransactionStartTimeout
		 {
			 get
			 {
				  return TransactionStartTimeoutConflict;
			 }
		 }

		 public virtual SchemaWriteGuard SchemaWriteGuard
		 {
			 get
			 {
				  return SchemaWriteGuardConflict;
			 }
		 }

		 public virtual TransactionHeaderInformationFactory HeaderInformationFactory
		 {
			 get
			 {
				  return HeaderInformationFactoryConflict;
			 }
		 }

		 public virtual ConstraintSemantics ConstraintSemantics
		 {
			 get
			 {
				  return ConstraintSemanticsConflict;
			 }
		 }

		 public virtual IOLimiter IoLimiter
		 {
			 get
			 {
				  return IoLimiterConflict;
			 }
		 }

		 public virtual AccessCapability AccessCapability
		 {
			 get
			 {
				  return AccessCapabilityConflict;
			 }
		 }

		 public virtual System.Func<File, FileSystemWatcherService> WatcherServiceFactory
		 {
			 get
			 {
				  return WatcherServiceFactoryConflict;
			 }
		 }

		 public virtual ThreadToStatementContextBridge ThreadToTransactionBridge
		 {
			 get
			 {
				  return ThreadToTransactionBridgeConflict;
			 }
		 }

		 public virtual NetworkConnectionTracker ConnectionTracker
		 {
			 get
			 {
				  return ConnectionTrackerConflict;
			 }
		 }

		 public virtual SecurityProvider SecurityProvider
		 {
			 get
			 {
				  return SecurityProviderConflict;
			 }
			 set
			 {
				  this.SecurityProviderConflict = value;
			 }
		 }

	}

}