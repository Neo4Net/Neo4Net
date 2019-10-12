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
namespace Neo4Net.Csv.Reader
{

	/// <summary>
	/// In a scenario where there's one thread, or perhaps a <seealso cref="ThreadAheadReadable"/> doing both the
	/// reading and parsing one <seealso cref="BufferedCharSeeker"/> is used over a stream of chunks, where the next
	/// chunk seamlessly transitions into the next, this class comes in handy. It uses a <seealso cref="CharReadable"/>
	/// and <seealso cref="SectionedCharBuffer"/> to do this.
	/// </summary>
	public class AutoReadingSource : Source
	{
		 private readonly CharReadable _reader;
		 private SectionedCharBuffer _charBuffer;

		 public AutoReadingSource( CharReadable reader, int bufferSize ) : this( reader, new SectionedCharBuffer( bufferSize ) )
		 {
		 }

		 public AutoReadingSource( CharReadable reader, SectionedCharBuffer charBuffer )
		 {
			  this._reader = reader;
			  this._charBuffer = charBuffer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Source_Chunk nextChunk(int seekStartPos) throws java.io.IOException
		 public override Source_Chunk NextChunk( int seekStartPos )
		 {
			  _charBuffer = _reader.read( _charBuffer, seekStartPos == -1 ? _charBuffer.pivot() : seekStartPos );

			  return new Source_ChunkAnonymousInnerClass( this );
		 }

		 private class Source_ChunkAnonymousInnerClass : Source_Chunk
		 {
			 private readonly AutoReadingSource _outerInstance;

			 public Source_ChunkAnonymousInnerClass( AutoReadingSource outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int startPosition()
			 {
				  return _outerInstance.charBuffer.pivot();
			 }

			 public string sourceDescription()
			 {
				  return _outerInstance.reader.sourceDescription();
			 }

			 public int backPosition()
			 {
				  return _outerInstance.charBuffer.back();
			 }

			 public int length()
			 {
				  return _outerInstance.charBuffer.available();
			 }

			 public int maxFieldSize()
			 {
				  return _outerInstance.charBuffer.pivot();
			 }

			 public char[] data()
			 {
				  return _outerInstance.charBuffer.array();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _reader.Dispose();
		 }
	}

}