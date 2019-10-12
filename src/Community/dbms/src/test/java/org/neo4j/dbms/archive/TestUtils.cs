using System.Collections.Generic;

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
namespace Neo4Net.Dbms.archive
{

	public class TestUtils
	{

		 private TestUtils()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.Closeable withPermissions(java.nio.file.Path file, java.util.Set<java.nio.file.attribute.PosixFilePermission> permissions) throws java.io.IOException
		 public static System.IDisposable WithPermissions( Path file, ISet<PosixFilePermission> permissions )
		 {
			  ISet<PosixFilePermission> originalPermissions = Files.getPosixFilePermissions( file );
			  Files.setPosixFilePermissions( file, permissions );
			  return () => Files.setPosixFilePermissions(file, originalPermissions);
		 }
	}

}