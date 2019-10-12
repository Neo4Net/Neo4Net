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

	/// <summary>
	/// Wraps an <seealso cref="System.Collections.IEnumerable"/> so that it returns items of another type. The
	/// iteration is done lazily.
	/// </summary>
	/// @param <T> the type of items to return </param>
	/// @param <U> the type of items to wrap/convert from </param>
	public abstract class IterableWrapper<T, U> : IEnumerable<T>
	{
		 private IEnumerable<U> _source;

		 public IterableWrapper( IEnumerable<U> iterableToWrap )
		 {
			  this._source = iterableToWrap;
		 }

		 protected internal abstract T UnderlyingObjectToObject( U @object );

		 public override IEnumerator<T> Iterator()
		 {
			  return new MyIteratorWrapper( this, _source.GetEnumerator() );
		 }

		 private class MyIteratorWrapper : IteratorWrapper<T, U>
		 {
			 private readonly IterableWrapper<T, U> _outerInstance;

			  internal MyIteratorWrapper( IterableWrapper<T, U> outerInstance, IEnumerator<U> iteratorToWrap ) : base( iteratorToWrap )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override T UnderlyingObjectToObject( U @object )
			  {
					return _outerInstance.underlyingObjectToObject( @object );
			  }
		 }
	}

}