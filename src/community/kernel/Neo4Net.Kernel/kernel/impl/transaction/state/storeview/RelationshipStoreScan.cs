using System;

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
namespace Neo4Net.Kernel.impl.transaction.state.storeview
{

	using Neo4Net.Collections.Helpers;
	using IEntityUpdates = Neo4Net.Kernel.Impl.Api.index.EntityUpdates;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using StorageRelationshipScanCursor = Neo4Net.Kernel.Api.StorageEngine.StorageRelationshipScanCursor;

	public class RelationshipStoreScan<FAILURE> : PropertyAwareEntityStoreScan<StorageRelationshipScanCursor, FAILURE> where FAILURE : Exception
	{
		 private readonly int[] _relationshipTypeIds;
		 private readonly Visitor<EntityUpdates, FAILURE> _propertyUpdatesVisitor;

		 public RelationshipStoreScan( StorageReader storageReader, LockService locks, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, int[] relationshipTypeIds, System.Func<int, bool> propertyKeyIdFilter ) : base( storageReader, storageReader.RelationshipsGetCount(), propertyKeyIdFilter, id -> locks.AcquireRelationshipLock(id, Neo4Net.kernel.impl.locking.LockService_LockType.ReadLock) )
		 {
			  this._relationshipTypeIds = relationshipTypeIds;
			  this._propertyUpdatesVisitor = propertyUpdatesVisitor;
		 }

		 protected internal override StorageRelationshipScanCursor AllocateCursor( StorageReader storageReader )
		 {
			  return storageReader.AllocateRelationshipScanCursor();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected boolean process(Neo4Net.Kernel.Api.StorageEngine.StorageRelationshipScanCursor cursor) throws FAILURE
		 protected internal override bool Process( StorageRelationshipScanCursor cursor )
		 {
			  int reltype = cursor.Type();

			  if ( _propertyUpdatesVisitor != null && containsAnyEntityToken( _relationshipTypeIds, reltype ) )
			  {
					// Notify the property update visitor
					EntityUpdates.Builder updates = IEntityUpdates.forEntity( cursor.EntityReference(), true ).withTokens(reltype);

					if ( hasRelevantProperty( cursor, updates ) )
					{
						 return _propertyUpdatesVisitor.visit( updates.Build() );
					}
			  }
			  return false;
		 }
	}

}