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
	/// Wraps a <seealso cref="Reader"/> into a <seealso cref="CharReadable"/>.
	/// </summary>
	internal class WrappedCharReadable : CharReadable_Adapter
	{
		 private readonly long _length;
		 private readonly Reader _reader;
		 private long _position;
		 private readonly string _sourceDescription;

		 internal WrappedCharReadable( long length, Reader reader )
		 {
			  this._length = length;
			  this._reader = reader;
			  _sourceDescription = reader.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SectionedCharBuffer read(SectionedCharBuffer buffer, int from) throws java.io.IOException
		 public override SectionedCharBuffer Read( SectionedCharBuffer buffer, int from )
		 {
			  buffer.Compact( buffer, from );
			  buffer.ReadFrom( _reader );
			  _position += buffer.Available();
			  return buffer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(char[] into, int offset, int length) throws java.io.IOException
		 public override int Read( char[] into, int offset, int length )
		 {
			  int totalRead = 0;
			  bool eof = false;
			  while ( totalRead < length )
			  {
					int read = _reader.read( into, offset + totalRead, length - totalRead );
					if ( read == -1 )
					{
						 eof = true;
						 break;
					}
					totalRead += read;
			  }
			  _position += totalRead;
			  return totalRead == 0 && eof ? -1 : totalRead;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _reader.close();
		 }

		 public override long Position()
		 {
			  return _position;
		 }

		 public override string SourceDescription()
		 {
			  return _sourceDescription;
		 }

		 public override long Length()
		 {
			  return _length;
		 }

		 public override string ToString()
		 {
			  return _sourceDescription;
		 }
	}

}