using System;

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
namespace Neo4Net.Kernel.Impl.Api
{
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using StoreApplyEvent = Neo4Net.Kernel.impl.transaction.tracing.StoreApplyEvent;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.exceptions.Status_Transaction.TransactionCommitFailed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.exceptions.Status_Transaction.TransactionLogError;

	public class TransactionRepresentationCommitProcess : TransactionCommitProcess
	{
		 private readonly TransactionAppender _appender;
		 private readonly StorageEngine _storageEngine;

		 public TransactionRepresentationCommitProcess( TransactionAppender appender, StorageEngine storageEngine )
		 {
			  this._appender = appender;
			  this._storageEngine = storageEngine;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long commit(TransactionToApply batch, Neo4Net.kernel.impl.transaction.tracing.CommitEvent commitEvent, Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode mode) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 public override long Commit( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
		 {
			  long lastTxId = AppendToLog( batch, commitEvent );
			  try
			  {
					ApplyToStore( batch, commitEvent, mode );
					return lastTxId;
			  }
			  finally
			  {
					Close( batch );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long appendToLog(TransactionToApply batch, Neo4Net.kernel.impl.transaction.tracing.CommitEvent commitEvent) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private long AppendToLog( TransactionToApply batch, CommitEvent commitEvent )
		 {
			  try
			  {
					  using ( LogAppendEvent logAppendEvent = commitEvent.BeginLogAppend() )
					  {
						return _appender.append( batch, logAppendEvent );
					  }
			  }
			  catch ( Exception cause )
			  {
					throw new TransactionFailureException( TransactionLogError, cause, "Could not append transaction representation to log" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void applyToStore(TransactionToApply batch, Neo4Net.kernel.impl.transaction.tracing.CommitEvent commitEvent, Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode mode) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 protected internal virtual void ApplyToStore( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
		 {
			  try
			  {
					  using ( StoreApplyEvent storeApplyEvent = commitEvent.BeginStoreApply() )
					  {
						_storageEngine.apply( batch, mode );
					  }
			  }
			  catch ( Exception cause )
			  {
					throw new TransactionFailureException( TransactionCommitFailed, cause, "Could not apply the transaction to the store after written to log" );
			  }
		 }

		 private void Close( TransactionToApply batch )
		 {
			  while ( batch != null )
			  {
					if ( batch.Commitment().markedAsCommitted() )
					{
						 batch.Commitment().publishAsClosed();
					}
					batch.Close();
					batch = batch.Next();
			  }
		 }
	}

}