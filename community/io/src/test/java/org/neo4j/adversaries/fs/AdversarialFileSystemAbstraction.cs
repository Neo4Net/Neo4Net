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
namespace Org.Neo4j.Adversaries.fs
{

	using AdversarialFileWatcher = Org.Neo4j.Adversaries.watcher.AdversarialFileWatcher;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileHandle = Org.Neo4j.Io.fs.FileHandle;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using StoreFileChannel = Org.Neo4j.Io.fs.StoreFileChannel;
	using StreamFilesRecursive = Org.Neo4j.Io.fs.StreamFilesRecursive;
	using FileWatcher = Org.Neo4j.Io.fs.watcher.FileWatcher;

	/// <summary>
	/// Used by the robustness suite to check for partial failures.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AdversarialFileSystemAbstraction implements org.neo4j.io.fs.FileSystemAbstraction
	public class AdversarialFileSystemAbstraction : FileSystemAbstraction
	{
		 private readonly FileSystemAbstraction @delegate;
		 private readonly Adversary _adversary;

		 public AdversarialFileSystemAbstraction( Adversary adversary ) : this( adversary, new DefaultFileSystemAbstraction() )
		 {
		 }

		 public AdversarialFileSystemAbstraction( Adversary adversary, FileSystemAbstraction @delegate )
		 {
			  this._adversary = adversary;
			  this.@delegate = @delegate;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.watcher.FileWatcher fileWatcher() throws java.io.IOException
		 public override FileWatcher FileWatcher()
		 {
			  _adversary.injectFailure( typeof( System.NotSupportedException ), typeof( IOException ) );
			  return new AdversarialFileWatcher( @delegate.FileWatcher(), _adversary );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
		 public override StoreChannel Open( File fileName, OpenMode openMode )
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ) );
			  return AdversarialFileChannel.Wrap( ( StoreFileChannel ) @delegate.Open( fileName, openMode ), _adversary );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File from, java.io.File to, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
		 public override void RenameFile( File from, File to, params CopyOption[] copyOptions )
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( SecurityException ) );
			  @delegate.RenameFile( from, to, copyOptions );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
		 public override Stream OpenAsOutputStream( File fileName, bool append )
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( SecurityException ) );
			  return new AdversarialOutputStream( @delegate.OpenAsOutputStream( fileName, append ), _adversary );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel create(java.io.File fileName) throws java.io.IOException
		 public override StoreChannel Create( File fileName )
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ) );
			  return AdversarialFileChannel.Wrap( ( StoreFileChannel ) @delegate.Create( fileName ), _adversary );
		 }

		 public override bool Mkdir( File fileName )
		 {
			  _adversary.injectFailure( typeof( SecurityException ) );
			  return @delegate.Mkdir( fileName );
		 }

		 public override File[] ListFiles( File directory )
		 {
			  _adversary.injectFailure( typeof( SecurityException ) );
			  return @delegate.ListFiles( directory );
		 }

		 public override File[] ListFiles( File directory, FilenameFilter filter )
		 {
			  _adversary.injectFailure( typeof( SecurityException ) );
			  return @delegate.ListFiles( directory, filter );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer openAsWriter(java.io.File fileName, java.nio.charset.Charset charset, boolean append) throws java.io.IOException
		 public override Writer OpenAsWriter( File fileName, Charset charset, bool append )
		 {
			  _adversary.injectFailure( typeof( UnsupportedEncodingException ), typeof( FileNotFoundException ), typeof( SecurityException ) );
			  return new AdversarialWriter( @delegate.OpenAsWriter( fileName, charset, append ), _adversary );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Reader openAsReader(java.io.File fileName, java.nio.charset.Charset charset) throws java.io.IOException
		 public override Reader OpenAsReader( File fileName, Charset charset )
		 {
			  _adversary.injectFailure( typeof( UnsupportedEncodingException ), typeof( FileNotFoundException ), typeof( SecurityException ) );
			  return new AdversarialReader( @delegate.OpenAsReader( fileName, charset ), _adversary );
		 }

		 public override long GetFileSize( File fileName )
		 {
			  _adversary.injectFailure( typeof( SecurityException ) );
			  return @delegate.GetFileSize( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyFile(java.io.File from, java.io.File to) throws java.io.IOException
		 public override void CopyFile( File from, File to )
		 {
			  _adversary.injectFailure( typeof( SecurityException ), typeof( FileNotFoundException ), typeof( IOException ) );
			  @delegate.CopyFile( from, to );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyRecursively( File fromDirectory, File toDirectory )
		 {
			  _adversary.injectFailure( typeof( SecurityException ), typeof( IOException ), typeof( System.NullReferenceException ) );
			  @delegate.CopyRecursively( fromDirectory, toDirectory );
		 }

		 public override bool DeleteFile( File fileName )
		 {
			  _adversary.injectFailure( typeof( SecurityException ) );
			  return @delegate.DeleteFile( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException
		 public override Stream OpenAsInputStream( File fileName )
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( SecurityException ) );
			  return new AdversarialInputStream( @delegate.OpenAsInputStream( fileName ), _adversary );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void MoveToDirectory( File file, File toDirectory )
		 {
			  _adversary.injectFailure( typeof( SecurityException ), typeof( System.ArgumentException ), typeof( FileNotFoundException ), typeof( System.NullReferenceException ), typeof( IOException ) );
			  @delegate.MoveToDirectory( file, toDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyToDirectory( File file, File toDirectory )
		 {
			  _adversary.injectFailure( typeof( SecurityException ), typeof( System.ArgumentException ), typeof( FileNotFoundException ), typeof( System.NullReferenceException ), typeof( IOException ) );
			  @delegate.CopyToDirectory( file, toDirectory );
		 }

		 public override bool IsDirectory( File file )
		 {
			  _adversary.injectFailure( typeof( SecurityException ) );
			  return @delegate.IsDirectory( file );
		 }

		 public override bool FileExists( File file )
		 {
			  _adversary.injectFailure( typeof( SecurityException ) );
			  return @delegate.FileExists( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mkdirs(java.io.File fileName) throws java.io.IOException
		 public override void Mkdirs( File fileName )
		 {
			  _adversary.injectFailure( typeof( SecurityException ), typeof( IOException ) );
			  @delegate.Mkdirs( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteRecursively(java.io.File directory) throws java.io.IOException
		 public override void DeleteRecursively( File directory )
		 {
			  _adversary.injectFailure( typeof( SecurityException ), typeof( System.NullReferenceException ), typeof( IOException ) );
			  @delegate.DeleteRecursively( directory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(java.io.File path, long size) throws java.io.IOException
		 public override void Truncate( File path, long size )
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( System.ArgumentException ), typeof( SecurityException ), typeof( System.NullReferenceException ) );
			  @delegate.Truncate( path, size );
		 }

		 public override long LastModifiedTime( File file )
		 {
			  _adversary.injectFailure( typeof( SecurityException ), typeof( System.NullReferenceException ) );
			  return @delegate.LastModifiedTime( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteFileOrThrow(java.io.File file) throws java.io.IOException
		 public override void DeleteFileOrThrow( File file )
		 {
			  _adversary.injectFailure( typeof( NoSuchFileException ), typeof( IOException ), typeof( SecurityException ) );
			  @delegate.DeleteFileOrThrow( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<org.neo4j.io.fs.FileHandle> streamFilesRecursive(java.io.File directory) throws java.io.IOException
		 public override Stream<FileHandle> StreamFilesRecursive( File directory )
		 {
			  return StreamFilesRecursive.streamFilesRecursive( directory, this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( IOException ), typeof( SecurityException ) );
			  @delegate.Dispose();
		 }
	}

}