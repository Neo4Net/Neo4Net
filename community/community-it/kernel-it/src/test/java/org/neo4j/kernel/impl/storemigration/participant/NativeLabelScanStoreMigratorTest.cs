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
namespace Org.Neo4j.Kernel.impl.storemigration.participant
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using LabelScanWriter = Org.Neo4j.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using FullStoreChangeStream = Org.Neo4j.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using NativeLabelScanStore = Org.Neo4j.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using InvalidIdGeneratorException = Org.Neo4j.Kernel.impl.store.InvalidIdGeneratorException;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using Position = Org.Neo4j.Kernel.impl.store.MetaDataStore.Position;
	using StandardV2_3 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_2 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_4;
	using ProgressReporter = Org.Neo4j.Kernel.impl.util.monitoring.ProgressReporter;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.versionStringToLong;

	public class NativeLabelScanStoreMigratorTest
	{
		private bool InstanceFieldsInitialized = false;

		public NativeLabelScanStoreMigratorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _fileSystemRule ).around( _pageCacheRule );
		}

		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

		 private File _storeDir;
		 private File _nativeLabelIndex;
		 private DatabaseLayout _migrationLayout;
		 private File _luceneLabelScanStore;

		 private readonly ProgressReporter _progressReporter = mock( typeof( ProgressReporter ) );

		 private FileSystemAbstraction _fileSystem;
		 private PageCache _pageCache;
		 private NativeLabelScanStoreMigrator _indexMigrator;
		 private DatabaseLayout _databaseLayout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _databaseLayout = _testDirectory.databaseLayout();
			  _storeDir = _databaseLayout.databaseDirectory();
			  _nativeLabelIndex = _databaseLayout.labelScanStore();
			  _migrationLayout = _testDirectory.databaseLayout( "migrationDir" );
			  _luceneLabelScanStore = _testDirectory.databaseDir().toPath().resolve(Paths.get("schema", "label", "lucene")).toFile();

			  _fileSystem = _fileSystemRule.get();
			  _pageCache = _pageCacheRule.getPageCache( _fileSystemRule );
			  _indexMigrator = new NativeLabelScanStoreMigrator( _fileSystem, _pageCache, Config.defaults() );
			  _fileSystem.mkdirs( _luceneLabelScanStore );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationIfNativeIndexExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SkipMigrationIfNativeIndexExist()
		 {
			  ByteBuffer sourceBuffer = WriteFile( _nativeLabelIndex, new sbyte[]{ 1, 2, 3 } );

			  _indexMigrator.migrate( _databaseLayout, _migrationLayout, _progressReporter, StandardV3_2.STORE_VERSION, StandardV3_2.STORE_VERSION );
			  _indexMigrator.moveMigratedFiles( _migrationLayout, _databaseLayout, StandardV3_2.STORE_VERSION, StandardV3_2.STORE_VERSION );

			  ByteBuffer resultBuffer = ReadFileContent( _nativeLabelIndex, 3 );
			  assertEquals( sourceBuffer, resultBuffer );
			  assertTrue( _fileSystem.fileExists( _luceneLabelScanStore ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.kernel.impl.store.InvalidIdGeneratorException.class) public void failMigrationWhenNodeIdFileIsBroken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailMigrationWhenNodeIdFileIsBroken()
		 {
			  PrepareEmpty23Database();
			  File nodeIdFile = _databaseLayout.idNodeStore();
			  WriteFile( nodeIdFile, new sbyte[]{ 1, 2, 3 } );

			  _indexMigrator.migrate( _databaseLayout, _migrationLayout, _progressReporter, StandardV3_2.STORE_VERSION, StandardV3_2.STORE_VERSION );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clearMigrationDirFromAnyLabelScanStoreBeforeMigrating() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClearMigrationDirFromAnyLabelScanStoreBeforeMigrating()
		 {
			  // given
			  PrepareEmpty23Database();
			  InitializeNativeLabelScanStoreWithContent( _migrationLayout );
			  File toBeDeleted = _migrationLayout.labelScanStore();
			  assertTrue( _fileSystem.fileExists( toBeDeleted ) );

			  // when
			  _indexMigrator.migrate( _databaseLayout, _migrationLayout, _progressReporter, StandardV3_2.STORE_VERSION, StandardV3_2.STORE_VERSION );

			  // then
			  AssertNoContentInNativeLabelScanStore( _migrationLayout );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void luceneLabelIndexRemovedAfterSuccessfulMigration() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LuceneLabelIndexRemovedAfterSuccessfulMigration()
		 {
			  PrepareEmpty23Database();

			  _indexMigrator.migrate( _databaseLayout, _migrationLayout, _progressReporter, StandardV2_3.STORE_VERSION, StandardV3_2.STORE_VERSION );
			  _indexMigrator.moveMigratedFiles( _migrationLayout, _databaseLayout, StandardV2_3.STORE_VERSION, StandardV3_2.STORE_VERSION );

			  assertFalse( _fileSystem.fileExists( _luceneLabelScanStore ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void moveCreatedNativeLabelIndexBackToStoreDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MoveCreatedNativeLabelIndexBackToStoreDirectory()
		 {
			  PrepareEmpty23Database();
			  _indexMigrator.migrate( _databaseLayout, _migrationLayout, _progressReporter, StandardV2_3.STORE_VERSION, StandardV3_2.STORE_VERSION );
			  File migrationNativeIndex = _migrationLayout.labelScanStore();
			  ByteBuffer migratedFileContent = WriteFile( migrationNativeIndex, new sbyte[]{ 5, 4, 3, 2, 1 } );

			  _indexMigrator.moveMigratedFiles( _migrationLayout, _databaseLayout, StandardV2_3.STORE_VERSION, StandardV3_2.STORE_VERSION );

			  ByteBuffer movedNativeIndex = ReadFileContent( _nativeLabelIndex, 5 );
			  assertEquals( migratedFileContent, movedNativeIndex );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populateNativeLabelScanIndexDuringMigration() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulateNativeLabelScanIndexDuringMigration()
		 {
			  Prepare34DatabaseWithNodes();
			  _indexMigrator.migrate( _databaseLayout, _migrationLayout, _progressReporter, StandardV3_4.STORE_VERSION, StandardV3_4.STORE_VERSION );
			  _indexMigrator.moveMigratedFiles( _migrationLayout, _databaseLayout, StandardV2_3.STORE_VERSION, StandardV3_2.STORE_VERSION );

			  using ( Lifespan lifespan = new Lifespan() )
			  {
					NativeLabelScanStore labelScanStore = GetNativeLabelScanStore( _databaseLayout, true );
					lifespan.Add( labelScanStore );
					for ( int labelId = 0; labelId < 10; labelId++ )
					{
						 using ( LabelScanReader labelScanReader = labelScanStore.NewReader() )
						 {
							  int nodeCount = PrimitiveLongCollections.count( labelScanReader.NodesWithLabel( labelId ) );
							  assertEquals( format( "Expected to see only one node for label %d but was %d.", labelId, nodeCount ), 1, nodeCount );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportProgressOnNativeIndexPopulation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportProgressOnNativeIndexPopulation()
		 {
			  Prepare34DatabaseWithNodes();
			  _indexMigrator.migrate( _databaseLayout, _migrationLayout, _progressReporter, StandardV3_4.STORE_VERSION, StandardV3_4.STORE_VERSION );
			  _indexMigrator.moveMigratedFiles( _migrationLayout, _databaseLayout, StandardV2_3.STORE_VERSION, StandardV3_2.STORE_VERSION );

			  verify( _progressReporter ).start( 10 );
			  verify( _progressReporter, times( 10 ) ).progress( 1 );
		 }

		 private NativeLabelScanStore GetNativeLabelScanStore( DatabaseLayout databaseLayout, bool readOnly )
		 {
			  return new NativeLabelScanStore( _pageCache, databaseLayout, _fileSystem, Org.Neo4j.Kernel.Impl.Api.scan.FullStoreChangeStream_Fields.Empty, readOnly, new Monitors(), RecoveryCleanupWorkCollector.ignore() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void initializeNativeLabelScanStoreWithContent(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private void InitializeNativeLabelScanStoreWithContent( DatabaseLayout databaseLayout )
		 {
			  using ( Lifespan lifespan = new Lifespan() )
			  {
					NativeLabelScanStore nativeLabelScanStore = GetNativeLabelScanStore( databaseLayout, false );
					lifespan.Add( nativeLabelScanStore );
					using ( LabelScanWriter labelScanWriter = nativeLabelScanStore.NewWriter() )
					{
						 labelScanWriter.Write( NodeLabelUpdate.labelChanges( 1, new long[0], new long[]{ 1 } ) );
					}
					nativeLabelScanStore.Force( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }
		 }

		 private void AssertNoContentInNativeLabelScanStore( DatabaseLayout databaseLayout )
		 {
			  using ( Lifespan lifespan = new Lifespan() )
			  {
					NativeLabelScanStore nativeLabelScanStore = GetNativeLabelScanStore( databaseLayout, true );
					lifespan.Add( nativeLabelScanStore );
					using ( LabelScanReader labelScanReader = nativeLabelScanStore.NewReader() )
					{
						 int count = PrimitiveLongCollections.count( labelScanReader.NodesWithLabel( 1 ) );
						 assertEquals( 0, count );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ByteBuffer writeFile(java.io.File file, byte[] content) throws java.io.IOException
		 private ByteBuffer WriteFile( File file, sbyte[] content )
		 {
			  ByteBuffer sourceBuffer = ByteBuffer.wrap( content );
			  StoreFileContent( file, sourceBuffer );
			  sourceBuffer.flip();
			  return sourceBuffer;
		 }

		 private void Prepare34DatabaseWithNodes()
		 {
			  GraphDatabaseService embeddedDatabase = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(_storeDir);
			  try
			  {
					using ( Transaction transaction = embeddedDatabase.BeginTx() )
					{
						 for ( int i = 0; i < 10; i++ )
						 {
							  embeddedDatabase.CreateNode( Label.label( "label" + i ) );
						 }
						 transaction.Success();
					}
			  }
			  finally
			  {
					embeddedDatabase.Shutdown();
			  }
			  _fileSystem.deleteFile( _nativeLabelIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareEmpty23Database() throws java.io.IOException
		 private void PrepareEmpty23Database()
		 {
			  ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(_storeDir).shutdown();
			  _fileSystem.deleteFile( _nativeLabelIndex );
			  MetaDataStore.setRecord( _pageCache, _databaseLayout.metadataStore(), MetaDataStore.Position.STORE_VERSION, versionStringToLong(StandardV2_3.STORE_VERSION) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ByteBuffer readFileContent(java.io.File nativeLabelIndex, int length) throws java.io.IOException
		 private ByteBuffer ReadFileContent( File nativeLabelIndex, int length )
		 {
			  using ( StoreChannel storeChannel = _fileSystem.open( nativeLabelIndex, OpenMode.READ ) )
			  {
					ByteBuffer readBuffer = ByteBuffer.allocate( length );
					//noinspection StatementWithEmptyBody
					while ( readBuffer.hasRemaining() && storeChannel.read(readBuffer) > 0 )
					{
						 // read till the end of store channel
					}
					readBuffer.flip();
					return readBuffer;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void storeFileContent(java.io.File file, ByteBuffer sourceBuffer) throws java.io.IOException
		 private void StoreFileContent( File file, ByteBuffer sourceBuffer )
		 {
			  using ( StoreChannel storeChannel = _fileSystem.create( file ) )
			  {
					storeChannel.WriteAll( sourceBuffer );
			  }
		 }
	}

}