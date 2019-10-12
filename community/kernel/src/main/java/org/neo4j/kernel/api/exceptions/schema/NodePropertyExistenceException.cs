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
namespace Org.Neo4j.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;

	public class NodePropertyExistenceException : ConstraintValidationException
	{
		 private readonly long _nodeId;
		 private readonly LabelSchemaDescriptor _schema;

		 public NodePropertyExistenceException( LabelSchemaDescriptor schema, ConstraintValidationException.Phase phase, long nodeId ) : base( ConstraintDescriptorFactory.existsForSchema( schema ), phase, format( "Node(%d)", nodeId ) )
		 {
			  this._schema = schema;
			  this._nodeId = nodeId;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  string propertyNoun = ( _schema.PropertyIds.length > 1 ) ? "properties" : "property";
			  return format( "Node(%d) with label `%s` must have the %s `%s`", _nodeId, tokenNameLookup.LabelGetName( _schema.LabelId ), propertyNoun, SchemaUtil.niceProperties( tokenNameLookup, _schema.PropertyIds ) );
		 }
	}

}