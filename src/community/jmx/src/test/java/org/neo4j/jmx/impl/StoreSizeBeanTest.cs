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
namespace Neo4Net.Jmx.impl
{
	using Assert = org.junit.Assert;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DefaultIndexProviderMap = Neo4Net.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using KernelData = Neo4Net.Kernel.Internal.KernelData;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;
	using EphemeralFileSystemExtension = Neo4Net.Test.extension.EphemeralFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.iterable;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) class StoreSizeBeanTest
	internal class StoreSizeBeanTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fs;
		 private EphemeralFileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
		 private readonly ExplicitIndexProvider _explicitIndexProviderLookup = mock( typeof( ExplicitIndexProvider ) );
		 private readonly IndexProvider _indexProvider = MockedIndexProvider( "provider1" );
		 private readonly IndexProvider _indexProvider2 = MockedIndexProvider( "provider2" );
		 private readonly LabelScanStore _labelScanStore = mock( typeof( LabelScanStore ) );
		 private StoreSize _storeSizeBean;
		 private LogFiles _logFiles;
		 private ManagementData _managementData;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  _logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _testDirectory.databaseDir(), _fs ).build();

			  Dependencies dependencies = new Dependencies();
			  Config config = Config.defaults( default_schema_provider, _indexProvider.ProviderDescriptor.name() );
			  DataSourceManager dataSourceManager = new DataSourceManager( config );
			  GraphDatabaseAPI db = mock( typeof( GraphDatabaseAPI ) );
			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );

			  dependencies.SatisfyDependency( _indexProvider );
			  dependencies.SatisfyDependency( _indexProvider2 );

			  DefaultIndexProviderMap indexProviderMap = new DefaultIndexProviderMap( dependencies, config );
			  indexProviderMap.Init();

			  // Setup all dependencies
			  dependencies.SatisfyDependency( _fs );
			  dependencies.SatisfyDependencies( dataSourceManager );
			  dependencies.SatisfyDependency( _logFiles );
			  dependencies.SatisfyDependency( _explicitIndexProviderLookup );
			  dependencies.SatisfyDependency( indexProviderMap );
			  dependencies.SatisfyDependency( _labelScanStore );
			  when( Db.DependencyResolver ).thenReturn( dependencies );
			  when( dataSource.DependencyResolver ).thenReturn( dependencies );
			  when( dataSource.DatabaseLayout ).thenReturn( _testDirectory.databaseLayout() );

			  // Start DataSourceManager
			  dataSourceManager.Register( dataSource );
			  dataSourceManager.Start();

			  // Create bean
			  KernelData kernelData = new KernelData( _fs, mock( typeof( PageCache ) ), _testDirectory.databaseDir(), config, dataSourceManager );
			  _managementData = new ManagementData( new StoreSizeBean(), kernelData, ManagementSupport.Load() );
			  _storeSizeBean = StoreSizeBean.CreateBean( _managementData, false, 0, mock( typeof( Clock ) ) );

			  when( _indexProvider.directoryStructure() ).thenReturn(mock(typeof(IndexDirectoryStructure)));
			  when( _indexProvider2.directoryStructure() ).thenReturn(mock(typeof(IndexDirectoryStructure)));
			  when( _labelScanStore.LabelScanStoreFile ).thenReturn( _testDirectory.databaseLayout().labelScanStore() );
		 }

		 private static IndexProvider MockedIndexProvider( string name )
		 {
			  IndexProvider provider = mock( typeof( IndexProvider ) );
			  when( provider.ProviderDescriptor ).thenReturn( new IndexProviderDescriptor( name, "1" ) );
			  return provider;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createFakeStoreDirectory() throws java.io.IOException
		 private void CreateFakeStoreDirectory()
		 {
			  IDictionary<File, int> dummyStore = new Dictionary<File, int>();
			  DatabaseLayout layout = _testDirectory.databaseLayout();
			  dummyStore[layout.NodeStore()] = 1;
			  dummyStore[layout.IdNodeStore()] = 2;
			  dummyStore[layout.NodeLabelStore()] = 3;
			  dummyStore[layout.IdNodeLabelStore()] = 4;
			  dummyStore[layout.PropertyStore()] = 5;
			  dummyStore[layout.IdPropertyStore()] = 6;
			  dummyStore[layout.PropertyKeyTokenStore()] = 7;
			  dummyStore[layout.IdPropertyKeyTokenStore()] = 8;
			  dummyStore[layout.PropertyKeyTokenNamesStore()] = 9;
			  dummyStore[layout.IdPropertyKeyTokenNamesStore()] = 10;
			  dummyStore[layout.PropertyStringStore()] = 11;
			  dummyStore[layout.IdPropertyStringStore()] = 12;
			  dummyStore[layout.PropertyArrayStore()] = 13;
			  dummyStore[layout.IdPropertyArrayStore()] = 14;
			  dummyStore[layout.RelationshipStore()] = 15;
			  dummyStore[layout.IdRelationshipStore()] = 16;
			  dummyStore[layout.RelationshipGroupStore()] = 17;
			  dummyStore[layout.IdRelationshipGroupStore()] = 18;
			  dummyStore[layout.RelationshipTypeTokenStore()] = 19;
			  dummyStore[layout.IdRelationshipTypeTokenStore()] = 20;
			  dummyStore[layout.RelationshipTypeTokenNamesStore()] = 21;
			  dummyStore[layout.IdRelationshipTypeTokenNamesStore()] = 22;
			  dummyStore[layout.LabelTokenStore()] = 23;
			  dummyStore[layout.IdLabelTokenStore()] = 24;
			  dummyStore[layout.LabelTokenNamesStore()] = 25;
			  dummyStore[layout.IdLabelTokenNamesStore()] = 26;
			  dummyStore[layout.SchemaStore()] = 27;
			  dummyStore[layout.IdSchemaStore()] = 28;
			  dummyStore[layout.CountStoreB()] = 29;
			  // COUNTS_STORE_B is created in the test

			  foreach ( KeyValuePair<File, int> fileEntry in dummyStore.SetOfKeyValuePairs() )
			  {
					CreateFileOfSize( fileEntry.Key, fileEntry.Value );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void verifyGroupingOfNodeRelatedFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VerifyGroupingOfNodeRelatedFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 1, 4 ), _storeSizeBean.NodeStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void verifyGroupingOfPropertyRelatedFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VerifyGroupingOfPropertyRelatedFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 5, 10 ), _storeSizeBean.PropertyStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void verifyGroupingOfStringRelatedFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VerifyGroupingOfStringRelatedFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 11, 12 ), _storeSizeBean.StringStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void verifyGroupingOfArrayRelatedFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VerifyGroupingOfArrayRelatedFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 13, 14 ), _storeSizeBean.ArrayStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void verifyGroupingOfRelationshipRelatedFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VerifyGroupingOfRelationshipRelatedFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 15, 22 ), _storeSizeBean.RelationshipStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void verifyGroupingOfLabelRelatedFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VerifyGroupingOfLabelRelatedFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 23, 26 ), _storeSizeBean.LabelStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void verifyGroupingOfCountStoreRelatedFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VerifyGroupingOfCountStoreRelatedFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 29, 29 ), _storeSizeBean.CountStoreSize );
			  CreateFileOfSize( _testDirectory.databaseLayout().countStoreA(), 30 );
			  assertEquals( GetExpected( 29, 30 ), _storeSizeBean.CountStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void verifyGroupingOfSchemaRelatedFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VerifyGroupingOfSchemaRelatedFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 27, 28 ), _storeSizeBean.SchemaStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void sumAllFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SumAllFiles()
		 {
			  CreateFakeStoreDirectory();
			  assertEquals( GetExpected( 0, 29 ), _storeSizeBean.TotalStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCountAllLogFiles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCountAllLogFiles()
		 {
			  CreateFileOfSize( _logFiles.getLogFileForVersion( 0 ), 1 );
			  CreateFileOfSize( _logFiles.getLogFileForVersion( 1 ), 2 );

			  assertEquals( 3L, _storeSizeBean.TransactionLogsSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCountAllIndexFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCountAllIndexFiles()
		 {
			  // Explicit index file
			  File explicitIndex = _testDirectory.databaseLayout().file("explicitIndex");
			  CreateFileOfSize( explicitIndex, 1 );

			  IndexImplementation indexImplementation = mock( typeof( IndexImplementation ) );
			  when( indexImplementation.GetIndexImplementationDirectory( any() ) ).thenReturn(explicitIndex);
			  when( _explicitIndexProviderLookup.allIndexProviders() ).thenReturn(iterable(indexImplementation));

			  // Schema index files
			  File schemaIndex = _testDirectory.databaseLayout().file("schemaIndex");
			  CreateFileOfSize( schemaIndex, 2 );
			  IndexDirectoryStructure directoryStructure = mock( typeof( IndexDirectoryStructure ) );
			  when( directoryStructure.RootDirectory() ).thenReturn(schemaIndex);
			  when( _indexProvider.directoryStructure() ).thenReturn(directoryStructure);

			  File schemaIndex2 = _testDirectory.databaseLayout().file("schemaIndex2");
			  CreateFileOfSize( schemaIndex2, 3 );
			  IndexDirectoryStructure directoryStructure2 = mock( typeof( IndexDirectoryStructure ) );
			  when( directoryStructure2.RootDirectory() ).thenReturn(schemaIndex2);
			  when( _indexProvider2.directoryStructure() ).thenReturn(directoryStructure2);

			  // Label scan store
			  File labelScan = _testDirectory.databaseLayout().labelScanStore();
			  CreateFileOfSize( labelScan, 4 );
			  when( _labelScanStore.LabelScanStoreFile ).thenReturn( labelScan );

			  // Count all files
			  assertEquals( 10, _storeSizeBean.IndexStoreSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCacheValues() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCacheValues()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.Clock clock = mock(java.time.Clock.class);
			  Clock clock = mock( typeof( Clock ) );
			  _storeSizeBean = StoreSizeBean.CreateBean( _managementData, false, 100, clock );
			  when( clock.millis() ).thenReturn(100L);

			  CreateFileOfSize( _logFiles.getLogFileForVersion( 0 ), 1 );
			  CreateFileOfSize( _logFiles.getLogFileForVersion( 1 ), 2 );

			  Assert.assertEquals( 3L, _storeSizeBean.TransactionLogsSize );

			  CreateFileOfSize( _logFiles.getLogFileForVersion( 2 ), 3 );
			  CreateFileOfSize( _logFiles.getLogFileForVersion( 3 ), 4 );

			  Assert.assertEquals( 3L, _storeSizeBean.TransactionLogsSize );

			  when( clock.millis() ).thenReturn(200L);

			  Assert.assertEquals( 10L, _storeSizeBean.TransactionLogsSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createFileOfSize(java.io.File file, int size) throws java.io.IOException
		 private void CreateFileOfSize( File file, int size )
		 {
			  using ( StoreChannel storeChannel = _fs.create( file ) )
			  {
					sbyte[] bytes = new sbyte[size];
					ByteBuffer buffer = ByteBuffer.wrap( bytes );
					storeChannel.WriteAll( buffer );
			  }
		 }

		 private static long GetExpected( int lower, int upper )
		 {
			  long expected = 0;
			  for ( int i = lower; i <= upper; i++ )
			  {
					expected += i;
			  }
			  return expected;
		 }
	}

}