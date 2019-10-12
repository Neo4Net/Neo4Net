using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
namespace Org.Neo4j.Graphdb.mockfs
{

	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using FileHandle = Org.Neo4j.Io.fs.FileHandle;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using StoreFileChannel = Org.Neo4j.Io.fs.StoreFileChannel;
	using StreamFilesRecursive = Org.Neo4j.Io.fs.StreamFilesRecursive;
	using FileWatcher = Org.Neo4j.Io.fs.watcher.FileWatcher;
	using ChannelInputStream = Org.Neo4j.Test.impl.ChannelInputStream;
	using ChannelOutputStream = Org.Neo4j.Test.impl.ChannelOutputStream;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class EphemeralFileSystemAbstraction : FileSystemAbstraction
	{
		 private readonly Clock _clock;
		 private volatile bool _closed;

		 internal interface Positionable
		 {
			  long Pos();

			  void Pos( long position );
		 }

		 private readonly ISet<File> _directories = Collections.newSetFromMap( new ConcurrentDictionary<File>() );
		 private readonly IDictionary<File, EphemeralFileData> _files;

		 public EphemeralFileSystemAbstraction() : this(Clock.systemUTC())
		 {
		 }

		 public EphemeralFileSystemAbstraction( Clock clock )
		 {
			  this._clock = clock;
			  this._files = new ConcurrentDictionary<File, EphemeralFileData>();
			  InitCurrentWorkingDirectory();
		 }

		 private void InitCurrentWorkingDirectory()
		 {
			  try
			  {
					Mkdirs( ( new File( "." ) ).CanonicalFile );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( "EphemeralFileSystemAbstraction could not initialise current working directory", e );
			  }
		 }

		 private EphemeralFileSystemAbstraction( ISet<File> directories, IDictionary<File, EphemeralFileData> files, Clock clock )
		 {
			  this._clock = clock;
			  this._files = new ConcurrentDictionary<File, EphemeralFileData>( files );
			  this._directories.addAll( directories );
			  InitCurrentWorkingDirectory();
		 }

		 /// <summary>
		 /// Simulate a filesystem crash, in which any changes that have not been <seealso cref="StoreChannel.force"/>d
		 /// will be lost. Practically, all files revert to the state when they are last <seealso cref="StoreChannel.force"/>d.
		 /// </summary>
		 public virtual void Crash()
		 {
			  _files.Values.forEach( EphemeralFileSystemAbstraction.EphemeralFileData.crash );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void close() throws java.io.IOException
		 public override void Close()
		 {
			 lock ( this )
			 {
				  CloseFiles();
				  _closed = true;
			 }
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return _closed;
			 }
		 }

		 private void CloseFiles()
		 {
			  foreach ( EphemeralFileData file in _files.Values )
			  {
					file.Free();
			  }
			  _files.Clear();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertNoOpenFiles() throws Exception
		 public virtual void AssertNoOpenFiles()
		 {
			  FileStillOpenException exception = null;
			  foreach ( EphemeralFileData file in _files.Values )
			  {
					IEnumerator<EphemeralFileChannel> channels = file.OpenChannels;
					while ( channels.MoveNext() )
					{
						 EphemeralFileChannel channel = channels.Current;
						 if ( exception == null )
						 {
							  exception = channel.OpenedAt;
						 }
						 else
						 {
							  exception.addSuppressed( channel.OpenedAt );
						 }
					}
			  }
			  if ( exception != null )
			  {
					throw exception;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dumpZip(java.io.OutputStream output) throws java.io.IOException
		 public virtual void DumpZip( Stream output )
		 {
			  using ( ZipOutputStream zip = new ZipOutputStream( output ) )
			  {
					string prefix = null;
					foreach ( KeyValuePair<File, EphemeralFileData> entry in _files.SetOfKeyValuePairs() )
					{
						 File file = entry.Key;
						 string parent = file.ParentFile.AbsolutePath;
						 if ( string.ReferenceEquals( prefix, null ) || prefix.StartsWith( parent, StringComparison.Ordinal ) )
						 {
							  prefix = parent;
						 }
						 zip.putNextEntry( new ZipEntry( file.AbsolutePath ) );
						 entry.Value.dumpTo( zip );
						 zip.closeEntry();
					}
					if ( !string.ReferenceEquals( prefix, null ) )
					{
						 File directory = new File( prefix );
						 if ( directory.exists() ) // things ended up on the file system...
						 {
							  AddRecursively( zip, directory );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addRecursively(java.util.zip.ZipOutputStream output, java.io.File input) throws java.io.IOException
		 private void AddRecursively( ZipOutputStream output, File input )
		 {
			  if ( input.File )
			  {
					output.putNextEntry( new ZipEntry( input.AbsolutePath ) );
					sbyte[] scratchPad = EphemeralFileData.ScratchPad.get();
					using ( FileStream source = new FileStream( input, FileMode.Open, FileAccess.Read ) )
					{
						 for ( int read; 0 <= ( read = source.Read( scratchPad, 0, scratchPad.Length ) ); )
						 {
							  output.write( scratchPad, 0, read );
						 }
					}
					output.closeEntry();
			  }
			  else
			  {
					File[] children = input.listFiles();
					if ( children != null )
					{
						 foreach ( File child in children )
						 {
							  AddRecursively( output, child );
						 }
					}
			  }
		 }

		 public override FileWatcher FileWatcher()
		 {
			  return Org.Neo4j.Io.fs.watcher.FileWatcher_Fields.SilentWatcher;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
		 public override StoreChannel Open( File fileName, OpenMode openMode )
		 {
			 lock ( this )
			 {
				  EphemeralFileData data = _files[CanonicalFile( fileName )];
				  if ( data != null )
				  {
						return new StoreFileChannel( new EphemeralFileChannel( data, new FileStillOpenException( fileName.Path ) ) );
				  }
				  return Create( fileName );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
		 public override Stream OpenAsOutputStream( File fileName, bool append )
		 {
			  return new ChannelOutputStream( Open( fileName, OpenMode.READ_WRITE ), append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException
		 public override Stream OpenAsInputStream( File fileName )
		 {
			  return new ChannelInputStream( Open( fileName, OpenMode.READ ) );
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
//ORIGINAL LINE: public synchronized org.neo4j.io.fs.StoreChannel create(java.io.File fileName) throws java.io.IOException
		 public override StoreChannel Create( File fileName )
		 {
			 lock ( this )
			 {
				  File parentFile = fileName.ParentFile;
				  if ( parentFile != null && !FileExists( parentFile ) )
				  {
						throw new FileNotFoundException( "'" + fileName + "' (The system cannot find the path specified)" );
				  }
      
				  EphemeralFileData data = _files.computeIfAbsent( CanonicalFile( fileName ), key => new EphemeralFileData( _clock ) );
				  return new StoreFileChannel( new EphemeralFileChannel( data, new FileStillOpenException( fileName.Path ) ) );
			 }
		 }

		 public override long GetFileSize( File fileName )
		 {
			  EphemeralFileData file = _files[CanonicalFile( fileName )];
			  return file == null ? 0 : file.Size();
		 }

		 public override bool FileExists( File file )
		 {
			  file = CanonicalFile( file );
			  return _directories.Contains( file ) || _files.ContainsKey( file );
		 }

		 private File CanonicalFile( File file )
		 {
			  try
			  {
					return file.CanonicalFile;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( "EphemeralFileSystemAbstraction could not canonicalise file: " + file, e );
			  }
		 }

		 public override bool IsDirectory( File file )
		 {
			  return _directories.Contains( CanonicalFile( file ) );
		 }

		 public override bool Mkdir( File directory )
		 {
			  if ( FileExists( directory ) )
			  {
					return false;
			  }

			  _directories.Add( CanonicalFile( directory ) );
			  return true;
		 }

		 public override void Mkdirs( File directory )
		 {
			  File currentDirectory = CanonicalFile( directory );

			  while ( currentDirectory != null )
			  {
					Mkdir( currentDirectory );
					currentDirectory = currentDirectory.ParentFile;
			  }
		 }

		 public override bool DeleteFile( File fileName )
		 {
			  fileName = CanonicalFile( fileName );
			  EphemeralFileData removed = _files.Remove( fileName );
			  if ( removed != null )
			  {
					removed.Free();
					return true;
			  }
			  else
			  {
					File[] files = ListFiles( fileName );
					return files != null && Files.Length == 0 && _directories.remove( fileName );
			  }
		 }

		 public override void DeleteRecursively( File path )
		 {
			  if ( IsDirectory( path ) )
			  {
					IList<string> directoryPathItems = SplitPath( CanonicalFile( path ) );
					foreach ( KeyValuePair<File, EphemeralFileData> file in _files.SetOfKeyValuePairs() )
					{
						 File fileName = file.Key;
						 IList<string> fileNamePathItems = SplitPath( fileName );
						 if ( DirectoryMatches( directoryPathItems, fileNamePathItems ) )
						 {
							  DeleteFile( fileName );
						 }
					}
			  }
			  DeleteFile( path );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File from, java.io.File to, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
		 public override void RenameFile( File from, File to, params CopyOption[] copyOptions )
		 {
			  from = CanonicalFile( from );
			  to = CanonicalFile( to );

			  if ( !_files.ContainsKey( from ) )
			  {
					throw new NoSuchFileException( "'" + from + "' doesn't exist" );
			  }

			  bool replaceExisting = false;
			  foreach ( CopyOption copyOption in copyOptions )
			  {
					replaceExisting |= copyOption == REPLACE_EXISTING;
			  }
			  if ( _files.ContainsKey( to ) && !replaceExisting )
			  {
					throw new FileAlreadyExistsException( "'" + to + "' already exists" );
			  }
			  if ( !IsDirectory( to.ParentFile ) )
			  {
					throw new NoSuchFileException( "Target directory[" + to.Parent + "] does not exists" );
			  }
			  _files[to] = _files.Remove( from );
		 }

		 public override File[] ListFiles( File directory )
		 {
			  directory = CanonicalFile( directory );
			  if ( _files.ContainsKey( directory ) || !_directories.Contains( directory ) )
			  {
					// This means that you're trying to list files on a file, not a directory.
					return null;
			  }

			  IList<string> directoryPathItems = SplitPath( directory );
			  ISet<File> found = new HashSet<File>();
			  IEnumerator<File> files = new CombiningIterator<File>( asList( this._files.Keys.GetEnumerator(), _directories.GetEnumerator() ) );
			  while ( Files.MoveNext() )
			  {
					File file = Files.Current;
					IList<string> fileNamePathItems = SplitPath( file );
					if ( DirectoryMatches( directoryPathItems, fileNamePathItems ) )
					{
						 found.Add( ConstructPath( fileNamePathItems, directoryPathItems ) );
					}
			  }

			  return found.toArray( new File[found.Count] );
		 }

		 public override File[] ListFiles( File directory, FilenameFilter filter )
		 {
			  directory = CanonicalFile( directory );
			  if ( _files.ContainsKey( directory ) )
			  {
			  // This means that you're trying to list files on a file, not a directory.
					return null;
			  }

			  IList<string> directoryPathItems = SplitPath( directory );
			  ISet<File> found = new HashSet<File>();
			  IEnumerator<File> files = new CombiningIterator<File>( asList( this._files.Keys.GetEnumerator(), _directories.GetEnumerator() ) );
			  while ( Files.MoveNext() )
			  {
					File file = Files.Current;
					IList<string> fileNamePathItems = SplitPath( file );
					if ( DirectoryMatches( directoryPathItems, fileNamePathItems ) )
					{
						 File path = ConstructPath( fileNamePathItems, directoryPathItems );
						 if ( filter.accept( path.ParentFile, path.Name ) )
						 {
							  found.Add( path );
						 }
					}
			  }
			  return found.toArray( new File[found.Count] );
		 }

		 private File ConstructPath( IList<string> pathItems, IList<string> @base )
		 {
			  File file = null;
			  if ( @base.Count > 0 )
			  {
					// We're not directly basing off the root directory
					pathItems = pathItems.subList( 0, @base.Count + 1 );
			  }
			  foreach ( string pathItem in pathItems )
			  {
					string pathItemName = pathItem + File.separator;
					file = file == null ? new File( pathItemName ) : new File( file, pathItemName );
			  }
			  return file;
		 }

		 private bool DirectoryMatches( IList<string> directoryPathItems, IList<string> fileNamePathItems )
		 {
			  return fileNamePathItems.Count > directoryPathItems.Count && fileNamePathItems.subList( 0, directoryPathItems.Count ).Equals( directoryPathItems );
		 }

		 private IList<string> SplitPath( File path )
		 {
			  return new IList<string> { path.Path.replaceAll( "\\\\", "/" ).Split( "/" ) };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void moveToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void MoveToDirectory( File file, File toDirectory )
		 {
			  if ( IsDirectory( file ) )
			  {
					File inner = new File( toDirectory, file.Name );
					Mkdir( inner );
					foreach ( File f in ListFiles( file ) )
					{
						 MoveToDirectory( f, inner );
					}
					DeleteFile( file );
			  }
			  else
			  {
					EphemeralFileData fileToMove = _files.Remove( CanonicalFile( file ) );
					if ( fileToMove == null )
					{
						 throw new FileNotFoundException( file.Path );
					}
					_files[CanonicalFile( new File( toDirectory, file.Name ) )] = fileToMove;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyToDirectory(java.io.File file, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyToDirectory( File file, File toDirectory )
		 {
			  File targetFile = new File( toDirectory, file.Name );
			  CopyFile( file, targetFile );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyFile(java.io.File from, java.io.File to) throws java.io.IOException
		 public override void CopyFile( File from, File to )
		 {
			  EphemeralFileData data = _files[CanonicalFile( from )];
			  if ( data == null )
			  {
					throw new FileNotFoundException( "File " + from + " not found" );
			  }
			  CopyFile( from, this, to, NewCopyBuffer() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException
		 public override void CopyRecursively( File fromDirectory, File toDirectory )
		 {
			  CopyRecursivelyFromOtherFs( fromDirectory, this, toDirectory, NewCopyBuffer() );
		 }

		 public virtual EphemeralFileSystemAbstraction Snapshot()
		 {
			  IDictionary<File, EphemeralFileData> copiedFiles = new Dictionary<File, EphemeralFileData>();
			  foreach ( KeyValuePair<File, EphemeralFileData> file in _files.SetOfKeyValuePairs() )
			  {
					copiedFiles[file.Key] = file.Value.copy();
			  }
			  return new EphemeralFileSystemAbstraction( _directories, copiedFiles, _clock );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void copyRecursivelyFromOtherFs(java.io.File from, org.neo4j.io.fs.FileSystemAbstraction fromFs, java.io.File to) throws java.io.IOException
		 public virtual void CopyRecursivelyFromOtherFs( File from, FileSystemAbstraction fromFs, File to )
		 {
			  CopyRecursivelyFromOtherFs( from, fromFs, to, NewCopyBuffer() );
		 }

		 public virtual long Checksum()
		 {
			  Checksum checksum = new CRC32();
			  sbyte[] data = new sbyte[( int ) ByteUnit.kibiBytes( 1 )];

			  // Go through file name list in sorted order, so that checksum is consistent
			  IList<File> names = new List<File>( _files.Count );
			  ( ( IList<File> )names ).AddRange( _files.Keys );

			  names.sort( System.Collections.IComparer.comparing( File.getAbsolutePath ) );

			  foreach ( File name in names )
			  {
					EphemeralFileData file = _files[name];
					ByteBuffer buf = file.FileAsBuffer.buf();
					buf.position( 0 );
					while ( buf.position() < buf.limit() )
					{
						 int len = Math.Min( data.Length, buf.limit() - buf.position() );
						 buf.get( data );
						 checksum.update( data, 0, len );
					}
			  }
			  return checksum.Value;
		 }

		 private ByteBuffer NewCopyBuffer()
		 {
			  return ByteBuffer.allocate( ( int ) ByteUnit.mebiBytes( 1 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyRecursivelyFromOtherFs(java.io.File from, org.neo4j.io.fs.FileSystemAbstraction fromFs, java.io.File to, ByteBuffer buffer) throws java.io.IOException
		 private void CopyRecursivelyFromOtherFs( File from, FileSystemAbstraction fromFs, File to, ByteBuffer buffer )
		 {
			  this.Mkdirs( to );
			  foreach ( File fromFile in fromFs.ListFiles( from ) )
			  {
					File toFile = new File( to, fromFile.Name );
					if ( fromFs.IsDirectory( fromFile ) )
					{
						 CopyRecursivelyFromOtherFs( fromFile, fromFs, toFile );
					}
					else
					{
						 CopyFile( fromFile, fromFs, toFile, buffer );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyFile(java.io.File from, org.neo4j.io.fs.FileSystemAbstraction fromFs, java.io.File to, ByteBuffer buffer) throws java.io.IOException
		 private void CopyFile( File from, FileSystemAbstraction fromFs, File to, ByteBuffer buffer )
		 {
			  using ( StoreChannel source = fromFs.Open( from, OpenMode.READ ), StoreChannel sink = this.Open( to, OpenMode.READ_WRITE ) )
			  {
					sink.Truncate( 0 );
					for ( int available; ( available = ( int )( source.size() - source.position() ) ) > 0; )
					{
						 buffer.clear();
						 buffer.limit( min( available, buffer.capacity() ) );
						 source.read( buffer );
						 buffer.flip();
						 sink.write( buffer );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(java.io.File file, long size) throws java.io.IOException
		 public override void Truncate( File file, long size )
		 {
			  EphemeralFileData data = _files[CanonicalFile( file )];
			  if ( data == null )
			  {
					throw new FileNotFoundException( "File " + file + " not found" );
			  }
			  data.Truncate( size );
		 }

		 public override long LastModifiedTime( File file )
		 {
			  EphemeralFileData data = _files[CanonicalFile( file )];
			  if ( data == null )
			  {
					return 0;
			  }
			  return data.LastModified;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteFileOrThrow(java.io.File file) throws java.io.IOException
		 public override void DeleteFileOrThrow( File file )
		 {
			  file = CanonicalFile( file );
			  if ( !FileExists( file ) )
			  {
					throw new NoSuchFileException( file.AbsolutePath );
			  }
			  if ( !DeleteFile( file ) )
			  {
					throw new IOException( "Could not delete file: " + file );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<org.neo4j.io.fs.FileHandle> streamFilesRecursive(java.io.File directory) throws java.io.IOException
		 public override Stream<FileHandle> StreamFilesRecursive( File directory )
		 {
			  return StreamFilesRecursive.streamFilesRecursive( directory, this );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") private static class FileStillOpenException extends Exception
		 private class FileStillOpenException : Exception
		 {
			  internal readonly string Filename;

			  internal FileStillOpenException( string filename ) : base( "File still open: [" + filename + "]" )
			  {
					this.Filename = filename;
			  }
		 }

		 internal class LocalPosition : Positionable
		 {
			  internal long Position;

			  internal LocalPosition( long position )
			  {
					this.Position = position;
			  }

			  public override long Pos()
			  {
					return Position;
			  }

			  public override void Pos( long position )
			  {
					this.Position = position;
			  }
		 }

		 private class EphemeralFileChannel : FileChannel, Positionable
		 {
			  internal readonly FileStillOpenException OpenedAt;
			  internal readonly EphemeralFileData Data;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long PositionConflict;

			  internal EphemeralFileChannel( EphemeralFileData data, FileStillOpenException opened )
			  {
					this.Data = data;
					this.OpenedAt = opened;
					data.Open( this );
			  }

			  public override string ToString()
			  {
					return string.Format( "{0}[{1}]", this.GetType().Name, OpenedAt.filename );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkIfClosedOrInterrupted() throws java.io.IOException
			  internal virtual void CheckIfClosedOrInterrupted()
			  {
					if ( !Open )
					{
						 throw new ClosedChannelException();
					}

					if ( Thread.CurrentThread.Interrupted )
					{
						 outerInstance.close();
						 throw new ClosedByInterruptException();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
			  public override int Read( ByteBuffer dst )
			  {
					CheckIfClosedOrInterrupted();
					return Data.read( this, dst );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(ByteBuffer[] dsts, int offset, int length) throws java.io.IOException
			  public override long Read( ByteBuffer[] dsts, int offset, int length )
			  {
					CheckIfClosedOrInterrupted();
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer src) throws java.io.IOException
			  public override int Write( ByteBuffer src )
			  {
					CheckIfClosedOrInterrupted();
					return Data.write( this, src );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs, int offset, int length) throws java.io.IOException
			  public override long Write( ByteBuffer[] srcs, int offset, int length )
			  {
					CheckIfClosedOrInterrupted();
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long position() throws java.io.IOException
			  public override long Position()
			  {
					CheckIfClosedOrInterrupted();
					return PositionConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.channels.FileChannel position(long newPosition) throws java.io.IOException
			  public override FileChannel Position( long newPosition )
			  {
					CheckIfClosedOrInterrupted();
					this.PositionConflict = newPosition;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long size() throws java.io.IOException
			  public override long Size()
			  {
					CheckIfClosedOrInterrupted();
					return Data.size();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.channels.FileChannel truncate(long size) throws java.io.IOException
			  public override FileChannel Truncate( long size )
			  {
					CheckIfClosedOrInterrupted();
					Data.truncate( size );
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(boolean metaData) throws java.io.IOException
			  public override void Force( bool metaData )
			  {
					CheckIfClosedOrInterrupted();
					// Otherwise no forcing of an in-memory file
					Data.force();
			  }

			  public override long TransferTo( long position, long count, WritableByteChannel target )
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long transferFrom(java.nio.channels.ReadableByteChannel src, long position, long count) throws java.io.IOException
			  public override long TransferFrom( ReadableByteChannel src, long position, long count )
			  {
					CheckIfClosedOrInterrupted();
					long previousPos = position();
					position( position );
					try
					{
						 long transferred = 0;
						 ByteBuffer intermediary = ByteBuffer.allocate( ( int ) ByteUnit.mebiBytes( 8 ) );
						 while ( transferred < count )
						 {
							  intermediary.clear();
							  intermediary.limit( ( int ) min( intermediary.capacity(), count - transferred ) );
							  int read = src.read( intermediary );
							  if ( read == -1 )
							  {
									break;
							  }
							  transferred += read;
							  intermediary.flip();
						 }
						 return transferred;
					}
					finally
					{
						 position( previousPos );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst, long position) throws java.io.IOException
			  public override int Read( ByteBuffer dst, long position )
			  {
					CheckIfClosedOrInterrupted();
					return Data.read( new LocalPosition( position ), dst );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer src, long position) throws java.io.IOException
			  public override int Write( ByteBuffer src, long position )
			  {
					CheckIfClosedOrInterrupted();
					return Data.write( new LocalPosition( position ), src );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.MappedByteBuffer map(java.nio.channels.FileChannel.MapMode mode, long position, long size) throws java.io.IOException
			  public override MappedByteBuffer Map( FileChannel.MapMode mode, long position, long size )
			  {
					CheckIfClosedOrInterrupted();
					throw new IOException( "Not supported" );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.channels.FileLock lock(long position, long size, boolean shared) throws java.io.IOException
			  public override java.nio.channels.FileLock Lock( long position, long size, bool shared )
			  {
					CheckIfClosedOrInterrupted();
					lock ( Data.channels )
					{
						 if ( !Data.@lock() )
						 {
							  return null;
						 }
						 return new EphemeralFileLock( this, Data );
					}
			  }

			  public override java.nio.channels.FileLock TryLock( long position, long size, bool shared )
			  {
					lock ( Data.channels )
					{
						 if ( !Data.@lock() )
						 {
							  throw new OverlappingFileLockException();
						 }
						 return new EphemeralFileLock( this, Data );
					}
			  }

			  protected internal override void ImplCloseChannel()
			  {
					Data.close( this );
			  }

			  public override long Pos()
			  {
					return PositionConflict;
			  }

			  public override void Pos( long position )
			  {
					this.PositionConflict = position;
			  }
		 }

		 private class EphemeralFileData
		 {
			  internal static readonly ThreadLocal<sbyte[]> ScratchPad = ThreadLocal.withInitial( () => new sbyte[(int) ByteUnit.kibiBytes(1)] );
			  internal DynamicByteBuffer FileAsBuffer;
			  internal DynamicByteBuffer ForcedBuffer;
			  internal readonly ICollection<WeakReference<EphemeralFileChannel>> Channels = new LinkedList<WeakReference<EphemeralFileChannel>>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int SizeConflict;
			  internal int ForcedSize;
			  internal int Locked;
			  internal readonly Clock Clock;
			  internal long LastModified;

			  internal EphemeralFileData( Clock clock ) : this( new DynamicByteBuffer(), clock )
			  {
			  }

			  internal EphemeralFileData( DynamicByteBuffer data, Clock clock )
			  {
					this.FileAsBuffer = data;
					this.ForcedBuffer = data.Copy();
					this.Clock = clock;
					this.LastModified = clock.millis();
			  }

			  internal virtual int Read( Positionable fc, ByteBuffer dst )
			  {
					int wanted = dst.limit() - dst.position();
					long size = size();
					int available = min( wanted, ( int )( size - fc.Pos() ) );
					if ( available <= 0 )
					{
						 return -1; // EOF
					}
					int pending = available;
					// Read up until our internal size
					sbyte[] scratchPad = ScratchPad.get();
					while ( pending > 0 )
					{
						 int howMuchToReadThisTime = min( pending, scratchPad.Length );
						 long pos = fc.Pos();
						 FileAsBuffer.get( ( int ) pos, scratchPad, 0, howMuchToReadThisTime );
						 fc.Pos( pos + howMuchToReadThisTime );
						 dst.put( scratchPad, 0, howMuchToReadThisTime );
						 pending -= howMuchToReadThisTime;
					}
					return available; // return how much data was read
			  }

			  internal virtual int Write( Positionable fc, ByteBuffer src )
			  {
				  lock ( this )
				  {
						int wanted = src.limit() - src.position();
						int pending = wanted;
						sbyte[] scratchPad = ScratchPad.get();
      
						while ( pending > 0 )
						{
							 int howMuchToWriteThisTime = min( pending, scratchPad.Length );
							 src.get( scratchPad, 0, howMuchToWriteThisTime );
							 long pos = fc.Pos();
							 FileAsBuffer.put( ( int ) pos, scratchPad, 0, howMuchToWriteThisTime );
							 fc.Pos( pos + howMuchToWriteThisTime );
							 pending -= howMuchToWriteThisTime;
						}
      
						// If we just made a jump in the file fill the rest of the gap with zeros
						int newSize = max( SizeConflict, ( int ) fc.Pos() );
						int intermediaryBytes = newSize - wanted - SizeConflict;
						if ( intermediaryBytes > 0 )
						{
							 FileAsBuffer.fillWithZeros( SizeConflict, intermediaryBytes );
						}
      
						SizeConflict = newSize;
						LastModified = Clock.millis();
						return wanted;
				  }
			  }

			  internal virtual EphemeralFileData Copy()
			  {
				  lock ( this )
				  {
						EphemeralFileData copy = new EphemeralFileData( FileAsBuffer.copy(), Clock );
						copy.SizeConflict = SizeConflict;
						return copy;
				  }
			  }

			  internal virtual void Free()
			  {
					FileAsBuffer.free();
			  }

			  internal virtual void Open( EphemeralFileChannel channel )
			  {
					lock ( Channels )
					{
						 Channels.Add( new WeakReference<>( channel ) );
					}
			  }

			  internal virtual void Force()
			  {
				  lock ( this )
				  {
						ForcedBuffer = FileAsBuffer.copy();
						ForcedSize = SizeConflict;
				  }
			  }

			  internal virtual void Crash()
			  {
				  lock ( this )
				  {
						FileAsBuffer = ForcedBuffer.copy();
						SizeConflict = ForcedSize;
				  }
			  }

			  internal virtual void Close( EphemeralFileChannel channel )
			  {
					lock ( Channels )
					{
						 Locked = 0; // Regular file systems seems to release all file locks when closed...
						 for ( IEnumerator<EphemeralFileChannel> iter = OpenChannels; iter.MoveNext(); )
						 {
							  if ( iter.Current == channel )
							  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
									iter.remove();
							  }
						 }
					}
			  }

			  internal virtual IEnumerator<EphemeralFileChannel> OpenChannels
			  {
				  get
				  {
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final java.util.Iterator<WeakReference<EphemeralFileChannel>> refs = channels.iterator();
						IEnumerator<WeakReference<EphemeralFileChannel>> refs = Channels.GetEnumerator();
   
						return new PrefetchingIteratorAnonymousInnerClass( this, refs );
				  }
			  }

			  private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<EphemeralFileChannel>
			  {
				  private readonly EphemeralFileData _outerInstance;

				  private IEnumerator<WeakReference<EphemeralFileChannel>> _refs;

				  public PrefetchingIteratorAnonymousInnerClass( EphemeralFileData outerInstance, IEnumerator<WeakReference<EphemeralFileChannel>> refs )
				  {
					  this.outerInstance = outerInstance;
					  this._refs = refs;
				  }

				  protected internal override EphemeralFileChannel fetchNextOrNull()
				  {
						while ( _refs.MoveNext() )
						{
							 EphemeralFileChannel channel = _refs.Current.get();
							 if ( channel != null )
							 {
								  return channel;
							 }
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							 _refs.remove();
						}
						return null;
				  }

				  public override void remove()
				  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						_refs.remove();
				  }
			  }

			  internal virtual long Size()
			  {
				  lock ( this )
				  {
						return SizeConflict;
				  }
			  }

			  internal virtual void Truncate( long newSize )
			  {
				  lock ( this )
				  {
						this.SizeConflict = ( int ) newSize;
				  }
			  }

			  internal virtual bool Lock()
			  {
					return Locked == 0;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized void dumpTo(java.io.OutputStream target) throws java.io.IOException
			  internal virtual void DumpTo( Stream target )
			  {
				  lock ( this )
				  {
						sbyte[] scratchPad = ScratchPad.get();
						FileAsBuffer.dump( target, scratchPad, SizeConflict );
				  }
			  }

			  public override string ToString()
			  {
					return "size: " + SizeConflict + ", locked:" + Locked;
			  }
		 }

		 private class EphemeralFileLock : java.nio.channels.FileLock
		 {
			  internal EphemeralFileData File;

			  internal EphemeralFileLock( EphemeralFileChannel channel, EphemeralFileData file ) : base( channel, 0, long.MaxValue, false )
			  {
					this.File = file;
					file.Locked++;
			  }

			  public override bool Valid
			  {
				  get
				  {
						return File != null;
				  }
			  }

			  public override void Release()
			  {
					lock ( File.channels )
					{
						 if ( File == null || File.locked == 0 )
						 {
							  return;
						 }
						 File.locked--;
						 File = null;
					}
			  }
		 }

		 /// <summary>
		 /// Dynamically expanding ByteBuffer substitute/wrapper. This will allocate ByteBuffers on the go
		 /// so that we don't have to allocate too big of a buffer up-front.
		 /// </summary>
		 internal class DynamicByteBuffer
		 {
			  internal static readonly sbyte[] ZeroBuffer = new sbyte[( int ) ByteUnit.kibiBytes( 1 )];
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal ByteBuffer BufConflict;
			  internal Exception FreeCall;

			  internal DynamicByteBuffer()
			  {
					BufConflict = Allocate( ByteUnit.kibiBytes( 1 ) );
			  }

			  public virtual ByteBuffer Buf()
			  {
					AssertNotFreed();
					return BufConflict;
			  }

			  /// <summary>
			  /// This is a copying constructor, the input buffer is just read from, never stored in 'this'. </summary>
			  internal DynamicByteBuffer( ByteBuffer toClone )
			  {
					BufConflict = Allocate( toClone.capacity() );
					CopyByteBufferContents( toClone, BufConflict );
			  }

			  internal virtual DynamicByteBuffer Copy()
			  {
				  lock ( this )
				  {
						return new DynamicByteBuffer( Buf() ); // invoke "copy constructor"
				  }
			  }

			  internal virtual void CopyByteBufferContents( ByteBuffer from, ByteBuffer to )
			  {
					int positionBefore = from.position();
					try
					{
						 from.position( 0 );
						 to.put( from );
					}
					finally
					{
						 from.position( positionBefore );
						 to.position( 0 );
					}
			  }

			  internal virtual ByteBuffer Allocate( long capacity )
			  {
					return ByteBuffer.allocate( Math.toIntExact( capacity ) );
			  }

			  internal virtual void Free()
			  {
					AssertNotFreed();
					try
					{
						 Clear();
					}
					finally
					{
						 BufConflict = null;
						 FreeCall = new Exception( "You're most likely seeing this exception because there was an attempt to use this buffer " + "after it was freed. This stack trace may help you figure out where and why it was freed" );
					}
			  }

			  internal virtual void Put( int pos, sbyte[] bytes, int offset, int length )
			  {
				  lock ( this )
				  {
						VerifySize( pos + length );
						ByteBuffer buf = buf();
						try
						{
							 buf.position( pos );
						}
						catch ( System.ArgumentException e )
						{
							 throw new System.ArgumentException( buf + ", " + pos, e );
						}
						buf.put( bytes, offset, length );
				  }
			  }

			  internal virtual void Get( int pos, sbyte[] scratchPad, int i, int howMuchToReadThisTime )
			  {
				  lock ( this )
				  {
						ByteBuffer buf = buf();
						buf.position( pos );
						buf.get( scratchPad, i, howMuchToReadThisTime );
				  }
			  }

			  internal virtual void FillWithZeros( int pos, int bytes )
			  {
				  lock ( this )
				  {
						ByteBuffer buf = buf();
						buf.position( pos );
						while ( bytes > 0 )
						{
							 int howMuchToReadThisTime = min( bytes, ZeroBuffer.Length );
							 buf.put( ZeroBuffer, 0, howMuchToReadThisTime );
							 bytes -= howMuchToReadThisTime;
						}
						buf.position( pos );
				  }
			  }

			  /// <summary>
			  /// Checks if more space needs to be allocated.
			  /// </summary>
			  internal virtual void VerifySize( int totalAmount )
			  {
					ByteBuffer buf = buf();
					if ( buf.capacity() >= totalAmount )
					{
						 return;
					}

					int newSize = buf.capacity();
					long maxSize = ByteUnit.gibiBytes( 1 );
					CheckAllowedSize( totalAmount, maxSize );
					while ( newSize < totalAmount )
					{
						 newSize = newSize << 1;
						 CheckAllowedSize( newSize, maxSize );
					}
					int oldPosition = buf.position();

					// allocate new buffer
					ByteBuffer newBuf = Allocate( newSize );

					// copy contents of current buffer into new buffer
					buf.position( 0 );
					newBuf.put( buf );

					// re-assign buffer to new buffer
					newBuf.position( oldPosition );
					this.BufConflict = newBuf;
			  }

			  internal virtual void CheckAllowedSize( long size, long maxSize )
			  {
					if ( size > maxSize )
					{
						 throw new Exception( "Requested file size is too big for ephemeral file system." );
					}
			  }

			  public virtual void Clear()
			  {
					Buf().clear();
			  }

			  internal virtual void AssertNotFreed()
			  {
					if ( this.BufConflict == null )
					{
						 throw new System.InvalidOperationException( "This buffer have been freed", FreeCall );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dump(java.io.OutputStream target, byte[] scratchPad, int size) throws java.io.IOException
			  internal virtual void Dump( Stream target, sbyte[] scratchPad, int size )
			  {
					ByteBuffer buf = buf();
					buf.position( 0 );
					while ( size > 0 )
					{
						 int read = min( size, scratchPad.Length );
						 buf.get( scratchPad, 0, read );
						 size -= read;
						 target.Write( scratchPad, 0, read );
					}
			  }
		 }

		 // Copied from kernel since we don't want to depend on that module here
		 private abstract class PrefetchingIterator<T> : IEnumerator<T>
		 {
			  internal bool HasFetchedNext;
			  internal T NextObject;

			  /// <returns> {@code true} if there is a next item to be returned from the next
			  /// call to <seealso cref="next()"/>. </returns>
			  public override bool HasNext()
			  {
					return Peek() != default(T);
			  }

			  /// <returns> the next element that will be returned from <seealso cref="next()"/> without
			  /// actually advancing the iterator </returns>
			  public virtual T Peek()
			  {
					if ( HasFetchedNext )
					{
						 return NextObject;
					}

					NextObject = FetchNextOrNull();
					HasFetchedNext = true;
					return NextObject;
			  }

			  /// <summary>
			  /// Uses <seealso cref="hasNext()"/> to try to fetch the next item and returns it
			  /// if found, otherwise it throws a <seealso cref="java.util.NoSuchElementException"/>.
			  /// </summary>
			  /// <returns> the next item in the iteration, or throws
			  /// <seealso cref="java.util.NoSuchElementException"/> if there's no more items to return. </returns>
			  public override T Next()
			  {
					if ( !HasNext() )
					{
						 throw new NoSuchElementException();
					}
					T result = NextObject;
					NextObject = default( T );
					HasFetchedNext = false;
					return result;
			  }

			  protected internal abstract T FetchNextOrNull();

			  public override void Remove()
			  {
					throw new System.NotSupportedException();
			  }
		 }

		 private class CombiningIterator<T> : PrefetchingIterator<T>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Iterator<? extends java.util.Iterator<T>> iterators;
			  internal IEnumerator<IEnumerator<T>> Iterators;
			  internal IEnumerator<T> CurrentIterator;

			  internal CombiningIterator<T1>( IEnumerable<T1> iterators ) where T1 : IEnumerator<T> : this( iterators.GetEnumerator() )
			  {
			  }

			  internal CombiningIterator<T1>( IEnumerator<T1> iterators ) where T1 : IEnumerator<T>
			  {
					this.Iterators = iterators;
			  }

			  protected internal override T FetchNextOrNull()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( CurrentIterator == null || !CurrentIterator.hasNext() )
					{
						 while ( ( CurrentIterator = NextIteratorOrNull() ) != null )
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  if ( CurrentIterator.hasNext() )
							  {
									break;
							  }
						 }
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return CurrentIterator != null && CurrentIterator.hasNext() ? CurrentIterator.next() : default(T);
			  }

			  protected internal virtual IEnumerator<T> NextIteratorOrNull()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( Iterators.hasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 return Iterators.next();
					}
					return null;
			  }
		 }
	}

}