using System.Collections.Generic;
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
	using Neo4Net.Kernel.impl.transaction.log;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;


	/// <summary>
	/// Recovers all the state required for operating the RAFT log and does some simple
	/// verifications; e.g. checking for gaps, verifying headers.
	/// </summary>
	internal class RecoveryProtocol
	{
		 private static readonly SegmentHeader.Marshal _headerMarshal = new SegmentHeader.Marshal();

		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly FileNames _fileNames;
		 private readonly ChannelMarshal<ReplicatedContent> _contentMarshal;
		 private readonly LogProvider _logProvider;
		 private readonly Log _log;
		 private ReaderPool _readerPool;

		 internal RecoveryProtocol( FileSystemAbstraction fileSystem, FileNames fileNames, ReaderPool readerPool, ChannelMarshal<ReplicatedContent> contentMarshal, LogProvider logProvider )
		 {
			  this._fileSystem = fileSystem;
			  this._fileNames = fileNames;
			  this._readerPool = readerPool;
			  this._contentMarshal = contentMarshal;
			  this._logProvider = logProvider;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: State run() throws java.io.IOException, DamagedLogStorageException, DisposedException
		 internal virtual State Run()
		 {
			  State state = new State();
			  SortedDictionary<long, File> files = _fileNames.getAllFiles( _fileSystem, _log );

			  if ( Files.SetOfKeyValuePairs().Empty )
			  {
					state.Segments = new Segments( _fileSystem, _fileNames, _readerPool, emptyList(), _contentMarshal, _logProvider, -1 );
					state.Segments.rotate( -1, -1, -1 );
					state.Terms = new Terms( -1, -1 );
					return state;
			  }

			  IList<SegmentFile> segmentFiles = new List<SegmentFile>();
			  SegmentFile segment = null;

			  long expectedVersion = Files.firstKey();
			  bool mustRecoverLastHeader = false;
			  bool skip = true; // the first file is treated the same as a skip

			  foreach ( KeyValuePair<long, File> entry in Files.SetOfKeyValuePairs() )
			  {
					long fileNameVersion = entry.Key;
					File file = entry.Value;
					SegmentHeader header;

					CheckVersionSequence( fileNameVersion, expectedVersion );

					try
					{
						 header = LoadHeader( _fileSystem, file );
						 CheckVersionMatches( header.Version(), fileNameVersion );
					}
					catch ( EndOfStreamException e )
					{
						 if ( Files.lastKey() != fileNameVersion )
						 {
							  throw new DamagedLogStorageException( e, "Intermediate file with incomplete or no header found: %s", file );
						 }
						 else if ( Files.Count == 1 )
						 {
							  throw new DamagedLogStorageException( e, "Single file with incomplete or no header found: %s", file );
						 }

						 /* Last file header must be recovered by scanning next-to-last file and writing a new header based on that. */
						 mustRecoverLastHeader = true;
						 break;
					}

					segment = new SegmentFile( _fileSystem, file, _readerPool, fileNameVersion, _contentMarshal, _logProvider, header );
					segmentFiles.Add( segment );

					if ( segment.Header().prevIndex() != segment.Header().prevFileLastIndex() )
					{
						 _log.info( format( "Skipping from index %d to %d.", segment.Header().prevFileLastIndex(), segment.Header().prevIndex() + 1 ) );
						 skip = true;
					}

					if ( skip )
					{
						 state.PrevIndex = segment.Header().prevIndex();
						 state.PrevTerm = segment.Header().prevTerm();
						 skip = false;
					}

					expectedVersion++;
			  }

			  Debug.Assert( segment != null );

			  state.AppendIndex = segment.Header().prevIndex();
			  state.Terms = new Terms( segment.Header().prevIndex(), segment.Header().prevTerm() );

			  using ( IOCursor<EntryRecord> cursor = segment.GetCursor( segment.Header().prevIndex() + 1 ) )
			  {
					while ( cursor.next() )
					{
						 EntryRecord entry = cursor.get();
						 state.AppendIndex = entry.LogIndex();
						 state.Terms.append( state.AppendIndex, entry.LogEntry().term() );
					}
			  }

			  if ( mustRecoverLastHeader )
			  {
					SegmentHeader header = new SegmentHeader( state.AppendIndex, expectedVersion, state.AppendIndex, state.Terms.latest() );
					_log.warn( "Recovering last file based on next-to-last file. " + header );

					File file = _fileNames.getForVersion( expectedVersion );
					WriteHeader( _fileSystem, file, header );

					segment = new SegmentFile( _fileSystem, file, _readerPool, expectedVersion, _contentMarshal, _logProvider, header );
					segmentFiles.Add( segment );
			  }

			  state.Segments = new Segments( _fileSystem, _fileNames, _readerPool, segmentFiles, _contentMarshal, _logProvider, segment.Header().version() );

			  return state;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static SegmentHeader loadHeader(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 private static SegmentHeader LoadHeader( FileSystemAbstraction fileSystem, File file )
		 {
			  using ( StoreChannel channel = fileSystem.Open( file, OpenMode.READ ) )
			  {
					return _headerMarshal.unmarshal( new ReadAheadChannel<>( channel, SegmentHeader.Size ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void writeHeader(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file, SegmentHeader header) throws java.io.IOException
		 private static void WriteHeader( FileSystemAbstraction fileSystem, File file, SegmentHeader header )
		 {
			  using ( StoreChannel channel = fileSystem.Open( file, OpenMode.READ_WRITE ) )
			  {
					channel.Position( 0 );
					PhysicalFlushableChannel writer = new PhysicalFlushableChannel( channel, SegmentHeader.Size );
					_headerMarshal.marshal( header, writer );
					writer.PrepareForFlush().flush();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkVersionSequence(long fileNameVersion, long expectedVersion) throws DamagedLogStorageException
		 private static void CheckVersionSequence( long fileNameVersion, long expectedVersion )
		 {
			  if ( fileNameVersion != expectedVersion )
			  {
					throw new DamagedLogStorageException( "File versions not strictly monotonic. Expected: %d but found: %d", expectedVersion, fileNameVersion );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkVersionMatches(long headerVersion, long fileNameVersion) throws DamagedLogStorageException
		 private static void CheckVersionMatches( long headerVersion, long fileNameVersion )
		 {
			  if ( headerVersion != fileNameVersion )
			  {
					throw new DamagedLogStorageException( "File version does not match header version. Expected: %d but found: %d", headerVersion, fileNameVersion );
			  }
		 }
	}

}