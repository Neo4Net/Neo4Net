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
namespace Neo4Net.Adversaries.watcher
{

	using FileWatchEventListener = Neo4Net.Io.fs.watcher.FileWatchEventListener;
	using FileWatcher = Neo4Net.Io.fs.watcher.FileWatcher;
	using WatchedResource = Neo4Net.Io.fs.watcher.resource.WatchedResource;

	/// <summary>
	/// File watcher that injects additional failures using provided <seealso cref="Adversary"/>
	/// and delegate all actual watching role to provided <seealso cref="FileWatcher"/>
	/// </summary>
	public class AdversarialFileWatcher : FileWatcher
	{
		 private readonly FileWatcher _fileWatcher;
		 private readonly Adversary _adversary;

		 public AdversarialFileWatcher( FileWatcher fileWatcher, Adversary adversary )
		 {
			  this._fileWatcher = fileWatcher;
			  this._adversary = adversary;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _fileWatcher.Dispose();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.watcher.resource.WatchedResource watch(java.io.File file) throws java.io.IOException
		 public override WatchedResource Watch( File file )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return _fileWatcher.watch( file );
		 }

		 public override void AddFileWatchEventListener( FileWatchEventListener listener )
		 {
			  _fileWatcher.addFileWatchEventListener( listener );
		 }

		 public override void RemoveFileWatchEventListener( FileWatchEventListener listener )
		 {
			  _fileWatcher.removeFileWatchEventListener( listener );
		 }

		 public override void StopWatching()
		 {
			  _fileWatcher.stopWatching();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startWatching() throws InterruptedException
		 public override void StartWatching()
		 {
			  _adversary.injectFailure( typeof( InterruptedException ) );
			  _fileWatcher.startWatching();
		 }
	}

}