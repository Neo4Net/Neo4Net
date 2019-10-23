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

namespace Neo4Net.GraphDb.Schema
{
   /// <summary>
   /// A builder for entering details about a constraint to create. After all details have been entered
   /// <seealso cref="create()"/> must be called for the constraint to actually be created. A constraint creator knows
   /// which <seealso cref="org.Neo4Net.graphdb.Label label"/> it is to be created for.
   ///
   /// All methods except <seealso cref="create()"/> will return an <seealso cref="IConstraintCreator"/> which should be
   /// used for further interaction.
   ///
   /// Compatibility note: New methods may be added to this interface without notice,
   /// backwards compatibility is only guaranteed for clients of this interface, not for
   /// implementors.
   /// </summary>
   /// <seealso cref= ISchema </seealso>
   public interface IConstraintCreator
   {
      /// <summary>
      /// Imposes a uniqueness constraint for the given property, such that
      /// there can be at most one node, having the given label, for any set value of that property key.
      /// </summary>
      /// <param name="propertyKey"> property to impose the uniqueness constraint for </param>
      /// <returns> a <seealso cref="IConstraintCreator"/> instance to be used for further interaction. </returns>
      IConstraintCreator AssertPropertyIsUnique(string propertyKey);

      /// <summary>
      /// Creates a constraint with the details specified by the other methods in this interface.
      /// </summary>
      /// <returns> the created <seealso cref="IConstraintDefinition constraint"/>. </returns>
      /// <exception cref="ConstraintViolationException"> if creating this constraint would violate any
      /// existing constraints. </exception>
      IConstraintDefinition Create();
   }
}