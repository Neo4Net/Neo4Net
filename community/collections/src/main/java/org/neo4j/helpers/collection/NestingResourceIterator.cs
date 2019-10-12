using System.Collections.Generic;

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
namespace Org.Neo4j.Helpers.Collection
{

	using Org.Neo4j.Graphdb;

	public abstract class NestingResourceIterator<T, U> : PrefetchingResourceIterator<T>
	{
		 private readonly IEnumerator<U> _source;
		 private ResourceIterator<T> _currentNestedIterator;
		 private U _currentSurfaceItem;

		 public NestingResourceIterator( IEnumerator<U> source )
		 {
			  this._source = source;
		 }

		 protected internal abstract ResourceIterator<T> CreateNestedIterator( U item );

		 protected internal override T FetchNextOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _currentNestedIterator == null || !_currentNestedIterator.hasNext() )
			  {
					while ( _source.MoveNext() )
					{
						 _currentSurfaceItem = _source.Current;
						 Close();
						 _currentNestedIterator = CreateNestedIterator( _currentSurfaceItem );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( _currentNestedIterator.hasNext() )
						 {
							  break;
						 }
					}
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _currentNestedIterator != null && _currentNestedIterator.hasNext() ? _currentNestedIterator.next() : default(T);
		 }

		 public override void Close()
		 {
			  if ( _currentNestedIterator != null )
			  {
					_currentNestedIterator.close();
			  }
		 }
	}

}