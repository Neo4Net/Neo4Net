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
namespace Neo4Net.Helpers.Collections
{

	public class ReverseArrayIterator<T> : IEnumerator<T>
	{
		 private readonly T[] _array;
		 private int _index;

		 public ReverseArrayIterator( T[] array )
		 {
			  this._array = array;
			  this._index = array.Length - 1;
		 }

		 public override bool HasNext()
		 {
			  return _index >= 0;
		 }

		 public override T Next()
		 {
			  if ( !HasNext() )
			  {
					throw new NoSuchElementException();
			  }
			  return _array[_index--];
		 }

		 public override void Remove()
		 {
			  throw new System.NotSupportedException();
		 }
	}

}