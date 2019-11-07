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

	using FileWatcher = Neo4Net.Io.fs.watcher.FileWatcher;

	public interface FileSystemAbstraction : System.IDisposable
	{

		 /// <summary>
		 /// Create file watcher that provides possibilities to monitor directories on underlying file system
		 /// abstraction
		 /// </summary>
		 /// <returns> specific file system abstract watcher </returns>
		 /// <exception cref="IOException"> in case exception occur during file watcher creation </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.io.fs.watcher.FileWatcher fileWatcher() throws java.io.IOException;
		 FileWatcher FileWatcher();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreChannel open(java.io.File fileName, OpenMode openMode) throws java.io.IOException;
		 StoreChannel Open( File fileName, OpenMode openMode );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException;
		 Stream OpenAsOutputStream( File fileName, bool append );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException;
		 Stream OpenAsInputStream( File fileName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.Reader openAsReader(java.io.File fileName, java.nio.charset.Charset charset) throws java.io.IOException;
		 Reader OpenAsReader( File fileName, Charset charset );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.Writer openAsWriter(java.io.File fileName, java.nio.charset.Charset charset, boolean append) throws java.io.IOException;
		 Writer OpenAsWriter( File fileName, Charset charset, bool append );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StoreChannel create(java.io.File fileName) throws java.io.IOException;
		 StoreChannel Create( File fileName );

		 bool FileExists( File file );

		 bool Mkdir( File fileName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void mkdirs(java.io.File fileName) throws java.io.IOException;
		 void Mkdirs( File fileName );

		 long GetFileSize( File fileName );

		 bool DeleteFile( File fileName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteRecursively(java.io.File directory) throws java.io.IOException;
		 void DeleteRecursively( File directory );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void renameFile(java.io.File from, java.io.File to, java.nio.file.CopyOption... copyOptions) throws java.io.IOException;
		 void RenameFile( File from, File to, params CopyOption[] copyOptions );

		 File[] ListFiles( File directory );

		 File[] ListFiles( File directory, FilenameFilter filter );

		 bool IsDirectory( File file );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void moveToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException;
		 void MoveToDirectory( File file, File toDirectory );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void copyToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException;
		 void CopyToDirectory( File file, File toDirectory );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void copyFile(java.io.File from, java.io.File to) throws java.io.IOException;
		 void CopyFile( File from, File to );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException;
		 void CopyRecursively( File fromDirectory, File toDirectory );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void truncate(java.io.File path, long size) throws java.io.IOException;
		 void Truncate( File path, long size );

		 long LastModifiedTime( File file );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteFileOrThrow(java.io.File file) throws java.io.IOException;
		 void DeleteFileOrThrow( File file );

		 /// <summary>
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
		 /// <returns> A stream of all files in the tree. </returns>
		 /// <exception cref="NoSuchFileException"> If the given base directory or file does not exists. </exception>
		 /// <exception cref="IOException"> If an I/O error occurs, possibly with the canonicalisation of the paths. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.stream.Stream<FileHandle> streamFilesRecursive(java.io.File directory) throws java.io.IOException;
		 Stream<FileHandle> StreamFilesRecursive( File directory );
	}

}