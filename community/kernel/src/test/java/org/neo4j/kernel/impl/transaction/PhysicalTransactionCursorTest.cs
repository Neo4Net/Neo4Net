﻿/*
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
namespace Org.Neo4j.Kernel.impl.transaction
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using Org.Neo4j.Kernel.impl.transaction.log;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using ReadableLogChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableLogChannel;
	using CheckPoint = Org.Neo4j.Kernel.impl.transaction.log.entry.CheckPoint;
	using LogEntry = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class PhysicalTransactionCursorTest
	{
		 private readonly ReadableLogChannel _channel = mock( typeof( ReadableLogChannel ), RETURNS_MOCKS );
		 private readonly LogEntryReader<ReadableLogChannel> _entryReader = mock( typeof( LogEntryReader ) );

		 private const LogEntry NULL_ENTRY = null;
		 private static readonly CheckPoint _aCheckPointEntry = new CheckPoint( LogPosition.UNSPECIFIED );
		 private static readonly LogEntryStart _aStartEntry = new LogEntryStart( 0, 0, 0L, 0L, null, LogPosition.UNSPECIFIED );
		 private static readonly LogEntryCommit _aCommitEntry = new LogEntryCommit( 42, 0 );
		 private static readonly LogEntryCommand _aCommandEntry = new LogEntryCommand( new Command.NodeCommand( new NodeRecord( 42 ), new NodeRecord( 42 ) ) );
		 private PhysicalTransactionCursor<ReadableLogChannel> _cursor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cursor = new PhysicalTransactionCursor<ReadableLogChannel>( _channel, _entryReader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTheUnderlyingChannel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseTheUnderlyingChannel()
		 {
			  // when
			  _cursor.close();

			  // then
			  verify( _channel, times( 1 ) ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseWhenThereAreNoEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseWhenThereAreNoEntries()
		 {
			  // given
			  when( _entryReader.readLogEntry( _channel ) ).thenReturn( NULL_ENTRY );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = cursor.next();
			  bool result = _cursor.next();

			  // then
			  assertFalse( result );
			  assertNull( _cursor.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseWhenThereIsAStartEntryButNoCommitEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseWhenThereIsAStartEntryButNoCommitEntries()
		 {
			  // given
			  when( _entryReader.readLogEntry( _channel ) ).thenReturn( _aStartEntry, NULL_ENTRY );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = cursor.next();
			  bool result = _cursor.next();

			  // then
			  assertFalse( result );
			  assertNull( _cursor.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallTheVisitorWithTheFoundTransaction() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallTheVisitorWithTheFoundTransaction()
		 {
			  // given
			  when( _entryReader.readLogEntry( _channel ) ).thenReturn( _aStartEntry, _aCommandEntry, _aCommitEntry );

			  // when
			  _cursor.next();

			  // then
			  PhysicalTransactionRepresentation txRepresentation = new PhysicalTransactionRepresentation( singletonList( _aCommandEntry.Command ) );
			  assertEquals( new CommittedTransactionRepresentation( _aStartEntry, txRepresentation, _aCommitEntry ), _cursor.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipCheckPoints() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipCheckPoints()
		 {
			  // given
			  when( _entryReader.readLogEntry( _channel ) ).thenReturn( _aCheckPointEntry, _aStartEntry, _aCommandEntry, _aCommitEntry, _aCheckPointEntry );

			  // when
			  _cursor.next();

			  // then
			  PhysicalTransactionRepresentation txRepresentation = new PhysicalTransactionRepresentation( singletonList( _aCommandEntry.Command ) );
			  assertEquals( new CommittedTransactionRepresentation( _aStartEntry, txRepresentation, _aCommitEntry ), _cursor.get() );
		 }
	}

}