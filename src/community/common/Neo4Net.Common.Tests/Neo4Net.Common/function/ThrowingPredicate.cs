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
	/// Represents a predicate (boolean-valued function) of one argument.
	/// </summary>
	/// @param <T> the type of the input to the predicate </param>
	/// @param <E> the type of exception that may be thrown from the operator </param>
	public interface ThrowingPredicate<T, E> where E : Exception
	{
		 /// <summary>
		 /// Evaluates this predicate on the given argument.
		 /// </summary>
		 /// <param name="t"> the input argument </param>
		 /// <returns> true if the input argument matches the predicate, otherwise false </returns>
		 /// <exception cref="E"> an exception if the predicate fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean test(T t) throws E;
		 bool Test( T t );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <TYPE> ThrowingPredicate<TYPE, RuntimeException> throwingPredicate(System.Predicate<TYPE> predicate)
	//	 {
	//		  return new ThrowingPredicate<TYPE,RuntimeException>()
	//		  {
	//				@@Override public boolean test(TYPE value)
	//				{
	//					 return predicate.test(value);
	//				}
	//
	//				@@Override public String toString()
	//				{
	//					 return predicate.toString();
	//				}
	//		  };
	//	 }
	}

}