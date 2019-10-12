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
namespace Neo4Net.Function
{
	/// <summary>
	/// Represents an operation that accepts a single input argument and returns no result.
	/// Unlike most other functional interfaces, ThrowingConsumer is expected to operate via side-effects.
	/// </summary>
	/// @param <T> the type of the input to the operation </param>
	/// @param <E> the type of exception that may be thrown from the function </param>
	public interface ThrowingConsumer<T, E> where E : Exception
	{
		 /// <summary>
		 /// Performs this operation on the given argument.
		 /// </summary>
		 /// <param name="t"> the input argument </param>
		 /// <exception cref="E"> an exception if the function fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void accept(T t) throws E;
		 void Accept( T t );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T, E> ThrowingConsumer<T, E> noop()
	//	 {
	//		  return t ->
	//		  {
	//		  };
	//	 }
	}

}