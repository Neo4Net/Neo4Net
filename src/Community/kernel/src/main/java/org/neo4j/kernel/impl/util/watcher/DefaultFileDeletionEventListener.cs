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
namespace Neo4Net.Kernel.impl.util.watcher
{

	using FileWatchEventListener = Neo4Net.Io.fs.watcher.FileWatchEventListener;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;

	/// <summary>
	/// Listener that will print notification about deleted filename into internal log.
	/// </summary>
	public class DefaultFileDeletionEventListener : FileWatchEventListener
	{

		 private readonly Log _internalLog;
		 private readonly System.Predicate<string> _fileNameFilter;

		 public DefaultFileDeletionEventListener( LogService logService, System.Predicate<string> fileNameFilter )
		 {
			  this._internalLog = logService.GetInternalLog( this.GetType() );
			  this._fileNameFilter = fileNameFilter;
		 }

		 public override void FileDeleted( string fileName )
		 {
			  if ( !_fileNameFilter.test( fileName ) )
			  {
					_internalLog.error( format( "'%s' which belongs to the store was deleted while database was running.", fileName ) );
			  }
		 }
	}

}