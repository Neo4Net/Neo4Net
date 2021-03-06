﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.repr
{

	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using ConstraintType = Org.Neo4j.Graphdb.schema.ConstraintType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationType.CONSTRAINT_DEFINITION;

	public class ConstraintDefinitionRepresentation : MappingRepresentation
	{
		 protected internal readonly ConstraintDefinition ConstraintDefinition;

		 public ConstraintDefinitionRepresentation( ConstraintDefinition constraintDefinition ) : base( CONSTRAINT_DEFINITION )
		 {
			  this.ConstraintDefinition = constraintDefinition;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  switch ( ConstraintDefinition.ConstraintType )
			  {
			  case UNIQUENESS:
			  case NODE_PROPERTY_EXISTENCE:
					serializer.PutString( "label", ConstraintDefinition.Label.name() );
					break;
			  case RELATIONSHIP_PROPERTY_EXISTENCE:
					serializer.PutString( "relationshipType", ConstraintDefinition.RelationshipType.name() );
					break;
			  default:
					throw new System.InvalidOperationException( "Unknown constraint type:" + ConstraintDefinition.ConstraintType );
			  }

			  ConstraintType type = ConstraintDefinition.ConstraintType;
			  serializer.PutString( "type", type.name() );
			  Serialize( ConstraintDefinition, serializer );
		 }

		 protected internal virtual void Serialize( ConstraintDefinition constraintDefinition, MappingSerializer serializer )
		 {
			  System.Func<string, Representation> converter = ValueRepresentation.@string;
			  IEnumerable<Representation> propertyKeyRepresentations = map( converter, constraintDefinition.PropertyKeys );
			  serializer.PutList( "property_keys", new ListRepresentation( RepresentationType.String, propertyKeyRepresentations ) );
		 }
	}

}