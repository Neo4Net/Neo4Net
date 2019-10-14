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
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AdversarialInputStream extends java.io.InputStream
	public class AdversarialInputStream : Stream
	{
		 private readonly Stream _inputStream;
		 private readonly Adversary _adversary;

		 public AdversarialInputStream( Stream inputStream, Adversary adversary )
		 {
			  this._inputStream = inputStream;
			  this._adversary = adversary;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read() throws java.io.IOException
		 public override int Read()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return _inputStream.Read();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(byte[] b) throws java.io.IOException
		 public override int Read( sbyte[] b )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ), typeof( System.NullReferenceException ) ) )
			  {
					sbyte[] dup = new sbyte[Math.Max( b.Length / 2, 1 )];
					int read = _inputStream.Read( dup, 0, dup.Length );
					Array.Copy( dup, 0, b, 0, read );
					return read;
			  }
			  return _inputStream.Read( b, 0, b.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(byte[] b, int off, int len) throws java.io.IOException
		 public override int Read( sbyte[] b, int off, int len )
		 {
			  if ( _adversary.injectFailureOrMischief( typeof( IOException ), typeof( System.NullReferenceException ), typeof( System.IndexOutOfRangeException ) ) )
			  {
					int halflen = Math.Max( len / 2, 1 );
					return _inputStream.Read( b, off, halflen );
			  }
			  return _inputStream.Read( b, off, len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long skip(long n) throws java.io.IOException
		 public override long Skip( long n )
		 {
			  _adversary.injectFailure( typeof( IOException ), typeof( System.NullReferenceException ), typeof( System.IndexOutOfRangeException ) );
			  return _inputStream.skip( n );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int available() throws java.io.IOException
		 public override int Available()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return _inputStream.available();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _inputStream.Close();
		 }

		 public override void Mark( int readlimit )
		 {
			  _adversary.injectFailure();
			  _inputStream.mark( readlimit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void reset() throws java.io.IOException
		 public override void Reset()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _inputStream.reset();
		 }

		 public override bool MarkSupported()
		 {
			  _adversary.injectFailure();
			  return _inputStream.markSupported();
		 }
	}

}