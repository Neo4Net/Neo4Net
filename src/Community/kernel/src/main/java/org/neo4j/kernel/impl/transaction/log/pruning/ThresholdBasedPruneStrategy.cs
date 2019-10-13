﻿/*
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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using TransactionLogFileInformation = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFileInformation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogVersionRepository_Fields.INITIAL_LOG_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

	public class ThresholdBasedPruneStrategy : LogPruneStrategy
	{
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly LogFiles _files;
		 private readonly Threshold _threshold;
		 private readonly TransactionLogFileInformation _logFileInformation;

		 internal ThresholdBasedPruneStrategy( FileSystemAbstraction fileSystem, LogFiles logFiles, Threshold threshold )
		 {
			  this._fileSystem = fileSystem;
			  this._files = logFiles;
			  this._logFileInformation = _files.LogFileInformation;
			  this._threshold = threshold;
		 }

		 public override LongStream FindLogVersionsToDelete( long upToVersion )
		 {
			 lock ( this )
			 {
				  if ( upToVersion == INITIAL_LOG_VERSION )
				  {
						return LongStream.empty();
				  }
      
				  _threshold.init();
				  long upper = upToVersion - 1;
				  bool exceeded = false;
				  while ( upper >= 0 )
				  {
						File file = _files.getLogFileForVersion( upper );
						if ( !_fileSystem.fileExists( file ) )
						{
							 // There aren't logs to prune anything. Just return
							 return LongStream.empty();
						}
      
						if ( _fileSystem.getFileSize( file ) > LOG_HEADER_SIZE && _threshold.reached( file, upper, _logFileInformation ) )
						{
							 exceeded = true;
							 break;
						}
						upper--;
				  }
      
				  if ( !exceeded )
				  {
						return LongStream.empty();
				  }
      
				  // Find out which log is the earliest existing (lower bound to prune)
				  long lower = upper;
				  while ( _fileSystem.fileExists( _files.getLogFileForVersion( lower - 1 ) ) )
				  {
						lower--;
				  }
      
				  /*
				   * Here we make sure that at least one historical log remains behind, in addition of course to the
				   * current one. This is in order to make sure that at least one transaction remains always available for
				   * serving to whomever asks for it.
				   * To illustrate, imagine that there is a threshold in place configured so that it enforces pruning of the
				   * log file that was just rotated out (for example, a file size threshold that is misconfigured to be smaller
				   * than the smallest log). In such a case, until the database commits a transaction there will be no
				   * transactions present on disk, making impossible to serve any to whichever client might ask, leading to
				   * errors on both sides of the conversation.
				   * This if statement does nothing more complicated than checking if the next-to-last log would be pruned
				   * and simply skipping it if so.
				   */
				  if ( upper == upToVersion - 1 )
				  {
						upper--;
				  }
      
				  // The reason we delete from lower to upper is that if it crashes in the middle we can be sure that no holes
				  // are created.
				  // We create a closed range because we want to include the 'upper' log version as well. The check above ensures
				  // we don't accidentally leave the database without any logs.
				  return LongStream.rangeClosed( lower, upper );
			 }
		 }
	}

}