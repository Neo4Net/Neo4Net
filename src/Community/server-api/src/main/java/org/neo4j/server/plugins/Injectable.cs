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
namespace Neo4Net.Server.plugins
{
	/// <summary>
	/// Used to allow custom values to be injected into JAX-RS classes.
	/// </summary>
	/// @param <T> the type of the value, or an interface the value implements. </param>
	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public interface Injectable<T>
	{
		 /// <summary>
		 /// Get the injectable value.
		 /// </summary>
		 /// <returns> the injectable value </returns>
		 T Value { get; }

		 /// <summary>
		 /// The type that resources should ask for to get this value;
		 /// this can either be the concrete class, or some interface the
		 /// value instance implements.
		 /// </summary>
		 /// <returns> a class that methods that want this value injected should ask for </returns>
		 Type<T> Type { get; }

		 /// <summary>
		 /// Utility to wrap a singleton value as an injectable.
		 /// </summary>
		 /// <param name="type"> the type that JAX-RS classes should ask for </param>
		 /// <param name="obj"> the value </param>
		 /// @param <T> same as type
		 /// @return </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
[Obsolete]
//		 static <T> Injectable<T> injectable(Class<T> type, T obj)
	//	 {
	//		  return new Injectable<T>()
	//		  {
	//				@@Override public T getValue()
	//				{
	//					 return obj;
	//				}
	//
	//				@@Override public Class<T> getType()
	//				{
	//					 return type;
	//				}
	//		  };
	//	 }
	}

}