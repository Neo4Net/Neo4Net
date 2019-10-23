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
namespace Neo4Net.Kernel.Api.Internal.Exceptions
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;

	public class IEntityNotFoundException : KernelException
	{
		 private readonly EntityType _entityType;
		 private readonly long _entityId;

		 public IEntityNotFoundException( EntityType EntityType, long IEntityId ) : base( org.Neo4Net.kernel.api.exceptions.Status_Statement.EntityNotFound, "Unable to load %s with id %s.", EntityType.name(), IEntityId )
		 {
			  this._entityType = EntityType;
			  this._entityId = IEntityId;
		 }

		 public virtual EntityType EntityType()
		 {
			  if ( _entityType == null )
			  {
					throw new System.InvalidOperationException( "No Entity type specified for this exception", this );
			  }
			  return _entityType;
		 }

		 public virtual long IEntityId()
		 {
			  if ( _entityId == -1 )
			  {
					throw new System.InvalidOperationException( "No Entity id specified for this exception", this );
			  }
			  return _entityId;
		 }
	}

}