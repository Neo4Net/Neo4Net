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
namespace Neo4Net.Kernel.impl.store
{

	/// <summary>
	/// Can accept offerings about <seealso cref="TransactionId"/>, but will always only keep the highest one,
	/// always available in <seealso cref="get()"/>.
	/// </summary>
	public class HighestTransactionId
	{
		 private readonly AtomicReference<TransactionId> _highest = new AtomicReference<TransactionId>();

		 public HighestTransactionId( long initialTransactionId, long initialChecksum, long commitTimestamp )
		 {
			  Set( initialTransactionId, initialChecksum, commitTimestamp );
		 }

		 /// <summary>
		 /// Offers a transaction id. Will be accepted if this is higher than the current highest.
		 /// This method is thread-safe.
		 /// </summary>
		 /// <param name="transactionId"> transaction id to compare for highest. </param>
		 /// <param name="checksum"> checksum of the transaction. </param>
		 /// <param name="commitTimestamp"> commit time for transaction with {@code transactionId}. </param>
		 /// <returns> {@code true} if the given transaction id was higher than the current highest,
		 /// {@code false}. </returns>
		 public virtual bool Offer( long transactionId, long checksum, long commitTimestamp )
		 {
			  TransactionId high = _highest.get();
			  if ( transactionId < high.TransactionIdConflict() )
			  { // a higher id has already been offered
					return false;
			  }

			  TransactionId update = new TransactionId( transactionId, checksum, commitTimestamp );
			  while ( !_highest.compareAndSet( high, update ) )
			  {
					high = _highest.get();
					if ( high.TransactionIdConflict() >= transactionId )
					{ // apparently someone else set a higher id while we were trying to set this id
						 return false;
					}
			  }
			  // we set our id as the highest
			  return true;
		 }

		 /// <summary>
		 /// Overrides the highest transaction id value, no matter what it currently is. Used for initialization purposes.
		 /// </summary>
		 /// <param name="transactionId"> id of the transaction. </param>
		 /// <param name="checksum"> checksum of the transaction. </param>
		 /// <param name="commitTimestamp"> commit time for transaction with {@code transactionId}. </param>
		 public void Set( long transactionId, long checksum, long commitTimestamp )
		 {
			  _highest.set( new TransactionId( transactionId, checksum, commitTimestamp ) );
		 }

		 /// <returns> the currently highest transaction together with its checksum. </returns>
		 public virtual TransactionId Get()
		 {
			  return _highest.get();
		 }
	}

}