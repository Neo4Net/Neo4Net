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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using StorageRelationshipScanCursor = Neo4Net.Storageengine.Api.StorageRelationshipScanCursor;

	internal class RecordRelationshipScanCursor : RecordRelationshipCursor, StorageRelationshipScanCursor
	{
		 private int _filterType;
		 private long _next;
		 private long _highMark;
		 private long _nextStoreReference;
		 private PageCursor _pageCursor;
		 private bool _open;

		 internal RecordRelationshipScanCursor( RelationshipStore relationshipStore ) : base( relationshipStore )
		 {
		 }

		 public override void Scan()
		 {
			  Scan( -1 );
		 }

		 public override void Scan( int type )
		 {
			  if ( Id != NO_ID )
			  {
					ResetState();
			  }
			  if ( _pageCursor == null )
			  {
					_pageCursor = RelationshipPage( 0 );
			  }
			  this._next = 0;
			  this._filterType = type;
			  this._highMark = RelationshipHighMark();
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
					_pageCursor = RelationshipPage( reference );
			  }
			  this._next = reference >= 0 ? reference : NO_ID;
			  this._filterType = -1;
			  this._highMark = NO_ID;
			  this._nextStoreReference = NO_ID;
			  this._open = true;
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
						 RelationshipAdvance( this, _pageCursor );
						 _next++;
						 _nextStoreReference++;
					}
					else
					{
						 Relationship( this, _next++, _pageCursor );
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
							  _highMark = RelationshipHighMark();
							  if ( _next > _highMark )
							  {
									_next = NO_ID;
									return WantedTypeAndInUse;
							  }
						 }
					}
			  } while ( !WantedTypeAndInUse );
			  return true;
		 }

		 private bool WantedTypeAndInUse
		 {
			 get
			 {
				  return ( _filterType == -1 || Type() == _filterType ) && InUse();
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
			  Id = _next = NO_ID;
		 }

		 public override string ToString()
		 {
			  if ( !_open )
			  {
					return "RelationshipScanCursor[closed state]";
			  }
			  else
			  {
					return "RelationshipScanCursor[id=" + Id + ", open state with: highMark=" + _highMark + ", next=" + _next + ", type=" + _filterType +
							  ", underlying record=" + base.ToString() + "]";
			  }
		 }

		 private bool Single
		 {
			 get
			 {
				  return _highMark == NO_ID;
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

		 private void RelationshipAdvance( RelationshipRecord record, PageCursor pageCursor )
		 {
			  // When scanning, we inspect RelationshipRecord.inUse(), so using RecordLoad.CHECK is fine
			  RelationshipStore.nextRecordByCursor( record, RecordLoad.CHECK, pageCursor );
		 }
	}

}