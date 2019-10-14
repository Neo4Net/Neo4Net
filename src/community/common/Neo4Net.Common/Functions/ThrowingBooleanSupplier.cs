using System;

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
namespace Neo4Net.Functions
{
	/// <summary>
	/// Represents a supplier of boolean-valued results. This is the boolean-producing primitive specialization of <seealso cref="ThrowingSupplier"/>.
	/// There is no requirement that a new or distinct result be returned each time the supplier is invoked.
	/// </summary>
	/// @param <E> the type of exception that may be thrown from the supplier </param>
	public interface ThrowingBooleanSupplier<E> where E : Exception
	{
		 /// <summary>
		 /// Gets a result.
		 /// </summary>
		 /// <returns> a result </returns>
		 /// <exception cref="E"> an exception if the supplier fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean getAsBoolean() throws E;
		 bool AsBoolean { get; }
	}

}