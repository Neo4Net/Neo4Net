using System.Collections.Concurrent;
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
namespace Neo4Net.Io.fs.watcher
{

	using WatchedResource = Neo4Net.Io.fs.watcher.resource.WatchedResource;

	/// <summary>
	/// File system delegate that will remember all the files that it was asked to watch
	/// and will register them in real delegate during <seealso cref="startWatching()"/> call.
	/// When delegate will be stopped all registered resources will be closed and delegate delegate will be stopped.
	/// 
	/// Described pattern allows to perform repeatable startWatching/stopWatching cycle for pre-configured set of files.
	/// </summary>
	public class RestartableFileSystemWatcher : FileWatcher
	{
		 private FileWatcher @delegate;
		 private ISet<File> _filesToWatch = Collections.newSetFromMap( new ConcurrentDictionary<File>() );
		 private ISet<WatchedResource> _watchedResources = Collections.newSetFromMap( new ConcurrentDictionary<WatchedResource>() );

		 public RestartableFileSystemWatcher( FileWatcher @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override WatchedResource Watch( File file )
		 {
			  _filesToWatch.Add( file );
			  return Neo4Net.Io.fs.watcher.resource.WatchedResource_Fields.Empty;
		 }

		 public override void AddFileWatchEventListener( FileWatchEventListener listener )
		 {
			  @delegate.AddFileWatchEventListener( listener );
		 }

		 public override void RemoveFileWatchEventListener( FileWatchEventListener listener )
		 {
			  @delegate.RemoveFileWatchEventListener( listener );
		 }

		 public override void StopWatching()
		 {
			  try
			  {
					IOUtils.closeAll( _watchedResources );
					_watchedResources.Clear();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  finally
			  {
					@delegate.StopWatching();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startWatching() throws InterruptedException
		 public override void StartWatching()
		 {
			  foreach ( File fileToWatch in _filesToWatch )
			  {
					WatchFile( fileToWatch );
			  }
			  @delegate.StartWatching();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  @delegate.Dispose();
		 }

		 private void WatchFile( File fileToWatch )
		 {
			  try
			  {
					_watchedResources.Add( @delegate.Watch( fileToWatch ) );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}