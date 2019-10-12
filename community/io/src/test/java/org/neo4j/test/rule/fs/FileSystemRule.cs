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
namespace Org.Neo4j.Test.rule.fs
{
	using ExternalResource = org.junit.rules.ExternalResource;


	using FileHandle = Org.Neo4j.Io.fs.FileHandle;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using FileWatcher = Org.Neo4j.Io.fs.watcher.FileWatcher;

	public abstract class FileSystemRule<FS> : ExternalResource, FileSystemAbstraction, System.Func<FileSystemAbstraction> where FS : Org.Neo4j.Io.fs.FileSystemAbstraction
	{
		 protected internal volatile FS Fs;

		 protected internal FileSystemRule( FS fs )
		 {
			  this.Fs = fs;
		 }

		 protected internal override void After()
		 {
			  try
			  {
					Fs.close();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  base.After();
		 }

		 public override FS Get()
		 {
			  return Fs;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  Fs.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.watcher.FileWatcher fileWatcher() throws java.io.IOException
		 public override FileWatcher FileWatcher()
		 {
			  return Fs.fileWatcher();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
		 public override StoreChannel Open( File fileName, OpenMode openMode )
		 {
			  return Fs.open( fileName, openMode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
		 public override Stream OpenAsOutputStream( File fileName, bool append )
		 {
			  return Fs.openAsOutputStream( fileName, append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException
		 public override Stream OpenAsInputStream( File fileName )
		 {
			  return Fs.openAsInputStream( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Reader openAsReader(java.io.File fileName, java.nio.charset.Charset charset) throws java.io.IOException
		 public override Reader OpenAsReader( File fileName, Charset charset )
		 {
			  return Fs.openAsReader( fileName, charset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer openAsWriter(java.io.File fileName, java.nio.charset.Charset charset, boolean append) throws java.io.IOException
		 public override Writer OpenAsWriter( File fileName, Charset charset, bool append )
		 {
			  return Fs.openAsWriter( fileName, charset, append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel create(java.io.File fileName) throws java.io.IOException
		 public override StoreChannel Create( File fileName )
		 {
			  return Fs.create( fileName );
		 }

		 public override bool FileExists( File file )
		 {
			  return Fs.fileExists( file );
		 }

		 public override bool Mkdir( File fileName )
		 {
			  return Fs.mkdir( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mkdirs(java.io.File fileName) throws java.io.IOException
		 public override void Mkdirs( File fileName )
		 {
			  Fs.mkdirs( fileName );
		 }

		 public override long GetFileSize( File fileName )
		 {
			  return Fs.getFileSize( fileName );
		 }

		 public override bool DeleteFile( File fileName )
		 {
			  return Fs.deleteFile( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteRecursively(java.io.File directory) throws java.io.IOException
		 public override void DeleteRecursively( File directory )
		 {
			  Fs.deleteRecursively( directory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File from, java.io.File to, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
		 public override void RenameFile( File from, File to, params CopyOption[] copyOptions )
		 {
			  Fs.renameFile( from, to, copyOptions );
		 }

		 public override File[] ListFiles( File directory )
		 {
			  return Fs.listFiles( directory );
		 }

		 public override File[] ListFiles( File directory, FilenameFilter filter )
		 {
			  return Fs.listFiles( directory, filter );
		 }

		 public override bool IsDirectory( File file )
		 {
			  return Fs.isDirectory( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void MoveToDirectory( File file, File toDirectory )
		 {
			  Fs.moveToDirectory( file, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyToDirectory( File file, File toDirectory )
		 {
			  Fs.copyToDirectory( file, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyFile(java.io.File from, java.io.File to) throws java.io.IOException
		 public override void CopyFile( File from, File to )
		 {
			  Fs.copyFile( from, to );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyRecursively( File fromDirectory, File toDirectory )
		 {
			  Fs.copyRecursively( fromDirectory, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(java.io.File path, long size) throws java.io.IOException
		 public override void Truncate( File path, long size )
		 {
			  Fs.truncate( path, size );
		 }

		 public override long LastModifiedTime( File file )
		 {
			  return Fs.lastModifiedTime( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteFileOrThrow(java.io.File file) throws java.io.IOException
		 public override void DeleteFileOrThrow( File file )
		 {
			  Fs.deleteFileOrThrow( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<org.neo4j.io.fs.FileHandle> streamFilesRecursive(java.io.File directory) throws java.io.IOException
		 public override Stream<FileHandle> StreamFilesRecursive( File directory )
		 {
			  return Fs.streamFilesRecursive( directory );
		 }

		 public override int GetHashCode()
		 {
			  return Fs.GetHashCode();
		 }

		 public override bool Equals( object obj )
		 {
			  return Fs.Equals( obj );
		 }

		 public override string ToString()
		 {
			  return Fs.ToString();
		 }
	}

}