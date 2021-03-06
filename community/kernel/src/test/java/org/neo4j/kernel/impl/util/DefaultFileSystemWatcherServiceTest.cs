﻿/*
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
namespace Org.Neo4j.Kernel.impl.util
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;

	using FileWatcher = Org.Neo4j.Io.fs.watcher.FileWatcher;
	using SilentFileWatcher = Org.Neo4j.Io.fs.watcher.SilentFileWatcher;
	using DefaultFileSystemWatcherService = Org.Neo4j.Kernel.impl.util.watcher.DefaultFileSystemWatcherService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	public class DefaultFileSystemWatcherServiceTest
	{

		 private static JobScheduler _jobScheduler;
		 private readonly FileWatcher _fileWatcher = Mockito.mock( typeof( FileWatcher ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUp()
		 public static void SetUp()
		 {
			  _jobScheduler = createInitialisedScheduler();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void TearDown()
		 {
			  _jobScheduler.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startMonitoringWhenLifecycleStarting() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartMonitoringWhenLifecycleStarting()
		 {
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  FileWatcher watcher = new TestFileWatcher( latch );
			  DefaultFileSystemWatcherService service = new DefaultFileSystemWatcherService( _jobScheduler, watcher );
			  service.Init();
			  service.Start();

			  latch.await();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopMonitoringWhenLifecycleStops() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StopMonitoringWhenLifecycleStops()
		 {
			  DefaultFileSystemWatcherService service = new DefaultFileSystemWatcherService( _jobScheduler, _fileWatcher );
			  service.Init();
			  service.Start();
			  service.Stop();

			  verify( _fileWatcher ).stopWatching();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeFileWatcherOnShutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseFileWatcherOnShutdown()
		 {
			  DefaultFileSystemWatcherService service = new DefaultFileSystemWatcherService( _jobScheduler, _fileWatcher );
			  service.Init();
			  service.Start();
			  service.Stop();
			  service.Shutdown();

			  verify( _fileWatcher ).close();
		 }

		 private class TestFileWatcher : SilentFileWatcher
		 {

			  internal System.Threading.CountdownEvent Latch;

			  internal TestFileWatcher( System.Threading.CountdownEvent latch )
			  {
					this.Latch = latch;
			  }

			  public override void StartWatching()
			  {
					Latch.Signal();
			  }
		 }
	}

}