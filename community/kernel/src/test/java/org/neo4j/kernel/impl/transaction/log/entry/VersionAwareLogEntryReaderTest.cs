using System;

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
namespace Org.Neo4j.Kernel.impl.transaction.log.entry
{
	using Test = org.junit.Test;

	using RecordStorageCommandReaderFactory = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using NeoCommandType = Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class VersionAwareLogEntryReaderTest
	{
		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadAStartLogEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadAStartLogEntry()
		 {
			  // given
			  LogEntryVersion version = LogEntryVersion.CURRENT;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryStart start = new LogEntryStart(version, 1, 2, 3, 4, new byte[]{5}, new org.neo4j.kernel.impl.transaction.log.LogPosition(0, 31));
			  LogEntryStart start = new LogEntryStart( version, 1, 2, 3, 4, new sbyte[]{ 5 }, new LogPosition( 0, 31 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.Put( version.byteCode() ); // version
			  channel.Put( LogEntryByteCodes.TxStart ); // type
			  channel.PutInt( start.MasterId );
			  channel.PutInt( start.LocalId );
			  channel.PutLong( start.TimeWritten );
			  channel.PutLong( start.LastCommittedTxWhenTransactionStarted );
			  channel.PutInt( start.AdditionalHeader.Length );
			  channel.Put( start.AdditionalHeader, start.AdditionalHeader.Length );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = logEntryReader.readLogEntry(channel);
			  LogEntry logEntry = _logEntryReader.readLogEntry( channel );

			  // then
			  assertEquals( start, logEntry );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadACommitLogEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadACommitLogEntry()
		 {
			  // given
			  LogEntryVersion version = LogEntryVersion.CURRENT;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryCommit commit = new LogEntryCommit(version, 42, 21);
			  LogEntryCommit commit = new LogEntryCommit( version, 42, 21 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.Put( version.byteCode() );
			  channel.Put( LogEntryByteCodes.TxCommit );
			  channel.PutLong( commit.TxId );
			  channel.PutLong( commit.TimeWritten );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = logEntryReader.readLogEntry(channel);
			  LogEntry logEntry = _logEntryReader.readLogEntry( channel );

			  // then
			  assertEquals( commit, logEntry );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadACommandLogEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadACommandLogEntry()
		 {
			  // given
			  LogEntryVersion version = LogEntryVersion.CURRENT;
			  Command.NodeCommand nodeCommand = new Command.NodeCommand( new NodeRecord( 11 ), new NodeRecord( 11 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryCommand command = new LogEntryCommand(version, nodeCommand);
			  LogEntryCommand command = new LogEntryCommand( version, nodeCommand );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.Put( version.byteCode() );
			  channel.Put( LogEntryByteCodes.Command );
			  nodeCommand.Serialize( channel );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = logEntryReader.readLogEntry(channel);
			  LogEntry logEntry = _logEntryReader.readLogEntry( channel );

			  // then
			  assertEquals( command, logEntry );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadACheckPointLogEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadACheckPointLogEntry()
		 {
			  // given
			  LogEntryVersion version = LogEntryVersion.CURRENT;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPosition logPosition = new org.neo4j.kernel.impl.transaction.log.LogPosition(42, 43);
			  LogPosition logPosition = new LogPosition( 42, 43 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CheckPoint checkPoint = new CheckPoint(version, logPosition);
			  CheckPoint checkPoint = new CheckPoint( version, logPosition );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.Put( version.byteCode() );
			  channel.Put( LogEntryByteCodes.CheckPoint );
			  channel.PutLong( logPosition.LogVersion );
			  channel.PutLong( logPosition.ByteOffset );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = logEntryReader.readLogEntry(channel);
			  LogEntry logEntry = _logEntryReader.readLogEntry( channel );

			  // then
			  assertEquals( checkPoint, logEntry );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullWhenThereIsNoCommand() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNullWhenThereIsNoCommand()
		 {
			  // given
			  LogEntryVersion version = LogEntryVersion.CURRENT;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.Put( version.byteCode() );
			  channel.Put( LogEntryByteCodes.Command );
			  channel.Put( Org.Neo4j.Kernel.impl.transaction.command.NeoCommandType_Fields.None );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = logEntryReader.readLogEntry(channel);
			  LogEntry logEntry = _logEntryReader.readLogEntry( channel );

			  // then
			  assertNull( logEntry );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullWhenNotEnoughDataInTheChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNullWhenNotEnoughDataInTheChannel()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = logEntryReader.readLogEntry(channel);
			  LogEntry logEntry = _logEntryReader.readLogEntry( channel );

			  // then
			  assertNull( logEntry );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSkipBadVersionAndTypeBytesInBetweenLogEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSkipBadVersionAndTypeBytesInBetweenLogEntries()
		 {
			  // GIVEN
			  AcceptingInvalidLogEntryHandler invalidLogEntryHandler = new AcceptingInvalidLogEntryHandler();
			  VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>( new RecordStorageCommandReaderFactory(), invalidLogEntryHandler );
			  InMemoryClosableChannel channel = new InMemoryClosableChannel( 1_000 );
			  LogEntryWriter writer = new LogEntryWriter( channel.Writer() );
			  long startTime = currentTimeMillis();
			  long commitTime = startTime + 10;
			  writer.WriteStartEntry( 1, 2, startTime, 3, new sbyte[0] );
			  writer.WriteCommitEntry( 4, commitTime );
			  channel.Put( ( sbyte ) 127 );
			  channel.Put( ( sbyte ) 126 );
			  channel.Put( ( sbyte ) 125 );
			  long secondStartTime = startTime + 100;
			  writer.WriteStartEntry( 1, 2, secondStartTime, 4, new sbyte[0] );

			  // WHEN
			  LogEntryStart readStartEntry = reader.ReadLogEntry( channel.Reader() ).@as();
			  LogEntryCommit readCommitEntry = reader.ReadLogEntry( channel.Reader() ).@as();
			  LogEntryStart readSecondStartEntry = reader.ReadLogEntry( channel.Reader() ).@as();

			  // THEN
			  assertEquals( 1, readStartEntry.MasterId );
			  assertEquals( 2, readStartEntry.LocalId );
			  assertEquals( startTime, readStartEntry.TimeWritten );

			  assertEquals( 4, readCommitEntry.TxId );
			  assertEquals( commitTime, readCommitEntry.TimeWritten );

			  assertEquals( 3, invalidLogEntryHandler.BytesSkippedConflict );
			  assertEquals( 3, invalidLogEntryHandler.InvalidEntryCalls );

			  assertEquals( 1, readSecondStartEntry.MasterId );
			  assertEquals( 2, readSecondStartEntry.LocalId );
			  assertEquals( secondStartTime, readSecondStartEntry.TimeWritten );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSkipBadLogEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSkipBadLogEntries()
		 {
			  // GIVEN
			  AcceptingInvalidLogEntryHandler invalidLogEntryHandler = new AcceptingInvalidLogEntryHandler();
			  VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>( new RecordStorageCommandReaderFactory(), invalidLogEntryHandler );
			  InMemoryClosableChannel channel = new InMemoryClosableChannel( 1_000 );
			  LogEntryWriter writer = new LogEntryWriter( channel.Writer() );
			  long startTime = currentTimeMillis();
			  long commitTime = startTime + 10;
			  writer.WriteStartEntry( 1, 2, startTime, 3, new sbyte[0] );

			  // Write command ...
			  int posBefore = channel.WriterPosition();
			  writer.Serialize( singletonList( new Command.NodeCommand( new NodeRecord( 1 ), ( new NodeRecord( 1 ) ).initialize( true, 1, false, 2, 0 ) ) ) );
			  int posAfter = channel.WriterPosition();
			  // ... which then gets overwritten with invalid data
			  channel.PositionWriter( posBefore );
			  while ( channel.WriterPosition() < posAfter )
			  {
					channel.Put( unchecked( ( sbyte ) 0xFF ) );
			  }

			  writer.WriteCommitEntry( 4, commitTime );
			  long secondStartTime = startTime + 100;
			  writer.WriteStartEntry( 1, 2, secondStartTime, 4, new sbyte[0] );

			  // WHEN
			  LogEntryStart readStartEntry = reader.ReadLogEntry( channel.Reader() ).@as();
			  LogEntryCommit readCommitEntry = reader.ReadLogEntry( channel.Reader() ).@as();
			  LogEntryStart readSecondStartEntry = reader.ReadLogEntry( channel.Reader() ).@as();

			  // THEN
			  assertEquals( 1, readStartEntry.MasterId );
			  assertEquals( 2, readStartEntry.LocalId );
			  assertEquals( startTime, readStartEntry.TimeWritten );

			  assertEquals( 4, readCommitEntry.TxId );
			  assertEquals( commitTime, readCommitEntry.TimeWritten );

			  assertEquals( posAfter - posBefore, invalidLogEntryHandler.BytesSkippedConflict );
			  assertEquals( posAfter - posBefore, invalidLogEntryHandler.InvalidEntryCalls );

			  assertEquals( 1, readSecondStartEntry.MasterId );
			  assertEquals( 2, readSecondStartEntry.LocalId );
			  assertEquals( secondStartTime, readSecondStartEntry.TimeWritten );
		 }

		 internal class AcceptingInvalidLogEntryHandler : InvalidLogEntryHandler
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long BytesSkippedConflict;
			  internal Exception E;
			  internal LogPosition Position;
			  internal int InvalidEntryCalls;

			  public override bool HandleInvalidEntry( Exception e, LogPosition position )
			  {
					this.E = e;
					this.Position = position;
					InvalidEntryCalls++;
					return true;
			  }

			  public override void BytesSkipped( long bytesSkipped )
			  {
					this.BytesSkippedConflict += bytesSkipped;
			  }
		 }
	}

}