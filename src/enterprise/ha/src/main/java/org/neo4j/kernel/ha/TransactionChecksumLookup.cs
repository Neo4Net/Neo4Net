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
namespace Neo4Net.Kernel.ha
{

	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using NoSuchTransactionException = Neo4Net.Kernel.impl.transaction.log.NoSuchTransactionException;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.withMessage;

	/// <summary>
	/// Transaction meta data can normally be looked up using <seealso cref="LogicalTransactionStore.getMetadataFor(long)"/>.
	/// The exception to that is when there are no transaction logs for the database, for example after a migration
	/// and we're looking up the checksum for transaction the migration was performed at. In that case we have to
	/// extract that checksum directly from <seealso cref="TransactionIdStore"/>, since it's not in any transaction log,
	/// at least not at the time of writing this class.
	/// </summary>
	public class TransactionChecksumLookup
	{
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly LogicalTransactionStore _logicalTransactionStore;
		 private TransactionId _upgradeTransaction;

		 public TransactionChecksumLookup( TransactionIdStore transactionIdStore, LogicalTransactionStore logicalTransactionStore )
		 {
			  this._transactionIdStore = transactionIdStore;
			  this._logicalTransactionStore = logicalTransactionStore;
			  this._upgradeTransaction = transactionIdStore.UpgradeTransaction;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long lookup(long txId) throws java.io.IOException
		 public virtual long Lookup( long txId )
		 {
			  // First off see if the requested txId is in fact the last committed transaction.
			  // If so then we can extract the checksum directly from the transaction id store.
			  TransactionId lastCommittedTransaction = _transactionIdStore.LastCommittedTransaction;
			  if ( lastCommittedTransaction.TransactionIdConflict() == txId )
			  {
					return lastCommittedTransaction.Checksum();
			  }

			  // Check if the requested txId is upgrade transaction
			  // if so then use checksum form transaction id store.
			  // That checksum can take specific values that should not be re-evaluated.
			  if ( _upgradeTransaction.transactionId() == txId )
			  {
					return _upgradeTransaction.checksum();
			  }

			  // It wasn't, so go look for it in the transaction store.
			  // Intentionally let potentially thrown IOException (and NoSuchTransactionException) be thrown
			  // from this call below, it's part of the contract of this method.
			  try
			  {
					return _logicalTransactionStore.getMetadataFor( txId ).Checksum;
			  }
			  catch ( NoSuchTransactionException e )
			  {
					// So we truly couldn't find the checksum for this txId, go ahead and throw
					throw withMessage( e, e.Message + " | transaction id store says last transaction is " + lastCommittedTransaction + " and last upgrade transaction is " + _upgradeTransaction );
			  }
		 }
	}

}