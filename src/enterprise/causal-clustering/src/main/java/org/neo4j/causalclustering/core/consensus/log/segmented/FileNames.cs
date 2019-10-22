using System;
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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Log = Neo4Net.Logging.Log;

	/// <summary>
	/// Deals with file names for the RAFT log. The files are named as
	/// 
	///   raft.log.0
	///   raft.log.1
	///   ...
	///   raft.log.23
	///   ...
	/// 
	/// where the suffix represents the version, which is a strictly monotonic sequence.
	/// </summary>
	public class FileNames
	{
		 internal const string BASE_FILE_NAME = "raft.log.";
		 private const string VERSION_MATCH = "(0|[1-9]\\d*)";

		 private readonly File _baseDirectory;
		 private readonly Pattern _logFilePattern;

		 /// <summary>
		 /// Creates an object useful for managing RAFT log file names.
		 /// </summary>
		 /// <param name="baseDirectory"> The base directory in which the RAFT log files reside. </param>
		 public FileNames( File baseDirectory )
		 {
			  this._baseDirectory = baseDirectory;
			  this._logFilePattern = Pattern.compile( BASE_FILE_NAME + VERSION_MATCH );
		 }

		 /// <summary>
		 /// Creates a file object for the specific version.
		 /// </summary>
		 /// <param name="version"> The version.
		 /// </param>
		 /// <returns> A file for the specific version. </returns>
		 internal virtual File GetForVersion( long version )
		 {
			  return new File( _baseDirectory, BASE_FILE_NAME + version );
		 }

		 /// <summary>
		 /// Looks in the base directory for all suitable RAFT log files and returns a sorted map
		 /// with the version as key and File as value.
		 /// </summary>
		 /// <param name="fileSystem"> The filesystem. </param>
		 /// <param name="log"> The message log.
		 /// </param>
		 /// <returns> The sorted version to file map. </returns>
		 public virtual SortedDictionary<long, File> GetAllFiles( FileSystemAbstraction fileSystem, Log log )
		 {
			  SortedDictionary<long, File> versionFileMap = new SortedDictionary<long, File>();

			  foreach ( File file in fileSystem.ListFiles( _baseDirectory ) )
			  {
					Matcher matcher = _logFilePattern.matcher( file.Name );

					if ( !matcher.matches() )
					{
						 log.Warn( "Found out of place file: " + file.Name );
						 continue;
					}

					versionFileMap[Convert.ToInt64( matcher.group( 1 ) )] = file;
			  }

			  return versionFileMap;
		 }
	}

}