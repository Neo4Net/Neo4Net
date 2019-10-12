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
namespace Neo4Net.Kernel.impl.transaction.state.storeview
{

	using Neo4Net.Helpers.Collection;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using EntityUpdates = Neo4Net.Kernel.Impl.Api.index.EntityUpdates;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.labelscan.NodeLabelUpdate.labelChanges;

	public class StoreViewNodeStoreScan<FAILURE> : PropertyAwareEntityStoreScan<StorageNodeCursor, FAILURE> where FAILURE : Exception
	{
		 private readonly Visitor<NodeLabelUpdate, FAILURE> _labelUpdateVisitor;
		 private readonly Visitor<EntityUpdates, FAILURE> _propertyUpdatesVisitor;
		 protected internal readonly int[] LabelIds;

		 public StoreViewNodeStoreScan( StorageReader storageReader, LockService locks, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, int[] labelIds, System.Func<int, bool> propertyKeyIdFilter ) : base( storageReader, storageReader.NodesGetCount(), propertyKeyIdFilter, id -> locks.AcquireNodeLock(id, org.neo4j.kernel.impl.locking.LockService_LockType.ReadLock) )
		 {
			  this._labelUpdateVisitor = labelUpdateVisitor;
			  this._propertyUpdatesVisitor = propertyUpdatesVisitor;
			  this.LabelIds = labelIds;
		 }

		 protected internal override StorageNodeCursor AllocateCursor( StorageReader storageReader )
		 {
			  return storageReader.AllocateNodeCursor();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean process(org.neo4j.storageengine.api.StorageNodeCursor cursor) throws FAILURE
		 public override bool Process( StorageNodeCursor cursor )
		 {
			  long[] labels = cursor.Labels();
			  if ( labels.Length == 0 && LabelIds.Length != 0 )
			  {
					// This node has no labels at all
					return false;
			  }

			  if ( _labelUpdateVisitor != null )
			  {
					// Notify the label update visitor
					_labelUpdateVisitor.visit( labelChanges( cursor.EntityReference(), EMPTY_LONG_ARRAY, labels ) );
			  }

			  if ( _propertyUpdatesVisitor != null && containsAnyEntityToken( LabelIds, labels ) )
			  {
					// Notify the property update visitor
					EntityUpdates.Builder updates = EntityUpdates.forEntity( cursor.EntityReference(), true ).withTokens(labels);

					if ( hasRelevantProperty( cursor, updates ) )
					{
						 return _propertyUpdatesVisitor.visit( updates.Build() );
					}
			  }
			  return false;
		 }
	}

}