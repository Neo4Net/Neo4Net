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
	/// This FileSystemAbstract implementation delegates all calls to a given <seealso cref="FileSystem"/> implementation.
	/// This is useful for testing with arbitrary 3rd party file systems, such as Jimfs.
	/// </summary>
	public class DelegateFileSystemAbstraction : FileSystemAbstraction
	{
		 private readonly FileSystem _fs;

		 public DelegateFileSystemAbstraction( FileSystem fs )
		 {
			  this._fs = fs;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.watcher.FileWatcher fileWatcher() throws java.io.IOException
		 public override FileWatcher FileWatcher()
		 {
			  return new DefaultFileSystemWatcher( _fs.newWatchService() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreChannel open(java.io.File fileName, OpenMode openMode) throws java.io.IOException
		 public override StoreChannel Open( File fileName, OpenMode openMode )
		 {
			  return new StoreFileChannel( FileUtils.Open( Path( fileName ), openMode ) );
		 }

		 private Path Path( File fileName )
		 {
			  return path( fileName.Path );
		 }

		 private Path Path( string fileName )
		 {
			  return _fs.getPath( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
		 public override Stream OpenAsOutputStream( File fileName, bool append )
		 {
			  return FileUtils.OpenAsOutputStream( Path( fileName ), append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException
		 public override Stream OpenAsInputStream( File fileName )
		 {
			  return FileUtils.OpenAsInputStream( Path( fileName ) );
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
//ORIGINAL LINE: public StoreChannel create(java.io.File fileName) throws java.io.IOException
		 public override StoreChannel Create( File fileName )
		 {
			  return Open( fileName, OpenMode.ReadWrite );
		 }

		 public override bool FileExists( File file )
		 {
			  return Files.exists( Path( file ) );
		 }

		 public override bool Mkdir( File fileName )
		 {
			  if ( !FileExists( fileName ) )
			  {
					try
					{
						 Files.createDirectory( Path( fileName ) );
						 return true;
					}
					catch ( IOException )
					{
					}
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mkdirs(java.io.File fileName) throws java.io.IOException
		 public override void Mkdirs( File fileName )
		 {
			  Files.createDirectories( Path( fileName ) );
		 }

		 public override long GetFileSize( File fileName )
		 {
			  try
			  {
					return Files.size( Path( fileName ) );
			  }
			  catch ( IOException )
			  {
					return 0;
			  }
		 }

		 public override bool DeleteFile( File fileName )
		 {
			  try
			  {
					Files.delete( Path( fileName ) );
					return true;
			  }
			  catch ( IOException )
			  {
					return false;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteRecursively(java.io.File directory) throws java.io.IOException
		 public override void DeleteRecursively( File directory )
		 {
			  if ( FileExists( directory ) )
			  {
					FileUtils.DeletePathRecursively( Path( directory ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File from, java.io.File to, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
		 public override void RenameFile( File from, File to, params CopyOption[] copyOptions )
		 {
			  Files.move( Path( from ), Path( to ), copyOptions );
		 }

		 public override File[] ListFiles( File directory )
		 {
			  try
			  {
					  using ( Stream<Path> listing = Files.list( Path( directory ) ) )
					  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						return listing.map( Path.toFile ).toArray( File[]::new );
					  }
			  }
			  catch ( IOException )
			  {
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.io.File[] listFiles(java.io.File directory, final java.io.FilenameFilter filter)
		 public override File[] ListFiles( File directory, FilenameFilter filter )
		 {
			  try
			  {
					  using ( Stream<Path> listing = Files.list( Path( directory ) ) )
					  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						return listing.filter( entry => filter.accept( entry.Parent.toFile(), entry.FileName.ToString() ) ).map(Path.toFile).toArray(File[]::new);
					  }
			  }
			  catch ( IOException )
			  {
					return null;
			  }
		 }

		 public override bool IsDirectory( File file )
		 {
			  return Files.isDirectory( Path( file ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void MoveToDirectory( File file, File toDirectory )
		 {
			  Files.move( Path( file ), Path( toDirectory ).resolve( Path( file.Name ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyToDirectory( File file, File toDirectory )
		 {
			  Files.copy( Path( file ), Path( toDirectory ).resolve( file.Name ), REPLACE_EXISTING );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyFile(java.io.File from, java.io.File to) throws java.io.IOException
		 public override void CopyFile( File from, File to )
		 {
			  Files.copy( Path( from ), Path( to ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyRecursively( File fromDirectory, File toDirectory )
		 {
			  Path target = Path( toDirectory );
			  Path source = Path( fromDirectory );
			  CopyRecursively( source, target );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyRecursively(java.nio.file.Path source, java.nio.file.Path target) throws java.io.IOException
		 private void CopyRecursively( Path source, Path target )
		 {
			  using ( DirectoryStream<Path> directoryStream = Files.newDirectoryStream( source ) )
			  {
					foreach ( Path sourcePath in directoryStream )
					{
						 Path targetPath = target.resolve( sourcePath.FileName );
						 if ( Files.isDirectory( sourcePath ) )
						 {
							  Files.createDirectories( targetPath );
							  CopyRecursively( sourcePath, targetPath );
						 }
						 else
						 {
							  Files.copy( sourcePath, targetPath, REPLACE_EXISTING, StandardCopyOption.COPY_ATTRIBUTES );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(java.io.File path, long size) throws java.io.IOException
		 public override void Truncate( File path, long size )
		 {
			  using ( FileChannel channel = FileChannel.open( path( path ) ) )
			  {
					channel.truncate( size );
			  }
		 }

		 public override long LastModifiedTime( File file )
		 {
			  try
			  {
					return Files.getLastModifiedTime( Path( file ) ).toMillis();
			  }
			  catch ( IOException )
			  {
					return 0;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteFileOrThrow(java.io.File file) throws java.io.IOException
		 public override void DeleteFileOrThrow( File file )
		 {
			  Files.delete( Path( file ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<FileHandle> streamFilesRecursive(java.io.File directory) throws java.io.IOException
		 public override Stream<FileHandle> StreamFilesRecursive( File directory )
		 {
			  return StreamFilesRecursive.StreamFilesRecursiveConflict( directory, this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _fs );
		 }
	}

}