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
namespace Org.Neo4j.Kernel.Api.Exceptions
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;

	public class PropertyNotFoundException : KernelException
	{
		 private readonly string _entity;
		 private readonly int _propertyKeyId;

		 public PropertyNotFoundException( int propertyKeyId, EntityType entityType, long entityId ) : this( entityType == EntityType.GRAPH ? "GraphProperties" : entityType.name() + "[" + entityId + "]", propertyKeyId )
		 {
		 }

		 private PropertyNotFoundException( string entity, int propertyKeyId ) : base( Status_Statement.PropertyNotFound, "%s has no property with propertyKeyId=%s.", entity, propertyKeyId )
		 {
			  this._entity = entity;
			  this._propertyKeyId = propertyKeyId;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return format( "%s has no property with propertyKey=\"%s\".", _entity, tokenNameLookup.PropertyKeyGetName( _propertyKeyId ) );
		 }
	}

}