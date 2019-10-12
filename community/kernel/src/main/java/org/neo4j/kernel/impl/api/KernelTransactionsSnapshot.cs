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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using KernelTransactionHandle = Org.Neo4j.Kernel.api.KernelTransactionHandle;

	/// <summary>
	/// An instance of this class can get a snapshot of all currently running transactions and be able to tell
	/// later if all transactions which were running when it was constructed have closed.
	/// <para>
	/// Creating a snapshot creates a list and one additional book keeping object per open transaction.
	/// No thread doing normal transaction work should create snapshots, only threads that monitor transactions.
	/// </para>
	/// </summary>
	public class KernelTransactionsSnapshot
	{
		 private Tx _relevantTransactions;
		 private readonly long _snapshotTime;

		 public KernelTransactionsSnapshot( ISet<KernelTransactionHandle> allTransactions, long snapshotTime )
		 {
			  Tx head = null;
			  foreach ( KernelTransactionHandle tx in allTransactions )
			  {
					if ( tx.Open )
					{
						 Tx current = new Tx( tx );
						 if ( head != null )
						 {
							  current.Next = head;
							  head = current;
						 }
						 else
						 {
							  head = current;
						 }
					}
			  }
			  _relevantTransactions = head;
			  this._snapshotTime = snapshotTime;
		 }

		 public virtual bool AllClosed()
		 {
			  while ( _relevantTransactions != null )
			  {
					if ( !_relevantTransactions.haveClosed() )
					{
						 // At least one transaction hasn't closed yet
						 return false;
					}

					// This transaction has been closed, unlink so we don't have to check it the next time
					_relevantTransactions = _relevantTransactions.next;
			  }

			  // All transactions have been closed
			  return true;
		 }

		 public virtual long SnapshotTime()
		 {
			  return _snapshotTime;
		 }

		 private class Tx
		 {
			  internal readonly KernelTransactionHandle Transaction;
			  internal Tx Next;

			  internal Tx( KernelTransactionHandle tx )
			  {
					this.Transaction = tx;
			  }

			  internal virtual bool HaveClosed()
			  {
					return !Transaction.Open;
			  }
		 }
	}

}