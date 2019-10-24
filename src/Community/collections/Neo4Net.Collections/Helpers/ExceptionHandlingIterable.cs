using System;
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
	/// allows to catch, analyze and react on exceptions that are thrown by the delegate iterable
	/// useful for exception conversion on iterator methods
	/// Uses sun.misc.Unsafe internally to rethrow original exceptions !
	/// </summary>
	/// @param <T> the type of elements </param>
	public class ExceptionHandlingIterable<T> : IEnumerable<T>
	{
		 private readonly IEnumerable<T> _source;

		 public ExceptionHandlingIterable( IEnumerable<T> source )
		 {
			  this._source = source;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <T extends Throwable> void sneakyThrow(Throwable throwable) throws T
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static void SneakyThrow<T>( Exception throwable ) where T : Exception
		 {
			  throw ( T ) throwable;
		 }

		 public override IEnumerator<T> Iterator()
		 {
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<T> it = source.iterator();
					IEnumerator<T> it = _source.GetEnumerator();
					return new IteratorAnonymousInnerClass( this, it );
			  }
			  catch ( Exception t )
			  {
					return ExceptionOnIterator( t );
			  }
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private readonly ExceptionHandlingIterable<T> _outerInstance;

			 private IEnumerator<T> _it;

			 public IteratorAnonymousInnerClass( ExceptionHandlingIterable<T> outerInstance, IEnumerator<T> it )
			 {
				 this.outerInstance = outerInstance;
				 this._it = it;
			 }

			 public bool hasNext()
			 {
				  try
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return _it.hasNext();
				  }
				  catch ( Exception t )
				  {
						return outerInstance.ExceptionOnHasNext( t );
				  }
			 }

			 public T next()
			 {
				  try
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return _it.next();
				  }
				  catch ( Exception t )
				  {
						return outerInstance.ExceptionOnNext( t );
				  }
			 }

			 public void remove()
			 {
				  try
				  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						_it.remove();
				  }
				  catch ( Exception t )
				  {
						outerInstance.ExceptionOnRemove( t );
				  }
			 }
		 }

		 protected internal virtual void Rethrow( Exception t )
		 {
			  // TODO it's pretty bad that we have to do this. We should refactor our exception hierarchy
			  // to eliminate the need for this hack.
			  ExceptionHandlingIterable.SneakyThrow( t );
		 }

		 protected internal virtual bool ExceptionOnHasNext( Exception t )
		 {
			  Rethrow( t );
			  return false;
		 }

		 protected internal virtual void ExceptionOnRemove( Exception t )
		 {
		 }

		 protected internal virtual T ExceptionOnNext( Exception t )
		 {
			  Rethrow( t );
			  return default( T );
		 }

		 protected internal virtual IEnumerator<T> ExceptionOnIterator( Exception t )
		 {
			  Rethrow( t );
			  return null;
		 }
	}

}