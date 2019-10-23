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
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;

	using LabelSet = Neo4Net.Kernel.Api.Internal.LabelSet;
	using NodeCursor = Neo4Net.Kernel.Api.Internal.NodeCursor;
	using NodeLabelIndexCursor = Neo4Net.Kernel.Api.Internal.NodeLabelIndexCursor;
	using LabelScanValueIndexProgressor = Neo4Net.Kernel.impl.index.labelscan.LabelScanValueIndexProgressor;
	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;
	using IndexProgressor_NodeLabelClient = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeLabelClient;
	using LongDiffSets = Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.PrimitiveLongCollections.mergeToSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	internal class DefaultNodeLabelIndexCursor : IndexCursor<IndexProgressor>, NodeLabelIndexCursor, IndexProgressor_NodeLabelClient
	{
		 private Read _read;
		 private long _node;
		 private LabelSet _labels;
		 private LongIterator _added;
		 private LongSet _removed;

		 private readonly DefaultCursors _pool;

		 internal DefaultNodeLabelIndexCursor( DefaultCursors pool )
		 {
			  this._pool = pool;
			  _node = NO_ID;
		 }

		 public override void Scan( IndexProgressor progressor, bool providesLabels, int label )
		 {
			  base.Initialize( progressor );
			  if ( _read.hasTxStateWithChanges() )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets changes = read.txState().nodesWithLabelChanged(label);
					LongDiffSets changes = _read.txState().nodesWithLabelChanged(label);
					_added = changes.Augment( ImmutableEmptyLongIterator.INSTANCE );
					_removed = mergeToSet( _read.txState().addedAndRemovedNodes().Removed, changes.Removed );
			  }
		 }

		 public override void UnionScan( IndexProgressor progressor, bool providesLabels, params int[] labels )
		 {
			  //TODO: Currently we don't have a good way of handling this in the tx state
			  //The problem is this case:
			  //Given a node with label :A
			  //we remove label A in a transaction and follow that by
			  //a scan of `:A and :B`. In order to figure this out we need
			  //to check both tx state and disk, which we currently don't.
			  throw new System.NotSupportedException();
		 }

		 public override void IntersectionScan( IndexProgressor progressor, bool providesLabels, params int[] labels )
		 {
			  //TODO: Currently we don't have a good way of handling this in the tx state
			  //The problem is for the nodes where some - but not all of the labels - are
			  //added in the transaction. For these we need to go to disk and check if they
			  //have the missing labels and hence return them or if not discard them.
			  throw new System.NotSupportedException();
		 }

		 public override bool AcceptNode( long reference, LabelSet labels )
		 {
			  if ( IsRemoved( reference ) )
			  {
					return false;
			  }
			  else
			  {
					this._node = reference;
					this._labels = labels;

					return true;
			  }
		 }

		 public override bool Next()
		 {
			  if ( _added != null && _added.hasNext() )
			  {
					this._node = _added.next();
					return true;
			  }
			  else
			  {
					return InnerNext();
			  }
		 }

		 public virtual Read Read
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

		 public override LabelSet Labels()
		 {
			  return _labels;
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					base.Close();
					_node = NO_ID;
					_labels = null;
					_read = null;
					_removed = null;

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
					return "NodeLabelIndexCursor[closed state]";
			  }
			  else
			  {
					return "NodeLabelIndexCursor[node=" + _node + ", labels= " + _labels +
							  ", underlying record=" + base.ToString() + "]";
			  }
		 }

		 private bool IsRemoved( long reference )
		 {
			  return _removed != null && _removed.contains( reference );
		 }

		 public virtual void Release()
		 {
			  // nothing to do
		 }
	}

}