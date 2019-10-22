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
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;

	public class RecordNodeCursor : NodeRecord, StorageNodeCursor
	{
		 private NodeStore _read;
		 private PageCursor _pageCursor;
		 private long _next;
		 private long _highMark;
		 private long _nextStoreReference;
		 private bool _open;

		 internal RecordNodeCursor( NodeStore read ) : base( NO_ID )
		 {
			  this._read = read;
		 }

		 public override void Scan()
		 {
			  if ( Id != NO_ID )
			  {
					ResetState();
			  }
			  if ( _pageCursor == null )
			  {
					_pageCursor = NodePage( 0 );
			  }
			  this._next = 0;
			  this._highMark = NodeHighMark();
			  this._nextStoreReference = NO_ID;
			  this._open = true;
		 }

		 public override void Single( long reference )
		 {
			  if ( Id != NO_ID )
			  {
					ResetState();
			  }
			  if ( _pageCursor == null )
			  {
					_pageCursor = NodePage( reference );
			  }
			  this._next = reference >= 0 ? reference : NO_ID;
			  //This marks the cursor as a "single cursor"
			  this._highMark = NO_ID;
			  this._nextStoreReference = NO_ID;
			  this._open = true;
		 }

		 public override long IEntityReference()
		 {
			  return Id;
		 }

		 public override long[] Labels()
		 {
			  return NodeLabelsField.get( this, _read );
		 }

		 public override bool HasLabel( int label )
		 {
			  //Get labels from store and put in intSet, unfortunately we get longs back
			  long[] longs = NodeLabelsField.get( this, _read );
			  foreach ( long labelToken in longs )
			  {
					if ( labelToken == label )
					{
						 assert( int ) labelToken == labelToken : "value too big to be represented as and int";
						 return true;
					}
			  }
			  return false;
		 }

		 public override bool HasProperties()
		 {
			  return NextPropConflict != NO_ID;
		 }

		 public override long RelationshipGroupReference()
		 {
			  return Dense ? NextRel : GroupReferenceEncoding.EncodeRelationship( NextRel );
		 }

		 public override long AllRelationshipsReference()
		 {
			  return Dense ? RelationshipReferenceEncoding.encodeGroup( NextRel ) : NextRel;
		 }

		 public override long PropertiesReference()
		 {
			  return NextProp;
		 }

		 public override bool Next()
		 {
			  if ( _next == NO_ID )
			  {
					ResetState();
					return false;
			  }

			  do
			  {
					if ( _nextStoreReference == _next )
					{
						 NodeAdvance( this, _pageCursor );
						 _next++;
						 _nextStoreReference++;
					}
					else
					{
						 Node( this, _next++, _pageCursor );
						 _nextStoreReference = _next;
					}

					if ( _next > _highMark )
					{
						 if ( Single )
						 {
							  //we are a "single cursor"
							  _next = NO_ID;
							  return InUse();
						 }
						 else
						 {
							  //we are a "scan cursor"
							  //Check if there is a new high mark
							  _highMark = NodeHighMark();
							  if ( _next > _highMark )
							  {
									_next = NO_ID;
									return InUse();
							  }
						 }
					}
			  } while ( !InUse() );
			  return true;
		 }

		 public virtual long Current
		 {
			 set
			 {
				  Id = value;
				  InUse = true;
			 }
		 }

		 public override void Reset()
		 {
			  if ( _open )
			  {
					_open = false;
					ResetState();
			  }
		 }

		 private void ResetState()
		 {
			  _next = NO_ID;
			  Id = NO_ID;
			  Clear();
		 }

		 private bool Single
		 {
			 get
			 {
				  return _highMark == NO_ID;
			 }
		 }

		 public override string ToString()
		 {
			  if ( !_open )
			  {
					return "RecordNodeCursor[closed state]";
			  }
			  else
			  {
					return "RecordNodeCursor[id=" + Id +
							  ", open state with: highMark=" + _highMark +
							  ", next=" + _next +
							  ", underlying record=" + base.ToString() + "]";
			  }
		 }

		 public override void Close()
		 {
			  if ( _pageCursor != null )
			  {
					_pageCursor.close();
					_pageCursor = null;
			  }
		 }

		 private PageCursor NodePage( long reference )
		 {
			  return _read.openPageCursorForReading( reference );
		 }

		 private long NodeHighMark()
		 {
			  return _read.HighestPossibleIdInUse;
		 }

		 private void Node( NodeRecord record, long reference, PageCursor pageCursor )
		 {
			  _read.getRecordByCursor( reference, record, RecordLoad.CHECK, pageCursor );
		 }

		 private void NodeAdvance( NodeRecord record, PageCursor pageCursor )
		 {
			  _read.nextRecordByCursor( record, RecordLoad.CHECK, pageCursor );
		 }
	}

}