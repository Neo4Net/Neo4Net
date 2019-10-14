using System.Collections.Generic;

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
namespace Neo4Net.Graphdb
{

	/// <summary>
	/// <seealso cref="System.Collections.IEnumerable"/> whose <seealso cref="ResourceIterator iterators"/> have associated resources
	/// that need to be released.
	/// 
	/// <seealso cref="ResourceIterator ResourceIterators"/> are always automatically released when their owning
	/// transaction is committed or rolled back.
	/// 
	/// Inside a long running transaction, it is possible to release associated resources early. To do so
	/// you must ensure that all returned ResourceIterators are either fully exhausted, or explicitly closed.
	/// <para>
	/// If you intend to exhaust the returned iterators, you can use conventional code as you would with a normal Iterable:
	/// 
	/// <pre>
	/// {@code
	/// ResourceIterable<Object> iterable;
	/// for ( Object item : iterable )
	/// {
	///     ...
	/// }
	/// }
	/// </pre>
	/// 
	/// However, if your code might not exhaust the iterator, (run until <seealso cref="java.util.Iterator.hasNext()"/>
	/// returns {@code false}), <seealso cref="ResourceIterator"/> provides you with a <seealso cref="ResourceIterator.close()"/> method that
	/// can be invoked to release its associated resources early, by using a {@code finally}-block, or try-with-resource.
	/// 
	/// <pre>
	/// {@code
	/// ResourceIterable<Object> iterable;
	/// ResourceIterator<Object> iterator = iterable.iterator();
	/// try
	/// {
	///     while ( iterator.hasNext() )
	///     {
	///         Object item = iterator.next();
	///         if ( ... )
	///         {
	///             return item; // iterator may not be exhausted.
	///         }
	///     }
	/// }
	/// finally
	/// {
	///     iterator.close();
	/// }
	/// }
	/// </pre>
	/// 
	/// </para>
	/// </summary>
	/// @param <T> the type of values returned through the iterators
	/// </param>
	/// <seealso cref= ResourceIterator </seealso>
	public interface ResourceIterable<T> : IEnumerable<T>
	{
		 /// <summary>
		 /// Returns an <seealso cref="ResourceIterator iterator"/> with associated resources that may be managed.
		 /// </summary>
		 ResourceIterator<T> Iterator();

		 /// <returns> this iterable as a <seealso cref="Stream"/> </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.stream.Stream<T> stream()
	//	 {
	//		  return iterator().stream();
	//	 }
	}

}