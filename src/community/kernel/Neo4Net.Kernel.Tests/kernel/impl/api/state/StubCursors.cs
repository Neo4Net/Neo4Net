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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	using Neo4Net.Cursors;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

	/// <summary>
	/// Stub cursors to be used for testing.
	/// </summary>
	public class StubCursors
	{
		 private StubCursors()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.eclipse.collections.api.set.primitive.MutableLongSet labels(final long... labels)
		 public static MutableLongSet Labels( params long[] labels )
		 {
			  return LongHashSet.newSetWith( labels );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> org.neo4j.cursor.Cursor<T> cursor(final T... items)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static ICursor<T> ICursor<T>( params T[] items )
		 {
			  return Cursor( Iterables.asIterable( items ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> org.neo4j.cursor.Cursor<T> cursor(final Iterable<T> items)
		 public static ICursor<T> ICursor<T>( IEnumerable<T> items )
		 {
			  return new CursorAnonymousInnerClass( items );
		 }

		 private class CursorAnonymousInnerClass : ICursor<T>
		 {
			 private IEnumerable<T> _items;

			 public CursorAnonymousInnerClass( IEnumerable<T> items )
			 {
				 this._items = items;
			 }

			 internal IEnumerator<T> iterator = _items.GetEnumerator();

			 internal T current;

			 public bool next()
			 {
				  if ( iterator.hasNext() )
				  {
						current = iterator.next();
						return true;
				  }
				  else
				  {
						return false;
				  }
			 }

			 public void close()
			 {
				  iterator = _items.GetEnumerator();
				  current = null;
			 }

			 public T get()
			 {
				  if ( current == null )
				  {
						throw new System.InvalidOperationException();
				  }

				  return current;
			 }
		 }
	}

}