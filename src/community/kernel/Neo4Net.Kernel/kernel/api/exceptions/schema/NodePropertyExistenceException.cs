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
namespace Neo4Net.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.Schema.SchemaUtil;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;

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