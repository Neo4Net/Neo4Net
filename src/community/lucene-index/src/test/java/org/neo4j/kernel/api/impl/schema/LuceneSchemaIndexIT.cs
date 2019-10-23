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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Neo4Net.GraphDb;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using LuceneAllDocumentsReader = Neo4Net.Kernel.Api.Impl.Index.LuceneAllDocumentsReader;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneSchemaIndexIT
	internal class LuceneSchemaIndexIT
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;

		 private readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.forLabel( 0, 0 );
		 private readonly Config _config = Config.defaults();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void before()
		 internal virtual void Before()
		 {
			  System.setProperty( "luceneSchemaIndex.maxPartitionSize", "10" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void after()
		 internal virtual void After()
		 {
			  System.setProperty( "luceneSchemaIndex.maxPartitionSize", "" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void snapshotForPartitionedIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SnapshotForPartitionedIndex()
		 {
			  // Given
			  using ( LuceneIndexAccessor indexAccessor = CreateDefaultIndexAccessor() )
			  {
					GenerateUpdates( indexAccessor, 32 );
					indexAccessor.Force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );

					// When & Then
					IList<string> singlePartitionFileTemplates = Arrays.asList( ".cfe", ".cfs", ".si", "segments_1" );
					using ( ResourceIterator<File> snapshotIterator = indexAccessor.SnapshotFiles() )
					{
						 IList<string> indexFileNames = AsFileInsidePartitionNames( snapshotIterator );

						 assertTrue( indexFileNames.Count >= ( singlePartitionFileTemplates.Count * 4 ), "Expect files from 4 partitions" );
						 IDictionary<string, int> templateMatches = CountTemplateMatches( singlePartitionFileTemplates, indexFileNames );

						 foreach ( string fileTemplate in singlePartitionFileTemplates )
						 {
							  int? matches = templateMatches[fileTemplate];
							  assertTrue( matches >= 4, "Expect to see at least 4 matches for template: " + fileTemplate );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void snapshotForIndexWithNoCommits() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SnapshotForIndexWithNoCommits()
		 {
			  // Given
			  // A completely un-used index
			  using ( LuceneIndexAccessor indexAccessor = CreateDefaultIndexAccessor(), ResourceIterator<File> snapshotIterator = indexAccessor.SnapshotFiles() )
			  {
					assertThat( AsUniqueSetOfNames( snapshotIterator ), equalTo( emptySet() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void updateMultiplePartitionedIndex() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void UpdateMultiplePartitionedIndex()
		 {
			  using ( SchemaIndex index = LuceneSchemaIndexBuilder.Create( _descriptor, _config ).withFileSystem( _fileSystem ).withIndexRootFolder( _testDir.directory( "partitionedIndexForUpdates" ) ).build() )
			  {
					index.create();
					index.open();
					AddDocumentToIndex( index, 45 );

					index.IndexWriter.updateDocument( LuceneDocumentStructure.NewTermForChangeOrRemove( 100 ), LuceneDocumentStructure.DocumentRepresentingProperties( ( long ) 100, Values.intValue( 100 ) ) );
					index.maybeRefreshBlocking();

					long documentsInIndex = Iterators.count( index.allDocumentsReader().GetEnumerator() );
					assertEquals( 46, documentsInIndex, "Index should contain 45 added and 1 updated document." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createPopulateDropIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreatePopulateDropIndex()
		 {
			  File crudOperation = _testDir.directory( "indexCRUDOperation" );
			  using ( SchemaIndex crudIndex = LuceneSchemaIndexBuilder.Create( _descriptor, _config ).withFileSystem( _fileSystem ).withIndexRootFolder( new File( crudOperation, "crudIndex" ) ).build() )
			  {
					crudIndex.open();

					AddDocumentToIndex( crudIndex, 1 );
					assertEquals( 1, crudIndex.Partitions.size() );

					AddDocumentToIndex( crudIndex, 21 );
					assertEquals( 3, crudIndex.Partitions.size() );

					crudIndex.drop();

					assertFalse( crudIndex.Open );
					assertEquals( 0, crudOperation.list().length );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createFailPartitionedIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreateFailPartitionedIndex()
		 {
			  using ( SchemaIndex failedIndex = LuceneSchemaIndexBuilder.Create( _descriptor, _config ).withFileSystem( _fileSystem ).withIndexRootFolder( new File( _testDir.directory( "failedIndexFolder" ), "failedIndex" ) ).build() )
			  {
					failedIndex.open();

					AddDocumentToIndex( failedIndex, 35 );
					assertEquals( 4, failedIndex.Partitions.size() );

					failedIndex.markAsFailed( "Some failure" );
					failedIndex.flush();

					assertTrue( failedIndex.Open );
					assertFalse( failedIndex.Online );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void openClosePartitionedIndex() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void OpenClosePartitionedIndex()
		 {
			  SchemaIndex reopenIndex = null;
			  try
			  {
					reopenIndex = LuceneSchemaIndexBuilder.Create( _descriptor, _config ).withFileSystem( _fileSystem ).withIndexRootFolder( new File( _testDir.directory( "reopenIndexFolder" ), "reopenIndex" ) ).build();
					reopenIndex.open();

					AddDocumentToIndex( reopenIndex, 1 );

					reopenIndex.close();
					assertFalse( reopenIndex.Open );

					reopenIndex.open();
					assertTrue( reopenIndex.Open );

					AddDocumentToIndex( reopenIndex, 10 );

					reopenIndex.close();
					assertFalse( reopenIndex.Open );

					reopenIndex.open();
					assertTrue( reopenIndex.Open );

					reopenIndex.close();
					reopenIndex.open();
					AddDocumentToIndex( reopenIndex, 100 );

					reopenIndex.maybeRefreshBlocking();

					using ( LuceneAllDocumentsReader allDocumentsReader = reopenIndex.allDocumentsReader() )
					{
						 assertEquals( 111, allDocumentsReader.MaxCount(), "All documents should be visible" );
					}
			  }
			  finally
			  {
					if ( reopenIndex != null )
					{
						 reopenIndex.close();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addDocumentToIndex(SchemaIndex index, int documents) throws java.io.IOException
		 private void AddDocumentToIndex( SchemaIndex index, int documents )
		 {
			  for ( int i = 0; i < documents; i++ )
			  {
					index.IndexWriter.addDocument( LuceneDocumentStructure.DocumentRepresentingProperties( ( long ) i, Values.intValue( i ) ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private LuceneIndexAccessor createDefaultIndexAccessor() throws java.io.IOException
		 private LuceneIndexAccessor CreateDefaultIndexAccessor()
		 {
			  SchemaIndex index = LuceneSchemaIndexBuilder.Create( _descriptor, _config ).withFileSystem( _fileSystem ).withIndexRootFolder( _testDir.directory( "testIndex" ) ).build();
			  index.create();
			  index.open();
			  return new LuceneIndexAccessor( index, _descriptor );
		 }

		 private IList<string> AsFileInsidePartitionNames( ResourceIterator<File> resources )
		 {
			  int testDirectoryPathLength = _testDir.directory().AbsolutePath.length();
			  return new IList<string> { resources };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void generateUpdates(LuceneIndexAccessor indexAccessor, int nodesToUpdate) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void GenerateUpdates( LuceneIndexAccessor indexAccessor, int nodesToUpdate )
		 {
			  using ( IndexUpdater updater = indexAccessor.NewUpdater( IndexUpdateMode.ONLINE ) )
			  {
					for ( int nodeId = 0; nodeId < nodesToUpdate; nodeId++ )
					{
						 updater.Process( Add( nodeId, nodeId ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> add(long nodeId, Object value)
		 private IndexEntryUpdate<object> Add( long nodeId, object value )
		 {
			  return IndexEntryUpdate.add( nodeId, _descriptor.schema(), Values.of(value) );
		 }

		 private static IDictionary<string, int> CountTemplateMatches( IList<string> nameTemplates, IList<string> fileNames )
		 {
			  IDictionary<string, int> templateMatches = new Dictionary<string, int>();
			  foreach ( string indexFileName in fileNames )
			  {
					foreach ( string template in nameTemplates )
					{
						 if ( indexFileName.EndsWith( template, StringComparison.Ordinal ) )
						 {
							  templateMatches[template] = templateMatches.getOrDefault( template, 0 ) + 1;
						 }
					}
			  }
			  return templateMatches;
		 }

		 private static ISet<string> AsUniqueSetOfNames( ResourceIterator<File> files )
		 {
			  List<string> @out = new List<string>();
			  while ( Files.MoveNext() )
			  {
					string name = Files.Current.Name;
					@out.Add( name );
			  }
			  return Iterables.asUniqueSet( @out );
		 }
	}

}