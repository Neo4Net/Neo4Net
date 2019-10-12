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
	internal class TransactionCommitment : Commitment
	{
		 private readonly bool _hasExplicitIndexChanges;
		 private readonly long _transactionId;
		 private readonly long _transactionChecksum;
		 private readonly long _transactionCommitTimestamp;
		 private readonly LogPosition _logPosition;
		 private readonly TransactionIdStore _transactionIdStore;
		 private bool _markedAsCommitted;

		 internal TransactionCommitment( bool hasExplicitIndexChanges, long transactionId, long transactionChecksum, long transactionCommitTimestamp, LogPosition logPosition, TransactionIdStore transactionIdStore )
		 {
			  this._hasExplicitIndexChanges = hasExplicitIndexChanges;
			  this._transactionId = transactionId;
			  this._transactionChecksum = transactionChecksum;
			  this._transactionCommitTimestamp = transactionCommitTimestamp;
			  this._logPosition = logPosition;
			  this._transactionIdStore = transactionIdStore;
		 }

		 public virtual LogPosition LogPosition()
		 {
			  return _logPosition;
		 }

		 public override void PublishAsCommitted()
		 {
			  _markedAsCommitted = true;
			  _transactionIdStore.transactionCommitted( _transactionId, _transactionChecksum, _transactionCommitTimestamp );
		 }

		 public override void PublishAsClosed()
		 {
			  _transactionIdStore.transactionClosed( _transactionId, _logPosition.LogVersion, _logPosition.ByteOffset );
		 }

		 public override bool MarkedAsCommitted()
		 {
			  return _markedAsCommitted;
		 }

		 public override bool HasExplicitIndexChanges()
		 {
			  return _hasExplicitIndexChanges;
		 }
	}

}