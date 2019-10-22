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
	using ComException = Neo4Net.com.ComException;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using TransientTransactionFailureException = Neo4Net.GraphDb.TransientTransactionFailureException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

	/// <summary>
	/// Commit process on slaves in HA. Transactions aren't committed here, but sent to the master, committed
	/// there and streamed back. Look at <seealso cref="org.Neo4Net.com.storecopy.TransactionCommittingResponseUnpacker"/>
	/// </summary>
	public class SlaveTransactionCommitProcess : TransactionCommitProcess
	{
		 private readonly Master _master;
		 private readonly RequestContextFactory _requestContextFactory;

		 public SlaveTransactionCommitProcess( Master master, RequestContextFactory requestContextFactory )
		 {
			  this._master = master;
			  this._requestContextFactory = requestContextFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long commit(org.Neo4Net.kernel.impl.api.TransactionToApply batch, org.Neo4Net.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.Neo4Net.storageengine.api.TransactionApplicationMode mode) throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
		 public override long Commit( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
		 {
			  if ( batch.Next() != null )
			  {
					throw new System.ArgumentException( "Only supports single-commit on slave --> master" );
			  }

			  try
			  {
					TransactionRepresentation representation = batch.TransactionRepresentation();
					RequestContext context = _requestContextFactory.newRequestContext( representation.LockSessionId );
					using ( Response<long> response = _master.commit( context, representation ) )
					{
						 return response.ResponseConflict();
					}
			  }
			  catch ( ComException e )
			  {
					throw new TransientTransactionFailureException( "Cannot commit this transaction on the master. " + "The master is either down, or we have network connectivity problems.", e );
			  }
		 }
	}

}