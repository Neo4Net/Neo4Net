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
namespace Neo4Net.Kernel.Api.Impl.Schema.reader
{
	using BooleanQuery = org.apache.lucene.search.BooleanQuery;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using MatchAllDocsQuery = org.apache.lucene.search.MatchAllDocsQuery;
	using MultiTermQuery = org.apache.lucene.search.MultiTermQuery;
	using NumericRangeQuery = org.apache.lucene.search.NumericRangeQuery;
	using TermRangeQuery = org.apache.lucene.search.TermRangeQuery;
	using TotalHitCountCollector = org.apache.lucene.search.TotalHitCountCollector;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


	using TaskCoordinator = Neo4Net.Helpers.TaskCoordinator;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using DocValuesCollector = Neo4Net.Kernel.Api.Impl.Index.collector.DocValuesCollector;
	using PartitionSearcher = Neo4Net.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using NonUniqueLuceneIndexSampler = Neo4Net.Kernel.Api.Impl.Schema.sampler.NonUniqueLuceneIndexSampler;
	using UniqueLuceneIndexSampler = Neo4Net.Kernel.Api.Impl.Schema.sampler.UniqueLuceneIndexSampler;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.range;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	internal class SimpleIndexReaderTest
	{
		 private readonly PartitionSearcher _partitionSearcher = mock( typeof( PartitionSearcher ) );
		 private readonly IndexSearcher _indexSearcher = mock( typeof( IndexSearcher ) );
		 private readonly IndexSamplingConfig _samplingConfig = new IndexSamplingConfig( Config.defaults() );
		 private readonly TaskCoordinator _taskCoordinator = new TaskCoordinator( 0, TimeUnit.MILLISECONDS );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  when( _partitionSearcher.IndexSearcher ).thenReturn( _indexSearcher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void releaseSearcherOnClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ReleaseSearcherOnClose()
		 {
			  IndexReader simpleIndexReader = UniqueSimpleReader;

			  simpleIndexReader.Close();

			  verify( _partitionSearcher ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void seekQueryReachSearcher() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SeekQueryReachSearcher()
		 {
			  IndexReader simpleIndexReader = UniqueSimpleReader;

			  simpleIndexReader.Query( IndexQuery.exact( 1, "test" ) );

			  verify( _indexSearcher ).search( any( typeof( BooleanQuery ) ), any( typeof( DocValuesCollector ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void scanQueryReachSearcher() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ScanQueryReachSearcher()
		 {
			  IndexReader simpleIndexReader = UniqueSimpleReader;

			  simpleIndexReader.Query( IndexQuery.exists( 1 ) );

			  verify( _indexSearcher ).search( any( typeof( MatchAllDocsQuery ) ), any( typeof( DocValuesCollector ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringRangeSeekQueryReachSearcher() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StringRangeSeekQueryReachSearcher()
		 {
			  IndexReader simpleIndexReader = UniqueSimpleReader;

			  simpleIndexReader.Query( range( 1, "a", false, "b", true ) );

			  verify( _indexSearcher ).search( any( typeof( TermRangeQuery ) ), any( typeof( DocValuesCollector ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void prefixRangeSeekQueryReachSearcher() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PrefixRangeSeekQueryReachSearcher()
		 {
			  IndexReader simpleIndexReader = UniqueSimpleReader;

			  simpleIndexReader.Query( IndexQuery.stringPrefix( 1, stringValue( "bb" ) ) );

			  verify( _indexSearcher ).search( any( typeof( MultiTermQuery ) ), any( typeof( DocValuesCollector ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void numberRangeSeekQueryReachSearcher() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NumberRangeSeekQueryReachSearcher()
		 {
			  IndexReader simpleIndexReader = UniqueSimpleReader;

			  simpleIndexReader.Query( range( 1, 7, true, 8, true ) );

			  verify( _indexSearcher ).search( any( typeof( NumericRangeQuery ) ), any( typeof( DocValuesCollector ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void countIndexedNodesReachSearcher() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CountIndexedNodesReachSearcher()
		 {
			  IndexReader simpleIndexReader = UniqueSimpleReader;

			  simpleIndexReader.CountIndexedNodes( 2, new int[] { 3 }, Values.of( "testValue" ) );

			  verify( _indexSearcher ).search( any( typeof( BooleanQuery ) ), any( typeof( TotalHitCountCollector ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uniqueIndexSamplerForUniqueIndex()
		 internal virtual void UniqueIndexSamplerForUniqueIndex()
		 {
			  SimpleIndexReader uniqueSimpleReader = UniqueSimpleReader;
			  assertThat( uniqueSimpleReader.CreateSampler(), instanceOf(typeof(UniqueLuceneIndexSampler)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nonUniqueIndexSamplerForNonUniqueIndex()
		 internal virtual void NonUniqueIndexSamplerForNonUniqueIndex()
		 {
			  SimpleIndexReader uniqueSimpleReader = NonUniqueSimpleReader;
			  assertThat( uniqueSimpleReader.CreateSampler(), instanceOf(typeof(NonUniqueLuceneIndexSampler)) );
		 }

		 private SimpleIndexReader NonUniqueSimpleReader
		 {
			 get
			 {
				  return new SimpleIndexReader( _partitionSearcher, TestIndexDescriptorFactory.forLabel( 0, 0 ), _samplingConfig, _taskCoordinator );
			 }
		 }

		 private SimpleIndexReader UniqueSimpleReader
		 {
			 get
			 {
				  return new SimpleIndexReader( _partitionSearcher, TestIndexDescriptorFactory.uniqueForLabel( 0, 0 ), _samplingConfig, _taskCoordinator );
			 }
		 }
	}

}