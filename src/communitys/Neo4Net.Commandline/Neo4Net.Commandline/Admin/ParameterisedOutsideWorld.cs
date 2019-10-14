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
namespace Neo4Net.CommandLine.Admin
{

	using IOUtils = Neo4Net.Io.IOUtils;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	/// <summary>
	/// An outside world where you can pick and choose which input/output are dummies.
	/// </summary>
	public class ParameterisedOutsideWorld : OutsideWorld
	{

		 private readonly PrintStream _stdout;
		 private readonly PrintStream _stderr;
		 private readonly Console _console;
		 private readonly Stream _stdin;
		 private readonly FileSystemAbstraction _fileSystemAbstraction;

		 public ParameterisedOutsideWorld( Console console, Stream stdout, Stream stderr, Stream stdin, FileSystemAbstraction fileSystemAbstraction )
		 {
			  this._stdout = new PrintStream( stdout );
			  this._stderr = new PrintStream( stderr );
			  this._stdin = stdin;
			  this._fileSystemAbstraction = fileSystemAbstraction;
			  this._console = console;
		 }

		 public override void StdOutLine( string text )
		 {
			  _stdout.println( text );
		 }

		 public override void StdErrLine( string text )
		 {
			  _stderr.println( text );
		 }

		 public override string ReadLine()
		 {
			  return _console.readLine();
		 }

		 public override string PromptLine( string fmt, params object[] args )
		 {
			  return _console.readLine( fmt, args );
		 }

		 public override char[] PromptPassword( string fmt, params object[] args )
		 {
			  return _console.readPassword( fmt, args );
		 }

		 public override void Exit( int status )
		 {
			  IOUtils.closeAllSilently( this );
		 }

		 public override void PrintStacktrace( Exception exception )
		 {
			  exception.printStackTrace( _stderr );
		 }

		 public override FileSystemAbstraction FileSystem()
		 {
			  return _fileSystemAbstraction;
		 }

		 public override PrintStream ErrorStream()
		 {
			  return _stderr;
		 }

		 public override PrintStream OutStream()
		 {
			  return _stdout;
		 }

		 public override Stream InStream()
		 {
			  return _stdin;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _fileSystemAbstraction.Dispose();
		 }
	}

}