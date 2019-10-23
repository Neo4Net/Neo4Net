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
namespace Neo4Net.Kernel.impl.coreapi.schema
{
	using Label = Neo4Net.GraphDb.Label;
	using ConstraintType = Neo4Net.GraphDb.Schema.ConstraintType;

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