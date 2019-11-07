using System;
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
namespace Neo4Net.Store.Watch
{
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using LuceneDataSource = Neo4Net.Index.impl.lucene.@explicit.LuceneDataSource;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using FileWatchEventListener = Neo4Net.Io.fs.watcher.FileWatchEventListener;
	using FileWatcher = Neo4Net.Io.fs.watcher.FileWatcher;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using DefaultFileDeletionEventListener = Neo4Net.Kernel.impl.util.watcher.DefaultFileDeletionEventListener;
	using FileSystemWatcherService = Neo4Net.Kernel.impl.util.watcher.FileSystemWatcherService;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeFalse;

	public class FileWatchIT
	{
		 private const long TEST_TIMEOUT = 600_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private File _storeDir;
		 private AssertableLogProvider _logProvider;
		 private IGraphDatabaseService _database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _storeDir = TestDirectory.storeDir();
			  _logProvider = new AssertableLogProvider();
			  _database = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(_logProvider).newEmbeddedDatabase(_storeDir);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  ShutdownDatabaseSilently( _database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void notifyAboutStoreFileDeletion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NotifyAboutStoreFileDeletion()
		 {
			  assumeFalse( SystemUtils.IS_OS_WINDOWS );

			  string fileName = TestDirectory.databaseLayout().metadataStore().Name;
			  FileWatcher fileWatcher = GetFileWatcher( _database );
			  CheckPointer checkpointer = GetCheckpointer( _database );
			  DeletionLatchEventListener deletionListener = new DeletionLatchEventListener( fileName );
			  fileWatcher.AddFileWatchEventListener( deletionListener );

			  do
			  {
					CreateNode( _database );
					ForceCheckpoint( checkpointer );
			  } while ( !deletionListener.AwaitModificationNotification() );

			  DeleteFile( TestDirectory.storeDir(), fileName );
			  deletionListener.AwaitDeletionNotification();

			  _logProvider.rawMessageMatcher().assertContains("'" + fileName + "' which belongs to the store was deleted while database was running.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void notifyWhenFileWatchingFailToStart()
		 public virtual void NotifyWhenFileWatchingFailToStart()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider( true );
			  IGraphDatabaseService db = null;
			  try
			  {
					db = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(logProvider).setFileSystem(new NonWatchableFileSystemAbstraction()).newEmbeddedDatabase(TestDirectory.storeDir("failed-start-db"));

					logProvider.RawMessageMatcher().assertContains("Can not create file watcher for current file system. " + "File monitoring capabilities for store files will be disabled.");
			  }
			  finally
			  {
					ShutdownDatabaseSilently( db );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void notifyAboutExplicitIndexFolderRemoval() throws InterruptedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NotifyAboutExplicitIndexFolderRemoval()
		 {
			  string monitoredDirectory = GetExplicitIndexDirectory( TestDirectory.databaseLayout() );

			  FileWatcher fileWatcher = GetFileWatcher( _database );
			  CheckPointer checkPointer = GetCheckpointer( _database );
			  DeletionLatchEventListener deletionListener = new DeletionLatchEventListener( monitoredDirectory );
			  string metadataStore = TestDirectory.databaseLayout().metadataStore().Name;
			  ModificationEventListener modificationEventListener = new ModificationEventListener( metadataStore );
			  fileWatcher.AddFileWatchEventListener( deletionListener );
			  fileWatcher.AddFileWatchEventListener( modificationEventListener );

			  do
			  {
					CreateNode( _database );
					ForceCheckpoint( checkPointer );
			  } while ( !modificationEventListener.AwaitModificationNotification() );

			  DeleteStoreDirectory( _storeDir, monitoredDirectory );
			  deletionListener.AwaitDeletionNotification();

			  _logProvider.rawMessageMatcher().assertContains("'" + monitoredDirectory + "' which belongs to the store was deleted while database was running.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void doNotNotifyAboutLuceneIndexFilesDeletion() throws InterruptedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotNotifyAboutLuceneIndexFilesDeletion()
		 {
			  DependencyResolver dependencyResolver = ( ( GraphDatabaseAPI ) _database ).DependencyResolver;
			  FileWatcher fileWatcher = GetFileWatcher( _database );
			  CheckPointer checkPointer = dependencyResolver.ResolveDependency( typeof( CheckPointer ) );

			  string propertyStoreName = TestDirectory.databaseLayout().propertyStore().Name;
			  AccumulativeDeletionEventListener accumulativeListener = new AccumulativeDeletionEventListener();
			  ModificationEventListener modificationListener = new ModificationEventListener( propertyStoreName );
			  fileWatcher.AddFileWatchEventListener( modificationListener );
			  fileWatcher.AddFileWatchEventListener( accumulativeListener );

			  string labelName = "labelName";
			  string propertyName = "propertyName";
			  Label testLabel = Label.label( labelName );
			  CreateIndexes( _database, propertyName, testLabel );
			  do
			  {
					CreateNode( _database, propertyName, testLabel );
					ForceCheckpoint( checkPointer );
			  } while ( !modificationListener.AwaitModificationNotification() );

			  fileWatcher.RemoveFileWatchEventListener( modificationListener );
			  ModificationEventListener afterRemovalListener = new ModificationEventListener( propertyStoreName );
			  fileWatcher.AddFileWatchEventListener( afterRemovalListener );

			  DropAllIndexes( _database );
			  do
			  {
					CreateNode( _database, propertyName, testLabel );
					ForceCheckpoint( checkPointer );
			  } while ( !afterRemovalListener.AwaitModificationNotification() );

			  accumulativeListener.AssertDoesNotHaveAnyDeletions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void doNotMonitorTransactionLogFiles() throws InterruptedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotMonitorTransactionLogFiles()
		 {
			  assumeFalse( SystemUtils.IS_OS_WINDOWS );

			  FileWatcher fileWatcher = GetFileWatcher( _database );
			  CheckPointer checkpointer = GetCheckpointer( _database );
			  string metadataStore = TestDirectory.databaseLayout().metadataStore().Name;
			  ModificationEventListener modificationEventListener = new ModificationEventListener( metadataStore );
			  fileWatcher.AddFileWatchEventListener( modificationEventListener );

			  do
			  {
					CreateNode( _database );
					ForceCheckpoint( checkpointer );
			  } while ( !modificationEventListener.AwaitModificationNotification() );

			  string fileName = TransactionLogFiles.DEFAULT_NAME + ".0";
			  DeletionLatchEventListener deletionListener = new DeletionLatchEventListener( fileName );
			  fileWatcher.AddFileWatchEventListener( deletionListener );
			  DeleteFile( TestDirectory.storeDir(), fileName );
			  deletionListener.AwaitDeletionNotification();

			  AssertableLogProvider.LogMatcher logMatcher = AssertableLogProvider.inLog( typeof( DefaultFileDeletionEventListener ) ).info( containsString( fileName ) );
			  _logProvider.assertNone( logMatcher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void notifyWhenWholeStoreDirectoryRemoved() throws java.io.IOException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NotifyWhenWholeStoreDirectoryRemoved()
		 {
			  assumeFalse( SystemUtils.IS_OS_WINDOWS );

			  string fileName = TestDirectory.databaseLayout().metadataStore().Name;
			  FileWatcher fileWatcher = GetFileWatcher( _database );
			  CheckPointer checkpointer = GetCheckpointer( _database );

			  ModificationEventListener modificationListener = new ModificationEventListener( fileName );
			  fileWatcher.AddFileWatchEventListener( modificationListener );
			  do
			  {
					CreateNode( _database );
					ForceCheckpoint( checkpointer );
			  } while ( !modificationListener.AwaitModificationNotification() );
			  fileWatcher.RemoveFileWatchEventListener( modificationListener );

			  string storeDirectoryName = TestDirectory.databaseLayout().databaseDirectory().Name;
			  DeletionLatchEventListener eventListener = new DeletionLatchEventListener( storeDirectoryName );
			  fileWatcher.AddFileWatchEventListener( eventListener );
			  FileUtils.deleteRecursively( TestDirectory.databaseLayout().databaseDirectory() );

			  eventListener.AwaitDeletionNotification();

			  _logProvider.rawMessageMatcher().assertContains("'" + storeDirectoryName + "' which belongs to the store was deleted while database was running.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TEST_TIMEOUT) public void shouldLogWhenDisabled()
		 public virtual void ShouldLogWhenDisabled()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider( true );
			  IGraphDatabaseService db = null;
			  try
			  {
					db = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(logProvider).setFileSystem(new NonWatchableFileSystemAbstraction()).newEmbeddedDatabaseBuilder(TestDirectory.directory("failed-start-db")).setConfig(GraphDatabaseSettings.filewatcher_enabled, Settings.FALSE).newGraphDatabase();

					logProvider.RawMessageMatcher().assertContains("File watcher disabled by configuration.");
			  }
			  finally
			  {
					ShutdownDatabaseSilently( db );
			  }
		 }

		 private static void ShutdownDatabaseSilently( IGraphDatabaseService databaseService )
		 {
			  if ( databaseService != null )
			  {
					try
					{
						 databaseService.Shutdown();
					}
					catch ( Exception )
					{
						 // ignored
					}
			  }
		 }

		 private static void DropAllIndexes( IGraphDatabaseService database )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					foreach ( IndexDefinition definition in database.Schema().Indexes )
					{
						 definition.Drop();
					}
					transaction.Success();
			  }
		 }

		 private static void CreateIndexes( IGraphDatabaseService database, string propertyName, Label testLabel )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					database.Schema().indexFor(testLabel).on(propertyName).create();
					transaction.Success();
			  }

			  using ( Transaction ignored = database.BeginTx() )
			  {
					database.Schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void forceCheckpoint(Neo4Net.kernel.impl.transaction.log.checkpoint.CheckPointer checkPointer) throws java.io.IOException
		 private static void ForceCheckpoint( CheckPointer checkPointer )
		 {
			  checkPointer.ForceCheckPoint( new SimpleTriggerInfo( "testForceCheckPoint" ) );
		 }

		 private static string GetExplicitIndexDirectory( DatabaseLayout databaseLayout )
		 {
			  File schemaIndexDirectory = LuceneDataSource.getLuceneIndexStoreDirectory( databaseLayout );
			  Path relativeIndexPath = databaseLayout.DatabaseDirectory().toPath().relativize(schemaIndexDirectory.toPath());
			  return relativeIndexPath.getName( 0 ).ToString();
		 }

		 private static void CreateNode( IGraphDatabaseService database, string propertyName, Label testLabel )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( testLabel );
					node.SetProperty( propertyName, "value" );
					transaction.Success();
			  }
		 }

		 private static CheckPointer GetCheckpointer( IGraphDatabaseService database )
		 {
			  return ( ( GraphDatabaseAPI ) database ).DependencyResolver.resolveDependency( typeof( CheckPointer ) );
		 }

		 private static FileWatcher GetFileWatcher( IGraphDatabaseService database )
		 {
			  DependencyResolver dependencyResolver = ( ( GraphDatabaseAPI ) database ).DependencyResolver;
			  return dependencyResolver.ResolveDependency( typeof( FileSystemWatcherService ) ).FileWatcher;
		 }

		 private static void DeleteFile( File storeDir, string fileName )
		 {
			  File metadataStore = new File( storeDir, fileName );
			  FileUtils.deleteFile( metadataStore );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void deleteStoreDirectory(java.io.File storeDir, String directoryName) throws java.io.IOException
		 private static void DeleteStoreDirectory( File storeDir, string directoryName )
		 {
			  File directory = new File( storeDir, directoryName );
			  FileUtils.deleteRecursively( directory );
		 }

		 private static void CreateNode( IGraphDatabaseService database )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					database.CreateNode();
					transaction.Success();
			  }
		 }

		 private class NonWatchableFileSystemAbstraction : DefaultFileSystemAbstraction
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.io.fs.watcher.FileWatcher fileWatcher() throws java.io.IOException
			  public override FileWatcher FileWatcher()
			  {
					throw new IOException( "You can't watch me!" );
			  }
		 }

		 private class AccumulativeDeletionEventListener : FileWatchEventListener
		 {
			  internal readonly IList<string> DeletedFiles = new List<string>();

			  public override void FileDeleted( string fileName )
			  {
					DeletedFiles.Add( fileName );
			  }

			  internal virtual void AssertDoesNotHaveAnyDeletions()
			  {
					assertThat( "Should not have any deletions registered", DeletedFiles, Matchers.empty() );
			  }
		 }

		 private class ModificationEventListener : FileWatchEventListener
		 {
			  internal readonly string ExpectedFileName;
			  internal readonly System.Threading.CountdownEvent ModificationLatch = new System.Threading.CountdownEvent( 1 );

			  internal ModificationEventListener( string expectedFileName )
			  {
					this.ExpectedFileName = expectedFileName;
			  }

			  public override void FileModified( string fileName )
			  {
					if ( ExpectedFileName.Equals( fileName ) )
					{
						 ModificationLatch.Signal();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean awaitModificationNotification() throws InterruptedException
			  internal virtual bool AwaitModificationNotification()
			  {
					return ModificationLatch.await( 1, TimeUnit.SECONDS );
			  }
		 }

		 private class DeletionLatchEventListener : ModificationEventListener
		 {
			  internal readonly System.Threading.CountdownEvent DeletionLatch = new System.Threading.CountdownEvent( 1 );

			  internal DeletionLatchEventListener( string expectedFileName ) : base( expectedFileName )
			  {
			  }

			  public override void FileDeleted( string fileName )
			  {
					if ( fileName.EndsWith( ExpectedFileName, StringComparison.Ordinal ) )
					{
						 DeletionLatch.Signal();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void awaitDeletionNotification() throws InterruptedException
			  internal virtual void AwaitDeletionNotification()
			  {
					DeletionLatch.await();
			  }

		 }
	}

}