using System.Diagnostics;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.util.watcher
{

	using FileWatcher = Neo4Net.Io.fs.watcher.FileWatcher;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// Factory used for construction of proper adaptor for available <seealso cref="FileWatcher"/>.
	/// In case if silent matcher is used dummy adapter will be used, otherwise will use default wrapper that will bind
	/// monitoring cycles to corresponding lifecycle phases.
	/// </summary>
	public class DefaultFileSystemWatcherService : FileSystemWatcherService
	{
		 private readonly IJobScheduler _jobScheduler;
		 private readonly FileWatcher _fileWatcher;
		 private readonly FileSystemEventWatcher _eventWatcher;
		 private ThreadFactory _fileWatchers;
		 private Thread _watcher;

		 public DefaultFileSystemWatcherService( IJobScheduler jobScheduler, FileWatcher fileWatcher )
		 {
			  this._jobScheduler = jobScheduler;
			  this._fileWatcher = fileWatcher;
			  this._eventWatcher = new FileSystemEventWatcher( this );
		 }

		 public override void Init()
		 {
			  _fileWatchers = _jobScheduler.threadFactory( Group.FILE_WATCHER );
		 }

		 public override void Start()
		 {
			 lock ( this )
			 {
				  Debug.Assert( _watcher == null );
				  _watcher = _fileWatchers.newThread( _eventWatcher );
				  _watcher.Start();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void stop() throws Throwable
		 public override void Stop()
		 {
			 lock ( this )
			 {
				  _eventWatcher.stopWatching();
				  if ( _watcher != null )
				  {
						_watcher.Interrupt();
						_watcher.Join();
						_watcher = null;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  _fileWatcher.Dispose();
		 }

		 public virtual FileWatcher FileWatcher
		 {
			 get
			 {
				  return _fileWatcher;
			 }
		 }

		 private class FileSystemEventWatcher : ThreadStart
		 {
			 private readonly DefaultFileSystemWatcherService _outerInstance;

			 public FileSystemEventWatcher( DefaultFileSystemWatcherService outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Run()
			  {
					try
					{
						 outerInstance.fileWatcher.StartWatching();
					}
					catch ( InterruptedException )
					{
					}
			  }

			  internal virtual void StopWatching()
			  {
					outerInstance.fileWatcher.StopWatching();
			  }
		 }
	}

}