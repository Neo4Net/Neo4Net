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
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using Neo4Net.Cursors;
	using Neo4Net.Cursors;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using Neo4Net.Kernel.impl.transaction.log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.EntryRecord.read;

	/// <summary>
	/// A cursor for iterating over RAFT log entries starting at an index and until the end of the segment is met.
	/// The segment is demarcated by the ReadAheadChannel provided, which should properly signal the end of the channel.
	/// </summary>
	internal class EntryRecordCursor : IOCursor<EntryRecord>
	{
		 private ReadAheadChannel<StoreChannel> _bufferedReader;

		 private readonly LogPosition _position;
		 private readonly CursorValue<EntryRecord> _currentRecord = new CursorValue<EntryRecord>();
		 private readonly Reader _reader;
		 private ChannelMarshal<ReplicatedContent> _contentMarshal;
		 private readonly SegmentFile _segment;

		 private bool _hadError;
		 private bool _closed;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: EntryRecordCursor(Reader reader, org.neo4j.causalclustering.messaging.marshalling.ChannelMarshal<org.neo4j.causalclustering.core.replication.ReplicatedContent> contentMarshal, long currentIndex, long wantedIndex, SegmentFile segment) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
		 internal EntryRecordCursor( Reader reader, ChannelMarshal<ReplicatedContent> contentMarshal, long currentIndex, long wantedIndex, SegmentFile segment )
		 {
			  this._bufferedReader = new ReadAheadChannel<StoreChannel>( reader.Channel() );
			  this._reader = reader;
			  this._contentMarshal = contentMarshal;
			  this._segment = segment;

			  /* The cache lookup might have given us an earlier position, scan forward to the exact position. */
			  while ( currentIndex < wantedIndex )
			  {
					read( _bufferedReader, contentMarshal );
					currentIndex++;
			  }

			  this._position = new LogPosition( currentIndex, _bufferedReader.position() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  EntryRecord entryRecord;
			  try
			  {
					entryRecord = read( _bufferedReader, _contentMarshal );
			  }
			  catch ( EndOfStreamException )
			  {
					_currentRecord.invalidate();
					return false;
			  }
			  catch ( IOException e )
			  {
					_hadError = true;
					throw e;
			  }

			  _currentRecord.set( entryRecord );
			  _position.byteOffset = _bufferedReader.position();
			  _position.logIndex++;
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  if ( _closed )
			  {
					/* This is just a defensive measure, for catching user errors from messing up the refCount. */
					throw new System.InvalidOperationException( "Already closed" );
			  }

			  _bufferedReader = null;
			  _closed = true;
			  _segment.refCount().decrease();

			  if ( _hadError )
			  {
					/* If the reader had en error, then it should be closed instead of returned to the pool. */
					_reader.Dispose();
			  }
			  else
			  {
					_segment.positionCache().put(_position);
					_segment.readerPool().release(_reader);
			  }
		 }

		 public override EntryRecord Get()
		 {
			  return _currentRecord.get();
		 }
	}

}