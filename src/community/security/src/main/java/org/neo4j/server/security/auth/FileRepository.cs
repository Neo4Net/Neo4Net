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
namespace Neo4Net.Server.Security.Auth
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Log = Neo4Net.Logging.Log;

	public interface FileRepository
	{
		 /// <summary>
		 /// This is used by different flavors of file repositories to agree on a naming convention
		 /// for repository files that are renamed after migration to prevent accidental reuse.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static java.io.File getMigratedFile(java.io.File file)
	//	 {
	//		  return new File(file.getParent(), file.getName() + ".migrated");
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static void assertNotMigrated(java.io.File file, org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, org.Neo4Net.logging.Log log)
	//	 {
	//		  File migratedFile = getMigratedFile(file);
	//		  if (fileSystem.fileExists(migratedFile))
	//		  {
	//				String message = "The repository file '" + file.getAbsolutePath() + "' has been marked as migrated. " +
	//						  "If you are sure that you want use this repository you need to manually rename the file '" +
	//						  migratedFile.getAbsolutePath() + "' to '" + file.getName() + "'";
	//				log.error(message);
	//				throw new IllegalStateException(message);
	//		  }
	//	 }
	}

}