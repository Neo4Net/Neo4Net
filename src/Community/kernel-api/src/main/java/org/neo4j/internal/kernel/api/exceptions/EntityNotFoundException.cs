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
namespace Neo4Net.@internal.Kernel.Api.exceptions
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;

	public class EntityNotFoundException : KernelException
	{
		 private readonly EntityType _entityType;
		 private readonly long _entityId;

		 public EntityNotFoundException( EntityType entityType, long entityId ) : base( org.neo4j.kernel.api.exceptions.Status_Statement.EntityNotFound, "Unable to load %s with id %s.", entityType.name(), entityId )
		 {
			  this._entityType = entityType;
			  this._entityId = entityId;
		 }

		 public virtual EntityType EntityType()
		 {
			  if ( _entityType == null )
			  {
					throw new System.InvalidOperationException( "No entity type specified for this exception", this );
			  }
			  return _entityType;
		 }

		 public virtual long EntityId()
		 {
			  if ( _entityId == -1 )
			  {
					throw new System.InvalidOperationException( "No entity id specified for this exception", this );
			  }
			  return _entityId;
		 }
	}

}