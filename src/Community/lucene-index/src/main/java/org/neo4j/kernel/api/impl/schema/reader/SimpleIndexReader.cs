using System;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Api.Impl.Schema.reader
{
	using Fields = Org.Apache.Lucene.Index.Fields;
	using MultiFields = Org.Apache.Lucene.Index.MultiFields;
	using Terms = Org.Apache.Lucene.Index.Terms;
	using TermsEnum = Org.Apache.Lucene.Index.TermsEnum;
	using BooleanClause = org.apache.lucene.search.BooleanClause;
	using BooleanQuery = org.apache.lucene.search.BooleanQuery;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using TotalHitCountCollector = org.apache.lucene.search.TotalHitCountCollector;
	using BytesRef = org.apache.lucene.util.BytesRef;
	using NumericUtils = org.apache.lucene.util.NumericUtils;


	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using TaskControl = Neo4Net.Helpers.TaskControl;
	using TaskCoordinator = Neo4Net.Helpers.TaskCoordinator;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using IndexQueryType = Neo4Net.@internal.Kernel.Api.IndexQuery.IndexQueryType;
	using IndexNotApplicableKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using DocValuesCollector = Neo4Net.Kernel.Api.Impl.Index.collector.DocValuesCollector;
	using PartitionSearcher = Neo4Net.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using NonUniqueLuceneIndexSampler = Neo4Net.Kernel.Api.Impl.Schema.sampler.NonUniqueLuceneIndexSampler;
	using UniqueLuceneIndexSampler = Neo4Net.Kernel.Api.Impl.Schema.sampler.UniqueLuceneIndexSampler;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using BridgingIndexProgressor = Neo4Net.Kernel.Impl.Api.schema.BridgingIndexProgressor;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using AbstractIndexReader = Neo4Net.Storageengine.Api.schema.AbstractIndexReader;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using IndexSampler = Neo4Net.Storageengine.Api.schema.IndexSampler;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.IndexQueryType.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.LuceneDocumentStructure.NODE_ID_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptor.Type.UNIQUE;

	/// <summary>
	/// Schema index reader that is able to read/sample a single partition of a partitioned Lucene index.
	/// </summary>
	/// <seealso cref= PartitionedIndexReader </seealso>
	public class SimpleIndexReader : AbstractIndexReader
	{
		 private readonly PartitionSearcher _partitionSearcher;
		 private readonly new IndexDescriptor _descriptor;
		 private readonly IndexSamplingConfig _samplingConfig;
		 private readonly TaskCoordinator _taskCoordinator;

		 public SimpleIndexReader( PartitionSearcher partitionSearcher, IndexDescriptor descriptor, IndexSamplingConfig samplingConfig, TaskCoordinator taskCoordinator ) : base( descriptor )
		 {
			  this._partitionSearcher = partitionSearcher;
			  this._descriptor = descriptor;
			  this._samplingConfig = samplingConfig;
			  this._taskCoordinator = taskCoordinator;
		 }

		 public override IndexSampler CreateSampler()
		 {
			  TaskControl taskControl = _taskCoordinator.newInstance();
			  if ( _descriptor.type() == UNIQUE )
			  {
					return new UniqueLuceneIndexSampler( IndexSearcher, taskControl );
			  }
			  else
			  {
					return new NonUniqueLuceneIndexSampler( IndexSearcher, taskControl, _samplingConfig );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(org.neo4j.storageengine.api.schema.IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] predicates )
		 {
			  Query query = ToLuceneQuery( predicates );
			  client.Initialize( _descriptor, Search( query ).getIndexProgressor( NODE_ID_KEY, client ), predicates, indexOrder, needsValues );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  Query query = ToLuceneQuery( predicates );
			  return Search( query ).getValuesIterator( NODE_ID_KEY );
		 }

		 private DocValuesCollector Search( Query query )
		 {
			  try
			  {
					DocValuesCollector docValuesCollector = new DocValuesCollector();
					IndexSearcher.search( query, docValuesCollector );
					return docValuesCollector;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.search.Query toLuceneQuery(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 private Query ToLuceneQuery( params IndexQuery[] predicates )
		 {
			  IndexQuery predicate = predicates[0];
			  switch ( predicate.Type() )
			  {
			  case exact:
					Value[] values = new Value[predicates.Length];
					for ( int i = 0; i < predicates.Length; i++ )
					{
						 Debug.Assert( predicates[i].Type() == exact, "Exact followed by another query predicate type is not supported at this moment." );
						 values[i] = ( ( IndexQuery.ExactPredicate ) predicates[i] ).value();
					}
					return LuceneDocumentStructure.newSeekQuery( values );
			  case exists:
					foreach ( IndexQuery p in predicates )
					{
						 if ( p.Type() != IndexQuery.IndexQueryType.exists )
						 {
							  throw new IndexNotApplicableKernelException( "Exists followed by another query predicate type is not supported." );
						 }
					}
					return LuceneDocumentStructure.newScanQuery();
			  case range:
					AssertNotComposite( predicates );
					switch ( predicate.ValueGroup() )
					{
					case NUMBER:
						 IndexQuery.NumberRangePredicate np = ( IndexQuery.NumberRangePredicate ) predicate;
						 return LuceneDocumentStructure.newInclusiveNumericRangeSeekQuery( np.From(), np.To() );

					case TEXT:
						 IndexQuery.TextRangePredicate sp = ( IndexQuery.TextRangePredicate ) predicate;
						 return LuceneDocumentStructure.newRangeSeekByStringQuery( sp.From(), sp.FromInclusive(), sp.To(), sp.ToInclusive() );

					default:
						 throw new System.NotSupportedException( format( "Range scans of value group %s are not supported", predicate.ValueGroup() ) );
					}

			  case stringPrefix:
					AssertNotComposite( predicates );
					IndexQuery.StringPrefixPredicate spp = ( IndexQuery.StringPrefixPredicate ) predicate;
					return LuceneDocumentStructure.newRangeSeekByPrefixQuery( spp.Prefix().stringValue() );
			  case stringContains:
					AssertNotComposite( predicates );
					IndexQuery.StringContainsPredicate scp = ( IndexQuery.StringContainsPredicate ) predicate;
					return LuceneDocumentStructure.newWildCardStringQuery( scp.Contains().stringValue() );
			  case stringSuffix:
					AssertNotComposite( predicates );
					IndexQuery.StringSuffixPredicate ssp = ( IndexQuery.StringSuffixPredicate ) predicate;
					return LuceneDocumentStructure.newSuffixStringQuery( ssp.Suffix().stringValue() );
			  default:
					// todo figure out a more specific exception
					throw new Exception( "Index query not supported: " + Arrays.ToString( predicates ) );
			  }
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return false;
		 }

		 /// <summary>
		 /// OBS this implementation can only provide values for properties of type <seealso cref="string"/>.
		 /// Other property types will still be counted as distinct, but {@code client} won't receive <seealso cref="Value"/>
		 /// instances for those. </summary>
		 /// <param name="client"> <seealso cref="IndexProgressor.NodeValueClient"/> to get initialized with this progression. </param>
		 /// <param name="propertyAccessor"> <seealso cref="NodePropertyAccessor"/> for reading property values. </param>
		 /// <param name="needsValues"> whether or not to load string values. </param>
		 public override void DistinctValues( Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  try
			  {
					IndexQuery[] noQueries = new IndexQuery[0];
					BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( client, _descriptor.schema().PropertyIds );
					Fields fields = MultiFields.getFields( IndexSearcher.IndexReader );
					foreach ( ValueEncoding valueEncoding in ValueEncoding.values() )
					{
						 Terms terms = fields.terms( valueEncoding.key() );
						 if ( terms != null )
						 {
							  System.Func<BytesRef, Value> valueMaterializer = valueEncoding == ValueEncoding.String && needsValues ? term => Values.stringValue( term.utf8ToString() ) : term => null;
							  TermsEnum termsIterator = terms.GetEnumerator();
							  if ( valueEncoding == ValueEncoding.Number )
							  {
									termsIterator = NumericUtils.filterPrefixCodedLongs( termsIterator );
							  }
							  multiProgressor.Initialize( _descriptor, new LuceneDistinctValuesProgressor( termsIterator, client, valueMaterializer ), noQueries, IndexOrder.NONE, needsValues );
						 }
					}
					client.Initialize( _descriptor, multiProgressor, noQueries, IndexOrder.NONE, needsValues );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void AssertNotComposite( IndexQuery[] predicates )
		 {
			  Debug.Assert( predicates.Length == 1, "composite indexes not yet supported for this operation" );
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  Query nodeIdQuery = new TermQuery( LuceneDocumentStructure.newTermForChangeOrRemove( nodeId ) );
			  Query valueQuery = LuceneDocumentStructure.newSeekQuery( propertyValues );
			  BooleanQuery.Builder nodeIdAndValueQuery = ( new BooleanQuery.Builder() ).setDisableCoord(true);
			  nodeIdAndValueQuery.add( nodeIdQuery, BooleanClause.Occur.MUST );
			  nodeIdAndValueQuery.add( valueQuery, BooleanClause.Occur.MUST );
			  try
			  {
					TotalHitCountCollector collector = new TotalHitCountCollector();
					IndexSearcher.search( nodeIdAndValueQuery.build(), collector );
					// A <label,propertyKeyId,nodeId> tuple should only match at most a single propertyValue
					return collector.TotalHits;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public override void Close()
		 {
			  try
			  {
					_partitionSearcher.Dispose();
			  }
			  catch ( IOException e )
			  {
					throw new IndexReaderCloseException( e );
			  }
		 }

		 private IndexSearcher IndexSearcher
		 {
			 get
			 {
				  return _partitionSearcher.IndexSearcher;
			 }
		 }
	}

}