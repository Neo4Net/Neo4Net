using System;
using System.IO;
using System.Text;
using System.Threading;

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
	using SystemUtils = org.apache.commons.lang3.SystemUtils;



	public class FileUtils
	{
		 private const int NUMBER_OF_RETRIES = 5;

		 private FileUtils()
		 {
			  throw new AssertionError();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void deleteRecursively(java.io.File directory) throws java.io.IOException
		 public static void DeleteRecursively( File directory )
		 {
			  if ( !directory.exists() )
			  {
					return;
			  }
			  Path path = directory.toPath();
			  DeletePathRecursively( path );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void deletePathRecursively(java.nio.file.Path path) throws java.io.IOException
		 public static void DeletePathRecursively( Path path )
		 {
			  Files.walkFileTree( path, new SimpleFileVisitorAnonymousInnerClass() );
		 }

		 private class SimpleFileVisitorAnonymousInnerClass : SimpleFileVisitor<Path>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult visitFile(java.nio.file.Path file, java.nio.file.attribute.BasicFileAttributes attrs) throws java.io.IOException
			 public override FileVisitResult visitFile( Path file, BasicFileAttributes attrs )
			 {
				  DeleteFile( file );
				  return FileVisitResult.CONTINUE;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult postVisitDirectory(java.nio.file.Path dir, java.io.IOException e) throws java.io.IOException
			 public override FileVisitResult postVisitDirectory( Path dir, IOException e )
			 {
				  if ( e != null )
				  {
						throw e;
				  }
				  Files.delete( dir );
				  return FileVisitResult.CONTINUE;
			 }
		 }

		 public static bool DeleteFile( File file )
		 {
			  if ( !file.exists() )
			  {
					return true;
			  }
			  int count = 0;
			  bool deleted;
			  do
			  {
					deleted = file.delete();
					if ( !deleted )
					{
						 count++;
						 WaitAndThenTriggerGC();
					}
			  } while ( !deleted && count <= NUMBER_OF_RETRIES );
			  return deleted;
		 }

		 /// <summary>
		 /// Utility method that moves a file from its current location to the
		 /// new target location. If rename fails (for example if the target is
		 /// another disk) a copy/delete will be performed instead. This is not a rename,
		 /// use <seealso cref="renameFile(File, File, CopyOption...)"/> instead.
		 /// </summary>
		 /// <param name="toMove"> The File object to move. </param>
		 /// <param name="target"> Target file to move to. </param>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void moveFile(java.io.File toMove, java.io.File target) throws java.io.IOException
		 public static void MoveFile( File toMove, File target )
		 {
			  if ( !toMove.exists() )
			  {
					throw new FileNotFoundException( "Source file[" + toMove.AbsolutePath + "] not found" );
			  }
			  if ( target.exists() )
			  {
					throw new IOException( "Target file[" + target.AbsolutePath + "] already exists" );
			  }

			  if ( toMove.renameTo( target ) )
			  {
					return;
			  }

			  if ( toMove.Directory )
			  {
					Files.createDirectories( target.toPath() );
					CopyRecursively( toMove, target );
					DeleteRecursively( toMove );
			  }
			  else
			  {
					CopyFile( toMove, target );
					DeleteFile( toMove );
			  }
		 }

		 /// <summary>
		 /// Utility method that moves a file from its current location to the
		 /// provided target directory. If rename fails (for example if the target is
		 /// another disk) a copy/delete will be performed instead. This is not a rename,
		 /// use <seealso cref="renameFile(File, File, CopyOption...)"/> instead.
		 /// </summary>
		 /// <param name="toMove"> The File object to move. </param>
		 /// <param name="targetDirectory"> the destination directory </param>
		 /// <returns> the new file, null iff the move was unsuccessful </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File moveFileToDirectory(java.io.File toMove, java.io.File targetDirectory) throws java.io.IOException
		 public static File MoveFileToDirectory( File toMove, File targetDirectory )
		 {
			  if ( !targetDirectory.Directory )
			  {
					throw new System.ArgumentException( "Move target must be a directory, not " + targetDirectory );
			  }

			  File target = new File( targetDirectory, toMove.Name );
			  MoveFile( toMove, target );
			  return target;
		 }

		 /// <summary>
		 /// Utility method that copy a file from its current location to the
		 /// provided target directory.
		 /// </summary>
		 /// <param name="file"> file that needs to be copied. </param>
		 /// <param name="targetDirectory"> the destination directory </param>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void copyFileToDirectory(java.io.File file, java.io.File targetDirectory) throws java.io.IOException
		 public static void CopyFileToDirectory( File file, File targetDirectory )
		 {
			  if ( !targetDirectory.exists() )
			  {
					Files.createDirectories( targetDirectory.toPath() );
			  }
			  if ( !targetDirectory.Directory )
			  {
					throw new System.ArgumentException( "Move target must be a directory, not " + targetDirectory );
			  }

			  File target = new File( targetDirectory, file.Name );
			  CopyFile( file, target );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void renameFile(java.io.File srcFile, java.io.File renameToFile, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
		 public static void RenameFile( File srcFile, File renameToFile, params CopyOption[] copyOptions )
		 {
			  Files.move( srcFile.toPath(), renameToFile.toPath(), copyOptions );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void truncateFile(java.nio.channels.SeekableByteChannel fileChannel, long position) throws java.io.IOException
		 public static void TruncateFile( SeekableByteChannel fileChannel, long position )
		 {
			  WindowsSafeIOOperation( () => fileChannel.truncate(position) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void truncateFile(java.io.File file, long position) throws java.io.IOException
		 public static void TruncateFile( File file, long position )
		 {
			  using ( RandomAccessFile access = new RandomAccessFile( file, "rw" ) )
			  {
					TruncateFile( access.Channel, position );
			  }
		 }

		 /*
		  * See http://bugs.java.com/bugdatabase/view_bug.do?bug_id=4715154.
		  */
		 private static void WaitAndThenTriggerGC()
		 {
			  try
			  {
					Thread.Sleep( 500 );
			  }
			  catch ( InterruptedException )
			  {
					Thread.interrupted();
			  } // ok
			  System.GC.Collect();
		 }

		 public static string FixSeparatorsInPath( string path )
		 {
			  string fileSeparator = System.getProperty( "file.separator" );
			  if ( "\\".Equals( fileSeparator ) )
			  {
					path = path.Replace( '/', '\\' );
			  }
			  else if ( "/".Equals( fileSeparator ) )
			  {
					path = path.Replace( '\\', '/' );
			  }
			  return path;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void copyFile(java.io.File srcFile, java.io.File dstFile) throws java.io.IOException
		 public static void CopyFile( File srcFile, File dstFile )
		 {
			  //noinspection ResultOfMethodCallIgnored
			  dstFile.ParentFile.mkdirs();
			  Files.copy( srcFile.toPath(), dstFile.toPath(), StandardCopyOption.REPLACE_EXISTING );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory) throws java.io.IOException
		 public static void CopyRecursively( File fromDirectory, File toDirectory )
		 {
			  CopyRecursively( fromDirectory, toDirectory, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void copyRecursively(java.io.File fromDirectory, java.io.File toDirectory, java.io.FileFilter filter) throws java.io.IOException
		 public static void CopyRecursively( File fromDirectory, File toDirectory, FileFilter filter )
		 {
			  File[] files = fromDirectory.listFiles( filter );
			  if ( files != null )
			  {
					foreach ( File fromFile in files )
					{
						 File toFile = new File( toDirectory, fromFile.Name );
						 if ( fromFile.Directory )
						 {
							  Files.createDirectories( toFile.toPath() );
							  CopyRecursively( fromFile, toFile, filter );
						 }
						 else
						 {
							  CopyFile( fromFile, toFile );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeToFile(java.io.File target, String text, boolean append) throws java.io.IOException
		 public static void WriteToFile( File target, string text, bool append )
		 {
			  if ( !target.exists() )
			  {
					Files.createDirectories( target.ParentFile.toPath() );
					//noinspection ResultOfMethodCallIgnored
					target.createNewFile();
			  }

			  using ( Writer @out = new StreamWriter( new FileStream( target, append ), Encoding.UTF8 ) )
			  {
					@out.write( text );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.PrintWriter newFilePrintWriter(java.io.File file, java.nio.charset.Charset charset) throws java.io.FileNotFoundException
		 public static PrintWriter NewFilePrintWriter( File file, Charset charset )
		 {
			  return new PrintWriter( new StreamWriter( new FileStream( file, true ), charset ) );
		 }

		 public static File Path( string root, params string[] path )
		 {
			  return path( new File( root ), path );
		 }

		 public static File Path( File root, params string[] path )
		 {
			  foreach ( string part in path )
			  {
					root = new File( root, part );
			  }
			  return root;
		 }

		 /// <summary>
		 /// Attempts to discern if the given path is mounted on a device that can likely sustain a very high IO throughput.
		 /// <para>
		 /// A high IO device is expected to have negligible seek time, if any, and be able to service multiple IO requests
		 /// in parallel.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="pathOnDevice"> Any path, hypothetical or real, that once fully resolved, would exist on a storage device
		 /// that either supports high IO, or not. </param>
		 /// <param name="defaultHunch"> The default hunch for whether the device supports high IO or not. This will be returned if
		 /// we otherwise have no clue about the nature of the storage device. </param>
		 /// <returns> Our best-effort estimate for whether or not this device supports a high IO workload. </returns>
		 public static bool HighIODevice( Path pathOnDevice, bool defaultHunch )
		 {
			  // This method has been manually tested and correctly identifies the high IO volumes on our test servers.
			  if ( SystemUtils.IS_OS_MAC )
			  {
					// Most macs have flash storage, so let's assume true for them.
					return true;
			  }

			  if ( SystemUtils.IS_OS_LINUX )
			  {
					try
					{
						 FileStore fileStore = Files.getFileStore( pathOnDevice );
						 string name = fileStore.name();
						 if ( name.Equals( "tmpfs" ) || name.Equals( "hugetlbfs" ) )
						 {
							  // This is a purely in-memory device. It doesn't get faster than this.
							  return true;
						 }

						 if ( name.StartsWith( "/dev/nvme", StringComparison.Ordinal ) )
						 {
							  // This is probably an NVMe device. Anything on that protocol is most likely very fast.
							  return true;
						 }

						 Path device = Paths.get( name ).toRealPath(); // Use toRealPath to resolve any symlinks.
						 Path deviceName = device.getName( device.NameCount - 1 );

						 Path rotational = RotationalPathFor( deviceName );
						 if ( Files.exists( rotational ) )
						 {
							  return ReadFirstCharacter( rotational ) == '0';
						 }
						 else
						 {
							  string namePart = deviceName.ToString();
							  int len = namePart.Length;
							  while ( char.IsDigit( namePart[len - 1] ) )
							  {
									len--;
							  }
							  deviceName = Paths.get( namePart.Substring( 0, len ) );
							  rotational = RotationalPathFor( deviceName );
							  if ( Files.exists( rotational ) )
							  {
									return ReadFirstCharacter( rotational ) == '0';
							  }
						 }
					}
					catch ( Exception )
					{
						 return defaultHunch;
					}
			  }

			  return defaultHunch;
		 }

		 private static Path RotationalPathFor( Path deviceName )
		 {
			  return Paths.get( "/sys/block" ).resolve( deviceName ).resolve( "queue" ).resolve( "rotational" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static int readFirstCharacter(java.nio.file.Path file) throws java.io.IOException
		 private static int ReadFirstCharacter( Path file )
		 {
			  using ( Stream @in = Files.newInputStream( file, StandardOpenOption.READ ) )
			  {
					return @in.Read();
			  }
		 }

		 /// <summary>
		 /// Useful when you want to move a file from one directory to another by renaming the file
		 /// and keep eventual sub directories. Example:
		 /// <para>
		 /// You want to move file /a/b1/c/d/file from /a/b1 to /a/b2 and keep the sub path /c/d/file.
		 /// <pre>
		 /// <code>fileToMove = new File( "/a/b1/c/d/file" );
		 /// fromDir = new File( "/a/b1" );
		 /// toDir = new File( "/a/b2" );
		 /// fileToMove.rename( pathToFileAfterMove( fromDir, toDir, fileToMove ) );
		 /// // fileToMove.getAbsolutePath() -> /a/b2/c/d/file</code>
		 /// </pre>
		 /// Calls <seealso cref="pathToFileAfterMove(Path, Path, Path)"/> after
		 /// transforming given files to paths by calling <seealso cref="File.toPath()"/>.
		 /// </para>
		 /// <para>
		 /// NOTE: This that this does not perform the move, it only calculates the new file name.
		 /// </para>
		 /// <para>
		 /// Throws <seealso cref="System.ArgumentException"/> is fileToMove is not a sub path to fromDir.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="fromDir"> Current parent directory for fileToMove </param>
		 /// <param name="toDir"> Directory denoting new parent directory for fileToMove after move </param>
		 /// <param name="fileToMove"> File denoting current location for fileToMove </param>
		 /// <returns> <seealso cref="File"/> denoting new abstract path for file after move. </returns>
		 public static File PathToFileAfterMove( File fromDir, File toDir, File fileToMove )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.file.Path fromDirPath = fromDir.toPath();
			  Path fromDirPath = fromDir.toPath();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.file.Path toDirPath = toDir.toPath();
			  Path toDirPath = toDir.toPath();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.file.Path fileToMovePath = fileToMove.toPath();
			  Path fileToMovePath = fileToMove.toPath();
			  return PathToFileAfterMove( fromDirPath, toDirPath, fileToMovePath ).toFile();
		 }

		 /// <summary>
		 /// Resolve toDir against fileToMove relativized against fromDir, resulting in a path denoting the location of
		 /// fileToMove after being moved fromDir toDir.
		 /// <para>
		 /// NOTE: This that this does not perform the move, it only calculates the new file name.
		 /// </para>
		 /// <para>
		 /// Throws <seealso cref="System.ArgumentException"/> is fileToMove is not a sub path to fromDir.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="fromDir"> Path denoting current parent directory for fileToMove </param>
		 /// <param name="toDir"> Path denoting location for fileToMove after move </param>
		 /// <param name="fileToMove"> Path denoting current location for fileToMove </param>
		 /// <returns> <seealso cref="Path"/> denoting new abstract path for file after move. </returns>
		 public static Path PathToFileAfterMove( Path fromDir, Path toDir, Path fileToMove )
		 {
			  // File to move must be true sub path to from dir
			  if ( !fileToMove.startsWith( fromDir ) || fileToMove.Equals( fromDir ) )
			  {
					throw new System.ArgumentException( "File " + fileToMove + " is not a sub path to dir " + fromDir );
			  }

			  return toDir.resolve( fromDir.relativize( fileToMove ) );
		 }

		 /// <summary>
		 /// Count the number of files and directories, contained in the given <seealso cref="Path"/>, which must be a directory. </summary>
		 /// <param name="dir"> The directory whose contents to count. </param>
		 /// <returns> The number of files and directories in the given directory. </returns>
		 /// <exception cref="NotDirectoryException"> If the given <seealso cref="Path"/> is not a directory. This exception is an optionally
		 /// specific exception. <seealso cref="IOException"/> might be thrown instead. </exception>
		 /// <exception cref="IOException"> If the given directory could not be opened for some reason. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static long countFilesInDirectoryPath(java.nio.file.Path dir) throws java.io.IOException
		 public static long CountFilesInDirectoryPath( Path dir )
		 {
			  using ( Stream<Path> listing = Files.list( dir ) )
			  {
					return listing.count();
			  }
		 }

		 public interface Operation
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void perform() throws java.io.IOException;
			  void Perform();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void windowsSafeIOOperation(Operation operation) throws java.io.IOException
		 public static void WindowsSafeIOOperation( Operation operation )
		 {
			  IOException storedIoe = null;
			  for ( int i = 0; i < NUMBER_OF_RETRIES; i++ )
			  {
					try
					{
						 operation.Perform();
						 return;
					}
					catch ( IOException e )
					{
						 storedIoe = e;
						 WaitAndThenTriggerGC();
					}
			  }
			  throw Objects.requireNonNull( storedIoe );
		 }

		 public interface LineListener
		 {
			  void Line( string line );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static LineListener echo(final java.io.PrintStream target)
		 public static LineListener Echo( PrintStream target )
		 {
			  return target.println;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void readTextFile(java.io.File file, LineListener listener) throws java.io.IOException
		 public static void ReadTextFile( File file, LineListener listener )
		 {
			  using ( StreamReader reader = new StreamReader( file ) )
			  {
					string line;
					while ( !string.ReferenceEquals( ( line = reader.ReadLine() ), null ) )
					{
						 listener.Line( line );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String readTextFile(java.io.File file, java.nio.charset.Charset charset) throws java.io.IOException
		 public static string ReadTextFile( File file, Charset charset )
		 {
			  StringBuilder @out = new StringBuilder();
			  foreach ( string s in Files.readAllLines( file.toPath(), charset ) )
			  {
					@out.Append( s ).Append( "\n" );
			  }
			  return @out.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void deleteFile(java.nio.file.Path path) throws java.io.IOException
		 private static void DeleteFile( Path path )
		 {
			  WindowsSafeIOOperation( () => Files.delete(path) );
		 }

		 /// <summary>
		 /// Given a directory and a path under it, return filename of the path
		 /// relative to the directory.
		 /// </summary>
		 /// <param name="baseDir"> The base directory, containing the storeFile </param>
		 /// <param name="storeFile"> The store file path, must be contained under
		 /// <code>baseDir</code> </param>
		 /// <returns> The relative path of <code>storeFile</code> to
		 /// <code>baseDir</code> </returns>
		 /// <exception cref="IOException"> As per <seealso cref="File.getCanonicalPath()"/> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String relativePath(java.io.File baseDir, java.io.File storeFile) throws java.io.IOException
		 public static string RelativePath( File baseDir, File storeFile )
		 {
			  string prefix = baseDir.CanonicalPath;
			  string path = storeFile.CanonicalPath;
			  if ( !path.StartsWith( prefix, StringComparison.Ordinal ) )
			  {
					throw new FileNotFoundException();
			  }
			  path = path.Substring( prefix.Length );
			  if ( path.StartsWith( File.separator, StringComparison.Ordinal ) )
			  {
					return path.Substring( 1 );
			  }
			  return path;
		 }

		 /// <summary>
		 /// Canonical file resolution on windows does not resolve links.
		 /// Real paths on windows can be resolved only using <seealso cref="Path.toRealPath(LinkOption...)"/>, but file should exist in that case.
		 /// We will try to do as much as possible and will try to use <seealso cref="Path.toRealPath(LinkOption...)"/> when file exist and will fallback to only
		 /// use <seealso cref="File.getCanonicalFile()"/> if file does not exist.
		 /// see JDK-8003887 for details </summary>
		 /// <param name="file"> - file to resolve canonical representation </param>
		 /// <returns> canonical file representation. </returns>
		 public static File GetCanonicalFile( File file )
		 {
			  try
			  {
					File fileToResolve = file.exists() ? file.toPath().toRealPath().toFile() : file;
					return fileToResolve.CanonicalFile;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeAll(java.nio.channels.FileChannel channel, ByteBuffer src, long position) throws java.io.IOException
		 public static void WriteAll( FileChannel channel, ByteBuffer src, long position )
		 {
			  long filePosition = position;
			  long expectedEndPosition = filePosition + src.limit() - src.position();
			  int bytesWritten;
			  while ( ( filePosition += bytesWritten = channel.write( src, filePosition ) ) < expectedEndPosition )
			  {
					if ( bytesWritten <= 0 )
					{
						 throw new IOException( "Unable to write to disk, reported bytes written was " + bytesWritten );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeAll(java.nio.channels.FileChannel channel, ByteBuffer src) throws java.io.IOException
		 public static void WriteAll( FileChannel channel, ByteBuffer src )
		 {
			  long bytesToWrite = src.limit() - src.position();
			  int bytesWritten;
			  while ( ( bytesToWrite -= bytesWritten = channel.write( src ) ) > 0 )
			  {
					if ( bytesWritten <= 0 )
					{
						 throw new IOException( "Unable to write to disk, reported bytes written was " + bytesWritten );
					}
			  }
		 }

		 public static OpenOption[] ConvertOpenMode( OpenMode mode )
		 {
			  OpenOption[] options;
			  switch ( mode.innerEnumValue )
			  {
			  case Neo4Net.Io.fs.OpenMode.InnerEnum.READ:
					options = new OpenOption[]{ READ };
					break;
			  case Neo4Net.Io.fs.OpenMode.InnerEnum.READ_WRITE:
					options = new OpenOption[]{ CREATE, READ, WRITE };
					break;
			  case Neo4Net.Io.fs.OpenMode.InnerEnum.SYNC:
					options = new OpenOption[]{ CREATE, READ, WRITE, SYNC };
					break;
			  case Neo4Net.Io.fs.OpenMode.InnerEnum.DSYNC:
					options = new OpenOption[]{ CREATE, READ, WRITE, DSYNC };
					break;
			  default:
					throw new System.ArgumentException( "Unsupported mode: " + mode );
			  }
			  return options;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.nio.channels.FileChannel open(java.nio.file.Path path, OpenMode openMode) throws java.io.IOException
		 public static FileChannel Open( Path path, OpenMode openMode )
		 {
			  return FileChannel.open( path, ConvertOpenMode( openMode ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.InputStream openAsInputStream(java.nio.file.Path path) throws java.io.IOException
		 public static Stream OpenAsInputStream( Path path )
		 {
			  return Files.newInputStream( path, READ );
		 }

		 /// <summary>
		 /// Check if directory is empty.
		 /// </summary>
		 /// <param name="directory"> - directory to check </param>
		 /// <returns> false if directory exists and empty, true otherwise. </returns>
		 /// <exception cref="IllegalArgumentException"> if specified directory represent a file </exception>
		 /// <exception cref="IOException"> if some problem encountered during reading directory content </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean isEmptyDirectory(java.io.File directory) throws java.io.IOException
		 public static bool IsEmptyDirectory( File directory )
		 {
			  if ( directory.exists() )
			  {
					if ( !directory.Directory )
					{
						 throw new System.ArgumentException( "Expected directory, but was file: " + directory );
					}
					else
					{
						 using ( DirectoryStream<Path> directoryStream = Files.newDirectoryStream( directory.toPath() ) )
						 {
							  return !directoryStream.GetEnumerator().hasNext();
						 }
					}
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.OutputStream openAsOutputStream(java.nio.file.Path path, boolean append) throws java.io.IOException
		 public static Stream OpenAsOutputStream( Path path, bool append )
		 {
			  OpenOption[] options;
			  if ( append )
			  {
					options = new OpenOption[]{ CREATE, WRITE, APPEND };
			  }
			  else
			  {
					options = new OpenOption[]{ CREATE, WRITE };
			  }
			  return Files.newOutputStream( path, options );
		 }

		 /// <summary>
		 /// Calculates the size of a given directory or file given the provided abstract filesystem.
		 /// </summary>
		 /// <param name="fs"> the filesystem abstraction to use </param>
		 /// <param name="file"> to the file or directory. </param>
		 /// <returns> the size, in bytes, of the file or the total size of the content in the directory, including
		 /// subdirectories. </returns>
		 public static long Size( FileSystemAbstraction fs, File file )
		 {
			  if ( fs.IsDirectory( file ) )
			  {
					long size = 0L;
					File[] files = fs.ListFiles( file );
					if ( files == null )
					{
						 return 0L;
					}
					foreach ( File child in files )
					{
						 size += size( fs, child );
					}
					return size;
			  }
			  else
			  {
					return fs.GetFileSize( file );
			  }
		 }
	}

}