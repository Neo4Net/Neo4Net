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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Test = org.junit.Test;

	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TransactionPositionLocatorTest
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionPositionLocatorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_start = new LogEntryStart( 0, 0, 0, 0, null, _startPosition );
			_commit = new LogEntryCommit( _txId, DateTimeHelper.CurrentUnixTimeMillis() );
		}

		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _logEntryReader = mock( typeof( LogEntryReader ) );
		 private readonly ReadableClosablePositionAwareChannel _channel = mock( typeof( ReadableClosablePositionAwareChannel ) );
		 private readonly TransactionMetadataCache _metadataCache = mock( typeof( TransactionMetadataCache ) );

		 private readonly long _txId = 42;
		 private readonly LogPosition _startPosition = new LogPosition( 1, 128 );

		 private LogEntryStart _start;
		 private readonly LogEntryCommand _command = new LogEntryCommand( new Command.NodeCommand( new NodeRecord( 42 ), new NodeRecord( 42 ) ) );
		 private LogEntryCommit _commit;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindTransactionLogPosition() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindTransactionLogPosition()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PhysicalLogicalTransactionStore.TransactionPositionLocator locator = new PhysicalLogicalTransactionStore.TransactionPositionLocator(txId, logEntryReader);
			  PhysicalLogicalTransactionStore.TransactionPositionLocator locator = new PhysicalLogicalTransactionStore.TransactionPositionLocator( _txId, _logEntryReader );

			  when( _logEntryReader.readLogEntry( _channel ) ).thenReturn( _start, _command, _commit, null );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = locator.visit(channel);
			  bool result = locator.Visit( _channel );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogPosition position = locator.getAndCacheFoundLogPosition(metadataCache);
			  LogPosition position = locator.GetAndCacheFoundLogPosition( _metadataCache );

			  // then
			  assertFalse( result );
			  assertEquals( _startPosition, position );
			  verify( _metadataCache, times( 1 ) ).cacheTransactionMetadata( _txId, _startPosition, _start.MasterId, _start.LocalId, LogEntryStart.checksum( _start ), _commit.TimeWritten );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindTransactionLogPosition() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindTransactionLogPosition()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PhysicalLogicalTransactionStore.TransactionPositionLocator locator = new PhysicalLogicalTransactionStore.TransactionPositionLocator(txId, logEntryReader);
			  PhysicalLogicalTransactionStore.TransactionPositionLocator locator = new PhysicalLogicalTransactionStore.TransactionPositionLocator( _txId, _logEntryReader );

			  when( _logEntryReader.readLogEntry( _channel ) ).thenReturn( _start, _command, null );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = locator.visit(channel);
			  bool result = locator.Visit( _channel );

			  // then
			  assertTrue( result );
			  try
			  {
					locator.GetAndCacheFoundLogPosition( _metadataCache );
					fail( "should have thrown" );
			  }
			  catch ( NoSuchTransactionException e )
			  {
					assertEquals( "Unable to find transaction " + _txId + " in any of my logical logs", e.Message );
			  }
		 }
	}

}