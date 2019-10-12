using System;
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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;

	/// <summary>
	/// Has a similar role to a <seealso cref="CharBuffer"/>, but is tailored to how <seealso cref="BufferedCharSeeker"/>
	/// works and to be able to take full advantage of <seealso cref="ThreadAheadReadable"/>.
	/// 
	/// First of all this thing wraps a {@code char[]} where the array, while still being one array,
	/// is sectioned into two equally sized parts: the back and the front. The flow of things is as follows:
	/// <ol>
	/// <li>Characters are read from a <seealso cref="Reader"/> using <seealso cref="readFrom(Reader)"/> into the front section,
	/// i.e. starting from the middle of the array and forwards.</li>
	/// <li>Consumer of the read characters access the <seealso cref="array()"/> and starts reading from the <seealso cref="pivot()"/>
	/// point, which is the middle, to finally reach the end, denoted by <seealso cref="front()"/> (exclusive).
	/// During the reading typically characters are read into some sort of fields/values, and so the last characters
	/// of the array might represent an incomplete value.</li>
	/// <li>For this reason, before reading more values into the array, the characters making up the last incomplete
	/// value can be moved to the back section using <seealso cref="compact(SectionedCharBuffer, int)"/> where
	/// after that call <seealso cref="back()"/> will point to the index in the <seealso cref="array()"/> containing the first value
	/// in the array.</li>
	/// <li>Now more characters can be read into the front section using <seealso cref="readFrom(Reader)"/>.</li>
	/// </ol>
	/// 
	/// This divide into back and front section enables a behaviour in <seealso cref="ThreadAheadReadable"/> where the
	/// thread that reads ahead reads into the front section of another buffer, a double buffer,
	/// and the current buffer that <seealso cref="BufferedCharSeeker"/> is working with can
	/// <seealso cref="compact(SectionedCharBuffer, int)"/> its last incomplete characters into that double buffer
	/// and flip so that the <seealso cref="BufferedCharSeeker"/> continues to read from the other buffer, i.e. flips
	/// buffer every call to <seealso cref="ThreadAheadReadable.read(SectionedCharBuffer, int)"/>. Without these sections
	/// the entire double buffer would have to be copied into the char seekers buffer to get the same behavior.
	/// </summary>
	public class SectionedCharBuffer
	{
		 private readonly char[] _buffer;
		 private readonly int _pivot;
		 private int _back;
		 private int _front; // exclusive

		 /// <param name="effectiveBuffserSize"> Size of each section, i.e. effective buffer size that can be
		 /// <seealso cref="readFrom(Reader) read"/> each time. </param>
		 public SectionedCharBuffer( int effectiveBuffserSize )
		 {
			  this._buffer = new char[effectiveBuffserSize * 2];
			  this._back = this._front = this._pivot = effectiveBuffserSize;
		 }

		 /// <returns> the underlying array which characters are <seealso cref="readFrom(Reader) read into"/>.
		 /// <seealso cref="back()"/>, <seealso cref="pivot()"/> and <seealso cref="front()"/> marks the noteworthy indexes into this array. </returns>
		 public virtual char[] Array()
		 {
			  return _buffer;
		 }

		 /// <summary>
		 /// Copies characters in the <seealso cref="array()"/> from (and including) the given {@code from} index of the array
		 /// and all characters forwards to <seealso cref="front()"/> (excluding) index. These characters are copied into
		 /// the <seealso cref="array()"/> of the given {@code into} buffer, where the character {@code array[from]} will
		 /// be be copied to {@code into.array[pivot-(front-from)]}, and so on. As an example:
		 /// 
		 /// <pre>
		 /// pivot (i.e. effective buffer size) = 16
		 /// buffer A
		 /// &lt;------ back ------&gt; &lt;------ front -----&gt;
		 /// [    .    .    .    |abcd.efgh.ijkl.mnop]
		 ///                                 ^ = 25
		 /// 
		 /// A.compactInto( B, 25 )
		 /// 
		 /// buffer B
		 /// &lt;------ back ------&gt; &lt;------ front -----&gt;
		 /// [    .    . jkl.mnop|    .    .    .    ]
		 /// </pre>
		 /// </summary>
		 /// <param name="into"> which buffer to compact into. </param>
		 /// <param name="from"> the array index to start compacting from. </param>
		 public virtual void Compact( SectionedCharBuffer into, int from )
		 {
			  Debug.Assert( _buffer.Length == into._buffer.Length );
			  int diff = _front - from;
			  into._back = _pivot - diff;
			  Array.Copy( _buffer, from, into._buffer, into._back, diff );
		 }

		 /// <summary>
		 /// Reads characters from {@code reader} into the front section of this buffer, setting <seealso cref="front()"/>
		 /// accordingly afterwards. If no characters were read due to end reached then <seealso cref="hasAvailable()"/> will
		 /// return {@code false} after this call, likewise <seealso cref="available()"/> will return 0.
		 /// </summary>
		 /// <param name="reader"> <seealso cref="Reader"/> to read from. </param>
		 /// <exception cref="IOException"> any exception from the <seealso cref="Reader"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readFrom(java.io.Reader reader) throws java.io.IOException
		 public virtual void ReadFrom( Reader reader )
		 {
			  ReadFrom( reader, _pivot );
		 }

		 /// <summary>
		 /// Like <seealso cref="readFrom(Reader)"/> but with added {@code max} argument for limiting the number of
		 /// characters read from the <seealso cref="Reader"/>.
		 /// </summary>
		 /// <seealso cref= #readFrom(Reader) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readFrom(java.io.Reader reader, int max) throws java.io.IOException
		 public virtual void ReadFrom( Reader reader, int max )
		 {
			  int read = reader.read( _buffer, _pivot, min( max, _pivot ) );
			  if ( read == -1 )
			  { // we reached the end
					_front = _pivot;
			  }
			  else
			  { // we did read something
					_front = _pivot + read;
			  }
		 }

		 /// <summary>
		 /// Puts a character into the front section of the buffer and increments the front index. </summary>
		 /// <param name="ch"> </param>
		 public virtual void Append( char ch )
		 {
			  _buffer[_front++] = ch;
		 }

		 /// <returns> the pivot point of the <seealso cref="array()"/>. Before the pivot there are characters saved
		 /// from a previous <seealso cref="compact(SectionedCharBuffer, int) compaction"/> and after (and including) this point
		 /// are characters read from <seealso cref="readFrom(Reader)"/>. </returns>
		 public virtual int Pivot()
		 {
			  return _pivot;
		 }

		 /// <returns> index of first available character, might be before pivot point if there have been
		 /// characters moved over from a previous compaction. </returns>
		 public virtual int Back()
		 {
			  return _back;
		 }

		 /// <returns> index of the last available character plus one. </returns>
		 public virtual int Front()
		 {
			  return _front;
		 }

		 /// <returns> whether or not there are characters read into the front section of the buffer. </returns>
		 public virtual bool HasAvailable()
		 {
			  return _front > _pivot;
		 }

		 /// <returns> the number of characters available in the front section of the buffer. </returns>
		 public virtual int Available()
		 {
			  return _front - _pivot;
		 }
	}

}