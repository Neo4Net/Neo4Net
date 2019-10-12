using System;

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
namespace Org.Neo4j.causalclustering.core.state.machines.tx
{
	using Org.Neo4j.Cursor;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding.decodeLogIndexFromTxHeader;

	/// <summary>
	/// Finds the last committed transaction in the transaction log, then decodes the header as a raft index.
	/// This allows us to correlate raft log with transaction log on recovery.
	/// </summary>
	public class LastCommittedIndexFinder
	{
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly LogicalTransactionStore _transactionStore;
		 private readonly Log _log;

		 public LastCommittedIndexFinder( TransactionIdStore transactionIdStore, LogicalTransactionStore transactionStore, LogProvider logProvider )
		 {
			  this._transactionIdStore = transactionIdStore;
			  this._transactionStore = transactionStore;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public virtual long LastCommittedIndex
		 {
			 get
			 {
				  long lastConsensusIndex;
				  long lastTxId = _transactionIdStore.LastCommittedTransactionId;
				  _log.info( "Last transaction id in metadata store %d", lastTxId );
   
				  CommittedTransactionRepresentation lastTx = null;
				  try
				  {
						  using ( IOCursor<CommittedTransactionRepresentation> transactions = _transactionStore.getTransactions( lastTxId ) )
						  {
							while ( transactions.next() )
							{
								 lastTx = transactions.get();
							}
						  }
				  }
				  catch ( Exception e )
				  {
						throw new Exception( e );
				  }
   
				  if ( lastTx == null )
				  {
						throw new Exception( "We must have at least one transaction telling us where we are at in the consensus log." );
				  }
   
				  _log.info( "Start id of last committed transaction in transaction log %d", lastTx.StartEntry.LastCommittedTxWhenTransactionStarted );
				  _log.info( "Last committed transaction id in transaction log %d", lastTx.CommitEntry.TxId );
   
				  sbyte[] lastHeaderFound = lastTx.StartEntry.AdditionalHeader;
				  lastConsensusIndex = decodeLogIndexFromTxHeader( lastHeaderFound );
   
				  _log.info( "Last committed consensus log index committed into tx log %d", lastConsensusIndex );
				  return lastConsensusIndex;
			 }
		 }
	}

}