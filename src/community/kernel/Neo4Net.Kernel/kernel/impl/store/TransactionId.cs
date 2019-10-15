﻿/*
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
namespace Neo4Net.Kernel.impl.store
{
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;

	/// <summary>
	/// Transaction id plus meta data that says something about its contents, for comparison.
	/// </summary>
	public class TransactionId
	{
		 private readonly long _transactionId;
		 private readonly long _checksum;
		 private readonly long _commitTimestamp;

		 public TransactionId( long transactionId, long checksum, long commitTimestamp )
		 {
			  this._transactionId = transactionId;
			  this._checksum = checksum;
			  this._commitTimestamp = commitTimestamp;
		 }

		 /// <summary>
		 /// Transaction id, generated by <seealso cref="TransactionIdStore.nextCommittingTransactionId()"/>,
		 /// here accessible after that transaction being committed.
		 /// </summary>
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public virtual long TransactionIdConflict()
		 {
			  return _transactionId;
		 }

		 /// <summary>
		 /// Commit timestamp. Timestamp when transaction with transactionId was committed.
		 /// </summary>
		 public virtual long CommitTimestamp()
		 {
			  return _commitTimestamp;
		 }

		 /// <summary>
		 /// Checksum of a transaction, generated from transaction meta data or its contents.
		 /// </summary>
		 public virtual long Checksum()
		 {
			  return _checksum;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  TransactionId that = ( TransactionId ) o;
			  return _transactionId == that._transactionId && _checksum == that._checksum && _commitTimestamp == that._commitTimestamp;
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _transactionId ^ ( ( long )( ( ulong )_transactionId >> 32 ) ) );
			  result = 31 * result + ( int )( _checksum ^ ( ( long )( ( ulong )_checksum >> 32 ) ) );
			  result = 31 * result + ( int )( _commitTimestamp ^ ( ( long )( ( ulong )_commitTimestamp >> 32 ) ) );
			  return result;
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "{" +
						 "transactionId=" + _transactionId +
						 ", checksum=" + _checksum +
						 ", commitTimestamp=" + _commitTimestamp +
						 '}';
		 }
	}

}