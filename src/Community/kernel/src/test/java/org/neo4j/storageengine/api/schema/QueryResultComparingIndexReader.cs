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
namespace Neo4Net.Storageengine.Api.schema
{
	using PrimitiveLongResourceCollections = Neo4Net.Collections.PrimitiveLongResourceCollections;
	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using NodeValueIterator = Neo4Net.Kernel.Impl.Index.Schema.NodeValueIterator;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	/// <summary>
	/// <seealso cref="IndexReader"/> that executes and compares results from both query methods, as long as they should exist, when using the either of the two query methods.
	/// </summary>
	public class QueryResultComparingIndexReader : IndexReader
	{
		 private readonly IndexReader _actual;

		 public QueryResultComparingIndexReader( IndexReader actual )
		 {
			  this._actual = actual;
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  return _actual.countIndexedNodes( nodeId, propertyKeyIds, propertyValues );
		 }

		 public override IndexSampler CreateSampler()
		 {
			  return _actual.createSampler();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  PrimitiveLongResourceIterator mainResult = _actual.query( predicates );

			  // Also call the other query method and bake comparison from it into a wrapped version of this iterator
			  NodeValueIterator otherResult = new NodeValueIterator();
			  _actual.query( otherResult, IndexOrder.NONE, false, predicates );
			  return new PrimitiveLongBaseResourceIteratorAnonymousInnerClass( this, mainResult, otherResult );
		 }

		 private class PrimitiveLongBaseResourceIteratorAnonymousInnerClass : PrimitiveLongResourceCollections.PrimitiveLongBaseResourceIterator
		 {
			 private readonly QueryResultComparingIndexReader _outerInstance;

			 private PrimitiveLongResourceIterator _mainResult;
			 private NodeValueIterator _otherResult;

			 public PrimitiveLongBaseResourceIteratorAnonymousInnerClass( QueryResultComparingIndexReader outerInstance, PrimitiveLongResourceIterator mainResult, NodeValueIterator otherResult ) : base( mainResult )
			 {
				 this.outerInstance = outerInstance;
				 this._mainResult = mainResult;
				 this._otherResult = otherResult;
			 }

			 protected internal override bool fetchNext()
			 {
				  if ( _mainResult.hasNext() )
				  {
						long mainValue = _mainResult.next();
						if ( !_otherResult.hasNext() )
						{
							 throw new System.InvalidOperationException( format( "Legacy query method returned %d, but new query method didn't have more values in it", mainValue ) );
						}
						long otherValue = _otherResult.next();
						if ( mainValue != otherValue )
						{
							 throw new System.InvalidOperationException( format( "Query methods disagreeing on next value legacy:%d new:%d", mainValue, otherValue ) );
						}
						return next( mainValue );
				  }
				  else if ( _otherResult.hasNext() )
				  {
						throw new System.InvalidOperationException( format( "Legacy query method exhausted, but new query method had more %d", _otherResult.next() ) );
				  }
				  return false;
			 }

			 public override void close()
			 {
				  base.close();
				  _otherResult.close();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  // Also call the other query method and bake comparison from it into a wrapped version of this iterator
			  PrimitiveLongResourceIterator otherResult = _actual.query( query );

			  // This is a client which gets driven by the client, such that it can know when there are no more values in it.
			  // Therefore we can hook in correct comparison on this type of client.
			  // Also call the other query method and bake comparison from it into a wrapped version of this iterator
			  IndexProgressor_NodeValueClient wrappedClient = new IndexProgressor_NodeValueClientAnonymousInnerClass( this, client, indexOrder, needsValues, query, otherResult );

			  _actual.query( wrappedClient, indexOrder, needsValues, query );
		 }

		 private class IndexProgressor_NodeValueClientAnonymousInnerClass : IndexProgressor_NodeValueClient
		 {
			 private readonly QueryResultComparingIndexReader _outerInstance;

			 private Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient _client;
			 private IndexOrder _indexOrder;
			 private bool _needsValues;
			 private IndexQuery[] _query;
			 private PrimitiveLongResourceIterator _otherResult;

			 public IndexProgressor_NodeValueClientAnonymousInnerClass( QueryResultComparingIndexReader outerInstance, Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, IndexQuery[] query, PrimitiveLongResourceIterator otherResult )
			 {
				 this.outerInstance = outerInstance;
				 this._client = client;
				 this._indexOrder = indexOrder;
				 this._needsValues = needsValues;
				 this._query = query;
				 this._otherResult = otherResult;
			 }

			 private long mainValue;

			 public void initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] query, IndexOrder indexOrder, bool needsValues )
			 {
				  IndexProgressor wrappedProgressor = new IndexProgressorAnonymousInnerClass( this, progressor );

				  _client.initialize( descriptor, wrappedProgressor, query, indexOrder, needsValues );
			 }

			 private class IndexProgressorAnonymousInnerClass : IndexProgressor
			 {
				 private readonly IndexProgressor_NodeValueClientAnonymousInnerClass _outerInstance;

				 private Neo4Net.Storageengine.Api.schema.IndexProgressor _progressor;

				 public IndexProgressorAnonymousInnerClass( IndexProgressor_NodeValueClientAnonymousInnerClass outerInstance, Neo4Net.Storageengine.Api.schema.IndexProgressor progressor )
				 {
					 this.outerInstance = outerInstance;
					 this._progressor = progressor;
				 }

				 public bool next()
				 {
					  mainValue = NO_ID;
					  if ( _progressor.next() )
					  {
							if ( !_outerInstance.otherResult.hasNext() )
							{
								 throw new System.InvalidOperationException( format( "new query method returned %d, but legacy query method didn't have more values in it", mainValue ) );
							}
							long otherValue = _outerInstance.otherResult.next();
							if ( mainValue != otherValue )
							{
								 throw new System.InvalidOperationException( format( "Query methods disagreeing on next value new:%d legacy:%d", mainValue, otherValue ) );
							}
							return true;
					  }
					  else if ( _outerInstance.otherResult.hasNext() )
					  {
							throw new System.InvalidOperationException( format( "New query method exhausted, but legacy query method had more %d", _outerInstance.otherResult.next() ) );
					  }
					  return false;
				 }

				 public void close()
				 {
					  _progressor.close();
					  _outerInstance.otherResult.close();
				 }
			 }

			 public bool acceptNode( long reference, params Value[] values )
			 {
				  mainValue = reference;
				  return _client.acceptNode( reference, values );
			 }

			 public bool needsValues()
			 {
				  return _client.needsValues();
			 }
		 }

		 public override void DistinctValues( IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  _actual.distinctValues( client, propertyAccessor, needsValues );
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return _actual.hasFullValuePrecision( predicates );
		 }

		 public override void Close()
		 {
			  _actual.close();
		 }
	}

}