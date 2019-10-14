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
namespace Neo4Net.Graphdb.mockfs
{

	using IOUtils = Neo4Net.Io.IOUtils;
	using FileHandle = Neo4Net.Io.fs.FileHandle;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using StreamFilesRecursive = Neo4Net.Io.fs.StreamFilesRecursive;
	using FileWatcher = Neo4Net.Io.fs.watcher.FileWatcher;

	/// <summary>
	/// Allows you to select different file system behaviour for one file and a different file system behaviour for
	/// everyone else
	/// e.g. Adversarial behaviour for the file under tests and normal behaviour for all other files.
	/// </summary>
	public class SelectiveFileSystemAbstraction : FileSystemAbstraction
	{
		 private readonly File _specialFile;
		 private readonly FileSystemAbstraction _specialFileSystem;
		 private readonly FileSystemAbstraction _defaultFileSystem;

		 public SelectiveFileSystemAbstraction( File specialFile, FileSystemAbstraction specialFileSystem, FileSystemAbstraction defaultFileSystem )
		 {
			  this._specialFile = specialFile;
			  this._specialFileSystem = specialFileSystem;
			  this._defaultFileSystem = defaultFileSystem;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.watcher.FileWatcher fileWatcher() throws java.io.IOException
		 public override FileWatcher FileWatcher()
		 {
			  return new SelectiveFileWatcher( _specialFile, _defaultFileSystem.fileWatcher(), _specialFileSystem.fileWatcher() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
		 public override StoreChannel Open( File fileName, OpenMode openMode )
		 {
			  return ChooseFileSystem( fileName ).open( fileName, openMode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
		 public override Stream OpenAsOutputStream( File fileName, bool append )
		 {
			  return ChooseFileSystem( fileName ).openAsOutputStream( fileName, append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException
		 public override Stream OpenAsInputStream( File fileName )
		 {
			  return ChooseFileSystem( fileName ).openAsInputStream( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Reader openAsReader(java.io.File fileName, java.nio.charset.Charset charset) throws java.io.IOException
		 public override Reader OpenAsReader( File fileName, Charset charset )
		 {
			  return ChooseFileSystem( fileName ).openAsReader( fileName, charset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer openAsWriter(java.io.File fileName, java.nio.charset.Charset charset, boolean append) throws java.io.IOException
		 public override Writer OpenAsWriter( File fileName, Charset charset, bool append )
		 {
			  return ChooseFileSystem( fileName ).openAsWriter( fileName, charset, append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel create(java.io.File fileName) throws java.io.IOException
		 public override StoreChannel Create( File fileName )
		 {
			  return ChooseFileSystem( fileName ).create( fileName );
		 }

		 public override bool FileExists( File file )
		 {
			  return ChooseFileSystem( file ).fileExists( file );
		 }

		 public override bool Mkdir( File fileName )
		 {
			  return ChooseFileSystem( fileName ).mkdir( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mkdirs(java.io.File fileName) throws java.io.IOException
		 public override void Mkdirs( File fileName )
		 {
			  ChooseFileSystem( fileName ).mkdirs( fileName );
		 }

		 public override long GetFileSize( File fileName )
		 {
			  return ChooseFileSystem( fileName ).getFileSize( fileName );
		 }

		 public override bool DeleteFile( File fileName )
		 {
			  return ChooseFileSystem( fileName ).deleteFile( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteRecursively(java.io.File directory) throws java.io.IOException
		 public override void DeleteRecursively( File directory )
		 {
			  ChooseFileSystem( directory ).deleteRecursively( directory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File from, java.io.File to, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
		 public override void RenameFile( File from, File to, params CopyOption[] copyOptions )
		 {
			  ChooseFileSystem( from ).renameFile( from, to, copyOptions );
		 }

		 public override File[] ListFiles( File directory )
		 {
			  return ChooseFileSystem( directory ).listFiles( directory );
		 }

		 public override File[] ListFiles( File directory, FilenameFilter filter )
		 {
			  return ChooseFileSystem( directory ).listFiles( directory, filter );
		 }

		 public override bool IsDirectory( File file )
		 {
			  return ChooseFileSystem( file ).isDirectory( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void MoveToDirectory( File file, File toDirectory )
		 {
			  ChooseFileSystem( file ).moveToDirectory( file, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyToDirectory( File file, File toDirectory )
		 {
			  ChooseFileSystem( file ).copyToDirectory( file, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyFile(java.io.File from, java.io.File to) throws java.io.IOException
		 public override void CopyFile( File from, File to )
		 {
			  ChooseFileSystem( from ).copyFile( from, to );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyRecursively( File fromDirectory, File toDirectory )
		 {
			  ChooseFileSystem( fromDirectory ).copyRecursively( fromDirectory, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(java.io.File path, long size) throws java.io.IOException
		 public override void Truncate( File path, long size )
		 {
			  ChooseFileSystem( path ).truncate( path, size );
		 }

		 public override long LastModifiedTime( File file )
		 {
			  return ChooseFileSystem( file ).lastModifiedTime( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteFileOrThrow(java.io.File file) throws java.io.IOException
		 public override void DeleteFileOrThrow( File file )
		 {
			  ChooseFileSystem( file ).deleteFileOrThrow( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<org.neo4j.io.fs.FileHandle> streamFilesRecursive(java.io.File directory) throws java.io.IOException
		 public override Stream<FileHandle> StreamFilesRecursive( File directory )
		 {
			  return StreamFilesRecursive.streamFilesRecursive( directory, this );

		 }

		 private FileSystemAbstraction ChooseFileSystem( File file )
		 {
			  return file.Equals( _specialFile ) ? _specialFileSystem : _defaultFileSystem;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _specialFileSystem, _defaultFileSystem );
		 }
	}

}