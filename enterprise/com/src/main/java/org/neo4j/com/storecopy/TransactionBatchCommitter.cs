using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.com.storecopy
{
	using KernelTransactionHandle = Org.Neo4j.Kernel.api.KernelTransactionHandle;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using KernelTransactions = Org.Neo4j.Kernel.Impl.Api.KernelTransactions;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionQueue = Org.Neo4j.Kernel.Impl.Api.TransactionQueue;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

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
//ORIGINAL LINE: public void apply(org.neo4j.kernel.impl.api.TransactionToApply first, org.neo4j.kernel.impl.api.TransactionToApply last) throws Exception
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
//ORIGINAL LINE: private long commit(org.neo4j.kernel.impl.api.TransactionToApply first) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
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

					if ( commitTimestamp != Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP && commitTimestamp < earliestSafeTimestamp )
					{
						 if ( txHandle.MarkForTermination( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Outdated ) )
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