using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.transaction.log.reverse
{

	using Neo4Net.Kernel.impl.transaction.log;
	using Neo4Net.Kernel.impl.transaction.log;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using UnsupportedLogVersionException = Neo4Net.Kernel.impl.transaction.log.entry.UnsupportedLogVersionException;

	/// <summary>
	/// Returns transactions in reverse order in a log file. It tries to keep peak memory consumption to a minimum
	/// by first sketching out the offsets of all transactions in the log. Then it starts from the end and moves backwards,
	/// taking advantage of read-ahead feature of the <seealso cref="ReadAheadLogChannel"/> by moving in chunks backwards in roughly
	/// the size of the read-ahead window. Coming across large transactions means moving further back to at least read one transaction
	/// per chunk "move". This is all internal, so from the outside it simply reverses a transaction log.
	/// The memory overhead compared to reading a log in the natural order is almost negligible.
	/// 
	/// This cursor currently only works for a single log file, such that the given <seealso cref="ReadAheadLogChannel"/> should not be
	/// instantiated with a <seealso cref="LogVersionBridge"/> moving it over to other versions when exhausted. For reversing a whole
	/// log stream consisting of multiple log files have a look at <seealso cref="ReversedMultiFileTransactionCursor"/>.
	/// 
	/// <pre>
	/// 
	///              ◄────────────────┤                          <seealso cref="chunkTransactions"/> for the current chunk, reading <seealso cref="readNextChunk()"/>.
	/// [2  |3|4    |5  |6          |7 |8   |9      |10  ]
	/// ▲   ▲ ▲     ▲   ▲           ▲  ▲    ▲       ▲
	/// │   │ │     │   │           │  │    │       │
	/// └───┴─┴─────┼───┴───────────┴──┴────┴───────┴─────────── <seealso cref="offsets"/>
	///             │
	///             └─────────────────────────────────────────── <seealso cref="chunkStartOffsetIndex"/> moves forward in <seealso cref="readNextChunk()"/>
	/// 
	/// </pre>
	/// </summary>
	/// <seealso cref= ReversedMultiFileTransactionCursor </seealso>
	public class ReversedSingleFileTransactionCursor : TransactionCursor
	{
		 // Should this be passed in or extracted from the read-ahead channel instead?
		 private static readonly int _chunkSize = ReadAheadChannel.DEFAULT_READ_AHEAD_SIZE;

		 private readonly ReadAheadLogChannel _channel;
		 private readonly bool _failOnCorruptedLogFiles;
		 private readonly ReversedTransactionCursorMonitor _monitor;
		 private readonly TransactionCursor _transactionCursor;
		 // Should be generally large enough to hold transactions in a chunk, where one chunk is the read-ahead size of ReadAheadLogChannel
		 private readonly Deque<CommittedTransactionRepresentation> _chunkTransactions = new LinkedList<CommittedTransactionRepresentation>( 20 );
		 private CommittedTransactionRepresentation _currentChunkTransaction;
		 // May be longer than required, offsetLength holds the actual length.
		 private readonly long[] _offsets;
		 private int _offsetsLength;
		 private int _chunkStartOffsetIndex;
		 private long _totalSize;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ReversedSingleFileTransactionCursor(org.neo4j.kernel.impl.transaction.log.ReadAheadLogChannel channel, org.neo4j.kernel.impl.transaction.log.entry.LogEntryReader<org.neo4j.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel> logEntryReader, boolean failOnCorruptedLogFiles, ReversedTransactionCursorMonitor monitor) throws java.io.IOException
		 internal ReversedSingleFileTransactionCursor( ReadAheadLogChannel channel, LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader, bool failOnCorruptedLogFiles, ReversedTransactionCursorMonitor monitor )
		 {
			  this._channel = channel;
			  this._failOnCorruptedLogFiles = failOnCorruptedLogFiles;
			  this._monitor = monitor;
			  // There's an assumption here: that the underlying channel can move in between calls and that the
			  // transaction cursor will just happily read from the new position.
			  this._transactionCursor = new PhysicalTransactionCursor<>( channel, logEntryReader );
			  this._offsets = SketchOutTransactionStartOffsets();
		 }

		 // Also initializes offset indexes
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long[] sketchOutTransactionStartOffsets() throws java.io.IOException
		 private long[] SketchOutTransactionStartOffsets()
		 {
			  // Grows on demand. Initially sized to be able to hold all transaction start offsets for a single log file
			  long[] offsets = new long[10_000];
			  int offsetCursor = 0;

			  long logVersion = _channel.Version;
			  long startOffset = _channel.position();
			  try
			  {
					while ( _transactionCursor.next() )
					{
						 if ( offsetCursor == offsets.Length )
						 { // Grow
							  offsets = Arrays.copyOf( offsets, offsetCursor * 2 );
						 }
						 offsets[offsetCursor++] = startOffset;
						 startOffset = _channel.position();
					}
			  }
			  catch ( Exception e ) when ( e is IOException || e is UnsupportedLogVersionException )
			  {
					_monitor.transactionalLogRecordReadFailure( offsets, offsetCursor, logVersion );
					if ( _failOnCorruptedLogFiles )
					{
						 throw e;
					}
			  }

			  if ( _channel.Version != logVersion )
			  {
					throw new System.ArgumentException( "The channel which was passed in bridged multiple log versions, it started at version " + logVersion + ", but continued through to version " + _channel.Version + ". This isn't supported" );
			  }

			  _offsetsLength = offsetCursor;
			  _chunkStartOffsetIndex = offsetCursor;
			  _totalSize = _channel.position();

			  return offsets;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  if ( !Exhausted() )
			  {
					if ( CurrentChunkExhausted() )
					{
						 ReadNextChunk();
					}
					_currentChunkTransaction = _chunkTransactions.pop();
					return true;
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readNextChunk() throws java.io.IOException
		 private void ReadNextChunk()
		 {
			  Debug.Assert( _chunkStartOffsetIndex > 0 );

			  // Start at lowOffsetIndex - 1 and count backwards until almost reaching the chunk size
			  long highOffset = _chunkStartOffsetIndex == _offsetsLength ? _totalSize : _offsets[_chunkStartOffsetIndex];
			  int newLowOffsetIndex = _chunkStartOffsetIndex;
			  while ( newLowOffsetIndex > 0 )
			  {
					long deltaOffset = highOffset - _offsets[--newLowOffsetIndex];
					if ( deltaOffset > _chunkSize )
					{ // We've now read more than the read-ahead size, let's call this the end of this chunk
						 break;
					}
			  }
			  Debug.Assert( _chunkStartOffsetIndex - newLowOffsetIndex > 0 );

			  // We've established the chunk boundaries. Initialize all offsets and read the transactions in this
			  // chunk into actual transaction objects
			  int chunkLength = _chunkStartOffsetIndex - newLowOffsetIndex;
			  _chunkStartOffsetIndex = newLowOffsetIndex;
			  _channel.CurrentPosition = _offsets[_chunkStartOffsetIndex];
			  Debug.Assert( _chunkTransactions.Empty );
			  for ( int i = 0; i < chunkLength; i++ )
			  {
					bool success = _transactionCursor.next();
					Debug.Assert( success );

					_chunkTransactions.push( _transactionCursor.get() );
			  }
		 }

		 private bool CurrentChunkExhausted()
		 {
			  return _chunkTransactions.Empty;
		 }

		 private bool Exhausted()
		 {
			  return _chunkStartOffsetIndex == 0 && CurrentChunkExhausted();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _transactionCursor.close(); // closes the channel too
		 }

		 public override CommittedTransactionRepresentation Get()
		 {
			  return _currentChunkTransaction;
		 }

		 public override LogPosition Position()
		 {
			  throw new System.NotSupportedException( "Should not be called" );
		 }
	}

}