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
namespace Org.Neo4j.Kernel.Api.Impl.Index
{
	using Document = org.apache.lucene.document.Document;
	using StoredField = org.apache.lucene.document.StoredField;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Test = org.junit.jupiter.api.Test;


	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using PartitionSearcher = Org.Neo4j.Kernel.Api.Impl.Index.partition.PartitionSearcher;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class LuceneAllDocumentsReaderTest
	{
		 private readonly PartitionSearcher _partitionSearcher1 = CreatePartitionSearcher( 1, 0, 2 );
		 private readonly PartitionSearcher _partitionSearcher2 = CreatePartitionSearcher( 2, 1, 2 );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: LuceneAllDocumentsReaderTest() throws java.io.IOException
		 internal LuceneAllDocumentsReaderTest()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allDocumentsMaxCount()
		 internal virtual void AllDocumentsMaxCount()
		 {
			  LuceneAllDocumentsReader allDocumentsReader = CreateAllDocumentsReader();
			  assertEquals( 3, allDocumentsReader.MaxCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeCorrespondingSearcherOnClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseCorrespondingSearcherOnClose()
		 {
			  LuceneAllDocumentsReader allDocumentsReader = CreateAllDocumentsReader();
			  allDocumentsReader.Close();

			  verify( _partitionSearcher1 ).close();
			  verify( _partitionSearcher2 ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readAllDocuments()
		 internal virtual void ReadAllDocuments()
		 {
			  LuceneAllDocumentsReader allDocumentsReader = CreateAllDocumentsReader();
			  IList<Document> documents = Iterators.asList( allDocumentsReader.GetEnumerator() );

			  assertEquals( 3, documents.Count, "Should have 1 document from first partition and 2 from second one." );
			  assertEquals( "1", documents[0].getField( "value" ).stringValue() );
			  assertEquals( "3", documents[1].getField( "value" ).stringValue() );
			  assertEquals( "4", documents[2].getField( "value" ).stringValue() );
		 }

		 private LuceneAllDocumentsReader CreateAllDocumentsReader()
		 {
			  return new LuceneAllDocumentsReader( CreatePartitionReaders() );
		 }

		 private IList<LucenePartitionAllDocumentsReader> CreatePartitionReaders()
		 {
			  LucenePartitionAllDocumentsReader reader1 = new LucenePartitionAllDocumentsReader( _partitionSearcher1 );
			  LucenePartitionAllDocumentsReader reader2 = new LucenePartitionAllDocumentsReader( _partitionSearcher2 );
			  return Arrays.asList( reader1, reader2 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.kernel.api.impl.index.partition.PartitionSearcher createPartitionSearcher(int maxDoc, int partition, int maxSize) throws java.io.IOException
		 private static PartitionSearcher CreatePartitionSearcher( int maxDoc, int partition, int maxSize )
		 {
			  PartitionSearcher partitionSearcher = mock( typeof( PartitionSearcher ) );
			  IndexSearcher indexSearcher = mock( typeof( IndexSearcher ) );
			  IndexReader indexReader = mock( typeof( IndexReader ) );

			  when( partitionSearcher.IndexSearcher ).thenReturn( indexSearcher );
			  when( indexSearcher.IndexReader ).thenReturn( indexReader );
			  when( indexReader.maxDoc() ).thenReturn(maxDoc);

			  when( indexSearcher.doc( 0 ) ).thenReturn( CreateDocument( UniqueDocValue( 1, partition, maxSize ) ) );
			  when( indexSearcher.doc( 1 ) ).thenReturn( CreateDocument( UniqueDocValue( 2, partition, maxSize ) ) );
			  when( indexSearcher.doc( 2 ) ).thenReturn( CreateDocument( UniqueDocValue( 3, partition, maxSize ) ) );

			  return partitionSearcher;
		 }

		 private static string UniqueDocValue( int value, int partition, int maxSize )
		 {
			  return ( value + ( partition * maxSize ) ).ToString();
		 }

		 private static Document CreateDocument( string value )
		 {
			  Document document = new Document();
			  document.add( new StoredField( "value", value ) );
			  return document;
		 }
	}

}