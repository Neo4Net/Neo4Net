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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using RelationshipReferenceEncoding = Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using StorageRelationshipTraversalCursor = Neo4Net.Storageengine.Api.StorageRelationshipTraversalCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.References.clearEncoding;

	internal class RecordRelationshipTraversalCursor : RecordRelationshipCursor, StorageRelationshipTraversalCursor
	{
		 private enum GroupState
		 {
			  Incoming,
			  Outgoing,
			  Loop,
			  None
		 }

		 private long _originNodeReference;
		 private long _next;
		 private Record _buffer;
		 private PageCursor _pageCursor;
		 private readonly RecordRelationshipGroupCursor _group;
		 private GroupState _groupState;
		 private bool _open;

		 internal RecordRelationshipTraversalCursor( RelationshipStore relationshipStore, RelationshipGroupStore groupStore ) : base( relationshipStore )
		 {
			  this._group = new RecordRelationshipGroupCursor( relationshipStore, groupStore );
		 }

		 public override void Init( long nodeReference, long reference )
		 {
			  /* There are basically two ways a relationship traversal cursor can be initialized:
			   *
			   * 1. From a dense node, where multiple relationship chains are discovered from relationship groups
			   *    as the internal group cursor sees them.
			   * 2. From a sparse node, where a single relationship chain is traversed.
			   */

			  RelationshipReferenceEncoding encoding = RelationshipReferenceEncoding.parseEncoding( reference );

			  switch ( encoding.innerEnumValue )
			  {
			  case RelationshipReferenceEncoding.InnerEnum.NONE: // this is a normal relationship reference
					Chain( nodeReference, reference );
					break;

			  case RelationshipReferenceEncoding.InnerEnum.GROUP: // this reference is actually to a group record
					Groups( nodeReference, clearEncoding( reference ) );
					break;

			  default:
					throw new System.InvalidOperationException( "Unknown encoding " + encoding );
			  }

			  _open = true;
		 }

		 /*
		  * Normal traversal. Traversal returns mixed types and directions.
		  */
		 private void Chain( long nodeReference, long reference )
		 {
			  if ( _pageCursor == null )
			  {
					_pageCursor = RelationshipPage( reference );
			  }
			  Id = NO_ID;
			  this._groupState = GroupState.None;
			  this._originNodeReference = nodeReference;
			  this._next = reference;
		 }

		 /*
		  * Reference to a group record. Traversal returns mixed types and directions.
		  */
		 private void Groups( long nodeReference, long groupReference )
		 {
			  Id = NO_ID;
			  this._next = NO_ID;
			  this._groupState = GroupState.Incoming;
			  this._originNodeReference = nodeReference;
			  _group.direct( nodeReference, groupReference );
		 }

		 public override long NeighbourNodeReference()
		 {
			  const long source = SourceNodeReference(), target = TargetNodeReference();
			  if ( source == _originNodeReference )
			  {
					return target;
			  }
			  else if ( target == _originNodeReference )
			  {
					return source;
			  }
			  else
			  {
					throw new System.InvalidOperationException( "NOT PART OF CHAIN" );
			  }
		 }

		 public override long OriginNodeReference()
		 {
			  return _originNodeReference;
		 }

		 public override bool Next()
		 {
			  if ( HasBufferedData() )
			  { // We have buffered data, iterate the chain of buffered records
					return NextBuffered();
			  }

			  do
			  {
					if ( TraversingDenseNode() )
					{
						 TraverseDenseNode();
					}

					if ( _next == NO_ID )
					{
						 ResetState();
						 return false;
					}

					RelationshipFull( this, _next, _pageCursor );
					ComputeNext();
			  } while ( !InUse() );

			  return true;
		 }

		 private bool NextBuffered()
		 {
			  _buffer = _buffer.next;
			  if ( !HasBufferedData() )
			  {
					ResetState();
					return false;
			  }
			  else
			  {
					// Copy buffer data to self
					CopyFromBuffer();
			  }

			  return true;
		 }

		 private void TraverseDenseNode()
		 {
			  while ( _next == NO_ID )
			  {
					 /*
					  Dense nodes looks something like:
	
					        Node(dense=true)
	
					                |
					                v
	
					            Group(:HOLDS)   -incoming-> Rel(id=2) -> Rel(id=3)
					                            -outgoing-> Rel(id=5) -> Rel(id=10) -> Rel(id=3)
					                            -loop->     Rel(id=9)
					                |
					                v
	
					            Group(:USES)    -incoming-> Rel(id=14)
					                            -outgoing-> Rel(id=55) -> Rel(id=51) -> ...
					                            -loop->     Rel(id=21) -> Rel(id=11)
	
					                |
					                v
					                ...
	
					  We iterate over dense nodes using a small state machine staring in state INCOMING.
					  1) fetch next group, if no more group stop.
					  2) set next to group.incomingReference, switch state to OUTGOING
					  3) Iterate relationship chain until we reach the end
					  4) set next to group.outgoingReference and state to LOOP
					  5) Iterate relationship chain until we reach the end
					  6) set next to group.loop and state back to INCOMING
					  7) Iterate relationship chain until we reach the end
					  8) GOTO 1
					 */
					switch ( _groupState )
					{
					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordRelationshipTraversalCursor.GroupState.Incoming:
						 bool hasNext = _group.next();
						 if ( !hasNext )
						 {
							  Debug.Assert( _next == NO_ID );
							  return; // no more groups nor relationships
						 }
						 _next = _group.incomingRawId();
						 if ( _pageCursor == null )
						 {
							  _pageCursor = RelationshipPage( Math.Max( _next, 0L ) );
						 }
						 _groupState = GroupState.Outgoing;
						 break;

					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordRelationshipTraversalCursor.GroupState.Outgoing:
						 _next = _group.outgoingRawId();
						 _groupState = GroupState.Loop;
						 break;

					case Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordRelationshipTraversalCursor.GroupState.Loop:
						 _next = _group.loopsRawId();
						 _groupState = GroupState.Incoming;
						 break;

					default:
						 throw new System.InvalidOperationException( "We cannot get here, but checkstyle forces this!" );
					}
			  }
		 }

		 private void ComputeNext()
		 {
			  const long source = SourceNodeReference(), target = TargetNodeReference();
			  if ( source == _originNodeReference )
			  {
					_next = FirstNextRel;
			  }
			  else if ( target == _originNodeReference )
			  {
					_next = SecondNextRel;
			  }
			  else
			  {
					throw new System.InvalidOperationException( "NOT PART OF CHAIN! " + this );
			  }
		 }

		 private void CopyFromBuffer()
		 {
			  this.Id = _buffer.id;
			  this.Type = _buffer.type;
			  this.NextProp = _buffer.nextProp;
			  this.FirstNode = _buffer.firstNode;
			  this.SecondNode = _buffer.secondNode;
		 }

		 private bool TraversingDenseNode()
		 {
			  return _groupState != GroupState.None;
		 }

		 public override void Reset()
		 {
			  if ( _open )
			  {
					_open = false;
					_buffer = null;
					ResetState();
			  }
		 }

		 private void ResetState()
		 {
			  Id = _next = NO_ID;
			  _groupState = GroupState.None;
			  _buffer = null;
		 }

		 public override void Close()
		 {
			  if ( _pageCursor != null )
			  {
					_pageCursor.close();
					_pageCursor = null;
			  }

			  _group.close();
		 }

		 public override string ToString()
		 {
			  if ( !_open )
			  {
					return "RelationshipTraversalCursor[closed state]";
			  }
			  else
			  {
					string dense = "denseNode=" + TraversingDenseNode();
					string mode = "mode=";

					if ( HasBufferedData() )
					{
						 mode = mode + "bufferedData";
					}
					else
					{
						 mode = mode + "regular";
					}
					return "RelationshipTraversalCursor[id=" + Id +
							  ", open state with: " + dense +
							  ", next=" + _next + ", " + mode +
							  ", underlying record=" + base.ToString() + "]";
			  }
		 }

		 private bool HasBufferedData()
		 {
			  return _buffer != null;
		 }

		 /*
		  * Record is both a data holder for buffering data from a RelationshipRecord
		  * as well as a linked list over the records in the group.
		  */
		 internal class Record
		 {
			  internal readonly long Id;
			  internal readonly int Type;
			  internal readonly long NextProp;
			  internal readonly long FirstNode;
			  internal readonly long SecondNode;
			  internal readonly Record Next;

			  /*
			   * Initialize the record chain or push a new record as the new head of the record chain
			   */
			  internal Record( RelationshipRecord record, Record next )
			  {
					if ( record != null )
					{
						 Id = record.Id;
						 Type = record.Type;
						 NextProp = record.NextProp;
						 FirstNode = record.FirstNode;
						 SecondNode = record.SecondNode;
					}
					else
					{
						 Id = NO_ID;
						 Type = NO_ID;
						 NextProp = NO_ID;
						 FirstNode = NO_ID;
						 SecondNode = NO_ID;
					}
					this.Next = next;
			  }
		 }
	}

}