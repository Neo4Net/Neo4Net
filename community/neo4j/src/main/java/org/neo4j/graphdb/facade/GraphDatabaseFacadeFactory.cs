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
namespace Org.Neo4j.Graphdb.facade
{

	using BoltServer = Org.Neo4j.Bolt.BoltServer;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using DataSourceModule = Org.Neo4j.Graphdb.factory.module.DataSourceModule;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using URLAccessRule = Org.Neo4j.Graphdb.security.URLAccessRule;
	using Geometry = Org.Neo4j.Graphdb.spatial.Geometry;
	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using Org.Neo4j.Helpers.Collection;
	using DataCollectorManager = Org.Neo4j.@internal.DataCollectorManager;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using SecurityProvider = Org.Neo4j.Kernel.api.security.provider.SecurityProvider;
	using AvailabilityGuardInstaller = Org.Neo4j.Kernel.availability.AvailabilityGuardInstaller;
	using StartupWaiter = Org.Neo4j.Kernel.availability.StartupWaiter;
	using SpecialBuiltInProcedures = Org.Neo4j.Kernel.builtinprocs.SpecialBuiltInProcedures;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.extension;
	using NonTransactionalDbmsOperations = Org.Neo4j.Kernel.Impl.Api.dbms.NonTransactionalDbmsOperations;
	using VmPauseMonitorComponent = Org.Neo4j.Kernel.impl.cache.VmPauseMonitorComponent;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using PublishPageCacheTracerMetricsAfterStart = Org.Neo4j.Kernel.impl.pagecache.PublishPageCacheTracerMetricsAfterStart;
	using ProcedureConfig = Org.Neo4j.Kernel.impl.proc.ProcedureConfig;
	using ProcedureTransactionProvider = Org.Neo4j.Kernel.impl.proc.ProcedureTransactionProvider;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using TerminationGuardProvider = Org.Neo4j.Kernel.impl.proc.TerminationGuardProvider;
	using QueryEngineProvider = Org.Neo4j.Kernel.impl.query.QueryEngineProvider;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Version = Org.Neo4j.Kernel.@internal.Version;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Logger = Org.Neo4j.Logging.Logger;
	using ProcedureTransaction = Org.Neo4j.Procedure.ProcedureTransaction;
	using DeferredExecutor = Org.Neo4j.Scheduler.DeferredExecutor;
	using Group = Org.Neo4j.Scheduler.Group;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTGeometry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTPoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTRelationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.DATABASE_API;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.DEPENDENCY_RESOLVER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.KERNEL_TRANSACTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.PROCEDURE_CALL_CONTEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.SECURITY_CONTEXT;

	/// <summary>
	/// This is the main factory for creating database instances. It delegates creation to three different modules
	/// (<seealso cref="PlatformModule"/>, <seealso cref="AbstractEditionModule"/>, and <seealso cref="DataSourceModule"/>),
	/// which create all the specific services needed to run a graph database.
	/// <para>
	/// To create test versions of databases, override an edition factory (e.g. {@link org.neo4j.kernel.impl.factory
	/// .CommunityFacadeFactory}), and replace modules
	/// with custom versions that instantiate alternative services.
	/// </para>
	/// </summary>
	public class GraphDatabaseFacadeFactory
	{
		 public interface Dependencies
		 {
			  /// <summary>
			  /// Allowed to be null. Null means that no external <seealso cref="org.neo4j.kernel.monitoring.Monitors"/> was created,
			  /// let the
			  /// database create its own monitors instance.
			  /// </summary>
			  Monitors Monitors();

			  LogProvider UserLogProvider();

			  IEnumerable<Type> SettingsClasses();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions();
			  IEnumerable<KernelExtensionFactory<object>> KernelExtensions();

			  IDictionary<string, URLAccessRule> UrlAccessRules();

			  IEnumerable<QueryEngineProvider> ExecutionEngines();

			  /// <summary>
			  /// Collection of command executors to start running once the db is started
			  /// </summary>
			  IEnumerable<Pair<DeferredExecutor, Group>> DeferredExecutors();

			  /// <summary>
			  /// Simple callback for providing a global availability guard to top level components
			  /// once it is created by <seealso cref="GraphDatabaseFacadeFactory.initFacade(File, Config, Dependencies, GraphDatabaseFacade)"/>.
			  /// By default this callback is a no-op
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default org.neo4j.kernel.availability.AvailabilityGuardInstaller availabilityGuardInstaller()
	//		  {
	//				return availabilityGuard ->
	//				{
	//				};
	//		  }
		 }

		 protected internal readonly DatabaseInfo DatabaseInfo;
		 private readonly System.Func<PlatformModule, AbstractEditionModule> _editionFactory;

		 public GraphDatabaseFacadeFactory( DatabaseInfo databaseInfo, System.Func<PlatformModule, AbstractEditionModule> editionFactory )
		 {
			  this.DatabaseInfo = databaseInfo;
			  this._editionFactory = editionFactory;
		 }

		 /// <summary>
		 /// Instantiate a graph database given configuration and dependencies.
		 /// </summary>
		 /// <param name="storeDir"> the directory where the Neo4j data store is located </param>
		 /// <param name="config"> configuration </param>
		 /// <param name="dependencies"> the dependencies required to construct the <seealso cref="GraphDatabaseFacade"/> </param>
		 /// <returns> the newly constructed <seealso cref="GraphDatabaseFacade"/> </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.kernel.impl.factory.GraphDatabaseFacade newFacade(java.io.File storeDir, org.neo4j.kernel.configuration.Config config, final Dependencies dependencies)
		 public virtual GraphDatabaseFacade NewFacade( File storeDir, Config config, Dependencies dependencies )
		 {
			  return InitFacade( storeDir, config, dependencies, new GraphDatabaseFacade() );
		 }

		 /// <summary>
		 /// Instantiate a graph database given configuration, dependencies, and a custom implementation of {@link org
		 /// .neo4j.kernel.impl.factory.GraphDatabaseFacade}.
		 /// </summary>
		 /// <param name="storeDir"> the directory where the Neo4j data store is located </param>
		 /// <param name="params"> configuration parameters </param>
		 /// <param name="dependencies"> the dependencies required to construct the <seealso cref="GraphDatabaseFacade"/> </param>
		 /// <param name="graphDatabaseFacade"> the already created facade which needs initialisation </param>
		 /// <returns> the initialised <seealso cref="GraphDatabaseFacade"/> </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.kernel.impl.factory.GraphDatabaseFacade initFacade(java.io.File storeDir, java.util.Map<String,String> params, final Dependencies dependencies, final org.neo4j.kernel.impl.factory.GraphDatabaseFacade graphDatabaseFacade)
		 public virtual GraphDatabaseFacade InitFacade( File storeDir, IDictionary<string, string> @params, Dependencies dependencies, GraphDatabaseFacade graphDatabaseFacade )
		 {
			  return InitFacade( storeDir, Config.defaults( @params ), dependencies, graphDatabaseFacade );
		 }

		 /// <summary>
		 /// Instantiate a graph database given configuration, dependencies, and a custom implementation of {@link org
		 /// .neo4j.kernel.impl.factory.GraphDatabaseFacade}.
		 /// </summary>
		 /// <param name="storeDir"> the directory where the Neo4j data store is located </param>
		 /// <param name="config"> configuration </param>
		 /// <param name="dependencies"> the dependencies required to construct the <seealso cref="GraphDatabaseFacade"/> </param>
		 /// <param name="graphDatabaseFacade"> the already created facade which needs initialisation </param>
		 /// <returns> the initialised <seealso cref="GraphDatabaseFacade"/> </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.kernel.impl.factory.GraphDatabaseFacade initFacade(java.io.File storeDir, org.neo4j.kernel.configuration.Config config, final Dependencies dependencies, final org.neo4j.kernel.impl.factory.GraphDatabaseFacade graphDatabaseFacade)
		 public virtual GraphDatabaseFacade InitFacade( File storeDir, Config config, Dependencies dependencies, GraphDatabaseFacade graphDatabaseFacade )
		 {
			  PlatformModule platform = CreatePlatform( storeDir, config, dependencies );
			  AbstractEditionModule edition = _editionFactory.apply( platform );
			  dependencies.AvailabilityGuardInstaller()(edition.GetGlobalAvailabilityGuard(platform.Clock, platform.Logging, platform.Config));

			  platform.Life.add( new VmPauseMonitorComponent( config, platform.Logging.getInternalLog( typeof( VmPauseMonitorComponent ) ), platform.JobScheduler ) );

			  Procedures procedures = SetupProcedures( platform, edition, graphDatabaseFacade );
			  platform.Dependencies.satisfyDependency( new NonTransactionalDbmsOperations( procedures ) );

			  Logger msgLog = platform.Logging.getInternalLog( this.GetType() ).infoLogger();
			  DatabaseManager databaseManager = edition.CreateDatabaseManager( graphDatabaseFacade, platform, edition, procedures, msgLog );
			  platform.Life.add( databaseManager );
			  platform.Dependencies.satisfyDependency( databaseManager );

			  DataCollectorManager dataCollectorManager = new DataCollectorManager( platform.DataSourceManager, platform.JobScheduler, procedures, platform.Monitors, platform.Config );
			  platform.Life.add( dataCollectorManager );

			  edition.CreateSecurityModule( platform, procedures );
			  SecurityProvider securityProvider = edition.SecurityProvider;
			  platform.Dependencies.satisfyDependencies( securityProvider.AuthManager() );
			  platform.Dependencies.satisfyDependencies( securityProvider.UserManagerSupplier() );

			  platform.Life.add( platform.GlobalKernelExtensions );
			  platform.Life.add( CreateBoltServer( platform, edition, databaseManager ) );
			  platform.Dependencies.satisfyDependency( edition.GlobalTransactionCounter() );
			  platform.Life.add( new PublishPageCacheTracerMetricsAfterStart( platform.Tracers.pageCursorTracerSupplier ) );
			  platform.Life.add( new StartupWaiter( edition.GetGlobalAvailabilityGuard( platform.Clock, platform.Logging, platform.Config ), edition.TransactionStartTimeout ) );
			  platform.Dependencies.satisfyDependency( edition.SchemaWriteGuard );
			  platform.Life.Last = platform.EventHandlers;

			  edition.CreateDatabases( databaseManager, config );

			  string activeDatabase = config.Get( GraphDatabaseSettings.active_database );
			  GraphDatabaseFacade databaseFacade = databaseManager.GetDatabaseFacade( activeDatabase ).orElseThrow( () => new System.InvalidOperationException(string.Format("Database {0} not found. Please check the logs for startup errors.", activeDatabase)) );

			  Exception error = null;
			  try
			  {
					platform.Life.start();
			  }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final Throwable throwable)
			  catch ( Exception throwable )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					error = new Exception( "Error starting " + this.GetType().FullName + ", " + platform.StoreLayout.storeDirectory(), throwable );
			  }
			  finally
			  {
					if ( error != null )
					{
						 try
						 {
							  graphDatabaseFacade.Shutdown();
						 }
						 catch ( Exception shutdownError )
						 {
							  error.addSuppressed( shutdownError );
						 }
					}
			  }

			  if ( error != null )
			  {
					msgLog.Log( "Failed to start database", error );
					throw error;
			  }

			  return databaseFacade;
		 }

		 /// <summary>
		 /// Create the platform module. Override to replace with custom module.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.graphdb.factory.module.PlatformModule createPlatform(java.io.File storeDir, org.neo4j.kernel.configuration.Config config, final Dependencies dependencies)
		 protected internal virtual PlatformModule CreatePlatform( File storeDir, Config config, Dependencies dependencies )
		 {
			  return new PlatformModule( storeDir, config, DatabaseInfo, dependencies );
		 }

		 private static Procedures SetupProcedures( PlatformModule platform, AbstractEditionModule editionModule, GraphDatabaseFacade facade )
		 {
			  File pluginDir = platform.Config.get( GraphDatabaseSettings.plugin_dir );
			  Log internalLog = platform.Logging.getInternalLog( typeof( Procedures ) );

			  ProcedureConfig procedureConfig = new ProcedureConfig( platform.Config );
			  Procedures procedures = new Procedures( facade, new SpecialBuiltInProcedures( Version.Neo4jVersion, platform.DatabaseInfo.edition.ToString() ), pluginDir, internalLog, procedureConfig );
			  platform.Life.add( procedures );
			  platform.Dependencies.satisfyDependency( procedures );

			  procedures.RegisterType( typeof( Node ), NTNode );
			  procedures.RegisterType( typeof( Relationship ), NTRelationship );
			  procedures.RegisterType( typeof( Path ), NTPath );
			  procedures.RegisterType( typeof( Geometry ), NTGeometry );
			  procedures.RegisterType( typeof( Point ), NTPoint );

			  // Register injected public API components
			  Log proceduresLog = platform.Logging.getUserLog( typeof( Procedures ) );
			  procedures.RegisterComponent( typeof( Log ), ctx => proceduresLog, true );

			  procedures.RegisterComponent( typeof( ProcedureTransaction ), new ProcedureTransactionProvider(), true );
			  procedures.RegisterComponent( typeof( Org.Neo4j.Procedure.TerminationGuard ), new TerminationGuardProvider(), true );

			  // Below components are not public API, but are made available for internal
			  // procedures to call, and to provide temporary workarounds for the following
			  // patterns:
			  //  - Batch-transaction imports (GDAPI, needs to be real and passed to background processing threads)
			  //  - Group-transaction writes (same pattern as above, but rather than splitting large transactions,
			  //                              combine lots of small ones)
			  //  - Bleeding-edge performance (KernelTransaction, to bypass overhead of working with Core API)
			  procedures.RegisterComponent( typeof( DependencyResolver ), ctx => ctx.get( DEPENDENCY_RESOLVER ), false );
			  procedures.RegisterComponent( typeof( KernelTransaction ), ctx => ctx.get( KERNEL_TRANSACTION ), false );
			  procedures.RegisterComponent( typeof( GraphDatabaseAPI ), ctx => ctx.get( DATABASE_API ), false );

			  // Security procedures
			  procedures.RegisterComponent( typeof( SecurityContext ), ctx => ctx.get( SECURITY_CONTEXT ), true );

			  procedures.RegisterComponent( typeof( ProcedureCallContext ), ctx => ctx.get( PROCEDURE_CALL_CONTEXT ), true );

			  // Edition procedures
			  try
			  {
					editionModule.RegisterProcedures( procedures, procedureConfig );
			  }
			  catch ( KernelException e )
			  {
					internalLog.Error( "Failed to register built-in edition procedures at start up: " + e.Message );
			  }

			  return procedures;
		 }

		 private static BoltServer CreateBoltServer( PlatformModule platform, AbstractEditionModule edition, DatabaseManager databaseManager )
		 {
			  return new BoltServer( databaseManager, platform.JobScheduler, platform.ConnectorPortRegister, edition.ConnectionTracker, platform.UsageData, platform.Config, platform.Clock, platform.Monitors, platform.Logging, platform.Dependencies );
		 }
	}

}