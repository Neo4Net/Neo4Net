using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;
	using Neo4Net.Cursors;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// The segmented RAFT log is an append only log supporting the operations required to support
	/// the RAFT consensus algorithm.
	/// <para>
	/// A RAFT log must be able to append new entries, but also truncate not yet committed entries,
	/// prune out old compacted entries and skip to a later starting point.
	/// </para>
	/// <para>
	/// The RAFT log consists of a sequence of individual log files, called segments, with
	/// the following format:
	/// </para>
	/// <para>
	/// [HEADER] [ENTRY]*
	/// </para>
	/// <para>
	/// So a header with zero or more entries following it. Each segment file contains a consecutive
	/// sequence of appended entries. The operations of truncating and skipping in the log is implemented
	/// by switching to the next segment file, called the next version. A new segment file is also started
	/// when the threshold for a particular file has been reached.
	/// </para>
	/// </summary>
	public class SegmentedRaftLog : LifecycleAdapter, RaftLog
	{
		 private readonly int _readerPoolMaxAge = 1; // minutes

		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly File _directory;
		 private readonly long _rotateAtSize;
		 private readonly ChannelMarshal<ReplicatedContent> _contentMarshal;
		 private readonly FileNames _fileNames;
		 private readonly IJobScheduler _scheduler;
		 private readonly Log _log;

		 private bool _needsRecovery;
		 private readonly LogProvider _logProvider;
		 private readonly SegmentedRaftLogPruner _pruner;

		 private State _state;
		 private readonly ReaderPool _readerPool;
		 private JobHandle _readerPoolPruner;

		 public SegmentedRaftLog( FileSystemAbstraction fileSystem, File directory, long rotateAtSize, ChannelMarshal<ReplicatedContent> contentMarshal, LogProvider logProvider, int readerPoolSize, Clock clock, IJobScheduler scheduler, CoreLogPruningStrategy pruningStrategy )
		 {
			  this._fileSystem = fileSystem;
			  this._directory = directory;
			  this._rotateAtSize = rotateAtSize;
			  this._contentMarshal = contentMarshal;
			  this._logProvider = logProvider;
			  this._scheduler = scheduler;

			  this._fileNames = new FileNames( directory );
			  this._readerPool = new ReaderPool( readerPoolSize, logProvider, _fileNames, fileSystem, clock );
			  this._pruner = new SegmentedRaftLogPruner( pruningStrategy );
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void start() throws java.io.IOException, DamagedLogStorageException, DisposedException
		 public override void Start()
		 {
			 lock ( this )
			 {
				  if ( !_directory.exists() && !_directory.mkdirs() )
				  {
						throw new IOException( "Could not create: " + _directory );
				  }
      
				  _state = ( new RecoveryProtocol( _fileSystem, _fileNames, _readerPool, _contentMarshal, _logProvider ) ).run();
				  _log.info( "log started with recovered state %s", _state );
				  /*
				   * Recovery guarantees that once complete the header of the last raft log file is intact. No such guarantee
				   * is made for the last log entry in the last file (or any of the files for that matter). To complete
				   * recovery we need to rotate away the last log file, so that any incomplete entries at the end of the last
				   * do not have entries appended after them, which would result in unaligned (and therefore wrong) reads.
				   * As an obvious optimization, we don't need to rotate if the file contains only the header, such as is
				   * the case of a newly created log.
				   */
				  if ( _state.segments.last().size() > SegmentHeader.Size )
				  {
						RotateSegment( _state.appendIndex, _state.appendIndex, _state.terms.latest() );
				  }
      
				  _readerPoolPruner = _scheduler.scheduleRecurring( Group.RAFT_READER_POOL_PRUNER, () => _readerPool.prune(_readerPoolMaxAge, MINUTES), _readerPoolMaxAge, _readerPoolMaxAge, MINUTES );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void stop() throws Throwable
		 public override void Stop()
		 {
			 lock ( this )
			 {
				  _readerPoolPruner.cancel( false );
				  _readerPool.close();
				  _state.segments.close();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized long append(org.neo4j.causalclustering.core.consensus.log.RaftLogEntry... entries) throws java.io.IOException
		 public override long Append( params RaftLogEntry[] entries )
		 {
			 lock ( this )
			 {
				  EnsureOk();
      
				  try
				  {
						foreach ( RaftLogEntry entry in entries )
						{
							 _state.appendIndex++;
							 _state.terms.append( _state.appendIndex, entry.Term() );
							 _state.segments.last().write(_state.appendIndex, entry);
						}
						_state.segments.last().flush();
				  }
				  catch ( Exception e )
				  {
						_needsRecovery = true;
						throw e;
				  }
      
				  if ( _state.segments.last().position() >= _rotateAtSize )
				  {
						RotateSegment( _state.appendIndex, _state.appendIndex, _state.terms.latest() );
				  }
      
				  return _state.appendIndex;
			 }
		 }

		 private void EnsureOk()
		 {
			  if ( _needsRecovery )
			  {
					throw new System.InvalidOperationException( "Raft log requires recovery" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void truncate(long fromIndex) throws java.io.IOException
		 public override void Truncate( long fromIndex )
		 {
			 lock ( this )
			 {
				  if ( _state.appendIndex < fromIndex )
				  {
						throw new System.ArgumentException( "Cannot truncate at index " + fromIndex + " when append index is " + _state.appendIndex );
				  }
      
				  long newAppendIndex = fromIndex - 1;
				  long newTerm = ReadEntryTerm( newAppendIndex );
				  TruncateSegment( _state.appendIndex, newAppendIndex, newTerm );
      
				  _state.appendIndex = newAppendIndex;
				  _state.terms.truncate( fromIndex );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void rotateSegment(long prevFileLastIndex, long prevIndex, long prevTerm) throws java.io.IOException
		 private void RotateSegment( long prevFileLastIndex, long prevIndex, long prevTerm )
		 {
			  _state.segments.last().closeWriter();
			  _state.segments.rotate( prevFileLastIndex, prevIndex, prevTerm );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void truncateSegment(long prevFileLastIndex, long prevIndex, long prevTerm) throws java.io.IOException
		 private void TruncateSegment( long prevFileLastIndex, long prevIndex, long prevTerm )
		 {
			  _state.segments.last().closeWriter();
			  _state.segments.truncate( prevFileLastIndex, prevIndex, prevTerm );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void skipSegment(long prevFileLastIndex, long prevIndex, long prevTerm) throws java.io.IOException
		 private void SkipSegment( long prevFileLastIndex, long prevIndex, long prevTerm )
		 {
			  _state.segments.last().closeWriter();
			  _state.segments.skip( prevFileLastIndex, prevIndex, prevTerm );
		 }

		 public override long AppendIndex()
		 {
			  return _state.appendIndex;
		 }

		 public override long PrevIndex()
		 {
			  return _state.prevIndex;
		 }

		 public override RaftLogCursor GetEntryCursor( long fromIndex )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cursor.IOCursor<org.neo4j.causalclustering.core.consensus.log.EntryRecord> inner = new EntryCursor(state.segments, fromIndex);
			  IOCursor<EntryRecord> inner = new EntryCursor( _state.segments, fromIndex );
			  return new SegmentedRaftLogCursor( fromIndex, inner );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized long skip(long newIndex, long newTerm) throws java.io.IOException
		 public override long Skip( long newIndex, long newTerm )
		 {
			 lock ( this )
			 {
				  _log.info( "Skipping from {index: %d, term: %d} to {index: %d, term: %d}", _state.appendIndex, _state.terms.latest(), newIndex, newTerm );
      
				  if ( _state.appendIndex < newIndex )
				  {
						SkipSegment( _state.appendIndex, newIndex, newTerm );
						_state.terms.skip( newIndex, newTerm );
      
						_state.prevIndex = newIndex;
						_state.prevTerm = newTerm;
						_state.appendIndex = newIndex;
				  }
      
				  return _state.appendIndex;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.core.consensus.log.RaftLogEntry readLogEntry(long logIndex) throws java.io.IOException
		 private RaftLogEntry ReadLogEntry( long logIndex )
		 {
			  using ( IOCursor<EntryRecord> cursor = new EntryCursor( _state.segments, logIndex ) )
			  {
					return cursor.next() ? cursor.get().logEntry() : null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readEntryTerm(long logIndex) throws java.io.IOException
		 public override long ReadEntryTerm( long logIndex )
		 {
			  if ( logIndex > _state.appendIndex )
			  {
					return -1;
			  }
			  long term = _state.terms.get( logIndex );
			  if ( term == -1 && logIndex >= _state.prevIndex )
			  {
					RaftLogEntry entry = ReadLogEntry( logIndex );
					term = ( entry != null ) ? entry.Term() : -1;
			  }
			  return term;
		 }

		 public override long Prune( long safeIndex )
		 {
			  long pruneIndex = _pruner.getIndexToPruneFrom( safeIndex, _state.segments );
			  SegmentFile oldestNotDisposed = _state.segments.prune( pruneIndex );

			  long newPrevIndex = oldestNotDisposed.Header().prevIndex();
			  long newPrevTerm = oldestNotDisposed.Header().prevTerm();

			  if ( newPrevIndex > _state.prevIndex )
			  {
					_state.prevIndex = newPrevIndex;
			  }

			  if ( newPrevTerm > _state.prevTerm )
			  {
					_state.prevTerm = newPrevTerm;
			  }

			  _state.terms.prune( _state.prevIndex );

			  return _state.prevIndex;
		 }
	}

}