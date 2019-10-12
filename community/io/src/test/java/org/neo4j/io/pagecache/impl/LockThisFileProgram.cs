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
namespace Org.Neo4j.Io.pagecache.impl
{

	public class LockThisFileProgram
	{
		 public const string LOCKED_OUTPUT = "locked";

		 private LockThisFileProgram()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  Path path = Paths.get( args[0] );
			  using ( FileChannel channel = FileChannel.open( path, StandardOpenOption.READ, StandardOpenOption.WRITE ), java.nio.channels.FileLock @lock = channel.@lock() )
			  {
					Console.WriteLine( LOCKED_OUTPUT );
					System.out.flush();
					Console.Read();
			  }
		 }
	}

}