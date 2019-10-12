using System;

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
namespace Neo4Net.Commandline.Admin
{

	using IOUtils = Neo4Net.Io.IOUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	public class RealOutsideWorld : OutsideWorld
	{
		 internal FileSystemAbstraction FileSystemAbstraction = new DefaultFileSystemAbstraction();
		 private readonly Stream @in;
		 private readonly PrintStream @out;
		 private readonly PrintStream _err;

		 public RealOutsideWorld() : this(System.out, System.err, System.in)
		 {
		 }

		 public RealOutsideWorld( PrintStream @out, PrintStream err, Stream inStream )
		 {
			  this.@in = inStream;
			  this.@out = @out;
			  this._err = err;
		 }

		 public override void StdOutLine( string text )
		 {
			  @out.println( text );
		 }

		 public override void StdErrLine( string text )
		 {
			  _err.println( text );
		 }

		 public override string ReadLine()
		 {
			  return System.console().readLine();
		 }

		 public override string PromptLine( string fmt, params object[] args )
		 {
			  return System.console().readLine(fmt, args);
		 }

		 public override char[] PromptPassword( string fmt, params object[] args )
		 {
			  return System.console().readPassword(fmt, args);
		 }

		 public override void Exit( int status )
		 {
			  IOUtils.closeAllSilently( this );
			  Environment.Exit( status );
		 }

		 public override void PrintStacktrace( Exception exception )
		 {
			  Console.WriteLine( exception.ToString() );
			  Console.Write( exception.StackTrace );
		 }

		 public override FileSystemAbstraction FileSystem()
		 {
			  return FileSystemAbstraction;
		 }

		 public override PrintStream ErrorStream()
		 {
			  return _err;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  FileSystemAbstraction.Dispose();
		 }

		 public override PrintStream OutStream()
		 {
			  return @out;
		 }

		 public override Stream InStream()
		 {
			  return @in;
		 }
	}

}