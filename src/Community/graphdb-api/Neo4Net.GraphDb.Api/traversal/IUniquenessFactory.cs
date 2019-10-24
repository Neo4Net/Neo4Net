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

namespace Neo4Net.GraphDb.Traversal
{
    /// <summary>
    /// Factory for <seealso cref="UniquenessFilter"/> filters, it can supply the
    /// created <seealso cref="UniquenessFilter"/> with an optional parameter which is
    /// up to the filter itself to validate and make sense of.
    /// </summary>
    public interface IUniquenessFactory
    {
        /// <summary>
        /// Creates a new <seealso cref="UniquenessFilter"/> optionally with a parameter
        /// to it, otherwise null if no parameter should be handed to it.
        /// </summary>
        /// <param name="optionalParameter"> an optional parameter to control the behavior
        /// of the returned <seealso cref="UniquenessFilter"/>. It's up to each filter implementation
        /// to decide what values are OK and what they mean and the caller of this
        /// method need to know that and pass in the correct parameter type. </param>
        /// <returns> a new <seealso cref="UniquenessFilter"/> of the type that this factory creates. </returns>
        IUniquenessFilter Create(object optionalParameter);

        /// <summary>
        /// Specifies if the <seealso cref="UniquenessFilter"/> must handle start branches eagerly. Depending on the
        /// level of uniqueness it is not always necessary to eagerly exhaust start
        /// branches which can speed up the execution of the traversal.
        /// </summary>
        /// <returns> {@code true} if eager start branches must be used, otherwise {@code false}. </returns>
        bool EagerStartBranches();
    }
}