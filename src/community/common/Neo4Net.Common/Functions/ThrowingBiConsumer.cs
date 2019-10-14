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
	/// Represents an operation that accepts two arguments and returns no result.
	/// Unlike most other functional interfaces, ThrowingBiConsumer is expected to operate via side-effects.
	/// </summary>
	/// @param <T> the type of the first argument to the function </param>
	/// @param <U> the type of the second argument to the function </param>
	/// @param <E> the type of exception that may be thrown from the operation </param>
	public interface ThrowingBiConsumer<T, U, E> where E : Exception
	{
		 /// <summary>
		 /// Performs this operation on the given arguments.
		 /// </summary>
		 /// <param name="t"> the first input argument </param>
		 /// <param name="u"> the second input argument </param>
		 /// <exception cref="E"> an exception if the operation fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void accept(T t, U u) throws E;
		 void Accept( T t, U u );
	}

}