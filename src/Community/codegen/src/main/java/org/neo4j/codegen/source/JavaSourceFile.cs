using System;
using System.Text;

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
namespace Neo4Net.Codegen.source
{

	internal class JavaSourceFile : SimpleJavaFileObject
	{
		 private readonly StringBuilder _content;

		 internal JavaSourceFile( URI uri, StringBuilder content ) : base( uri, SOURCE )
		 {
			  this._content = content;
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + toUri() + "]";
		 }

		 public override CharSequence GetCharContent( bool ignoreEncodingErrors )
		 {
			  return _content;
		 }

		 /// <summary>
		 /// Reads characters into an array.
		 /// </summary>
		 /// <param name="pos"> The position of this file to start reading from </param>
		 /// <param name="cbuf"> Destination buffer </param>
		 /// <param name="off"> Offset at which to start storing characters </param>
		 /// <param name="len"> Maximum number of characters to read (> 0) </param>
		 /// <returns> The number of characters read (0 if no characters remain) </returns>
		 /// <seealso cref= java.io.Reader#read(char[], int, int) </seealso>
		 public virtual int Read( int pos, char[] cbuf, int off, int len )
		 {
			  len = Math.Min( _content.Length - pos, len );
			  _content.getChars( pos, pos + len, cbuf, off );
			  return len;
		 }
	}

}