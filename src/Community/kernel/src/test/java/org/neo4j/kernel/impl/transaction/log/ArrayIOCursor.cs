using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using Neo4Net.Cursors;

	/// <summary>
	/// <seealso cref="IOCursor"/> abstraction over a given array
	/// </summary>
	public class ArrayIOCursor<T> : IOCursor<T>
	{
		 private readonly T[] _entries;
		 private int _pos;
		 private bool _closed;

		 public ArrayIOCursor( params T[] entries )
		 {
			  this._entries = entries;
		 }

		 public override T Get()
		 {
			  Debug.Assert( !_closed );
			  return _entries[_pos - 1];
		 }

		 public override bool Next()
		 {
			  Debug.Assert( !_closed );
			  return _pos++ < _entries.Length;
		 }

		 public override void Close()
		 {
			  _closed = true;
		 }
	}

}