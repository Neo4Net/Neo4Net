﻿/*
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
namespace Org.Neo4j.Kernel.api.txstate
{
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;

	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CountsRecordState = Org.Neo4j.Kernel.Impl.Api.CountsRecordState;
	using DegreeVisitor = Org.Neo4j.Kernel.Impl.Api.DegreeVisitor;
	using RelationshipDataExtractor = Org.Neo4j.Kernel.Impl.Api.RelationshipDataExtractor;
	using StorageNodeCursor = Org.Neo4j.Storageengine.Api.StorageNodeCursor;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using StorageRelationshipGroupCursor = Org.Neo4j.Storageengine.Api.StorageRelationshipGroupCursor;
	using LongDiffSets = Org.Neo4j.Storageengine.Api.txstate.LongDiffSets;
	using ReadableTransactionState = Org.Neo4j.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.ANY_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.ANY_RELATIONSHIP_TYPE;

	public class TransactionCountingStateVisitor : Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor_Delegator
	{
		 private readonly RelationshipDataExtractor _edge = new RelationshipDataExtractor();
		 private readonly StorageReader _storageReader;
		 private readonly CountsRecordState _counts;
		 private readonly ReadableTransactionState _txState;
		 private readonly StorageNodeCursor _nodeCursor;
		 private readonly StorageRelationshipGroupCursor _groupCursor;

		 public TransactionCountingStateVisitor( TxStateVisitor next, StorageReader storageReader, ReadableTransactionState txState, CountsRecordState counts ) : base( next )
		 {
			  this._storageReader = storageReader;
			  this._txState = txState;
			  this._counts = counts;
			  this._nodeCursor = storageReader.AllocateNodeCursor();
			  this._groupCursor = storageReader.AllocateRelationshipGroupCursor();
		 }

		 public override void VisitCreatedNode( long id )
		 {
			  _counts.incrementNodeCount( ANY_LABEL, 1 );
			  base.VisitCreatedNode( id );
		 }

		 public override void VisitDeletedNode( long id )
		 {
			  _counts.incrementNodeCount( ANY_LABEL, -1 );
			  _nodeCursor.single( id );
			  if ( _nodeCursor.next() )
			  {
					DecrementCountForLabelsAndRelationships( _nodeCursor );
			  }
			  base.VisitDeletedNode( id );
		 }

		 private void DecrementCountForLabelsAndRelationships( StorageNodeCursor node )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] labelIds = node.labels();
			  long[] labelIds = node.Labels();
			  foreach ( long labelId in labelIds )
			  {
					_counts.incrementNodeCount( labelId, -1 );
			  }

			  VisitDegrees( node, ( type, @out, @in ) => updateRelationshipsCountsFromDegrees( labelIds, type, -@out, -@in ) );
		 }

		 private void VisitDegrees( StorageNodeCursor node, DegreeVisitor visitor )
		 {
			  _groupCursor.init( node.EntityReference(), node.RelationshipGroupReference() );
			  while ( _groupCursor.next() )
			  {
					int loopCount = _groupCursor.loopCount();
					visitor.VisitDegree( _groupCursor.type(), _groupCursor.outgoingCount() + loopCount, _groupCursor.incomingCount() + loopCount );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitCreatedRelationship(long id, int type, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException
		 public override void VisitCreatedRelationship( long id, int type, long startNode, long endNode )
		 {
			  UpdateRelationshipCount( startNode, type, endNode, 1 );
			  base.VisitCreatedRelationship( id, type, startNode, endNode );
		 }

		 public override void VisitDeletedRelationship( long id )
		 {
			  try
			  {
					_storageReader.relationshipVisit( id, _edge );
					UpdateRelationshipCount( _edge.startNode(), _edge.type(), _edge.endNode(), -1 );
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new System.InvalidOperationException( "Relationship being deleted should exist along with its nodes.", e );
			  }
			  base.VisitDeletedRelationship( id );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitNodeLabelChanges(long id, final org.eclipse.collections.api.set.primitive.LongSet added, final org.eclipse.collections.api.set.primitive.LongSet removed) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override void VisitNodeLabelChanges( long id, LongSet added, LongSet removed )
		 {
			  // update counts
			  if ( !( added.Empty && removed.Empty ) )
			  {
					added.each( label => _counts.incrementNodeCount( label, 1 ) );
					removed.each( label => _counts.incrementNodeCount( label, -1 ) );
					// get the relationship counts from *before* this transaction,
					// the relationship changes will compensate for what happens during the transaction

					_nodeCursor.single( id );
					if ( _nodeCursor.next() )
					{
						 VisitDegrees(_nodeCursor, (type, @out, @in) =>
						 {
						  added.forEach( label => updateRelationshipsCountsFromDegrees( type, label, @out, @in ) );
						  removed.forEach( label => updateRelationshipsCountsFromDegrees( type, label, -@out, -@in ) );
						 });
					}
			  }
			  base.VisitNodeLabelChanges( id, added, removed );
		 }

		 private void UpdateRelationshipsCountsFromDegrees( long[] labels, int type, long outgoing, long incoming )
		 {
			  foreach ( long label in labels )
			  {
					UpdateRelationshipsCountsFromDegrees( type, label, outgoing, incoming );
			  }
		 }

		 private bool UpdateRelationshipsCountsFromDegrees( int type, long label, long outgoing, long incoming )
		 {
			  // untyped
			  _counts.incrementRelationshipCount( label, ANY_RELATIONSHIP_TYPE, ANY_LABEL, outgoing );
			  _counts.incrementRelationshipCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, label, incoming );
			  // typed
			  _counts.incrementRelationshipCount( label, type, ANY_LABEL, outgoing );
			  _counts.incrementRelationshipCount( ANY_LABEL, type, label, incoming );
			  return false;
		 }

		 private void UpdateRelationshipCount( long startNode, int type, long endNode, int delta )
		 {
			  updateRelationshipsCountsFromDegrees( type, ANY_LABEL, delta, 0 );
			  VisitLabels( startNode, labelId => updateRelationshipsCountsFromDegrees( type, labelId, delta, 0 ) );
			  VisitLabels( endNode, labelId => updateRelationshipsCountsFromDegrees( type, labelId, 0, delta ) );
		 }

		 private void VisitLabels( long nodeId, System.Action<long> visitor )
		 {
			  // This transaction state visitor doesn't have access to higher level cursors that combine store- and tx-state,
			  // but however has access to the two individually, and so does this combining here directly.
			  if ( _txState.nodeIsDeletedInThisTx( nodeId ) )
			  {
					return;
			  }

			  if ( _txState.nodeIsAddedInThisTx( nodeId ) )
			  {
					_txState.getNodeState( nodeId ).labelDiffSets().Added.forEach(visitor.accept);
			  }
			  else
			  {
					_nodeCursor.single( nodeId );
					if ( _nodeCursor.next() )
					{
						 long[] labels = _nodeCursor.labels();
						 LongDiffSets labelDiff = _txState.getNodeState( nodeId ).labelDiffSets();
						 labelDiff.Added.forEach( visitor.accept );
						 foreach ( long label in labels )
						 {
							  if ( !labelDiff.IsRemoved( label ) )
							  {
									visitor( label );
							  }
						 }
					}
			  }
		 }
	}

}