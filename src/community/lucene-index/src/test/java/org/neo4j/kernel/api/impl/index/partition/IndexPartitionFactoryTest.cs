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
namespace Neo4Net.Kernel.Api.Impl.Index.partition
{
	using Document = org.apache.lucene.document.Document;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Directory = org.apache.lucene.store.Directory;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class IndexPartitionFactoryTest
	internal class IndexPartitionFactoryTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
		 private Directory _directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  _directory = DirectoryFactory.PERSISTENT.open( _testDirectory.directory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createReadOnlyPartition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreateReadOnlyPartition()
		 {
			  PrepareIndex();
			  using ( AbstractIndexPartition indexPartition = ( new ReadOnlyIndexPartitionFactory() ).CreatePartition(_testDirectory.directory(), _directory) )
			  {
					assertThrows( typeof( System.NotSupportedException ), indexPartition.getIndexWriter );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createWritablePartition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreateWritablePartition()
		 {
			  try (AbstractIndexPartition indexPartition = new WritableIndexPartitionFactory(IndexWriterConfigs.standard)
											.createPartition( _testDirectory.directory(), _directory ))
											{

					using ( IndexWriter indexWriter = indexPartition.IndexWriter )
					{
						 indexWriter.addDocument( new Document() );
						 indexWriter.commit();
						 indexPartition.maybeRefreshBlocking();
						 using ( PartitionSearcher searcher = indexPartition.acquireSearcher() )
						 {
							  assertEquals( 1, searcher.IndexSearcher.IndexReader.numDocs(), "We should be able to see newly added document " );
						 }
					}
											}
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareIndex() throws java.io.IOException
		 private void PrepareIndex()
		 {
			  File location = _testDirectory.directory();
			  try (AbstractIndexPartition ignored = new WritableIndexPartitionFactory(IndexWriterConfigs.standard)
											.createPartition( location, DirectoryFactory.PERSISTENT.open( location ) ))
											{
					// empty
											}
		 }
	}

}