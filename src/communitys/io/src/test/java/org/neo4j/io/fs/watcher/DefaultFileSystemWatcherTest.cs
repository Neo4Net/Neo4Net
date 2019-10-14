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
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using WatchedResource = Neo4Net.Io.fs.watcher.resource.WatchedResource;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class DefaultFileSystemWatcherTest
	internal class DefaultFileSystemWatcherTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory testDirectory;
		 internal TestDirectory TestDirectory;
		 private WatchService _watchServiceMock = mock( typeof( WatchService ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileWatchRegistrationIsIllegal()
		 internal virtual void FileWatchRegistrationIsIllegal()
		 {
			  DefaultFileSystemWatcher watcher = CreateWatcher();

			  System.ArgumentException exception = assertThrows( typeof( System.ArgumentException ), () => watcher.Watch(new File("notADirectory")) );
			  assertThat( exception.Message, containsString( "Only directories can be registered to be monitored." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void registerMultipleDirectoriesForMonitoring() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RegisterMultipleDirectoriesForMonitoring()
		 {
			  using ( DefaultFileSystemWatcher watcher = new DefaultFileSystemWatcher( FileSystems.Default.newWatchService() ) )
			  {
					File directory1 = TestDirectory.directory( "test1" );
					File directory2 = TestDirectory.directory( "test2" );
					WatchedResource watchedResource1 = watcher.Watch( directory1 );
					WatchedResource watchedResource2 = watcher.Watch( directory2 );
					assertNotSame( watchedResource1, watchedResource2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notifyListenersOnDeletion() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NotifyListenersOnDeletion()
		 {
			  TestFileSystemWatcher watcher = CreateWatcher();
			  AssertableFileEventListener listener1 = new AssertableFileEventListener();
			  AssertableFileEventListener listener2 = new AssertableFileEventListener();

			  watcher.AddFileWatchEventListener( listener1 );
			  watcher.AddFileWatchEventListener( listener2 );

			  TestWatchEvent<Path> watchEvent = new TestWatchEvent<Path>( ENTRY_DELETE, Paths.get( "file1" ) );
			  TestWatchEvent<Path> watchEvent2 = new TestWatchEvent<Path>( ENTRY_DELETE, Paths.get( "file2" ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: TestWatchKey watchKey = new TestWatchKey(asList(watchEvent, watchEvent2));
			  TestWatchKey watchKey = new TestWatchKey( new IList<WatchEvent<object>> { watchEvent, watchEvent2 } );

			  PrepareWatcher( watchKey );

			  Watch( watcher );

			  listener1.AssertDeleted( "file1" );
			  listener1.AssertDeleted( "file2" );
			  listener2.AssertDeleted( "file1" );
			  listener2.AssertDeleted( "file2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notifyListenersOnModification() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NotifyListenersOnModification()
		 {
			  TestFileSystemWatcher watcher = CreateWatcher();
			  AssertableFileEventListener listener1 = new AssertableFileEventListener();
			  AssertableFileEventListener listener2 = new AssertableFileEventListener();

			  watcher.AddFileWatchEventListener( listener1 );
			  watcher.AddFileWatchEventListener( listener2 );

			  TestWatchEvent<Path> watchEvent = new TestWatchEvent<Path>( ENTRY_MODIFY, Paths.get( "a" ) );
			  TestWatchEvent<Path> watchEvent2 = new TestWatchEvent<Path>( ENTRY_MODIFY, Paths.get( "b" ) );
			  TestWatchEvent<Path> watchEvent3 = new TestWatchEvent<Path>( ENTRY_MODIFY, Paths.get( "c" ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: TestWatchKey watchKey = new TestWatchKey(asList(watchEvent, watchEvent2, watchEvent3));
			  TestWatchKey watchKey = new TestWatchKey( new IList<WatchEvent<object>> { watchEvent, watchEvent2, watchEvent3 } );

			  PrepareWatcher( watchKey );

			  Watch( watcher );

			  listener1.AssertModified( "a" );
			  listener1.AssertModified( "b" );
			  listener1.AssertModified( "c" );
			  listener2.AssertModified( "a" );
			  listener2.AssertModified( "b" );
			  listener2.AssertModified( "c" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stopWatchingAndCloseEverythingOnClosed() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StopWatchingAndCloseEverythingOnClosed()
		 {
			  TestFileSystemWatcher watcher = CreateWatcher();
			  watcher.Dispose();

			  verify( _watchServiceMock ).close();
			  assertTrue( watcher.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void skipEmptyEvent() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SkipEmptyEvent()
		 {
			  TestFileSystemWatcher watcher = CreateWatcher();

			  AssertableFileEventListener listener = new AssertableFileEventListener();
			  watcher.AddFileWatchEventListener( listener );

			  TestWatchEvent<string> @event = new TestWatchEvent( ENTRY_MODIFY, null );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: TestWatchKey watchKey = new TestWatchKey(asList(event));
			  TestWatchKey watchKey = new TestWatchKey( new IList<WatchEvent<object>> { @event } );

			  PrepareWatcher( watchKey );

			  Watch( watcher );

			  listener.AssertNoEvents();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareWatcher(TestWatchKey watchKey) throws InterruptedException
		 private void PrepareWatcher( TestWatchKey watchKey )
		 {
			  when( _watchServiceMock.take() ).thenReturn(watchKey).thenThrow(typeof(InterruptedException));
		 }

		 private void Watch( TestFileSystemWatcher watcher )
		 {
			  try
			  {
					watcher.StartWatching();
			  }
			  catch ( InterruptedException )
			  {
					// expected
			  }
		 }

		 private TestFileSystemWatcher CreateWatcher()
		 {
			  return new TestFileSystemWatcher( _watchServiceMock );
		 }

		 private class TestFileSystemWatcher : DefaultFileSystemWatcher
		 {

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ClosedConflict;

			  internal TestFileSystemWatcher( WatchService watchService ) : base( watchService )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			  public override void Close()
			  {
					base.Dispose();
					ClosedConflict = true;
			  }

			  public virtual bool Closed
			  {
				  get
				  {
						return ClosedConflict;
				  }
			  }
		 }

		 private class TestWatchKey : WatchKey
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<java.nio.file.WatchEvent<?>> events;
			  internal IList<WatchEvent<object>> Events;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CanceledConflict;

			  internal TestWatchKey<T1>( IList<T1> events )
			  {
					this.Events = events;
			  }

			  public override bool Valid
			  {
				  get
				  {
						return false;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.List<java.nio.file.WatchEvent<?>> pollEvents()
			  public override IList<WatchEvent<object>> PollEvents()
			  {
					return Events;
			  }

			  public override bool Reset()
			  {
					return false;
			  }

			  public override void Cancel()
			  {
					CanceledConflict = true;
			  }

			  public override Watchable Watchable()
			  {
					return null;
			  }

			  public virtual bool Canceled
			  {
				  get
				  {
						return CanceledConflict;
				  }
			  }
		 }

		 private class TestWatchEvent<T> : WatchEvent
		 {

			  internal Kind<T> EventKind;
			  internal T FileName;

			  internal TestWatchEvent( Kind<T> eventKind, T fileName )
			  {
					this.EventKind = eventKind;
					this.FileName = fileName;
			  }

			  public override Kind Kind()
			  {
					return EventKind;
			  }

			  public override int Count()
			  {
					return 0;
			  }

			  public override T Context()
			  {
					return FileName;
			  }
		 }

		 private class AssertableFileEventListener : FileWatchEventListener
		 {
			  internal readonly IList<string> DeletedFileNames = new List<string>();
			  internal readonly IList<string> ModifiedFileNames = new List<string>();

			  public override void FileDeleted( string fileName )
			  {
					DeletedFileNames.Add( fileName );
			  }

			  public override void FileModified( string fileName )
			  {
					ModifiedFileNames.Add( fileName );
			  }

			  internal virtual void AssertNoEvents()
			  {
					assertThat( "Should not have any deletion events", DeletedFileNames, empty() );
					assertThat( "Should not have any modification events", ModifiedFileNames, empty() );
			  }

			  internal virtual void AssertDeleted( string fileName )
			  {
					assertThat( "Was expected to find notification about deletion.", DeletedFileNames, hasItem( fileName ) );
			  }

			  internal virtual void AssertModified( string fileName )
			  {
					assertThat( "Was expected to find notification about modification.", ModifiedFileNames, hasItem( fileName ) );
			  }
		 }
	}

}