using System.Collections.Generic;

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
namespace Neo4Net.Io.fs.watcher
{
	using SensitivityWatchEventModifier = com.sun.nio.file.SensitivityWatchEventModifier;
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using WatchedFile = Neo4Net.Io.fs.watcher.resource.WatchedFile;
	using WatchedResource = Neo4Net.Io.fs.watcher.resource.WatchedResource;

	/// <summary>
	/// File watcher that monitors registered directories state using possibilities provided by <seealso cref="WatchService"/>.
	/// 
	/// Safe to be used from multiple threads
	/// </summary>
	public class DefaultFileSystemWatcher : FileWatcher
	{
		 private static readonly WatchEvent.Kind[] _observedEvents = new WatchEvent.Kind[]{ StandardWatchEventKinds.ENTRY_DELETE, StandardWatchEventKinds.ENTRY_MODIFY };
		 private readonly WatchService _watchService;
		 private readonly IList<FileWatchEventListener> _listeners = new CopyOnWriteArrayList<FileWatchEventListener>();
		 private volatile bool _watch;

		 public DefaultFileSystemWatcher( WatchService watchService )
		 {
			  this._watchService = watchService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.io.fs.watcher.resource.WatchedResource watch(java.io.File file) throws java.io.IOException
		 public override WatchedResource Watch( File file )
		 {
			  if ( !file.Directory )
			  {
					throw new System.ArgumentException( format( "File `%s` is not a directory. Only directories can be " + "registered to be monitored.", file.CanonicalPath ) );
			  }
			  WatchKey watchKey = file.toPath().register(_watchService, _observedEvents, SensitivityWatchEventModifier.HIGH);
			  return new WatchedFile( watchKey );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startWatching() throws InterruptedException
		 public override void StartWatching()
		 {
			  _watch = true;
			  while ( _watch )
			  {
					WatchKey key = _watchService.take();
					if ( key != null )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.nio.file.WatchEvent<?>> watchEvents = key.pollEvents();
						 IList<WatchEvent<object>> watchEvents = key.pollEvents();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.nio.file.WatchEvent<?> watchEvent : watchEvents)
						 foreach ( WatchEvent<object> watchEvent in watchEvents )
						 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.nio.file.WatchEvent.Kind<?> kind = watchEvent.kind();
							  WatchEvent.Kind<object> kind = watchEvent.kind();
							  if ( StandardWatchEventKinds.ENTRY_MODIFY == kind )
							  {
									NotifyAboutModification( watchEvent );
							  }
							  if ( StandardWatchEventKinds.ENTRY_DELETE == kind )
							  {
									NotifyAboutDeletion( watchEvent );
							  }
						 }
						 key.reset();
					}
			  }
		 }

		 public override void StopWatching()
		 {
			  _watch = false;
		 }

		 public override void AddFileWatchEventListener( FileWatchEventListener listener )
		 {
			  _listeners.Add( listener );
		 }

		 public override void RemoveFileWatchEventListener( FileWatchEventListener listener )
		 {
			  _listeners.Remove( listener );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  StopWatching();
			  _watchService.close();
		 }

		 private void NotifyAboutModification<T1>( WatchEvent<T1> watchEvent )
		 {
			  string context = GetContext( watchEvent );
			  if ( StringUtils.isNotEmpty( context ) )
			  {
					foreach ( FileWatchEventListener listener in _listeners )
					{
						 listener.FileModified( context );
					}
			  }
		 }

		 private void NotifyAboutDeletion<T1>( WatchEvent<T1> watchEvent )
		 {
			  string context = GetContext( watchEvent );
			  if ( StringUtils.isNotEmpty( context ) )
			  {
					foreach ( FileWatchEventListener listener in _listeners )
					{
						 listener.FileDeleted( context );
					}
			  }
		 }

		 private static string GetContext<T1>( WatchEvent<T1> watchEvent )
		 {
			  return Objects.ToString( watchEvent.context(), StringUtils.EMPTY );
		 }
	}

}