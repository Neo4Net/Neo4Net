using System;
using System.Collections.Generic;
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
namespace Neo4Net.Logging
{

	using NullOutputStream = Neo4Net.Io.NullOutputStream;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.file.Files.createOrOpenAsOutputStream;

	/// <summary>
	/// A <seealso cref="Supplier"/> of <seealso cref="System.IO.Stream_Output"/>s backed by on-disk files, which
	/// are rotated and archived when a specified size is reached. The <seealso cref="get()"/> method
	/// will always return an OutputStream to the current output file without directly performing
	/// any IO or blocking, and, when necessary, will trigger rotation via the <seealso cref="Executor"/>
	/// supplied during construction.
	/// </summary>
	public class RotatingFileOutputStreamSupplier : System.Func<Stream>, System.IDisposable
	{
		 /// <summary>
		 /// A listener for the rotation process
		 /// </summary>
		 public class RotationListener
		 {
			  public virtual void OutputFileCreated( Stream @out )
			  {
			  }

			  public virtual void RotationCompleted( Stream @out )
			  {
			  }

			  public virtual void RotationError( Exception e, Stream @out )
			  {
			  }
		 }

		 private static readonly System.Func<long> _defaultCurrentTimeSupplier = System.currentTimeMillis;

		 // Used only in case no new output file can be created during rotation
		 private static readonly Stream _nullStream = NullOutputStream.NULL_OUTPUT_STREAM;

		 private readonly System.Func<long> _currentTimeSupplier;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly File _outputFile;
		 private readonly long _rotationThresholdBytes;
		 private readonly long _rotationDelay;
		 private readonly int _maxArchives;
		 private readonly RotationListener _rotationListener;
		 private readonly Executor _rotationExecutor;
		 private readonly ReadWriteLock _logFileLock = new ReentrantReadWriteLock( true );
		 private readonly Stream _streamWrapper;
		 private readonly AtomicBoolean _closed = new AtomicBoolean( false );
		 private readonly AtomicBoolean _rotating = new AtomicBoolean( false );
		 private readonly AtomicLong _earliestRotationTimeRef = new AtomicLong( 0 );
		 private Stream _outRef = _nullStream;

		 /// <param name="fileSystem"> The filesystem to use </param>
		 /// <param name="outputFile"> The file that the latest <seealso cref="System.IO.Stream_Output"/> should output to </param>
		 /// <param name="rotationThresholdBytes"> The size above which the file should be rotated </param>
		 /// <param name="rotationDelay"> The minimum time (ms) after last rotation before the file may be rotated again </param>
		 /// <param name="maxArchives"> The maximum number of archived output files to keep </param>
		 /// <param name="rotationExecutor"> An <seealso cref="Executor"/> for performing the rotation </param>
		 /// <exception cref="IOException"> If the output file cannot be created </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RotatingFileOutputStreamSupplier(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File outputFile, long rotationThresholdBytes, long rotationDelay, int maxArchives, java.util.concurrent.Executor rotationExecutor) throws java.io.IOException
		 public RotatingFileOutputStreamSupplier( FileSystemAbstraction fileSystem, File outputFile, long rotationThresholdBytes, long rotationDelay, int maxArchives, Executor rotationExecutor ) : this( fileSystem, outputFile, rotationThresholdBytes, rotationDelay, maxArchives, rotationExecutor, new RotationListener() )
		 {
		 }

		 /// <param name="fileSystem"> The filesystem to use </param>
		 /// <param name="outputFile"> The file that the latest <seealso cref="System.IO.Stream_Output"/> should output to </param>
		 /// <param name="rotationThresholdBytes"> The size above which the file should be rotated </param>
		 /// <param name="rotationDelay"> The minimum time (ms) after last rotation before the file may be rotated again </param>
		 /// <param name="maxArchives"> The maximum number of archived output files to keep </param>
		 /// <param name="rotationExecutor"> An <seealso cref="Executor"/> for performing the rotation </param>
		 /// <param name="rotationListener"> A <seealso cref="org.neo4j.logging.RotatingFileOutputStreamSupplier.RotationListener"/> that can
		 /// observe the rotation process and be notified of errors </param>
		 /// <exception cref="IOException"> If the output file cannot be created </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RotatingFileOutputStreamSupplier(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File outputFile, long rotationThresholdBytes, long rotationDelay, int maxArchives, java.util.concurrent.Executor rotationExecutor, RotationListener rotationListener) throws java.io.IOException
		 public RotatingFileOutputStreamSupplier( FileSystemAbstraction fileSystem, File outputFile, long rotationThresholdBytes, long rotationDelay, int maxArchives, Executor rotationExecutor, RotationListener rotationListener ) : this( _defaultCurrentTimeSupplier, fileSystem, outputFile, rotationThresholdBytes, rotationDelay, maxArchives, rotationExecutor, rotationListener )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: RotatingFileOutputStreamSupplier(System.Func<long> currentTimeSupplier, org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File outputFile, long rotationThresholdBytes, long rotationDelay, int maxArchives, java.util.concurrent.Executor rotationExecutor, RotationListener rotationListener) throws java.io.IOException
		 internal RotatingFileOutputStreamSupplier( System.Func<long> currentTimeSupplier, FileSystemAbstraction fileSystem, File outputFile, long rotationThresholdBytes, long rotationDelay, int maxArchives, Executor rotationExecutor, RotationListener rotationListener )
		 {
			  this._currentTimeSupplier = currentTimeSupplier;
			  this._fileSystem = fileSystem;
			  this._outputFile = outputFile;
			  this._rotationThresholdBytes = rotationThresholdBytes;
			  this._rotationDelay = rotationDelay;
			  this._maxArchives = maxArchives;
			  this._rotationListener = rotationListener;
			  this._rotationExecutor = rotationExecutor;
			  this._outRef = OpenOutputFile();
			  // Wrap the actual reference to prevent race conditions during log rotation
			  this._streamWrapper = new OutputStreamAnonymousInnerClass( this );
		 }

		 private class OutputStreamAnonymousInnerClass : Stream
		 {
			 private readonly RotatingFileOutputStreamSupplier _outerInstance;

			 public OutputStreamAnonymousInnerClass( RotatingFileOutputStreamSupplier outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(int i) throws java.io.IOException
			 public override void write( int i )
			 {
				  _outerInstance.logFileLock.readLock().@lock();
				  try
				  {
						_outerInstance.outRef.WriteByte( i );
				  }
				  finally
				  {
						_outerInstance.logFileLock.readLock().unlock();
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] bytes) throws java.io.IOException
			 public override void write( sbyte[] bytes )
			 {
				  _outerInstance.logFileLock.readLock().@lock();
				  try
				  {
						_outerInstance.outRef.Write( bytes, 0, bytes.Length );
				  }
				  finally
				  {
						_outerInstance.logFileLock.readLock().unlock();
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] bytes, int off, int len) throws java.io.IOException
			 public override void write( sbyte[] bytes, int off, int len )
			 {
				  _outerInstance.logFileLock.readLock().@lock();
				  try
				  {
						_outerInstance.outRef.Write( bytes, off, len );
				  }
				  finally
				  {
						_outerInstance.logFileLock.readLock().unlock();
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
			 public override void flush()
			 {
				  _outerInstance.logFileLock.readLock().@lock();
				  try
				  {
						_outerInstance.outRef.Flush();
				  }
				  finally
				  {
						_outerInstance.logFileLock.readLock().unlock();
				  }
			 }
		 }

		 /// <returns> A stream outputting to the latest output file </returns>
		 public override Stream Get()
		 {
			  if ( !_closed.get() && !_rotating.get() )
			  {
					// In case output file doesn't exist, call rotate so that it gets created
					if ( RotationDelayExceeded() && RotationThresholdExceeded() || !_fileSystem.fileExists(_outputFile) )
					{
						 Rotate();
					}
			  }
			  return this._streamWrapper;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _logFileLock.writeLock().@lock();
			  try
			  {
					_closed.set( true );
					_outRef.Close();
			  }
			  finally
			  {
					_outRef = _nullStream;
					_logFileLock.writeLock().unlock();
			  }
		 }

		 private bool RotationThresholdExceeded()
		 {
			  return _fileSystem.fileExists( _outputFile ) && _rotationThresholdBytes > 0 && _fileSystem.getFileSize( _outputFile ) >= _rotationThresholdBytes;
		 }

		 private bool RotationDelayExceeded()
		 {
			  return _earliestRotationTimeRef.get() <= _currentTimeSupplier.AsLong;
		 }

		 internal virtual void Rotate()
		 {
			  if ( _rotating.getAndSet( true ) )
			  {
					// Already rotating
					return;
			  }

			  MemoryStream bufferingOutputStream = new MemoryStream();
			  ThreadStart runnable = () =>
			  {
				_logFileLock.writeLock().@lock();
				try
				{
					 try
					 {
						  // Must close file prior to doing any operations on it or else it won't work on Windows
						  try
						  {
								_outRef.Flush();
								_outRef.Close();
								_outRef = _nullStream;
						  }
						  catch ( Exception e )
						  {
								_rotationListener.rotationError( e, bufferingOutputStream );
								return;
						  }

						  try
						  {
								if ( _fileSystem.fileExists( _outputFile ) )
								{
									 ShiftArchivedOutputFiles();
									 _fileSystem.renameFile( _outputFile, ArchivedOutputFile( _outputFile, 1 ) );
								}
						  }
						  catch ( Exception e )
						  {
								_rotationListener.rotationError( e, bufferingOutputStream );
								return;
						  }
					 }
					 finally
					 {
						  try
						  {
								if ( !_closed.get() && _outRef.Equals(_nullStream) )
								{
									 _outRef = OpenOutputFile();
									 _rotationListener.outputFileCreated( bufferingOutputStream );
								}
						  }
						  catch ( IOException e )
						  {
								Console.Error.WriteLine( "Failed to open log file after log rotation: " + e.Message );
								_rotationListener.rotationError( e, bufferingOutputStream );
						  }
					 }

					 if ( _rotationDelay > 0 )
					 {
						  _earliestRotationTimeRef.set( _currentTimeSupplier.AsLong + _rotationDelay );
					 }
					 _rotationListener.rotationCompleted( bufferingOutputStream );
				}
				finally
				{
					 _rotating.set( false );
					 try
					 {
						  bufferingOutputStream.writeTo( _streamWrapper );
					 }
					 catch ( IOException e )
					 {
						  _rotationListener.rotationError( e, _streamWrapper );
					 }
					 _logFileLock.writeLock().unlock();
				}
			  };

			  try
			  {
					_rotationExecutor.execute( runnable );
			  }
			  catch ( Exception e )
			  {
					_rotationListener.rotationError( e, _streamWrapper );
					_rotating.set( false );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.OutputStream openOutputFile() throws java.io.IOException
		 private Stream OpenOutputFile()
		 {
			  return createOrOpenAsOutputStream( _fileSystem, _outputFile, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shiftArchivedOutputFiles() throws java.io.IOException
		 private void ShiftArchivedOutputFiles()
		 {
			  for ( int i = LastArchivedOutputFileNumber( _fileSystem, _outputFile ); i > 0; --i )
			  {
					File archive = ArchivedOutputFile( _outputFile, i );
					if ( i >= _maxArchives )
					{
						 _fileSystem.deleteFile( archive );
					}
					else
					{
						 _fileSystem.renameFile( archive, ArchivedOutputFile( _outputFile, i + 1 ) );
					}
			  }
		 }

		 private static int LastArchivedOutputFileNumber( FileSystemAbstraction fileSystem, File outputFile )
		 {
			  int i = 1;
			  while ( fileSystem.FileExists( ArchivedOutputFile( outputFile, i ) ) )
			  {
					i++;
			  }
			  return i - 1;
		 }

		 private static File ArchivedOutputFile( File outputFile, int archiveNumber )
		 {
			  return new File( string.Format( "{0}.{1:D}", outputFile.Path, archiveNumber ) );
		 }

		 /// <summary>
		 /// Exposes the algorithm for collecting existing rotated log files.
		 /// </summary>
		 public static IList<File> GetAllArchives( FileSystemAbstraction fileSystem, File outputFile )
		 {
			  List<File> ret = new List<File>();
			  int i = 1;
			  while ( true )
			  {
					File file = ArchivedOutputFile( outputFile, i );
					if ( !fileSystem.FileExists( file ) )
					{
						 break;
					}
					ret.Add( file );
					i++;
			  }
			  return ret;
		 }
	}

}