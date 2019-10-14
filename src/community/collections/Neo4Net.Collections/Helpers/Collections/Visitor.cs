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
namespace Neo4Net.Helpers.Collections
{

	/// <summary>
	/// A visitor to internalize iteration.
	/// </summary>
	/// @param <E> the element type the visitor accepts. </param>
	/// @param <FAILURE> the type of exception the visitor might throw </param>
	public interface Visitor<E, FAILURE> where FAILURE : Exception
	{
		 /// <summary>
		 /// Invoked for each element in a collection. Return <code>true</code> to
		 /// terminate the iteration, <code>false</code> to continue.
		 /// </summary>
		 /// <param name="element"> an element from the collection. </param>
		 /// <returns> <code>true</code> to terminate the iteration, <code>false</code>
		 ///         to continue. </returns>
		 /// <exception cref="FAILURE"> exception thrown by the visitor </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visit(E element) throws FAILURE;
		 bool Visit( E element );
	}

	 public sealed class Visitor_SafeGenerics
	 {
		  /// <summary>
		  /// Useful for determining "is this an object that can visit the things I can provide?"
		  /// 
		  /// Checks if the passed in object is a <seealso cref="Visitor"/> and if the objects it can
		  /// <seealso cref="Visitor.visit(object) visit"/> is compatible (super type of) with the provided type. Returns the
		  /// visitor cast to compatible type parameters. If the passed in object is not an instance of <seealso cref="Visitor"/>,
		  /// or if it is a <seealso cref="Visitor"/> but one that <seealso cref="Visitor.visit(object) visits"/> another type of object, this
		  /// method returns {@code null}.
		  /// </summary>
		  /// <param name="eType"> element type of the visitor </param>
		  /// <param name="fType"> failure type of the visitor </param>
		  /// <param name="visitor"> the visitor </param>
		  /// @param <T> type of the elements </param>
		  /// @param <F> type of the exception </param>
		  /// <returns> the visitor cast to compatible type parameters or {@code null} </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T, F extends Exception> Visitor<? super T, ? extends F> castOrNull(Class<T> eType, Class<F> fType, Object visitor)
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		  public static Visitor<object, ? extends F> CastOrNull<T, F>( Type eType, Type fType, object visitor ) where F : Exception
		  {
				  fType = typeof( F );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (visitor instanceof Visitor<?, ?>)
				if ( visitor is Visitor<object, ?> )
				{
					 foreach ( Type iface in visitor.GetType().GenericInterfaces )
					 {
						  if ( iface is ParameterizedType )
						  {
								ParameterizedType paramType = ( ParameterizedType ) iface;
								if ( paramType.RawType == typeof( Visitor ) )
								{
									 Type arg = paramType.ActualTypeArguments[0];
									 if ( arg is ParameterizedType )
									 {
										  arg = ( ( ParameterizedType ) arg ).RawType;
									 }
									 if ( ( arg is Type ) && ( ( Type ) arg ).IsAssignableFrom( eType ) )
									 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: return (Visitor<? super T, ? extends F>) visitor;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
										  return ( Visitor<object, ? extends F> ) visitor;
									 }
								}
						  }
					 }
				}
				return null;
		  }

		  internal Visitor_SafeGenerics()
		  {
		  }
	 }

}