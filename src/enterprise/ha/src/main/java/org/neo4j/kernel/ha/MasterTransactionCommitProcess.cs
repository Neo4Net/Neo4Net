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
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionPropagator = Neo4Net.Kernel.ha.transaction.TransactionPropagator;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using IntegrityValidator = Neo4Net.Kernel.impl.transaction.state.IntegrityValidator;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

	/// <summary>
	/// Commit process on the master side in HA, where transactions either comes in from slaves committing,
	/// or gets created and committed directly on the master.
	/// </summary>
	public class MasterTransactionCommitProcess : TransactionCommitProcess
	{

		 private readonly TransactionCommitProcess _inner;
		 private readonly TransactionPropagator _txPropagator;
		 private readonly IntegrityValidator _validator;
		 private readonly Monitor _monitor;

		 public interface Monitor
		 {
			  void MissedReplicas( int number );
		 }

		 public MasterTransactionCommitProcess( TransactionCommitProcess commitProcess, TransactionPropagator txPropagator, IntegrityValidator validator, Monitor monitor )
		 {
			  this._inner = commitProcess;
			  this._txPropagator = txPropagator;
			  this._validator = validator;
			  this._monitor = monitor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long commit(org.Neo4Net.kernel.impl.api.TransactionToApply batch, org.Neo4Net.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.Neo4Net.storageengine.api.TransactionApplicationMode mode) throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
		 public override long Commit( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
		 {
			  Validate( batch );
			  long result = _inner.commit( batch, commitEvent, mode );

			  // Assuming all the transactions come from the same author
			  int missedReplicas = _txPropagator.committed( result, batch.TransactionRepresentation().AuthorId );

			  if ( missedReplicas > 0 )
			  {
					_monitor.missedReplicas( missedReplicas );
			  }

			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validate(org.Neo4Net.kernel.impl.api.TransactionToApply batch) throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
		 private void Validate( TransactionToApply batch )
		 {
			  while ( batch != null )
			  {
					_validator.validateTransactionStartKnowledge( batch.TransactionRepresentation().LatestCommittedTxWhenStarted );
					batch = batch.Next();
			  }
		 }

	}

}