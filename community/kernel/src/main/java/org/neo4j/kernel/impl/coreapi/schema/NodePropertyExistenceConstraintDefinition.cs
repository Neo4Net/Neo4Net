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
namespace Org.Neo4j.Kernel.impl.coreapi.schema
{
	using Label = Org.Neo4j.Graphdb.Label;
	using ConstraintType = Org.Neo4j.Graphdb.schema.ConstraintType;

	public class NodePropertyExistenceConstraintDefinition : NodeConstraintDefinition
	{
		 public NodePropertyExistenceConstraintDefinition( InternalSchemaActions actions, Label label, string[] propertyKeys ) : base( actions, label, propertyKeys )
		 {
		 }

		 public override void Drop()
		 {
			  AssertInUnterminatedTransaction();
			  Actions.dropNodePropertyExistenceConstraint( LabelConflict, PropertyKeysConflict );
		 }

		 public virtual ConstraintType ConstraintType
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return ConstraintType.NODE_PROPERTY_EXISTENCE;
			 }
		 }

		 public override string ToString()
		 {
			  if ( PropertyKeysConflict.Length == 1 )
			  {
					return format( "ON (%1$s:%2$s) ASSERT exists(%3$s)", LabelConflict.name().ToLower(), LabelConflict.name(), PropertyText() );
			  }
			  else
			  {
					return format( "ON (%1$s:%2$s) ASSERT %3$s IS COMPOSITE KEY", LabelConflict.name().ToLower(), LabelConflict.name(), PropertyText() );
			  }
		 }
	}

}