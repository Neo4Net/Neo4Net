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
	/// Represents a function that accepts one argument and produces a result, or throws an exception.
	/// </summary>
	/// @param <T> the type of the input to the function </param>
	/// @param <R> the type of the result of the function </param>
	/// @param <E> the type of exception that may be thrown from the function </param>
	public interface ThrowingFunction<T, R, E> where E : Exception
	{
		 /// <summary>
		 /// Apply a value to this function
		 /// </summary>
		 /// <param name="t"> the function argument </param>
		 /// <returns> the function result </returns>
		 /// <exception cref="E"> an exception if the function fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: R apply(T t) throws E;
		 R Apply( T t );

		 /// <summary>
		 /// Construct a regular function that calls a throwing function and catches all checked exceptions
		 /// declared and thrown by the throwing function and rethrows them as <seealso cref="UncaughtCheckedException"/>
		 /// for handling further up the stack.
		 /// </summary>
		 /// <seealso cref= UncaughtCheckedException
		 /// </seealso>
		 /// <param name="throwing"> the throwing function to wtap </param>
		 /// @param <T> type of arguments </param>
		 /// @param <R> type of results </param>
		 /// @param <E> type of checked exceptions thrown by the throwing function </param>
		 /// <returns> a new, non-throwing function </returns>
		 /// <exception cref="IllegalStateException"> if an unexpected exception is caught (ie. neither of type E or a runtime exception) </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T, R, E> System.Func<T, R> catchThrown(Class<E> clazz, ThrowingFunction<T, R, E> throwing)
	//	 {
	//		  return input ->
	//		  {
	//				try
	//				{
	//					 return throwing.apply(input);
	//				}
	//				catch (Exception e)
	//				{
	//					 if (clazz.isInstance(e))
	//					 {
	//						  throw new UncaughtCheckedException(throwing, clazz.cast(e));
	//					 }
	//					 else if (e instanceof RuntimeException)
	//					 {
	//						  throw (RuntimeException) e;
	//					 }
	//					 else
	//					 {
	//						  throw new IllegalStateException("Unexpected exception", e);
	//					 }
	//				}
	//		  };
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalUsedAsFieldOrParameterType") static <E extends Exception> void throwIfPresent(java.util.Optional<E> exception) throws E
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <E> void throwIfPresent(java.util.Optional<E> exception) throws E
	//	 {
	//		  if (exception.isPresent())
	//		  {
	//				throw exception.get();
	//		  }
	//	 }
	}

}