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
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;

	/// <summary>
	/// Wraps an <seealso cref="ResourceIterable"/> so that it returns items of another type. The
	/// iteration is done lazily.
	/// </summary>
	/// @param <T> the type of items to return </param>
	/// @param <U> the type of items to wrap/convert from </param>
	public abstract class ResourceIterableWrapper<T, U> : ResourceIterable<T>
	{
		public abstract java.util.stream.Stream<T> Stream();
		 private ResourceIterable<U> _source;

		 public ResourceIterableWrapper( ResourceIterable<U> source )
		 {
			  this._source = source;
		 }

		 protected internal abstract T Map( U @object );

		 public override ResourceIterator<T> Iterator()
		 {
			  return new MappingResourceIteratorAnonymousInnerClass( this, _source.GetEnumerator() );
		 }

		 private class MappingResourceIteratorAnonymousInnerClass : MappingResourceIterator<T, U>
		 {
			 private readonly ResourceIterableWrapper<T, U> _outerInstance;

			 public MappingResourceIteratorAnonymousInnerClass( ResourceIterableWrapper<T, U> outerInstance, ResourceIterator<U> iterator ) : base( iterator )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override T map( U @object )
			 {
				  return _outerInstance.map( @object );
			 }
		 }
	}

}