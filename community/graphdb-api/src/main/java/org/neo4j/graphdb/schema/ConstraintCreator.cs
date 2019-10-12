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
	/// A builder for entering details about a constraint to create. After all details have been entered
	/// <seealso cref="create()"/> must be called for the constraint to actually be created. A constraint creator knows
	/// which <seealso cref="org.neo4j.graphdb.Label label"/> it is to be created for.
	/// 
	/// All methods except <seealso cref="create()"/> will return an <seealso cref="ConstraintCreator"/> which should be
	/// used for further interaction.
	/// 
	/// Compatibility note: New methods may be added to this interface without notice,
	/// backwards compatibility is only guaranteed for clients of this interface, not for
	/// implementors.
	/// </summary>
	/// <seealso cref= Schema </seealso>
	public interface ConstraintCreator
	{
		 /// <summary>
		 /// Imposes a uniqueness constraint for the given property, such that
		 /// there can be at most one node, having the given label, for any set value of that property key.
		 /// </summary>
		 /// <param name="propertyKey"> property to impose the uniqueness constraint for </param>
		 /// <returns> a <seealso cref="ConstraintCreator"/> instance to be used for further interaction. </returns>
		 ConstraintCreator AssertPropertyIsUnique( string propertyKey );

		 /// <summary>
		 /// Creates a constraint with the details specified by the other methods in this interface.
		 /// </summary>
		 /// <returns> the created <seealso cref="ConstraintDefinition constraint"/>. </returns>
		 /// <exception cref="ConstraintViolationException"> if creating this constraint would violate any
		 /// existing constraints. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConstraintDefinition create() throws org.neo4j.graphdb.ConstraintViolationException;
		 ConstraintDefinition Create();
	}

}