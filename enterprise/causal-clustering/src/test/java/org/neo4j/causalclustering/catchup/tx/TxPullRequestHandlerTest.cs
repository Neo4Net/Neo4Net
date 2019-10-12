/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.catchup.tx
{
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using Org.Neo4j.Cursor;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using Commands = Org.Neo4j.Kernel.impl.transaction.command.Commands;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using NoSuchTransactionException = Org.Neo4j.Kernel.impl.transaction.log.NoSuchTransactionException;
	using TransactionCursor = Org.Neo4j.Kernel.impl.transaction.log.TransactionCursor;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_STORE_ID_MISMATCH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_STORE_UNAVAILABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_TRANSACTION_PRUNED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.state.StubCursors.cursor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.Commands.createNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class TxPullRequestHandlerTest
	{
		 private readonly ChannelHandlerContext _context = mock( typeof( ChannelHandlerContext ) );
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();

		 private StoreId _storeId = new StoreId( 1, 2, 3, 4 );
		 private NeoStoreDataSource _datasource = mock( typeof( NeoStoreDataSource ) );
		 private LogicalTransactionStore _logicalTransactionStore = mock( typeof( LogicalTransactionStore ) );
		 private TransactionIdStore _transactionIdStore = mock( typeof( TransactionIdStore ) );

		 private TxPullRequestHandler _txPullRequestHandler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  DependencyResolver dependencyResolver = mock( typeof( DependencyResolver ) );
			  when( _datasource.DependencyResolver ).thenReturn( dependencyResolver );
			  when( dependencyResolver.ResolveDependency( typeof( LogicalTransactionStore ) ) ).thenReturn( _logicalTransactionStore );
			  when( dependencyResolver.ResolveDependency( typeof( TransactionIdStore ) ) ).thenReturn( _transactionIdStore );
			  when( _transactionIdStore.LastCommittedTransactionId ).thenReturn( 15L );
			  _txPullRequestHandler = new TxPullRequestHandler( new CatchupServerProtocol(), () => _storeId, () => true, () => _datasource, new Monitors(), _logProvider );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithCompleteStreamOfTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWithCompleteStreamOfTransactions()
		 {
			  // given
			  when( _transactionIdStore.LastCommittedTransactionId ).thenReturn( 15L );
			  when( _logicalTransactionStore.getTransactions( 14L ) ).thenReturn( TxCursor( cursor( Tx( 14 ), Tx( 15 ) ) ) );
			  ChannelFuture channelFuture = mock( typeof( ChannelFuture ) );
			  when( _context.writeAndFlush( any() ) ).thenReturn(channelFuture);

			  // when
			  _txPullRequestHandler.channelRead0( _context, new TxPullRequest( 13, _storeId ) );

			  // then
			  verify( _context ).writeAndFlush( isA( typeof( ChunkedTransactionStream ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithEndOfStreamIfThereAreNoTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWithEndOfStreamIfThereAreNoTransactions()
		 {
			  // given
			  when( _transactionIdStore.LastCommittedTransactionId ).thenReturn( 14L );

			  // when
			  _txPullRequestHandler.channelRead0( _context, new TxPullRequest( 14, _storeId ) );

			  // then
			  verify( _context ).write( ResponseMessageType.TX_STREAM_FINISHED );
			  verify( _context ).writeAndFlush( new TxStreamFinishedResponse( SUCCESS_END_OF_STREAM, 14L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithoutTransactionsIfTheyDoNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWithoutTransactionsIfTheyDoNotExist()
		 {
			  // given
			  when( _transactionIdStore.LastCommittedTransactionId ).thenReturn( 15L );
			  when( _logicalTransactionStore.getTransactions( 14L ) ).thenThrow( new NoSuchTransactionException( 14 ) );

			  // when
			  _txPullRequestHandler.channelRead0( _context, new TxPullRequest( 13, _storeId ) );

			  // then
			  verify( _context, never() ).write(isA(typeof(ChunkedTransactionStream)));
			  verify( _context, never() ).writeAndFlush(isA(typeof(ChunkedTransactionStream)));

			  verify( _context ).write( ResponseMessageType.TX_STREAM_FINISHED );
			  verify( _context ).writeAndFlush( new TxStreamFinishedResponse( E_TRANSACTION_PRUNED, 15L ) );
			  _logProvider.assertAtLeastOnce( inLog( typeof( TxPullRequestHandler ) ).info( "Failed to serve TxPullRequest for tx %d because the transaction does not exist.", 14L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStreamTxEntriesIfStoreIdMismatches() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotStreamTxEntriesIfStoreIdMismatches()
		 {
			  // given
			  StoreId serverStoreId = new StoreId( 1, 2, 3, 4 );
			  StoreId clientStoreId = new StoreId( 5, 6, 7, 8 );

			  TxPullRequestHandler txPullRequestHandler = new TxPullRequestHandler( new CatchupServerProtocol(), () => serverStoreId, () => true, () => _datasource, new Monitors(), _logProvider );

			  // when
			  txPullRequestHandler.ChannelRead0( _context, new TxPullRequest( 1, clientStoreId ) );

			  // then
			  verify( _context ).write( ResponseMessageType.TX_STREAM_FINISHED );
			  verify( _context ).writeAndFlush( new TxStreamFinishedResponse( E_STORE_ID_MISMATCH, 15L ) );
			  _logProvider.assertAtLeastOnce( inLog( typeof( TxPullRequestHandler ) ).info( "Failed to serve TxPullRequest for tx %d and storeId %s because that storeId is different " + "from this machine with %s", 2L, clientStoreId, serverStoreId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStreamTxsAndReportErrorIfTheLocalDatabaseIsNotAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotStreamTxsAndReportErrorIfTheLocalDatabaseIsNotAvailable()
		 {
			  // given
			  when( _transactionIdStore.LastCommittedTransactionId ).thenReturn( 15L );

			  TxPullRequestHandler txPullRequestHandler = new TxPullRequestHandler( new CatchupServerProtocol(), () => _storeId, () => false, () => _datasource, new Monitors(), _logProvider );

			  // when
			  txPullRequestHandler.ChannelRead0( _context, new TxPullRequest( 1, _storeId ) );

			  // then
			  verify( _context ).write( ResponseMessageType.TX_STREAM_FINISHED );
			  verify( _context ).writeAndFlush( new TxStreamFinishedResponse( E_STORE_UNAVAILABLE, 15L ) );
			  _logProvider.assertAtLeastOnce( inLog( typeof( TxPullRequestHandler ) ).info( "Failed to serve TxPullRequest for tx %d because the local database is unavailable.", 2L ) );
		 }

		 private static CommittedTransactionRepresentation Tx( int id )
		 {
			  return new CommittedTransactionRepresentation( new LogEntryStart( id, id, id, id - 1, new sbyte[]{}, LogPosition.UNSPECIFIED ), Commands.transactionRepresentation( createNode( 0 ) ), new LogEntryCommit( id, id ) );
		 }

		 private static TransactionCursor TxCursor( Cursor<CommittedTransactionRepresentation> cursor )
		 {
			  return new TransactionCursorAnonymousInnerClass( cursor );
		 }

		 private class TransactionCursorAnonymousInnerClass : TransactionCursor
		 {
			 private Cursor<CommittedTransactionRepresentation> _cursor;

			 public TransactionCursorAnonymousInnerClass( Cursor<CommittedTransactionRepresentation> cursor )
			 {
				 this._cursor = cursor;
			 }

			 public LogPosition position()
			 {
				  throw new System.NotSupportedException( "LogPosition does not apply when moving a generic cursor over a list of transactions" );
			 }

			 public bool next()
			 {
				  return _cursor.next();
			 }

			 public void close()
			 {
				  _cursor.close();
			 }

			 public CommittedTransactionRepresentation get()
			 {
				  return _cursor.get();
			 }
		 }
	}

}