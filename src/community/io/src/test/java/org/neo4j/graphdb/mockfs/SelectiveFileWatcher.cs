/*
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
namespace Neo4Net.GraphDb.mockfs
{

	using FileWatchEventListener = Neo4Net.Io.fs.watcher.FileWatchEventListener;
	using FileWatcher = Neo4Net.Io.fs.watcher.FileWatcher;
	using WatchedResource = Neo4Net.Io.fs.watcher.resource.WatchedResource;

	/// <summary>
	/// File watcher that will perform watching activities using specific file watcher in case if
	/// requested resource will match to provided <seealso cref="File specificFile"/>.
	/// </summary>
	public class SelectiveFileWatcher : FileWatcher
	{
		 private File _specialFile;
		 private readonly FileWatcher _defaultFileWatcher;
		 private readonly FileWatcher _specificFileWatcher;

		 internal SelectiveFileWatcher( File specialFile, FileWatcher defaultFileWatcher, FileWatcher specificFileWatcher )
		 {
			  this._specialFile = specialFile;
			  this._defaultFileWatcher = defaultFileWatcher;
			  this._specificFileWatcher = specificFileWatcher;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.io.fs.watcher.resource.WatchedResource watch(java.io.File file) throws java.io.IOException
		 public override WatchedResource Watch( File file )
		 {
			  return ChooseFileWatcher( file ).watch( file );
		 }

		 public override void AddFileWatchEventListener( FileWatchEventListener listener )
		 {
			  _defaultFileWatcher.addFileWatchEventListener( listener );
			  _specificFileWatcher.addFileWatchEventListener( listener );
		 }

		 public override void RemoveFileWatchEventListener( FileWatchEventListener listener )
		 {
			  _defaultFileWatcher.removeFileWatchEventListener( listener );
			  _specificFileWatcher.removeFileWatchEventListener( listener );
		 }

		 public override void StopWatching()
		 {
			  _defaultFileWatcher.stopWatching();
			  _specificFileWatcher.stopWatching();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startWatching() throws InterruptedException
		 public override void StartWatching()
		 {
			  _defaultFileWatcher.startWatching();
			  _specificFileWatcher.startWatching();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _defaultFileWatcher.Dispose();
			  _specificFileWatcher.Dispose();
		 }

		 private FileWatcher ChooseFileWatcher( File file )
		 {
			  return file.Equals( _specialFile ) ? _specificFileWatcher : _defaultFileWatcher;
		 }
	}

}