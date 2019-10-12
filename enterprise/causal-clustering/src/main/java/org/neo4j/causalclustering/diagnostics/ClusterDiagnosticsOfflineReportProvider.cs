using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.diagnostics
{

	using FileNames = Org.Neo4j.causalclustering.core.consensus.log.segmented.FileNames;
	using ClusterStateDirectory = Org.Neo4j.causalclustering.core.state.ClusterStateDirectory;
	using ClusterStateException = Org.Neo4j.causalclustering.core.state.ClusterStateException;
	using DiagnosticsOfflineReportProvider = Org.Neo4j.Diagnostics.DiagnosticsOfflineReportProvider;
	using DiagnosticsReportSource = Org.Neo4j.Diagnostics.DiagnosticsReportSource;
	using DiagnosticsReportSources = Org.Neo4j.Diagnostics.DiagnosticsReportSources;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLog = Org.Neo4j.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;

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
//ORIGINAL LINE: final java.io.File dataDir = config.get(org.neo4j.graphdb.factory.GraphDatabaseSettings.data_directory);
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