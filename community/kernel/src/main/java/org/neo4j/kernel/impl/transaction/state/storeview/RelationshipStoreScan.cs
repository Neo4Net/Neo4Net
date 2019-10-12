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
namespace Org.Neo4j.Kernel.impl.transaction.state.storeview
{

	using Org.Neo4j.Helpers.Collection;
	using EntityUpdates = Org.Neo4j.Kernel.Impl.Api.index.EntityUpdates;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using StorageRelationshipScanCursor = Org.Neo4j.Storageengine.Api.StorageRelationshipScanCursor;

	public class RelationshipStoreScan<FAILURE> : PropertyAwareEntityStoreScan<StorageRelationshipScanCursor, FAILURE> where FAILURE : Exception
	{
		 private readonly int[] _relationshipTypeIds;
		 private readonly Visitor<EntityUpdates, FAILURE> _propertyUpdatesVisitor;

		 public RelationshipStoreScan( StorageReader storageReader, LockService locks, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, int[] relationshipTypeIds, System.Func<int, bool> propertyKeyIdFilter ) : base( storageReader, storageReader.RelationshipsGetCount(), propertyKeyIdFilter, id -> locks.AcquireRelationshipLock(id, org.neo4j.kernel.impl.locking.LockService_LockType.ReadLock) )
		 {
			  this._relationshipTypeIds = relationshipTypeIds;
			  this._propertyUpdatesVisitor = propertyUpdatesVisitor;
		 }

		 protected internal override StorageRelationshipScanCursor AllocateCursor( StorageReader storageReader )
		 {
			  return storageReader.AllocateRelationshipScanCursor();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected boolean process(org.neo4j.storageengine.api.StorageRelationshipScanCursor cursor) throws FAILURE
		 protected internal override bool Process( StorageRelationshipScanCursor cursor )
		 {
			  int reltype = cursor.Type();

			  if ( _propertyUpdatesVisitor != null && containsAnyEntityToken( _relationshipTypeIds, reltype ) )
			  {
					// Notify the property update visitor
					EntityUpdates.Builder updates = EntityUpdates.forEntity( cursor.EntityReference(), true ).withTokens(reltype);

					if ( hasRelevantProperty( cursor, updates ) )
					{
						 return _propertyUpdatesVisitor.visit( updates.Build() );
					}
			  }
			  return false;
		 }
	}

}