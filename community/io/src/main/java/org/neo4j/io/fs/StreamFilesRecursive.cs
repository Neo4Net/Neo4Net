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
namespace Org.Neo4j.Io.fs
{

	public class StreamFilesRecursive
	{
		 private StreamFilesRecursive()
		 {
			  //This is a helper class, do not instantiate it.
		 }

		 /// <summary>
		 /// Static implementation of <seealso cref="FileSystemAbstraction.streamFilesRecursive(File)"/> that does not require
		 /// any external state, other than what is presented through the given <seealso cref="FileSystemAbstraction"/>.
		 /// 
		 /// Return a stream of <seealso cref="FileHandle file handles"/> for every file in the given directory, and its
		 /// sub-directories.
		 /// <para>
		 /// Alternatively, if the <seealso cref="File"/> given as an argument refers to a file instead of a directory, then a stream
		 /// will be returned with a file handle for just that file.
		 /// </para>
		 /// <para>
		 /// The stream is based on a snapshot of the file tree, so changes made to the tree using the returned file handles
		 /// will not be reflected in the stream.
		 /// </para>
		 /// <para>
		 /// No directories will be returned. Only files. If a file handle ends up leaving a directory empty through a
		 /// rename or a delete, then the empty directory will automatically be deleted as well.
		 /// Likewise, if a file is moved to a path where not all of the directories in the path exists, then those missing
		 /// directories will be created prior to the file rename.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="directory"> The base directory to start streaming files from, or the specific individual file to stream. </param>
		 /// <param name="fs"> The <seealso cref="FileSystemAbstraction"/> to use for manipulating files. </param>
		 /// <returns> A <seealso cref="Stream"/> of <seealso cref="FileHandle"/>s </returns>
		 /// <exception cref="IOException"> If an I/O error occurs, possibly with the canonicalisation of the paths. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.stream.Stream<FileHandle> streamFilesRecursive(java.io.File directory, FileSystemAbstraction fs) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Stream<FileHandle> StreamFilesRecursiveConflict( File directory, FileSystemAbstraction fs )
		 {
			  File canonicalizedDirectory = directory.CanonicalFile;
			  // We grab a snapshot of the file tree to avoid seeing the same file twice or more due to renames.
			  IList<File> snapshot = StreamFilesRecursiveInner( canonicalizedDirectory, fs ).collect( toList() );
			  return snapshot.Select( f => new WrappingFileHandle( f, canonicalizedDirectory, fs ) );
		 }

		 private static Stream<File> StreamFilesRecursiveInner( File directory, FileSystemAbstraction fs )
		 {
			  File[] files = fs.ListFiles( directory );
			  if ( files == null )
			  {
					if ( !fs.FileExists( directory ) )
					{
						 return Stream.empty();
					}
					return Stream.of( directory );
			  }
			  else
			  {
					return Stream.of( files ).flatMap( f => fs.IsDirectory( f ) ? StreamFilesRecursiveInner( f, fs ) : Stream.of( f ) );
			  }
		 }
	}

}