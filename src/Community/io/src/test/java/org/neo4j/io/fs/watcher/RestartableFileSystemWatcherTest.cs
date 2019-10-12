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
	using Test = org.junit.jupiter.api.Test;


	using WatchedResource = Neo4Net.Io.fs.watcher.resource.WatchedResource;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class RestartableFileSystemWatcherTest
	{
		private bool InstanceFieldsInitialized = false;

		public RestartableFileSystemWatcherTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_watcher = new RestartableFileSystemWatcher( @delegate );
		}


		 private FileWatcher @delegate = mock( typeof( FileWatcher ) );
		 private RestartableFileSystemWatcher _watcher;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void delegateListenersCallToRealWatcher()
		 internal virtual void DelegateListenersCallToRealWatcher()
		 {
			  FileWatchEventListener listener = mock( typeof( FileWatchEventListener ) );

			  _watcher.addFileWatchEventListener( listener );
			  verify( @delegate ).addFileWatchEventListener( listener );

			  _watcher.removeFileWatchEventListener( listener );
			  verify( @delegate ).removeFileWatchEventListener( listener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeDelegateOnClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseDelegateOnClose()
		 {
			  _watcher.Dispose();
			  verify( @delegate ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void startStopFileWatchingCycle() throws java.io.IOException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StartStopFileWatchingCycle()
		 {
			  File file1 = mock( typeof( File ) );
			  File file2 = mock( typeof( File ) );
			  WatchedResource resource1 = mock( typeof( WatchedResource ) );
			  WatchedResource resource2 = mock( typeof( WatchedResource ) );
			  _watcher.watch( file1 );
			  _watcher.watch( file2 );

			  when( @delegate.Watch( file1 ) ).thenReturn( resource1 );
			  when( @delegate.Watch( file2 ) ).thenReturn( resource2 );

			  int invocations = 100;
			  for ( int i = 0; i < invocations; i++ )
			  {
					StartStopWatching();
			  }

			  verify( @delegate, times( invocations ) ).watch( file1 );
			  verify( @delegate, times( invocations ) ).watch( file2 );
			  verify( @delegate, times( invocations ) ).startWatching();
			  verify( @delegate, times( invocations ) ).stopWatching();

			  verify( resource1, times( invocations ) ).close();
			  verify( resource2, times( invocations ) ).close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startStopWatching() throws InterruptedException
		 private void StartStopWatching()
		 {
			  _watcher.startWatching();
			  _watcher.stopWatching();
		 }
	}

}