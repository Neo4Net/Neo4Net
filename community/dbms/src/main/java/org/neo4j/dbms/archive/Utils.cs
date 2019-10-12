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
namespace Org.Neo4j.Dbms.archive
{


	public class Utils
	{
		 private Utils()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void checkWritableDirectory(java.nio.file.Path directory) throws java.nio.file.FileSystemException
		 public static void CheckWritableDirectory( Path directory )
		 {
			  if ( !exists( directory ) )
			  {
					throw new NoSuchFileException( directory.ToString() );
			  }
			  if ( isRegularFile( directory ) )
			  {
					throw new FileSystemException( directory.ToString() + ": Not a directory" );
			  }
			  if ( !isWritable( directory ) )
			  {
					throw new AccessDeniedException( directory.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void copy(java.io.InputStream in, java.io.OutputStream out, ArchiveProgressPrinter progressPrinter) throws java.io.IOException
		 public static void Copy( Stream @in, Stream @out, ArchiveProgressPrinter progressPrinter )
		 {
			  progressPrinter.BeginFile();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] buffer = new byte[8192];
			  sbyte[] buffer = new sbyte[8192];
			  int n;
			  while ( -1 != ( n = @in.Read( buffer, 0, buffer.Length ) ) )
			  {
					@out.Write( buffer, 0, n );
					progressPrinter.AddBytes( n );
			  }
			  progressPrinter.EndFile();
		 }
	}

}