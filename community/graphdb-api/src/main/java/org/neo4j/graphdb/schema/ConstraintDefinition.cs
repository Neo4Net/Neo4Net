using System.Collections.Generic;

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
namespace Org.Neo4j.Graphdb.schema
{

	/// <summary>
	/// Definition of a constraint.
	/// 
	/// <b>Note:</b> This interface is going to be changed/removed in next major release to better cope with node and
	/// relationship constraints which are quite different concepts.
	/// </summary>
	public interface ConstraintDefinition
	{
		 /// <summary>
		 /// This accessor method returns a label which this constraint is associated with if this constraint has type
		 /// <seealso cref="ConstraintType.UNIQUENESS"/> or <seealso cref="ConstraintType.NODE_PROPERTY_EXISTENCE"/>.
		 /// Type of the constraint can be examined by calling <seealso cref="getConstraintType()"/> or
		 /// <seealso cref="isConstraintType(ConstraintType)"/> methods.
		 /// </summary>
		 /// <returns> the <seealso cref="Label"/> this constraint is associated with. </returns>
		 /// <exception cref="IllegalStateException"> when this constraint is associated with relationships. </exception>
		 Label Label { get; }

		 /// <summary>
		 /// This accessor method returns a relationship type which this constraint is associated with if this constraint
		 /// has type <seealso cref="ConstraintType.UNIQUENESS"/> or <seealso cref="ConstraintType.NODE_PROPERTY_EXISTENCE"/>.
		 /// Type of the constraint can be examined by calling <seealso cref="getConstraintType()"/> or
		 /// <seealso cref="isConstraintType(ConstraintType)"/> methods.
		 /// </summary>
		 /// <returns> the <seealso cref="RelationshipType"/> this constraint is associated with. </returns>
		 /// <exception cref="IllegalStateException"> when this constraint is associated with nodes. </exception>
		 RelationshipType RelationshipType { get; }

		 /// <returns> the property keys this constraint is about. </returns>
		 IEnumerable<string> PropertyKeys { get; }

		 /// <summary>
		 /// Drops this constraint.
		 /// </summary>
		 void Drop();

		 /// <returns> the <seealso cref="ConstraintType"/> of constraint. </returns>
		 ConstraintType ConstraintType { get; }

		 /// <param name="type"> a constraint type </param>
		 /// <returns> true if this constraint definition's type is equal to the provided type </returns>
		 bool IsConstraintType( ConstraintType type );
	}

}