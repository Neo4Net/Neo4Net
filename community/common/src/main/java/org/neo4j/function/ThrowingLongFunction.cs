using System;

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
namespace Org.Neo4j.Function
{
	/// <summary>
	/// Represents a function that accepts a long-valued argument and produces a result.
	/// This is the long-consuming primitive specialization for <seealso cref="ThrowingFunction"/>.
	/// </summary>
	/// @param <R> the type of the result of the function </param>
	/// @param <E> the type of exception that may be thrown from the function </param>
	public interface ThrowingLongFunction<R, E> where E : Exception
	{
		 /// <summary>
		 /// Applies this function to the given argument.
		 /// </summary>
		 /// <param name="value"> the function argument </param>
		 /// <returns> the function result </returns>
		 /// <exception cref="E"> an exception if the function fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: R apply(long value) throws E;
		 R Apply( long value );
	}

}