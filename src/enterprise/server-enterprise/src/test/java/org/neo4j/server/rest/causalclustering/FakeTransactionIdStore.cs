using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Server.rest.causalclustering
{

	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;

	internal class FakeTransactionIdStore : TransactionIdStore
	{

		 private long _transactionId;
		 private long _checksum;
		 private long _commitTimestamp;

		 internal FakeTransactionIdStore()
		 {
		 }

		 public override long NextCommittingTransactionId()
		 {
			  return _transactionId;
		 }

		 public override long CommittingTransactionId()
		 {
			  return _transactionId;
		 }

		 public override void TransactionCommitted( long transactionId, long checksum, long commitTimestamp )
		 {
			  this._transactionId = transactionId;
			  this._checksum = checksum;
			  this._commitTimestamp = commitTimestamp;
		 }

		 public virtual long LastCommittedTransactionId
		 {
			 get
			 {
				  return _transactionId;
			 }
		 }

		 public virtual TransactionId LastCommittedTransaction
		 {
			 get
			 {
				  return new TransactionId( _transactionId, _checksum, _commitTimestamp );
			 }
		 }

		 public virtual TransactionId UpgradeTransaction
		 {
			 get
			 {
				  return new TransactionId( _transactionId, _checksum, _commitTimestamp );
			 }
		 }

		 public virtual long LastClosedTransactionId
		 {
			 get
			 {
				  return _transactionId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitClosedTransactionId(long txId, long timeoutMillis) throws InterruptedException, java.util.concurrent.TimeoutException
		 public override void AwaitClosedTransactionId( long txId, long timeoutMillis )
		 {
		 }

		 public virtual long[] LastClosedTransaction
		 {
			 get
			 {
				  throw new Exception( "Unimplemented" );
			 }
		 }

		 public override void SetLastCommittedAndClosedTransactionId( long transactionId, long checksum, long commitTimestamp, long byteOffset, long logVersion )
		 {
			  this._transactionId = transactionId;
			  this._checksum = checksum;
			  this._commitTimestamp = commitTimestamp;
		 }

		 public override void TransactionClosed( long transactionId, long logVersion, long byteOffset )
		 {
			  throw new Exception( "Unimplemented" );
		 }

		 public override void Flush()
		 {
		 }
	}

}