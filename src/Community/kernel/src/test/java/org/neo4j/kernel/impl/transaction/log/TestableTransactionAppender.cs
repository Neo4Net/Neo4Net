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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogCheckPointEvent = Neo4Net.Kernel.impl.transaction.tracing.LogCheckPointEvent;

	public class TestableTransactionAppender : TransactionAppender
	{
		 private readonly TransactionIdStore _transactionIdStore;

		 public TestableTransactionAppender( TransactionIdStore transactionIdStore )
		 {
			  this._transactionIdStore = transactionIdStore;
		 }

		 public override long Append( TransactionToApply batch, LogAppendEvent logAppendEvent )
		 {
			  long txId = TransactionIdStore_Fields.BASE_TX_ID;
			  while ( batch != null )
			  {
					txId = _transactionIdStore.nextCommittingTransactionId();
					batch.Commitment( new FakeCommitment( txId, _transactionIdStore ), txId );
					batch.Commitment().publishAsCommitted();
					batch = batch.Next();
			  }
			  return txId;
		 }

		 public override void CheckPoint( LogPosition logPosition, LogCheckPointEvent logCheckPointEvent )
		 {
		 }
	}

}