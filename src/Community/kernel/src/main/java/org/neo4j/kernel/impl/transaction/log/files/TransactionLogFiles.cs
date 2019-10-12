using System;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.transaction.log.files
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderReader.readLogHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeaderWriter.writeLogHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogVersions.CURRENT_LOG_VERSION;

	/// <summary>
	/// Used to figure out what logical log file to open when the database
	/// starts up.
	/// </summary>
	public class TransactionLogFiles : LifecycleAdapter, LogFiles
	{
		 public const string DEFAULT_NAME = "neostore.transaction.db";
		 public static readonly FilenameFilter DefaultFilenameFilter = TransactionLogFilesHelper.DefaultFilenameFilter;
		 private static readonly File[] _emptyFilesArray = new File[] {};

		 private readonly TransactionLogFilesContext _logFilesContext;
		 private readonly TransactionLogFileInformation _logFileInformation;

		 private readonly LogHeaderCache _logHeaderCache;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly LogFileCreationMonitor _monitor;
		 private readonly TransactionLogFilesHelper _fileHelper;
		 private readonly TransactionLogFile _logFile;
		 private readonly File _logsDirectory;

		 internal TransactionLogFiles( File logsDirectory, string name, TransactionLogFilesContext context )
		 {
			  this._logFilesContext = context;
			  this._logsDirectory = logsDirectory;
			  this._fileHelper = new TransactionLogFilesHelper( logsDirectory, name );
			  this._fileSystem = context.FileSystem;
			  this._monitor = context.LogFileCreationMonitor;
			  this._logHeaderCache = new LogHeaderCache( 1000 );
			  this._logFileInformation = new TransactionLogFileInformation( this, _logHeaderCache, context );
			  this._logFile = new TransactionLogFile( this, context );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws java.io.IOException
		 public override void Init()
		 {
			  _logFile.init();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws java.io.IOException
		 public override void Start()
		 {
			  _logFile.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws java.io.IOException
		 public override void Shutdown()
		 {
			  _logFile.shutdown();
		 }

		 public override long GetLogVersion( File historyLogFile )
		 {
			  return getLogVersion( historyLogFile.Name );
		 }

		 public override long GetLogVersion( string historyLogFilename )
		 {
			  return _fileHelper.getLogVersion( historyLogFilename );
		 }

		 public override File[] LogFiles()
		 {
			  File[] files = _fileSystem.listFiles( _fileHelper.ParentDirectory, _fileHelper.LogFilenameFilter );
			  if ( files == null )
			  {
					return _emptyFilesArray;
			  }
			  return files;
		 }

		 public override bool IsLogFile( File file )
		 {
			  return _fileHelper.LogFilenameFilter.accept( null, file.Name );
		 }

		 public override File LogFilesDirectory()
		 {
			  return _logsDirectory;
		 }

		 public override File GetLogFileForVersion( long version )
		 {
			  return _fileHelper.getLogFileForVersion( version );
		 }

		 public virtual File HighestLogFile
		 {
			 get
			 {
				  return GetLogFileForVersion( HighestLogVersion );
			 }
		 }

		 public override bool VersionExists( long version )
		 {
			  return _fileSystem.fileExists( GetLogFileForVersion( version ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.transaction.log.entry.LogHeader extractHeader(long version) throws java.io.IOException
		 public override LogHeader ExtractHeader( long version )
		 {
			  return readLogHeader( _fileSystem, GetLogFileForVersion( version ) );
		 }

		 public override bool HasAnyEntries( long version )
		 {
			  return _fileSystem.getFileSize( GetLogFileForVersion( version ) ) > LOG_HEADER_SIZE;
		 }

		 public virtual long HighestLogVersion
		 {
			 get
			 {
				  RangeLogVersionVisitor visitor = new RangeLogVersionVisitor();
				  Accept( visitor );
				  return visitor.Highest;
			 }
		 }

		 public virtual long LowestLogVersion
		 {
			 get
			 {
				  RangeLogVersionVisitor visitor = new RangeLogVersionVisitor();
				  Accept( visitor );
				  return visitor.Lowest;
			 }
		 }

		 public override void Accept( LogVersionVisitor visitor )
		 {
			  foreach ( File file in LogFiles() )
			  {
					visitor.Visit( file, GetLogVersion( file ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel openForVersion(long version) throws java.io.IOException
		 public override PhysicalLogVersionedStoreChannel OpenForVersion( long version )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File fileToOpen = getLogFileForVersion(version);
			  File fileToOpen = GetLogFileForVersion( version );

			  if ( !VersionExists( version ) )
			  {
					throw new FileNotFoundException( format( "File does not exist [%s]", fileToOpen.CanonicalPath ) );
			  }

			  StoreChannel rawChannel = null;
			  try
			  {
					rawChannel = OpenLogFileChannel( fileToOpen, OpenMode.READ );
					ByteBuffer buffer = ByteBuffer.allocate( LOG_HEADER_SIZE );
					LogHeader header = readLogHeader( buffer, rawChannel, true, fileToOpen );
					if ( ( header == null ) || ( header.LogVersion != version ) )
					{
						 throw new System.InvalidOperationException( format( "Unexpected log file header. Expected header version: %d, actual header: %s", version, header != null ? header.ToString() : "null header." ) );
					}
					return new PhysicalLogVersionedStoreChannel( rawChannel, version, header.LogFormatVersion );
			  }
			  catch ( FileNotFoundException cause )
			  {
					throw ( FileNotFoundException ) ( new FileNotFoundException( format( "File could not be opened [%s]", fileToOpen.CanonicalPath ) ) ).initCause( cause );
			  }
			  catch ( Exception unexpectedError )
			  {
					if ( rawChannel != null )
					{
						 // If we managed to open the file before failing, then close the channel
						 try
						 {
							  rawChannel.close();
						 }
						 catch ( IOException e )
						 {
							  unexpectedError.addSuppressed( e );
						 }
					}
					throw unexpectedError;
			  }
		 }

		 /// <summary>
		 /// Creates a new channel for the specified version, creating the backing file if it doesn't already exist.
		 /// If the file exists then the header is verified to be of correct version. Having an existing file there
		 /// could happen after a previous crash in the middle of rotation, where the new file was created,
		 /// but the incremented log version changed hadn't made it to persistent storage.
		 /// </summary>
		 /// <param name="forVersion"> log version for the file/channel to create. </param>
		 /// <param name="mode"> mode in which open log file </param>
		 /// <param name="lastTransactionIdSupplier"> supplier of last transaction id that was written into previous log file </param>
		 /// <returns> <seealso cref="PhysicalLogVersionedStoreChannel"/> for newly created/opened log file. </returns>
		 /// <exception cref="IOException"> if there's any I/O related error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel createLogChannelForVersion(long forVersion, org.neo4j.io.fs.OpenMode mode, System.Func<long> lastTransactionIdSupplier) throws java.io.IOException
		 internal virtual PhysicalLogVersionedStoreChannel CreateLogChannelForVersion( long forVersion, OpenMode mode, System.Func<long> lastTransactionIdSupplier )
		 {
			  File toOpen = GetLogFileForVersion( forVersion );
			  StoreChannel storeChannel = _fileSystem.open( toOpen, mode );
			  ByteBuffer headerBuffer = ByteBuffer.allocate( LOG_HEADER_SIZE );
			  LogHeader header = readLogHeader( headerBuffer, storeChannel, false, toOpen );
			  if ( header == null )
			  {
					// Either the header is not there in full or the file was new. Don't care
					long lastTxId = lastTransactionIdSupplier();
					writeLogHeader( headerBuffer, forVersion, lastTxId );
					_logHeaderCache.putHeader( forVersion, lastTxId );
					storeChannel.WriteAll( headerBuffer );
					_monitor.created( toOpen, forVersion, lastTxId );
			  }
			  sbyte formatVersion = header == null ? CURRENT_LOG_VERSION : header.LogFormatVersion;
			  return new PhysicalLogVersionedStoreChannel( storeChannel, forVersion, formatVersion );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(LogHeaderVisitor visitor) throws java.io.IOException
		 public override void Accept( LogHeaderVisitor visitor )
		 {
			  // Start from the where we're currently at and go backwards in time (versions)
			  long logVersion = HighestLogVersion;
			  long highTransactionId = _logFilesContext.LastCommittedTransactionId;
			  while ( VersionExists( logVersion ) )
			  {
					long? previousLogLastTxId = _logHeaderCache.getLogHeader( logVersion );
					if ( previousLogLastTxId == null )
					{
						 LogHeader header = readLogHeader( _fileSystem, GetLogFileForVersion( logVersion ), false );
						 if ( header != null )
						 {
							  Debug.Assert( logVersion == header.LogVersion );
							  _logHeaderCache.putHeader( header.LogVersion, header.LastCommittedTxId );
							  previousLogLastTxId = header.LastCommittedTxId;
						 }
					}

					if ( previousLogLastTxId != null )
					{
						 long lowTransactionId = previousLogLastTxId + 1;
						 LogPosition position = LogPosition.start( logVersion );
						 if ( !visitor.Visit( position, lowTransactionId, highTransactionId ) )
						 {
							  break;
						 }
						 highTransactionId = previousLogLastTxId.Value;
					}
					logVersion--;
			  }
		 }

		 public virtual LogFile LogFile
		 {
			 get
			 {
				  return _logFile;
			 }
		 }

		 public virtual TransactionLogFileInformation LogFileInformation
		 {
			 get
			 {
				  return _logFileInformation;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.io.fs.StoreChannel openLogFileChannel(java.io.File file, org.neo4j.io.fs.OpenMode mode) throws java.io.IOException
		 private StoreChannel OpenLogFileChannel( File file, OpenMode mode )
		 {
			  return _fileSystem.open( file, mode );
		 }

		 private class RangeLogVersionVisitor : LogVersionVisitor
		 {
			  internal long Lowest = -1;
			  internal long Highest = -1;

			  public override void Visit( File file, long logVersion )
			  {
					Highest = max( Highest, logVersion );
					Lowest = Lowest == -1 ? logVersion : min( Lowest, logVersion );
			  }
		 }
	}

}