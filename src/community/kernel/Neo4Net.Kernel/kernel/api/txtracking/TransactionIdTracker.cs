using System;
using System.Threading;

/*
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
namespace Neo4Net.Kernel.api.txtracking
{

	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	/// <summary>
	/// Facility to allow a user to run a query on different members of the cluster and ensure that a member is at least as
	/// up to date as the state it read or wrote elsewhere.
	/// </summary>
	public class TransactionIdTracker
	{
		 private readonly System.Func<TransactionIdStore> _transactionIdStoreSupplier;
		 private readonly AvailabilityGuard _databaseAvailabilityGuard;

		 public TransactionIdTracker( System.Func<TransactionIdStore> transactionIdStoreSupplier, AvailabilityGuard databaseAvailabilityGuard )
		 {
			  this._databaseAvailabilityGuard = databaseAvailabilityGuard;
			  this._transactionIdStoreSupplier = transactionIdStoreSupplier;
		 }

		 /// <summary>
		 /// Wait for a specific transaction (the Oldest Acceptable Transaction - OAT) to have been applied before
		 /// continuing. This method is useful in a clustered deployment, where different members of the cluster are expected
		 /// to apply transactions at slightly different times.
		 /// <para>
		 /// We assume the OAT will always have been applied on one member of the cluster, therefore it is sensible to wait
		 /// for it to be applied on this member.
		 /// </para>
		 /// <para>
		 /// The effect is either:
		 /// <ol>
		 ///     <li>If the transaction in question has already been applied, return immediately.
		 ///     This is the most common case because we expect the interval between dependent requests from the client
		 ///     to be longer than the replication lag between cluster members.</li>
		 ///     <li>The transaction has not yet been applied, block until the background replication process has applied it,
		 ///     or timeout.</li>
		 /// </ol>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="oldestAcceptableTxId"> id of the Oldest Acceptable Transaction (OAT) that must have been applied before
		 ///                             continuing work. </param>
		 /// <param name="timeout"> maximum duration to wait for OAT to be applied </param>
		 /// <exception cref="TransactionFailureException"> when OAT did not get applied within the given duration </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitUpToDate(long oldestAcceptableTxId, java.time.Duration timeout) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 public virtual void AwaitUpToDate( long oldestAcceptableTxId, Duration timeout )
		 {
			  if ( oldestAcceptableTxId <= BASE_TX_ID )
			  {
					return;
			  }

			  if ( !_databaseAvailabilityGuard.Available )
			  {
					throw new TransactionFailureException( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, "Database unavailable" );
			  }

			  try
			  {
					// await for the last closed transaction id to to have at least the expected value
					// it has to be "last closed" and not "last committed" becase all transactions before the expected one should also be committed
					TransactionIdStore().awaitClosedTransactionId(oldestAcceptableTxId, timeout.toMillis());
			  }
			  catch ( Exception e ) when ( e is InterruptedException || e is TimeoutException )
			  {
					if ( e is InterruptedException )
					{
						 Thread.CurrentThread.Interrupt();
					}

					throw new TransactionFailureException( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.InstanceStateChanged, e, "Database not up to the requested version: %d. Latest database version is %d", oldestAcceptableTxId, TransactionIdStore().LastClosedTransactionId );
			  }
		 }

		 private TransactionIdStore TransactionIdStore()
		 {
			  // We need to resolve this as late as possible in case the database has been restarted as part of store copy.
			  // This causes TransactionIdStore staleness and we could get a MetaDataStore closed exception.
			  // Ideally we'd fix this with some life cycle wizardry but not going to do that for now.
			  return _transactionIdStoreSupplier.get();
		 }

		 /// <summary>
		 /// Find the id of the Newest Encountered Transaction (NET) that could have been seen on this server.
		 /// We expect the returned id to be sent back the client and ultimately supplied to
		 /// <seealso cref="awaitUpToDate(long, Duration)"/> on this server, or on a different server in the cluster.
		 /// </summary>
		 /// <returns> id of the Newest Encountered Transaction (NET). </returns>
		 public virtual long NewestEncounteredTxId()
		 {
			  // return the "last committed" because it is the newest id
			  // "last closed" will return the last gap-free id, pottentially for some old transaction because there might be other committing transactions
			  return TransactionIdStore().LastCommittedTransactionId;
		 }
	}

}