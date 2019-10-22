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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	using LabelSet = Neo4Net.Internal.Kernel.Api.LabelSet;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using RelationshipGroupCursor = Neo4Net.Internal.Kernel.Api.RelationshipGroupCursor;
	using RelationshipTraversalCursor = Neo4Net.Internal.Kernel.Api.RelationshipTraversalCursor;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;
	using LongDiffSets = Neo4Net.Storageengine.Api.txstate.LongDiffSets;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	internal class DefaultNodeCursor : NodeCursor
	{
		 private Read _read;
		 private HasChanges _hasChanges = HasChanges.Maybe;
		 private LongIterator _addedNodes;
		 private StorageNodeCursor _storeCursor;
		 private long _single;

		 private readonly DefaultCursors _pool;

		 internal DefaultNodeCursor( DefaultCursors pool, StorageNodeCursor storeCursor )
		 {
			  this._pool = pool;
			  this._storeCursor = storeCursor;
		 }

		 internal virtual void Scan( Read read )
		 {
			  _storeCursor.scan();
			  this._read = read;
			  this._single = NO_ID;
			  this._hasChanges = HasChanges.Maybe;
			  this._addedNodes = ImmutableEmptyLongIterator.INSTANCE;
		 }

		 internal virtual void Single( long reference, Read read )
		 {
			  _storeCursor.single( reference );
			  this._read = read;
			  this._single = reference;
			  this._hasChanges = HasChanges.Maybe;
			  this._addedNodes = ImmutableEmptyLongIterator.INSTANCE;
		 }

		 public override long NodeReference()
		 {
			  return _storeCursor.entityReference();
		 }

		 public override LabelSet Labels()
		 {
			  if ( HasChanges() )
			  {
					TransactionState txState = _read.txState();
					if ( txState.NodeIsAddedInThisTx( _storeCursor.entityReference() ) )
					{
						 //Node just added, no reason to go down to store and check
						 return Labels.From( txState.NodeStateLabelDiffSets( _storeCursor.entityReference() ).Added );
					}
					else
					{
						 //Get labels from store and put in intSet, unfortunately we get longs back
						 long[] longs = _storeCursor.labels();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet labels = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
						 MutableLongSet labels = new LongHashSet();
						 foreach ( long labelToken in longs )
						 {
							  labels.add( labelToken );
						 }

						 //Augment what was found in store with what we have in tx state
						 return Labels.From( txState.AugmentLabels( labels, txState.GetNodeState( _storeCursor.entityReference() ) ) );
					}
			  }
			  else
			  {
					//Nothing in tx state, just read the data.
					return Labels.From( _storeCursor.labels() );
			  }
		 }

		 public override bool HasLabel( int label )
		 {
			  if ( HasChanges() )
			  {
					TransactionState txState = _read.txState();
					LongDiffSets diffSets = txState.NodeStateLabelDiffSets( _storeCursor.entityReference() );
					if ( diffSets.Added.contains( label ) )
					{
						 return true;
					}
					if ( diffSets.Removed.contains( label ) )
					{
						 return false;
					}
			  }

			  //Get labels from store and put in intSet, unfortunately we get longs back
			  return _storeCursor.hasLabel( label );
		 }

		 public override void Relationships( RelationshipGroupCursor cursor )
		 {
			  ( ( DefaultRelationshipGroupCursor ) cursor ).Init( NodeReference(), RelationshipGroupReference(), _read );
		 }

		 public override void AllRelationships( RelationshipTraversalCursor cursor )
		 {
			  ( ( DefaultRelationshipTraversalCursor ) cursor ).Init( NodeReference(), AllRelationshipsReference(), _read );
		 }

		 public override void Properties( PropertyCursor cursor )
		 {
			  ( ( DefaultPropertyCursor ) cursor ).InitNode( NodeReference(), PropertiesReference(), _read, _read );
		 }

		 public override long RelationshipGroupReference()
		 {
			  return _storeCursor.relationshipGroupReference();
		 }

		 public override long AllRelationshipsReference()
		 {
			  return _storeCursor.allRelationshipsReference();
		 }

		 public override long PropertiesReference()
		 {
			  return _storeCursor.propertiesReference();
		 }

		 public virtual bool Dense
		 {
			 get
			 {
				  return _storeCursor.Dense;
			 }
		 }

		 public override bool Next()
		 {
			  // Check tx state
			  bool hasChanges = hasChanges();

			  if ( hasChanges && _addedNodes.hasNext() )
			  {
					_storeCursor.Current = _addedNodes.next();
					return true;
			  }

			  while ( _storeCursor.next() )
			  {
					if ( !hasChanges || !_read.txState().nodeIsDeletedInThisTx(_storeCursor.entityReference()) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					_read = null;
					_hasChanges = HasChanges.Maybe;
					_addedNodes = ImmutableEmptyLongIterator.INSTANCE;
					_storeCursor.reset();

					_pool.accept( this );
			  }
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return _read == null;
			 }
		 }

		 /// <summary>
		 /// NodeCursor should only see changes that are there from the beginning
		 /// otherwise it will not be stable.
		 /// </summary>
		 private bool HasChanges()
		 {
			  switch ( _hasChanges )
			  {
			  case Neo4Net.Kernel.Impl.Newapi.HasChanges.Maybe:
					bool changes = _read.hasTxStateWithChanges();
					if ( changes )
					{
						 if ( _single != NO_ID )
						 {
							  _addedNodes = _read.txState().nodeIsAddedInThisTx(_single) ? LongSets.immutable.of(_single).longIterator() : ImmutableEmptyLongIterator.INSTANCE;
						 }
						 else
						 {
							  _addedNodes = _read.txState().addedAndRemovedNodes().Added.freeze().longIterator();
						 }
						 _hasChanges = HasChanges.Yes;
					}
					else
					{
						 _hasChanges = HasChanges.No;
					}
					return changes;
			  case Neo4Net.Kernel.Impl.Newapi.HasChanges.Yes:
					return true;
			  case Neo4Net.Kernel.Impl.Newapi.HasChanges.No:
					return false;
			  default:
					throw new System.InvalidOperationException( "Style guide, why are you making me do this" );
			  }
		 }

		 public override string ToString()
		 {
			  if ( Closed )
			  {
					return "NodeCursor[closed state]";
			  }
			  else
			  {
					return "NodeCursor[id=" + NodeReference() + ", " + _storeCursor.ToString() + "]";
			  }
		 }

		 internal virtual void Release()
		 {
			  _storeCursor.close();
		 }
	}

}