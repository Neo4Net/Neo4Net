//////////////////// OBSOLETE $!!$ tac
//////////////////using System;

///////////////////*
////////////////// * Copyright © 2018-2020 "Neo4Net,"
////////////////// * Team NeoN [http://neo4net.com]. All Rights Reserved.
////////////////// *
////////////////// * This file is part of Neo4Net.
////////////////// *
////////////////// * Neo4Net is free software: you can redistribute it and/or modify
////////////////// * it under the terms of the GNU General Public License as published by
////////////////// * the Free Software Foundation, either version 3 of the License, or
////////////////// * (at your option) any later version.
////////////////// *
////////////////// * This program is distributed in the hope that it will be useful,
////////////////// * but WITHOUT ANY WARRANTY; without even the implied warranty of
////////////////// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
////////////////// * GNU General Public License for more details.
////////////////// *
////////////////// * You should have received a copy of the GNU General Public License
////////////////// * along with this program.  If not, see <http://www.gnu.org/licenses/>.
////////////////// */
//////////////////namespace Neo4Net.GraphDb.index
//////////////////{

//////////////////	using Neo4Net.GraphDb;
//////////////////	using Neo4Net.GraphDb;
//////////////////	using Iterators = Neo4Net.Helpers.Collections.Iterators;

//////////////////	/// <summary>
//////////////////	/// An <seealso cref="System.Collections.IEnumerator"/> with additional <seealso cref="size()"/> and <seealso cref="close()"/>
//////////////////	/// methods on it, used for iterating over index query results. It is first and
//////////////////	/// foremost an <seealso cref="System.Collections.IEnumerator"/>, but also an <seealso cref="System.Collections.IEnumerable"/> JUST so that it
//////////////////	/// can be used in a for-each loop. The <code>iterator()</code> method
//////////////////	/// <i>always</i> returns <code>this</code>.
//////////////////	/// 
//////////////////	/// The size is calculated before-hand so that calling it is always fast.
//////////////////	/// 
//////////////////	/// When you're done with the result and haven't reached the end of the
//////////////////	/// iteration <seealso cref="close()"/> must be called. Results which are looped through
//////////////////	/// entirely closes automatically. Typical use:
//////////////////	/// 
//////////////////	/// <pre>
//////////////////	/// <code>
//////////////////	/// IndexHits&lt;Node&gt; hits = index.get( "key", "value" );
//////////////////	/// try
//////////////////	/// {
//////////////////	///     for ( Node node : hits )
//////////////////	///     {
//////////////////	///         // do something with the hit
//////////////////	///     }
//////////////////	/// }
//////////////////	/// finally
//////////////////	/// {
//////////////////	///     hits.close();
//////////////////	/// }
//////////////////	/// </code>
//////////////////	/// </pre>
//////////////////	/// </summary>
//////////////////	/// @param <T> the type of items in the Iterator. </param>
//////////////////	/// @deprecated This API will be removed in the next major release. Please consider using schema indexes instead. 
//////////////////	[Obsolete("This API will be removed in the next major release. Please consider using schema indexes instead.")]
//////////////////	public interface IndexHits<T> : ResourceIterator<T>, ResourceIterable<T>
//////////////////	{
//////////////////		 /// <summary>
//////////////////		 /// Returns the size of this iterable, in most scenarios this value is accurate
//////////////////		 /// while in some scenarios near-accurate.
//////////////////		 /// 
//////////////////		 /// There's no cost in calling this method. It's considered near-accurate if this
//////////////////		 /// <seealso cref="IndexHits"/> object has been returned when inside a <seealso cref="Transaction"/>
//////////////////		 /// which has index modifications, of a certain nature. Also entities
//////////////////		 /// (<seealso cref="INode"/>s/<seealso cref="IRelationship"/>s) which have been deleted from the graph,
//////////////////		 /// but are still in the index will also affect the accuracy of the returned size.
//////////////////		 /// </summary>
//////////////////		 /// <returns> the near-accurate size if this iterable. </returns>
//////////////////		 [Obsolete]
//////////////////		 int Size();

//////////////////		 /// <summary>
//////////////////		 /// Closes the underlying search result. This method should be called
//////////////////		 /// whenever you've got what you wanted from the result and won't use it
//////////////////		 /// anymore. It's necessary to call it so that underlying indexes can dispose
//////////////////		 /// of allocated resources for this search result.
//////////////////		 /// 
//////////////////		 /// You can however skip to call this method if you loop through the whole
//////////////////		 /// result, then close() will be called automatically. Even if you loop
//////////////////		 /// through the entire result and then call this method it will silently
//////////////////		 /// ignore any consecutive call (for convenience).
//////////////////		 /// </summary>
//////////////////		 [Obsolete]
//////////////////		 void Close();

//////////////////		 /// <returns> these index hits in a <seealso cref="Stream"/> </returns>
////////////////////JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//////////////////[Obsolete]
////////////////////		 default java.util.stream.Stream<T> stream()
//////////////////	//	 {
//////////////////	//		  // Implementation note: we need this for two reasons:
//////////////////	//		  // one, to disambiguate #stream between ResourceIterator and ResourceIterable,
//////////////////	//		  // two, because implementations of this return themselves on #iterator, so we can't
//////////////////	//		  //      use #iterator().stream(), that then causes stack overflows.
//////////////////	//		  return Iterators.stream(this);
//////////////////	//	 }

//////////////////		 /// <summary>
//////////////////		 /// Returns the first and only item from the result iterator, or {@code null}
//////////////////		 /// if there was none. If there were more than one item in the result a
//////////////////		 /// <seealso cref="NoSuchElementException"/> will be thrown. This method must be called
//////////////////		 /// first in the iteration and will grab the first item from the iteration,
//////////////////		 /// so the result is considered broken after this call.
//////////////////		 /// </summary>
//////////////////		 /// <returns> the first and only item, or {@code null} if none. </returns>
//////////////////		 [Obsolete]
//////////////////		 T Single { get; }

//////////////////		 /// <summary>
//////////////////		 /// Returns the score of the most recently fetched item from this iterator
//////////////////		 /// (from <seealso cref="next()"/>). The range of the returned values is up to the
//////////////////		 /// <seealso cref="Index"/> implementation to dictate. </summary>
//////////////////		 /// <returns> the score of the most recently fetched item from this iterator. </returns>
//////////////////		 [Obsolete]
//////////////////		 float CurrentScore();
//////////////////	}

//////////////////}