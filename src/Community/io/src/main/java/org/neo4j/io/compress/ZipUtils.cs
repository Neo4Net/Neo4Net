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
namespace Neo4Net.Io.compress
{

	using MapUtil = Neo4Net.Helpers.Collection.MapUtil;
	using FileHandle = Neo4Net.Io.fs.FileHandle;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	public class ZipUtils
	{
		 /// <summary>
		 /// Create zip archive for requested <code>sourceToCompress</code>.
		 /// If <code>sourceToCompress</code> is a directory then content of that directory and all its sub-directories will be added to the archive.
		 /// If <code>sourceToCompress</code> does not exist or is an empty directory then archive will not be created. </summary>
		 /// <param name="fileSystem"> source file system </param>
		 /// <param name="sourceToCompress"> source file to compress </param>
		 /// <param name="destinationZip"> zip file compress source to </param>
		 /// <exception cref="IOException"> when underlying file system access produce IOException </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void zip(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File sourceToCompress, java.io.File destinationZip) throws java.io.IOException
		 public static void Zip( FileSystemAbstraction fileSystem, File sourceToCompress, File destinationZip )
		 {
			  if ( !fileSystem.FileExists( sourceToCompress ) )
			  {
					return;
			  }
			  if ( IsEmptyDirectory( fileSystem, sourceToCompress ) )
			  {
					return;
			  }
			  IDictionary<string, string> env = MapUtil.stringMap( "create", "true" );
			  Path rootPath = sourceToCompress.toPath();
			  URI archiveAbsoluteURI = URI.create( "jar:file:" + destinationZip.toURI().RawPath );

			  using ( FileSystem zipFs = FileSystems.newFileSystem( archiveAbsoluteURI, env ) )
			  {
					IList<FileHandle> fileHandles = fileSystem.StreamFilesRecursive( sourceToCompress ).collect( toList() );
					foreach ( FileHandle fileHandle in fileHandles )
					{
						 Path sourcePath = fileHandle.File.toPath();
						 Path zipFsPath = fileSystem.IsDirectory( sourceToCompress ) ? zipFs.getPath( rootPath.relativize( sourcePath ).ToString() ) : zipFs.getPath(sourcePath.FileName.ToString());
						 if ( zipFsPath.Parent != null )
						 {
							  Files.createDirectories( zipFsPath.Parent );
						 }
						 Files.copy( sourcePath, zipFsPath );
					}
			  }
		 }

		 private static bool IsEmptyDirectory( FileSystemAbstraction fileSystem, File sourceToCompress )
		 {
			  if ( fileSystem.IsDirectory( sourceToCompress ) )
			  {
					File[] files = fileSystem.ListFiles( sourceToCompress );
					return files == null || Files.Length == 0;
			  }
			  return false;
		 }
	}

}