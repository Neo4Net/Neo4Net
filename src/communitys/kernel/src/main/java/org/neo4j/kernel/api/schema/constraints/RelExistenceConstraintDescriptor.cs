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
namespace Neo4Net.Kernel.api.schema.constraints
{
	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;
	using RelationTypeSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;

	public class RelExistenceConstraintDescriptor : ConstraintDescriptor
	{
		 private readonly RelationTypeSchemaDescriptor _schema;

		 internal RelExistenceConstraintDescriptor( RelationTypeSchemaDescriptor schema ) : base( org.neo4j.@internal.kernel.api.schema.constraints.ConstraintDescriptor_Type.Exists )
		 {
			  this._schema = schema;
		 }

		 public override RelationTypeSchemaDescriptor Schema()
		 {
			  return _schema;
		 }

		 public override string PrettyPrint( TokenNameLookup tokenNameLookup )
		 {
			  string typeName = EscapeLabelOrRelTyp( tokenNameLookup.RelationshipTypeGetName( _schema.RelTypeId ) );
			  string relName = typeName.ToLower();
			  string propertyName = tokenNameLookup.PropertyKeyGetName( _schema.PropertyId );

			  return string.Format( "CONSTRAINT ON ()-[ {0}:{1} ]-() ASSERT exists({2}.{3})", relName, typeName, relName, propertyName );
		 }
	}

}