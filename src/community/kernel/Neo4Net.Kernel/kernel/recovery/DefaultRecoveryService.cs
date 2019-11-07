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
namespace Neo4Net.Kernel.recovery
{

	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionCursor = Neo4Net.Kernel.impl.transaction.log.TransactionCursor;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryStart;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.log.Commitment.NO_COMMITMENT;

	public class DefaultRecoveryService : RecoveryService
	{
		 private readonly RecoveryStartInformationProvider _recoveryStartInformationProvider;
		 private readonly StorageEngine _storageEngine;
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly LogicalTransactionStore _logicalTransactionStore;
		 private readonly LogVersionRepository _logVersionRepository;

		 public DefaultRecoveryService( StorageEngine storageEngine, LogTailScanner logTailScanner, TransactionIdStore transactionIdStore, LogicalTransactionStore logicalTransactionStore, LogVersionRepository logVersionRepository, RecoveryStartInformationProvider.Monitor monitor )
		 {
			  this._storageEngine = storageEngine;
			  this._transactionIdStore = transactionIdStore;
			  this._logicalTransactionStore = logicalTransactionStore;
			  this._logVersionRepository = logVersionRepository;
			  this._recoveryStartInformationProvider = new RecoveryStartInformationProvider( logTailScanner, monitor );
		 }

		 public virtual RecoveryStartInformation RecoveryStartInformation
		 {
			 get
			 {
				  return _recoveryStartInformationProvider.get();
			 }
		 }

		 public override void StartRecovery()
		 {
			  // Calling this method means that recovery is required, tell storage engine about it
			  // This method will be called before recovery actually starts and so will ensure that
			  // each store is aware that recovery will be performed. At this point all the stores have
			  // already started btw.
			  // Go and read more at {@link CommonAbstractStore#deleteIdGenerator()}
			  _storageEngine.prepareForRecoveryRequired();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RecoveryApplier getRecoveryApplier(Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode mode) throws Exception
		 public override RecoveryApplier GetRecoveryApplier( TransactionApplicationMode mode )
		 {
			  return new RecoveryVisitor( _storageEngine, mode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.kernel.impl.transaction.log.TransactionCursor getTransactions(Neo4Net.kernel.impl.transaction.log.LogPosition position) throws java.io.IOException
		 public override TransactionCursor GetTransactions( LogPosition position )
		 {
			  return _logicalTransactionStore.getTransactions( position );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.kernel.impl.transaction.log.TransactionCursor getTransactionsInReverseOrder(Neo4Net.kernel.impl.transaction.log.LogPosition position) throws java.io.IOException
		 public override TransactionCursor GetTransactionsInReverseOrder( LogPosition position )
		 {
			  return _logicalTransactionStore.getTransactionsInReverseOrder( position );
		 }

		 public override void TransactionsRecovered( CommittedTransactionRepresentation lastRecoveredTransaction, LogPosition positionAfterLastRecoveredTransaction )
		 {
			  long recoveredTransactionLogVersion = positionAfterLastRecoveredTransaction.LogVersion;
			  long recoveredTransactionOffset = positionAfterLastRecoveredTransaction.ByteOffset;
			  if ( lastRecoveredTransaction != null )
			  {
					LogEntryCommit commitEntry = lastRecoveredTransaction.CommitEntry;
					_transactionIdStore.setLastCommittedAndClosedTransactionId( commitEntry.TxId, LogEntryStart.checksum( lastRecoveredTransaction.StartEntry ), commitEntry.TimeWritten, recoveredTransactionOffset, recoveredTransactionLogVersion );
			  }
			  _logVersionRepository.CurrentLogVersion = recoveredTransactionLogVersion;
		 }

		 internal class RecoveryVisitor : RecoveryApplier
		 {
			  internal readonly StorageEngine StorageEngine;
			  internal readonly TransactionApplicationMode Mode;

			  internal RecoveryVisitor( StorageEngine storageEngine, TransactionApplicationMode mode )
			  {
					this.StorageEngine = storageEngine;
					this.Mode = mode;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(Neo4Net.kernel.impl.transaction.CommittedTransactionRepresentation transaction) throws Exception
			  public override bool Visit( CommittedTransactionRepresentation transaction )
			  {
					TransactionRepresentation txRepresentation = transaction.TransactionRepresentation;
					long txId = transaction.CommitEntry.TxId;
					TransactionToApply tx = new TransactionToApply( txRepresentation, txId );
					tx.Commitment( NO_COMMITMENT, txId );
					tx.LogPosition( transaction.StartEntry.StartPosition );
					StorageEngine.apply( tx, Mode );
					return false;
			  }

			  public override void Close()
			  { // nothing to close
			  }
		 }
	}

}