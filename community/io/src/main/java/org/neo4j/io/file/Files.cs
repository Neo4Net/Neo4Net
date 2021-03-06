﻿/*
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
namespace Org.Neo4j.Io.file
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;

	/// <summary>
	/// This class consists exclusively of static methods that operate on files, directories, or other types of files.
	/// </summary>
	public class Files
	{
		 private Files()
		 {
		 }

		 /// <summary>
		 /// Creates a file, or opens an existing file. If necessary, parent directories will be created.
		 /// </summary>
		 /// <param name="fileSystem"> The filesystem abstraction to use </param>
		 /// <param name="file"> The file to create or open </param>
		 /// <returns> An output stream </returns>
		 /// <exception cref="IOException"> If an error occurs creating directories or opening the file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.OutputStream createOrOpenAsOutputStream(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file, boolean append) throws java.io.IOException
		 public static Stream CreateOrOpenAsOutputStream( FileSystemAbstraction fileSystem, File file, bool append )
		 {
			  if ( file.ParentFile != null )
			  {
					fileSystem.Mkdirs( file.ParentFile );
			  }
			  return fileSystem.OpenAsOutputStream( file, append );
		 }
	}

}