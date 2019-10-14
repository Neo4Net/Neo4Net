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

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	public interface OutsideWorld : System.IDisposable
	{
		 void StdOutLine( string text );

		 void StdErrLine( string text );

		 /// <seealso cref= java.io.Console#readLine() </seealso>
		 string ReadLine();

		 /// <seealso cref= java.io.Console#readLine(String, Object...) </seealso>
		 string PromptLine( string fmt, params object[] args );

		 /// <summary>
		 /// It is strongly advised that the return character array is overwritten as soon as the password has been processed,
		 /// to avoid having it linger in memory any longer than strictly necessary.
		 /// </summary>
		 /// <seealso cref= java.io.Console#readPassword(String, Object...) </seealso>
		 char[] PromptPassword( string fmt, params object[] args );

		 void Exit( int status );

		 void PrintStacktrace( Exception exception );

		 FileSystemAbstraction FileSystem();

		 PrintStream ErrorStream();

		 PrintStream OutStream();

		 Stream InStream();
	}

}