using System;

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
namespace Neo4Net.Adversaries.fs
{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AdversarialReader extends java.io.Reader
	public class AdversarialReader : Reader
	{
		 private readonly Reader _reader;
		 private readonly Adversary _adversary;

		 public AdversarialReader( Reader reader, Adversary adversary )
		 {
			  this._reader = reader;
			  this._adversary = adversary;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(java.nio.CharBuffer target) throws java.io.IOException
		 public override int Read( CharBuffer target )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ), typeof( BufferOverflowException ), typeof( System.IndexOutOfRangeException ) ) )
			  {
					CharBuffer dup = target.duplicate();
					dup.limit( Math.Max( target.limit() / 2, 1 ) );
					return _reader.read( dup );
			  }
			  return _reader.read( target );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read() throws java.io.IOException
		 public override int Read()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return _reader.read();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(char[] cbuf) throws java.io.IOException
		 public override int Read( char[] cbuf )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					char[] dup = new char[Math.Max( cbuf.Length / 2, 1 )];
					int read = _reader.read( dup );
					Array.Copy( dup, 0, cbuf, 0, read );
					return read;
			  }
			  return _reader.read( cbuf );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(char[] cbuf, int off, int len) throws java.io.IOException
		 public override int Read( char[] cbuf, int off, int len )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ) ) )
			  {
					return _reader.read( cbuf, off, Math.Max( len / 2, 1 ) );
			  }
			  return _reader.read( cbuf, off, len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long skip(long n) throws java.io.IOException
		 public override long Skip( long n )
		 {
			  _adversary.injectFailure( typeof( System.ArgumentException ), typeof( IOException ) );
			  return _reader.skip( n );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean ready() throws java.io.IOException
		 public override bool Ready()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return _reader.ready();
		 }

		 public override bool MarkSupported()
		 {
			  _adversary.injectFailure();
			  return _reader.markSupported();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mark(int readAheadLimit) throws java.io.IOException
		 public override void Mark( int readAheadLimit )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _reader.mark( readAheadLimit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void reset() throws java.io.IOException
		 public override void Reset()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _reader.reset();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _reader.close();
		 }
	}

}