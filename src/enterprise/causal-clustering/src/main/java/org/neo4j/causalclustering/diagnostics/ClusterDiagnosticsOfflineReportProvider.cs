using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.diagnostics
{

	using FileNames = Neo4Net.causalclustering.core.consensus.log.segmented.FileNames;
	using ClusterStateDirectory = Neo4Net.causalclustering.core.state.ClusterStateDirectory;
	using ClusterStateException = Neo4Net.causalclustering.core.state.ClusterStateException;
	using DiagnosticsOfflineReportProvider = Neo4Net.Diagnostics.DiagnosticsOfflineReportProvider;
	using DiagnosticsReportSource = Neo4Net.Diagnostics.DiagnosticsReportSource;
	using DiagnosticsReportSources = Neo4Net.Diagnostics.DiagnosticsReportSources;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;

	public class ClusterDiagnosticsOfflineReportProvider : DiagnosticsOfflineReportProvider
	{
		 private FileSystemAbstraction _fs;
		 private File _clusterStateDirectory;
		 private ClusterStateException _clusterStateException;

		 public ClusterDiagnosticsOfflineReportProvider() : base("cc", "raft", "ccstate")
		 {
		 }

		 public override void Init( FileSystemAbstraction fs, Config config, File storeDirectory )
		 {
			  this._fs = fs;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File dataDir = config.get(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.data_directory);
			  File dataDir = config.Get( GraphDatabaseSettings.data_directory );
			  try
			  {
					_clusterStateDirectory = ( new ClusterStateDirectory( dataDir, storeDirectory, true ) ).initialize( fs ).get();
			  }
			  catch ( ClusterStateException e )
			  {
					_clusterStateException = e;
			  }
		 }

		 protected internal override IList<DiagnosticsReportSource> ProvideSources( ISet<string> classifiers )
		 {
			  IList<DiagnosticsReportSource> sources = new List<DiagnosticsReportSource>();
			  if ( classifiers.Contains( "raft" ) )
			  {
					GetRaftLogs( sources );
			  }
			  if ( classifiers.Contains( "ccstate" ) )
			  {
					GetClusterState( sources );
			  }

			  return sources;
		 }

		 private void GetRaftLogs( IList<DiagnosticsReportSource> sources )
		 {
			  if ( _clusterStateDirectory == null )
			  {
					sources.Add( DiagnosticsReportSources.newDiagnosticsString( "raft.txt", () => "error creating ClusterStateDirectory: " + _clusterStateException.Message ) );
					return;
			  }

			  File raftLogDirectory = new File( _clusterStateDirectory, RAFT_LOG_DIRECTORY_NAME );
			  FileNames fileNames = new FileNames( raftLogDirectory );
			  SortedDictionary<long, File> allFiles = fileNames.GetAllFiles( _fs, NullLog.Instance );

			  foreach ( File logFile in allFiles.Values )
			  {
					sources.Add( DiagnosticsReportSources.newDiagnosticsFile( "raft/" + logFile.Name, _fs, logFile ) );
			  }
		 }

		 private void GetClusterState( IList<DiagnosticsReportSource> sources )
		 {
			  if ( _clusterStateDirectory == null )
			  {
					sources.Add( DiagnosticsReportSources.newDiagnosticsString( "ccstate.txt", () => "error creating ClusterStateDirectory: " + _clusterStateException.Message ) );
					return;
			  }

			  foreach ( File file in _fs.listFiles( _clusterStateDirectory, ( dir, name ) => !name.Equals( RAFT_LOG_DIRECTORY_NAME ) ) )
			  {
					AddDirectory( "ccstate", file, sources );
			  }
		 }

		 /// <summary>
		 /// Add all files in a directory recursively.
		 /// </summary>
		 /// <param name="path"> current relative path for destination. </param>
		 /// <param name="dir"> current directory or file. </param>
		 /// <param name="sources"> list of source that will be accumulated. </param>
		 private void AddDirectory( string path, File dir, IList<DiagnosticsReportSource> sources )
		 {
			  string currentLevel = path + File.separator + dir.Name;
			  if ( _fs.isDirectory( dir ) )
			  {
					File[] files = _fs.listFiles( dir );
					if ( files != null )
					{
						 foreach ( File file in files )
						 {
							  AddDirectory( currentLevel, file, sources );
						 }
					}
			  }
			  else // File
			  {
					sources.Add( DiagnosticsReportSources.newDiagnosticsFile( currentLevel, _fs, dir ) );
			  }
		 }
	}

}