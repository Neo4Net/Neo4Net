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
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;

	using RelationshipGroupCursor = Neo4Net.@internal.Kernel.Api.RelationshipGroupCursor;
	using RelationshipTraversalCursor = Neo4Net.@internal.Kernel.Api.RelationshipTraversalCursor;
	using RelationshipDirection = Neo4Net.Storageengine.Api.RelationshipDirection;
	using StorageRelationshipGroupCursor = Neo4Net.Storageengine.Api.StorageRelationshipGroupCursor;
	using NodeState = Neo4Net.Storageengine.Api.txstate.NodeState;
	using RelationshipState = Neo4Net.Storageengine.Api.txstate.RelationshipState;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.encodeNoIncomingRels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.encodeNoLoopRels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.encodeNoOutgoingRels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	internal class DefaultRelationshipGroupCursor : RelationshipGroupCursor
	{
		 private Read _read;
		 private readonly DefaultCursors _pool;

		 private StorageRelationshipGroupCursor _storeCursor;
		 private bool _hasCheckedTxState;
		 private readonly MutableIntSet _txTypes = new IntHashSet();
		 private IntIterator _txTypeIterator;

		 internal DefaultRelationshipGroupCursor( DefaultCursors pool, StorageRelationshipGroupCursor storeCursor )
		 {
			  this._pool = pool;
			  this._storeCursor = storeCursor;
		 }

		 internal virtual void Init( long nodeReference, long reference, Read read )
		 {
			  _storeCursor.init( nodeReference, reference );
			  this._txTypes.clear();
			  this._txTypeIterator = null;
			  this._hasCheckedTxState = false;
			  this._read = read;
		 }

		 public override Neo4Net.@internal.Kernel.Api.RelationshipGroupCursor_Position Suspend()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Resume( Neo4Net.@internal.Kernel.Api.RelationshipGroupCursor_Position position )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool Next()
		 {
			  //We need to check tx state if there are new types added
			  //on the first call of next
			  if ( !_hasCheckedTxState )
			  {
					CheckTxStateForUpdates();
					_hasCheckedTxState = true;
			  }

			  if ( !_storeCursor.next() )
			  {
					//We have now run out of groups from the store, however there may still
					//be new types that was added in the transaction that we haven't visited yet.
					return NextFromTxState();
			  }

			  MarkTypeAsSeen( Type() );
			  return true;
		 }

		 private bool NextFromTxState()
		 {
			  if ( _txTypeIterator == null && !_txTypes.Empty )
			  {
					_txTypeIterator = _txTypes.intIterator();
					//here it may be tempting to do txTypes.clear()
					//however that will also clear the iterator
			  }
			  if ( _txTypeIterator != null && _txTypeIterator.hasNext() )
			  {
					_storeCursor.setCurrent( _txTypeIterator.next(), NO_ID, NO_ID, NO_ID );
					return true;
			  }
			  return false;
		 }

		 /// <summary>
		 /// Marks the given type as already seen </summary>
		 /// <param name="type"> the type we have seen </param>
		 private void MarkTypeAsSeen( int type )
		 {
			  _txTypes.remove( type );
		 }

		 /// <summary>
		 /// Store all types that was added in the transaction for the current node
		 /// </summary>
		 private void CheckTxStateForUpdates()
		 {
			  if ( _read.hasTxStateWithChanges() )
			  {
					NodeState nodeState = _read.txState().getNodeState(_storeCursor.OwningNode);
					LongIterator addedRelationships = nodeState.AddedRelationships;
					while ( addedRelationships.hasNext() )
					{
						 RelationshipState relationshipState = _read.txState().getRelationshipState(addedRelationships.next());
						 relationshipState.Accept( ( relationshipId, typeId, startNodeId, endNodeId ) => _txTypes.add( typeId ) );
					}
			  }
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					_read = null;
					_storeCursor.reset();

					if ( _pool != null )
					{
						 _pool.accept( this );
					}
			  }
		 }

		 public override int Type()
		 {
			  return _storeCursor.type();
		 }

		 public override int OutgoingCount()
		 {
			  int count = _storeCursor.outgoingCount();
			  return _read.hasTxStateWithChanges() ? _read.txState().getNodeState(_storeCursor.OwningNode).augmentDegree(RelationshipDirection.OUTGOING, count, _storeCursor.type()) : count;
		 }

		 public override int IncomingCount()
		 {
			  int count = _storeCursor.incomingCount();
			  return _read.hasTxStateWithChanges() ? _read.txState().getNodeState(_storeCursor.OwningNode).augmentDegree(RelationshipDirection.INCOMING, count, _storeCursor.type()) : count;
		 }

		 public override int LoopCount()
		 {
			  int count = _storeCursor.loopCount();
			  return _read.hasTxStateWithChanges() ? _read.txState().getNodeState(_storeCursor.OwningNode).augmentDegree(RelationshipDirection.LOOP, count, _storeCursor.type()) : count;

		 }

		 public override void Outgoing( RelationshipTraversalCursor cursor )
		 {
			  ( ( DefaultRelationshipTraversalCursor ) cursor ).Init( _storeCursor.OwningNode, OutgoingReference(), _read );
		 }

		 public override void Incoming( RelationshipTraversalCursor cursor )
		 {
			  ( ( DefaultRelationshipTraversalCursor ) cursor ).Init( _storeCursor.OwningNode, IncomingReference(), _read );
		 }

		 public override void Loops( RelationshipTraversalCursor cursor )
		 {
			  ( ( DefaultRelationshipTraversalCursor ) cursor ).Init( _storeCursor.OwningNode, LoopsReference(), _read );
		 }

		 public override long OutgoingReference()
		 {
			  long reference = _storeCursor.outgoingReference();
			  return reference == NO_ID ? encodeNoOutgoingRels( _storeCursor.type() ) : reference;
		 }

		 public override long IncomingReference()
		 {
			  long reference = _storeCursor.incomingReference();
			  return reference == NO_ID ? encodeNoIncomingRels( _storeCursor.type() ) : reference;
		 }

		 public override long LoopsReference()
		 {
			  long reference = _storeCursor.loopsReference();
			  return reference == NO_ID ? encodeNoLoopRels( _storeCursor.type() ) : reference;
		 }

		 public override bool Closed
		 {
			 get
			 {
				  return _read == null;
			 }
		 }

		 public override string ToString()
		 {
			  if ( Closed )
			  {
					return "RelationshipGroupCursor[closed state]";
			  }
			  else
			  {
					return "RelationshipGroupCursor[id=" + _storeCursor.groupReference() + ", " + _storeCursor.ToString() + "]";
			  }
		 }

		 public virtual void Release()
		 {
			  _storeCursor.close();
		 }
	}

}