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
namespace Org.Neo4j.Kernel.Api.Impl.Index.backup
{
	using Document = org.apache.lucene.document.Document;
	using Field = org.apache.lucene.document.Field;
	using StringField = org.apache.lucene.document.StringField;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Directory = org.apache.lucene.store.Directory;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Org.Neo4j.Graphdb;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) public class ReadOnlyIndexSnapshotFileIteratorTest
	public class ReadOnlyIndexSnapshotFileIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

		 internal File IndexDir;
		 protected internal Directory Dir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  IndexDir = _testDir.databaseDir();
			  Dir = DirectoryFactory.PERSISTENT.open( IndexDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach public void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  IOUtils.closeAll( Dir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnRealSnapshotIfIndexAllowsIt() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReturnRealSnapshotIfIndexAllowsIt()
		 {
			  PrepareIndex();

			  ISet<string> files = ListDir( Dir );
			  assertFalse( Files.Count == 0 );

			  using ( ResourceIterator<File> snapshot = MakeSnapshot() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ISet<string> snapshotFiles = snapshot.Select( File.getName ).collect( toSet() );
					assertEquals( files, snapshotFiles );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnEmptyIteratorWhenNoCommitsHaveBeenMade() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReturnEmptyIteratorWhenNoCommitsHaveBeenMade()
		 {
			  using ( ResourceIterator<File> snapshot = MakeSnapshot() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( snapshot.hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareIndex() throws java.io.IOException
		 private void PrepareIndex()
		 {
			  using ( IndexWriter writer = new IndexWriter( Dir, IndexWriterConfigs.standard() ) )
			  {
					InsertRandomDocuments( writer );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.graphdb.ResourceIterator<java.io.File> makeSnapshot() throws java.io.IOException
		 protected internal virtual ResourceIterator<File> MakeSnapshot()
		 {
			  return LuceneIndexSnapshots.ForIndex( IndexDir, Dir );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void insertRandomDocuments(org.apache.lucene.index.IndexWriter writer) throws java.io.IOException
		 private static void InsertRandomDocuments( IndexWriter writer )
		 {
			  Document doc = new Document();
			  doc.add( new StringField( "a", "b", Field.Store.YES ) );
			  doc.add( new StringField( "c", "d", Field.Store.NO ) );
			  writer.addDocument( doc );
			  writer.commit();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.Set<String> listDir(org.apache.lucene.store.Directory dir) throws java.io.IOException
		 private static ISet<string> ListDir( Directory dir )
		 {
			  string[] files = dir.listAll();
			  return Stream.of( files ).filter( file => !IndexWriter.WRITE_LOCK_NAME.Equals( file ) ).collect( toSet() );
		 }

	}

}