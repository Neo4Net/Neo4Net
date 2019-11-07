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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Resource = Neo4Net.GraphDb.Resource;
	using Neo4Net.GraphDb;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using LabelScanStore = Neo4Net.Kernel.Api.LabelScan.LabelScanStore;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using StoreFileMetadata = Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asResourceIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.IndexConfigStore.INDEX_DB_FILE_NAME;

	public class NeoStoreFileListingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.EmbeddedDatabaseRule db = new Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public readonly EmbeddedDatabaseRule Db = new EmbeddedDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private NeoStoreDataSource _neoStoreDataSource;
		 private static readonly string[] _standardStoreDirFiles = new string[]{ "index", "lock", "debug.log", "neostore", "neostore.id", "neostore.counts.db.a", "neostore.counts.db.b", "neostore.labelscanstore.db", "neostore.labeltokenstore.db", "neostore.labeltokenstore.db.id", "neostore.labeltokenstore.db.names", "neostore.labeltokenstore.db.names.id", "neostore.nodestore.db", "neostore.nodestore.db.id", "neostore.nodestore.db.labels", "neostore.nodestore.db.labels.id", "neostore.propertystore.db", "neostore.propertystore.db.arrays", "neostore.propertystore.db.arrays.id", "neostore.propertystore.db.id", "neostore.propertystore.db.index", "neostore.propertystore.db.index.id", "neostore.propertystore.db.index.keys", "neostore.propertystore.db.index.keys.id", "neostore.propertystore.db.strings", "neostore.propertystore.db.strings.id", "neostore.relationshipgroupstore.db", "neostore.relationshipgroupstore.db.id", "neostore.relationshipstore.db", "neostore.relationshipstore.db.id", "neostore.relationshiptypestore.db", "neostore.relationshiptypestore.db.id", "neostore.relationshiptypestore.db.names", "neostore.relationshiptypestore.db.names.id", "neostore.schemastore.db", "neostore.schemastore.db.id", "neostore.transaction.db.0", "neostore.transaction.db.1", "neostore.transaction.db.2", "store_lock" };

		 private static readonly string[] _standardStoreDirDirectories = new string[]{ "schema", "index", "branched" };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  CreateIndexDbFile();
			  _neoStoreDataSource = Db.DependencyResolver.resolveDependency( typeof( NeoStoreDataSource ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseIndexAndLabelScanSnapshots() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseIndexAndLabelScanSnapshots()
		 {
			  // Given
			  LabelScanStore labelScanStore = mock( typeof( LabelScanStore ) );
			  IndexingService indexingService = mock( typeof( IndexingService ) );
			  ExplicitIndexProvider explicitIndexes = mock( typeof( ExplicitIndexProvider ) );
			  when( explicitIndexes.AllIndexProviders() ).thenReturn(Collections.emptyList());
			  DatabaseLayout databaseLayout = mock( typeof( DatabaseLayout ) );
			  when( databaseLayout.MetadataStore() ).thenReturn(mock(typeof(File)));
			  LogFiles logFiles = mock( typeof( LogFiles ) );
			  FilesInStoreDirAre( databaseLayout, _standardStoreDirFiles, _standardStoreDirDirectories );
			  StorageEngine storageEngine = mock( typeof( StorageEngine ) );
			  NeoStoreFileListing fileListing = new NeoStoreFileListing( databaseLayout, logFiles, labelScanStore, indexingService, explicitIndexes, storageEngine );

			  IResourceIterator<File> scanSnapshot = ScanStoreFilesAre( labelScanStore, new string[]{ "blah/scan.store", "scan.more" } );
			  IResourceIterator<File> indexSnapshot = IndexFilesAre( indexingService, new string[]{ "schema/index/my.index" } );

			  IResourceIterator<StoreFileMetadata> result = fileListing.Builder().excludeLogFiles().build();

			  // When
			  result.Close();

			  // Then
			  verify( scanSnapshot ).close();
			  verify( indexSnapshot ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListMetaDataStoreLast() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListMetaDataStoreLast()
		 {
			  StoreFileMetadata fileMetadata = Iterators.last( _neoStoreDataSource.listStoreFiles( false ) );
			  assertEquals( fileMetadata.File(), _neoStoreDataSource.DatabaseLayout.metadataStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListMetaDataStoreLastWithTxLogs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListMetaDataStoreLastWithTxLogs()
		 {
			  StoreFileMetadata fileMetadata = Iterators.last( _neoStoreDataSource.listStoreFiles( true ) );
			  assertEquals( fileMetadata.File(), _neoStoreDataSource.DatabaseLayout.metadataStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListTransactionLogsFromCustomLocationWhenConfigured() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListTransactionLogsFromCustomLocationWhenConfigured()
		 {
			  string logFilesPath = "customTxFolder";
			  VerifyLogFilesWithCustomPathListing( logFilesPath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListTransactionLogsFromCustomAbsoluteLocationWhenConfigured() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListTransactionLogsFromCustomAbsoluteLocationWhenConfigured()
		 {
			  File customLogLocation = TestDirectory.directory( "customLogLocation" );
			  VerifyLogFilesWithCustomPathListing( customLogLocation.AbsolutePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListTxLogFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListTxLogFiles()
		 {
			  assertTrue( _neoStoreDataSource.listStoreFiles( true ).Select( metaData => metaData.file().Name ).Any(fileName => TransactionLogFiles.DEFAULT_FILENAME_FILTER.accept(null, fileName)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotListTxLogFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotListTxLogFiles()
		 {
			  assertTrue( _neoStoreDataSource.listStoreFiles( false ).Select( metaData => metaData.file().Name ).noneMatch(fileName => TransactionLogFiles.DEFAULT_FILENAME_FILTER.accept(null, fileName)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListNeostoreFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListNeostoreFiles()
		 {
			  DatabaseLayout layout = _neoStoreDataSource.DatabaseLayout;
			  ISet<File> expectedFiles = layout.StoreFiles();
			  // there was no rotation
			  expectedFiles.remove( layout.CountStoreB() );
			  IResourceIterator<StoreFileMetadata> storeFiles = _neoStoreDataSource.listStoreFiles( false );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<File> listedStoreFiles = storeFiles.Select( StoreFileMetadata::file ).Where( file => !file.Name.Equals( INDEX_DB_FILE_NAME ) ).collect( Collectors.toSet() );
			  assertEquals( expectedFiles, listedStoreFiles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotListFilesFromAdditionalProviderThatRegisterTwice() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotListFilesFromAdditionalProviderThatRegisterTwice()
		 {
			  NeoStoreFileListing neoStoreFileListing = _neoStoreDataSource.NeoStoreFileListing;
			  MarkerFileProvider provider = new MarkerFileProvider();
			  neoStoreFileListing.RegisterStoreFileProvider( provider );
			  neoStoreFileListing.RegisterStoreFileProvider( provider );
			  IResourceIterator<StoreFileMetadata> metadataResourceIterator = neoStoreFileListing.Builder().build();
			  assertEquals( 1, metadataResourceIterator.Where( metadata => "marker".Equals( metadata.file().Name ) ).Count() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyLogFilesWithCustomPathListing(String path) throws java.io.IOException
		 private void VerifyLogFilesWithCustomPathListing( string path )
		 {
			  GraphDatabaseAPI graphDatabase = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.databaseDir("customDb")).setConfig(GraphDatabaseSettings.logical_logs_location, path).newGraphDatabase();
			  NeoStoreDataSource dataSource = graphDatabase.DependencyResolver.resolveDependency( typeof( NeoStoreDataSource ) );
			  LogFiles logFiles = graphDatabase.DependencyResolver.resolveDependency( typeof( LogFiles ) );
			  assertTrue( dataSource.ListStoreFiles( true ).Any( metadata => metadata.LogFile && logFiles.IsLogFile( metadata.file() ) ) );
			  assertEquals( Paths.get( path ).FileName.ToString(), logFiles.LogFilesDirectory().Name );
			  graphDatabase.Shutdown();
		 }

		 private static void FilesInStoreDirAre( DatabaseLayout databaseLayout, string[] filenames, string[] dirs )
		 {
			  List<File> files = new List<File>();
			  MockFiles( filenames, files, false );
			  MockFiles( dirs, files, true );
			  when( databaseLayout.ListDatabaseFiles( any() ) ).thenReturn(Files.ToArray());
		 }

		 private static IResourceIterator<File> ScanStoreFilesAre( LabelScanStore labelScanStore, string[] fileNames )
		 {
			  List<File> files = new List<File>();
			  MockFiles( fileNames, files, false );
			  IResourceIterator<File> snapshot = spy( asResourceIterator( Files.GetEnumerator() ) );
			  when( labelScanStore.SnapshotStoreFiles() ).thenReturn(snapshot);
			  return snapshot;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Neo4Net.graphdb.ResourceIterator<java.io.File> indexFilesAre(Neo4Net.kernel.impl.api.index.IndexingService indexingService, String[] fileNames) throws java.io.IOException
		 private static IResourceIterator<File> IndexFilesAre( IndexingService indexingService, string[] fileNames )
		 {
			  List<File> files = new List<File>();
			  MockFiles( fileNames, files, false );
			  IResourceIterator<File> snapshot = spy( asResourceIterator( Files.GetEnumerator() ) );
			  when( indexingService.SnapshotIndexFiles() ).thenReturn(snapshot);
			  return snapshot;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createIndexDbFile() throws java.io.IOException
		 private void CreateIndexDbFile()
		 {
			  DatabaseLayout databaseLayout = Db.databaseLayout();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File indexFile = databaseLayout.file("index.db");
			  File indexFile = databaseLayout.File( "index.db" );
			  if ( !indexFile.exists() )
			  {
					assertTrue( indexFile.createNewFile() );
			  }
		 }

		 private static void MockFiles( string[] filenames, List<File> files, bool isDirectories )
		 {
			  foreach ( string filename in filenames )
			  {
					File file = mock( typeof( File ) );

					string[] fileNameParts = filename.Split( "/", true );
					when( file.Name ).thenReturn( fileNameParts[fileNameParts.Length - 1] );

					when( file.File ).thenReturn( !isDirectories );
					when( file.Directory ).thenReturn( isDirectories );
					when( file.exists() ).thenReturn(true);
					when( file.Path ).thenReturn( filename );
					Files.Add( file );
			  }
		 }

		 private class MarkerFileProvider : NeoStoreFileListing.StoreFileProvider
		 {
			  public override Resource AddFilesTo( ICollection<StoreFileMetadata> fileMetadataCollection )
			  {
					fileMetadataCollection.Add( new StoreFileMetadata( new File( "marker" ), 0 ) );
					return Neo4Net.GraphDb.Resource_Fields.Empty;
			  }
		 }
	}

}