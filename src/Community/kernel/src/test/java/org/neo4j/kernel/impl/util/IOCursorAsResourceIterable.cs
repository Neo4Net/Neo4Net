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
namespace Neo4Net.Kernel.impl.util
{

	using Neo4Net.Cursors;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;

	public class IOCursorAsResourceIterable<T> : ResourceIterable<T>
	{
		 private readonly IOCursor<T> _cursor;

		 public IOCursorAsResourceIterable( IOCursor<T> cursor )
		 {
			  this._cursor = cursor;
		 }

		 public override ResourceIterator<T> Iterator()
		 {
			  try
			  {
					if ( _cursor.next() )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final T first = cursor.get();
						 T first = _cursor.get();

						 return new ResourceIteratorAnonymousInnerClass( this, first );
					}

					_cursor.close();
					return Iterators.asResourceIterator( Collections.emptyIterator() );
			  }
			  catch ( IOException )
			  {
					return Iterators.asResourceIterator( Collections.emptyIterator() );
			  }
		 }

		 private class ResourceIteratorAnonymousInnerClass : ResourceIterator<T>
		 {
			 private readonly IOCursorAsResourceIterable<T> _outerInstance;

			 private T _first;

			 public ResourceIteratorAnonymousInnerClass( IOCursorAsResourceIterable<T> outerInstance, T first )
			 {
				 this.outerInstance = outerInstance;
				 this._first = first;
				 instance = first;
			 }

			 internal T instance;

			 public bool hasNext()
			 {
				  return instance != null;
			 }

			 public T next()
			 {
				  try
				  {
						return instance;
				  }
				  finally
				  {
						try
						{
							 if ( _outerInstance.cursor.next() )
							 {
								  instance = _outerInstance.cursor.get();
							 }
							 else
							 {
								  _outerInstance.cursor.close();
								  instance = null;
							 }
						}
						catch ( IOException )
						{
							 instance = null;
						}
				  }
			 }

			 public void remove()
			 {
				  throw new System.NotSupportedException();
			 }

			 public void close()
			 {
				  try
				  {
						_outerInstance.cursor.close();
				  }
				  catch ( IOException )
				  {
						// Ignore
				  }
			 }
		 }
	}

}