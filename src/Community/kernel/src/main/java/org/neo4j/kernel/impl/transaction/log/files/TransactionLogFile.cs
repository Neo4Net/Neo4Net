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
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// <seealso cref="LogFile"/> backed by one or more files in a <seealso cref="FileSystemAbstraction"/>.
	/// </summary>
	internal class TransactionLogFile : LifecycleAdapter, LogFile
	{
		 private readonly AtomicLong _rotateAtSize;
		 private readonly TransactionLogFiles _logFiles;
		 private readonly TransactionLogFilesContext _context;
		 private readonly LogVersionBridge _readerLogVersionBridge;
		 private PositionAwarePhysicalFlushableChannel _writer;
		 private LogVersionRepository _logVersionRepository;

		 private volatile PhysicalLogVersionedStoreChannel _channel;

		 internal TransactionLogFile( TransactionLogFiles logFiles, TransactionLogFilesContext context )
		 {
			  this._rotateAtSize = context.RotationThreshold;
			  this._context = context;
			  this._logFiles = logFiles;
			  this._readerLogVersionBridge = new ReaderLogVersionBridge( logFiles );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws java.io.IOException
		 public override void Init()
		 {
			  _logVersionRepository = _context.LogVersionRepository;
			  // Make sure at least a bare bones log file is available before recovery
			  long lastLogVersionUsed = this._logVersionRepository.CurrentLogVersion;
			  _channel = _logFiles.createLogChannelForVersion( lastLogVersionUsed, OpenMode.READ_WRITE, _context.getLastCommittedTransactionId );
			  _channel.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws java.io.IOException
		 public override void Start()
		 {
			  // Recovery has taken place before this, so the log file has been truncated to last known good tx
			  // Just read header and move to the end
			  long lastLogVersionUsed = _logVersionRepository.CurrentLogVersion;
			  _channel = _logFiles.createLogChannelForVersion( lastLogVersionUsed, OpenMode.READ_WRITE, _context.getLastCommittedTransactionId );
			  // Move to the end
			  _channel.position( _channel.size() );
			  _writer = new PositionAwarePhysicalFlushableChannel( _channel );
		 }

		 // In order to be able to write into a logfile after life.stop during shutdown sequence
		 // we will close channel and writer only during shutdown phase when all pending changes (like last
		 // checkpoint) are already in
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws java.io.IOException
		 public override void Shutdown()
		 {
			  if ( _writer != null )
			  {
					_writer.Dispose();
			  }
			  if ( _channel != null )
			  {
					_channel.close();
			  }
		 }

		 public override bool RotationNeeded()
		 {
			  /*
			   * Whereas channel.size() should be fine, we're safer calling position() due to possibility
			   * of this file being memory mapped or whatever.
			   */
			  return _channel.position() >= _rotateAtSize.get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void rotate() throws java.io.IOException
		 public override void Rotate()
		 {
			 lock ( this )
			 {
				  _channel = Rotate( _channel );
				  _writer.Channel = _channel;
			 }
		 }

		 /// <summary>
		 /// Rotates the current log file, continuing into next (version) log file.
		 /// This method must be recovery safe, which means a crash at any point should be recoverable.
		 /// Concurrent readers must also be able to parry for concurrent rotation.
		 /// Concurrent writes will not be an issue since rotation and writing contends on the same monitor.
		 /// 
		 /// Steps during rotation are:
		 /// <ol>
		 /// <li>1: Increment log version, <seealso cref="LogVersionRepository.incrementAndGetVersion()"/> (also flushes the store)</li>
		 /// <li>2: Flush current log</li>
		 /// <li>3: Create new log file</li>
		 /// <li>4: Write header</li>
		 /// </ol>
		 /// 
		 /// Recovery: what happens if crash between:
		 /// <ol>
		 /// <li>1-2: New log version has been set, starting the writer will create the new log file idempotently.
		 /// At this point there may have been half-written transactions in the previous log version,
		 /// although they haven't been considered committed and so they will be truncated from log during recovery</li>
		 /// <li>2-3: New log version has been set, starting the writer will create the new log file idempotently.
		 /// At this point there may be complete transactions in the previous log version which may not have been
		 /// acknowledged to be committed back to the user, but will be considered committed anyway.</li>
		 /// <li>3-4: New log version has been set, starting the writer will see that the new file exists and
		 /// will be forgiving when trying to read the header of it, so that if it isn't complete a fresh
		 /// header will be set.</li>
		 /// </ol>
		 /// 
		 /// Reading: what happens when rotation is between:
		 /// <ol>
		 /// <li>1-2: Reader bridge will see that there's a new version (when asking <seealso cref="LogVersionRepository"/>
		 /// and try to open it. The log file doesn't exist yet though. The bridge can parry for this by catching
		 /// <seealso cref="FileNotFoundException"/> and tell the reader that the stream has ended</li>
		 /// <li>2-3: Same as (1-2)</li>
		 /// <li>3-4: Here the new log file exists, but the header may not be fully written yet.
		 /// the reader will fail when trying to read the header since it's reading it strictly and bridge
		 /// catches that exception, treating it the same as if the file didn't exist.</li>
		 /// </ol>
		 /// </summary>
		 /// <param name="currentLog"> current <seealso cref="LogVersionedStoreChannel channel"/> to flush and close. </param>
		 /// <returns> the channel of the newly opened/created log file. </returns>
		 /// <exception cref="IOException"> if an error regarding closing or opening log files occur. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel rotate(org.neo4j.kernel.impl.transaction.log.LogVersionedStoreChannel currentLog) throws java.io.IOException
		 private PhysicalLogVersionedStoreChannel Rotate( LogVersionedStoreChannel currentLog )
		 {
			  /*
			   * The store is now flushed. If we fail now the recovery code will open the
			   * current log file and replay everything. That's unnecessary but totally ok.
			   */
			  long newLogVersion = _logVersionRepository.incrementAndGetVersion();
			  /*
			   * Rotation can happen at any point, although not concurrently with an append,
			   * although an append may have (most likely actually) left at least some bytes left
			   * in the buffer for future flushing. Flushing that buffer now makes the last appended
			   * transaction complete in the log we're rotating away. Awesome.
			   */
			  _writer.prepareForFlush().flush();
			  /*
			   * The log version is now in the store, flushed and persistent. If we crash
			   * now, on recovery we'll attempt to open the version we're about to create
			   * (but haven't yet), discover it's not there. That will lead to creating
			   * the file, setting the header and continuing.
			   * We using committing transaction id as a source of last transaction id here since
			   * we can have transactions that are not yet published as committed but were already stored
			   * into transaction log that was just rotated.
			   */
			  PhysicalLogVersionedStoreChannel newLog = _logFiles.createLogChannelForVersion( newLogVersion, OpenMode.READ_WRITE, _context.committingTransactionId );
			  currentLog.close();
			  return newLog;
		 }

		 public virtual FlushablePositionAwareChannel Writer
		 {
			 get
			 {
				  return _writer;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.transaction.log.ReadableLogChannel getReader(org.neo4j.kernel.impl.transaction.log.LogPosition position) throws java.io.IOException
		 public override ReadableLogChannel GetReader( LogPosition position )
		 {
			  return GetReader( position, _readerLogVersionBridge );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.transaction.log.ReadableLogChannel getReader(org.neo4j.kernel.impl.transaction.log.LogPosition position, org.neo4j.kernel.impl.transaction.log.LogVersionBridge logVersionBridge) throws java.io.IOException
		 public override ReadableLogChannel GetReader( LogPosition position, LogVersionBridge logVersionBridge )
		 {
			  PhysicalLogVersionedStoreChannel logChannel = _logFiles.openForVersion( position.LogVersion );
			  logChannel.Position( position.ByteOffset );
			  return new ReadAheadLogChannel( logChannel, logVersionBridge );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(LogFile_LogFileVisitor visitor, org.neo4j.kernel.impl.transaction.log.LogPosition startingFromPosition) throws java.io.IOException
		 public override void Accept( LogFile_LogFileVisitor visitor, LogPosition startingFromPosition )
		 {
			  using ( ReadableLogChannel reader = GetReader( startingFromPosition ) )
			  {
					visitor.Visit( reader );
			  }
		 }
	}

}