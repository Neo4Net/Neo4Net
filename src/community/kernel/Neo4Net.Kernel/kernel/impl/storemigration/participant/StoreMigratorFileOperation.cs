using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.storemigration.participant
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.stream.Streams.ofOptional;

	internal class StoreMigratorFileOperation
	{
		 /// <summary>
		 /// Performs a file operation on a database's store files from one directory
		 /// to another. Remember that in the case of <seealso cref="FileOperation.MOVE moving files"/>, the way that's done is to
		 /// just rename files (the standard way of moving with JDK6) from and to must be on the same disk partition.
		 /// </summary>
		 /// <param name="fromLayout"> directory that hosts the database files. </param>
		 /// <param name="toLayout"> directory to receive the database files. </param>
		 /// <exception cref="IOException"> if any of the operations fail for any reason. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void fileOperation(Neo4Net.kernel.impl.storemigration.FileOperation operation, Neo4Net.io.fs.FileSystemAbstraction fs, Neo4Net.io.layout.DatabaseLayout fromLayout, Neo4Net.io.layout.DatabaseLayout toLayout, Iterable<Neo4Net.io.layout.DatabaseFile> databaseFiles, boolean allowSkipNonExistentFiles, Neo4Net.kernel.impl.storemigration.ExistingTargetStrategy existingTargetStrategy) throws java.io.IOException
		 internal static void FileOperation( FileOperation operation, FileSystemAbstraction fs, DatabaseLayout fromLayout, DatabaseLayout toLayout, IEnumerable<DatabaseFile> databaseFiles, bool allowSkipNonExistentFiles, ExistingTargetStrategy existingTargetStrategy )
		 {
			  foreach ( DatabaseFile databaseStore in databaseFiles )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					File[] files = Stream.concat( fromLayout.File( databaseStore ), ofOptional( fromLayout.IdFile( databaseStore ) ) ).toArray( File[]::new );
					Perform( operation, fs, fromLayout, toLayout, allowSkipNonExistentFiles, existingTargetStrategy, files );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void perform(Neo4Net.kernel.impl.storemigration.FileOperation operation, Neo4Net.io.fs.FileSystemAbstraction fs, Neo4Net.io.layout.DatabaseLayout fromLayout, Neo4Net.io.layout.DatabaseLayout toLayout, boolean allowSkipNonExistentFiles, Neo4Net.kernel.impl.storemigration.ExistingTargetStrategy existingTargetStrategy, java.io.File[] files) throws java.io.IOException
		 private static void Perform( FileOperation operation, FileSystemAbstraction fs, DatabaseLayout fromLayout, DatabaseLayout toLayout, bool allowSkipNonExistentFiles, ExistingTargetStrategy existingTargetStrategy, File[] files )
		 {
			  foreach ( File file in files )
			  {
					if ( file != null )
					{
						 operation.perform( fs, file.Name, fromLayout.DatabaseDirectory(), allowSkipNonExistentFiles, toLayout.DatabaseDirectory(), existingTargetStrategy );
					}
			  }
		 }
	}

}