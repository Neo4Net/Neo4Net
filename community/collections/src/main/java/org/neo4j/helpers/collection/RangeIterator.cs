﻿/*
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
	/// <summary>
	/// Iterates over a range, where the start value is inclusive, but the
	/// end value is exclusive.
	/// </summary>
	public class RangeIterator : PrefetchingIterator<int>
	{
		 private int _current;
		 private readonly int _end;
		 private readonly int _stride;

		 public RangeIterator( int end ) : this( 0, end )
		 {
		 }

		 public RangeIterator( int start, int end ) : this( start, end, 1 )
		 {
		 }

		 public RangeIterator( int start, int end, int stride )
		 {
			  this._current = start;
			  this._end = end;
			  this._stride = stride;
		 }

		 protected internal override int? FetchNextOrNull()
		 {
			  try
			  {
					return _current < _end ? _current : null;
			  }
			  finally
			  {
					_current += _stride;
			  }
		 }
	}

}