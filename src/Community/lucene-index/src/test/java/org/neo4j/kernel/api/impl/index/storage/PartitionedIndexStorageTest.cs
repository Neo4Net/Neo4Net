using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.Impl.Index.storage
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using Document = org.apache.lucene.document.Document;
	using Field = org.apache.lucene.document.Field;
	using StringField = org.apache.lucene.document.StringField;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Directory = org.apache.lucene.store.Directory;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using IOUtils = Neo4Net.Io.IOUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class PartitionedIndexStorageTest
	internal class PartitionedIndexStorageTest
	{
		 private static readonly DirectoryFactory_InMemoryDirectoryFactory _directoryFactory = new DirectoryFactory_InMemoryDirectoryFactory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fs;
		 private DefaultFileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

		 private PartitionedIndexStorage _storage;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void createIndexStorage()
		 internal virtual void CreateIndexStorage()
		 {
			  _storage = new PartitionedIndexStorage( _directoryFactory, _fs, _testDir.databaseDir() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void prepareFolderCreatesFolder() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PrepareFolderCreatesFolder()
		 {
			  File folder = CreateRandomFolder( _testDir.databaseDir() );

			  _storage.prepareFolder( folder );

			  assertTrue( _fs.fileExists( folder ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void prepareFolderRemovesFromFileSystem() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PrepareFolderRemovesFromFileSystem()
		 {
			  File folder = CreateRandomFolder( _testDir.databaseDir() );
			  CreateRandomFilesAndFolders( folder );

			  _storage.prepareFolder( folder );

			  assertTrue( _fs.fileExists( folder ) );
			  assertTrue( ArrayUtil.isEmpty( _fs.listFiles( folder ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void prepareFolderRemovesFromLucene() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PrepareFolderRemovesFromLucene()
		 {
			  File folder = CreateRandomFolder( _testDir.databaseDir() );
			  Directory dir = CreateRandomLuceneDir( folder );

			  assertFalse( ArrayUtil.isEmpty( dir.listAll() ) );

			  _storage.prepareFolder( folder );

			  assertTrue( _fs.fileExists( folder ) );
			  assertTrue( ArrayUtil.isEmpty( dir.listAll() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void openIndexDirectoriesForEmptyIndex() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void OpenIndexDirectoriesForEmptyIndex()
		 {
			  File indexFolder = _storage.IndexFolder;

			  IDictionary<File, Directory> directories = _storage.openIndexDirectories();

			  assertTrue( directories.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void openIndexDirectories() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void OpenIndexDirectories()
		 {
			  File indexFolder = _storage.IndexFolder;
			  CreateRandomLuceneDir( indexFolder ).close();
			  CreateRandomLuceneDir( indexFolder ).close();

			  IDictionary<File, Directory> directories = _storage.openIndexDirectories();
			  try
			  {
					assertEquals( 2, directories.Count );
					foreach ( Directory dir in directories.Values )
					{
						 assertFalse( ArrayUtil.isEmpty( dir.listAll() ) );
					}
			  }
			  finally
			  {
					IOUtils.closeAll( directories.Values );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void listFoldersForEmptyFolder() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ListFoldersForEmptyFolder()
		 {
			  File indexFolder = _storage.IndexFolder;
			  _fs.mkdirs( indexFolder );

			  IList<File> folders = _storage.listFolders();

			  assertTrue( folders.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void listFolders() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ListFolders()
		 {
			  File indexFolder = _storage.IndexFolder;
			  _fs.mkdirs( indexFolder );

			  CreateRandomFile( indexFolder );
			  CreateRandomFile( indexFolder );
			  File folder1 = CreateRandomFolder( indexFolder );
			  File folder2 = CreateRandomFolder( indexFolder );

			  IList<File> folders = _storage.listFolders();

			  assertEquals( asSet( folder1, folder2 ), new HashSet<>( folders ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldListIndexPartitionsSorted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldListIndexPartitionsSorted()
		 {
			  // GIVEN
			  try ( FileSystemAbstraction scramblingFs = new DefaultFileSystemAbstractionAnonymousInnerClass( this ) ){PartitionedIndexStorage myStorage = new PartitionedIndexStorage( _directoryFactory, scramblingFs, _testDir.databaseDir() );
					File parent = myStorage.IndexFolder;
					int directoryCount = 10;
					for ( int i = 0; i < directoryCount; i++ )
					{
						 scramblingFs.mkdirs( new File( parent, ( i + 1 ).ToString() ) );
					}

					// WHEN
					IDictionary<File, Directory> directories = myStorage.openIndexDirectories();

					// THEN
					assertEquals( directoryCount, directories.Count );
					int previous = 0;
					foreach ( KeyValuePair<File, Directory> directory in directories.SetOfKeyValuePairs() )
					{
						 int current = parseInt( directory.Key.Name );
						 assertTrue( current > previous, "Wanted directory " + current + " to have higher id than previous " + previous );
						 previous = current;
					}
		 }

			  private class DefaultFileSystemAbstractionAnonymousInnerClass : DefaultFileSystemAbstraction
			  {
				  private readonly PartitionedIndexStorageTest _outerInstance;

				  public DefaultFileSystemAbstractionAnonymousInnerClass( PartitionedIndexStorageTest outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override File[] listFiles( File directory )
				  {
						IList<File> files = new IList<File> { base.listFiles( directory ) };
						Collections.shuffle( files );
						return Files.ToArray();
				  }
			  }
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createRandomFilesAndFolders(java.io.File rootFolder) throws java.io.IOException
		 private void createRandomFilesAndFolders( File rootFolder )
		 {
			  int count = ThreadLocalRandom.current().Next(10) + 1;
			  for ( int i = 0; i < count; i++ )
			  {
					if ( ThreadLocalRandom.current().nextBoolean() )
					{
						 createRandomFile( rootFolder );
					}
					else
					{
						 createRandomFolder( rootFolder );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.store.Directory createRandomLuceneDir(java.io.File rootFolder) throws java.io.IOException
		 private Directory createRandomLuceneDir( File rootFolder )
		 {
			  File folder = createRandomFolder( rootFolder );
			  Directory directory = directoryFactory.open( folder );
			  using ( IndexWriter writer = new IndexWriter( directory, IndexWriterConfigs.standard() ) )
			  {
					writer.addDocument( randomDocument() );
					writer.commit();
			  }
			  return directory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createRandomFile(java.io.File rootFolder) throws java.io.IOException
		 private void createRandomFile( File rootFolder )
		 {
			  File file = new File( rootFolder, RandomStringUtils.randomNumeric( 5 ) );
			  using ( StoreChannel channel = fs.create( file ) )
			  {
					channel.WriteAll( ByteBuffer.allocate( 100 ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createRandomFolder(java.io.File rootFolder) throws java.io.IOException
		 private File createRandomFolder( File rootFolder )
		 {
			  File folder = new File( rootFolder, RandomStringUtils.randomNumeric( 5 ) );
			  fs.mkdirs( folder );
			  return folder;
		 }

		 private static Document randomDocument()
		 {
			  Document doc = new Document();
			  doc.add( new StringField( "field", RandomStringUtils.randomNumeric( 5 ), Field.Store.YES ) );
			  return doc;
		 }
}

}