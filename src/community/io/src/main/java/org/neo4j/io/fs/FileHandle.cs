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
namespace Neo4Net.Io.fs
{

	/// <summary>
	/// A handle to a file as seen by the page cache. The file may or may not be mapped. </summary>
	/// <seealso cref= FileSystemAbstraction#streamFilesRecursive(File) </seealso>
	public interface FileHandle
	{
		 /// <summary>
		 /// Useful consumer when doing deletion in stream pipeline.
		 /// <para>
		 /// Possible IOException caused by fileHandle.delete() is wrapped in UncheckedIOException
		 /// </para>
		 /// </summary>

		 /// <summary>
		 /// Create a consumer of FileHandle that uses fileHandle.rename to move file held by file handle to move from
		 /// directory to directory.
		 /// <para>
		 /// Possibly IOException will be wrapped in UncheckedIOException
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="from"> Directory to move file from. </param>
		 /// <param name="to"> Directory to move file to. </param>
		 /// <returns> A new Consumer that moves the file wrapped by the file handle. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static System.Action<FileHandle> handleRenameBetweenDirectories(java.io.File from, java.io.File to)
	//	 {
	//		  return fileHandle ->
	//		  {
	//				try
	//				{
	//					 fileHandle.rename(FileUtils.pathToFileAfterMove(from, to, fileHandle.getFile()));
	//				}
	//				catch (IOException e)
	//				{
	//					 throw new UncheckedIOException(e);
	//				}
	//		  };
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static System.Action<FileHandle> handleRename(java.io.File to)
	//	 {
	//		  return fileHandle ->
	//		  {
	//				try
	//				{
	//					 fileHandle.rename(to);
	//				}
	//				catch (IOException e)
	//				{
	//					 throw new UncheckedIOException(e);
	//				}
	//		  };
	//	 }

		 /// <summary>
		 /// Get a <seealso cref="File"/> object for the abstract path name that this file handle represents.
		 /// 
		 /// Note that the File is not guaranteed to support any operations other than querying the path name.
		 /// For instance, to delete the file you have to use the <seealso cref="delete()"/> method of this file handle, instead of
		 /// <seealso cref="File.delete()"/>. </summary>
		 /// <returns> A <seealso cref="File"/> for this file handle. </returns>
		 File File { get; }

		 /// <summary>
		 /// Get a <seealso cref="File"/> object for the abstract path name that this file handle represents, and that is
		 /// <em>relative</em> to the base path that was passed into the
		 /// <seealso cref="FileSystemAbstraction.streamFilesRecursive(File)"/> method.
		 /// <para>
		 /// This method is otherwise behaviourally the same as <seealso cref="getFile()"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> A <seealso cref="File"/> for this file handle. </returns>
		 File RelativeFile { get; }

		 /// <summary>
		 /// Rename source file to the given target file, effectively moving the file from source to target.
		 /// 
		 /// Both files have to be unmapped when performing the rename, otherwise an exception will be thrown.
		 /// 
		 /// If the file is moved to a path where some of the directories of the path don't already exists, then those missing
		 /// directories will be created prior to the move. This is not an atomic operation, and an exception may be thrown if
		 /// the a directory in the path is deleted concurrently with the file rename operation.
		 /// 
		 /// Likewise, if the file rename causes a directory to become empty, then those directories will be deleted
		 /// automatically. This operation is also not atomic, so if files are added to such directories concurrently with
		 /// the rename operation, then an exception can be thrown.
		 /// </summary>
		 /// <param name="to"> The new name of the file after the rename. </param>
		 /// <param name="options"> Options to modify the behaviour of the move in possibly platform specific ways. In particular,
		 /// <seealso cref="java.nio.file.StandardCopyOption.REPLACE_EXISTING"/> may be used to overwrite any existing file at the
		 /// target path name, instead of throwing an exception. </param>
		 /// <exception cref="org.neo4j.io.pagecache.impl.FileIsMappedException"> if either the file represented by this file handle is
		 /// mapped, or the target file is mapped. </exception>
		 /// <exception cref="java.nio.file.FileAlreadyExistsException"> if the target file already exists, and the
		 /// <seealso cref="java.nio.file.StandardCopyOption.REPLACE_EXISTING"/> open option was not specified. </exception>
		 /// <exception cref="IOException"> if an I/O error occurs, for instance when canonicalising the {@code to} path. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void rename(java.io.File to, java.nio.file.CopyOption... options) throws java.io.IOException;
		 void Rename( File to, params CopyOption[] options );

		 /// <summary>
		 /// Delete the file that this file handle represents.
		 /// </summary>
		 /// <exception cref="org.neo4j.io.pagecache.impl.FileIsMappedException"> if this file is mapped by the page cache. </exception>
		 /// <exception cref="java.nio.file.NoSuchFileException"> if the underlying file was deleted after this handle was created. </exception>
		 /// <exception cref="IOException"> if an I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void delete() throws java.io.IOException;
		 void Delete();
	}

	public static class FileHandle_Fields
	{
		 public static readonly System.Action<FileHandle> HandleDelete = fh =>
		 {
		  try
		  {
				fh.delete();
		  }
		  catch ( IOException e )
		  {
				throw new UncheckedIOException( e );
		  }
		 };

	}

}