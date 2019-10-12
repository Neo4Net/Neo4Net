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
namespace Org.Neo4j.Diagnostics
{

	/// <summary>
	/// Implements an <seealso cref="System.IO.Stream_Input"/> that keeps track of the progress. This assumes that the total size is available
	/// before reading starts.
	/// </summary>
	public class ProgressAwareInputStream : Stream
	{
		 private readonly OnProgressListener _listener;
		 private readonly Stream _wrappedInputStream;
		 private readonly long _size;
		 private long _totalRead;
		 private int _lastReportedPercent;

		 public ProgressAwareInputStream( Stream wrappedInputStream, long size, OnProgressListener listener )
		 {
			  this._wrappedInputStream = wrappedInputStream;
			  this._size = size;
			  this._listener = listener;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read() throws java.io.IOException
		 public override int Read()
		 {
			  int data = _wrappedInputStream.Read();
			  if ( data >= 0 )
			  {
					_totalRead += 1;
					RecalculatePercent();
			  }
			  return data;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(byte[] b) throws java.io.IOException
		 public override int Read( sbyte[] b )
		 {
			  int n = _wrappedInputStream.Read( b, 0, b.Length );
			  if ( n > 0 )
			  {
					_totalRead += n;
					RecalculatePercent();
			  }
			  return n;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(byte[] b, int offset, int length) throws java.io.IOException
		 public override int Read( sbyte[] b, int offset, int length )
		 {
			  int n = _wrappedInputStream.Read( b, offset, length );
			  if ( n > 0 )
			  {
					_totalRead += n;
					RecalculatePercent();
			  }
			  return n;
		 }

		 private void RecalculatePercent()
		 {
			  int percent = ( int )( _totalRead * 100 / _size );
			  if ( percent > 100 )
			  {
					percent = 100;
			  }
			  if ( percent < 0 )
			  {
					percent = 0;
			  }
			  if ( percent > _lastReportedPercent )
			  {
					_lastReportedPercent = percent;
					_listener.onProgress( percent );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long skip(long n) throws java.io.IOException
		 public override long Skip( long n )
		 {
			  return _wrappedInputStream.skip( n );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int available() throws java.io.IOException
		 public override int Available()
		 {
			  return _wrappedInputStream.available();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _wrappedInputStream.Close();
		 }

		 public override void Mark( int readLimit )
		 {
			  _wrappedInputStream.mark( readLimit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void reset() throws java.io.IOException
		 public override void Reset()
		 {
			  _wrappedInputStream.reset();
		 }

		 public override bool MarkSupported()
		 {
			  return _wrappedInputStream.markSupported();
		 }

		 /// <summary>
		 /// Interface for classes that want to monitor this input stream
		 /// </summary>
		 public interface OnProgressListener
		 {
			  void OnProgress( int percentage );
		 }
	}

}