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
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	using Neo4Net.Functions;
	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using RelationshipSelectionCursor = Neo4Net.@internal.Kernel.Api.helpers.RelationshipSelectionCursor;
	using RelationshipSelections = Neo4Net.@internal.Kernel.Api.helpers.RelationshipSelections;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.NO_SUCH_RELATIONSHIP;

	internal class TwoPhaseNodeForRelationshipLocking
	{
		 private readonly ThrowingConsumer<long, KernelException> _relIdAction;

		 private long _firstRelId;
		 private long[] _sortedNodeIds;
		 private static readonly long[] _empty = new long[0];
		 private readonly Neo4Net.Kernel.impl.locking.Locks_Client _locks;
		 private readonly LockTracer _lockTracer;

		 internal TwoPhaseNodeForRelationshipLocking( ThrowingConsumer<long, KernelException> relIdAction, Neo4Net.Kernel.impl.locking.Locks_Client locks, LockTracer lockTracer )
		 {
			  this._relIdAction = relIdAction;
			  this._locks = locks;
			  this._lockTracer = lockTracer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void lockAllNodesAndConsumeRelationships(long nodeId, final org.neo4j.internal.kernel.api.Transaction transaction, org.neo4j.internal.kernel.api.NodeCursor nodes) throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 internal virtual void LockAllNodesAndConsumeRelationships( long nodeId, Transaction transaction, NodeCursor nodes )
		 {
			  bool retry;
			  do
			  {
					retry = false;
					_firstRelId = NO_SUCH_RELATIONSHIP;

					// lock all the nodes involved by following the node id ordering
					CollectAndSortNodeIds( nodeId, transaction, nodes );
					LockAllNodes( _sortedNodeIds );

					// perform the action on each relationship, we will retry if the the relationship iterator contains
					// new relationships
					Neo4Net.@internal.Kernel.Api.Read read = transaction.DataRead();
					read.SingleNode( nodeId, nodes );
					//if the node is not there, someone else probably deleted it, just ignore
					if ( nodes.Next() )
					{
						 using ( RelationshipSelectionCursor rels = RelationshipSelections.allCursor( transaction.Cursors(), nodes, null ) )
						 {
							  bool first = true;
							  while ( rels.Next() && !retry )
							  {
									retry = PerformAction( rels.RelationshipReference(), first );
									first = false;
							  }
						 }
					}
			  } while ( retry );
		 }

		 private void CollectAndSortNodeIds( long nodeId, Transaction transaction, NodeCursor nodes )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet nodeIdSet = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
			  MutableLongSet nodeIdSet = new LongHashSet();
			  nodeIdSet.add( nodeId );

			  Neo4Net.@internal.Kernel.Api.Read read = transaction.DataRead();
			  read.SingleNode( nodeId, nodes );
			  if ( !nodes.Next() )
			  {
					this._sortedNodeIds = _empty;
					return;
			  }
			  using ( RelationshipSelectionCursor rels = RelationshipSelections.allCursor( transaction.Cursors(), nodes, null ) )
			  {
					while ( rels.Next() )
					{
						 if ( _firstRelId == NO_SUCH_RELATIONSHIP )
						 {
							  _firstRelId = rels.RelationshipReference();
						 }

						 nodeIdSet.add( rels.SourceNodeReference() );
						 nodeIdSet.add( rels.TargetNodeReference() );
					}
			  }

			  this._sortedNodeIds = nodeIdSet.toSortedArray();
		 }

		 private void LockAllNodes( long[] nodeIds )
		 {
			  _locks.acquireExclusive( _lockTracer, ResourceTypes.NODE, nodeIds );
		 }

		 private void UnlockAllNodes( long[] nodeIds )
		 {
			  _locks.releaseExclusive( ResourceTypes.NODE, nodeIds );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean performAction(long rel, boolean first) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private bool PerformAction( long rel, bool first )
		 {
			  if ( first )
			  {
					if ( rel != _firstRelId )
					{
						 // if the first relationship is not the same someone added some new rels, so we need to
						 // lock them all again
						 UnlockAllNodes( _sortedNodeIds );
						 _sortedNodeIds = null;
						 return true;
					}
			  }

			  _relIdAction.accept( rel );
			  return false;
		 }
	}

}