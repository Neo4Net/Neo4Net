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
namespace Org.Neo4j.@internal.Kernel.Api.helpers
{
	using Org.Neo4j.@internal.Kernel.Api;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class StubRead : Read
	{
		 public override void NodeIndexSeek( IndexReference index, NodeValueIndexCursor cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void NodeIndexDistinctValues( IndexReference index, NodeValueIndexCursor cursor, bool needsValues )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long lockingNodeUniqueIndexSeek(org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.internal.kernel.api.IndexQuery.ExactPredicate... predicates) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override long LockingNodeUniqueIndexSeek( IndexReference index, params IndexQuery.ExactPredicate[] predicates )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void NodeIndexScan( IndexReference index, NodeValueIndexCursor cursor, IndexOrder indexOrder, bool needsValues )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void NodeLabelScan( int label, NodeLabelIndexCursor cursor )
		 {
			  ( ( StubNodeLabelIndexCursor ) cursor ).Initialize( label );
		 }

		 public override void NodeLabelUnionScan( NodeLabelIndexCursor cursor, params int[] labels )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void NodeLabelIntersectionScan( NodeLabelIndexCursor cursor, params int[] labels )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override Scan<NodeLabelIndexCursor> NodeLabelScan( int label )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void AllNodesScan( NodeCursor cursor )
		 {
			  ( ( StubNodeCursor ) cursor ).Scan();
		 }

		 public override Scan<NodeCursor> AllNodesScan()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void SingleNode( long reference, NodeCursor cursor )
		 {
			  ( ( StubNodeCursor ) cursor ).Single( reference );
		 }

		 public override bool NodeExists( long id )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long CountsForNode( int labelId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long CountsForNodeWithoutTxState( int labelId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long CountsForRelationship( int startLabelId, int typeId, int endLabelId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long CountsForRelationshipWithoutTxState( int startLabelId, int typeId, int endLabelId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long NodesGetCount()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long RelationshipsGetCount()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void SingleRelationship( long reference, RelationshipScanCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool RelationshipExists( long reference )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void AllRelationshipsScan( RelationshipScanCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override Scan<RelationshipScanCursor> AllRelationshipsScan()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void RelationshipTypeScan( int type, RelationshipScanCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Relationships( long nodeReference, long reference, RelationshipTraversalCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override Scan<RelationshipScanCursor> RelationshipTypeScan( int type )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void NodeProperties( long nodeReference, long reference, PropertyCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void RelationshipProperties( long relationshipReference, long reference, PropertyCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void RelationshipGroups( long nodeReference, long reference, RelationshipGroupCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool NodeDeletedInTransaction( long node )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool RelationshipDeletedInTransaction( long relationship )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override Value NodePropertyChangeInTransactionOrNull( long node, int propertyKeyId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void GraphProperties( PropertyCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void FutureNodeReferenceRead( long reference )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void FutureRelationshipsReferenceRead( long reference )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void FutureNodePropertyReferenceRead( long reference )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void FutureRelationshipPropertyReferenceRead( long reference )
		 {
			  throw new System.NotSupportedException();
		 }
	}

}