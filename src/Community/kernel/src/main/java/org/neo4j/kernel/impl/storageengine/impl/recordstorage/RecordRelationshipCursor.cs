using System;

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
	using Neo4Net.Storageengine.Api;
	using StorageRelationshipCursor = Neo4Net.Storageengine.Api.StorageRelationshipCursor;

	internal abstract class RecordRelationshipCursor : RelationshipRecord, RelationshipVisitor<Exception>, StorageRelationshipCursor
	{
		public abstract void Close();
		public abstract void Reset();
		public abstract bool Next();
		 internal readonly RelationshipStore RelationshipStore;

		 internal RecordRelationshipCursor( RelationshipStore relationshipStore ) : base( NO_ID )
		 {
			  this.RelationshipStore = relationshipStore;
		 }

		 public override long EntityReference()
		 {
			  return Id;
		 }

		 public override int Type()
		 {
			  return Type;
		 }

		 public override bool HasProperties()
		 {
			  return NextPropConflict != NO_ID;
		 }

		 public override long SourceNodeReference()
		 {
			  return FirstNode;
		 }

		 public override long TargetNodeReference()
		 {
			  return SecondNode;
		 }

		 public override long PropertiesReference()
		 {
			  return NextProp;
		 }

		 // used to visit transaction state
		 public override void Visit( long relationshipId, int typeId, long startNodeId, long endNodeId )
		 {
			  Id = relationshipId;
			  Initialize( true, NO_ID, startNodeId, endNodeId, typeId, NO_ID, NO_ID, NO_ID, NO_ID, false, false );
		 }

		 internal virtual PageCursor RelationshipPage( long reference )
		 {
			  return RelationshipStore.openPageCursorForReading( reference );
		 }

		 internal virtual void Relationship( RelationshipRecord record, long reference, PageCursor pageCursor )
		 {
			  // When scanning, we inspect RelationshipRecord.inUse(), so using RecordLoad.CHECK is fine
			  RelationshipStore.getRecordByCursor( reference, record, RecordLoad.CHECK, pageCursor );
		 }

		 internal virtual void RelationshipFull( RelationshipRecord record, long reference, PageCursor pageCursor )
		 {
			  // We need to load forcefully for relationship chain traversal since otherwise we cannot
			  // traverse over relationship records which have been concurrently deleted
			  // (flagged as inUse = false).
			  // see
			  //      org.neo4j.kernel.impl.store.RelationshipChainPointerChasingTest
			  //      org.neo4j.kernel.impl.locking.RelationshipCreateDeleteIT
			  RelationshipStore.getRecordByCursor( reference, record, RecordLoad.FORCE, pageCursor );
		 }

		 internal virtual long RelationshipHighMark()
		 {
			  return RelationshipStore.HighestPossibleIdInUse;
		 }
	}

}