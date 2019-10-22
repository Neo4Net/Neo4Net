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
namespace Neo4Net.Kernel.impl.query
{

	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using FormattedLog = Neo4Net.Logging.FormattedLog;
	using Log = Neo4Net.Logging.Log;
	using RotatingFileOutputStreamSupplier = Neo4Net.Logging.RotatingFileOutputStreamSupplier;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.file.Files.createOrOpenAsOutputStream;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.query.QueryLogger.NO_LOG;

	internal class DynamicLoggingQueryExecutionMonitor : LifecycleAdapter, QueryExecutionMonitor
	{
		 private readonly Config _config;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly IJobScheduler _scheduler;
		 private readonly Log _debugLog;

		 /// <summary>
		 /// The currently configured QueryLogger.
		 /// This may be accessed concurrently by any thread, even while the logger is being reconfigured.
		 /// </summary>
		 private volatile QueryLogger _currentLog = NO_LOG;

		 // These fields are only accessed during (re-) configuration, and are protected from concurrent access
		 // by the monitor lock on DynamicQueryLogger.
		 private ZoneId _currentLogTimeZone;
		 private FormattedLog.Builder _logBuilder;
		 private File _currentQueryLogFile;
		 private long _currentRotationThreshold;
		 private int _currentMaxArchives;
		 private Log _log;
		 private System.IDisposable _closable;

		 internal DynamicLoggingQueryExecutionMonitor( Config config, FileSystemAbstraction fileSystem, IJobScheduler scheduler, Log debugLog )
		 {
			  this._config = config;
			  this._fileSystem = fileSystem;
			  this._scheduler = scheduler;
			  this._debugLog = debugLog;
		 }

		 public override void Init()
		 {
			 lock ( this )
			 {
				  // This set of settings are currently not dynamic:
				  _currentLogTimeZone = _config.get( GraphDatabaseSettings.db_timezone ).ZoneId;
				  _logBuilder = FormattedLog.withZoneId( _currentLogTimeZone );
				  _currentQueryLogFile = _config.get( GraphDatabaseSettings.log_queries_filename );
      
				  UpdateSettings();
      
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries );
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries_threshold );
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries_rotation_threshold );
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries_max_archives );
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries_runtime_logging_enabled );
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries_parameter_logging_enabled );
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries_page_detail_logging_enabled );
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries_allocation_logging_enabled );
				  RegisterDynamicSettingUpdater( GraphDatabaseSettings.log_queries_detailed_time_logging_enabled );
			 }
		 }

		 private void RegisterDynamicSettingUpdater<T>( Setting<T> setting )
		 {
			  _config.registerDynamicUpdateListener( setting, ( a,b ) => updateSettings() );
		 }

		 private void UpdateSettings()
		 {
			 lock ( this )
			 {
				  UpdateLogSettings();
				  UpdateQueryLoggerSettings();
			 }
		 }

		 private void UpdateQueryLoggerSettings()
		 {
			  // This method depends on any log settings having been updated before hand, via updateLogSettings.
			  // The only dynamic settings here are log_queries, and log_queries_threshold which is read by the
			  // ConfiguredQueryLogger constructor. We can add more in the future, though. The various content settings
			  // are prime candidates.
			  if ( _config.get( GraphDatabaseSettings.log_queries ) )
			  {
					_currentLog = new ConfiguredQueryLogger( _log, _config );
			  }
			  else
			  {
					_currentLog = NO_LOG;
			  }
		 }

		 private void UpdateLogSettings()
		 {
			  // The dynamic setting here is log_queries, log_queries_rotation_threshold, and log_queries_max_archives.
			  // NOTE: We can't register this method as a settings update callback, because we don't update the `currentLog`
			  // field in this method. Settings updates must always go via the `updateQueryLoggerSettings` method.
			  if ( _config.get( GraphDatabaseSettings.log_queries ) )
			  {
					long rotationThreshold = _config.get( GraphDatabaseSettings.log_queries_rotation_threshold );
					int maxArchives = _config.get( GraphDatabaseSettings.log_queries_max_archives );

					try
					{
						 if ( LogRotationIsEnabled( rotationThreshold ) )
						 {
							  bool needsRebuild = _closable == null; // We need to rebuild the log if we currently don't have any,
							  needsRebuild |= _currentRotationThreshold != rotationThreshold; // or if rotation threshold has changed,
							  needsRebuild |= _currentMaxArchives != maxArchives; // or if the max archives setting has changed.
							  if ( needsRebuild )
							  {
									CloseCurrentLogIfAny();
									BuildRotatingLog( rotationThreshold, maxArchives );
							  }
						 }
						 else if ( _currentRotationThreshold != rotationThreshold || _closable == null )
						 {
							  // We go from rotating (or uninitialised) log to non-rotating. Always rebuild.
							  CloseCurrentLogIfAny();
							  BuildNonRotatingLog();
						 }

						 _currentRotationThreshold = rotationThreshold;
						 _currentMaxArchives = maxArchives;
					}
					catch ( IOException exception )
					{
						 _debugLog.warn( "Failed to build query log", exception );
					}
			  }
			  else
			  {
					CloseCurrentLogIfAny();
			  }
		 }

		 private bool LogRotationIsEnabled( long threshold )
		 {
			  return threshold > 0;
		 }

		 private void CloseCurrentLogIfAny()
		 {
			  if ( _closable != null )
			  {
					try
					{
						 _closable.Dispose();
					}
					catch ( IOException exception )
					{
						 _debugLog.warn( "Failed to close current log: " + _closable, exception );
					}
					_closable = null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void buildRotatingLog(long rotationThreshold, int maxArchives) throws java.io.IOException
		 private void BuildRotatingLog( long rotationThreshold, int maxArchives )
		 {
			  RotatingFileOutputStreamSupplier rotatingSupplier = new RotatingFileOutputStreamSupplier( _fileSystem, _currentQueryLogFile, rotationThreshold, 0, maxArchives, _scheduler.executor( Group.LOG_ROTATION ) );
			  _log = _logBuilder.toOutputStream( rotatingSupplier );
			  _closable = rotatingSupplier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void buildNonRotatingLog() throws java.io.IOException
		 private void BuildNonRotatingLog()
		 {
			  Stream logOutputStream = createOrOpenAsOutputStream( _fileSystem, _currentQueryLogFile, true );
			  _log = _logBuilder.toOutputStream( logOutputStream );
			  _closable = logOutputStream;
		 }

		 public override void Shutdown()
		 {
			 lock ( this )
			 {
				  CloseCurrentLogIfAny();
			 }
		 }

		 public override void EndFailure( ExecutingQuery query, Exception failure )
		 {
			  _currentLog.failure( query, failure );
		 }

		 public override void EndSuccess( ExecutingQuery query )
		 {
			  _currentLog.success( query );
		 }
	}

}