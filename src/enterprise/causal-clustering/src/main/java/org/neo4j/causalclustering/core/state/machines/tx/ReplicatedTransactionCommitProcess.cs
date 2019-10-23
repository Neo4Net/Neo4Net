using System;

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
namespace Neo4Net.causalclustering.core.state.machines.tx
{

	using ReplicationFailureException = Neo4Net.causalclustering.core.replication.ReplicationFailureException;
	using Replicator = Neo4Net.causalclustering.core.replication.Replicator;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.exceptions.Status_Cluster.ReplicationFailure;

	public class ReplicatedTransactionCommitProcess : TransactionCommitProcess
	{
		 private readonly Replicator _replicator;

		 public ReplicatedTransactionCommitProcess( Replicator replicator )
		 {
			  this._replicator = replicator;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long commit(final org.Neo4Net.kernel.impl.api.TransactionToApply tx, final org.Neo4Net.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode mode) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override long Commit( TransactionToApply tx, CommitEvent commitEvent, TransactionApplicationMode mode )
		 {
			  TransactionRepresentationReplicatedTransaction transaction = ReplicatedTransaction.from( tx.TransactionRepresentation() );
			  Future<object> futureTxId;
			  try
			  {
					futureTxId = _replicator.replicate( transaction, true );
			  }
			  catch ( ReplicationFailureException e )
			  {
					throw new TransactionFailureException( ReplicationFailure, e );
			  }

			  try
			  {
					return ( long ) futureTxId.get();
			  }
			  catch ( ExecutionException e )
			  {
					if ( e.InnerException is TransactionFailureException )
					{
						 throw ( TransactionFailureException ) e.InnerException;
					}
					// TODO: Panic?
					throw new Exception( e );
			  }
			  catch ( InterruptedException e )
			  {
					// TODO Wait for the transaction to possibly finish within a user configurable time, before aborting.
					throw new TransactionFailureException( "Interrupted while waiting for txId", e );
			  }
		 }
	}

}