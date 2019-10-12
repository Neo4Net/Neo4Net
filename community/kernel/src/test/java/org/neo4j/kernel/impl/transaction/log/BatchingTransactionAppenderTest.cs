using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.transaction.log
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using NodeCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.NodeCommand;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFile = Org.Neo4j.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using LogAppendEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogCheckPointEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogCheckPointEvent;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.rotation.LogRotation.NO_ROTATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IdOrderingQueue.BYPASS;

	public class BatchingTransactionAppenderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.kernel.lifecycle.LifeRule life = new org.neo4j.kernel.lifecycle.LifeRule(true);
		 public readonly LifeRule Life = new LifeRule( true );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();

		 private readonly InMemoryVersionableReadableClosablePositionAwareChannel _channel = new InMemoryVersionableReadableClosablePositionAwareChannel();
		 private readonly LogAppendEvent _logAppendEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent_Fields.Null;
		 private readonly DatabaseHealth _databaseHealth = mock( typeof( DatabaseHealth ) );
		 private readonly LogFile _logFile = mock( typeof( LogFile ) );
		 private readonly LogFiles _logFiles = mock( typeof( TransactionLogFiles ) );
		 private readonly TransactionIdStore _transactionIdStore = mock( typeof( TransactionIdStore ) );
		 private readonly TransactionMetadataCache _positionCache = new TransactionMetadataCache();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  when( _logFiles.LogFile ).thenReturn( _logFile );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendSingleTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendSingleTransaction()
		 {
			  // GIVEN
			  when( _logFile.Writer ).thenReturn( _channel );
			  long txId = 15;
			  when( _transactionIdStore.nextCommittingTransactionId() ).thenReturn(txId);
			  TransactionAppender appender = Life.add( CreateTransactionAppender() );

			  // WHEN
			  TransactionRepresentation transaction = transaction( SingleCreateNodeCommand( 0 ), new sbyte[]{ 1, 2, 5 }, 2, 1, 12345, 4545, 12345 + 10 );

			  appender.Append( new TransactionToApply( transaction ), _logAppendEvent );

			  // THEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.entry.LogEntryReader<ReadableLogChannel> logEntryReader = new org.neo4j.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>();
			  LogEntryReader<ReadableLogChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableLogChannel>();
			  using ( PhysicalTransactionCursor<ReadableLogChannel> reader = new PhysicalTransactionCursor<ReadableLogChannel>( _channel, logEntryReader ) )
			  {
					reader.Next();
					TransactionRepresentation tx = reader.Get().TransactionRepresentation;
					assertArrayEquals( transaction.AdditionalHeader(), tx.AdditionalHeader() );
					assertEquals( transaction.MasterId, tx.MasterId );
					assertEquals( transaction.AuthorId, tx.AuthorId );
					assertEquals( transaction.TimeStarted, tx.TimeStarted );
					assertEquals( transaction.TimeCommitted, tx.TimeCommitted );
					assertEquals( transaction.LatestCommittedTxWhenStarted, tx.LatestCommittedTxWhenStarted );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendBatchOfTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendBatchOfTransactions()
		 {
			  // GIVEN
			  when( _logFile.Writer ).thenReturn( _channel );
			  TransactionAppender appender = Life.add( CreateTransactionAppender() );
			  when( _transactionIdStore.nextCommittingTransactionId() ).thenReturn(2L, 3L, 4L);
			  TransactionToApply batch = BatchOf( Transaction( SingleCreateNodeCommand( 0 ), new sbyte[0], 0, 0, 0, 1, 0 ), Transaction( SingleCreateNodeCommand( 1 ), new sbyte[0], 0, 0, 0, 1, 0 ), Transaction( SingleCreateNodeCommand( 2 ), new sbyte[0], 0, 0, 0, 1, 0 ) );

			  // WHEN
			  appender.Append( batch, _logAppendEvent );

			  // THEN
			  TransactionToApply tx = batch;
			  assertEquals( 2L, tx.TransactionId() );
			  tx = tx.Next();
			  assertEquals( 3L, tx.TransactionId() );
			  tx = tx.Next();
			  assertEquals( 4L, tx.TransactionId() );
			  assertNull( tx.Next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendCommittedTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendCommittedTransactions()
		 {
			  // GIVEN
			  when( _logFile.Writer ).thenReturn( _channel );
			  long nextTxId = 15;
			  when( _transactionIdStore.nextCommittingTransactionId() ).thenReturn(nextTxId);
			  TransactionAppender appender = Life.add( new BatchingTransactionAppender( _logFiles, NO_ROTATION, _positionCache, _transactionIdStore, BYPASS, _databaseHealth ) );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] additionalHeader = new byte[]{1, 2, 5};
			  sbyte[] additionalHeader = new sbyte[]{ 1, 2, 5 };
			  const int masterId = 2;
			  int authorId = 1;
			  const long timeStarted = 12345;
			  long latestCommittedTxWhenStarted = nextTxId - 5;
			  long timeCommitted = timeStarted + 10;
			  PhysicalTransactionRepresentation transactionRepresentation = new PhysicalTransactionRepresentation( SingleCreateNodeCommand( 0 ) );
			  transactionRepresentation.SetHeader( additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted, -1 );

			  LogEntryStart start = new LogEntryStart( 0, 0, 0L, latestCommittedTxWhenStarted, null, LogPosition.UNSPECIFIED );
			  LogEntryCommit commit = new LogEntryCommit( nextTxId, 0L );
			  CommittedTransactionRepresentation transaction = new CommittedTransactionRepresentation( start, transactionRepresentation, commit );

			  appender.Append( new TransactionToApply( transactionRepresentation, transaction.CommitEntry.TxId ), _logAppendEvent );

			  // THEN
			  LogEntryReader<ReadableLogChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableLogChannel>();
			  using ( PhysicalTransactionCursor<ReadableLogChannel> reader = new PhysicalTransactionCursor<ReadableLogChannel>( _channel, logEntryReader ) )
			  {
					reader.Next();
					TransactionRepresentation result = reader.Get().TransactionRepresentation;
					assertArrayEquals( additionalHeader, result.AdditionalHeader() );
					assertEquals( masterId, result.MasterId );
					assertEquals( authorId, result.AuthorId );
					assertEquals( timeStarted, result.TimeStarted );
					assertEquals( timeCommitted, result.TimeCommitted );
					assertEquals( latestCommittedTxWhenStarted, result.LatestCommittedTxWhenStarted );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAppendCommittedTransactionsWhenTooFarAhead()
		 public virtual void ShouldNotAppendCommittedTransactionsWhenTooFarAhead()
		 {
			  // GIVEN
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  when( _logFile.Writer ).thenReturn( channel );
			  TransactionAppender appender = Life.add( CreateTransactionAppender() );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] additionalHeader = new byte[]{1, 2, 5};
			  sbyte[] additionalHeader = new sbyte[]{ 1, 2, 5 };
			  const int masterId = 2;
			  int authorId = 1;
			  const long timeStarted = 12345;
			  long latestCommittedTxWhenStarted = 4545;
			  long timeCommitted = timeStarted + 10;
			  PhysicalTransactionRepresentation transactionRepresentation = new PhysicalTransactionRepresentation( SingleCreateNodeCommand( 0 ) );
			  transactionRepresentation.SetHeader( additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted, -1 );

			  when( _transactionIdStore.LastCommittedTransactionId ).thenReturn( latestCommittedTxWhenStarted );

			  LogEntryStart start = new LogEntryStart( 0, 0, 0L, latestCommittedTxWhenStarted, null, LogPosition.UNSPECIFIED );
			  LogEntryCommit commit = new LogEntryCommit( latestCommittedTxWhenStarted + 2, 0L );
			  CommittedTransactionRepresentation transaction = new CommittedTransactionRepresentation( start, transactionRepresentation, commit );

			  try
			  {
					appender.Append( new TransactionToApply( transaction.TransactionRepresentation, transaction.CommitEntry.TxId ), _logAppendEvent );
					fail( "should have thrown" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.Message, containsString( "to be applied, but appending it ended up generating an" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCallTransactionClosedOnFailedAppendedTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCallTransactionClosedOnFailedAppendedTransaction()
		 {
			  // GIVEN
			  long txId = 3;
			  string failureMessage = "Forces a failure";
			  FlushablePositionAwareChannel channel = spy( new PositionAwarePhysicalFlushableChannel( mock( typeof( PhysicalLogVersionedStoreChannel ) ) ) );
			  IOException failure = new IOException( failureMessage );
			  when( channel.PutInt( anyInt() ) ).thenThrow(failure);
			  when( _logFile.Writer ).thenReturn( channel );
			  when( _transactionIdStore.nextCommittingTransactionId() ).thenReturn(txId);
			  Mockito.reset( _databaseHealth );
			  TransactionAppender appender = Life.add( CreateTransactionAppender() );

			  // WHEN
			  TransactionRepresentation transaction = mock( typeof( TransactionRepresentation ) );
			  when( transaction.AdditionalHeader() ).thenReturn(new sbyte[0]);
			  try
			  {
					appender.Append( new TransactionToApply( transaction ), _logAppendEvent );
					fail( "Expected append to fail. Something is wrong with the test itself" );
			  }
			  catch ( IOException e )
			  {
					// THEN
					assertSame( failure, e );
					verify( _transactionIdStore, times( 1 ) ).nextCommittingTransactionId();
					verify( _transactionIdStore, never() ).transactionClosed(eq(txId), anyLong(), anyLong());
					verify( _databaseHealth ).panic( failure );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCallTransactionClosedOnFailedForceLogToDisk() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCallTransactionClosedOnFailedForceLogToDisk()
		 {
			  // GIVEN
			  long txId = 3;
			  string failureMessage = "Forces a failure";
			  FlushablePositionAwareChannel channel = spy( new InMemoryClosableChannel() );
			  IOException failure = new IOException( failureMessage );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.Flushable flushable = mock(java.io.Flushable.class);
			  Flushable flushable = mock( typeof( Flushable ) );
			  doAnswer(invocation =>
			  {
				invocation.callRealMethod();
				return flushable;
			  }).when( channel ).prepareForFlush();
			  doThrow( failure ).when( flushable ).flush();
			  when( _logFile.Writer ).thenReturn( channel );
			  TransactionMetadataCache metadataCache = new TransactionMetadataCache();
			  TransactionIdStore transactionIdStore = mock( typeof( TransactionIdStore ) );
			  when( transactionIdStore.NextCommittingTransactionId() ).thenReturn(txId);
			  Mockito.reset( _databaseHealth );
			  TransactionAppender appender = Life.add( new BatchingTransactionAppender( _logFiles, NO_ROTATION, metadataCache, transactionIdStore, BYPASS, _databaseHealth ) );

			  // WHEN
			  TransactionRepresentation transaction = mock( typeof( TransactionRepresentation ) );
			  when( transaction.AdditionalHeader() ).thenReturn(new sbyte[0]);
			  try
			  {
					appender.Append( new TransactionToApply( transaction ), _logAppendEvent );
					fail( "Expected append to fail. Something is wrong with the test itself" );
			  }
			  catch ( IOException e )
			  {
					// THEN
					assertSame( failure, e );
					verify( transactionIdStore, times( 1 ) ).nextCommittingTransactionId();
					verify( transactionIdStore, never() ).transactionClosed(eq(txId), anyLong(), anyLong());
					verify( _databaseHealth ).panic( failure );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteACheckPoint() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToWriteACheckPoint()
		 {
			  // Given
			  FlushablePositionAwareChannel channel = mock( typeof( FlushablePositionAwareChannel ), RETURNS_MOCKS );
			  Flushable flushable = mock( typeof( Flushable ) );
			  when( channel.PrepareForFlush() ).thenReturn(flushable);
			  when( channel.PutLong( anyLong() ) ).thenReturn(channel);
			  when( _logFile.Writer ).thenReturn( channel );
			  BatchingTransactionAppender appender = Life.add( CreateTransactionAppender() );

			  // When
			  appender.CheckPoint( new LogPosition( 1L, 2L ), LogCheckPointEvent.NULL );

			  // Then
			  verify( channel, times( 1 ) ).putLong( 1L );
			  verify( channel, times( 1 ) ).putLong( 2L );
			  verify( channel, times( 1 ) ).prepareForFlush();
			  verify( flushable, times( 1 ) ).flush();
			  verify( _databaseHealth, never() ).panic(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKernelPanicIfNotAbleToWriteACheckPoint() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKernelPanicIfNotAbleToWriteACheckPoint()
		 {
			  // Given
			  IOException ioex = new IOException( "boom!" );
			  FlushablePositionAwareChannel channel = mock( typeof( FlushablePositionAwareChannel ), RETURNS_MOCKS );
			  when( channel.Put( anyByte() ) ).thenReturn(channel);
			  when( channel.PutLong( anyLong() ) ).thenThrow(ioex);
			  when( channel.Put( anyByte() ) ).thenThrow(ioex);
			  when( _logFile.Writer ).thenReturn( channel );
			  BatchingTransactionAppender appender = Life.add( CreateTransactionAppender() );

			  // When
			  try
			  {
					appender.CheckPoint( new LogPosition( 0L, 0L ), LogCheckPointEvent.NULL );
					fail( "should have thrown " );
			  }
			  catch ( IOException ex )
			  {
					assertEquals( ioex, ex );
			  }

			  // Then
			  verify( _databaseHealth, times( 1 ) ).panic( ioex );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKernelPanicIfTransactionIdsMismatch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKernelPanicIfTransactionIdsMismatch()
		 {
			  // Given
			  BatchingTransactionAppender appender = Life.add( CreateTransactionAppender() );
			  when( _transactionIdStore.nextCommittingTransactionId() ).thenReturn(42L);
			  TransactionToApply batch = new TransactionToApply( mock( typeof( TransactionRepresentation ) ), 43L );

			  // When
			  try
			  {
					appender.Append( batch, Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent_Fields.Null );
					fail( "should have thrown " );
			  }
			  catch ( System.InvalidOperationException ex )
			  {
					// Then
					verify( _databaseHealth, times( 1 ) ).panic( ex );
			  }

		 }

		 private BatchingTransactionAppender CreateTransactionAppender()
		 {
			  return new BatchingTransactionAppender( _logFiles, NO_ROTATION, _positionCache, _transactionIdStore, BYPASS, _databaseHealth );
		 }

		 private TransactionRepresentation Transaction( ICollection<StorageCommand> commands, sbyte[] additionalHeader, int masterId, int authorId, long timeStarted, long latestCommittedTxWhenStarted, long timeCommitted )
		 {
			  PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( commands );
			  tx.SetHeader( additionalHeader, masterId, authorId, timeStarted, latestCommittedTxWhenStarted, timeCommitted, -1 );
			  return tx;
		 }

		 private ICollection<StorageCommand> SingleCreateNodeCommand( long id )
		 {
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  NodeRecord before = new NodeRecord( id );
			  NodeRecord after = new NodeRecord( id );
			  after.InUse = true;
			  commands.Add( new NodeCommand( before, after ) );
			  return commands;
		 }

		 private TransactionToApply BatchOf( params TransactionRepresentation[] transactions )
		 {
			  TransactionToApply first = null;
			  TransactionToApply last = null;
			  foreach ( TransactionRepresentation transaction in transactions )
			  {
					TransactionToApply tx = new TransactionToApply( transaction );
					if ( first == null )
					{
						 first = last = tx;
					}
					else
					{
						 last.Next( tx );
						 last = tx;
					}
			  }
			  return first;
		 }
	}

}