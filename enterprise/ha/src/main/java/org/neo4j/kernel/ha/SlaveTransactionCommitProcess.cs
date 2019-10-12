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
namespace Org.Neo4j.Kernel.ha
{
	using ComException = Org.Neo4j.com.ComException;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.com;
	using TransientTransactionFailureException = Org.Neo4j.Graphdb.TransientTransactionFailureException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

	/// <summary>
	/// Commit process on slaves in HA. Transactions aren't committed here, but sent to the master, committed
	/// there and streamed back. Look at <seealso cref="org.neo4j.com.storecopy.TransactionCommittingResponseUnpacker"/>
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
//ORIGINAL LINE: public long commit(org.neo4j.kernel.impl.api.TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.neo4j.storageengine.api.TransactionApplicationMode mode) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
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