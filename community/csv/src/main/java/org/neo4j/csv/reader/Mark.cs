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
namespace Org.Neo4j.Csv.Reader
{

	/// <summary>
	/// A mutable marker that is changed to hold progress made to a <seealso cref="BufferedCharSeeker"/>.
	/// It holds information such as start/end position in the data stream, which character
	/// was the match and whether or not this denotes the last value of the line.
	/// </summary>
	public class Mark
	{
		 public const int END_OF_LINE_CHARACTER = -1;

		 private int _startPosition;
		 private int _position;
		 private int _character;
		 private bool _quoted;

		 /// <param name="startPosition"> position of first character in value (inclusive). </param>
		 /// <param name="position"> position of last character in value (exclusive). </param>
		 /// <param name="character"> use {@code -1} to denote that the matching character was an end-of-line or end-of-file </param>
		 /// <param name="quoted"> whether or not the original data was quoted. </param>
		 internal virtual void Set( int startPosition, int position, int character, bool quoted )
		 {
			  this._startPosition = startPosition;
			  this._position = position;
			  this._character = character;
			  this._quoted = quoted;
		 }

		 public virtual int Character()
		 {
			  Debug.Assert( !EndOfLine );
			  return _character;
		 }

		 public virtual bool EndOfLine
		 {
			 get
			 {
				  return _character == -1;
			 }
		 }

		 public virtual bool Quoted
		 {
			 get
			 {
				  return _quoted;
			 }
		 }

		 internal virtual int Position()
		 {
			  if ( _position == -1 )
			  {
					throw new System.InvalidOperationException( "No value to extract here" );
			  }
			  return _position;
		 }

		 internal virtual int StartPosition()
		 {
			  if ( _startPosition == -1 )
			  {
					throw new System.InvalidOperationException( "No value to extract here" );
			  }
			  return _startPosition;
		 }

		 internal virtual int Length()
		 {
			  return _position - _startPosition;
		 }

		 public override string ToString()
		 {
			  return format( "Mark[from:%d, to:%d, quoted:%b]", _startPosition, _position, _quoted );
		 }
	}

}