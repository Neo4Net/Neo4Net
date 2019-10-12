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
namespace Neo4Net.Collection
{

	using Neo4Net.Function;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;

	/// <summary>
	/// Just like <seealso cref="System.Collections.IEnumerator"/>, but with the addition that <seealso cref="hasNext()"/> and <seealso cref="next()"/> can
	/// be declared to throw a checked exception.
	/// </summary>
	/// @param <T> type of items in this iterator. </param>
	/// @param <EXCEPTION> type of exception thrown from <seealso cref="hasNext()"/> and <seealso cref="next()"/>. </param>
	public interface RawIterator<T, EXCEPTION> where EXCEPTION : Exception
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean hasNext() throws EXCEPTION;
		 bool HasNext();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T next() throws EXCEPTION;
		 T Next();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void remove()
	//	 {
	//		  throw new UnsupportedOperationException();
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static <T, EXCEPTION extends Exception> RawIterator<T,EXCEPTION> empty()
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T, EXCEPTION> RawIterator<T, EXCEPTION> empty()
	//	 {
	//		  return (RawIterator<T,EXCEPTION>) EMPTY_ITERATOR;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T, EX> RawIterator<T, EX> of(T... values)
	//	 {
	//		  return new RawIterator<T,EX>()
	//		  {
	//				private int position;
	//
	//				@@Override public boolean hasNext() throws EX
	//				{
	//					 return position < values.length;
	//				}
	//
	//				@@Override public T next() throws EX
	//				{
	//					 if (hasNext())
	//					 {
	//						  return values[position++];
	//					 }
	//					 throw new NoSuchElementException();
	//				}
	//		  };
	//	 }

		 /// <summary>
		 /// Create a raw iterator from the provided <seealso cref="ThrowingSupplier"/> - the iterator will end
		 /// when the supplier returns null.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T, EX> RawIterator<T, EX> from(org.neo4j.function.ThrowingSupplier<T, EX> supplier)
	//	 {
	//		  return new PrefetchingRawIterator<T,EX>()
	//		  {
	//				@@Override protected T fetchNextOrNull() throws EX
	//				{
	//					 return supplier.get();
	//				}
	//		  };
	//	 }

		 /// <summary>
		 /// Create a raw iterator from a regular iterator, assuming no exceptions are being thrown
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T, EX> RawIterator<T, EX> wrap(final java.util.Iterator<T> iterator)
	//	 {
	//		  return Iterators.asRawIterator(iterator);
	//	 }
	}

	public static class RawIterator_Fields
	{
		 public static readonly RawIterator<object, Exception> EmptyIterator = RawIterator.of();
	}

}