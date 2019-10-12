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
namespace Neo4Net.Graphdb
{
	/// <summary>
	/// An input position refers to a specific point in a query string.
	/// </summary>
	public sealed class InputPosition
	{
		 private readonly int _offset;
		 private readonly int _line;
		 private readonly int _column;

		 /// <summary>
		 /// The empty position
		 /// </summary>
		 public static readonly InputPosition Empty = new InputPosition( -1, -1, -1 );

		 /// <summary>
		 /// Creating a position from and offset, line number and a column number. </summary>
		 /// <param name="offset"> the offset from the start of the string, starting from 0. </param>
		 /// <param name="line"> the line number, starting from 1. </param>
		 /// <param name="column"> the column number, starting from 1. </param>
		 public InputPosition( int offset, int line, int column )
		 {
			  this._offset = offset;
			  this._line = line;
			  this._column = column;
		 }

		 /// <summary>
		 /// The character offset referred to by this position; offset numbers start at 0. </summary>
		 /// <returns> the offset of this position. </returns>
		 public int Offset
		 {
			 get
			 {
				  return _offset;
			 }
		 }

		 /// <summary>
		 /// The line number referred to by the position; line numbers start at 1. </summary>
		 /// <returns> the line number of this position. </returns>
		 public int Line
		 {
			 get
			 {
				  return _line;
			 }
		 }

		 /// <summary>
		 /// The column number referred to by the position; column numbers start at 1. </summary>
		 /// <returns> the column number of this position. </returns>
		 public int Column
		 {
			 get
			 {
				  return _column;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  InputPosition that = ( InputPosition ) o;

			  if ( _offset != that._offset )
			  {
					return false;
			  }
			  if ( _line != that._line )
			  {
					return false;
			  }
			  return _column == that._column;

		 }

		 public override int GetHashCode()
		 {
			  int result = _offset;
			  result = 31 * result + _line;
			  result = 31 * result + _column;
			  return result;
		 }

		 public override string ToString()
		 {
			  return "InputPosition{" + "offset=" + _offset + ", line=" + _line + ", column=" + _column + '}';
		 }
	}

}