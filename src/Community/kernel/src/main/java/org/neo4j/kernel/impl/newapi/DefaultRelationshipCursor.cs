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
	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.@internal.Kernel.Api.PropertyCursor;
	using RelationshipDataAccessor = Neo4Net.@internal.Kernel.Api.RelationshipDataAccessor;
	using StorageRelationshipCursor = Neo4Net.Storageengine.Api.StorageRelationshipCursor;

	internal abstract class DefaultRelationshipCursor<STORECURSOR> : RelationshipDataAccessor where STORECURSOR : Neo4Net.Storageengine.Api.StorageRelationshipCursor
	{
		 private bool _hasChanges;
		 private bool _checkHasChanges;
		 internal readonly DefaultCursors Pool;
		 internal Read Read;

		 internal readonly STORECURSOR StoreCursor;

		 internal DefaultRelationshipCursor( DefaultCursors pool, STORECURSOR storeCursor )
		 {
			  this.Pool = pool;
			  this.StoreCursor = storeCursor;
		 }

		 protected internal virtual void Init( Read read )
		 {
			  this.Read = read;
			  this._checkHasChanges = true;
		 }

		 public override long RelationshipReference()
		 {
			  return StoreCursor.entityReference();
		 }

		 public override int Type()
		 {
			  return StoreCursor.type();
		 }

		 public override void Source( NodeCursor cursor )
		 {
			  Read.singleNode( SourceNodeReference(), cursor );
		 }

		 public override void Target( NodeCursor cursor )
		 {
			  Read.singleNode( TargetNodeReference(), cursor );
		 }

		 public override void Properties( PropertyCursor cursor )
		 {
			  ( ( DefaultPropertyCursor ) cursor ).InitRelationship( RelationshipReference(), PropertiesReference(), Read, Read );
		 }

		 public override long SourceNodeReference()
		 {
			  return StoreCursor.sourceNodeReference();
		 }

		 public override long TargetNodeReference()
		 {
			  return StoreCursor.targetNodeReference();
		 }

		 public override long PropertiesReference()
		 {
			  return StoreCursor.propertiesReference();
		 }

		 protected internal abstract void CollectAddedTxStateSnapshot();

		 /// <summary>
		 /// RelationshipCursor should only see changes that are there from the beginning
		 /// otherwise it will not be stable.
		 /// </summary>
		 protected internal virtual bool HasChanges()
		 {
			  if ( _checkHasChanges )
			  {
					_hasChanges = Read.hasTxStateWithChanges();
					if ( _hasChanges )
					{
						 CollectAddedTxStateSnapshot();
					}
					_checkHasChanges = false;
			  }

			  return _hasChanges;
		 }
	}

}