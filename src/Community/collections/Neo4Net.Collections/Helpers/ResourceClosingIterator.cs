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

	using Resource = Neo4Net.GraphDb.Resource;
	using Neo4Net.GraphDb;
	using ResourceUtils = Neo4Net.GraphDb.ResourceUtils;

	public abstract class ResourceClosingIterator<T, V> : ResourceIterator<V>
	{
		public abstract ResourceIterator<R> Map( System.Func<T, R> map );
		public abstract java.util.stream.Stream<T> Stream();
		 /// @deprecated use <seealso cref="newResourceIterator(System.Collections.IEnumerator, Resource...)"/> 
		 [Obsolete("use <seealso cref=\"newResourceIterator(System.Collections.IEnumerator, Resource...)\"/>")]
		 public static ResourceIterator<R> NewResourceIterator<R>( Resource resource, IEnumerator<R> iterator )
		 {
			  return NewResourceIterator( iterator, resource );
		 }

		 public static ResourceIterator<R> NewResourceIterator<R>( IEnumerator<R> iterator, params Resource[] resources )
		 {
			  return new ResourceClosingIteratorAnonymousInnerClass( iterator, resources );
		 }

		 private class ResourceClosingIteratorAnonymousInnerClass : ResourceClosingIterator<R, R>
		 {
			 public ResourceClosingIteratorAnonymousInnerClass( IEnumerator<R> iterator, Resource[] resources ) : base( iterator, resources )
			 {
			 }

			 public override R map( R elem )
			 {
				  return elem;
			 }
		 }

		 private Resource[] _resources;
		 private readonly IEnumerator<T> _iterator;

		 internal ResourceClosingIterator( IEnumerator<T> iterator, params Resource[] resources )
		 {
			  this._resources = resources;
			  this._iterator = iterator;
		 }

		 public override void Close()
		 {
			  if ( _resources != null )
			  {
					ResourceUtils.closeAll( _resources );
					_resources = null;
			  }
		 }

		 public override bool HasNext()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  bool hasNext = _iterator.hasNext();
			  if ( !hasNext )
			  {
					Close();
			  }
			  return hasNext;
		 }

		 public abstract V Map( T elem );

		 public override V Next()
		 {
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return Map( _iterator.next() );
			  }
			  catch ( NoSuchElementException e )
			  {
					Close();
					throw e;
			  }
		 }

		 public override void Remove()
		 {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
			  _iterator.remove();
		 }
	}

}