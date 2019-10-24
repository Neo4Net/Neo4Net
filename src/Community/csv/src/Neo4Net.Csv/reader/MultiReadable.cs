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
namespace Neo4Net.Csv.Reader
{

	using Neo4Net.Collections;

	/// <summary>
	/// Joins multiple <seealso cref="CharReadable"/> into one. There will never be one read which reads from multiple sources.
	/// If the end of one source is reached those (smaller amount of) characters are returned as one read and the next
	/// read will start reading from the new source.
	/// 
	/// Newline will be injected in between two sources, even if the former doesn't end with such. This to not have the
	/// last line in the former and first in the latter to look like one long line, if reading characters off of this
	/// reader character by character (w/o knowing that there are multiple sources underneath).
	/// </summary>
	public class MultiReadable : CharReadable
	{
		 private readonly RawIterator<CharReadable, IOException> _actual;

		 private CharReadable _current = CharReadable.EMPTY;
		 private bool _requiresNewLine;
		 private long _previousPosition;

		 public MultiReadable( RawIterator<CharReadable, IOException> readers )
		 {
			  this._actual = readers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  CloseCurrent();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeCurrent() throws java.io.IOException
		 private void CloseCurrent()
		 {
			  if ( _current != null )
			  {
					_current.Dispose();
			  }
		 }

		 public override string SourceDescription()
		 {
			  return _current.sourceDescription();
		 }

		 public override long Position()
		 {
			  return _previousPosition + _current.position();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean goToNextSource() throws java.io.IOException
		 private bool GoToNextSource()
		 {
			  if ( _actual.hasNext() )
			  {
					if ( _current != null )
					{
						 _previousPosition += _current.position();
					}
					CloseCurrent();
					_current = _actual.next();
					return true;
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SectionedCharBuffer read(SectionedCharBuffer buffer, int from) throws java.io.IOException
		 public override SectionedCharBuffer Read( SectionedCharBuffer buffer, int from )
		 {
			  while ( true )
			  {
					_current.read( buffer, from );
					if ( buffer.HasAvailable() )
					{
						 // OK we read something from the current reader
						 CheckNewLineRequirement( buffer.Array(), buffer.Front() - 1 );
						 return buffer;
					}

					// Even if there's no line-ending at the end of this source we should introduce one
					// otherwise the last line of this source and the first line of the next source will
					// look like one long line.
					if ( _requiresNewLine )
					{
						 buffer.Append( '\n' );
						 _requiresNewLine = false;
						 return buffer;
					}

					if ( !GoToNextSource() )
					{
						 break;
					}
					from = buffer.Pivot();
			  }
			  return buffer;
		 }

		 private void CheckNewLineRequirement( char[] array, int lastIndex )
		 {
			  char lastChar = array[lastIndex];
			  _requiresNewLine = lastChar != '\n' && lastChar != '\r';
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(char[] into, int offset, int length) throws java.io.IOException
		 public override int Read( char[] into, int offset, int length )
		 {
			  int totalRead = 0;
			  while ( totalRead < length )
			  {
					int read = _current.read( into, offset + totalRead, length - totalRead );
					if ( read == -1 )
					{
						 if ( totalRead > 0 )
						 {
							  // Something has been read, but we couldn't fulfill the request with the current source.
							  // Return what we've read so far so that we don't mix multiple sources into the same read,
							  // for source traceability reasons.
							  return totalRead;
						 }

						 if ( !GoToNextSource() )
						 {
							  break;
						 }

						 if ( _requiresNewLine )
						 {
							  into[offset + totalRead] = '\n';
							  totalRead++;
							  _requiresNewLine = false;
						 }
					}
					else if ( read > 0 )
					{
						 totalRead += read;
						 CheckNewLineRequirement( into, offset + totalRead - 1 );
					}
			  }
			  return totalRead;
		 }

		 public override long Length()
		 {
			  return _current.length();
		 }
	}

}