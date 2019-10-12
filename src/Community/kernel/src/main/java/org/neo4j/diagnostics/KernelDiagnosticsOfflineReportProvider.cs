using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Diagnostics
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using DiagnosticsPhase = Neo4Net.@internal.Diagnostics.DiagnosticsPhase;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using KernelDiagnostics = Neo4Net.Kernel.@internal.KernelDiagnostics;
	using BufferingLog = Neo4Net.Logging.BufferingLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.diagnostics.DiagnosticsReportSources.newDiagnosticsFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.diagnostics.DiagnosticsReportSources.newDiagnosticsRotatingFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.diagnostics.DiagnosticsReportSources.newDiagnosticsString;

	public class KernelDiagnosticsOfflineReportProvider : DiagnosticsOfflineReportProvider
	{
		 private FileSystemAbstraction _fs;
		 private Config _config;
		 private DatabaseLayout _databaseLayout;

		 public KernelDiagnosticsOfflineReportProvider() : base("kernel", "logs", "plugins", "tree", "tx")
		 {
		 }

		 public override void Init( FileSystemAbstraction fs, Config config, File storeDirectory )
		 {
			  this._fs = fs;
			  this._config = config;
			  this._databaseLayout = DatabaseLayout.of( storeDirectory );
		 }

		 protected internal override IList<DiagnosticsReportSource> ProvideSources( ISet<string> classifiers )
		 {
			  IList<DiagnosticsReportSource> sources = new List<DiagnosticsReportSource>();
			  if ( classifiers.Contains( "logs" ) )
			  {
					GetLogFiles( sources );
			  }
			  if ( classifiers.Contains( "plugins" ) )
			  {
					ListPlugins( sources );
			  }
			  if ( classifiers.Contains( "tree" ) )
			  {
					ListDataDirectory( sources );
			  }
			  if ( classifiers.Contains( "tx" ) )
			  {
					GetTransactionLogFiles( sources );
			  }

			  return sources;
		 }

		 /// <summary>
		 /// Collect a list of all the files in the plugins directory.
		 /// </summary>
		 /// <param name="sources"> destination of the sources. </param>
		 private void ListPlugins( IList<DiagnosticsReportSource> sources )
		 {
			  File pluginDirectory = _config.get( GraphDatabaseSettings.plugin_dir );
			  if ( _fs.fileExists( pluginDirectory ) )
			  {
					StringBuilder sb = new StringBuilder();
					sb.Append( "List of plugin directory:" ).Append( Environment.NewLine );
					ListContentOfDirectory( pluginDirectory, "  ", sb );

					sources.Add( newDiagnosticsString( "plugins.txt", sb.toString ) );
			  }
		 }

		 private void ListContentOfDirectory( File directory, string prefix, StringBuilder sb )
		 {
			  if ( !_fs.isDirectory( directory ) )
			  {
					return;
			  }

			  File[] files = _fs.listFiles( directory );
			  foreach ( File file in files )
			  {
					if ( _fs.isDirectory( file ) )
					{
						 ListContentOfDirectory( file, prefix + File.separator + file.Name, sb );
					}
					else
					{
						 sb.Append( prefix ).Append( file.Name ).Append( Environment.NewLine );
					}
			  }
		 }

		 /// <summary>
		 /// Print a tree view of all the files in the database directory with files sizes.
		 /// </summary>
		 /// <param name="sources"> destination of the sources. </param>
		 private void ListDataDirectory( IList<DiagnosticsReportSource> sources )
		 {
			  KernelDiagnostics.StoreFiles storeFiles = new KernelDiagnostics.StoreFiles( _databaseLayout );

			  BufferingLog logger = new BufferingLog();
			  storeFiles.Dump( DiagnosticsPhase.INITIALIZED, logger.DebugLogger() );

			  sources.Add( newDiagnosticsString( "tree.txt", logger.toString ) );
		 }

		 /// <summary>
		 /// Add {@code debug.log}, {@code neo4j.log} and {@code gc.log}. All with all available rotated files.
		 /// </summary>
		 /// <param name="sources"> destination of the sources. </param>
		 private void GetLogFiles( IList<DiagnosticsReportSource> sources )
		 {
			  // debug.log
			  File debugLogFile = _config.get( GraphDatabaseSettings.store_internal_log_path );
			  if ( _fs.fileExists( debugLogFile ) )
			  {
					( ( IList<DiagnosticsReportSource> )sources ).AddRange( newDiagnosticsRotatingFile( "logs/debug.log", _fs, debugLogFile ) );
			  }

			  // neo4j.log
			  File logDirectory = _config.get( GraphDatabaseSettings.logs_directory );
			  File neo4jLog = new File( logDirectory, "neo4j.log" );
			  if ( _fs.fileExists( neo4jLog ) )
			  {
					sources.Add( newDiagnosticsFile( "logs/neo4j.log", _fs, neo4jLog ) );
			  }

			  // gc.log
			  File gcLog = new File( logDirectory, "gc.log" );
			  if ( _fs.fileExists( gcLog ) )
			  {
					sources.Add( newDiagnosticsFile( "logs/gc.log", _fs, gcLog ) );
			  }
			  // we might have rotation activated, check
			  int i = 0;
			  while ( true )
			  {
					File gcRotationLog = new File( logDirectory, "gc.log." + i );
					if ( !_fs.fileExists( gcRotationLog ) )
					{
						 break;
					}
					sources.Add( newDiagnosticsFile( "logs/gc.log." + i, _fs, gcRotationLog ) );
					i++;
			  }
			  // there are other rotation schemas but nothing we can predict...
		 }

		 /// <summary>
		 /// Add all available log files as sources.
		 /// </summary>
		 /// <param name="sources"> destination of the sources. </param>
		 private void GetTransactionLogFiles( IList<DiagnosticsReportSource> sources )
		 {
			  try
			  {
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _databaseLayout.databaseDirectory(), _fs ).build();
					foreach ( File file in logFiles.LogFilesConflict() )
					{
						 sources.Add( DiagnosticsReportSources.NewDiagnosticsFile( "tx/" + file.Name, _fs, file ) );
					}
			  }
			  catch ( IOException e )
			  {
					sources.Add( DiagnosticsReportSources.NewDiagnosticsString( "tx.txt", () => "Error getting tx logs: " + e.Message ) );
			  }
		 }

	}

}