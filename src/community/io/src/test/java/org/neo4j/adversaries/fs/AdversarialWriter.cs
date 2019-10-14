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
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AdversarialWriter extends java.io.Writer
	public class AdversarialWriter : Writer
	{
		 private readonly Writer _writer;
		 private readonly Adversary _adversary;

		 public AdversarialWriter( Writer writer, Adversary adversary )
		 {
			  this._writer = writer;
			  this._adversary = adversary;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(int c) throws java.io.IOException
		 public override void Write( int c )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _writer.write( c );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(char[] cbuf) throws java.io.IOException
		 public override void Write( char[] cbuf )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _writer.write( cbuf );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(char[] cbuf, int off, int len) throws java.io.IOException
		 public override void Write( char[] cbuf, int off, int len )
		 {
			  _writer.write( cbuf, off, len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(String str) throws java.io.IOException
		 public override void Write( string str )
		 {
			  _adversary.injectFailure( typeof( StringIndexOutOfBoundsException ), typeof( IOException ), typeof( System.IndexOutOfRangeException ), typeof( ArrayStoreException ), typeof( System.NullReferenceException ) );
			  _writer.write( str );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(String str, int off, int len) throws java.io.IOException
		 public override void Write( string str, int off, int len )
		 {
			  _adversary.injectFailure( typeof( StringIndexOutOfBoundsException ), typeof( IOException ), typeof( System.IndexOutOfRangeException ), typeof( ArrayStoreException ), typeof( System.NullReferenceException ) );
			  _writer.write( str, off, len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer append(CharSequence csq) throws java.io.IOException
		 public override Writer Append( CharSequence csq )
		 {
			  _adversary.injectFailure( typeof( StringIndexOutOfBoundsException ), typeof( IOException ), typeof( System.IndexOutOfRangeException ), typeof( ArrayStoreException ), typeof( System.NullReferenceException ) );
			  return _writer.append( csq );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer append(CharSequence csq, int start, int end) throws java.io.IOException
		 public override Writer Append( CharSequence csq, int start, int end )
		 {
			  _adversary.injectFailure( typeof( StringIndexOutOfBoundsException ), typeof( IOException ), typeof( System.IndexOutOfRangeException ), typeof( ArrayStoreException ), typeof( System.NullReferenceException ) );
			  return _writer.append( csq, start, end );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer append(char c) throws java.io.IOException
		 public override Writer Append( char c )
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  return _writer.append( c );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _writer.flush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( IOException ) );
			  _writer.close();
		 }
	}

}