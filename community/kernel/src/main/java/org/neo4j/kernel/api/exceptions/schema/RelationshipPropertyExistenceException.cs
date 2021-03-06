﻿/*
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
namespace Org.Neo4j.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using RelationTypeSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;

	public class RelationshipPropertyExistenceException : ConstraintValidationException
	{
		 private readonly RelationTypeSchemaDescriptor _schema;
		 private readonly long _relationshipId;

		 public RelationshipPropertyExistenceException( RelationTypeSchemaDescriptor schema, ConstraintValidationException.Phase phase, long relationshipId ) : base( ConstraintDescriptorFactory.existsForSchema( schema ), phase, format( "Relationship(%s)", relationshipId ) )
		 {
			  this._schema = schema;
			  this._relationshipId = relationshipId;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return format( "Relationship(%s) with type `%s` must have the property `%s`", _relationshipId, tokenNameLookup.RelationshipTypeGetName( _schema.RelTypeId ), tokenNameLookup.PropertyKeyGetName( _schema.PropertyId ) );
		 }
	}

}