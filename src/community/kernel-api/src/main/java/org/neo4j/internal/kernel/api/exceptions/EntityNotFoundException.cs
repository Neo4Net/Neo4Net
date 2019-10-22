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
namespace Neo4Net.Internal.Kernel.Api.exceptions
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;

	public class IEntityNotFoundException : KernelException
	{
		 private readonly IEntityType _entityType;
		 private readonly long _entityId;

		 public IEntityNotFoundException( IEntityType IEntityType, long IEntityId ) : base( org.Neo4Net.kernel.api.exceptions.Status_Statement.EntityNotFound, "Unable to load %s with id %s.", IEntityType.name(), IEntityId )
		 {
			  this._entityType = IEntityType;
			  this._entityId = IEntityId;
		 }

		 public virtual IEntityType IEntityType()
		 {
			  if ( _entityType == null )
			  {
					throw new System.InvalidOperationException( "No IEntity type specified for this exception", this );
			  }
			  return _entityType;
		 }

		 public virtual long IEntityId()
		 {
			  if ( _entityId == -1 )
			  {
					throw new System.InvalidOperationException( "No IEntity id specified for this exception", this );
			  }
			  return _entityId;
		 }
	}

}