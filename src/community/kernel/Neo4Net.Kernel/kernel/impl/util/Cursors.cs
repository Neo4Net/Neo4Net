﻿using System;
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
namespace Neo4Net.Kernel.impl.util
{

	using Neo4Net.Cursors;
	using Neo4Net.Cursors;
	using Neo4Net.Cursors;

	public class Cursors
	{
		 private static ICursor<object> EMPTY = new CursorAnonymousInnerClass();

		 private class CursorAnonymousInnerClass : ICursor<object>
		 {
			 public bool next()
			 {
				  return false;
			 }

			 public object get()
			 {
				  throw new System.InvalidOperationException( "no elements" );
			 }

			 public void close()
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private Cursors()
		 private Cursors()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> org.neo4j.cursor.Cursor<T> empty()
		 public static ICursor<T> Empty<T>()
		 {
			  return ( ICursor<T> ) EMPTY;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <E extends Exception> int count(org.neo4j.cursor.RawCursor<?,E> cursor) throws E
		 public static int Count<E, T1>( IRawCursor<T1> cursor ) where E : Exception
		 {
			  try
			  {
					int count = 0;
					while ( cursor.Next() )
					{
						 count++;
					}
					return count;
			  }
			  finally
			  {
					cursor.Close();
			  }
		 }

		 public static IRawCursor<T, EX> RawCursorOf<T, EX>( params T[] values ) where EX : Exception
		 {
			  return new RawCursorAnonymousInnerClass( values );
		 }

		 private class RawCursorAnonymousInnerClass : IRawCursor<T, EX>
		 {
			 private T[] _values;

			 public RawCursorAnonymousInnerClass( T[] values )
			 {
				 this._values = values;
			 }

			 private int idx;
			 private CursorValue<T> current = new CursorValue<T>();

			 public T get()
			 {
				  return current.get();
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws EX
			 public bool next()
			 {
				  if ( idx >= _values.Length )
				  {
						current.invalidate();
						return false;
				  }

				  current.set( _values[idx] );
				  idx++;

				  return true;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws EX
			 public void close()
			 {
				  idx = _values.Length;
				  current.invalidate();
			 }
		 }

		 public static IRawCursor<T, EX> RawCursorOf<T, EX>( IEnumerable<T> iterable ) where EX : Exception
		 {
			  return new RawCursorAnonymousInnerClass2( iterable );
		 }

		 private class RawCursorAnonymousInnerClass2 : IRawCursor<T, EX>
		 {
			 private IEnumerable<T> _iterable;

			 public RawCursorAnonymousInnerClass2( IEnumerable<T> iterable )
			 {
				 this._iterable = iterable;
			 }

			 private CursorValue<T> current = new CursorValue<T>();
			 private IEnumerator<T> itr = _iterable.GetEnumerator();

			 public T get()
			 {
				  return current.get();
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws EX
			 public bool next()
			 {
				  if ( itr.hasNext() )
				  {
						current.set( itr.next() );
						return true;
				  }
				  else
				  {
						current.invalidate();
						return false;
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws EX
			 public void close()
			 {
				  current.invalidate();
			 }
		 }
	}

}