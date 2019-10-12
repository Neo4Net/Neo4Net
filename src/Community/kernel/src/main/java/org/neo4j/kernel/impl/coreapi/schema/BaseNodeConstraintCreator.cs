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
	using Label = Neo4Net.Graphdb.Label;
	using ConstraintCreator = Neo4Net.Graphdb.schema.ConstraintCreator;

	public class BaseNodeConstraintCreator : AbstractConstraintCreator, ConstraintCreator
	{
		 protected internal readonly Label Label;

		 public BaseNodeConstraintCreator( InternalSchemaActions actions, Label label ) : base( actions )
		 {
			  this.Label = label;

			  AssertInUnterminatedTransaction();
		 }

		 public override ConstraintCreator AssertPropertyIsUnique( string propertyKey )
		 {
			  return new NodePropertyUniqueConstraintCreator( Actions, Label, propertyKey );
		 }
	}

}