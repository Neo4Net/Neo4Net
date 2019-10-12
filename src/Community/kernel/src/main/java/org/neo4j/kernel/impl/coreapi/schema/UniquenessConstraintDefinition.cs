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
	using ConstraintType = Neo4Net.Graphdb.schema.ConstraintType;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;

	public class UniquenessConstraintDefinition : NodeConstraintDefinition
	{
		 public UniquenessConstraintDefinition( InternalSchemaActions actions, IndexDefinition indexDefinition ) : base( actions, indexDefinition )
		 {
		 }

		 public override void Drop()
		 {
			  AssertInUnterminatedTransaction();
			  Actions.dropPropertyUniquenessConstraint( LabelConflict, PropertyKeysConflict );
		 }

		 public virtual ConstraintType ConstraintType
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return ConstraintType.UNIQUENESS;
			 }
		 }

		 public override string ToString()
		 {
			  return format( "ON (%1$s:%2$s) ASSERT %3$s IS UNIQUE", LabelConflict.name().ToLower(), LabelConflict.name(), PropertyText() );
		 }
	}

}