using System.IO;

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

	using DefaultFileSystemWatcher = Neo4Net.Io.fs.watcher.DefaultFileSystemWatcher;
	using FileWatcher = Neo4Net.Io.fs.watcher.FileWatcher;

	/// <summary>
	/// Default file system abstraction that creates files using the underlying file system.
	/// </summary>
	public class DefaultFileSystemAbstraction : FileSystemAbstraction
	{
		 internal const string UNABLE_TO_CREATE_DIRECTORY_FORMAT = "Unable to create directory path [%s] for Neo4Net store.";

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.io.fs.watcher.FileWatcher fileWatcher() throws java.io.IOException
		 public override FileWatcher FileWatcher()
		 {
			  WatchService watchService = FileSystems.Default.newWatchService();
			  return new DefaultFileSystemWatcher( watchService );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreFileChannel open(java.io.File fileName, OpenMode openMode) throws java.io.IOException
		 public override StoreFileChannel Open( File fileName, OpenMode openMode )
		 {
			  // Returning only the channel is ok, because the channel, when close()d will close its parent File.
			  FileChannel channel = ( new RandomAccessFile( fileName, openMode.mode() ) ).Channel;
			  return GetStoreFileChannel( channel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
		 public override Stream OpenAsOutputStream( File fileName, bool append )
		 {
			  return new FileStream( fileName, append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException
		 public override Stream OpenAsInputStream( File fileName )
		 {
			  return new FileStream( fileName, FileMode.Open, FileAccess.Read );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Reader openAsReader(java.io.File fileName, java.nio.charset.Charset charset) throws java.io.IOException
		 public override Reader OpenAsReader( File fileName, Charset charset )
		 {
			  return new StreamReader( OpenAsInputStream( fileName ), charset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer openAsWriter(java.io.File fileName, java.nio.charset.Charset charset, boolean append) throws java.io.IOException
		 public override Writer OpenAsWriter( File fileName, Charset charset, bool append )
		 {
			  return new StreamWriter( OpenAsOutputStream( fileName, append ), charset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreFileChannel create(java.io.File fileName) throws java.io.IOException
		 public override StoreFileChannel Create( File fileName )
		 {
			  return Open( fileName, OpenMode.ReadWrite );
		 }

		 public override bool Mkdir( File fileName )
		 {
			  return fileName.mkdir();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mkdirs(java.io.File path) throws java.io.IOException
		 public override void Mkdirs( File path )
		 {
			  if ( path.exists() )
			  {
					return;
			  }

			  path.mkdirs();

			  if ( path.exists() )
			  {
					return;
			  }

			  throw new IOException( format( UNABLE_TO_CREATE_DIRECTORY_FORMAT, path ) );
		 }

		 public override bool FileExists( File file )
		 {
			  return file.exists();
		 }

		 public override long GetFileSize( File file )
		 {
			  return file.length();
		 }

		 public override bool DeleteFile( File fileName )
		 {
			  return FileUtils.DeleteFile( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteRecursively(java.io.File directory) throws java.io.IOException
		 public override void DeleteRecursively( File directory )
		 {
			  FileUtils.DeleteRecursively( directory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File from, java.io.File to, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
		 public override void RenameFile( File from, File to, params CopyOption[] copyOptions )
		 {
			  Files.move( from.toPath(), to.toPath(), copyOptions );
		 }

		 public override File[] ListFiles( File directory )
		 {
			  return directory.listFiles();
		 }

		 public override File[] ListFiles( File directory, FilenameFilter filter )
		 {
			  return directory.listFiles( filter );
		 }

		 public override bool IsDirectory( File file )
		 {
			  return file.Directory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void MoveToDirectory( File file, File toDirectory )
		 {
			  FileUtils.MoveFileToDirectory( file, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyToDirectory( File file, File toDirectory )
		 {
			  FileUtils.CopyFileToDirectory( file, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyFile(java.io.File from, java.io.File to) throws java.io.IOException
		 public override void CopyFile( File from, File to )
		 {
			  FileUtils.CopyFile( from, to );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyRecursively( File fromDirectory, File toDirectory )
		 {
			  FileUtils.CopyRecursively( fromDirectory, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(java.io.File path, long size) throws java.io.IOException
		 public override void Truncate( File path, long size )
		 {
			  FileUtils.TruncateFile( path, size );
		 }

		 public override long LastModifiedTime( File file )
		 {
			  return file.lastModified();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteFileOrThrow(java.io.File file) throws java.io.IOException
		 public override void DeleteFileOrThrow( File file )
		 {
			  Files.delete( file.toPath() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<FileHandle> streamFilesRecursive(java.io.File directory) throws java.io.IOException
		 public override Stream<FileHandle> StreamFilesRecursive( File directory )
		 {
			  return StreamFilesRecursive.StreamFilesRecursiveConflict( directory, this );
		 }

		 protected internal virtual StoreFileChannel GetStoreFileChannel( FileChannel channel )
		 {
			  return new StoreFileChannel( channel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  // nothing
		 }
	}

}