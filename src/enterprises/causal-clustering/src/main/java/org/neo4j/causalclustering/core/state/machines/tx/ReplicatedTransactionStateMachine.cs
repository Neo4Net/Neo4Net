using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.state.machines.tx
{

	using Neo4Net.causalclustering.core.state.machines;
	using CommandIndexTracker = Neo4Net.causalclustering.core.state.machines.id.CommandIndexTracker;
	using ReplicatedLockTokenStateMachine = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionQueue = Neo4Net.Kernel.Impl.Api.TransactionQueue;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding.encodeLogIndexAsTxHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.LockSessionExpired;

	public class ReplicatedTransactionStateMachine : StateMachine<ReplicatedTransaction>
	{
		 private readonly CommandIndexTracker _commandIndexTracker;
		 private readonly ReplicatedLockTokenStateMachine _lockTokenStateMachine;
		 private readonly int _maxBatchSize;
		 private readonly Log _log;
		 private readonly PageCursorTracerSupplier _pageCursorTracerSupplier;
		 private readonly VersionContextSupplier _versionContextSupplier;

		 private TransactionQueue _queue;
		 private long _lastCommittedIndex = -1;

		 public ReplicatedTransactionStateMachine( CommandIndexTracker commandIndexTracker, ReplicatedLockTokenStateMachine lockStateMachine, int maxBatchSize, LogProvider logProvider, PageCursorTracerSupplier pageCursorTracerSupplier, VersionContextSupplier versionContextSupplier )
		 {
			  this._commandIndexTracker = commandIndexTracker;
			  this._lockTokenStateMachine = lockStateMachine;
			  this._maxBatchSize = maxBatchSize;
			  this._log = logProvider.getLog( this.GetType() );
			  this._pageCursorTracerSupplier = pageCursorTracerSupplier;
			  this._versionContextSupplier = versionContextSupplier;
		 }

		 public virtual void InstallCommitProcess( TransactionCommitProcess commitProcess, long lastCommittedIndex )
		 {
			 lock ( this )
			 {
				  this._lastCommittedIndex = lastCommittedIndex;
				  _commandIndexTracker.AppliedCommandIndex = lastCommittedIndex;
				  _log.info( format( "Updated lastCommittedIndex to %d", lastCommittedIndex ) );
				  this._queue = new TransactionQueue(_maxBatchSize, (first, last) =>
				  {
					commitProcess.Commit( first, CommitEvent.NULL, TransactionApplicationMode.EXTERNAL );
					_pageCursorTracerSupplier.get().reportEvents(); // Report paging metrics for the commit
				  });
			 }
		 }

		 public override void ApplyCommand( ReplicatedTransaction replicatedTx, long commandIndex, System.Action<Result> callback )
		 {
			 lock ( this )
			 {
				  if ( commandIndex <= _lastCommittedIndex )
				  {
						_log.debug( "Ignoring transaction at log index %d since already committed up to %d", commandIndex, _lastCommittedIndex );
						return;
				  }
      
				  TransactionRepresentation tx;
      
				  sbyte[] extraHeader = encodeLogIndexAsTxHeader( commandIndex );
				  tx = ReplicatedTransactionFactory.ExtractTransactionRepresentation( replicatedTx, extraHeader );
      
				  int currentTokenId = _lockTokenStateMachine.currentToken().id();
				  int txLockSessionId = tx.LockSessionId;
      
				  if ( currentTokenId != txLockSessionId && txLockSessionId != Neo4Net.Kernel.impl.locking.Locks_Client_Fields.NO_LOCK_SESSION_ID )
				  {
						callback( Result.of( new TransactionFailureException( LockSessionExpired, "The lock session in the cluster has changed: [current lock session id:%d, tx lock session id:%d]", currentTokenId, txLockSessionId ) ) );
				  }
				  else
				  {
						try
						{
							 TransactionToApply transaction = new TransactionToApply( tx, _versionContextSupplier.VersionContext );
							 transaction.OnClose(txId =>
							 {
							  if ( tx.LatestCommittedTxWhenStarted >= txId )
							  {
									throw new System.InvalidOperationException( format( "Out of order transaction. Expected that %d < %d", tx.LatestCommittedTxWhenStarted, txId ) );
							  }

							  callback( Result.of( txId ) );
							  _commandIndexTracker.AppliedCommandIndex = commandIndex;
							 });
							 _queue.queue( transaction );
						}
						catch ( Exception e )
						{
							 throw PanicException( e );
						}
				  }
			 }
		 }

		 public override void Flush()
		 {
			  // implicitly flushed
		 }

		 public override long LastAppliedIndex()
		 {
			  if ( _queue == null )
			  {
					/// <summary>
					/// See <seealso cref="installCommitProcess"/>. </summary>
					throw new System.InvalidOperationException( "Value has not been installed" );
			  }
			  return _lastCommittedIndex;
		 }

		 public virtual void EnsuredApplied()
		 {
			 lock ( this )
			 {
				  try
				  {
						_queue.empty();
				  }
				  catch ( Exception e )
				  {
						throw PanicException( e );
				  }
			 }
		 }

		 private System.InvalidOperationException PanicException( Exception e )
		 {
			  return new System.InvalidOperationException( "Failed to locally commit a transaction that has already been " + "committed to the RAFT log. This server cannot process later transactions and needs to be " + "restarted once the underlying cause has been addressed.", e );
		 }
	}

}