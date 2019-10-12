using System;
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
namespace Neo4Net.Helpers.Collection
{

	public abstract class CatchingIteratorWrapper<T, U> : PrefetchingIterator<T>
	{
		 private readonly IEnumerator<U> _source;

		 public CatchingIteratorWrapper( IEnumerator<U> source )
		 {
			  this._source = source;
		 }

		 protected internal override T FetchNextOrNull()
		 {
			  while ( _source.MoveNext() )
			  {
					U nextItem = default( U );
					try
					{
						 nextItem = FetchNextOrNullFromSource( _source );
						 if ( nextItem != default( U ) )
						 {
							  return UnderlyingObjectToObject( nextItem );
						 }
					}
					catch ( Exception t )
					{
						 if ( ExceptionOk( t ) )
						 {
							  ItemDodged( nextItem );
							  continue;
						 }
						 if ( t is Exception )
						 {
							  throw ( Exception ) t;
						 }
						 else if ( t is Exception )
						 {
							  throw ( Exception ) t;
						 }
						 throw new Exception( t );
					}
			  }
			  return default( T );
		 }

		 protected internal virtual U FetchNextOrNullFromSource( IEnumerator<U> source )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return source.next();
		 }

		 protected internal virtual void ItemDodged( U item )
		 {
		 }

		 protected internal virtual bool ExceptionOk( Exception t )
		 {
			  return true;
		 }

		 protected internal abstract T UnderlyingObjectToObject( U @object );
	}

}