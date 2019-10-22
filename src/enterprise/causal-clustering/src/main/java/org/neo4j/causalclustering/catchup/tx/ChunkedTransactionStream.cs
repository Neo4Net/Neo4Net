using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.tx
{
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChunkedInput = io.netty.handler.stream.ChunkedInput;

	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using Neo4Net.Cursors;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.catchup.CatchupResult.E_TRANSACTION_PRUNED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;

	/// <summary>
	/// Returns a chunked stream of transactions.
	/// </summary>
	public class ChunkedTransactionStream : ChunkedInput<object>
	{
		 private readonly Log _log;
		 private readonly StoreId _storeId;
		 private readonly IOCursor<CommittedTransactionRepresentation> _txCursor;
		 private readonly CatchupServerProtocol _protocol;
		 private readonly long _txIdPromise;

		 private bool _endOfInput;
		 private bool _noMoreTransactions;
		 private long _expectedTxId;
		 private long _lastTxId;

		 private object _pending;

		 internal ChunkedTransactionStream( Log log, StoreId storeId, long firstTxId, long txIdPromise, IOCursor<CommittedTransactionRepresentation> txCursor, CatchupServerProtocol protocol )
		 {
			  this._log = log;
			  this._storeId = storeId;
			  this._expectedTxId = firstTxId;
			  this._txIdPromise = txIdPromise;
			  this._txCursor = txCursor;
			  this._protocol = protocol;
		 }

		 public override bool EndOfInput
		 {
			 get
			 {
				  return _endOfInput;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _txCursor.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object readChunk(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		 public override object ReadChunk( ChannelHandlerContext ctx )
		 {
			  return ReadChunk( ctx.alloc() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object readChunk(io.netty.buffer.ByteBufAllocator allocator) throws Exception
		 public override object ReadChunk( ByteBufAllocator allocator )
		 {
			  Debug.Assert( !_endOfInput );

			  if ( _pending != null )
			  {
					if ( _noMoreTransactions )
					{
						 _endOfInput = true;
					}

					return ConsumePending();
			  }
			  else if ( _noMoreTransactions )
			  {
					/* finalization should always have a last ending message */
					throw new System.InvalidOperationException();
			  }
			  else if ( _txCursor.next() )
			  {
					Debug.Assert( _pending == null );

					CommittedTransactionRepresentation tx = _txCursor.get();
					_lastTxId = tx.CommitEntry.TxId;
					if ( _lastTxId != _expectedTxId )
					{
						 string msg = format( "Transaction cursor out of order. Expected %d but was %d", _expectedTxId, _lastTxId );
						 throw new System.InvalidOperationException( msg );
					}
					_expectedTxId++;
					_pending = new TxPullResponse( _storeId, tx );
					return ResponseMessageType.TX;
			  }
			  else
			  {
					Debug.Assert( _pending == null );

					_noMoreTransactions = true;
					_protocol.expect( CatchupServerProtocol.State.MESSAGE_TYPE );
					CatchupResult result;
					if ( _lastTxId >= _txIdPromise )
					{
						 result = SUCCESS_END_OF_STREAM;
					}
					else
					{
						 result = E_TRANSACTION_PRUNED;
						 _log.warn( "Transaction cursor fell short. Expected at least %d but only got to %d.", _txIdPromise, _lastTxId );
					}
					_pending = new TxStreamFinishedResponse( result, _lastTxId );
					return ResponseMessageType.TX_STREAM_FINISHED;
			  }
		 }

		 private object ConsumePending()
		 {
			  object prevPending = _pending;
			  _pending = null;
			  return prevPending;
		 }

		 public override long Length()
		 {
			  return -1;
		 }

		 public override long Progress()
		 {
			  return 0;
		 }

		 public virtual long LastTxId()
		 {
			  return _lastTxId;
		 }
	}

}