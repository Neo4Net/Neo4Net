using System.Collections.Generic;

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
	using ConstraintDefinition = Neo4Net.GraphDb.schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.schema.ConstraintType;

	internal abstract class PropertyConstraintDefinition : ConstraintDefinition
	{
		public abstract ConstraintType ConstraintType { get; }
		public abstract void Drop();
		public abstract Neo4Net.GraphDb.RelationshipType RelationshipType { get; }
		public abstract Neo4Net.GraphDb.Label Label { get; }
		 protected internal readonly InternalSchemaActions Actions;

		 protected internal PropertyConstraintDefinition( InternalSchemaActions actions )
		 {
			  this.Actions = requireNonNull( actions );
		 }

		 public override abstract IEnumerable<string> PropertyKeys { get; }

		 public override bool IsConstraintType( ConstraintType type )
		 {
			  AssertInUnterminatedTransaction();
			  return ConstraintType.Equals( type );
		 }

		 public override abstract boolean ( object o );

		 public override abstract int ();

		 /// <summary>
		 /// Returned string is used in shell's constraint listing.
		 /// </summary>
		 public override abstract String ();

		 protected internal virtual void AssertInUnterminatedTransaction()
		 {
			  Actions.assertInOpenTransaction();
		 }
	}

}