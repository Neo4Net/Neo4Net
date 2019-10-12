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
namespace Org.Neo4j.Kernel.impl.util.collection
{

	using Org.Neo4j.Cursor;

	/// <summary>
	/// <seealso cref="Cursor"/> which moves over one or more arrays, automatically transitioning to the next
	/// array when one runs out of items.
	/// </summary>
	public class ContinuableArrayCursor<T> : Cursor<T>
	{
		 private readonly System.Func<T[]> _supplier;
		 private T[] _current;
		 private int _cursor;

		 public ContinuableArrayCursor( System.Func<T[]> supplier )
		 {
			  this._supplier = supplier;
		 }

		 public override bool Next()
		 {
			  while ( _current == null || _cursor >= _current.Length )
			  {
					_current = _supplier.get();
					if ( _current == null )
					{ // End reached
						 return false;
					}

					_cursor = 0;
			  }
			  _cursor++;
			  return true;
		 }

		 public override void Close()
		 {
			  // Do nothing
		 }

		 public override T Get()
		 {
			  if ( _current == null )
			  {
					throw new System.InvalidOperationException();
			  }
			  return _current[_cursor - 1];
		 }
	}

}