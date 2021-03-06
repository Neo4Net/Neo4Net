﻿using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using LogAppendEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent;
	using StoreApplyEvent = Org.Neo4j.Kernel.impl.transaction.tracing.StoreApplyEvent;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.TransactionCommitFailed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.TransactionLogError;

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
//ORIGINAL LINE: public long commit(TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.neo4j.storageengine.api.TransactionApplicationMode mode) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
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
//ORIGINAL LINE: private long appendToLog(TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.CommitEvent commitEvent) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
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
//ORIGINAL LINE: protected void applyToStore(TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.neo4j.storageengine.api.TransactionApplicationMode mode) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
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