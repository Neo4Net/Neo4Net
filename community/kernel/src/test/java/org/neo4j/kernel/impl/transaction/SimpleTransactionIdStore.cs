using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.impl.transaction
{

	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using TransactionId = Org.Neo4j.Kernel.impl.store.TransactionId;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using ArrayQueueOutOfOrderSequence = Org.Neo4j.Kernel.impl.util.ArrayQueueOutOfOrderSequence;
	using OutOfOrderSequence = Org.Neo4j.Kernel.impl.util.OutOfOrderSequence;

	/// <summary>
	/// Duplicates the <seealso cref="TransactionIdStore"/> parts of <seealso cref="NeoStores"/>, which is somewhat bad to have to keep
	/// in sync.
	/// </summary>
	public class SimpleTransactionIdStore : TransactionIdStore
	{
		 private readonly AtomicLong _committingTransactionId = new AtomicLong();
		 private readonly OutOfOrderSequence _closedTransactionId = new ArrayQueueOutOfOrderSequence( -1, 100, new long[1] );
		 private readonly AtomicReference<TransactionId> _committedTransactionId = new AtomicReference<TransactionId>( new TransactionId( log.TransactionIdStore_Fields.BASE_TX_ID, log.TransactionIdStore_Fields.BASE_TX_CHECKSUM, log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP ) );
		 private readonly long _previouslyCommittedTxId;
		 private readonly long _initialTransactionChecksum;
		 private readonly long _previouslyCommittedTxCommitTimestamp;

		 public SimpleTransactionIdStore() : this(log.TransactionIdStore_Fields.BASE_TX_ID, 0, log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP, log.TransactionIdStore_Fields.BASE_TX_LOG_VERSION, log.TransactionIdStore_Fields.BaseTxLogByteOffset)
		 {
		 }

		 public SimpleTransactionIdStore( long previouslyCommittedTxId, long checksum, long previouslyCommittedTxCommitTimestamp, long previouslyCommittedTxLogVersion, long previouslyCommittedTxLogByteOffset )
		 {
			  Debug.Assert( previouslyCommittedTxId >= log.TransactionIdStore_Fields.BASE_TX_ID, "cannot start from a tx id less than BASE_TX_ID" );
			  SetLastCommittedAndClosedTransactionId( previouslyCommittedTxId, checksum, previouslyCommittedTxCommitTimestamp, previouslyCommittedTxLogByteOffset, previouslyCommittedTxLogVersion );
			  this._previouslyCommittedTxId = previouslyCommittedTxId;
			  this._initialTransactionChecksum = checksum;
			  this._previouslyCommittedTxCommitTimestamp = previouslyCommittedTxCommitTimestamp;
		 }

		 public override long NextCommittingTransactionId()
		 {
			  return _committingTransactionId.incrementAndGet();
		 }

		 public override long CommittingTransactionId()
		 {
			  return _committingTransactionId.get();
		 }

		 public override void TransactionCommitted( long transactionId, long checksum, long commitTimestamp )
		 {
			 lock ( this )
			 {
				  TransactionId current = _committedTransactionId.get();
				  if ( current == null || transactionId > current.TransactionIdConflict() )
				  {
						_committedTransactionId.set( new TransactionId( transactionId, checksum, commitTimestamp ) );
				  }
			 }
		 }

		 public virtual long LastCommittedTransactionId
		 {
			 get
			 {
				  return _committedTransactionId.get().transactionId();
			 }
		 }

		 public virtual TransactionId LastCommittedTransaction
		 {
			 get
			 {
				  return _committedTransactionId.get();
			 }
		 }

		 public virtual TransactionId UpgradeTransaction
		 {
			 get
			 {
				  return new TransactionId( _previouslyCommittedTxId, _initialTransactionChecksum, _previouslyCommittedTxCommitTimestamp );
			 }
		 }

		 public virtual long LastClosedTransactionId
		 {
			 get
			 {
				  return _closedTransactionId.HighestGapFreeNumber;
			 }
		 }

		 public override void AwaitClosedTransactionId( long txId, long timeoutMillis )
		 {
			  throw new System.NotSupportedException( "Not implemented" );
		 }

		 public virtual long[] LastClosedTransaction
		 {
			 get
			 {
				  return _closedTransactionId.get();
			 }
		 }

		 public override void SetLastCommittedAndClosedTransactionId( long transactionId, long checksum, long commitTimestamp, long byteOffset, long logVersion )
		 {
			  _committingTransactionId.set( transactionId );
			  _committedTransactionId.set( new TransactionId( transactionId, checksum, commitTimestamp ) );
			  _closedTransactionId.set( transactionId, new long[]{ logVersion, byteOffset } );
		 }

		 public override void TransactionClosed( long transactionId, long logVersion, long byteOffset )
		 {
			  _closedTransactionId.offer( transactionId, new long[]{ logVersion, byteOffset } );
		 }

		 public override void Flush()
		 {
		 }
	}

}