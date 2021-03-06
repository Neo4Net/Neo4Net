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
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

	internal abstract class RelationshipConstraintDefinition : SinglePropertyConstraintDefinition
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly RelationshipType RelationshipTypeConflict;

		 protected internal RelationshipConstraintDefinition( InternalSchemaActions actions, RelationshipType relationshipType, string propertyKey ) : base( actions, propertyKey )
		 {
			  this.RelationshipTypeConflict = requireNonNull( relationshipType );
		 }

		 public override Label Label
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  throw new System.InvalidOperationException( "Constraint is associated with relationships" );
			 }
		 }

		 public override RelationshipType RelationshipType
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return RelationshipTypeConflict;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  RelationshipConstraintDefinition that = ( RelationshipConstraintDefinition ) o;
			  return RelationshipTypeConflict.name().Equals(that.RelationshipTypeConflict.name()) && PropertyKey.Equals(that.PropertyKey);
		 }

		 public override int GetHashCode()
		 {
			  return 31 * RelationshipTypeConflict.name().GetHashCode() + PropertyKey.GetHashCode();
		 }
	}

}