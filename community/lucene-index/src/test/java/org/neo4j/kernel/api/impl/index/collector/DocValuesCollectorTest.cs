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
namespace Org.Neo4j.Kernel.Api.Impl.Index.collector
{
	using Document = org.apache.lucene.document.Document;
	using NumericDocValues = Org.Apache.Lucene.Index.NumericDocValues;
	using ConstantScoreScorer = org.apache.lucene.search.ConstantScoreScorer;
	using DocIdSetIterator = org.apache.lucene.search.DocIdSetIterator;
	using Scorer = org.apache.lucene.search.Scorer;
	using Sort = org.apache.lucene.search.Sort;
	using SortField = org.apache.lucene.search.SortField;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Test = org.junit.jupiter.api.Test;


	using Org.Neo4j.Graphdb.index;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;

	internal sealed class DocValuesCollectorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStartWithEmptyMatchingDocs()
		 internal void ShouldStartWithEmptyMatchingDocs()
		 {
			  //given
			  DocValuesCollector collector = new DocValuesCollector();

			  // when
			  // then
			  assertEquals( emptyList(), collector.GetMatchingDocs() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCollectAllHitsPerSegment() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldCollectAllHitsPerSegment()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector();
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.Collect( 3 );
			  collector.Collect( 5 );
			  collector.Collect( 9 );

			  // then
			  assertEquals( 4, collector.TotalHits );
			  IList<DocValuesCollector.MatchingDocs> allMatchingDocs = collector.GetMatchingDocs();
			  assertEquals( 1, allMatchingDocs.Count );
			  DocValuesCollector.MatchingDocs matchingDocs = allMatchingDocs[0];
			  assertSame( readerStub.Context, matchingDocs.Context );
			  assertEquals( 4, matchingDocs.TotalHits );
			  DocIdSetIterator idIterator = matchingDocs.DocIdSet.GetEnumerator();
			  assertEquals( 1, idIterator.nextDoc() );
			  assertEquals( 3, idIterator.nextDoc() );
			  assertEquals( 5, idIterator.nextDoc() );
			  assertEquals( 9, idIterator.nextDoc() );
			  assertEquals( DocIdSetIterator.NO_MORE_DOCS, idIterator.nextDoc() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCollectOneMatchingDocsPerSegment() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldCollectOneMatchingDocsPerSegment()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector();
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.Collect( 3 );
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 5 );
			  collector.Collect( 9 );

			  // then
			  assertEquals( 4, collector.TotalHits );
			  IList<DocValuesCollector.MatchingDocs> allMatchingDocs = collector.GetMatchingDocs();
			  assertEquals( 2, allMatchingDocs.Count );

			  DocValuesCollector.MatchingDocs matchingDocs = allMatchingDocs[0];
			  assertSame( readerStub.Context, matchingDocs.Context );
			  assertEquals( 2, matchingDocs.TotalHits );
			  DocIdSetIterator idIterator = matchingDocs.DocIdSet.GetEnumerator();
			  assertEquals( 1, idIterator.nextDoc() );
			  assertEquals( 3, idIterator.nextDoc() );
			  assertEquals( DocIdSetIterator.NO_MORE_DOCS, idIterator.nextDoc() );

			  matchingDocs = allMatchingDocs[1];
			  assertSame( readerStub.Context, matchingDocs.Context );
			  assertEquals( 2, matchingDocs.TotalHits );
			  idIterator = matchingDocs.DocIdSet.GetEnumerator();
			  assertEquals( 5, idIterator.nextDoc() );
			  assertEquals( 9, idIterator.nextDoc() );
			  assertEquals( DocIdSetIterator.NO_MORE_DOCS, idIterator.nextDoc() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotSaveScoresWhenNotRequired() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldNotSaveScoresWhenNotRequired()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );

			  // then
			  DocValuesCollector.MatchingDocs matchingDocs = collector.GetMatchingDocs()[0];
			  assertNull( matchingDocs.Scores );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSaveScoresWhenRequired() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldSaveScoresWhenRequired()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( true );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Scorer = ConstantScorer( 13.42f );
			  collector.Collect( 1 );

			  // then
			  DocValuesCollector.MatchingDocs matchingDocs = collector.GetMatchingDocs()[0];
			  assertArrayEquals( new float[]{ 13.42f }, matchingDocs.Scores, 0.001f );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSaveScoresInADenseArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldSaveScoresInADenseArray()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( true );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Scorer = ConstantScorer( 1.0f );
			  collector.Collect( 1 );
			  collector.Scorer = ConstantScorer( 41.0f );
			  collector.Collect( 41 );

			  // then
			  DocValuesCollector.MatchingDocs matchingDocs = collector.GetMatchingDocs()[0];
			  assertArrayEquals( new float[]{ 1.0f, 41.0f }, matchingDocs.Scores, 0.001f );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDynamicallyResizeScoresArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldDynamicallyResizeScoresArray()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( true );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Scorer = ConstantScorer( 1.0f );
			  // scores starts with array size of 32, adding 42 docs forces resize
			  for ( int i = 0; i < 42; i++ )
			  {
					collector.Collect( i );
			  }

			  // then
			  DocValuesCollector.MatchingDocs matchingDocs = collector.GetMatchingDocs()[0];
			  float[] scores = new float[42];
			  Arrays.fill( scores, 1.0f );
			  assertArrayEquals( scores, matchingDocs.Scores, 0.001f );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnIndexHitsInIndexOrderWhenNoSortIsGiven() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReturnIndexHitsInIndexOrderWhenNoSortIsGiven()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector();
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.Collect( 2 );

			  // then
			  IndexHits<Document> indexHits = collector.GetIndexHits( null );
			  assertEquals( 2, indexHits.Size() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "1", indexHits.next().get("id") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "2", indexHits.next().get("id") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( indexHits.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnIndexHitsOrderedByRelevance() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReturnIndexHitsOrderedByRelevance()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( true );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Scorer = ConstantScorer( 1.0f );
			  collector.Collect( 1 );
			  collector.Scorer = ConstantScorer( 2.0f );
			  collector.Collect( 2 );

			  // then
			  IndexHits<Document> indexHits = collector.GetIndexHits( Sort.RELEVANCE );
			  assertEquals( 2, indexHits.Size() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "2", indexHits.next().get("id") );
			  assertEquals( 2.0f, indexHits.CurrentScore(), 0.001f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "1", indexHits.next().get("id") );
			  assertEquals( 1.0f, indexHits.CurrentScore(), 0.001f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( indexHits.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnIndexHitsInGivenSortOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReturnIndexHitsInGivenSortOrder()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 43 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.Collect( 3 );
			  collector.Collect( 37 );
			  collector.Collect( 42 );

			  // then
			  Sort byIdDescending = new Sort( new SortField( "id", SortField.Type.LONG, true ) );
			  IndexHits<Document> indexHits = collector.GetIndexHits( byIdDescending );
			  assertEquals( 4, indexHits.Size() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "42", indexHits.next().get("id") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "37", indexHits.next().get("id") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "3", indexHits.next().get("id") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "1", indexHits.next().get("id") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( indexHits.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSilentlyMergeAllSegments() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldSilentlyMergeAllSegments()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 2 );

			  // then
			  IndexHits<Document> indexHits = collector.GetIndexHits( null );
			  assertEquals( 2, indexHits.Size() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "1", indexHits.next().get("id") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "2", indexHits.next().get("id") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( indexHits.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnEmptyIteratorWhenNoHits() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReturnEmptyIteratorWhenNoHits()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );

			  // then
			  IndexHits<Document> indexHits = collector.GetIndexHits( null );
			  assertEquals( 0, indexHits.Size() );
			  assertEquals( Float.NaN, indexHits.CurrentScore(), 0.001f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( indexHits.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadDocValuesInIndexOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReadDocValuesInIndexOrder()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.Collect( 2 );

			  // then
			  DocValuesCollector.LongValuesIterator valuesIterator = collector.GetValuesIterator( "id" );
			  assertEquals( 2, valuesIterator.Remaining() );
			  assertEquals( 1, valuesIterator.Next() );
			  assertEquals( 2, valuesIterator.Next() );
			  assertFalse( valuesIterator.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSilentlyMergeSegmentsWhenReadingDocValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldSilentlyMergeSegmentsWhenReadingDocValues()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 2 );

			  // then
			  DocValuesCollector.LongValuesIterator valuesIterator = collector.GetValuesIterator( "id" );
			  assertEquals( 2, valuesIterator.Remaining() );
			  assertEquals( 1, valuesIterator.Next() );
			  assertEquals( 2, valuesIterator.Next() );
			  assertFalse( valuesIterator.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnEmptyIteratorWhenNoDocValues()
		 internal void ShouldReturnEmptyIteratorWhenNoDocValues()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );

			  // then
			  DocValuesCollector.LongValuesIterator valuesIterator = collector.GetValuesIterator( "id" );
			  assertEquals( 0, valuesIterator.Remaining() );
			  assertFalse( valuesIterator.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnDocValuesInIndexOrderWhenNoSortIsGiven() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReturnDocValuesInIndexOrderWhenNoSortIsGiven()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.Collect( 2 );

			  // then
			  LongIterator valuesIterator = collector.GetSortedValuesIterator( "id", null );
			  assertEquals( 1, valuesIterator.next() );
			  assertEquals( 2, valuesIterator.next() );
			  assertFalse( valuesIterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnDocValuesInRelevanceOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReturnDocValuesInRelevanceOrder()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( true );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Scorer = ConstantScorer( 1.0f );
			  collector.Collect( 1 );
			  collector.Scorer = ConstantScorer( 2.0f );
			  collector.Collect( 2 );

			  // then
			  LongIterator valuesIterator = collector.GetSortedValuesIterator( "id", Sort.RELEVANCE );
			  assertEquals( 2, valuesIterator.next() );
			  assertEquals( 1, valuesIterator.next() );
			  assertFalse( valuesIterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnDocValuesInGivenOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReturnDocValuesInGivenOrder()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Collect( 1 );
			  collector.Collect( 2 );

			  // then
			  Sort byIdDescending = new Sort( new SortField( "id", SortField.Type.LONG, true ) );
			  LongIterator valuesIterator = collector.GetSortedValuesIterator( "id", byIdDescending );
			  assertEquals( 2, valuesIterator.next() );
			  assertEquals( 1, valuesIterator.next() );
			  assertFalse( valuesIterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSilentlyMergeSegmentsWhenReturnDocValuesInOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldSilentlyMergeSegmentsWhenReturnDocValuesInOrder()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( true );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Scorer = ConstantScorer( 1.0f );
			  collector.Collect( 1 );
			  collector.DoSetNextReader( readerStub.Context );
			  collector.Scorer = ConstantScorer( 2.0f );
			  collector.Collect( 2 );

			  // then
			  LongIterator valuesIterator = collector.GetSortedValuesIterator( "id", Sort.RELEVANCE );
			  assertEquals( 2, valuesIterator.next() );
			  assertEquals( 1, valuesIterator.next() );
			  assertFalse( valuesIterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnEmptyIteratorWhenNoDocValuesInOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal void ShouldReturnEmptyIteratorWhenNoDocValuesInOrder()
		 {
			  // given
			  DocValuesCollector collector = new DocValuesCollector( false );
			  IndexReaderStub readerStub = IndexReaderWithMaxDocs( 42 );

			  // when
			  collector.DoSetNextReader( readerStub.Context );

			  // then
			  LongIterator valuesIterator = collector.GetSortedValuesIterator( "id", Sort.RELEVANCE );
			  assertFalse( valuesIterator.hasNext() );
		 }

		 private static IndexReaderStub IndexReaderWithMaxDocs( int maxDocs )
		 {
			  NumericDocValues identityValues = new NumericDocValuesAnonymousInnerClass();
			  IndexReaderStub stub = new IndexReaderStub( identityValues );
			  stub.Elements = new string[maxDocs];
			  return stub;
		 }

		 private class NumericDocValuesAnonymousInnerClass : NumericDocValues
		 {
			 public override long get( int docID )
			 {
				  return docID;
			 }
		 }

		 private static Scorer ConstantScorer( float score )
		 {
			  return new ConstantScoreScorer( null, score, ( DocIdSetIterator ) null );
		 }
	}

}