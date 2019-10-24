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
namespace Neo4Net.Collections.Helpers
{

	/// <summary>
	/// A <seealso cref="CachingIterator"/> which can more easily divide the items
	/// into pages, where optionally each page can be seen as its own
	/// <seealso cref="System.Collections.IEnumerator"/> instance for convenience using <seealso cref="nextPage()"/>.
	/// 
	/// @author Mattias Persson
	/// </summary>
	/// @param <T> the type of items in this iterator. </param>
	public class PagingIterator<T> : CachingIterator<T>
	{
		 private readonly int _pageSize;

		 /// <summary>
		 /// Creates a new paging iterator with {@code source} as its underlying
		 /// <seealso cref="System.Collections.IEnumerator"/> to lazily get items from.
		 /// </summary>
		 /// <param name="source"> the underlying <seealso cref="System.Collections.IEnumerator"/> to lazily get items from. </param>
		 /// <param name="pageSize"> the max number of items in each page. </param>
		 public PagingIterator( IEnumerator<T> source, int pageSize ) : base( source )
		 {
			  this._pageSize = pageSize;
		 }

		 /// <returns> the page the iterator is currently at, starting a {@code 0}.
		 /// This value is based on the <seealso cref="position()"/> and the page size. </returns>
		 public virtual int Page()
		 {
			  return Position() / _pageSize;
		 }

		 /// <summary>
		 /// Sets the current page of the iterator. {@code 0} means the first page. </summary>
		 /// <param name="newPage"> the current page to set for the iterator, must be
		 /// non-negative. The next item returned by the iterator will be the first
		 /// item in that page. </param>
		 /// <returns> the page before changing to the new page. </returns>
		 public virtual int Page( int newPage )
		 {
			  int previousPage = Page();
			  Position( newPage * _pageSize );
			  return previousPage;
		 }

		 /// <summary>
		 /// Returns a new <seealso cref="System.Collections.IEnumerator"/> instance which exposes the current page
		 /// as its own iterator, which fetches items lazily from the underlying
		 /// iterator. It is discouraged to use an <seealso cref="System.Collections.IEnumerator"/> returned from
		 /// this method at the same time as using methods like <seealso cref="next()"/> or
		 /// <seealso cref="previous()"/>, where the results may be unpredictable. So either
		 /// use only <seealso cref="nextPage()"/> (in conjunction with <seealso cref="page(int)"/> if
		 /// necessary) or go with regular <seealso cref="next()"/>/<seealso cref="previous()"/>.
		 /// </summary>
		 /// <returns> the next page as an <seealso cref="System.Collections.IEnumerator"/>. </returns>
		 public virtual IEnumerator<T> NextPage()
		 {
			  Page( Page() );
			  return new PrefetchingIteratorAnonymousInnerClass( this );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<T>
		 {
			 private readonly PagingIterator<T> _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( PagingIterator<T> outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 end = outerInstance.Position() + outerInstance.pageSize;
			 }

			 private readonly int end;

			 protected internal override T fetchNextOrNull()
			 {
				  if ( outerInstance.Position() >= end )
				  {
						return default( T );
				  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _outerInstance.hasNext() ? _outerInstance.next() : default(T);
			 }
		 }
	}

}