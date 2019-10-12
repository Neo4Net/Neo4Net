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
namespace Neo4Net.causalclustering.catchup.tx
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;


	using State = Neo4Net.causalclustering.catchup.CatchupServerProtocol.State;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using Neo4Net.Cursors;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using NoSuchTransactionException = Neo4Net.Kernel.impl.transaction.log.NoSuchTransactionException;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_INVALID_REQUEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_STORE_ID_MISMATCH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_STORE_UNAVAILABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_TRANSACTION_PRUNED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;

	public class TxPullRequestHandler : SimpleChannelInboundHandler<TxPullRequest>
	{
		 private readonly CatchupServerProtocol _protocol;
		 private readonly System.Func<StoreId> _storeIdSupplier;
		 private readonly System.Func<bool> _databaseAvailable;
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly LogicalTransactionStore _logicalTransactionStore;
		 private readonly TxPullRequestsMonitor _monitor;
		 private readonly Log _log;

		 public TxPullRequestHandler( CatchupServerProtocol protocol, System.Func<StoreId> storeIdSupplier, System.Func<bool> databaseAvailable, System.Func<NeoStoreDataSource> dataSourceSupplier, Monitors monitors, LogProvider logProvider )
		 {
			  this._protocol = protocol;
			  this._storeIdSupplier = storeIdSupplier;
			  this._databaseAvailable = databaseAvailable;
			  DependencyResolver dependencies = dataSourceSupplier().DependencyResolver;
			  this._transactionIdStore = dependencies.ResolveDependency( typeof( TransactionIdStore ) );
			  this._logicalTransactionStore = dependencies.ResolveDependency( typeof( LogicalTransactionStore ) );
			  this._monitor = monitors.NewMonitor( typeof( TxPullRequestsMonitor ) );
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void channelRead0(io.netty.channel.ChannelHandlerContext ctx, final TxPullRequest msg) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 protected internal override void ChannelRead0( ChannelHandlerContext ctx, TxPullRequest msg )
		 {
			  _monitor.increment();

			  if ( msg.PreviousTxId() <= 0 )
			  {
					_log.error( "Illegal tx pull request" );
					EndInteraction( ctx, E_INVALID_REQUEST, -1 );
					return;
			  }

			  StoreId localStoreId = _storeIdSupplier.get();
			  StoreId expectedStoreId = msg.ExpectedStoreId();

			  long firstTxId = msg.PreviousTxId() + 1;

			  /*
			   * This is the minimum transaction id we must send to consider our streaming operation successful. The kernel can
			   * concurrently prune even future transactions while iterating and the cursor will silently fail on iteration, so
			   * we need to add our own protection for this reason and also as a generally important sanity check for the fulfillment
			   * of the consistent recovery contract which requires us to stream transactions at least as far as the time when the
			   * file copy operation completed.
			   */
			  long txIdPromise = _transactionIdStore.LastCommittedTransactionId;
			  IOCursor<CommittedTransactionRepresentation> txCursor = GetCursor( txIdPromise, ctx, firstTxId, localStoreId, expectedStoreId );

			  if ( txCursor != null )
			  {
					ChunkedTransactionStream txStream = new ChunkedTransactionStream( _log, localStoreId, firstTxId, txIdPromise, txCursor, _protocol );
					// chunked transaction stream ends the interaction internally and closes the cursor
					ctx.writeAndFlush( txStream ).addListener(f =>
					{
					 if ( _log.DebugEnabled || !f.Success )
					 {
						  string message = format( "Streamed transactions [%d--%d] to %s", firstTxId, txStream.LastTxId(), ctx.channel().remoteAddress() );
						  if ( f.Success )
						  {
								_log.debug( message );
						  }
						  else
						  {
								_log.warn( message, f.cause() );
						  }
					 }
					});
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.cursor.IOCursor<org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation> getCursor(long txIdPromise, io.netty.channel.ChannelHandlerContext ctx, long firstTxId, org.neo4j.causalclustering.identity.StoreId localStoreId, org.neo4j.causalclustering.identity.StoreId expectedStoreId) throws java.io.IOException
		 private IOCursor<CommittedTransactionRepresentation> GetCursor( long txIdPromise, ChannelHandlerContext ctx, long firstTxId, StoreId localStoreId, StoreId expectedStoreId )
		 {
			  if ( localStoreId == null || !localStoreId.Equals( expectedStoreId ) )
			  {
					_log.info( "Failed to serve TxPullRequest for tx %d and storeId %s because that storeId is different " + "from this machine with %s", firstTxId, expectedStoreId, localStoreId );
					EndInteraction( ctx, E_STORE_ID_MISMATCH, txIdPromise );
					return null;
			  }
			  else if ( !_databaseAvailable.AsBoolean )
			  {
					_log.info( "Failed to serve TxPullRequest for tx %d because the local database is unavailable.", firstTxId );
					EndInteraction( ctx, E_STORE_UNAVAILABLE, txIdPromise );
					return null;
			  }
			  else if ( txIdPromise < firstTxId )
			  {
					EndInteraction( ctx, SUCCESS_END_OF_STREAM, txIdPromise );
					return null;
			  }

			  try
			  {
					return _logicalTransactionStore.getTransactions( firstTxId );
			  }
			  catch ( NoSuchTransactionException )
			  {
					_log.info( "Failed to serve TxPullRequest for tx %d because the transaction does not exist.", firstTxId );
					EndInteraction( ctx, E_TRANSACTION_PRUNED, txIdPromise );
					return null;
			  }
		 }

		 private void EndInteraction( ChannelHandlerContext ctx, CatchupResult status, long lastCommittedTransactionId )
		 {
			  ctx.write( ResponseMessageType.TX_STREAM_FINISHED );
			  ctx.writeAndFlush( new TxStreamFinishedResponse( status, lastCommittedTransactionId ) );
			  _protocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
		 }
	}

}