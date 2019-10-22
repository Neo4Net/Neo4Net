using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.index;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using NeoStoreFileListing = Neo4Net.Kernel.impl.transaction.state.NeoStoreFileListing;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.relativePath;

	public class CatchupServerIT
	{
		private bool InstanceFieldsInitialized = false;

		public CatchupServerIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			TestDirectory = TestDirectory.testDirectory( FileSystemRule );
			_fsa = FileSystemRule.get();
		}

		 private const string EXISTING_FILE_NAME = "neostore.nodestore.db";
		 private static readonly StoreId _wrongStoreId = new StoreId( 123, 221, 3131, 45678 );
		 private static readonly LogProvider _logProvider = NullLogProvider.Instance;

		 private const string PROP_NAME = "name";
		 private const string PROP = "prop";
		 public static readonly Label Label = label( "MyLabel" );
		 private const string INDEX = "index";
		 private GraphDatabaseAPI _graphDb;
		 private TestCatchupServer _catchupServer;
		 private File _temporaryDirectory;

		 private PageCache _pageCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory(fileSystemRule);
		 public TestDirectory TestDirectory;
		 private CatchUpClient _catchupClient;
		 private DefaultFileSystemAbstraction _fsa;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startDb() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartDb()
		 {
			  _temporaryDirectory = TestDirectory.directory( "temp" );
			  _graphDb = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).setFileSystem(_fsa).newEmbeddedDatabase(TestDirectory.databaseDir());
			  CreateLegacyIndex();
			  CreatePropertyIndex();
			  AddData( _graphDb );

			  _catchupServer = new TestCatchupServer( _fsa, _graphDb );
			  _catchupServer.start();
			  _catchupClient = ( new CatchupClientBuilder() ).build();
			  _catchupClient.start();
			  _pageCache = _graphDb.DependencyResolver.resolveDependency( typeof( PageCache ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopDb() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StopDb()
		 {
			  _pageCache.flushAndForce();
			  if ( _graphDb != null )
			  {
					_graphDb.shutdown();
			  }
			  if ( _catchupClient != null )
			  {
					_catchupClient.stop();
			  }
			  if ( _catchupServer != null )
			  {
					_catchupServer.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListExpectedFilesCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListExpectedFilesCorrectly()
		 {
			  // given (setup) required runtime subject dependencies
			  NeoStoreDataSource neoStoreDataSource = GetNeoStoreDataSource( _graphDb );
			  SimpleCatchupClient simpleCatchupClient = new SimpleCatchupClient( _graphDb, _fsa, _catchupClient, _catchupServer, _temporaryDirectory, _logProvider );

			  // when
			  PrepareStoreCopyResponse prepareStoreCopyResponse = simpleCatchupClient.RequestListOfFilesFromServer();
			  simpleCatchupClient.Close();

			  // then
			  ListOfDownloadedFilesMatchesServer( neoStoreDataSource, prepareStoreCopyResponse.Files );

			  // and downloaded files are identical to source
			  IList<File> expectedCountStoreFiles = ListServerExpectedNonReplayableFiles( neoStoreDataSource );
			  foreach ( File storeFileSnapshot in expectedCountStoreFiles )
			  {
					FileContentEquals( DatabaseFileToClientFile( storeFileSnapshot ), storeFileSnapshot );
			  }

			  // and
			  AssertTransactionIdMatches( prepareStoreCopyResponse.LastTransactionId() );

			  //and
			  assertTrue( "Expected an empty set of ids. Found size " + prepareStoreCopyResponse.IndexIds.size(), prepareStoreCopyResponse.IndexIds.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommunicateErrorIfStoreIdDoesNotMatchRequest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommunicateErrorIfStoreIdDoesNotMatchRequest()
		 {
			  // given (setup) required runtime subject dependencies
			  AddData( _graphDb );
			  SimpleCatchupClient simpleCatchupClient = new SimpleCatchupClient( _graphDb, _fsa, _catchupClient, _catchupServer, _temporaryDirectory, _logProvider );

			  // when the list of files are requested from the server with the wrong storeId
			  PrepareStoreCopyResponse prepareStoreCopyResponse = simpleCatchupClient.RequestListOfFilesFromServer( _wrongStoreId );
			  simpleCatchupClient.Close();

			  // then the response is not a list of files but an error
			  assertEquals( PrepareStoreCopyResponse.Status.EStoreIdMismatch, prepareStoreCopyResponse.Status() );

			  // and the list of files is empty because the request should have failed
			  File[] remoteFiles = prepareStoreCopyResponse.Files;
			  assertArrayEquals( new File[]{}, remoteFiles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void individualFileCopyWorks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndividualFileCopyWorks()
		 {
			  // given a file exists on the server
			  AddData( _graphDb );
			  File existingFile = new File( _temporaryDirectory, EXISTING_FILE_NAME );

			  // and
			  SimpleCatchupClient simpleCatchupClient = new SimpleCatchupClient( _graphDb, _fsa, _catchupClient, _catchupServer, _temporaryDirectory, _logProvider );

			  // when we copy that file
			  _pageCache.flushAndForce();
			  StoreCopyFinishedResponse storeCopyFinishedResponse = simpleCatchupClient.RequestIndividualFile( existingFile );
			  simpleCatchupClient.Close();

			  // then the response is successful
			  assertEquals( StoreCopyFinishedResponse.Status.Success, storeCopyFinishedResponse.Status() );

			  // then the contents matches
			  FileContentEquals( ClientFileToDatabaseFile( existingFile ), existingFile );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void individualIndexSnapshotCopyWorks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndividualIndexSnapshotCopyWorks()
		 {

			  // given
			  NeoStoreDataSource neoStoreDataSource = GetNeoStoreDataSource( _graphDb );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> expectingFiles = neoStoreDataSource.NeoStoreFileListing.builder().excludeAll().includeSchemaIndexStoreFiles().build().Select(StoreFileMetadata::file).ToList();
			  SimpleCatchupClient simpleCatchupClient = new SimpleCatchupClient( _graphDb, _fsa, _catchupClient, _catchupServer, _temporaryDirectory, _logProvider );

			  // and
			  LongIterator indexIds = GetExpectedIndexIds( neoStoreDataSource ).longIterator();

			  // when
			  while ( indexIds.hasNext() )
			  {
					long indexId = indexIds.next();
					StoreCopyFinishedResponse response = simpleCatchupClient.RequestIndexSnapshot( indexId );
					simpleCatchupClient.Close();
					assertEquals( StoreCopyFinishedResponse.Status.Success, response.Status() );
			  }

			  // then
			  FileContentEquals( expectingFiles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void individualFileCopyFailsIfStoreIdMismatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndividualFileCopyFailsIfStoreIdMismatch()
		 {
			  // given a file exists on the server
			  AddData( _graphDb );
			  File expectedExistingFile = _graphDb.databaseLayout().file(EXISTING_FILE_NAME);

			  // and
			  SimpleCatchupClient simpleCatchupClient = new SimpleCatchupClient( _graphDb, _fsa, _catchupClient, _catchupServer, _temporaryDirectory, _logProvider );

			  // when we copy that file using a different storeId
			  StoreCopyFinishedResponse storeCopyFinishedResponse = simpleCatchupClient.RequestIndividualFile( expectedExistingFile, _wrongStoreId );
			  simpleCatchupClient.Close();

			  // then the response from the server should be an error message that describes a store ID mismatch
			  assertEquals( StoreCopyFinishedResponse.Status.EStoreIdMismatch, storeCopyFinishedResponse.Status() );
		 }

		 private void AssertTransactionIdMatches( long lastTxId )
		 {
			  long expectedTransactionId = GetCheckPointer( _graphDb ).lastCheckPointedTransactionId();
			  assertEquals( expectedTransactionId, lastTxId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void fileContentEquals(java.util.Collection<java.io.File> countStore) throws java.io.IOException
		 private void FileContentEquals( ICollection<File> countStore )
		 {
			  foreach ( File file in countStore )
			  {
					FileContentEquals( DatabaseFileToClientFile( file ), file );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File databaseFileToClientFile(java.io.File file) throws java.io.IOException
		 private File DatabaseFileToClientFile( File file )
		 {
			  string relativePathToDatabaseDir = relativePath( TestDirectory.databaseDir(), file );
			  return new File( _temporaryDirectory, relativePathToDatabaseDir );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File clientFileToDatabaseFile(java.io.File file) throws java.io.IOException
		 private File ClientFileToDatabaseFile( File file )
		 {
			  string relativePathToDatabaseDir = relativePath( _temporaryDirectory, file );
			  return new File( TestDirectory.databaseDir(), relativePathToDatabaseDir );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void fileContentEquals(java.io.File fileA, java.io.File fileB) throws java.io.IOException
		 private void FileContentEquals( File fileA, File fileB )
		 {
			  assertNotEquals( fileA.Path, fileB.Path );
			  string message = string.Format( "Expected file: {0}\ndoes not match actual file: {1}", fileA, fileB );
			  assertEquals( message, StoreCopyClientIT.FileContent( fileA, _fsa ), StoreCopyClientIT.FileContent( fileB, _fsa ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void listOfDownloadedFilesMatchesServer(org.Neo4Net.kernel.NeoStoreDataSource neoStoreDataSource, java.io.File[] files) throws java.io.IOException
		 private void ListOfDownloadedFilesMatchesServer( NeoStoreDataSource neoStoreDataSource, File[] files )
		 {
			  IList<string> expectedStoreFiles = GetExpectedStoreFiles( neoStoreDataSource );
			  IList<string> givenFile = java.util.files.Select( File.getName ).ToList();
			  assertThat( givenFile, containsInAnyOrder( expectedStoreFiles.ToArray() ) );
		 }

		 private static LongSet GetExpectedIndexIds( NeoStoreDataSource neoStoreDataSource )
		 {
			  return neoStoreDataSource.NeoStoreFileListing.NeoStoreFileIndexListing.IndexIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.List<java.io.File> listServerExpectedNonReplayableFiles(org.Neo4Net.kernel.NeoStoreDataSource neoStoreDataSource) throws java.io.IOException
		 private static IList<File> ListServerExpectedNonReplayableFiles( NeoStoreDataSource neoStoreDataSource )
		 {
			  using ( Stream<StoreFileMetadata> countStoreStream = neoStoreDataSource.NeoStoreFileListing.builder().excludeAll().includeNeoStoreFiles().build().stream(), Stream<StoreFileMetadata> explicitIndexStream = neoStoreDataSource.NeoStoreFileListing.builder().excludeAll().includeExplicitIndexStoreStoreFiles().build().stream() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return Stream.concat( countStoreStream.filter( IsCountFile( neoStoreDataSource.DatabaseLayout ) ), explicitIndexStream ).map( StoreFileMetadata::file ).collect( toList() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<String> getExpectedStoreFiles(org.Neo4Net.kernel.NeoStoreDataSource neoStoreDataSource) throws java.io.IOException
		 private IList<string> GetExpectedStoreFiles( NeoStoreDataSource neoStoreDataSource )
		 {
			  NeoStoreFileListing.StoreFileListingBuilder builder = neoStoreDataSource.NeoStoreFileListing.builder();
			  builder.ExcludeLogFiles().excludeExplicitIndexStoreFiles().excludeSchemaIndexStoreFiles().excludeAdditionalProviders();
			  using ( Stream<StoreFileMetadata> stream = builder.Build().stream() )
			  {
					return stream.filter( IsCountFile( neoStoreDataSource.DatabaseLayout ).negate() ).map(sfm => sfm.file().Name).collect(toList());
			  }
		 }

		 private static System.Predicate<StoreFileMetadata> IsCountFile( DatabaseLayout databaseLayout )
		 {
			  return storeFileMetadata => databaseLayout.CountStoreA().Equals(storeFileMetadata.file()) || databaseLayout.CountStoreB().Equals(storeFileMetadata.file());
		 }

		 private static void AddData( GraphDatabaseAPI graphDb )
		 {
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Node node = graphDb.CreateNode();
					node.AddLabel( Label );
					node.SetProperty( PROP_NAME, "Neo" );
					node.SetProperty( PROP, GlobalRandom.NextDouble * 10000 );
					graphDb.CreateNode().createRelationshipTo(node, RelationshipType.withName("KNOWS"));
					tx.Success();
			  }
		 }

		 private void CreatePropertyIndex()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					_graphDb.schema().indexFor(Label).on(PROP_NAME).create();
					tx.Success();
			  }
		 }

		 private void CreateLegacyIndex()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					Index<Node> nodeIndex = _graphDb.index().forNodes(INDEX);
					nodeIndex.Add( _graphDb.createNode(), "some-key", "som-value" );
					tx.Success();
			  }
		 }

		 private static CheckPointer GetCheckPointer( GraphDatabaseAPI graphDb )
		 {
			  return graphDb.DependencyResolver.resolveDependency( typeof( CheckPointer ) );
		 }

		 private static NeoStoreDataSource GetNeoStoreDataSource( GraphDatabaseAPI graphDb )
		 {
			  return graphDb.DependencyResolver.resolveDependency( typeof( NeoStoreDataSource ) );
		 }
	}

}