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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{
	using Test = org.junit.Test;

	using RecordStorageCommandReaderFactory = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NeoCommandType = Neo4Net.Kernel.impl.transaction.command.NeoCommandType;
	using CommandReaderFactory = Neo4Net.Storageengine.Api.CommandReaderFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;

	public class LogEntryParserDispatcherV6Test
	{
		 private readonly LogEntryVersion _version = LogEntryVersion.CURRENT;
		 private readonly CommandReaderFactory _commandReader = new RecordStorageCommandReaderFactory();
		 private readonly LogPositionMarker _marker = new LogPositionMarker();
		 private readonly LogPosition _position = new LogPosition( 0, 29 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParserStartEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParserStartEntry()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryStart start = new LogEntryStart(version, 1, 2, 3, 4, new byte[]{5}, position);
			  LogEntryStart start = new LogEntryStart( _version, 1, 2, 3, 4, new sbyte[]{ 5 }, _position );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.PutInt( start.MasterId );
			  channel.PutInt( start.LocalId );
			  channel.PutLong( start.TimeWritten );
			  channel.PutLong( start.LastCommittedTxWhenTransactionStarted );
			  channel.PutInt( start.AdditionalHeader.Length );
			  channel.Put( start.AdditionalHeader, start.AdditionalHeader.Length );

			  channel.GetCurrentPosition( _marker );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryParser parser = version.entryParser(LogEntryByteCodes.TX_START);
			  LogEntryParser parser = _version.entryParser( LogEntryByteCodes.TxStart );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = parser.parse(version, channel, marker, commandReader);
			  LogEntry logEntry = parser.parse( _version, channel, _marker, _commandReader );

			  // then
			  assertEquals( start, logEntry );
			  assertFalse( parser.skip() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParserOnePhaseCommitEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParserOnePhaseCommitEntry()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryCommit commit = new LogEntryCommit(version, 42, 21);
			  LogEntryCommit commit = new LogEntryCommit( _version, 42, 21 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.PutLong( commit.TxId );
			  channel.PutLong( commit.TimeWritten );

			  channel.GetCurrentPosition( _marker );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryParser parser = version.entryParser(LogEntryByteCodes.TX_COMMIT);
			  LogEntryParser parser = _version.entryParser( LogEntryByteCodes.TxCommit );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = parser.parse(version, channel, marker, commandReader);
			  LogEntry logEntry = parser.parse( _version, channel, _marker, _commandReader );

			  // then
			  assertEquals( commit, logEntry );
			  assertFalse( parser.skip() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParserCommandsUsingAGivenFactory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParserCommandsUsingAGivenFactory()
		 {
			  // given
			  // The record, it will be used as before and after
			  NodeRecord theRecord = new NodeRecord( 1 );
			  Command.NodeCommand nodeCommand = new Command.NodeCommand( theRecord, theRecord );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryCommand command = new LogEntryCommand(version, nodeCommand);
			  LogEntryCommand command = new LogEntryCommand( _version, nodeCommand );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.Put( Neo4Net.Kernel.impl.transaction.command.NeoCommandType_Fields.NodeCommand );
			  channel.PutLong( theRecord.Id );

			  // record image before
			  channel.Put( ( sbyte ) 0 ); // not in use
			  channel.PutInt( 0 ); // number of dynamic records in use
			  // record image after
			  channel.Put( ( sbyte ) 0 ); // not in use
			  channel.PutInt( 0 ); // number of dynamic records in use

			  channel.GetCurrentPosition( _marker );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryParser parser = version.entryParser(LogEntryByteCodes.COMMAND);
			  LogEntryParser parser = _version.entryParser( LogEntryByteCodes.Command );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = parser.parse(version, channel, marker, commandReader);
			  LogEntry logEntry = parser.parse( _version, channel, _marker, _commandReader );

			  // then
			  assertEquals( command, logEntry );
			  assertFalse( parser.skip() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseCheckPointEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseCheckPointEntry()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CheckPoint checkPoint = new CheckPoint(new org.neo4j.kernel.impl.transaction.log.LogPosition(43, 44));
			  CheckPoint checkPoint = new CheckPoint( new LogPosition( 43, 44 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel channel = new org.neo4j.kernel.impl.transaction.log.InMemoryClosableChannel();
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.PutLong( checkPoint.LogPosition.LogVersion );
			  channel.PutLong( checkPoint.LogPosition.ByteOffset );

			  channel.GetCurrentPosition( _marker );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntryParser parser = version.entryParser(LogEntryByteCodes.CHECK_POINT);
			  LogEntryParser parser = _version.entryParser( LogEntryByteCodes.CheckPoint );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogEntry logEntry = parser.parse(version, channel, marker, commandReader);
			  LogEntry logEntry = parser.parse( _version, channel, _marker, _commandReader );

			  // then
			  assertEquals( checkPoint, logEntry );
			  assertFalse( parser.skip() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldThrowWhenParsingUnknownEntry()
		 public virtual void ShouldThrowWhenParsingUnknownEntry()
		 {
			  // when
			  _version.entryParser( ( sbyte )42 ); // unused, at lest for now

			  // then
			  // it should throw exception
		 }
	}

}