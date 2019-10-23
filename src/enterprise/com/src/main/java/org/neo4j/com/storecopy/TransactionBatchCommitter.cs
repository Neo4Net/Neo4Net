using System.Diagnostics;

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
namespace Neo4Net.com.storecopy
{
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionQueue = Neo4Net.Kernel.Impl.Api.TransactionQueue;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode.EXTERNAL;

	internal class TransactionBatchCommitter : TransactionQueue.Applier
	{
		 private readonly KernelTransactions _kernelTransactions;
		 private readonly long _idReuseSafeZoneTime;
		 private readonly TransactionCommitProcess _commitProcess;
		 private readonly Log _log;

		 internal TransactionBatchCommitter( KernelTransactions kernelTransactions, long idReuseSafeZoneTime, TransactionCommitProcess commitProcess, Log log )
		 {
			  Debug.Assert( log != null );

			  this._kernelTransactions = kernelTransactions;
			  this._idReuseSafeZoneTime = idReuseSafeZoneTime;
			  this._commitProcess = commitProcess;
			  this._log = log;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(org.Neo4Net.kernel.impl.api.TransactionToApply first, org.Neo4Net.kernel.impl.api.TransactionToApply last) throws Exception
		 public override void Apply( TransactionToApply first, TransactionToApply last )
		 {
			  /*
			    Case 1 (Not really a problem):
			     - chunk of batch is smaller than safe zone
			     - tx started after activeTransactions() is called
			     is safe because those transactions will see the latest state of store before chunk is applied and
			     because chunk is smaller than safe zone we are guarantied to not see two different states of any record
			     when applying the chunk.
	
			       activeTransactions() is called
			       |        start committing chunk
			    ---|----+---|--|------> TIME
			            |      |
			            |      Start applying chunk
			            New tx starts here. Does not get terminated because not among active transactions, this is safe.
	
			    Case 2:
			     - chunk of batch is larger than safe zone
			     - tx started after activeTransactions() but before apply
	
			       activeTransactions() is called
			       |        start committing chunk
			    ---|--------|+-|------> TIME
			                 | |
			                 | Start applying chunk
			                 New tx starts here. Does not get terminated because not among active transactions, but will
			                 read outdated data and can be affected by reuse contamination.
			   */

			  if ( BatchSizeExceedsSafeZone( first, last ) )
			  {
					// We stop new transactions from starting to avoid problem described in (2)
					_kernelTransactions.blockNewTransactions();
					try
					{
						 MarkUnsafeTransactionsForTermination( first, last );
						 Commit( first );
					}
					finally
					{
						 _kernelTransactions.unblockNewTransactions();
					}
			  }
			  else
			  {
					MarkUnsafeTransactionsForTermination( first, last );
					Commit( first );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long commit(org.Neo4Net.kernel.impl.api.TransactionToApply first) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private long Commit( TransactionToApply first )
		 {
			  return _commitProcess.commit( first, CommitEvent.NULL, EXTERNAL );
		 }

		 private bool BatchSizeExceedsSafeZone( TransactionToApply first, TransactionToApply last )
		 {
			  long lastAppliedTimestamp = last.TransactionRepresentation().TimeCommitted;
			  long firstAppliedTimestamp = first.TransactionRepresentation().TimeCommitted;
			  long chunkLength = lastAppliedTimestamp - firstAppliedTimestamp;

			  return chunkLength > _idReuseSafeZoneTime;
		 }

		 private void MarkUnsafeTransactionsForTermination( TransactionToApply first, TransactionToApply last )
		 {
			  long firstCommittedTimestamp = first.TransactionRepresentation().TimeCommitted;
			  long lastCommittedTimestamp = last.TransactionRepresentation().TimeCommitted;
			  long earliestSafeTimestamp = lastCommittedTimestamp - _idReuseSafeZoneTime;

			  foreach ( KernelTransactionHandle txHandle in _kernelTransactions.activeTransactions() )
			  {
					long commitTimestamp = txHandle.LastTransactionTimestampWhenStarted();

					if ( commitTimestamp != Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP && commitTimestamp < earliestSafeTimestamp )
					{
						 if ( txHandle.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Outdated ) )
						 {
							  _log.info( "Marking transaction for termination, " + "invalidated due to an upcoming batch of changes being applied:" + "\n" + "  Batch: firstCommittedTxId:" + first.TransactionId() + ", firstCommittedTimestamp:" + InformativeTimestamp(firstCommittedTimestamp) + ", lastCommittedTxId:" + last.TransactionId() + ", lastCommittedTimestamp:" + InformativeTimestamp(lastCommittedTimestamp) + ", batchTimeRange:" + InformativeDuration(lastCommittedTimestamp - firstCommittedTimestamp) + ", earliestSafeTimestamp:" + InformativeTimestamp(earliestSafeTimestamp) + ", safeZoneDuration:" + InformativeDuration(_idReuseSafeZoneTime) + "\n" + "  Transaction: lastCommittedTimestamp:" + InformativeTimestamp(txHandle.LastTransactionTimestampWhenStarted()) + ", lastCommittedTxId:" + txHandle.LastTransactionIdWhenStarted() + ", localStartTimestamp:" + InformativeTimestamp(txHandle.StartTime()) );
						 }
					}
			  }
		 }

		 private static string InformativeDuration( long duration )
		 {
			  return duration( duration ) + "/" + duration;
		 }

		 private static string InformativeTimestamp( long timestamp )
		 {
			  return time( timestamp ) + "/" + timestamp;
		 }
	}

}