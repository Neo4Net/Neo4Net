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
namespace Org.Neo4j.Io.fs.watcher
{

	using WatchedResource = Org.Neo4j.Io.fs.watcher.resource.WatchedResource;

	/// <summary>
	/// Watcher that allows receive notification about files modifications/removal for particular underlying file system.
	/// 
	/// To be able to get notification users need to register resource they are interested in using
	/// <seealso cref="watch(File)"/> method call and add by adding <seealso cref="FileWatchEventListener listener"/> to be able to receive
	/// status updates.
	/// </summary>
	/// <seealso cref= WatchService </seealso>
	public interface FileWatcher : System.IDisposable
	{

		 /// <summary>
		 /// Register provided directory in list of resources that we would like to watch and receive status modification
		 /// updates </summary>
		 /// <param name="file"> directory to be monitored for updates </param>
		 /// <returns> closable resource that represent watched file </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.io.fs.watcher.resource.WatchedResource watch(java.io.File file) throws java.io.IOException;
		 WatchedResource Watch( File file );

		 /// <summary>
		 /// Register listener to receive updates about registered resources. </summary>
		 /// <param name="listener"> listener to register </param>
		 void AddFileWatchEventListener( FileWatchEventListener listener );

		 /// <summary>
		 /// Remove listener from a list of updates receivers. </summary>
		 /// <param name="listener"> listener to remove </param>
		 void RemoveFileWatchEventListener( FileWatchEventListener listener );

		 /// <summary>
		 /// Stop monitoring of registered directories
		 /// </summary>
		 void StopWatching();

		 /// <summary>
		 /// Start monitoring of registered directories.
		 /// This method we will wait for notification about registered resources, meaning that it will block thread where
		 /// it was called. If it is desired to start file watching as background task - watcher should be started in
		 /// separate thread.
		 /// Watching can be stopped by calling <seealso cref="stopWatching()"/>. </summary>
		 /// <exception cref="InterruptedException"> when interrupted while waiting for update notification to come </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void startWatching() throws InterruptedException;
		 void StartWatching();
	}

	public static class FileWatcher_Fields
	{
		 public static readonly FileWatcher SilentWatcher = new SilentFileWatcher();
	}

}