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
namespace Neo4Net.Adversaries.fs
{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AdversarialOutputStream extends java.io.OutputStream
	public class AdversarialOutputStream : Stream
	{
		 private readonly Stream _outputStream;
		 private readonly Adversary _adversary;

		 public AdversarialOutputStream( Stream outputStream, Adversary adversary )
		 {
			  this._outputStream = outputStream;
			  this._adversary = adversary;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(int b) throws java.io.IOException
		 public override void Write( int b )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _outputStream.WriteByte( b );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b) throws java.io.IOException
		 public override void Write( sbyte[] b )
		 {
			  _adversary.injectFailure( typeof( System.NullReferenceException ), typeof( System.IndexOutOfRangeException ), typeof( IOException ) );
			  _outputStream.Write( b, 0, b.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b, int off, int len) throws java.io.IOException
		 public override void Write( sbyte[] b, int off, int len )
		 {
			  _adversary.injectFailure( typeof( System.NullReferenceException ), typeof( System.IndexOutOfRangeException ), typeof( IOException ) );
			  _outputStream.Write( b, off, len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _outputStream.Flush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _outputStream.Close();
		 }
	}

}