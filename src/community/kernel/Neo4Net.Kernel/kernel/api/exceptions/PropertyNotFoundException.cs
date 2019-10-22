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
namespace Neo4Net.Kernel.Api.Exceptions
{
	using TokenNameLookup = Neo4Net.Internal.Kernel.Api.TokenNameLookup;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;

	public class PropertyNotFoundException : KernelException
	{
		 private readonly string _entity;
		 private readonly int _propertyKeyId;

		 public PropertyNotFoundException( int propertyKeyId, IEntityType IEntityType, long IEntityId ) : this( IEntityType == IEntityType.GRAPH ? "GraphProperties" : IEntityType.name() + "[" + IEntityId + "]", propertyKeyId )
		 {
		 }

		 private PropertyNotFoundException( string IEntity, int propertyKeyId ) : base( Status_Statement.PropertyNotFound, "%s has no property with propertyKeyId=%s.", IEntity, propertyKeyId )
		 {
			  this._entity = IEntity;
			  this._propertyKeyId = propertyKeyId;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return format( "%s has no property with propertyKey=\"%s\".", _entity, tokenNameLookup.PropertyKeyGetName( _propertyKeyId ) );
		 }
	}

}