using System.Diagnostics;

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
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using Neo4Net.causalclustering.messaging.marshalling;
	using Neo4Net.Cursors;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using PhysicalFlushableChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalFlushableChannel;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Keeps track of a segment of the RAFT log, i.e. a consecutive set of entries.
	/// Concurrent reading is thread-safe.
	/// </summary>
	internal class SegmentFile : IDisposable
	{
		 private static readonly SegmentHeader.Marshal _headerMarshal = new SegmentHeader.Marshal();

		 private readonly Log _log;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly File _file;
		 private readonly ReaderPool _readerPool;
		 private readonly ChannelMarshal<ReplicatedContent> _contentMarshal;

		 private readonly PositionCache _positionCache;
		 private readonly ReferenceCounter _refCount;

		 private readonly SegmentHeader _header;
		 private readonly long _version;

		 private PhysicalFlushableChannel _bufferedWriter;

		 internal SegmentFile( FileSystemAbstraction fileSystem, File file, ReaderPool readerPool, long version, ChannelMarshal<ReplicatedContent> contentMarshal, LogProvider logProvider, SegmentHeader header )
		 {
			  this._fileSystem = fileSystem;
			  this._file = file;
			  this._readerPool = readerPool;
			  this._contentMarshal = contentMarshal;
			  this._header = header;
			  this._version = version;

			  this._positionCache = new PositionCache();
			  this._refCount = new ReferenceCounter();

			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static SegmentFile create(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file, ReaderPool readerPool, long version, org.neo4j.causalclustering.messaging.marshalling.ChannelMarshal<org.neo4j.causalclustering.core.replication.ReplicatedContent> contentMarshal, org.neo4j.logging.LogProvider logProvider, SegmentHeader header) throws java.io.IOException
		 internal static SegmentFile Create( FileSystemAbstraction fileSystem, File file, ReaderPool readerPool, long version, ChannelMarshal<ReplicatedContent> contentMarshal, LogProvider logProvider, SegmentHeader header )
		 {
			  if ( fileSystem.FileExists( file ) )
			  {
					throw new System.InvalidOperationException( "File was not expected to exist" );
			  }

			  SegmentFile segment = new SegmentFile( fileSystem, file, readerPool, version, contentMarshal, logProvider, header );
			  _headerMarshal.marshal( header, segment.OrCreateWriter );
			  segment.Flush();

			  return segment;
		 }

		 /// <summary>
		 /// Channels must be closed when no longer used, so that they are released back to the pool of readers.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.cursor.IOCursor<org.neo4j.causalclustering.core.consensus.log.EntryRecord> getCursor(long logIndex) throws java.io.IOException, DisposedException
		 internal virtual IOCursor<EntryRecord> GetCursor( long logIndex )
		 {
			  Debug.Assert( logIndex > _header.prevIndex() );

			  if ( !_refCount.increase() )
			  {
					throw new DisposedException();
			  }

			  /* This is the relative index within the file, starting from zero. */
			  long offsetIndex = logIndex - ( _header.prevIndex() + 1 );

			  LogPosition position = _positionCache.lookup( offsetIndex );
			  Reader reader = _readerPool.acquire( _version, position.ByteOffset );

			  try
			  {
					long currentIndex = position.LogIndex;
					return new EntryRecordCursor( reader, _contentMarshal, currentIndex, offsetIndex, this );
			  }
			  catch ( EndOfStreamException )
			  {
					_readerPool.release( reader );
					_refCount.decrease();
					return IOCursor.Empty;
			  }
			  catch ( IOException e )
			  {
					reader.Dispose();
					_refCount.decrease();
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized org.neo4j.kernel.impl.transaction.log.PhysicalFlushableChannel getOrCreateWriter() throws java.io.IOException
		 private PhysicalFlushableChannel OrCreateWriter
		 {
			 get
			 {
				 lock ( this )
				 {
					  if ( _bufferedWriter == null )
					  {
							if ( !_refCount.increase() )
							{
								 throw new IOException( "Writer has been closed" );
							}
         
							StoreChannel channel = _fileSystem.open( _file, OpenMode.READ_WRITE );
							channel.Position( channel.size() );
							_bufferedWriter = new PhysicalFlushableChannel( channel );
					  }
					  return _bufferedWriter;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized long position() throws java.io.IOException
		 internal virtual long Position()
		 {
			 lock ( this )
			 {
				  return OrCreateWriter.position();
			 }
		 }

		 /// <summary>
		 /// Idempotently closes the writer.
		 /// </summary>
		 internal virtual void CloseWriter()
		 {
			 lock ( this )
			 {
				  if ( _bufferedWriter != null )
				  {
						try
						{
							 Flush();
							 _bufferedWriter.Dispose();
						}
						catch ( IOException e )
						{
							 _log.error( "Failed to close writer for: " + _file, e );
						}
      
						_bufferedWriter = null;
						_refCount.decrease();
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void write(long logIndex, org.neo4j.causalclustering.core.consensus.log.RaftLogEntry entry) throws java.io.IOException
		 public virtual void Write( long logIndex, RaftLogEntry entry )
		 {
			 lock ( this )
			 {
				  EntryRecord.write( OrCreateWriter, _contentMarshal, logIndex, entry.Term(), entry.Content() );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized void flush() throws java.io.IOException
		 internal virtual void Flush()
		 {
			 lock ( this )
			 {
				  _bufferedWriter.prepareForFlush().flush();
			 }
		 }

		 public virtual bool Delete()
		 {
			  return _fileSystem.deleteFile( _file );
		 }

		 public virtual SegmentHeader Header()
		 {
			  return _header;
		 }

		 public virtual long Size()
		 {
			  return _fileSystem.getFileSize( _file );
		 }

		 internal virtual string Filename
		 {
			 get
			 {
				  return _file.Name;
			 }
		 }

		 /// <summary>
		 /// Called by the pruner when it wants to prune this segment. If there are no open
		 /// readers or writers then the segment will be closed.
		 /// </summary>
		 /// <returns> True if the segment can be pruned at this time, false otherwise. </returns>
		 internal virtual bool TryClose()
		 {
			  if ( _refCount.tryDispose() )
			  {
					Close();
					return true;
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  CloseWriter();
			  _readerPool.prune( _version );

			  if ( !_refCount.tryDispose() )
			  {
					throw new System.InvalidOperationException( format( "Segment still referenced. Value: %d", _refCount.get() ) );
			  }
		 }

		 public override string ToString()
		 {
			  return "SegmentFile{" +
						"file=" + _file.Name +
						", header=" + _header +
						'}';
		 }

		 internal virtual ReferenceCounter RefCount()
		 {
			  return _refCount;
		 }

		 internal virtual PositionCache PositionCache()
		 {
			  return _positionCache;
		 }

		 public virtual ReaderPool ReaderPool()
		 {
			  return _readerPool;
		 }
	}

}