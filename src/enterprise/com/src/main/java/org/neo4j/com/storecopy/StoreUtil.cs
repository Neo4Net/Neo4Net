/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.com.storecopy
{
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using FileUtils = Neo4Net.Io.fs.FileUtils;

	public class StoreUtil
	{
		 // Branched directories will end up in <dbStoreDir>/branched/<timestamp>/
		 public const string BRANCH_SUBDIRECTORY = "branched";
		 private static readonly string[] _dontMoveDirectories = new string[] { "metrics", "logs", "certificates" };
		 public const string TEMP_COPY_DIRECTORY_NAME = "temp-copy";

		 private static readonly FileFilter _storeFileFilter = file =>
		 {
		  foreach ( string directory in _dontMoveDirectories )
		  {
				if ( file.Name.Equals( directory ) )
				{
					 return false;
				}
		  }
		  return !IsBranchedDataRootDirectory( file ) && !IsTemporaryCopy( file );
		 };

		 private StoreUtil()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void cleanStoreDir(java.io.File databaseDirectory) throws java.io.IOException
		 public static void CleanStoreDir( File databaseDirectory )
		 {
			  foreach ( File file in RelevantDbFiles( databaseDirectory ) )
			  {
					FileUtils.deleteRecursively( file );
			  }
		 }

		 public static File NewBranchedDataDir( File databaseDirectory )
		 {
			  File result = GetBranchedDataDirectory( databaseDirectory, DateTimeHelper.CurrentUnixTimeMillis() );
			  result.mkdirs();
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void moveAwayDb(java.io.File databaseDirectory, java.io.File branchedDataDir) throws java.io.IOException
		 public static void MoveAwayDb( File databaseDirectory, File branchedDataDir )
		 {
			  foreach ( File file in RelevantDbFiles( databaseDirectory ) )
			  {
					FileUtils.moveFileToDirectory( file, branchedDataDir );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void deleteRecursive(java.io.File databaseDirectory) throws java.io.IOException
		 public static void DeleteRecursive( File databaseDirectory )
		 {
			  FileUtils.deleteRecursively( databaseDirectory );
		 }

		 public static bool IsBranchedDataDirectory( File file )
		 {
			  return file.Directory && file.ParentFile.Name.Equals( BRANCH_SUBDIRECTORY ) && StringUtils.isNumeric( file.Name );
		 }

		 public static File GetBranchedDataRootDirectory( File databaseDirectory )
		 {
			  return new File( databaseDirectory, BRANCH_SUBDIRECTORY );
		 }

		 public static File GetBranchedDataDirectory( File databaseDirectory, long timestamp )
		 {
			  return new File( GetBranchedDataRootDirectory( databaseDirectory ), "" + timestamp );
		 }

		 public static File[] RelevantDbFiles( File databaseDirectory )
		 {
			  if ( !databaseDirectory.exists() )
			  {
					return new File[0];
			  }

			  return databaseDirectory.listFiles( _storeFileFilter );
		 }

		 private static bool IsBranchedDataRootDirectory( File file )
		 {
			  return file.Directory && BRANCH_SUBDIRECTORY.Equals( file.Name );
		 }

		 private static bool IsTemporaryCopy( File file )
		 {
			  return file.Directory && file.Name.Equals( TEMP_COPY_DIRECTORY_NAME );
		 }

	}

}