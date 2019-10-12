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
namespace Neo4Net.Kernel.impl.coreapi.schema
{
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using ConstraintType = Neo4Net.Graphdb.schema.ConstraintType;

	public class RelationshipPropertyExistenceConstraintDefinition : RelationshipConstraintDefinition
	{
		 public RelationshipPropertyExistenceConstraintDefinition( InternalSchemaActions actions, RelationshipType relationshipType, string propertyKey ) : base( actions, relationshipType, propertyKey )
		 {
		 }

		 public override void Drop()
		 {
			  AssertInUnterminatedTransaction();
			  Actions.dropRelationshipPropertyExistenceConstraint( RelationshipTypeConflict, PropertyKey );
		 }

		 public override ConstraintType ConstraintType
		 {
			 get
			 {
				  return ConstraintType.RELATIONSHIP_PROPERTY_EXISTENCE;
			 }
		 }

		 public override string ToString()
		 {
			  return format( "ON ()-[%1$s:%2$s]-() ASSERT exists(%1$s.%3$s)", RelationshipTypeConflict.name().ToLower(), RelationshipTypeConflict.name(), PropertyKey );
		 }
	}

}