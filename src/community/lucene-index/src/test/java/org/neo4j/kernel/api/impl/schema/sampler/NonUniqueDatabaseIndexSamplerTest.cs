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
namespace Neo4Net.Kernel.Api.Impl.Schema.sampler
{
	using Document = org.apache.lucene.document.Document;
	using Fields = Org.Apache.Lucene.Index.Fields;
	using Terms = Org.Apache.Lucene.Index.Terms;
	using TermsEnum = Org.Apache.Lucene.Index.TermsEnum;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using RAMDirectory = org.apache.lucene.store.RAMDirectory;
	using BytesRef = org.apache.lucene.util.BytesRef;
	using Test = org.junit.jupiter.api.Test;
	using Mockito = org.mockito.Mockito;


	using TaskCoordinator = Neo4Net.Helpers.TaskCoordinator;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexReaderStub = Neo4Net.Kernel.Api.Impl.Index.IndexReaderStub;
	using IndexWriterConfigs = Neo4Net.Kernel.Api.Impl.Index.IndexWriterConfigs;
	using PartitionSearcher = Neo4Net.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using WritableIndexPartition = Neo4Net.Kernel.Api.Impl.Index.partition.WritableIndexPartition;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class NonUniqueDatabaseIndexSamplerTest
	{
		 private readonly IndexSearcher _indexSearcher = mock( typeof( IndexSearcher ), Mockito.RETURNS_DEEP_STUBS );
		 private readonly TaskCoordinator _taskControl = new TaskCoordinator( 0, TimeUnit.MILLISECONDS );
		 private readonly IndexSamplingConfig _indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nonUniqueSamplingCancel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NonUniqueSamplingCancel()
		 {
			  Terms terms = GetTerms( "test", 1 );
			  IDictionary<string, Terms> fieldTermsMap = MapUtil.genericMap( "0string", terms, "id", terms, "0string", terms );
			  IndexReaderStub indexReader = new IndexReaderStub( new SamplingFields( fieldTermsMap ) );
			  when( _indexSearcher.IndexReader ).thenReturn( indexReader );

			  NonUniqueLuceneIndexSampler luceneIndexSampler = CreateSampler();
			  _taskControl.cancel();
			  IndexNotFoundKernelException notFoundKernelException = assertThrows( typeof( IndexNotFoundKernelException ), luceneIndexSampler.sampleIndex );
			  assertEquals( notFoundKernelException.Message, "Index dropped while sampling." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nonUniqueIndexSampling() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NonUniqueIndexSampling()
		 {
			  Terms aTerms = GetTerms( "a", 1 );
			  Terms idTerms = GetTerms( "id", 2 );
			  Terms bTerms = GetTerms( "b", 3 );
			  IDictionary<string, Terms> fieldTermsMap = MapUtil.genericMap( "0string", aTerms, "id", idTerms, "0array", bTerms );
			  IndexReaderStub indexReader = new IndexReaderStub( new SamplingFields( fieldTermsMap ) );
			  indexReader.Elements = new string[4];
			  when( _indexSearcher.IndexReader ).thenReturn( indexReader );

			  assertEquals( new IndexSample( 4, 2, 4 ), CreateSampler().sampleIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void samplingOfLargeNumericValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SamplingOfLargeNumericValues()
		 {
			  using ( RAMDirectory dir = new RAMDirectory(), WritableIndexPartition indexPartition = new WritableIndexPartition(new File("testPartition"), dir, IndexWriterConfigs.standard()) )
			  {
					InsertDocument( indexPartition, 1, long.MaxValue );
					InsertDocument( indexPartition, 2, int.MaxValue );

					indexPartition.MaybeRefreshBlocking();

					using ( PartitionSearcher searcher = indexPartition.AcquireSearcher() )
					{
						 NonUniqueLuceneIndexSampler sampler = new NonUniqueLuceneIndexSampler( searcher.IndexSearcher, _taskControl.newInstance(), new IndexSamplingConfig(Config.defaults()) );

						 assertEquals( new IndexSample( 2, 2, 2 ), sampler.SampleIndex() );
					}
			  }
		 }

		 private NonUniqueLuceneIndexSampler CreateSampler()
		 {
			  return new NonUniqueLuceneIndexSampler( _indexSearcher, _taskControl.newInstance(), _indexSamplingConfig );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.apache.lucene.index.Terms getTerms(String value, int frequency) throws java.io.IOException
		 private static Terms GetTerms( string value, int frequency )
		 {
			  TermsEnum termsEnum = mock( typeof( TermsEnum ) );
			  Terms terms = mock( typeof( Terms ) );
			  when( terms.GetEnumerator() ).thenReturn(termsEnum);
			  when( termsEnum.next() ).thenReturn(new BytesRef(value.GetBytes())).thenReturn(null);
			  when( termsEnum.docFreq() ).thenReturn(frequency);
			  return terms;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void insertDocument(org.neo4j.kernel.api.impl.index.partition.WritableIndexPartition partition, long nodeId, Object propertyValue) throws java.io.IOException
		 private static void InsertDocument( WritableIndexPartition partition, long nodeId, object propertyValue )
		 {
			  Document doc = LuceneDocumentStructure.documentRepresentingProperties( nodeId, Values.of( propertyValue ) );
			  partition.IndexWriter.addDocument( doc );
		 }

		 private class SamplingFields : Fields
		 {

			  internal IDictionary<string, Terms> FieldTermsMap;

			  internal SamplingFields( IDictionary<string, Terms> fieldTermsMap )
			  {
					this.FieldTermsMap = fieldTermsMap;
			  }

			  public override IEnumerator<string> Iterator()
			  {
					return FieldTermsMap.Keys.GetEnumerator();
			  }

			  public override Terms Terms( string field )
			  {
					return FieldTermsMap[field];
			  }

			  public override int Size()
			  {
					return FieldTermsMap.Count;
			  }
		 }

	}

}