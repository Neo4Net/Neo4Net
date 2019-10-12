using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;


	using Resource = Neo4Net.Graphdb.Resource;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using NodeValueIndexCursor = Neo4Net.@internal.Kernel.Api.NodeValueIndexCursor;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using AddedAndRemoved = Neo4Net.Kernel.Impl.Newapi.TxStateIndexChanges.AddedAndRemoved;
	using AddedWithValuesAndRemoved = Neo4Net.Kernel.Impl.Newapi.TxStateIndexChanges.AddedWithValuesAndRemoved;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using IndexProgressor_NodeValueClient = Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.mergeToSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForRangeSeek;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForRangeSeekByPrefix;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForScan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForSeek;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesForSuffixOrContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesWithValuesForRangeSeek;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesWithValuesForRangeSeekByPrefix;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesWithValuesForScan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TxStateIndexChanges.indexUpdatesWithValuesForSuffixOrContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	internal sealed class DefaultNodeValueIndexCursor : IndexCursor<IndexProgressor>, NodeValueIndexCursor, IndexProgressor_NodeValueClient, SortedMergeJoin.Sink
	{
		 private Read _read;
		 private long _node;
		 private IndexQuery[] _query;
		 private Value[] _values;
		 private LongIterator _added = ImmutableEmptyLongIterator.INSTANCE;
		 private IEnumerator<NodeWithPropertyValues> _addedWithValues = Collections.emptyIterator();
		 private LongSet _removed = LongSets.immutable.empty();
		 private bool _needsValues;
		 private IndexOrder _indexOrder;
		 private readonly DefaultCursors _pool;
		 private SortedMergeJoin _sortedMergeJoin = new SortedMergeJoin();

		 internal DefaultNodeValueIndexCursor( DefaultCursors pool )
		 {
			  this._pool = pool;
			  _node = NO_ID;
			  _indexOrder = IndexOrder.NONE;
		 }

		 public override void Initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] query, IndexOrder indexOrder, bool needsValues )
		 {
			  Debug.Assert( query != null );
			  base.Initialize( progressor );
			  _sortedMergeJoin.initialize( indexOrder );

			  this._indexOrder = indexOrder;
			  this._needsValues = needsValues;
			  this._query = query;

			  if ( _read.hasTxStateWithChanges() && query.Length > 0 )
			  {
					IndexQuery firstPredicate = query[0];
					switch ( firstPredicate.Type() )
					{
					case exact:
						 // No need to order, all values are the same
						 this._indexOrder = IndexOrder.NONE;
						 SeekQuery( descriptor, query );
						 break;

					case exists:
						 SetNeedsValuesIfRequiresOrder();
						 ScanQuery( descriptor );
						 break;

					case range:
						 Debug.Assert( query.Length == 1 );
						 SetNeedsValuesIfRequiresOrder();
						 RangeQuery( descriptor, ( IndexQuery.RangePredicate ) firstPredicate );
						 break;

					case stringPrefix:
						 Debug.Assert( query.Length == 1 );
						 SetNeedsValuesIfRequiresOrder();
						 PrefixQuery( descriptor, ( IndexQuery.StringPrefixPredicate ) firstPredicate );
						 break;

					case stringSuffix:
					case stringContains:
						 Debug.Assert( query.Length == 1 );
						 SuffixOrContainsQuery( descriptor, firstPredicate );
						 break;

					default:
						 throw new System.NotSupportedException( "Query not supported: " + Arrays.ToString( query ) );
					}
			  }
		 }

		 /// <summary>
		 /// If we require order, we can only do the merge sort if we also get values.
		 /// This implicitly relies on the fact that if we can get order, we can also get values.
		 /// </summary>
		 private void SetNeedsValuesIfRequiresOrder()
		 {
			  if ( _indexOrder != IndexOrder.NONE )
			  {
					this._needsValues = true;
			  }
		 }

		 private bool IsRemoved( long reference )
		 {
			  return _removed.contains( reference );
		 }

		 public override bool AcceptNode( long reference, Value[] values )
		 {
			  if ( IsRemoved( reference ) )
			  {
					return false;
			  }
			  else
			  {
					this._node = reference;
					this._values = values;
					return true;
			  }
		 }

		 public override bool NeedsValues()
		 {
			  return _needsValues;
		 }

		 public override bool Next()
		 {
			  if ( _indexOrder == IndexOrder.NONE )
			  {
					return NextWithoutOrder();
			  }
			  else
			  {
					return NextWithOrdering();
			  }
		 }

		 private bool NextWithoutOrder()
		 {
			  if ( !_needsValues && _added.hasNext() )
			  {
					this._node = _added.next();
					this._values = null;
					return true;
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  else if ( _needsValues && _addedWithValues.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					NodeWithPropertyValues nodeWithPropertyValues = _addedWithValues.next();
					this._node = nodeWithPropertyValues.NodeId;
					this._values = nodeWithPropertyValues.Values;
					return true;
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  else if ( _added.hasNext() || _addedWithValues.hasNext() )
			  {
					throw new System.InvalidOperationException( "Index cursor cannot have transaction state with values and without values simultaneously" );
			  }
			  else
			  {
					return InnerNext();
			  }
		 }

		 private bool NextWithOrdering()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _sortedMergeJoin.needsA() && _addedWithValues.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					NodeWithPropertyValues nodeWithPropertyValues = _addedWithValues.next();
					_sortedMergeJoin.setA( nodeWithPropertyValues.NodeId, nodeWithPropertyValues.Values );
			  }

			  if ( _sortedMergeJoin.needsB() && InnerNext() )
			  {
					_sortedMergeJoin.setB( _node, _values );
			  }

			  _sortedMergeJoin.next( this );
			  return _node != -1;
		 }

		 public override void AcceptSortedMergeJoin( long nodeId, Value[] values )
		 {
			  this._node = nodeId;
			  this._values = values;
		 }

		 public Read Read
		 {
			 set
			 {
				  this._read = value;
			 }
		 }

		 public override void Node( NodeCursor cursor )
		 {
			  _read.singleNode( _node, cursor );
		 }

		 public override long NodeReference()
		 {
			  return _node;
		 }

		 public override int NumberOfProperties()
		 {
			  return _query == null ? 0 : _query.Length;
		 }

		 public override int PropertyKey( int offset )
		 {
			  return _query[offset].propertyKeyId();
		 }

		 public override bool HasValue()
		 {
			  return _values != null;
		 }

		 public override Value PropertyValue( int offset )
		 {
			  return _values[offset];
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					base.Close();
					this._node = NO_ID;
					this._query = null;
					this._values = null;
					this._read = null;
					this._added = ImmutableEmptyLongIterator.INSTANCE;
					this._addedWithValues = Collections.emptyIterator();
					this._removed = LongSets.immutable.empty();

					_pool.accept( this );
			  }
		 }

		 public override bool Closed
		 {
			 get
			 {
				  return base.Closed;
			 }
		 }

		 public override string ToString()
		 {
			  if ( Closed )
			  {
					return "NodeValueIndexCursor[closed state]";
			  }
			  else
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					string keys = _query == null ? "unknown" : Arrays.ToString( stream( _query ).map( IndexQuery::propertyKeyId ).toArray( int?[]::new ) );
					return "NodeValueIndexCursor[node=" + _node + ", open state with: keys=" + keys +
							  ", values=" + Arrays.ToString( _values ) +
							  ", underlying record=" + base.ToString() + "]";
			  }
		 }

		 private void PrefixQuery( IndexDescriptor descriptor, IndexQuery.StringPrefixPredicate predicate )
		 {
			  TransactionState txState = _read.txState();

			  if ( _needsValues )
			  {
					AddedWithValuesAndRemoved changes = indexUpdatesWithValuesForRangeSeekByPrefix( txState, descriptor, predicate.Prefix(), _indexOrder );
					_addedWithValues = changes.Added.GetEnumerator();
					_removed = _removed( txState, changes.Removed );
			  }
			  else
			  {
					AddedAndRemoved changes = indexUpdatesForRangeSeekByPrefix( txState, descriptor, predicate.Prefix(), _indexOrder );
					_added = changes.Added.longIterator();
					_removed = _removed( txState, changes.Removed );
			  }
		 }

		 private void RangeQuery<T1>( IndexDescriptor descriptor, IndexQuery.RangePredicate<T1> predicate )
		 {
			  TransactionState txState = _read.txState();

			  if ( _needsValues )
			  {
					AddedWithValuesAndRemoved changes = indexUpdatesWithValuesForRangeSeek( txState, descriptor, predicate, _indexOrder );
					_addedWithValues = changes.Added.GetEnumerator();
					_removed = _removed( txState, changes.Removed );
			  }
			  else
			  {
					AddedAndRemoved changes = indexUpdatesForRangeSeek( txState, descriptor, predicate, _indexOrder );
					_added = changes.Added.longIterator();
					_removed = _removed( txState, changes.Removed );
			  }
		 }

		 private void ScanQuery( IndexDescriptor descriptor )
		 {
			  TransactionState txState = _read.txState();

			  if ( _needsValues )
			  {
					AddedWithValuesAndRemoved changes = indexUpdatesWithValuesForScan( txState, descriptor, _indexOrder );
					_addedWithValues = changes.Added.GetEnumerator();
					_removed = _removed( txState, changes.Removed );
			  }
			  else
			  {
					AddedAndRemoved changes = indexUpdatesForScan( txState, descriptor, _indexOrder );
					_added = changes.Added.longIterator();
					_removed = _removed( txState, changes.Removed );
			  }
		 }

		 private void SuffixOrContainsQuery( IndexDescriptor descriptor, IndexQuery query )
		 {
			  TransactionState txState = _read.txState();

			  if ( _needsValues )
			  {
					AddedWithValuesAndRemoved changes = indexUpdatesWithValuesForSuffixOrContains( txState, descriptor, query, _indexOrder );
					_addedWithValues = changes.Added.GetEnumerator();
					_removed = _removed( txState, changes.Removed );
			  }
			  else
			  {
					AddedAndRemoved changes = indexUpdatesForSuffixOrContains( txState, descriptor, query, _indexOrder );
					_added = changes.Added.longIterator();
					_removed = _removed( txState, changes.Removed );
			  }
		 }

		 private void SeekQuery( IndexDescriptor descriptor, IndexQuery[] query )
		 {
			  IndexQuery.ExactPredicate[] exactPreds = AssertOnlyExactPredicates( query );
			  TransactionState txState = _read.txState();

			  AddedAndRemoved changes = indexUpdatesForSeek( txState, descriptor, IndexQuery.asValueTuple( exactPreds ) );
			  _added = changes.Added.longIterator();
			  _removed = _removed( txState, changes.Removed );
		 }

		 private LongSet Removed( TransactionState txState, LongSet removedFromIndex )
		 {
			  return mergeToSet( txState.AddedAndRemovedNodes().Removed, removedFromIndex );
		 }

		 private static IndexQuery.ExactPredicate[] AssertOnlyExactPredicates( IndexQuery[] predicates )
		 {
			  IndexQuery.ExactPredicate[] exactPredicates;
			  if ( predicates.GetType() == typeof(IndexQuery.ExactPredicate[]) )
			  {
					exactPredicates = ( IndexQuery.ExactPredicate[] ) predicates;
			  }
			  else
			  {
					exactPredicates = new IndexQuery.ExactPredicate[predicates.Length];
					for ( int i = 0; i < predicates.Length; i++ )
					{
						 if ( predicates[i] is IndexQuery.ExactPredicate )
						 {
							  exactPredicates[i] = ( IndexQuery.ExactPredicate ) predicates[i];
						 }
						 else
						 {
							  // TODO: what to throw?
							  throw new System.ArgumentException( "Query not supported: " + Arrays.ToString( predicates ) );
						 }
					}
			  }
			  return exactPredicates;
		 }

		 public void Release()
		 {
			  // nothing to do
		 }
	}

}